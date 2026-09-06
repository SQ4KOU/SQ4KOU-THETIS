from pathlib import Path

DB = Path(r"Project Files/Source/Console/database.cs")
MAN = Path(r"Project Files/Source/Console/clsDBMan.cs")

db = DB.read_text(encoding="utf-8-sig")
man = MAN.read_text(encoding="utf-8-sig")


def replace_between(text: str, start: str, end: str, replacement: str, start_at: int = 0):
    a = text.find(start, start_at)
    if a < 0:
        raise RuntimeError(f"start marker not found: {start}")
    b = text.find(end, a + len(start))
    if b < 0:
        raise RuntimeError(f"end marker not found: {end}")
    return text[:a] + replacement + text[b:], a + len(replacement)


# -----------------------------------------------------------------------------
# SQ4KOU DB compatibility rebuild.
# One coherent compatibility layer; no field-specific migration list.
# -----------------------------------------------------------------------------
compat_core = r'''        // SQ4KOU_DB_COMPAT_REBUILD
        // Compatibility policy:
        //   * existing legacy non-null data is authoritative,
        //   * current fields/tables missing from an old DB are added from THIS build,
        //   * unknown legacy tables/columns/keys are retained,
        //   * schema metadata is not by itself a reason to reject a readable old DB,
        //   * a candidate is accepted only when old data preservation can be proven.
        private static readonly object _schemaTemplateSync = new object();

        private static DataSet BuildCurrentSchemaTemplate()
        {
            lock (_schemaTemplateSync)
            {
                DataSet originalDs = ds;
                bool originalSuspend = _suspendDirtyTracking;
                try
                {
                    ds = new DataSet("Data");
                    _suspendDirtyTracking = true;
                    // This is deliberately the same table factory path used by DB.Init().
                    VerifyTables();
                    DataSet template = ds.Copy();
                    template.DataSetName = "Data";
                    return template;
                }
                finally
                {
                    ds = originalDs;
                    _suspendDirtyTracking = originalSuspend;
                }
            }
        }

        private static DataColumn AddColumnLike(DataTable target, DataColumn source)
        {
            DataColumn c = new DataColumn(source.ColumnName, source.DataType);
            c.AllowDBNull = source.AllowDBNull;
            c.Caption = source.Caption;
            c.ColumnMapping = source.ColumnMapping;
            if (source.DataType == typeof(string)) c.MaxLength = source.MaxLength;
            c.Namespace = source.Namespace;
            c.Prefix = source.Prefix;
            c.ReadOnly = source.ReadOnly;
            c.Unique = false; // uniqueness is validated after all rows are reconstructed
            if (source.DefaultValue != null) c.DefaultValue = source.DefaultValue;
            target.Columns.Add(c);
            return c;
        }

        private static bool IsVersionOwnedStateKey(DataRow row)
        {
            if (row == null || row.Table == null || row.Table.TableName != "State" || !row.Table.Columns.Contains("Key")) return false;
            string key = Convert.ToString(row["Key"]);
            return key == "Version" || key == "VersionNumber" || key == "DatabaseSchemaVersion";
        }

        private static string[] IdentityColumns(DataTable table)
        {
            if (table == null) return null;
            string n = table.TableName;
            if (table.Columns.Contains("Key")) return new[] { "Key" };
            if ((n == "TXProfile" || n == "TXProfileDef") && table.Columns.Contains("Name")) return new[] { "Name" };
            if (n == "BandText" && table.Columns.Contains("Low") && table.Columns.Contains("High") && table.Columns.Contains("Name"))
                return new[] { "Low", "High", "Name" };
            if (n == "GroupList" && table.Columns.Contains("GroupID")) return new[] { "GroupID" };
            if (n == "BandStack2HiddenEntries" && table.Columns.Contains("FilterGUID") && table.Columns.Contains("EntryGUID"))
                return new[] { "FilterGUID", "EntryGUID" };
            if (n == "BandStack2FilterBands" && table.Columns.Contains("FilterGUID") && table.Columns.Contains("Band"))
                return new[] { "FilterGUID", "Band" };
            if (n == "BandStack2FilterModes" && table.Columns.Contains("FilterGUID") && table.Columns.Contains("Mode"))
                return new[] { "FilterGUID", "Mode" };
            if (n == "BandStack2FilterSubModes" && table.Columns.Contains("FilterGUID") && table.Columns.Contains("SubMode"))
                return new[] { "FilterGUID", "SubMode" };
            if (n == "BandStack2FilterFrequencies" && table.Columns.Contains("FilterGUID") && table.Columns.Contains("Low") && table.Columns.Contains("High"))
                return new[] { "FilterGUID", "Low", "High" };
            if (table.Columns.Contains("GUID")) return new[] { "GUID" };
            // Name is useful for factory/profile-like tables, but not as a blanket rule for arbitrary data.
            return null;
        }

        private static bool DataValuesEqual(object a, object b)
        {
            if (a == null || a == DBNull.Value) return b == null || b == DBNull.Value;
            if (b == null || b == DBNull.Value) return false;
            if (a is byte[] ba && b is byte[] bb)
            {
                if (ba.Length != bb.Length) return false;
                for (int i = 0; i < ba.Length; i++) if (ba[i] != bb[i]) return false;
                return true;
            }
            if (a.Equals(b)) return true;
            return string.Equals(Convert.ToString(a, System.Globalization.CultureInfo.InvariantCulture),
                                 Convert.ToString(b, System.Globalization.CultureInfo.InvariantCulture),
                                 StringComparison.Ordinal);
        }

        private static bool RowsHaveSameIdentity(DataRow a, DataRow b, string[] identity)
        {
            if (a == null || b == null || identity == null || identity.Length == 0) return false;
            foreach (string c in identity)
            {
                if (!a.Table.Columns.Contains(c) || !b.Table.Columns.Contains(c)) return false;
                if (!DataValuesEqual(a[c], b[c])) return false;
            }
            return true;
        }

        private static DataRow FindByIdentity(DataTable table, DataRow source, string[] identity)
        {
            if (table == null || source == null || identity == null) return null;
            foreach (DataRow r in table.Rows)
            {
                if (r.RowState == DataRowState.Deleted) continue;
                if (RowsHaveSameIdentity(r, source, identity)) return r;
            }
            return null;
        }

        private static void CopyValuesPreferSource(DataRow source, DataRow target, bool throwOnConversionFailure)
        {
            foreach (DataColumn sc in source.Table.Columns)
            {
                if (!target.Table.Columns.Contains(sc.ColumnName)) continue;
                object value = source[sc.ColumnName];
                if (value == null || value == DBNull.Value) continue;
                DataColumn tc = target.Table.Columns[sc.ColumnName];
                try
                {
                    if (sc.DataType == tc.DataType || tc.DataType.IsAssignableFrom(sc.DataType))
                        target[tc.ColumnName] = value;
                    else if (tc.DataType.IsEnum)
                        target[tc.ColumnName] = Enum.Parse(tc.DataType, Convert.ToString(value), true);
                    else
                        target[tc.ColumnName] = Convert.ChangeType(value, tc.DataType, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    if (throwOnConversionFailure)
                        throw new InvalidDataException("Cannot preserve value " + source.Table.TableName + "." + sc.ColumnName +
                            " while converting " + sc.DataType.FullName + " -> " + tc.DataType.FullName + ".", ex);
                }
            }
        }

        private static DataRow CurrentSeedForLegacyRow(DataTable currentTable, DataRow oldRow)
        {
            string[] identity = IdentityColumns(currentTable);
            DataRow seed = FindByIdentity(currentTable, oldRow, identity);
            if (seed != null) return seed;

            // A user TX profile normally has no same-name factory row. New fields must come
            // from the CURRENT Default definition, not from a hard-coded field value.
            if ((currentTable.TableName == "TXProfile" || currentTable.TableName == "TXProfileDef") &&
                currentTable.DataSet != null && currentTable.DataSet.Tables.Contains("TXProfileDef"))
            {
                DataTable def = currentTable.DataSet.Tables["TXProfileDef"];
                if (def.Columns.Contains("Name"))
                {
                    DataRow[] defaults = def.Select("Name = 'Default'");
                    if (defaults.Length == 1) return defaults[0];
                }
            }
            return null;
        }

        private static DataTable MergeLegacyTablePreferOld(DataTable currentTable, DataTable oldTable)
        {
            if (oldTable == null) return currentTable == null ? null : currentTable.Copy();
            if (currentTable == null) return oldTable.Copy();

            // Start from the legacy schema so unknown columns, metadata and ordering survive.
            DataTable result = oldTable.Clone();
            result.TableName = currentTable.TableName;

            // Add every field introduced by the current build. Overlapping fields must remain
            // type-compatible; silent type substitution would violate lossless migration.
            foreach (DataColumn cc in currentTable.Columns)
            {
                if (!result.Columns.Contains(cc.ColumnName))
                    AddColumnLike(result, cc);
                else if (result.Columns[cc.ColumnName].DataType != cc.DataType)
                {
                    // Historical type changes are allowed only if every old non-null value can
                    // be represented by the current type. Rebuild this table from current schema
                    // plus legacy-only columns; preservation validation checks every old value.
                    DataTable converted = currentTable.Clone();
                    foreach (DataColumn oc in oldTable.Columns)
                        if (!converted.Columns.Contains(oc.ColumnName)) AddColumnLike(converted, oc);

                    foreach (DataRow oldRow in oldTable.Rows)
                    {
                        if (oldRow.RowState == DataRowState.Deleted) continue;
                        DataRow nr = converted.NewRow();
                        DataRow seed = CurrentSeedForLegacyRow(currentTable, oldRow);
                        if (seed != null) CopyValuesPreferSource(seed, nr, false);
                        CopyValuesPreferSource(oldRow, nr, true);
                        converted.Rows.Add(nr);
                    }
                    string[] convertedIdentity = IdentityColumns(currentTable);
                    if (convertedIdentity != null)
                    {
                        foreach (DataRow cr in currentTable.Rows)
                        {
                            if (cr.RowState == DataRowState.Deleted) continue;
                            if (FindByIdentity(converted, cr, convertedIdentity) != null) continue;
                            DataRow nr = converted.NewRow();
                            CopyValuesPreferSource(cr, nr, true);
                            converted.Rows.Add(nr);
                        }
                    }
                    return converted;
                }
            }

            foreach (DataRow oldRow in oldTable.Rows)
            {
                if (oldRow.RowState == DataRowState.Deleted) continue;
                DataRow nr = result.NewRow();
                DataRow seed = CurrentSeedForLegacyRow(currentTable, oldRow);
                if (seed != null) CopyValuesPreferSource(seed, nr, false);
                // Old non-null data is always last, therefore authoritative.
                CopyValuesPreferSource(oldRow, nr, true);
                result.Rows.Add(nr);
            }

            // Add genuinely new factory/default rows only when a stable identity exists.
            string[] identity = IdentityColumns(currentTable);
            if (identity != null)
            {
                foreach (DataRow cr in currentTable.Rows)
                {
                    if (cr.RowState == DataRowState.Deleted) continue;
                    if (FindByIdentity(result, cr, identity) != null) continue;
                    DataRow nr = result.NewRow();
                    CopyValuesPreferSource(cr, nr, true);
                    result.Rows.Add(nr);
                }
            }
            return result;
        }

        private static bool RowPreserved(DataRow oldRow, DataTable candidateTable)
        {
            string[] identity = IdentityColumns(oldRow.Table);
            if (identity != null)
            {
                DataRow candidate = FindByIdentity(candidateTable, oldRow, identity);
                if (candidate == null) return false;
                foreach (DataColumn oc in oldRow.Table.Columns)
                {
                    if (!candidateTable.Columns.Contains(oc.ColumnName)) return false;
                    object ov = oldRow[oc.ColumnName];
                    if (ov == null || ov == DBNull.Value) continue; // a missing legacy value may be repaired by a current default
                    if (oldRow.Table.TableName == "State" && oc.ColumnName == "Value" && IsVersionOwnedStateKey(oldRow)) continue;
                    if (!DataValuesEqual(ov, candidate[oc.ColumnName])) return false;
                }
                return true;
            }

            // No stable key: require a row containing every old non-null value.
            foreach (DataRow candidate in candidateTable.Rows)
            {
                if (candidate.RowState == DataRowState.Deleted) continue;
                bool match = true;
                foreach (DataColumn oc in oldRow.Table.Columns)
                {
                    if (!candidateTable.Columns.Contains(oc.ColumnName)) { match = false; break; }
                    object ov = oldRow[oc.ColumnName];
                    if (ov == null || ov == DBNull.Value) continue;
                    if (!DataValuesEqual(ov, candidate[oc.ColumnName])) { match = false; break; }
                }
                if (match) return true;
            }
            return false;
        }

        private static bool ValidateLegacyDataPreserved(DataSet oldData, DataSet candidate, out string problem)
        {
            problem = "";
            if (oldData == null || candidate == null) { problem = "Source or candidate DataSet is null."; return false; }

            foreach (DataTable oldTable in oldData.Tables)
            {
                if (!candidate.Tables.Contains(oldTable.TableName))
                {
                    problem = "Candidate lost legacy table '" + oldTable.TableName + "'.";
                    return false;
                }
                DataTable ct = candidate.Tables[oldTable.TableName];
                foreach (DataColumn oc in oldTable.Columns)
                {
                    if (!ct.Columns.Contains(oc.ColumnName))
                    {
                        problem = "Candidate lost legacy column '" + oldTable.TableName + "." + oc.ColumnName + "'.";
                        return false;
                    }
                }
                if (IdentityColumns(oldTable) == null && ct.Rows.Count < oldTable.Rows.Count)
                {
                    problem = "Candidate has fewer rows than source table '" + oldTable.TableName + "'.";
                    return false;
                }
                foreach (DataRow oldRow in oldTable.Rows)
                {
                    if (oldRow.RowState == DataRowState.Deleted) continue;
                    if (!RowPreserved(oldRow, ct))
                    {
                        problem = "Candidate does not preserve a legacy row/value from table '" + oldTable.TableName + "'.";
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool TryReadImportedDatabaseWithRecovery(string filename, out DataSet imported, out string recoveryLog)
        {
            imported = null;
            recoveryLog = "";
            try
            {
                DataSet normal = new DataSet();
                normal.ReadXml(filename);
                imported = normal;
                return true;
            }
            catch (Exception firstEx)
            {
                // Old Thetis versions wrote database.xml non-atomically. Recovery is intentionally
                // conservative: keep only complete top-level rows and never alter the source file.
                try
                {
                    string xml = File.ReadAllText(filename);
                    int searchFrom = xml.Length;
                    for (int attempt = 0; attempt < 512; attempt++)
                    {
                        int cut = xml.LastIndexOf("\n  </", Math.Max(0, searchFrom - 1), StringComparison.Ordinal);
                        if (cut < 0) break;
                        int closeEnd = xml.IndexOf('>', cut);
                        if (closeEnd < 0) break;
                        string closing = xml.Substring(cut, closeEnd - cut + 1).Trim();
                        searchFrom = cut;
                        if (closing == "</Data>" || closing.IndexOf("schema", StringComparison.OrdinalIgnoreCase) >= 0) continue;

                        string repaired = xml.Substring(0, closeEnd + 1) + Environment.NewLine + "</Data>" + Environment.NewLine;
                        try
                        {
                            DataSet candidate = new DataSet();
                            using (StringReader sr = new StringReader(repaired)) candidate.ReadXml(sr);
                            string basic = ValidateDataSet(candidate, false);
                            if (string.IsNullOrEmpty(basic) && candidate.Tables.Count > 0)
                            {
                                imported = candidate;
                                recoveryLog = "WARNING: recovered complete rows from a truncated legacy XML tail. Original file was not modified. " +
                                    firstEx.GetType().Name + ": " + firstEx.Message;
                                return true;
                            }
                        }
                        catch { }
                    }
                }
                catch { }

                recoveryLog = "Unable to read database: " + firstEx.GetType().Name + ": " + firstEx.Message;
                return false;
            }
        }

        private static bool NormalizeDataSetForCurrentBuild(DataSet source, out DataSet normalized, out bool changed, out string report)
        {
            normalized = null;
            changed = false;
            report = "";
            if (source == null) { report = "Source DataSet is null."; return false; }

            try
            {
                DataSet current = BuildCurrentSchemaTemplate();
                DataSet work = source.Copy();
                work.DataSetName = "Data";
                if (work.Relations.Count != 0)
                    throw new InvalidDataException("Unexpected DataRelations are present; refusing a lossy table replacement.");

                foreach (DataTable currentTable in current.Tables)
                {
                    if (!work.Tables.Contains(currentTable.TableName))
                    {
                        work.Tables.Add(currentTable.Copy());
                        changed = true;
                        report += "Added current table: " + currentTable.TableName + Environment.NewLine;
                        continue;
                    }

                    DataTable oldTable = work.Tables[currentTable.TableName];
                    DataTable merged = MergeLegacyTablePreferOld(currentTable, oldTable);

                    // Detect a real change before replacing. Schema/row count is enough for logging;
                    // preservation validation below is the authoritative safety gate.
                    if (oldTable.Columns.Count != merged.Columns.Count || oldTable.Rows.Count != merged.Rows.Count) changed = true;
                    else
                    {
                        foreach (DataColumn cc in currentTable.Columns)
                        {
                            if (!oldTable.Columns.Contains(cc.ColumnName)) { changed = true; break; }
                        }
                        if (!changed)
                        {
                            foreach (DataRow oldRow in oldTable.Rows)
                            {
                                string[] id = IdentityColumns(oldTable);
                                DataRow mr = id == null ? null : FindByIdentity(merged, oldRow, id);
                                if (mr != null)
                                {
                                    foreach (DataColumn cc in currentTable.Columns)
                                    {
                                        if (oldTable.Columns.Contains(cc.ColumnName) && oldRow[cc.ColumnName] == DBNull.Value && mr[cc.ColumnName] != DBNull.Value)
                                        { changed = true; break; }
                                    }
                                }
                                if (changed) break;
                            }
                        }
                    }

                    work.Tables.Remove(currentTable.TableName);
                    work.Tables.Add(merged);
                }

                if (!ValidateLegacyDataPreserved(source, work, out string preservationProblem))
                    throw new InvalidDataException(preservationProblem);

                normalized = work;
                return true;
            }
            catch (Exception ex)
            {
                report = ex.GetType().Name + ": " + ex.Message;
                normalized = null;
                return false;
            }
        }

        public static bool ValidateDatabasePreservesSource(string sourceFilename, string candidateFilename, out string problems)
        {
            problems = "";
            try
            {
                if (!TryReadImportedDatabaseWithRecovery(sourceFilename, out DataSet source, out string sourceRead))
                { problems = sourceRead; return false; }
                DataSet candidate = new DataSet();
                candidate.ReadXml(candidateFilename);
                if (!ValidateLegacyDataPreserved(source, candidate, out problems)) return false;
                return true;
            }
            catch (Exception ex)
            {
                problems = ex.GetType().Name + ": " + ex.Message;
                return false;
            }
        }

'''

