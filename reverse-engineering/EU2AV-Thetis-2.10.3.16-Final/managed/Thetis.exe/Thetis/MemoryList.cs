using System;
using System.IO;
using System.Xml.Serialization;

namespace Thetis;

public class MemoryList
{
	private SortableBindingList<MemoryRecord> list = new SortableBindingList<MemoryRecord>();

	private static string app_data_path = "";

	private static int current_major_version = 1;

	private int major_version = 1;

	private static int current_minor_version = 1;

	private int minor_version = 1;

	public SortableBindingList<MemoryRecord> List => list;

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

	private void Save(string file_name)
	{
		TextWriter textWriter = new StreamWriter(file_name);
		try
		{
			new XmlSerializer(typeof(MemoryList), new Type[3]
			{
				typeof(MemoryRecord),
				typeof(SortableBindingList<MemoryRecord>),
				typeof(int)
			}).Serialize(textWriter, this);
		}
		catch (Exception)
		{
		}
		textWriter.Close();
	}

	public void Save()
	{
		string file_name = app_data_path + "memory.xml";
		Save(file_name);
	}

	public static MemoryList Restore()
	{
		string text = app_data_path;
		string path = text + "memory.xml";
		string text2 = text + "memory_bak.xml";
		MemoryList memoryList = new MemoryList();
		StreamReader streamReader;
		try
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException();
			}
			streamReader = new StreamReader(path);
			memoryList = (MemoryList)new XmlSerializer(typeof(MemoryList), new Type[3]
			{
				typeof(MemoryRecord),
				typeof(SortableBindingList<MemoryRecord>),
				typeof(int)
			}).Deserialize(streamReader);
			memoryList.Save(text2);
		}
		catch (Exception)
		{
			if (!File.Exists(text2))
			{
				return memoryList;
			}
			streamReader = new StreamReader(text2);
			try
			{
				memoryList = (MemoryList)new XmlSerializer(typeof(MemoryList), new Type[3]
				{
					typeof(MemoryRecord),
					typeof(SortableBindingList<MemoryRecord>),
					typeof(int)
				}).Deserialize(streamReader);
			}
			catch (Exception)
			{
			}
		}
		streamReader.Close();
		return memoryList;
	}

	public void CheckVersion()
	{
		if ((major_version != current_major_version || minor_version != current_minor_version) && major_version == 1)
		{
			_ = minor_version;
		}
	}
}
