from pathlib import Path

p = Path(r"Project Files/Source/Console/database.cs")
text = p.read_text(encoding="utf-8-sig")


def replace_between(src: str, start_marker: str, end_marker: str, replacement: str, search_from: int = 0):
    start = src.find(start_marker, search_from)
    if start < 0:
        raise RuntimeError(f"start marker not found: {start_marker}")
    end = src.find(end_marker, start)
    if end < 0:
        raise RuntimeError(f"end marker not found: {end_marker}")
    return src[:start] + replacement + src[end:], start + len(replacement)


# -----------------------------------------------------------------------------
# 1) Current-schema validator: require CURRENT fields, tolerate legacy extras.
# -----------------------------------------------------------------------------
validator_start = text.find('            if (data.Tables.Contains("TXProfile") && data.Tables.Contains("TXProfileDef"))')
if validator_start < 0:
    raise RuntimeError("TXProfile validator block not found")
validator_end = text.find('            return string.Join(Environment.NewLine, problems.ToArray());', validator_start)
if validator_end < 0:
    raise RuntimeError("TXProfile validator end not found")

validator_block = r'''            if (data.Tables.Contains("TXProfile") && data.Tables.Contains("TXProfileDef"))
            {
                DataTable tx = data.Tables["TXProfile"];
                DataTable def = data.Tables["TXProfileDef"];
                if (!tx.Columns.Contains("Name")) problems.Add("TXProfile.Name is missing.");
                if (!def.Columns.Contains("Name")) problems.Add("TXProfileDef.Name is missing.");

                // Backward compatibility rule: validate only fields required by THIS build.
                // Extra legacy/future columns are harmless and must not invalidate an old DB.
                try
                {
                    DataTable expected = BuildCurrentTXProfileDefinition();
                    foreach (DataColumn col in expected.Columns)
                    {
                        if (!tx.Columns.Contains(col.ColumnName))
                            problems.Add("TXProfile missing current column: " + col.ColumnName);
                        else if (tx.Columns[col.ColumnName].DataType != col.DataType)
                            problems.Add("TXProfile current column type mismatch: " + col.ColumnName);

                        if (!def.Columns.Contains(col.ColumnName))
                            problems.Add("TXProfileDef missing current column: " + col.ColumnName);
                        else if (def.Columns[col.ColumnName].DataType != col.DataType)
                            problems.Add("TXProfileDef current column type mismatch: " + col.ColumnName);
                    }

                    if (def.Columns.Contains("Name"))
                    {
                        DataRow[] defaults = def.Select("Name = 'Default'");
                        if (defaults.Length != 1)
                            problems.Add("TXProfileDef must contain exactly one Default row; found " + defaults.Length + ".");
                        else
                        {
                            foreach (DataColumn col in expected.Columns)
                            {
                                if (def.Columns.Contains(col.ColumnName) && defaults[0].IsNull(col.ColumnName))
                                    problems.Add("TXProfileDef Default has no value for current field: " + col.ColumnName);
                            }
                        }
                    }

                    foreach (DataRow row in tx.Rows)
                    {
                        if (row.RowState == DataRowState.Deleted) continue;
                        foreach (DataColumn col in expected.Columns)
                        {
                            if (tx.Columns.Contains(col.ColumnName) && row.IsNull(col.ColumnName))
                                problems.Add("TXProfile has no value for current field: " + col.ColumnName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    problems.Add("Unable to validate current TXProfile schema: " + ex.Message);
                }
            }

'''
text = text[:validator_start] + validator_block + text[validator_end:]


