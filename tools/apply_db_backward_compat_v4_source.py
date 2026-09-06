from pathlib import Path

DB_FILE = Path(r"Project Files/Source/Console/database.cs")
DBMAN_FILE = Path(r"Project Files/Source/Console/clsDBMan.cs")

db = DB_FILE.read_text(encoding="utf-8-sig")
dbman = DBMAN_FILE.read_text(encoding="utf-8-sig")

MARKER = "SQ4KOU_DB_COMPAT_PRESERVATION_V4"
if MARKER in db and MARKER in dbman:
    print("V4 source compatibility patch already present.")
    raise SystemExit(0)


def replace_method(src: str, signature: str, replacement: str) -> str:
    start = src.find(signature)
    if start < 0:
        raise RuntimeError(f"method signature not found: {signature}")
    brace = src.find("{", start)
    if brace < 0:
        raise RuntimeError(f"opening brace not found: {signature}")
    depth = 0
    i = brace
    while i < len(src):
        if src[i] == "{":
            depth += 1
        elif src[i] == "}":
            depth -= 1
            if depth == 0:
                return src[:start] + replacement + src[i + 1:]
        i += 1
    raise RuntimeError(f"closing brace not found: {signature}")


# -----------------------------------------------------------------------------
# DATABASE.CS
# -----------------------------------------------------------------------------

# Track repairs performed while dirty tracking is intentionally suspended in Init().
needle = "        private static bool _checkpointingEnabled = true;"
if needle not in db:
    raise RuntimeError("checkpoint field marker not found")
db = db.replace(
    needle,
    needle + "\n        // " + MARKER + "\n        private static bool _compatibilityRepairChanged = false;",
    1,
)

# V3 already performs the correct old-value-authoritative TX normalization. Make
# those repairs visible to Init() so they are atomically persisted even when the
# database already carries the current schema marker.
v3_start = db.find("        // SQ4KOU V3: normalize incomplete legacy TX profiles in-place BEFORE")
v3_end = db.find("        #region BandStack2", v3_start)
if v3_start < 0 or v3_end < 0:
    raise RuntimeError("V3 TX normalizer not found; V1/V2/V3 must run before V4")
v3 = db[v3_start:v3_end]
v3 = v3.replace(
    "                        DataColumn added = target.Columns.Add(currentCol.ColumnName, currentCol.DataType);",
    "                        DataColumn added = target.Columns.Add(currentCol.ColumnName, currentCol.DataType);\n                        _compatibilityRepairChanged = true;",
)
v3 = v3.replace(
    "                        target.Rows.Add(row);",
    "                        target.Rows.Add(row);\n                        _compatibilityRepairChanged = true;",
    1,
)
v3 = v3.replace(
    "                            row[currentCol.ColumnName] = value;\n                            LogDatabaseEvent(\"LEGACY_TXPROFILE_NORMALIZE\"",
    "                            row[currentCol.ColumnName] = value;\n                            _compatibilityRepairChanged = true;\n                            LogDatabaseEvent(\"LEGACY_TXPROFILE_NORMALIZE\"",
)
db = db[:v3_start] + v3 + db[v3_end:]

# Init() must persist compatibility repairs made while event tracking is suspended.
init_pos = db.find("        public static bool Init()")
if init_pos < 0:
    raise RuntimeError("DB.Init not found")
suspend_pos = db.find("            _suspendDirtyTracking = true;", init_pos)
if suspend_pos < 0:
    raise RuntimeError("DB.Init suspend marker not found")
suspend_end = suspend_pos + len("            _suspendDirtyTracking = true;")
db = db[:suspend_end] + "\n            _compatibilityRepairChanged = false;" + db[suspend_end:]
verify_pos = db.find("            VerifyTables();", suspend_end)
if verify_pos < 0:
    raise RuntimeError("VerifyTables call in DB.Init not found")
verify_end = verify_pos + len("            VerifyTables();")
db = db[:verify_end] + "\n            if (_compatibilityRepairChanged) changed = true;" + db[verify_end:]