# Insert compatibility core immediately before validation.
validate_marker = "        private static string ValidateDataSet(DataSet data, bool requireCurrentSchema)"
pos = db.find(validate_marker)
if pos < 0:
    raise RuntimeError("ValidateDataSet marker not found")
db = db[:pos] + compat_core + db[pos:]

# Replace current-schema validator with a template-driven minimum-current validator.
new_validator = r'''        private static string ValidateDataSet(DataSet data, bool requireCurrentSchema)
        {
            List<string> problems = new List<string>();
            if (data == null)
            {
                problems.Add("DataSet is null.");
                return string.Join(Environment.NewLine, problems.ToArray());
            }
            if (data.HasErrors) problems.Add("DataSet.HasErrors=true.");
            if (data.DataSetName != "Data") problems.Add("Unexpected DataSetName: " + data.DataSetName);
            if (data.Tables.Count == 0) problems.Add("Database contains no tables.");
            foreach (DataTable table in data.Tables)
            {
                if (table.HasErrors) problems.Add("Table has errors: " + table.TableName);
                foreach (DataRow row in table.Rows)
                    if (row.HasErrors) problems.Add("Row has errors in table: " + table.TableName);
            }
            if (!requireCurrentSchema) return string.Join(Environment.NewLine, problems.ToArray());

            DataSet expected;
            try { expected = BuildCurrentSchemaTemplate(); }
            catch (Exception ex)
            {
                problems.Add("Could not build current database schema template: " + ex.Message);
                return string.Join(Environment.NewLine, problems.ToArray());
            }

            // Minimum-current contract: every table/column required by THIS build must exist
            // and have the current type. Extra legacy/future tables and columns are valid.
            foreach (DataTable et in expected.Tables)
            {
                if (!data.Tables.Contains(et.TableName))
                {
                    problems.Add("Missing current table: " + et.TableName);
                    continue;
                }
                DataTable at = data.Tables[et.TableName];
                foreach (DataColumn ec in et.Columns)
                {
                    if (!at.Columns.Contains(ec.ColumnName))
                        problems.Add(et.TableName + " missing current column: " + ec.ColumnName);
                    else if (at.Columns[ec.ColumnName].DataType != ec.DataType)
                        problems.Add(et.TableName + "." + ec.ColumnName + " has type " + at.Columns[ec.ColumnName].DataType.FullName +
                            "; expected " + ec.DataType.FullName + ".");
                }
            }

            if (data.Tables.Contains("State"))
            {
                DataTable state = data.Tables["State"];
                if (!state.Columns.Contains("Key") || !state.Columns.Contains("Value"))
                    problems.Add("State Key/Value schema is invalid.");
                else
                {
                    HashSet<string> keys = new HashSet<string>();
                    string schemaValue = null;
                    foreach (DataRow row in state.Rows)
                    {
                        if (row.RowState == DataRowState.Deleted) continue;
                        string key = Convert.ToString(row["Key"]);
                        if (!keys.Add(key)) problems.Add("Duplicate State key: " + key);
                        if (key == "DatabaseSchemaVersion") schemaValue = Convert.ToString(row["Value"]);
                    }
                    if (!int.TryParse(schemaValue, out int schema) || schema != CurrentDatabaseSchemaVersion)
                        problems.Add("DatabaseSchemaVersion=" + (schemaValue ?? "<missing>") + ", expected " + CurrentDatabaseSchemaVersion + ".");
                    if (!keys.Contains("VersionNumber")) problems.Add("State.VersionNumber is missing.");
                    if (!keys.Contains("Version")) problems.Add("State.Version is missing.");
                }
            }

            foreach (string tn in new[] { "TXProfile", "TXProfileDef" })
            {
                if (!data.Tables.Contains(tn) || !expected.Tables.Contains(tn)) continue;
                DataTable at = data.Tables[tn];
                DataTable et = expected.Tables[tn];
                if (!at.Columns.Contains("Name")) { problems.Add(tn + ".Name is missing."); continue; }
                HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (DataRow row in at.Rows)
                {
                    if (row.RowState == DataRowState.Deleted) continue;
                    string name = Convert.ToString(row["Name"]);
                    if (!names.Add(name)) problems.Add("Duplicate " + tn + " name: " + name);
                    foreach (DataColumn ec in et.Columns)
                    {
                        if (at.Columns.Contains(ec.ColumnName) && row.IsNull(ec.ColumnName))
                            problems.Add(tn + " '" + name + "' has no value for current field '" + ec.ColumnName + "'.");
                    }
                }
                if (tn == "TXProfileDef")
                {
                    DataRow[] defaults = at.Select("Name = 'Default'");
                    if (defaults.Length != 1) problems.Add("TXProfileDef must contain exactly one Default row; found " + defaults.Length + ".");
                }
            }

            foreach (string tn in new[] { "BandStack2Entries", "BandStack2Filters" })
            {
                if (!data.Tables.Contains(tn) || !data.Tables[tn].Columns.Contains("GUID")) continue;
                HashSet<string> guids = new HashSet<string>();
                foreach (DataRow row in data.Tables[tn].Rows)
                {
                    if (row.RowState == DataRowState.Deleted) continue;
                    string guid = Convert.ToString(row["GUID"]);
                    if (string.IsNullOrEmpty(guid)) problems.Add(tn + " contains an empty GUID.");
                    else if (!guids.Add(guid)) problems.Add("Duplicate " + tn + " GUID: " + guid);
                }
            }

            return string.Join(Environment.NewLine, problems.ToArray());
        }

'''
db, _ = replace_between(db, validate_marker, "        private static bool TryReadDatabaseFile", new_validator)

