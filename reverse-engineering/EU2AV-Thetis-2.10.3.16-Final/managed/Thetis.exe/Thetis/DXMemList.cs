using System;
using System.IO;
using System.Xml.Serialization;

namespace Thetis;

public class DXMemList
{
	private SortableBindingList<DXMemRecord> list = new SortableBindingList<DXMemRecord>();

	private static string app_data_path = "";

	private static int current_major_version = 1;

	private int major_version = 1;

	private static int current_minor_version = 1;

	private int minor_version = 1;

	public SortableBindingList<DXMemRecord> List => list;

	public static string AppDataPath
	{
		set
		{
			app_data_path = value;
		}
	}

	public int MajorVersion
	{
		get
		{
			return major_version;
		}
		set
		{
			major_version = value;
		}
	}

	public int MinorVersion
	{
		get
		{
			return minor_version;
		}
		set
		{
			minor_version = value;
		}
	}

	private void Save1(string file_name)
	{
		TextWriter textWriter = new StreamWriter(file_name);
		try
		{
			new XmlSerializer(typeof(DXMemList), new Type[3]
			{
				typeof(DXMemRecord),
				typeof(SortableBindingList<DXMemRecord>),
				typeof(int)
			}).Serialize(textWriter, this);
		}
		catch (Exception)
		{
		}
		textWriter.Close();
	}

	public void Save1()
	{
		string file_name = app_data_path + "DXMemory.xml";
		Save1(file_name);
	}

	public static DXMemList Restore1()
	{
		string text = app_data_path;
		string path = text + "DXMemory.xml";
		string text2 = text + "DXMemory_bak.xml";
		DXMemList dXMemList = new DXMemList();
		StreamReader streamReader;
		try
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException();
			}
			streamReader = new StreamReader(path);
			dXMemList = (DXMemList)new XmlSerializer(typeof(DXMemList), new Type[3]
			{
				typeof(DXMemRecord),
				typeof(SortableBindingList<DXMemRecord>),
				typeof(int)
			}).Deserialize(streamReader);
			dXMemList.Save1(text2);
		}
		catch (Exception)
		{
			if (!File.Exists(text2))
			{
				return dXMemList;
			}
			streamReader = new StreamReader(text2);
			try
			{
				dXMemList = (DXMemList)new XmlSerializer(typeof(DXMemList), new Type[3]
				{
					typeof(DXMemRecord),
					typeof(SortableBindingList<DXMemRecord>),
					typeof(int)
				}).Deserialize(streamReader);
			}
			catch (Exception)
			{
			}
		}
		streamReader.Close();
		return dXMemList;
	}

	public void CheckVersion1()
	{
		if ((major_version != current_major_version || minor_version != current_minor_version) && major_version == 1)
		{
			_ = minor_version;
		}
	}
}