# Structural compatibility is independent of SQ4KOU's schema metadata number.
# The minimum-current-schema validator remains authoritative for actual fields.
is_compatible = r'''        // SQ4KOU_DB_COMPAT_PRESERVATION_V4
        // Compatibility means: every field required by this build is usable.
        // Extra legacy/future tables and columns are explicitly allowed. The
        // DatabaseSchemaVersion marker is metadata and by itself never justifies
        // a destructive/fresh-database migration.
        public static bool IsDatabaseCompatible(out string reason)
        {
            reason = "";
            if (ds == null)
            {
                reason = "Dataset is null.";
                return false;
            }

            string validation = ValidateDataSet(ds, true);
            if (!string.IsNullOrEmpty(validation))
            {
                string[] lines = validation.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string raw in lines)
                {
                    string line = raw.Trim();
                    if (line.StartsWith("DatabaseSchemaVersion=", StringComparison.Ordinal) ||
                        line == "DatabaseSchemaVersion is missing or invalid.")
                        continue;
                    reason = line;
                    return false;
                }
            }

            if (!ds.Tables.Contains("State"))
            {
                reason = "Required table 'State' is missing.";
                return false;
            }
            Dictionary<string, string> state = GetVarsDictionary("State");
            if (!state.ContainsKey("VersionNumber"))
            {
                reason = "State table is missing VersionNumber.";
                return false;
            }
            if (!state.ContainsKey("Version"))
            {
                reason = "State table is missing Version.";
                return false;
            }
            return true;
        }'''
db = replace_method(db, "        public static bool IsDatabaseCompatible(out string reason)", is_compatible)

# Preservation-first helpers used by automatic upgrades and regression checks.
insert_before = "        public static bool ImportAndMergeDatabase(string filename, out string log, bool ignore_merged)"
insert_at = db.find(insert_before)
if insert_at < 0:
    raise RuntimeError("ImportAndMergeDatabase insertion marker not found")