# Replace IsDatabaseCompatible with the same canonical validator used by writes/candidates.
compat_method = r'''        public static bool IsDatabaseCompatible(out string reason)
        {
            reason = ValidateDataSet(ds, true);
            return string.IsNullOrEmpty(reason);
        }

'''
db, _ = replace_between(db, "        public static bool IsDatabaseCompatible(out string reason)", "        #region BandStack2", compat_method)

# Normalize an old but readable DB in-memory before DBMan decides whether a version migration is needed.
schema_start = '            if (!d.ContainsKey("DatabaseSchemaVersion"))'
schema_end = '            if (changed)\n                SaveVarsDictionary("State", ref d, true);'
new_schema_block = r'''            int sourceSchemaVersion = 0;
            if (d.ContainsKey("DatabaseSchemaVersion"))
            {
                if (!int.TryParse(d["DatabaseSchemaVersion"], out sourceSchemaVersion))
                {
                    LogDatabaseEvent("VALIDATE_FAIL", "Invalid DatabaseSchemaVersion value.");
                    _suspendDirtyTracking = false;
                    return false;
                }
                if (sourceSchemaVersion > CurrentDatabaseSchemaVersion)
                {
                    LogDatabaseEvent("VALIDATE_FAIL", "Database schema is newer than this Thetis build: " + sourceSchemaVersion);
                    _suspendDirtyTracking = false;
                    return false;
                }
            }

            DataSet sourceBeforeNormalization = ds.Copy();
            if (!NormalizeDataSetForCurrentBuild(ds, out DataSet normalizedData, out bool normalizedChanged, out string normalizationReport))
            {
                LogDatabaseEvent("COMPAT_NORMALIZE_FAIL", normalizationReport);
                _suspendDirtyTracking = false;
                return false;
            }
            ds = normalizedData;
            if (normalizedChanged)
            {
                changed = true;
                LogDatabaseEvent("COMPAT_NORMALIZE", normalizationReport);
            }

            // Schema version is a migration marker, not a compatibility veto. Once the current
            // schema contract has been satisfied and old data preservation was proven, promote it.
            LoadedDatabaseSchemaVersion = CurrentDatabaseSchemaVersion;
            d = GetVarsDictionary("State");
            string currentSchemaText = CurrentDatabaseSchemaVersion.ToString();
            if (!d.ContainsKey("DatabaseSchemaVersion") || d["DatabaseSchemaVersion"] != currentSchemaText)
            {
                d["DatabaseSchemaVersion"] = currentSchemaText;
                changed = true;
            }

            if (!ValidateLegacyDataPreserved(sourceBeforeNormalization, ds, out string initPreservationProblem))
            {
                LogDatabaseEvent("COMPAT_PRESERVE_FAIL", initPreservationProblem);
                _suspendDirtyTracking = false;
                return false;
            }

'''
db, _ = replace_between(db, schema_start, schema_end, new_schema_block)