# -----------------------------------------------------------------------------
# 2) TX profiles: old non-null values are authoritative; add current defaults
#    only for genuinely new/missing fields; retain extra legacy columns.
# -----------------------------------------------------------------------------
expand_method = r'''        //-W2PA Expand an old TxProfile table into the current schema.
        // Old non-null values are authoritative. Missing/new CURRENT fields use the
        // CURRENT Default row. Extra legacy columns are retained instead of discarded.
        private static DataTable ExpandOldTxProfileTable(DataTable oldTable)
        {
            if (oldTable == null || !oldTable.Columns.Contains("Name")) return null;
            if (!ds.Tables.Contains("TXProfileDef")) return null;

            DataTable dsTXPDefTable = ds.Tables["TXProfileDef"];
            DataTable expandedTable = dsTXPDefTable.Clone();
            DataRow[] defaultRows = dsTXPDefTable.Select("Name = 'Default'");
            if (defaultRows.Length != 1) return null;
            DataRow defaultRow = defaultRows[0];

            // Preserve columns which existed in an old database even when this build
            // no longer knows them. They must not make a legacy database incompatible.
            foreach (DataColumn oldCol in oldTable.Columns)
            {
                if (!expandedTable.Columns.Contains(oldCol.ColumnName))
                {
                    DataColumn extra = expandedTable.Columns.Add(oldCol.ColumnName, oldCol.DataType);
                    extra.AllowDBNull = true;
                }
            }

            foreach (DataRow oldRow in oldTable.Rows)
            {
                if (oldRow.RowState == DataRowState.Deleted) continue;

                string profileName = Convert.ToString(oldRow["Name"]);
                if (string.IsNullOrEmpty(profileName) || profileName == "Default") continue;

                DataRow newRow = expandedTable.NewRow();

                // Seed only CURRENT columns from the current Default row.
                foreach (DataColumn currentCol in dsTXPDefTable.Columns)
                {
                    if (!defaultRow.IsNull(currentCol))
                        newRow[currentCol.ColumnName] = defaultRow[currentCol.ColumnName];
                }

                // Overlay every non-null old value. This is the key compatibility rule:
                // existing user data always wins over a new default.
                foreach (DataColumn sourceCol in oldTable.Columns)
                {
                    if (!expandedTable.Columns.Contains(sourceCol.ColumnName)) continue;
                    object oldValue = oldRow[sourceCol.ColumnName];
                    if (oldValue == null || oldValue == DBNull.Value) continue;

                    DataColumn targetCol = expandedTable.Columns[sourceCol.ColumnName];
                    try
                    {
                        if (sourceCol.DataType == targetCol.DataType || targetCol.DataType.IsAssignableFrom(sourceCol.DataType))
                            newRow[targetCol.ColumnName] = oldValue;
                        else
                            newRow[targetCol.ColumnName] = Convert.ChangeType(oldValue, targetCol.DataType);
                    }
                    catch
                    {
                        // If an obsolete value cannot be converted, keep the current default
                        // for a current field; an extra legacy field remains DBNull.
                    }
                }

                newRow["Name"] = profileName;
                expandedTable.Rows.Add(newRow);
            }

            return expandedTable;
        }

'''
text, _ = replace_between(
    text,
    "        //-W2PA Expand an old TxProfile table into the current schema.",
    "        //-W2PA Write a message to the ImportLog file during the import process",
    expand_method,
)


# -----------------------------------------------------------------------------
# 3) Generic old-authoritative table merge + truncated XML read recovery.
# -----------------------------------------------------------------------------
func_pos = text.find("        public static bool ImportAndMergeDatabase(string filename, out string log, bool ignore_merged)")
if func_pos < 0:
    raise RuntimeError("active ImportAndMergeDatabase not found")