helpers = r'''
        // SQ4KOU_DB_COMPAT_PRESERVATION_V4
        private static string[] CompatibilityIdentityColumns(DataTable table)
        {
            if (table == null) return null;
            if (table.TableName == "BandText" && table.Columns.Contains("Low") && table.Columns.Contains("High") && table.Columns.Contains("Name"))
                return new[] { "Low", "High", "Name" };
            if (table.TableName == "BandStack2FilterFrequencies" && table.Columns.Contains("FilterGUID") && table.Columns.Contains("Low") && table.Columns.Contains("High") && table.Columns.Contains("Band"))
                return new[] { "FilterGUID", "Low", "High", "Band" };
            if (table.TableName == "BandStack2FilterModes" && table.Columns.Contains("FilterGUID") && table.Columns.Contains("Mode"))
                return new[] { "FilterGUID", "Mode" };
            if (table.TableName == "BandStack2FilterSubModes" && table.Columns.Contains("FilterGUID") && table.Columns.Contains("SubMode"))
                return new[] { "FilterGUID", "SubMode" };
            if (table.TableName == "BandStack2FilterBands" && table.Columns.Contains("FilterGUID") && table.Columns.Contains("Band"))
                return new[] { "FilterGUID", "Band" };
            if (table.TableName == "BandStack2HiddenEntries" && table.Columns.Contains("FilterGUID") && table.Columns.Contains("EntryGUID"))
                return new[] { "FilterGUID", "EntryGUID" };
            if (table.Columns.Contains("Key")) return new[] { "Key" };
            if (table.Columns.Contains("Name")) return new[] { "Name" };
            if (table.Columns.Contains("GUID")) return new[] { "GUID" };
            if (table.Columns.Contains("GroupID")) return new[] { "GroupID" };
            return null;
        }

        private static bool CompatibilityValuesEqual(object a, object b)
        {
            if (a == DBNull.Value && b == DBNull.Value) return true;
            if (a == DBNull.Value || b == DBNull.Value || a == null || b == null) return false;
            byte[] ba = a as byte[];
            byte[] bb = b as byte[];
            if (ba != null || bb != null)
            {
                if (ba == null || bb == null || ba.Length != bb.Length) return false;
                for (int i = 0; i < ba.Length; i++) if (ba[i] != bb[i]) return false;
                return true;
            }
            return a.Equals(b);
        }

        private static bool CompatibilityIdentityMatch(DataRow a, DataRow b, string[] identity)
        {
            if (a == null || b == null || identity == null || identity.Length == 0) return false;
            foreach (string col in identity)
            {
                if (!a.Table.Columns.Contains(col) || !b.Table.Columns.Contains(col)) return false;
                if (!CompatibilityValuesEqual(a[col], b[col])) return false;
            }
            return true;
        }

        private static DataRow FindCompatibilityRow(DataTable table, DataRow source, string[] identity)
        {
            if (table == null || source == null || identity == null) return null;
            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;
                if (CompatibilityIdentityMatch(row, source, identity)) return row;
            }
            return null;
        }

        private static object CompatibilityClrDefault(Type t)
        {
            if (t == typeof(string)) return "";
            if (t == typeof(bool)) return false;
            if (t == typeof(byte)) return (byte)0;
            if (t == typeof(short)) return (short)0;
            if (t == typeof(int)) return 0;
            if (t == typeof(long)) return (long)0;
            if (t == typeof(float)) return (float)0;
            if (t == typeof(double)) return (double)0;
            if (t == typeof(decimal)) return (decimal)0;
            if (t == typeof(DateTime)) return DateTime.MinValue;
            if (t == typeof(Guid)) return Guid.Empty;
            return DBNull.Value;
        }

        private static bool TryCopyCompatibilityValue(DataRow source, DataColumn sourceCol, DataRow target, DataColumn targetCol)
        {
            object value = source[sourceCol];
            if (value == null || value == DBNull.Value) return false;
            try
            {
                if (sourceCol.DataType == targetCol.DataType || targetCol.DataType.IsAssignableFrom(sourceCol.DataType))
                    target[targetCol] = value;
                else
                    target[targetCol] = Convert.ChangeType(value, targetCol.DataType);
                return true;
            }
            catch { return false; }
        }

        // Merge CURRENT defaults into a database that already contains the legacy
        // database. Existing non-null legacy values are never replaced. Unknown old
        // tables/columns are never removed. This is the inverse of the historical
        // "fresh DB + copy selected old values" algorithm.
        public static bool MergeMissingDefaultsFromDatabase(string defaultsFilename, out string log)
        {
            log = "";
            try
            {
                DataSet defaults = new DataSet();
                defaults.ReadXml(defaultsFilename);

                foreach (DataTable current in defaults.Tables)
                {
                    if (!ds.Tables.Contains(current.TableName))
                    {
                        ds.Tables.Add(current.Copy());
                        _compatibilityRepairChanged = true;
                        log += "Added missing current table <" + current.TableName + ">.\n";
                        continue;
                    }

                    DataTable target = ds.Tables[current.TableName];
                    foreach (DataColumn currentCol in current.Columns)
                    {
                        if (target.Columns.Contains(currentCol.ColumnName))
                        {
                            if (target.Columns[currentCol.ColumnName].DataType != currentCol.DataType)
                            {
                                log += "Type mismatch preserved/rejected: " + current.TableName + "." + currentCol.ColumnName +
                                    " old=" + target.Columns[currentCol.ColumnName].DataType.FullName +
                                    " current=" + currentCol.DataType.FullName + ".\n";
                                return false;
                            }
                            continue;
                        }

                        DataColumn added = target.Columns.Add(currentCol.ColumnName, currentCol.DataType);
                        added.AllowDBNull = true;
                        if (currentCol.DefaultValue != null && currentCol.DefaultValue != DBNull.Value)
                            added.DefaultValue = currentCol.DefaultValue;
                        _compatibilityRepairChanged = true;
                        log += "Added missing current column " + current.TableName + "." + currentCol.ColumnName + ".\n";
                    }

                    string[] identity = CompatibilityIdentityColumns(current);

                    // Fill only DBNull in fields known to the current build. Prefer a
                    // same-identity current row; otherwise use DataColumn/CLR defaults.
                    foreach (DataRow oldRow in target.Rows)
                    {
                        if (oldRow.RowState == DataRowState.Deleted) continue;
                        DataRow seed = identity == null ? null : FindCompatibilityRow(current, oldRow, identity);
                        foreach (DataColumn currentCol in current.Columns)
                        {
                            if (!target.Columns.Contains(currentCol.ColumnName)) continue;
                            if (!oldRow.IsNull(currentCol.ColumnName)) continue;

                            bool filled = false;
                            if (seed != null && !seed.IsNull(currentCol.ColumnName))
                                filled = TryCopyCompatibilityValue(seed, currentCol, oldRow, target.Columns[currentCol.ColumnName]);
                            if (!filled && currentCol.DefaultValue != null && currentCol.DefaultValue != DBNull.Value)
                            {
                                try { oldRow[currentCol.ColumnName] = currentCol.DefaultValue; filled = true; }
                                catch { }
                            }
                            if (!filled)
                            {
                                object fallback = CompatibilityClrDefault(currentCol.DataType);
                                if (fallback != DBNull.Value)
                                {
                                    try { oldRow[currentCol.ColumnName] = fallback; filled = true; }
                                    catch { }
                                }
                            }
                            if (filled) _compatibilityRepairChanged = true;
                        }
                    }

                    // Add genuinely new factory/default rows only where a stable identity
                    // exists. User rows remain untouched and in their original table.
                    if (identity != null)
                    {
                        foreach (DataRow currentRow in current.Rows)
                        {
                            if (currentRow.RowState == DataRowState.Deleted) continue;
                            if (FindCompatibilityRow(target, currentRow, identity) != null) continue;
                            DataRow addedRow = target.NewRow();
                            foreach (DataColumn currentCol in current.Columns)
                            {
                                if (currentRow.IsNull(currentCol)) continue;
                                TryCopyCompatibilityValue(currentRow, currentCol, addedRow, target.Columns[currentCol.ColumnName]);
                            }
                            target.Rows.Add(addedRow);
                            _compatibilityRepairChanged = true;
                            log += "Added new current row to <" + current.TableName + ">.\n";
                        }
                    }
                    else if (target.Rows.Count == 0 && current.Rows.Count > 0)
                    {
                        foreach (DataRow currentRow in current.Rows)
                        {
                            DataRow addedRow = target.NewRow();
                            foreach (DataColumn currentCol in current.Columns)
                            {
                                if (currentRow.IsNull(currentCol)) continue;
                                TryCopyCompatibilityValue(currentRow, currentCol, addedRow, target.Columns[currentCol.ColumnName]);
                            }
                            target.Rows.Add(addedRow);
                        }
                        _compatibilityRepairChanged = true;
                    }
                }

                log += "Preservation-first additive merge completed.\n";
                return true;
            }
            catch (Exception ex)
            {
                log += "Additive merge failed: " + ex.GetType().Name + ": " + ex.Message + "\n";
                LogDatabaseEvent("COMPAT_MERGE_FAIL", log, ex);
                return false;
            }
        }

        public static bool MarkCurrentDatabaseMetadata(bool writeNow)
        {
            try
            {
                if (ds == null || !ds.Tables.Contains("State")) return false;
                Dictionary<string, string> state = GetVarsDictionary("State");
                VersionString = TitleBar.GetString(false);
                VersionNumber = Common.GetVerNum();
                state["Version"] = VersionString;
                state["VersionNumber"] = VersionNumber;
                state["DatabaseSchemaVersion"] = CurrentDatabaseSchemaVersion.ToString();
                SaveVarsDictionary("State", ref state, true);
                LoadedDatabaseSchemaVersion = CurrentDatabaseSchemaVersion;
                _compatibilityRepairChanged = true;
                LogDatabaseEvent("COMPAT_METADATA", "Database compatibility metadata advanced to current build/schema.");
                return !writeNow || WriteDB(_file_name, ds);
            }
            catch (Exception ex)
            {
                LogDatabaseEvent("COMPAT_METADATA_FAIL", "Could not advance compatibility metadata.", ex);
                return false;
            }
        }

        private static bool IsCompatibilityMetadataStateRow(DataRow row)
        {
            if (row == null || row.Table.TableName != "State" || !row.Table.Columns.Contains("Key")) return false;
            string key = Convert.ToString(row["Key"]);
            return key == "Version" || key == "VersionNumber" || key == "DatabaseSchemaVersion";
        }

        private static bool CandidateRowPreservesLegacy(DataRow oldRow, DataRow candidateRow)
        {
            if (oldRow == null || candidateRow == null) return false;
            if (IsCompatibilityMetadataStateRow(oldRow)) return true;
            foreach (DataColumn oldCol in oldRow.Table.Columns)
            {
                if (!candidateRow.Table.Columns.Contains(oldCol.ColumnName)) return false;
                object oldValue = oldRow[oldCol];
                if (oldValue == null || oldValue == DBNull.Value) continue; // filling a legacy NULL is allowed
                if (!CompatibilityValuesEqual(oldValue, candidateRow[oldCol.ColumnName])) return false;
            }
            return true;
        }

        // Hard gate before activation: every original table/column and every original
        // non-null value must still exist in the persisted candidate. Only compatibility
        // metadata and previously-null values are allowed to change.
        public static bool VerifyDatabasePreservesOriginal(string originalFilename, string candidateFilename, out string problems)
        {
            problems = "";
            try
            {
                DataSet original = new DataSet();
                DataSet candidate = new DataSet();
                original.ReadXml(originalFilename);
                candidate.ReadXml(candidateFilename);

                foreach (DataTable oldTable in original.Tables)
                {
                    if (!candidate.Tables.Contains(oldTable.TableName))
                    {
                        problems = "Candidate lost legacy table: " + oldTable.TableName;
                        return false;
                    }
                    DataTable candidateTable = candidate.Tables[oldTable.TableName];
                    foreach (DataColumn oldCol in oldTable.Columns)
                    {
                        if (!candidateTable.Columns.Contains(oldCol.ColumnName))
                        {
                            problems = "Candidate lost legacy column: " + oldTable.TableName + "." + oldCol.ColumnName;
                            return false;
                        }
                        if (candidateTable.Columns[oldCol.ColumnName].DataType != oldCol.DataType)
                        {
                            problems = "Candidate changed legacy column type: " + oldTable.TableName + "." + oldCol.ColumnName;
                            return false;
                        }
                    }

                    string[] identity = CompatibilityIdentityColumns(oldTable);
                    int rowIndex = 0;
                    foreach (DataRow oldRow in oldTable.Rows)
                    {
                        if (oldRow.RowState == DataRowState.Deleted) continue;
                        DataRow match = null;
                        if (identity != null)
                        {
                            foreach (DataRow candidateRow in candidateTable.Rows)
                            {
                                if (candidateRow.RowState == DataRowState.Deleted) continue;
                                if (!CompatibilityIdentityMatch(oldRow, candidateRow, identity)) continue;
                                if (CandidateRowPreservesLegacy(oldRow, candidateRow)) { match = candidateRow; break; }
                            }
                        }
                        else if (rowIndex < candidateTable.Rows.Count)
                        {
                            DataRow candidateRow = candidateTable.Rows[rowIndex];
                            if (CandidateRowPreservesLegacy(oldRow, candidateRow)) match = candidateRow;
                        }

                        if (match == null)
                        {
                            problems = "Candidate changed/lost legacy row in table <" + oldTable.TableName + "> at legacy row " + rowIndex + ".";
                            return false;
                        }
                        rowIndex++;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                problems = ex.GetType().Name + ": " + ex.Message;
                return false;
            }
        }

'''
db = db[:insert_at] + helpers + db[insert_at:]