# Rebuild old TX profiles with current defaults while keeping every old value/extra column.
expand_start = "        //-W2PA Expand an old TxProfile table into a newer one with more colunms. Fill in missing ones with default values."
if expand_start not in db:
    expand_start = "        //-W2PA Expand an old TxProfile table into the current schema."
new_expand = r'''        //-W2PA Expand an old TXProfile table into the current schema without losing legacy data.
        private static DataTable ExpandOldTxProfileTable(DataTable oldTable)
        {
            if (oldTable == null || !oldTable.Columns.Contains("Name")) return null;
            if (!ds.Tables.Contains("TXProfile")) return null;
            return MergeLegacyTablePreferOld(ds.Tables["TXProfile"], oldTable);
        }

'''
db, _ = replace_between(db, expand_start, "        //-W2PA Write a message to the ImportLog file during the import process", new_expand)

# Active import: recovery-aware read.
func_pos = db.find("        public static bool ImportAndMergeDatabase(string filename, out string log, bool ignore_merged)")
if func_pos < 0: raise RuntimeError("active ImportAndMergeDatabase not found")
old_read = r'''            // Read in DB to be imported and merged 
            DataSet being_importedDB = new DataSet();
            try
            {
                being_importedDB.ReadXml(filename);
            }
            catch (Exception)
            {
                return false;
            }
            log += "Read <" + filename + ">\n\n";
'''
new_read = r'''            // Read old DB without rejecting a recoverable torn tail from legacy non-atomic writers.
            if (!TryReadImportedDatabaseWithRecovery(filename, out DataSet being_importedDB, out string recoveryLog))
            {
                log += recoveryLog + "\n";
                return false;
            }
            log += "Read <" + filename + ">\n";
            if (!string.IsNullOrEmpty(recoveryLog)) log += recoveryLog + "\n";
            log += "\n";
'''
active = db[func_pos:]
if old_read not in active: raise RuntimeError("active import ReadXml block not found")
active = active.replace(old_read, new_read, 1)
db = db[:func_pos] + active
func_pos = db.find("        public static bool ImportAndMergeDatabase(string filename, out string log, bool ignore_merged)")