helpers = r'''        private static void CopyCompatibleValues(DataRow source, DataRow target)
        {
            foreach (DataColumn sourceCol in source.Table.Columns)
            {
                if (!target.Table.Columns.Contains(sourceCol.ColumnName)) continue;
                object value = source[sourceCol.ColumnName];
                if (value == null || value == DBNull.Value) continue;

                DataColumn targetCol = target.Table.Columns[sourceCol.ColumnName];
                try
                {
                    if (sourceCol.DataType == targetCol.DataType || targetCol.DataType.IsAssignableFrom(sourceCol.DataType))
                        target[targetCol.ColumnName] = value;
                    else
                        target[targetCol.ColumnName] = Convert.ChangeType(value, targetCol.DataType);
                }
                catch
                {
                    // Keep the seeded/current value if an obsolete value cannot be converted.
                }
            }
        }

        private static string[] LegacyIdentityColumns(DataTable currentTable, DataTable oldTable)
        {
            if (currentTable == null || oldTable == null) return null;
            string name = currentTable.TableName;

            if (name == "BandText" && currentTable.Columns.Contains("Low") && currentTable.Columns.Contains("High") && currentTable.Columns.Contains("Name") &&
                oldTable.Columns.Contains("Low") && oldTable.Columns.Contains("High") && oldTable.Columns.Contains("Name"))
                return new[] { "Low", "High", "Name" };

            if (currentTable.Columns.Contains("Key") && oldTable.Columns.Contains("Key")) return new[] { "Key" };
            if (currentTable.Columns.Contains("GroupID") && oldTable.Columns.Contains("GroupID")) return new[] { "GroupID" };
            if (currentTable.Columns.Contains("GUID") && oldTable.Columns.Contains("GUID")) return new[] { "GUID" };
            if (currentTable.Columns.Contains("Name") && oldTable.Columns.Contains("Name")) return new[] { "Name" };
            return null;
        }

        private static bool RowsMatch(DataRow a, DataRow b, string[] columns)
        {
            if (a == null || b == null || columns == null || columns.Length == 0) return false;
            foreach (string column in columns)
            {
                if (!a.Table.Columns.Contains(column) || !b.Table.Columns.Contains(column)) return false;
                object av = a[column];
                object bv = b[column];
                if (av == DBNull.Value && bv == DBNull.Value) continue;
                if (av == DBNull.Value || bv == DBNull.Value) return false;
                if (!string.Equals(Convert.ToString(av), Convert.ToString(bv), StringComparison.Ordinal)) return false;
            }
            return true;
        }

        private static DataRow FindMatchingRow(DataTable table, DataRow source, string[] columns)
        {
            if (table == null || source == null || columns == null) return null;
            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;
                if (RowsMatch(row, source, columns)) return row;
            }
            return null;
        }

        private static DataTable MergeLegacyTablePreferOld(DataTable currentTable, DataTable oldTable)
        {
            if (currentTable == null) return oldTable == null ? null : oldTable.Copy();
            if (oldTable == null) return currentTable.Copy();

            DataTable result = currentTable.Clone();

            // Preserve legacy-only columns.
            foreach (DataColumn oldCol in oldTable.Columns)
            {
                if (!result.Columns.Contains(oldCol.ColumnName))
                {
                    DataColumn extra = result.Columns.Add(oldCol.ColumnName, oldCol.DataType);
                    extra.AllowDBNull = true;
                }
            }

            string[] identity = LegacyIdentityColumns(currentTable, oldTable);

            // Every old row is retained. Seed a matching current row first, then overlay
            // old values so new fields get defaults without overwriting old user data.
            foreach (DataRow oldRow in oldTable.Rows)
            {
                if (oldRow.RowState == DataRowState.Deleted) continue;
                DataRow newRow = result.NewRow();
                DataRow seed = FindMatchingRow(currentTable, oldRow, identity);

                if (seed != null)
                    CopyCompatibleValues(seed, newRow);
                else
                {
                    foreach (DataColumn col in currentTable.Columns)
                    {
                        if (col.DefaultValue != null && col.DefaultValue != DBNull.Value)
                        {
                            try { newRow[col.ColumnName] = col.DefaultValue; }
                            catch { }
                        }
                    }
                }

                CopyCompatibleValues(oldRow, newRow);
                result.Rows.Add(newRow);
            }

            // For keyable/default tables keep genuinely new rows introduced by this build.
            if (identity != null)
            {
                foreach (DataRow currentRow in currentTable.Rows)
                {
                    if (currentRow.RowState == DataRowState.Deleted) continue;
                    if (FindMatchingRow(result, currentRow, identity) != null) continue;
                    DataRow newRow = result.NewRow();
                    CopyCompatibleValues(currentRow, newRow);
                    result.Rows.Add(newRow);
                }
            }

            return result;
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
                // Compatibility is separate from corruption, but old Thetis versions wrote
                // database.xml non-atomically. A copied legacy file can therefore contain a
                // complete schema and thousands of complete rows followed by one torn tail row.
                // Recover ONLY complete top-level rows; never modify the source file.
                try
                {
                    string xml = File.ReadAllText(filename);
                    int searchFrom = xml.Length;
                    for (int attempt = 0; attempt < 256; attempt++)
                    {
                        int cut = xml.LastIndexOf("\n  </", Math.Max(0, searchFrom - 1), StringComparison.Ordinal);
                        if (cut < 0) break;
                        int closeEnd = xml.IndexOf('>', cut);
                        if (closeEnd < 0) break;

                        string closing = xml.Substring(cut, closeEnd - cut + 1).Trim();
                        searchFrom = cut;
                        if (closing == "</Data>" || closing.IndexOf("schema", StringComparison.OrdinalIgnoreCase) >= 0)
                            continue;

                        string repaired = xml.Substring(0, closeEnd + 1) + Environment.NewLine + "</Data>" + Environment.NewLine;
                        try
                        {
                            DataSet candidate = new DataSet();
                            using (StringReader sr = new StringReader(repaired))
                                candidate.ReadXml(sr);

                            string basicProblems = ValidateDataSet(candidate, false);
                            int totalRows = 0;
                            foreach (DataTable t in candidate.Tables) totalRows += t.Rows.Count;
                            if (string.IsNullOrEmpty(basicProblems) && totalRows > 0)
                            {
                                imported = candidate;
                                recoveryLog = "WARNING: legacy database XML ended with an incomplete/truncated tail. " +
                                    "Recovered " + totalRows + " complete rows; the incomplete final row was ignored. " +
                                    "Original file was not modified. Original read error: " + firstEx.GetType().Name + ": " + firstEx.Message;
                                return true;
                            }
                        }
                        catch
                        {
                            // Try the previous complete top-level row.
                        }
                    }
                }
                catch
                {
                }

                recoveryLog = "Unable to read imported database: " + firstEx.GetType().Name + ": " + firstEx.Message;
                return false;
            }
        }

'''
text = text[:func_pos] + helpers + text[func_pos:]
func_pos = text.find("        public static bool ImportAndMergeDatabase(string filename, out string log, bool ignore_merged)")


