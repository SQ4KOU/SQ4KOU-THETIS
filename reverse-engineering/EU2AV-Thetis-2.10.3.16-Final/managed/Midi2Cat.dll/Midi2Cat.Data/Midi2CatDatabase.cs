using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Midi2Cat.Data;

public class Midi2CatDatabase
{
	public DataSet importDS;

	public DataSet ds;

	private const string SettingsTable = "Midi2Cat--Settings";

	private const string DefaultTableName = "Not Saved";

	private string file_name;

	public string FileName => file_name;

	public Midi2CatDatabase(string fileName)
	{
		file_name = fileName;
		ds = new DataSet("Midi2CatData");
		try
		{
			if (File.Exists(file_name))
			{
				ds.ReadXml(file_name);
			}
		}
		catch
		{
		}
	}

	public void SaveChanges(string MidiDeviceName)
	{
		try
		{
			if (MidiDeviceName != null)
			{
				string loadedMappingName = GetLoadedMappingName(MidiDeviceName);
				if (loadedMappingName != null)
				{
					SaveMappingAs(MidiDeviceName, loadedMappingName, replace: true);
				}
			}
			ds.WriteXml(file_name, XmlWriteMode.WriteSchema);
		}
		catch
		{
		}
	}

	public void Exit()
	{
		SaveChanges(null);
		ds = null;
	}

	private DataTable GetTable(string MidiDeviceName, DataSet overrideDS = null)
	{
		DataSet dataSet = ds;
		if (overrideDS != null)
		{
			dataSet = overrideDS;
		}
		DataTable dataTable = null;
		if (dataSet.Tables.IndexOf(MidiDeviceName) < 0)
		{
			dataSet.Tables.Add(MidiDeviceName);
			DataColumn[] array = new DataColumn[1];
			dataTable = dataSet.Tables[MidiDeviceName];
			dataTable.Columns.Add("MidiControlId", typeof(int));
			dataTable.Columns.Add("MidiControlName", typeof(string));
			dataTable.Columns.Add("MidiControlType", typeof(int));
			dataTable.Columns.Add("MinValue", typeof(int));
			dataTable.Columns.Add("MaxValue", typeof(int));
			dataTable.Columns.Add("CatCmdId", typeof(int));
			dataTable.Columns.Add("MidiOutCmdDown", typeof(string));
			dataTable.Columns.Add("MidiOutCmdUp", typeof(string));
			dataTable.Columns.Add("MidiOutCmdSetValue", typeof(string));
			array[0] = dataTable.Columns[0];
			dataTable.PrimaryKey = array;
		}
		else
		{
			dataTable = dataSet.Tables[MidiDeviceName];
			bool flag = false;
			if (dataTable.Columns["MidiOutCmdDown"] == null)
			{
				flag = true;
				dataTable.Columns.Add("MidiOutCmdDown", typeof(string));
			}
			if (dataTable.Columns["MidiOutCmdUp"] == null)
			{
				flag = true;
				dataTable.Columns.Add("MidiOutCmdUp", typeof(string));
			}
			if (dataTable.Columns["MidiOutCmdSetValue"] == null)
			{
				flag = true;
				dataTable.Columns.Add("MidiOutCmdSetValue", typeof(string));
			}
			if (flag)
			{
				SaveChanges(MidiDeviceName);
			}
		}
		return dataTable;
	}

	public void AddRow(string MidiDeviceName, DataRow row)
	{
		GetTable(MidiDeviceName).Rows.Add(row);
	}

	private void AddRow(string MidiDeviceName, ControllerMapping mapping)
	{
		DataTable table = GetTable(MidiDeviceName);
		DataRow row = PopulateRow(table.NewRow(), mapping);
		table.Rows.Add(row);
	}

	private bool UpdateRow(string MidiDeviceName, ControllerMapping mapping)
	{
		DataRow dataRow = GetTable(MidiDeviceName).Rows.Find(mapping.MidiControlId);
		if (dataRow != null)
		{
			dataRow = PopulateRow(dataRow, mapping);
			return true;
		}
		return false;
	}

	private ControllerMapping GetRow(string MidiDeviceName, int Idx)
	{
		DataRow dataRow = GetTable(MidiDeviceName).Rows[Idx];
		if (dataRow != null)
		{
			return PopulateMapping(dataRow);
		}
		return null;
	}