# BandText/GroupList/Memory: old content must not be discarded.
old_data_cases = r'''                    case "BandText":
                    case "GroupList":
                    case "Memory":
                        newDB.Merge(current_inuseDB_table); // don't overwrite current tables for these cases
                        log += "Did not import table <" + current_inuseDB_table.TableName + "> into database.\n";
                        break;
'''
new_data_cases = r'''                    case "BandText":
                    case "GroupList":
                    case "Memory":
                        if (being_importedDB.Tables.Contains(current_inuseDB_table.TableName))
                        {
                            DataTable legacyMerged = MergeLegacyTablePreferOld(current_inuseDB_table, being_importedDB.Tables[current_inuseDB_table.TableName]);
                            newDB.Merge(legacyMerged);
                            log += "Imported legacy-authoritative table <" + current_inuseDB_table.TableName + ">.\n";
                        }
                        else
                            newDB.Merge(current_inuseDB_table);
                        break;
'''
if old_data_cases not in db[func_pos:]: raise RuntimeError("BandText/GroupList/Memory import block not found")
db = db[:func_pos] + db[func_pos:].replace(old_data_cases, new_data_cases, 1)
func_pos = db.find("        public static bool ImportAndMergeDatabase(string filename, out string log, bool ignore_merged)")

# TXProfile: one generic old-wins merge, no detached-row bug and no hard-coded fields.
case_start = db.find('                    case "TXProfile":', func_pos)
case_end = db.find('                    case "TXProfileDef":', case_start)
if case_start < 0 or case_end < 0: raise RuntimeError("TXProfile cases not found")
tx_case = r'''                    case "TXProfile":
                        if (being_importedDB.Tables.Contains("TXProfile"))
                        {
                            DataTable mergedProfiles = MergeLegacyTablePreferOld(current_inuseDB_table, being_importedDB.Tables["TXProfile"]);
                            newDB.Merge(mergedProfiles);
                            log += "Imported legacy-authoritative table <TXProfile>.\n";
                        }
                        else
                            newDB.Merge(current_inuseDB_table);
                        break;

'''
db = db[:case_start] + tx_case + db[case_end:]
func_pos = db.find("        public static bool ImportAndMergeDatabase(string filename, out string log, bool ignore_merged)")