# -----------------------------------------------------------------------------
# 4) Use recovery-aware read instead of silent ReadXml failure.
# -----------------------------------------------------------------------------
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
new_read = r'''            // Read in DB to be imported and merged. Legacy files are read normally;
            // a torn/truncated tail from old non-atomic writers gets a conservative recovery pass.
            if (!TryReadImportedDatabaseWithRecovery(filename, out DataSet being_importedDB, out string recoveryLog))
            {
                log += recoveryLog + "\n";
                return false;
            }
            log += "Read <" + filename + ">\n";
            if (!string.IsNullOrEmpty(recoveryLog)) log += recoveryLog + "\n";
            log += "\n";
'''
if old_read not in text[func_pos:]:
    raise RuntimeError("active imported DB ReadXml block not found")
text = text[:func_pos] + text[func_pos:].replace(old_read, new_read, 1)


# -----------------------------------------------------------------------------
# 5) BandText / GroupList / Memory were explicitly NOT imported upstream.
#    That violates 100% backward compatibility. Old rows now win.
# -----------------------------------------------------------------------------
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
                            DataTable preservedLegacy = MergeLegacyTablePreferOld(
                                current_inuseDB_table,
                                being_importedDB.Tables[current_inuseDB_table.TableName]);
                            newDB.Merge(preservedLegacy);
                            log += "Imported legacy-authoritative table <" + current_inuseDB_table.TableName + "> into database.\n";
                        }
                        else
                        {
                            newDB.Merge(current_inuseDB_table);
                            log += "Legacy table not present; retained current <" + current_inuseDB_table.TableName + ">.\n";
                        }
                        break;
