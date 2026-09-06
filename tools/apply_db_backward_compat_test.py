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


compat_method = r'''        private static DataTable BuildCurrentTXProfileDefinition()
        {
            // Build the canonical TX-profile schema directly from the current source code.
            // This avoids comparing an old TXProfile table only with its equally-old TXProfileDef.
            DataSet original = ds;
            try
            {
                DataSet scratch = new DataSet("Data");
                ds = scratch;
                AddTXProfileTable("__CurrentTXProfileDef", true);
                DataTable expected = scratch.Tables["__CurrentTXProfileDef"].Copy();
                expected.TableName = "TXProfileDef";
                return expected;
            }
            finally
            {
                ds = original;
            }
        }

        /// <summary>
        /// Checks whether the loaded database contains every TX-profile field required by
        /// this build. Extra legacy/future columns are tolerated. Missing/currently-invalid
        /// fields trigger the normal safe create+merge migration path.
        /// </summary>
        public static bool IsDatabaseCompatible(out string reason)
        {
            reason = "";

            if (ds == null)
            {
                reason = "Dataset is null.";
                return false;
            }

            foreach (string tableName in new[] { "TXProfile", "TXProfileDef", "State" })
            {
                if (!ds.Tables.Contains(tableName))
                {
                    reason = $"Required table '{tableName}' is missing.";
                    return false;
                }
            }

            DataTable txp = ds.Tables["TXProfile"];
            DataTable txpDef = ds.Tables["TXProfileDef"];
            DataTable expected;
            try
            {
                expected = BuildCurrentTXProfileDefinition();
            }
            catch (Exception ex)
            {
                reason = "Unable to build current TXProfile schema: " + ex.Message;
                return false;
            }

            // Compare both loaded profile tables against the schema compiled into this build.
            // Do not reject extra columns: only missing or type-incompatible required columns matter.
            foreach (DataColumn col in expected.Columns)
            {
                if (!txp.Columns.Contains(col.ColumnName))
                {
                    reason = $"TXProfile table is missing current column '{col.ColumnName}'.";
                    return false;
                }
                if (txp.Columns[col.ColumnName].DataType != col.DataType)
                {
                    reason = $"TXProfile column '{col.ColumnName}' has type {txp.Columns[col.ColumnName].DataType.FullName}; expected {col.DataType.FullName}.";
                    return false;
                }
                if (!txpDef.Columns.Contains(col.ColumnName))
                {
                    reason = $"TXProfileDef table is missing current column '{col.ColumnName}'.";
                    return false;
                }
                if (txpDef.Columns[col.ColumnName].DataType != col.DataType)
                {
                    reason = $"TXProfileDef column '{col.ColumnName}' has type {txpDef.Columns[col.ColumnName].DataType.FullName}; expected {col.DataType.FullName}.";
                    return false;
                }
            }

            if (!txpDef.Columns.Contains("Name"))
            {
                reason = "TXProfileDef table has no Name column.";
                return false;
            }

            DataRow[] defaultRows = txpDef.Select("Name = 'Default'");
            if (defaultRows.Length != 1)
            {
                reason = "TXProfileDef must contain exactly one 'Default' row; found " + defaultRows.Length + ".";
                return false;
            }

            // A current field with DBNull is treated as incomplete and repaired by migration.
            foreach (DataRow row in txp.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;
                string profileName = txp.Columns.Contains("Name") ? Convert.ToString(row["Name"]) : "<unknown>";
                foreach (DataColumn col in expected.Columns)
                {
                    if (row.IsNull(col.ColumnName))
                    {
                        reason = $"TXProfile '{profileName}' has no value for current field '{col.ColumnName}'.";
                        return false;
                    }
                }
            }

            foreach (DataColumn col in expected.Columns)
            {
                if (defaultRows[0].IsNull(col.ColumnName))
                {
                    reason = $"TXProfileDef Default has no value for current field '{col.ColumnName}'.";
                    return false;
                }
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
        }

'''

text, _ = replace_between(
    text,
    "        public static bool IsDatabaseCompatible(out string reason)",
    "        #region BandStack2",
    compat_method,
)