# TXProfileDef: preserve old definitions/extra fields while adding current definitions/new fields.
old_def = r'''                    case "TXProfileDef":
                        newDB.Merge(current_inuseDB_table); // don't overwrite current table of defaults
                        log += "Did not import table <" + current_inuseDB_table.TableName + "> into database.\n";
                        break;
'''
new_def = r'''                    case "TXProfileDef":
                        if (being_importedDB.Tables.Contains("TXProfileDef"))
                        {
                            DataTable mergedDefs = MergeLegacyTablePreferOld(current_inuseDB_table, being_importedDB.Tables["TXProfileDef"]);
                            newDB.Merge(mergedDefs);
                            log += "Imported legacy-authoritative table <TXProfileDef>.\n";
                        }
                        else
                            newDB.Merge(current_inuseDB_table);
                        break;
'''
if old_def not in db[func_pos:]: raise RuntimeError("TXProfileDef block not found")
db = db[:func_pos] + db[func_pos:].replace(old_def, new_def, 1)
func_pos = db.find("        public static bool ImportAndMergeDatabase(string filename, out string log, bool ignore_merged)")

# Preserve all old-only keys in recognized Key/Value tables after existing specialized migrations run.
anchor = db.find("                        // this block of code uses a string that contains all the default settings", func_pos)
if anchor < 0: raise RuntimeError("State default-settings block not found")
merge_marker = "                        // Merge in the assembled temp table into mergedDB \n                        newDB.Merge(tempMergedTable);"
merge_pos = db.find(merge_marker, anchor)
if merge_pos < 0: raise RuntimeError("key/value merge marker not found")
preserve_keys = r'''                        // Compatibility rebuild: preserve every legacy-only Key/Value entry, including
                        // settings unknown to this build. Existing/current keys above remain authoritative only
                        // where Thetis explicitly owns migration semantics (Version*, schema, etc.).
                        if (tempTable.Columns.Contains("Key") && tempMergedTable.Columns.Contains("Key"))
                        {
                            foreach (DataRow legacyRow in tempTable.Rows)
                            {
                                if (legacyRow.RowState == DataRowState.Deleted) continue;
                                string legacyKey = Convert.ToString(legacyRow["Key"]);
                                string escapedKey = legacyKey.Replace("'", "''");
                                DataRow[] already = tempMergedTable.Select("Key = '" + escapedKey + "'");
                                if (already.Length == 0)
                                {
                                    tempMergedTable.ImportRow(legacyRow);
                                    log += "Preserved legacy-only key <" + current_inuseDB_table.TableName + ":" + legacyKey + ">.\n";
                                }
                            }
                        }

'''
db = db[:merge_pos] + preserve_keys + db[merge_pos:]
func_pos = db.find("        public static bool ImportAndMergeDatabase(string filename, out string log, bool ignore_merged)")