'''
if old_data_cases not in text[func_pos:]:
    raise RuntimeError("BandText/GroupList/Memory compatibility block not found")
text = text[:func_pos] + text[func_pos:].replace(old_data_cases, new_data_cases, 1)


# -----------------------------------------------------------------------------
# 6) Key/value tables: retain ALL legacy-only keys, not only a hard-coded list.
# -----------------------------------------------------------------------------
kv_start = text.find('                    case "State":', func_pos)
if kv_start < 0:
    raise RuntimeError("active key/value switch block not found")
kv_merge_marker = text.find('                        // Merge in the assembled temp table into mergedDB', kv_start)
if kv_merge_marker < 0:
    raise RuntimeError("active key/value merge marker not found")

preserve_legacy_keys = r'''                        // 100% backward compatibility: retain legacy-only Key/Value settings too.
                        // The old importer only copied keys already known by the current build (plus a
                        // partial hard-coded list), which silently discarded valid older user settings.
                        if (tempTable.Columns.Contains("Key") && tempMergedTable.Columns.Contains("Key"))
                        {
                            foreach (DataRow oldRow in tempTable.Rows)
                            {
                                if (oldRow.RowState == DataRowState.Deleted) continue;
                                string oldKey = Convert.ToString(oldRow["Key"]);
                                if (string.IsNullOrEmpty(oldKey)) continue;

                                // These are metadata owned by the current build, not user settings.
                                if (tempTable.TableName == "State" &&
                                    (oldKey == "VersionNumber" || oldKey == "Version" || oldKey == "DatabaseSchemaVersion"))
                                    continue;

                                bool exists = false;
                                foreach (DataRow mergedRow in tempMergedTable.Rows)
                                {
                                    if (mergedRow.RowState == DataRowState.Deleted) continue;
                                    if (string.Equals(Convert.ToString(mergedRow["Key"]), oldKey, StringComparison.Ordinal))
                                    {
                                        exists = true;
                                        break;
                                    }
                                }
                                if (!exists)
                                {
                                    tempMergedTable.ImportRow(oldRow);
                                    log += "Preserved legacy-only key <" + tempTable.TableName + ":" + oldKey + ">.\n";
                                }
                            }
                        }

'''
text = text[:kv_merge_marker] + preserve_legacy_keys + text[kv_merge_marker:]


# -----------------------------------------------------------------------------
# 7) Current tables unknown to the old switch must not become empty. Prefer old
#    data where present; otherwise keep current defaults.
# -----------------------------------------------------------------------------
default_start = text.find('                    default:\n                        // Unrecognized table', kv_merge_marker)
if default_start < 0:
    raise RuntimeError("active default switch case not found")
default_break = text.find('                        break;', default_start)
if default_break < 0:
    raise RuntimeError("active default switch break not found")
default_end = default_break + len('                        break;\n')

default_case = r'''                    default:
                        if (being_importedDB.Tables.Contains(current_inuseDB_table.TableName))
                        {
                            DataTable preservedLegacy = MergeLegacyTablePreferOld(
                                current_inuseDB_table,
                                being_importedDB.Tables[current_inuseDB_table.TableName]);
                            newDB.Merge(preservedLegacy);
                            log += "Imported legacy-authoritative unclassified table <" + current_inuseDB_table.TableName + ">.\n";
                        }
                        else
                        {
                            newDB.Merge(current_inuseDB_table);
                            log += "Retained current unclassified table <" + current_inuseDB_table.TableName + ">.\n";
                        }
                        break;
'''
text = text[:default_start] + default_case + text[default_end:]


required = [
    "TryReadImportedDatabaseWithRecovery",
    "MergeLegacyTablePreferOld",
    "Imported legacy-authoritative table",
    "Preserved legacy-only key",
    "Extra legacy/future columns are harmless",
    "Recovered \" + totalRows + \" complete rows",
]
for token in required:
    if token not in text:
        raise RuntimeError(f"V2 patched source verification failed: {token}")

p.write_text(text, encoding="utf-8-sig", newline="\n")
print("DB backward-compatibility V2 patch applied and verified.")