	public void DeleteRow(string MidiDeviceName, ControllerMapping mapping)
	{
		DataTable table = GetTable(MidiDeviceName);
		DataRow dataRow = table.Rows.Find(mapping.MidiControlId);
		if (dataRow != null)
		{
			table.Rows.Remove(dataRow);
		}
	}

	public void DeleteRow(string MidiDeviceName, int MidiControlId)
	{
		DataTable table = GetTable(MidiDeviceName);
		DataRow dataRow = table.Rows.Find(MidiControlId);
		if (dataRow != null)
		{
			table.Rows.Remove(dataRow);
		}
	}

	public void UpdateOrAdd(string MidiDeviceName, ControllerMapping mapping)
	{
		if (!UpdateRow(MidiDeviceName, mapping))
		{
			AddRow(MidiDeviceName, mapping);
		}
	}

	public List<ControllerMapping> GetMappings(string MidiDeviceName, MappingFilter filter)
	{
		List<ControllerMapping> list = new List<ControllerMapping>();
		DataTable table = GetTable(MidiDeviceName);
		for (int i = 0; i < table.Rows.Count; i++)
		{
			ControllerMapping row = GetRow(MidiDeviceName, i);
			row.CatCmd = CatCmdDb.Get(row.CatCmd.CatCommandId);
			switch (filter)
			{
			case MappingFilter.None:
				list.Add(row);
				break;
			case MappingFilter.Active:
				if (row.CatCmd.CatCommandId != CatCmd.None)
				{
					list.Add(row);
				}
				break;
			case MappingFilter.InActive:
				if (row.CatCmd.CatCommandId == CatCmd.None)
				{
					list.Add(row);
				}
				break;
			}
		}
		return list;
	}

	public ControllerMapping GetMapping(string MidiDeviceName, int MidiControlId)
	{
		ControllerMapping result = null;
		DataRow dataRow = GetTable(MidiDeviceName).Rows.Find(MidiControlId);
		if (dataRow != null)
		{
			result = PopulateMapping(dataRow);
		}
		return result;
	}

	public ControllerMapping GetReverseMapping(string MidiDeviceName, CatCmd cmd)
	{
		ControllerMapping result = null;
		DataTable table = GetTable(MidiDeviceName);
		table.Rows.Find(cmd);
		string filterExpression = "CatCmdId = " + Convert.ToString((int)cmd);
		DataRow[] array = table.Select(filterExpression);
		if (array.Length == 0)
		{
			return result;
		}
		if (array[0] != null)
		{
			result = PopulateMapping(array[0]);
		}
		return result;
	}

	private ControllerMapping PopulateMapping(DataRow dr)
	{
		ControllerMapping obj = new ControllerMapping
		{
			MidiControlId = (int)dr["MidiControlId"],
			MidiControlName = (string)dr["MidiControlName"],
			MidiControlType = FixUp.FixControlType((int)dr["MidiControlType"]),
			MinValue = (int)dr["MinValue"],
			MaxValue = (int)dr["MaxValue"],
			CatCmdId = (CatCmd)dr["CatCmdId"]
		};
		obj.CatCmd = CatCmdDb.Get(obj.CatCmdId);
		obj.MidiOutCmdDown = ConvertFromDBVal<string>(dr["MidiOutCmdDown"]);
		obj.MidiOutCmdUp = ConvertFromDBVal<string>(dr["MidiOutCmdUp"]);
		obj.MidiOutCmdSetValue = ConvertFromDBVal<string>(dr["MidiOutCmdSetValue"]);
		return obj;
	}

	public DataRow PopulateRow(DataRow dr, ControllerMapping mapping)
	{
		dr["MidiControlId"] = mapping.MidiControlId;
		dr["MidiControlName"] = mapping.MidiControlName;
		dr["MidiControlType"] = mapping.MidiControlType;
		dr["MinValue"] = mapping.MinValue;
		dr["MaxValue"] = mapping.MaxValue;
		dr["CatCmdId"] = (int)mapping.CatCmd.CatCommandId;
		dr["MidiOutCmdDown"] = mapping.MidiOutCmdDown;
		dr["MidiOutCmdUp"] = mapping.MidiOutCmdUp;
		dr["MidiOutCmdSetValue"] = mapping.MidiOutCmdSetValue;
		return dr;
	}