# Default shared tables: never drop an unrecognized old table merely because it is not in the switch.
default_old = r'''                    default:
                        // Unrecognized table
                        log += "Unrecognized table: " + current_inuseDB_table.TableName + "\n";
                        break;
'''
default_new = r'''                    default:
                        if (being_importedDB.Tables.Contains(current_inuseDB_table.TableName))
                        {
                            DataTable genericMerged = MergeLegacyTablePreferOld(current_inuseDB_table, being_importedDB.Tables[current_inuseDB_table.TableName]);
                            newDB.Merge(genericMerged);
                            log += "Imported unrecognized/shared legacy table <" + current_inuseDB_table.TableName + "> without discarding old data.\n";
                        }
                        else
                        {
                            newDB.Merge(current_inuseDB_table);
                            log += "Retained new current table <" + current_inuseDB_table.TableName + ">.\n";
                        }
                        break;
'''
if default_old not in db[func_pos:]: raise RuntimeError("active default import case not found")
db = db[:func_pos] + db[func_pos:].replace(default_old, default_new, 1)

# Before accepting an in-memory import, prove that all readable old data survived.
activate_marker = r'''            // If we've gotten this far, activate the newly merged DB
            if (!ignore_merged) _merged = true;
            ds = newDB.Copy();
'''
activate_new = r'''            // If we've gotten this far, first prove the merge did not lose old data.
            if (!ValidateLegacyDataPreserved(being_importedDB, newDB, out string preserveProblem))
            {
                log += "Compatibility preservation check failed: " + preserveProblem + "\n";
                return false;
            }

            if (!ignore_merged) _merged = true;
            ds = newDB.Copy();
'''
if activate_marker not in db[func_pos:]: raise RuntimeError("import activation marker not found")
db = db[:func_pos] + db[func_pos:].replace(activate_marker, activate_new, 1)

