using System;
using System.Data;
using System.Windows.Forms;

namespace Midi2Cat.Data;

public class EnumsDB
{
	private DataSet _ds;

	public DataSet ds
	{
		get
		{
			if (_ds == null)
			{
				_ds = new DataSet();
				AddControlTypes();
				AddCatCmds();
			}
			return _ds;
		}
	}

	private void AddControlTypes()
	{
		ds.Tables.Add("ControlTypes");
		DataTable dataTable = null;
		DataColumn[] array = new DataColumn[2];
		dataTable = ds.Tables["ControlTypes"];
		dataTable.Columns.Add("ControlId", typeof(int));
		dataTable.Columns.Add("ControlDescription", typeof(string));
		array[0] = dataTable.Columns[0];
		array[1] = dataTable.Columns[1];
		dataTable.PrimaryKey = array;
		ControlType[] array2 = (ControlType[])Enum.GetValues(typeof(ControlType));
		for (int i = 0; i < array2.Length; i++)
		{
			ControlType controlType = array2[i];
			DataRow dataRow = dataTable.NewRow();
			dataRow["ControlId"] = (int)controlType;
			dataRow["ControlDescription"] = controlType.ToString().Replace("_", " ");
			dataTable.Rows.Add(dataRow);
			dataRow.AcceptChanges();
		}
	}

	private void AddCatCmds()
	{
		ds.Tables.Add("CatCmds");
		DataTable dataTable = null;
		DataColumn[] array = new DataColumn[1];
		dataTable = ds.Tables["CatCmds"];
		dataTable.Columns.Add("CmdId", typeof(int));
		dataTable.Columns.Add("CmdDescription", typeof(string));
		dataTable.Columns.Add("ControlType", typeof(int));
		dataTable.Columns.Add("InUse", typeof(bool));
		array[0] = dataTable.Columns[0];
		dataTable.PrimaryKey = array;
		CatCmd[] array2 = (CatCmd[])Enum.GetValues(typeof(CatCmd));
		foreach (CatCmd catCmd in array2)
		{
			CatCommandAttribute catCommandAttribute = CatCmdDb.Get(catCmd);
			DataRow dataRow = dataTable.NewRow();
			dataRow["CmdId"] = (int)catCmd;
			dataRow["CmdDescription"] = catCommandAttribute.Desc;
			dataRow["ControlType"] = catCommandAttribute.ControlType;
			dataRow["InUse"] = false;
			dataTable.Rows.Add(dataRow);
			dataRow.AcceptChanges();
		}
	}

	public void BindToDataSource(BindingSource source, string tableName)
	{
		DataView dataSource = new DataView(ds.Tables[tableName]);
		source.DataSource = dataSource;
	}

	public void SetCatCmdInUse(CatCmd catCmd, bool inUse)
	{
		DataRow dataRow = ds.Tables["CatCmds"].Rows.Find(catCmd);
		if (dataRow != null)
		{
			dataRow["InUse"] = inUse;
			dataRow.AcceptChanges();
		}
	}
}