# -----------------------------------------------------------------------------
# CLSDBMAN.CS -- automatic upgrades now clone the old DB first, then add only
# missing current defaults. The old database is never reconstructed from a fresh
# database by selectively copying values.
# -----------------------------------------------------------------------------
check_version = r'''        // SQ4KOU_DB_COMPAT_PRESERVATION_V4
        // Automatic compatibility upgrade: COPY legacy DB -> add only missing
        // current defaults -> validate -> persisted preservation audit -> activate.
        private static bool checkVersion(bool made_new, bool force_upgrade = false, bool force_upgrade_via_file = false, bool schema_mismatch = false, string schema_mismatch_reason = "")
        {
            _migration_failed_keep_original = false;
            string version;
            Dictionary<string, string> vals = DB.GetVarsDictionary("State");
            if (vals.ContainsKey("VersionNumber")) version = vals["VersionNumber"];
            else version = "? version";

            if (made_new) return true;
            bool schemaUpgradeRequired = DB.LoadedDatabaseSchemaVersion < DB.CurrentDatabaseSchemaVersion;

            // A schema-number-only difference is metadata, not incompatibility. If
            // every current field is already usable and the application version is
            // unchanged, normalize metadata atomically and continue with ZERO dialog.
            if (!force_upgrade && !force_upgrade_via_file && !schema_mismatch && Common.GetVerNum() == version)
            {
                if (schemaUpgradeRequired)
                {
                    DB.LogDatabaseEvent("COMPAT_METADATA_ONLY", "Legacy schema marker only; no data migration required.");
                    return DB.MarkCurrentDatabaseMetadata(true);
                }
                return true;
            }

            Guid guid_original = _dbman_settings == null ? Guid.Empty : _dbman_settings.ActiveDB_GUID;
            string original_db_filename_xml = DB.FileName;
            Dictionary<Guid, DatabaseInfo> before = getAvailableDBs();
            Guid candidateGuid = Guid.Empty;
            string candidateFile = "";
            string currentDefaultsFile = "";
            bool ok = false;

            DB.LogDatabaseEvent("COMPAT_COPY_BEGIN",
                "Original=" + guid_original + ", oldVersion=" + version +
                ", oldSchema=" + DB.LoadedDatabaseSchemaVersion +
                (schema_mismatch ? ", mismatch=" + schema_mismatch_reason : ""));
            DB.SetCheckpointingEnabled(false);
            try
            {
                // createNewDB gives us a valid CURRENT default database and metadata
                // folder, but it is used only as a source of defaults.
                ok = createNewDB(false, false, out bool _);
                if (ok)
                {
                    Dictionary<Guid, DatabaseInfo> after = getAvailableDBs();
                    foreach (Guid g in after.Keys)
                    {
                        if (!before.ContainsKey(g))
                        {
                            if (candidateGuid != Guid.Empty)
                            {
                                ok = false;
                                DB.LogDatabaseEvent("COMPAT_COPY_FAIL", "More than one candidate database detected.");
                                break;
                            }
                            candidateGuid = g;
                        }
                    }
                    if (candidateGuid == Guid.Empty)
                    {
                        ok = false;
                        DB.LogDatabaseEvent("COMPAT_COPY_FAIL", "Candidate database GUID was not detected.");
                    }
                }

                if (ok)
                {
                    candidateFile = _db_data_path + candidateGuid.ToString() + "\\database.xml";
                    currentDefaultsFile = candidateFile + ".currentdefaults.xml";
                    File.Copy(candidateFile, currentDefaultsFile, true);

                    // Candidate starts as an exact byte-for-byte copy of the legacy DB.
                    // Remove fresh lastgood/tmp files first so a failed legacy read can
                    // never silently fall back to a blank/default database.
                    string candidateDir = Path.GetDirectoryName(candidateFile);
                    string freshLastGood = Path.Combine(candidateDir, "database.lastgood.xml");
                    string freshTemp = candidateFile + ".tmp";
                    try { if (File.Exists(freshLastGood)) File.Delete(freshLastGood); } catch { }
                    try { if (File.Exists(freshTemp)) File.Delete(freshTemp); } catch { }
                    File.Copy(original_db_filename_xml, candidateFile, true);

                    DB.FileName = candidateFile;
                    _ignore_written = true;
                    ok = DB.Init();
                    _ignore_written = false;
                    DB.SetCheckpointingEnabled(false);
                    if (ok) DB.LogDatabaseEvent("COMPAT_COPY", "Legacy database copied into candidate " + candidateGuid + ".");
                }

                string mergeLog = "";
                if (ok)
                {
                    ok = DB.MergeMissingDefaultsFromDatabase(currentDefaultsFile, out mergeLog);
                    try { File.WriteAllText(_app_data_path + "ImportLog_dbupdate.txt", mergeLog); }
                    catch (Exception ex) { DB.LogDatabaseEvent("COMPAT_LOG_WARN", "Could not write ImportLog_dbupdate.txt", ex); }
                }

                if (ok) ok = DB.MarkCurrentDatabaseMetadata(false);

                if (ok)
                {
                    ok = DB.ValidateCurrentDatabase(out string validationProblems);
                    if (!ok) DB.LogDatabaseEvent("COMPAT_VALIDATE_FAIL", validationProblems);
                    else DB.LogDatabaseEvent("COMPAT_VALIDATE", "In-memory additive candidate OK.");
                }

                if (ok)
                {
                    _ignore_written = true;
                    ok = DB.WriteDB(candidateFile);
                    _ignore_written = false;
                }

                if (ok)
                {
                    ok = DB.ValidateDatabaseFile(candidateFile, true, out string persistedProblems);
                    if (!ok) DB.LogDatabaseEvent("COMPAT_VERIFY_FAIL", persistedProblems);
                    else DB.LogDatabaseEvent("COMPAT_VERIFY", "Persisted candidate schema/data OK.");
                }

                if (ok)
                {
                    ok = DB.VerifyDatabasePreservesOriginal(original_db_filename_xml, candidateFile, out string preservationProblems);
                    if (!ok) DB.LogDatabaseEvent("COMPAT_PRESERVATION_FAIL", preservationProblems);
                    else DB.LogDatabaseEvent("COMPAT_PRESERVATION", "Every original non-null value preserved.");
                }

                if (ok)
                {
                    ok = makeDBActive(candidateGuid);
                    if (ok)
                    {
                        _dbman_settings = getActiveDB();
                        DB.LogDatabaseEvent("COMPAT_ACTIVATE", candidateGuid.ToString());
                    }
                }

                if (ok)
                {
                    try { DBWritten(); }
                    catch (Exception ex) { DB.LogDatabaseEvent("COMPAT_METADATA_WARN", "DBWritten metadata update failed.", ex); }
                    if (force_upgrade_via_file) renameUpdatedb();
                    _migration_failed_keep_original = false;
                    return true;
                }

                DB.LogDatabaseEvent("COMPAT_FAIL", "Candidate rejected; original database remains active.");
                return false;
            }
            catch (Exception ex)
            {
                ok = false;
                DB.LogDatabaseEvent("COMPAT_EXCEPTION", "Compatibility upgrade aborted.", ex);
                return false;
            }
            finally
            {
                _ignore_written = false;
                try { if (!string.IsNullOrEmpty(currentDefaultsFile) && File.Exists(currentDefaultsFile)) File.Delete(currentDefaultsFile); } catch { }

                if (!ok)
                {
                    bool rollbackOk = false;
                    try
                    {
                        bool activeOk = guid_original != Guid.Empty && makeDBActive(guid_original);
                        DB.FileName = original_db_filename_xml;
                        _ignore_written = true;
                        bool initOk = DB.Init();
                        _ignore_written = false;
                        _dbman_settings = getActiveDB();
                        rollbackOk = activeOk && initOk && _dbman_settings != null && _dbman_settings.ActiveDB_GUID == guid_original;
                        if (rollbackOk) DB.LogDatabaseEvent("COMPAT_ROLLBACK_OK", guid_original.ToString());
                        else DB.LogDatabaseEvent("COMPAT_ROLLBACK_FAIL", "Original database could not be fully restored.");
                    }
                    catch (Exception ex)
                    {
                        _ignore_written = false;
                        DB.LogDatabaseEvent("COMPAT_ROLLBACK_FAIL", original_db_filename_xml, ex);
                    }

                    if (candidateGuid != Guid.Empty && candidateGuid != guid_original)
                    {
                        try { moveToBroken(candidateGuid); } catch { }
                    }
                    _migration_failed_keep_original = rollbackOk;

                    MessageBox.Show(rollbackOk
                            ? "The database compatibility upgrade failed. The original database was restored and remains active. See Database.log."
                            : "The database compatibility upgrade failed and the original database could not be fully restored. See Database.log.",
                        "Database Manager",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                }
                DB.SetCheckpointingEnabled(true);
            }
        }'''