# clsDBMan: candidate must also preserve source after actual WriteXml/ReadXml round-trip.
persist_marker = r'''                if (ok)
                {
                    ok = DB.ValidateDatabaseFile(candidateFile, true, out string persistedProblems);
                    if (!ok) DB.LogDatabaseEvent("MIGRATE_VERIFY_FAIL", persistedProblems);
                    else DB.LogDatabaseEvent("MIGRATE_VERIFY", "Persisted candidate OK.");
                }

'''
if persist_marker not in man:
    raise RuntimeError("clsDBMan persisted candidate validation block not found")
persist_new = persist_marker + r'''                if (ok)
                {
                    ok = DB.ValidateDatabasePreservesSource(original_db_filename_xml, candidateFile, out string preservationProblems);
                    if (!ok) DB.LogDatabaseEvent("MIGRATE_PRESERVE_FAIL", preservationProblems);
                    else DB.LogDatabaseEvent("MIGRATE_PRESERVE", "Persisted candidate preserves source data.");
                }

'''
man = man.replace(persist_marker, persist_new, 1)

# Static guards against the exact regressions already observed.
required_db = [
    "SQ4KOU_DB_COMPAT_REBUILD",
    "BuildCurrentSchemaTemplate",
    "NormalizeDataSetForCurrentBuild",
    "ValidateLegacyDataPreserved",
    "ValidateDatabasePreservesSource",
    "Imported legacy-authoritative table <TXProfile>",
    "Preserved legacy-only key",
    "schema metadata is not by itself a reason",
]
for token in required_db:
    if token not in db: raise RuntimeError("database.cs missing guard token: " + token)
if "expandedTable.ImportRow(newRow)" in db:
    raise RuntimeError("detached NewRow/ImportRow migration bug is still present")
if 'newDB.Merge(current_inuseDB_table); // don\'t overwrite current tables for these cases' in db:
    raise RuntimeError("legacy BandText/GroupList/Memory discard path is still present")
if "MIGRATE_PRESERVE" not in man:
    raise RuntimeError("clsDBMan persisted preservation gate missing")

DB.write_text(db, encoding="utf-8-sig", newline="\n")
MAN.write_text(man, encoding="utf-8-sig", newline="\n")
print("DB compatibility rebuild applied successfully.")