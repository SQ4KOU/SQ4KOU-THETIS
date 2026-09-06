from pathlib import Path

p = Path(r"Project Files/Source/Console/database.cs")
text = p.read_text(encoding="utf-8-sig")


def replace_method(src: str, signature: str, replacement: str) -> str:
    start = src.find(signature)
    if start < 0:
        raise RuntimeError(f"method signature not found: {signature}")
    open_brace = src.find("{", start)
    if open_brace < 0:
        raise RuntimeError("opening brace not found")
    depth = 0
    i = open_brace
    while i < len(src):
        ch = src[i]
        if ch == "{":
            depth += 1
        elif ch == "}":
            depth -= 1
            if depth == 0:
                end = i + 1
                return src[:start] + replacement + src[end:]
        i += 1
    raise RuntimeError("closing brace not found")


replacement = r'''        // SQ4KOU V3: normalize incomplete legacy TX profiles in-place BEFORE
        // DBMan runs the compatibility gate. Existing non-null legacy values always win.
        // Only missing current columns / DBNull current fields are filled from the
        // current compiled profile definition. This avoids forcing a migration merely
        // because one old/custom profile omitted fields such as DXLevel/Tune_Power.
        private static void VerifyTXProfileColumns()
        {
            DataTable expected;
            try
            {
                expected = BuildCurrentTXProfileDefinition();
            }
            catch (Exception ex)
            {
                LogDatabaseEvent("LEGACY_TXPROFILE_NORMALIZE_FAIL", "Unable to build current TXProfile definition", ex);
                return;
            }

            DataRow[] expectedDefaults = expected.Select("Name = 'Default'");
            if (expectedDefaults.Length != 1)
            {
                LogDatabaseEvent("LEGACY_TXPROFILE_NORMALIZE_FAIL", "Current TXProfile definition has no unique Default row");
                return;
            }
            DataRow expectedDefault = expectedDefaults[0];

            foreach (string tableName in new[] { "TXProfileDef", "TXProfile" })
            {
                if (!ds.Tables.Contains(tableName)) continue;
                DataTable target = ds.Tables[tableName];

                // Add only fields required by this build. Legacy-only fields are retained.
                foreach (DataColumn currentCol in expected.Columns)
                {
                    if (!target.Columns.Contains(currentCol.ColumnName))
                    {
                        DataColumn added = target.Columns.Add(currentCol.ColumnName, currentCol.DataType);
                        added.AllowDBNull = true;
                        if (currentCol.DefaultValue != null && currentCol.DefaultValue != DBNull.Value)
                            added.DefaultValue = currentCol.DefaultValue;
                    }
                }

                // If an old TXProfileDef somehow lacks the Default row, add the current
                // one without touching any existing rows. Multiple Default rows remain an
                // error and are deliberately not guessed/repaired here.
                if (tableName == "TXProfileDef" && target.Columns.Contains("Name"))
                {
                    DataRow[] defaults = target.Select("Name = 'Default'");
                    if (defaults.Length == 0)
                    {
                        DataRow row = target.NewRow();
                        foreach (DataColumn currentCol in expected.Columns)
                        {
                            if (!target.Columns.Contains(currentCol.ColumnName)) continue;
                            if (!expectedDefault.IsNull(currentCol.ColumnName))
                                row[currentCol.ColumnName] = expectedDefault[currentCol.ColumnName];
                        }
                        target.Rows.Add(row);
                    }
                }

                foreach (DataRow row in target.Rows)
                {
                    if (row.RowState == DataRowState.Deleted) continue;
                    string profileName = target.Columns.Contains("Name") ? Convert.ToString(row["Name"]) : "";

                    DataRow seed = expectedDefault;
                    if (!string.IsNullOrEmpty(profileName) && expected.Columns.Contains("Name"))
                    {
                        string escaped = profileName.Replace("'", "''");
                        DataRow[] sameName = expected.Select("Name = '" + escaped + "'");
                        if (sameName.Length == 1) seed = sameName[0];
                    }

                    foreach (DataColumn currentCol in expected.Columns)
                    {
                        if (!target.Columns.Contains(currentCol.ColumnName)) continue;
                        if (!row.IsNull(currentCol.ColumnName)) continue; // old value is authoritative

                        object value = DBNull.Value;
                        if (seed != null && !seed.IsNull(currentCol.ColumnName))
                            value = seed[currentCol.ColumnName];
                        else if (!expectedDefault.IsNull(currentCol.ColumnName))
                            value = expectedDefault[currentCol.ColumnName];
                        else if (currentCol.DefaultValue != null && currentCol.DefaultValue != DBNull.Value)
                            value = currentCol.DefaultValue;

                        if (value == null || value == DBNull.Value) continue;
                        try
                        {
                            row[currentCol.ColumnName] = value;
                            LogDatabaseEvent("LEGACY_TXPROFILE_NORMALIZE",
                                tableName + " '" + profileName + "': filled missing current field '" + currentCol.ColumnName + "'.");
                        }
                        catch (Exception ex)
                        {
                            LogDatabaseEvent("LEGACY_TXPROFILE_NORMALIZE_FAIL",
                                tableName + " '" + profileName + "': could not fill '" + currentCol.ColumnName + "'.", ex);
                        }
                    }
                }
            }
        }'''

text = replace_method(text, "        private static void VerifyTXProfileColumns()", replacement)

required = [
    "SQ4KOU V3: normalize incomplete legacy TX profiles in-place BEFORE",
    "LEGACY_TXPROFILE_NORMALIZE",
    "profileName.Replace(\"'\", \"''\")",
    "if (!row.IsNull(currentCol.ColumnName)) continue; // old value is authoritative",
]
for token in required:
    if token not in text:
        raise RuntimeError(f"V3 verification failed: {token}")

p.write_text(text, encoding="utf-8-sig", newline="\n")
print("DB backward-compatibility V3 load-time normalizer applied and verified.")