	public void BindToDataSource(BindingSource source, string MidiDeviceName)
	{
		source.DataSource = ds;
		GetTable(MidiDeviceName);
		source.DataMember = MidiDeviceName;
	}

	public bool IsDeviceSetup(string MidiDeviceName)
	{
		if (ds.Tables.IndexOf(MidiDeviceName) >= 0)
		{
			GetTable(MidiDeviceName);
			if (GetMappings(MidiDeviceName, MappingFilter.Active).Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveMapping(string MidiDeviceName, int catCmd)
	{
		if (ds.Tables.IndexOf(MidiDeviceName) < 0)
		{
			return;
		}
		foreach (DataRow row in GetTable(MidiDeviceName).Rows)
		{
			if ((int)row["CatCmdId"] == catCmd)
			{
				row["CatCmdId"] = 0;
				row["MidiOutCmdSetValue"] = "";
				row.AcceptChanges();
				break;
			}
		}
	}

	public T ConvertFromDBVal<T>(object obj)
	{
		if (obj == null || obj == DBNull.Value)
		{
			return default(T);
		}
		return (T)obj;
	}

	public bool SaveMappingAs(string midiDeviceName, string Name, bool replace)
	{
		if (ds.Tables.IndexOf(Name) >= 0 && !replace)
		{
			return false;
		}
		DataTable table = GetTable(midiDeviceName);
		DataTable table2 = GetTable(Name);
		table2.Clear();
		foreach (DataRow row in table.Rows)
		{
			table2.ImportRow(row);
		}
		string prefixFromMidiDeviceName = GetPrefixFromMidiDeviceName(midiDeviceName);
		SetSetting(prefixFromMidiDeviceName + "LoadedMapping", Name);
		SaveChanges(null);
		return true;
	}

	public string[] GetSavedMappings()
	{
		List<string> list = new List<string>();
		foreach (DataTable table in ds.Tables)
		{
			if (!table.TableName.Contains('-') && table.TableName != "Not Saved")
			{
				list.Add(table.TableName);
			}
		}
		return list.ToArray();
	}

	public bool LoadMapping(string midiDeviceName, string Name)
	{
		if (midiDeviceName.ToLower() == Name.ToLower())
		{
			return false;
		}
		DataTable table = GetTable(midiDeviceName);
		DataTable table2 = GetTable(Name);
		table.Rows.Clear();
		foreach (DataRow row in table2.Rows)
		{
			table.ImportRow(row);
		}
		string prefixFromMidiDeviceName = GetPrefixFromMidiDeviceName(midiDeviceName);
		SetSetting(prefixFromMidiDeviceName + "LoadedMapping", Name);
		SaveChanges(null);
		return true;
	}

	public bool ExportMappings(string fileName, string[] mappings)
	{
		try
		{
			DataSet dataSet = new DataSet("ExportMidi2Cat");
			foreach (DataTable table2 in ds.Tables)
			{
				if (table2.TableName.Contains('-') || !mappings.Contains(table2.TableName))
				{
					continue;
				}
				DataTable table = GetTable(table2.TableName, dataSet);
				foreach (DataRow row in GetTable(table2.TableName).Rows)
				{
					table.ImportRow(row);
				}
			}
			dataSet.WriteXml(fileName, XmlWriteMode.WriteSchema);
		}
		catch
		{
			return false;
		}
		return true;
	}

	public bool ImportMappings(string fileName)
	{
		importDS = new DataSet("Midi2CatData");
		try
		{
			if (File.Exists(fileName))
			{
				importDS.ReadXml(fileName);
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public string[] GetImportedMappings()
	{
		List<string> list = new List<string>();
		foreach (DataTable table in importDS.Tables)
		{
			if (!table.TableName.Contains('-'))
			{
				list.Add(table.TableName);
			}
		}
		return list.ToArray();
	}

	public bool AddFromImport(string[] mappings)
	{
		bool result = true;
		try
		{
			foreach (DataTable table2 in importDS.Tables)
			{
				if (table2.TableName.Contains('-') || !mappings.Contains(table2.TableName))
				{
					continue;
				}
				_ = table2.TableName;
				string text = "";
				while (ds.Tables.Contains(table2.TableName + text))
				{
					Thread.Sleep(1010);
					text = " Imported on " + DateTime.Now.ToLongDateString() + " at " + DateTime.Now.ToString("HH:mm:ss");
				}
				DataTable table = GetTable(table2.TableName + text);
				foreach (DataRow row in GetTable(table2.TableName, importDS).Rows)
				{
					table.ImportRow(row);
				}
			}
			SaveChanges(null);
		}
		catch
		{
			result = false;
		}
		importDS = null;
		return result;
	}

	public bool RemoveSavedMapping(string midiDeviceName, string Name)
	{
		bool result = true;
		try
		{
			if (!Name.Contains('-'))
			{
				ds.Tables.Remove(Name);
				SaveChanges(null);
				string loadedMappingName = GetLoadedMappingName(midiDeviceName);
				if (Name == loadedMappingName)
				{
					string prefixFromMidiDeviceName = GetPrefixFromMidiDeviceName(midiDeviceName);
					SetSetting(prefixFromMidiDeviceName + "LoadedMapping", "Not Saved");
				}
			}
			else
			{
				result = false;
			}
		}
		catch
		{
			result = false;
		}
		return result;
	}

	public bool RenameSavedMapping(string midiDeviceName, string OldName, string NewName)
	{
		bool result = true;
		try
		{
			if (!NewName.Contains('-'))
			{
				ds.Tables[OldName].TableName = NewName;
				SaveChanges(null);
				string loadedMappingName = GetLoadedMappingName(midiDeviceName);
				if (OldName == loadedMappingName)
				{
					string prefixFromMidiDeviceName = GetPrefixFromMidiDeviceName(midiDeviceName);
					SetSetting(prefixFromMidiDeviceName + "LoadedMapping", NewName);
				}
			}
			else
			{
				result = false;
			}
		}
		catch
		{
			result = false;
		}
		return result;
	}

	private DataTable GetSettingTable()
	{
		DataSet dataSet = ds;
		DataTable dataTable = null;
		if (dataSet.Tables.IndexOf("Midi2Cat--Settings") < 0)
		{
			dataSet.Tables.Add("Midi2Cat--Settings");
			DataColumn[] array = new DataColumn[1];
			dataTable = dataSet.Tables["Midi2Cat--Settings"];
			dataTable.Columns.Add("Name", typeof(string));
			dataTable.Columns.Add("Value", typeof(string));
			dataTable.Columns.Add("ValueType", typeof(string));
			array[0] = dataTable.Columns[0];
			dataTable.PrimaryKey = array;
		}
		else
		{
			dataTable = dataSet.Tables["Midi2Cat--Settings"];
		}
		return dataTable;
	}

	private void SetSetting(string name, string value)
	{
		DataTable settingTable = GetSettingTable();
		DataRow dataRow = settingTable.Rows.Find(name);
		if (dataRow != null)
		{
			dataRow["Value"] = value;
			dataRow["ValueType"] = "string";
			dataRow.AcceptChanges();
		}
		else
		{
			dataRow = settingTable.NewRow();
			dataRow["Name"] = name;
			dataRow["Value"] = value;
			dataRow["ValueType"] = "string";
			settingTable.Rows.Add(dataRow);
		}
		SaveChanges(null);
	}

	private DataRow GetSetting(string name)
	{
		return GetSettingTable().Rows.Find(name);
	}

	private string GetStringSetting(string name, string defaultValue)
	{
		DataRow setting = GetSetting(name);
		if (setting != null)
		{
			return (string)setting["Value"];
		}
		return defaultValue;
	}

	private string GetPrefixFromMidiDeviceName(string midiDeviceName)
	{
		int num = midiDeviceName.IndexOf('-');
		if (num >= 0)
		{
			return midiDeviceName.Substring(0, num + 1);
		}
		return null;
	}

	public string GetLoadedMappingName(string midiDeviceName)
	{
		string prefixFromMidiDeviceName = GetPrefixFromMidiDeviceName(midiDeviceName);
		return GetStringSetting(prefixFromMidiDeviceName + "LoadedMapping", "Not Saved");
	}
}