dbman = replace_method(dbman, "        private static bool checkVersion(bool made_new", check_version)

# Final static contract checks.
required_db = [
    MARKER,
    "MergeMissingDefaultsFromDatabase",
    "VerifyDatabasePreservesOriginal",
    "Every original non-null value preserved",
    "MarkCurrentDatabaseMetadata",
    "CompatibilityIdentityColumns",
    "if (_compatibilityRepairChanged) changed = true;",
    "Rows.Add(newRow)",
]
for token in required_db:
    if token not in db:
        raise RuntimeError(f"database.cs V4 token missing: {token}")

required_dbman = [
    MARKER,
    "File.Copy(original_db_filename_xml, candidateFile, true)",
    "MergeMissingDefaultsFromDatabase",
    "VerifyDatabasePreservesOriginal",
    "COMPAT_METADATA_ONLY",
]
for token in required_dbman:
    if token not in dbman:
        raise RuntimeError(f"clsDBMan.cs V4 token missing: {token}")

# Historical automatic path must not remain in checkVersion.
new_check_start = dbman.find("        // SQ4KOU_DB_COMPAT_PRESERVATION_V4")
new_check_end = dbman.find("        private static void moveToBroken", new_check_start)
new_check = dbman[new_check_start:new_check_end]
if "ImportAndMergeDatabase(original_db_filename_xml" in new_check:
    raise RuntimeError("fresh+partial automatic migration call still present")

DB_FILE.write_text(db, encoding="utf-8-sig", newline="\n")
DBMAN_FILE.write_text(dbman, encoding="utf-8-sig", newline="\n")
print("V4 preservation-first source compatibility patch applied.")