expand_method = r'''        //-W2PA Expand an old TxProfile table into the current schema.
        // Missing/new fields use the CURRENT Default row. Existing compatible values are kept.
        // Extra obsolete fields are ignored; DBNull never overwrites a valid current default.
        private static DataTable ExpandOldTxProfileTable(DataTable oldTable)
        {
            if (oldTable == null || !oldTable.Columns.Contains("Name")) return null;
            if (!ds.Tables.Contains("TXProfileDef")) return null;

            DataTable dsTXPDefTable = ds.Tables["TXProfileDef"];
            DataTable expandedTable = dsTXPDefTable.Clone();
            DataRow[] defaultRows = dsTXPDefTable.Select("Name = 'Default'");
            if (defaultRows.Length != 1) return null;
            DataRow defaultRow = defaultRows[0];

            foreach (DataRow oldRow in oldTable.Rows)
            {
                if (oldRow.RowState == DataRowState.Deleted) continue;

                string profileName = Convert.ToString(oldRow["Name"]);
                if (string.IsNullOrEmpty(profileName) || profileName == "Default") continue;

                DataRow newRow = expandedTable.NewRow();
                newRow.ItemArray = (object[])defaultRow.ItemArray.Clone();

                foreach (DataColumn targetCol in expandedTable.Columns)
                {
                    if (!oldTable.Columns.Contains(targetCol.ColumnName)) continue;
                    object oldValue = oldRow[targetCol.ColumnName];
                    if (oldValue == null || oldValue == DBNull.Value) continue;

                    DataColumn sourceCol = oldTable.Columns[targetCol.ColumnName];
                    try
                    {
                        if (sourceCol.DataType == targetCol.DataType || targetCol.DataType.IsAssignableFrom(sourceCol.DataType))
                            newRow[targetCol.ColumnName] = oldValue;
                        else
                            newRow[targetCol.ColumnName] = Convert.ChangeType(oldValue, targetCol.DataType);
                    }
                    catch
                    {
                        // Keep the current-build default if an obsolete value cannot be converted safely.
                    }
                }

                // NewRow() is Detached. ImportRow() silently ignores detached rows on .NET;
                // Rows.Add() is required or the migrated user TX profiles disappear.
                newRow["Name"] = profileName;
                expandedTable.Rows.Add(newRow);
            }

            return expandedTable;
        }

'''

text, _ = replace_between(
    text,
    "        //-W2PA Expand an old TxProfile table into a newer one with more colunms. Fill in missing ones with default values.",
    "        //-W2PA Write a message to the ImportLog file during the import process",
    expand_method,
)

# Harden the active TXProfile merge case. Locate it only inside the live ImportAndMergeDatabase implementation.
func_pos = text.find("        public static bool ImportAndMergeDatabase(string filename, out string log, bool ignore_merged)")
if func_pos < 0:
    raise RuntimeError("active ImportAndMergeDatabase not found")
case_start = text.find('                    case "TXProfile":', func_pos)
case_end = text.find('                    case "TXProfileDef":', case_start)
if case_start < 0 or case_end < 0:
    raise RuntimeError("active TXProfile merge case not found")

tx_case = r'''                    case "TXProfile":
                        // Missing TXProfile in a very old/partial DB is not fatal: retain current defaults.
                        if (!being_importedDB.Tables.Contains("TXProfile"))
                        {
                            newDB.Merge(current_inuseDB_table);
                            log += "TXProfile not found in imported database; retained current profiles.\n";
                            break;
                        }

                        DataTable tempOldTable = being_importedDB.Tables["TXProfile"].Copy();
                        tempMergedTable.Clear();

                        // Expand every old user profile into the CURRENT schema. Old values win only
                        // where the field still exists and is safely type-compatible; new fields keep defaults.
                        DataTable expandedProfiles = ExpandOldTxProfileTable(tempOldTable);
                        if (expandedProfiles != null)
                            tempMergedTable.Merge(expandedProfiles);
                        else
                        {
                            tempMergedTable.Merge(current_inuseDB_table);
                            log += "TXProfile could not be expanded; retained current profiles.\n";
                            newDB.Merge(tempMergedTable);
                            break;
                        }

                        // Keep current factory/new profiles that did not exist in the imported DB.
                        foreach (DataRow row in current_inuseDB_table.Rows)
                        {
                            string profileName = Convert.ToString(row["Name"]);
                            string selector = "Name = '" + profileName.Replace("'", "''") + "'";
                            DataRow[] foundRow = tempMergedTable.Select(selector);
                            if (foundRow.Length == 0) tempMergedTable.ImportRow(row);
                        }

                        newDB.Merge(tempMergedTable);
                        log += "Imported table <" + current_inuseDB_table.TableName + "> into database.\n";
                        break;

'''
text = text[:case_start] + tx_case + text[case_end:]

# Static guards: fail the build patch step if any critical repair was not installed.
required = [
    "BuildCurrentTXProfileDefinition",
    "Rows.Add(newRow)",
    "TXProfile not found in imported database; retained current profiles.",
    "profileName.Replace(\"'\", \"''\")",
]
for token in required:
    if token not in text:
        raise RuntimeError(f"patched source verification failed: {token}")

p.write_text(text, encoding="utf-8-sig", newline="\n")
print("DB backward-compatibility patch applied and verified.")
