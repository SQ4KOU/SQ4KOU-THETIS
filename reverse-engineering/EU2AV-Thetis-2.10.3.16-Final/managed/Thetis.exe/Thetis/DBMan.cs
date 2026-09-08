using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Thetis;

internal static class DBMan
{
	private class DBSettings
	{
		public Guid ActiveDB_GUID { get; set; }

		public string ActiveDB_File { get; set; }

		public DBSettings()
		{
			ActiveDB_GUID = Guid.Empty;
			ActiveDB_File = "";
		}
	}

	public class DatabaseInfo
	{
		public class DatabaseInfoDefaultStringEnumConverter : StringEnumConverter
		{
			private readonly HPSDRModel _defaultValue;

			public DatabaseInfoDefaultStringEnumConverter(HPSDRModel defaultValue)
			{
				_defaultValue = defaultValue;
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				try
				{
					return base.ReadJson(reader, objectType, existingValue, serializer);
				}
				catch
				{
					return _defaultValue;
				}
			}
		}

		public Guid GUID { get; set; }

		public string FullPath { get; set; }

		public DateTime FolderCreationTime { get; set; }

		public long TotalContentsSize { get; set; }

		public long Size { get; set; }

		public string Description { get; set; }

		public DateTime LastChanged { get; set; }

		public DateTime CreationTime { get; set; }

		public string VersionString { get; set; }

		public string VersionNumber { get; set; }

		public bool BackupOnStartup { get; set; }

		public bool BackupOnShutdown { get; set; }

		[JsonConverter(typeof(DatabaseInfoDefaultStringEnumConverter), new object[] { HPSDRModel.HERMES })]
		public HPSDRModel Model { get; set; }

		public DatabaseInfo()
		{
			GUID = Guid.Empty;
			FullPath = "";
			FolderCreationTime = DateTime.Now;
			TotalContentsSize = 0L;
			Size = 0L;
			Description = "";
			LastChanged = DateTime.Now;
			Model = HPSDRModel.HERMES;
			CreationTime = DateTime.Now;
			VersionString = "unknown";
			VersionNumber = "unknown";
			BackupOnStartup = false;
			BackupOnShutdown = false;
		}
	}

	public class BackupFileInfo
	{
		[JsonIgnore]
		public string FullFilePath { get; set; }

		[JsonIgnore]
		public DateTime DateTimeOfBackup { get; set; }

		[JsonIgnore]
		public long SecondsSinceEpoch { get; set; }

		[JsonIgnore]
		public TimeSpan AgeSinceBackedUp { get; set; }

		public string Description { get; set; }

		public bool Auto { get; set; }

		public BackupFileInfo()
		{
			Auto = false;
			Description = "Default";
		}
	}

	private static frmDBMan _frm_dbman;

	private static string _app_data_path;

	private static string _db_data_path;

	private static DBSettings _dbman_settings;

	private static bool _ignore_written;

	private static string _unique_instance_id;

	private static bool _prune_backups;

	public static bool IsVisible
	{
		get
		{
			if (_frm_dbman == null)
			{
				return false;
			}
			return _frm_dbman.Visible;
		}
	}

	public static string AppDataPath
	{
		set
		{
			_app_data_path = value;
			_db_data_path = _app_data_path + "DB\\";
			if (!Directory.Exists(_db_data_path))
			{
				Directory.CreateDirectory(_db_data_path);
			}
		}
	}

	public static bool PruneBackups
	{
		get
		{
			return _prune_backups;
		}
		set
		{
			_prune_backups = value;
		}
	}

	static DBMan()
	{
		_ignore_written = false;
		_dbman_settings = null;
		_app_data_path = "";
		_db_data_path = "";
		_unique_instance_id = "";
		_prune_backups = false;
		_frm_dbman = new frmDBMan();
	}

	public static void ShowDBMan()
	{
		if (_dbman_settings == null)
		{
			return;
		}
		Console console = Console.getConsole();
		if (console.IsSetupFormNull)
		{
			return;
		}
		if (console.PowerOn)
		{
			if (MessageBox.Show("The Database Manager can not be used whilst the radio is powered on. The radio will be powered off.", "Database Manager Issue", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2, (MessageBoxOptions)262144) != DialogResult.OK)
			{
				return;
			}
			console.PowerOn = false;
			if (console.PowerOn)
			{
				MessageBox.Show("Unable to power off the radio. You will need to do it manually and then try again.", "Database Manager Issue", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
				return;
			}
		}
		if (console.SetupForm.Visible)
		{
			MessageBox.Show("The Database Manager can not be used whilst the Setup window is shown. Please close it and try again.", "Database Manager Issue", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			return;
		}
		Dictionary<Guid, DatabaseInfo> availableDBs = getAvailableDBs();
		_frm_dbman.InitAvailableDBs(availableDBs, _dbman_settings.ActiveDB_GUID, Guid.Empty);
		_frm_dbman.Restore();
		_frm_dbman.PruneBackups = _prune_backups;
		_frm_dbman.ShowDialog();
	}

	public static bool LoadDB(string[] args, out string broken_folder)
	{
		_dbman_settings = null;
		broken_folder = "";
		if (!Common.IsValidPath(_db_data_path))
		{
			MessageBox.Show("There is an issue with the database data path.\n\n[" + _db_data_path + "]\n\nIt is not valid. Please fix and try again.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			return false;
		}
		foreach (string text in args)
		{
			if (text.StartsWith("-dbfilename:"))
			{
				MessageBox.Show("-dbfilename: command line option is no longer supported.\nPlease use -dbid: to provide the Database Manager with a unique ID to use for this instance.\nYou can import your existing database using the Database Manager.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
				break;
			}
			if (text.StartsWith("-dbid:"))
			{
				_unique_instance_id = text.Trim().Substring(text.Trim().IndexOf(":") + 1) + "_";
				if (!Common.IsValidFilename(_unique_instance_id + "dbman_settings.json"))
				{
					MessageBox.Show("There is an issue with the database dbman_settings file name.\n\n[" + _unique_instance_id + "dbman_settings.json]\n\nIt is not valid. Please fix and try again.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
					return false;
				}
			}
		}
		bool flag = false;
		if (Keyboard.IsKeyDown(Keys.LShiftKey) || Keyboard.IsKeyDown(Keys.RShiftKey))
		{
			Thread.Sleep(500);
			if ((Keyboard.IsKeyDown(Keys.LShiftKey) || Keyboard.IsKeyDown(Keys.RShiftKey)) && MessageBox.Show("The database reset function has been triggered. Would you like to use a fresh new database?\n\nYour existing database will be untouched, and a new one will be used.\n\nIt will have the description 'Default' in the Database Manager.", "New Database?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, (MessageBoxOptions)262144) == DialogResult.Yes)
			{
				flag = true;
			}
		}
		bool force_upgrade = false;
		bool flag2 = updateFileExists();
		if (!flag && ((Keyboard.IsKeyDown(Keys.LControlKey) || Keyboard.IsKeyDown(Keys.RControlKey)) | flag2))
		{
			Thread.Sleep(500);
			if (((Keyboard.IsKeyDown(Keys.LControlKey) || Keyboard.IsKeyDown(Keys.RControlKey)) | flag2) && MessageBox.Show("The database force update has been triggered. Do you want to do this?\n\n", "Force Update Database?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, (MessageBoxOptions)262144) == DialogResult.Yes)
			{
				force_upgrade = true;
			}
		}
		bool flag3 = false;
		bool made_new = false;
		bool old_db_found = false;
		if ((getAvailableDBs().Count == 0) | flag)
		{
			flag3 = createNewDB(check_for_old_db: true, make_active: true, out old_db_found);
			if (flag3)
			{
				if (!old_db_found)
				{
					made_new = true;
				}
				flag3 = getAvailableDBs().Count > 0;
			}
		}
		else
		{
			flag3 = true;
		}
		if (flag3)
		{
			_dbman_settings = getActiveDB();
			if (_dbman_settings != null)
			{
				string text2 = _db_data_path + _dbman_settings.ActiveDB_GUID.ToString() + "\\database.xml";
				if (!File.Exists(text2))
				{
					MessageBox.Show("The last active Database could not be located. Using a blank new one.", "Database Manager Issue", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
					flag3 = createNewDB(check_for_old_db: false, make_active: true, out var _);
					if (flag3)
					{
						made_new = true;
						flag3 = getAvailableDBs().Count > 0;
						if (flag3)
						{
							if (_dbman_settings != null)
							{
								text2 = _db_data_path + _dbman_settings.ActiveDB_GUID.ToString() + "\\database.xml";
								try
								{
									flag3 = File.Exists(text2);
								}
								catch
								{
									flag3 = false;
								}
							}
							else
							{
								flag3 = false;
							}
						}
					}
				}
				if (flag3)
				{
					bool flag4 = false;
					try
					{
						string path = _db_data_path + _dbman_settings.ActiveDB_GUID.ToString() + "\\dbman.json";
						if (File.Exists(path) && JsonConvert.DeserializeObject<DatabaseInfo>(File.ReadAllText(path)).BackupOnStartup)
						{
							flag4 = true;
							TakeBackup(Guid.Empty, "Startup", auto: true);
						}
					}
					catch
					{
					}
					DB.FileName = text2;
					_ignore_written = true;
					flag3 = DB.Init();
					_ignore_written = false;
					if (flag3)
					{
						bool schema_mismatch = !DB.IsDatabaseCompatible(out var reason);
						flag3 = checkVersion(made_new, force_upgrade, flag2, schema_mismatch, reason);
					}
					if (flag3)
					{
						Dictionary<string, string> varsDictionary = DB.GetVarsDictionary("State");
						bool result = false;
						if (varsDictionary.ContainsKey("PruneBackups"))
						{
							bool.TryParse(varsDictionary["PruneBackups"], out result);
						}
						_frm_dbman.PruneBackups = result;
						if (result & flag4)
						{
							pruneForGFS(Path.GetDirectoryName(text2) + "\\backups");
						}
					}
				}
			}
			else
			{
				flag3 = false;
			}
		}
		if (!flag3 && _dbman_settings != null)
		{
			broken_folder = _dbman_settings.ActiveDB_GUID.ToString();
			moveToBroken(_dbman_settings.ActiveDB_GUID);
		}
		return flag3;
	}

	private static bool updateFileExists()
	{
		try
		{
			return File.Exists(Path.Combine(_app_data_path, "updatedb.txt"));
		}
		catch
		{
			return false;
		}
	}

	private static bool renameUpdatedb()
	{
		try
		{
			string text = Path.Combine(_app_data_path, "updatedb.txt");
			if (!File.Exists(text))
			{
				return false;
			}
			string text2 = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			string text3 = Path.Combine(_app_data_path, "updatedb_success_" + text2 + ".txt");
			int num = 1;
			while (File.Exists(text3))
			{
				text3 = Path.Combine(_app_data_path, "updatedb_success_" + text2 + "_" + num + ".txt");
				num++;
			}
			File.Move(text, text3);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static bool checkVersion(bool made_new, bool force_upgrade = false, bool force_upgrade_via_file = false, bool schema_mismatch = false, string schema_mismatch_reason = "")
	{
		Dictionary<string, string> varsDictionary = DB.GetVarsDictionary("State");
		string text = ((!varsDictionary.ContainsKey("VersionNumber")) ? "? version" : varsDictionary["VersionNumber"]);
		if (made_new)
		{
			return true;
		}
		if (!(force_upgrade | force_upgrade_via_file | schema_mismatch) && !(Common.GetVerNum() != text))
		{
			return true;
		}
		string text2 = "";
		if (force_upgrade)
		{
			text2 = "Force database update requested.\n\n";
		}
		else if (schema_mismatch)
		{
			text2 = "The database is missing data required by this version of Thetis.\n" + schema_mismatch_reason + "\n\n";
		}
		if (MessageBox.Show(text2 + "This version [" + Common.GetVerNum() + "] of Thetis requires your database [" + text + "] to be updated.\n\nA backup of your current database will be created before the update.\n\nA new updated database will be created, and your old database merged into it. It will be made active.\n\nDo you want to proceed?", "Database Update Required", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2, (MessageBoxOptions)262144) != DialogResult.Yes)
		{
			MessageBox.Show("Thetis cannot continue with an incompatible database.\n\nYou can restart and hold Ctrl while launching to force an update, create an empty file named 'updatedb.txt' in the data folder, or use the Database Manager to restore a backup.", "Database Update Declined", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			return false;
		}
		TakeBackup(Guid.Empty, "PreUpgrade", auto: true);
		if (_dbman_settings != null)
		{
			_ = _dbman_settings.ActiveDB_GUID;
		}
		else
		{
			_ = Guid.Empty;
		}
		string fileName = DB.FileName;
		bool flag = createNewDB(check_for_old_db: false, make_active: true, out var _);
		if (flag)
		{
			flag = DB.ImportAndMergeDatabase(fileName, out var log, ignore_merged: true);
			try
			{
				File.WriteAllText(_app_data_path + "ImportLog_dbupdate.txt", log);
			}
			catch
			{
			}
		}
		if (flag)
		{
			DBWritten();
			if (force_upgrade_via_file)
			{
				renameUpdatedb();
			}
			MessageBox.Show("The database update was completed sucessfully.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		}
		else
		{
			MessageBox.Show("The database update did not complete.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		}
		return flag;
	}

	private static void moveToBroken(Guid guid)
	{
		try
		{
			string text = _db_data_path + guid.ToString();
			string text2 = _db_data_path + "broken";
			if (Directory.Exists(text))
			{
				if (!Directory.Exists(text2))
				{
					Directory.CreateDirectory(text2);
				}
				text2 = text2 + "\\" + guid.ToString();
				Directory.Move(text, text2);
			}
		}
		catch
		{
		}
	}

	public static void Shutdown()
	{
		if (_dbman_settings == null)
		{
			return;
		}
		try
		{
			string path = _db_data_path + _dbman_settings.ActiveDB_GUID.ToString() + "\\dbman.json";
			if (File.Exists(path) && JsonConvert.DeserializeObject<DatabaseInfo>(File.ReadAllText(path)).BackupOnShutdown)
			{
				TakeBackup(Guid.Empty, "Shutdown", auto: true);
			}
		}
		catch
		{
		}
	}

	public static void DBWritten()
	{
		if (_ignore_written || _dbman_settings == null)
		{
			return;
		}
		string text = _db_data_path + _dbman_settings.ActiveDB_GUID.ToString();
		string text2 = text + "\\dbman.json";
		if (!File.Exists(text2))
		{
			return;
		}
		string value = File.ReadAllText(text2);
		DatabaseInfo databaseInfo = JsonConvert.DeserializeObject<DatabaseInfo>(value);
		Dictionary<string, string> varsDictionary = DB.GetVarsDictionary("Options");
		if (varsDictionary.ContainsKey("comboRadioModel"))
		{
			databaseInfo.Model = HardwareSpecific.StringModelToEnum(varsDictionary["comboRadioModel"]);
		}
		else
		{
			databaseInfo.Model = HPSDRModel.HERMES;
		}
		databaseInfo.VersionString = DB.VersionString;
		databaseInfo.VersionNumber = DB.VersionNumber;
		DirectoryInfo directoryInfo = new DirectoryInfo(text);
		databaseInfo.TotalContentsSize = calculateFolderSize(directoryInfo);
		FileInfo fileInfo = new FileInfo(text2);
		databaseInfo.Size = fileInfo.Length;
		databaseInfo.LastChanged = fileInfo.LastWriteTime;
		value = JsonConvert.SerializeObject(databaseInfo, Newtonsoft.Json.Formatting.Indented);
		try
		{
			File.WriteAllText(text2, value);
		}
		catch (Exception)
		{
		}
	}

	private static bool createNewDB(bool check_for_old_db, bool make_active, out bool old_db_found, string description = "")
	{
		bool flag = true;
		string text = createNewDBFolder(out var guid);
		old_db_found = false;
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		string text2 = _app_data_path + "database.xml";
		string text3 = text + "\\database.xml";
		if (check_for_old_db && File.Exists(text2))
		{
			File.Copy(text2, text3, overwrite: true);
			string text4 = _app_data_path + "old_database.xml";
			int num = 1;
			while (File.Exists(text4))
			{
				text4 = Path.Combine(_app_data_path, $"old{num}_database.xml");
				num++;
			}
			File.Move(text2, text4);
			old_db_found = true;
		}
		_ignore_written = true;
		string text5 = "";
		if (!string.IsNullOrEmpty(DB.FileName))
		{
			text5 = DB.FileName;
			DB.Exit();
		}
		DB.FileName = text3;
		flag = DB.Init();
		_ignore_written = false;
		if (flag)
		{
			Dictionary<string, string> varsDictionary = DB.GetVarsDictionary("Options");
			DatabaseInfo databaseInfo = new DatabaseInfo();
			databaseInfo.GUID = guid;
			databaseInfo.FullPath = text;
			databaseInfo.Description = (string.IsNullOrEmpty(description) ? "Default" : description);
			if (varsDictionary.ContainsKey("comboRadioModel"))
			{
				databaseInfo.Model = HardwareSpecific.StringModelToEnum(varsDictionary["comboRadioModel"]);
			}
			databaseInfo.VersionString = DB.VersionString;
			databaseInfo.VersionNumber = DB.VersionNumber;
			string contents = JsonConvert.SerializeObject(databaseInfo, Newtonsoft.Json.Formatting.Indented);
			try
			{
				File.WriteAllText(text + "\\dbman.json", contents);
			}
			catch (Exception)
			{
				flag = false;
			}
			if (make_active & flag)
			{
				flag = makeDBActive(guid);
				_dbman_settings = getActiveDB();
			}
		}
		if (!make_active && !string.IsNullOrEmpty(text5))
		{
			_ignore_written = true;
			DB.FileName = text5;
			DB.Init();
			_ignore_written = false;
		}
		return flag;
	}

	private static DBSettings getActiveDB()
	{
		bool flag = false;
		DBSettings result = null;
		try
		{
			string path = _db_data_path + _unique_instance_id + "dbman_settings.json";
			if (File.Exists(path))
			{
				result = JsonConvert.DeserializeObject<DBSettings>(File.ReadAllText(path));
				flag = true;
			}
		}
		catch
		{
		}
		if (flag)
		{
			return result;
		}
		return null;
	}

	private static bool makeDBActive(Guid guid)
	{
		bool result = true;
		try
		{
			if (File.Exists(_db_data_path + guid.ToString() + "\\database.xml"))
			{
				string contents = JsonConvert.SerializeObject(new DBSettings
				{
					ActiveDB_GUID = guid,
					ActiveDB_File = guid.ToString() + "\\database.xml"
				}, Newtonsoft.Json.Formatting.Indented);
				try
				{
					File.WriteAllText(_db_data_path + _unique_instance_id + "dbman_settings.json", contents);
				}
				catch (Exception)
				{
					result = false;
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

	private static string createNewDBFolder(out Guid guid)
	{
		try
		{
			guid = Guid.NewGuid();
			string text = _db_data_path + guid.ToString();
			Directory.CreateDirectory(text);
			Directory.CreateDirectory(text + "\\backups");
			return text;
		}
		catch
		{
			guid = Guid.Empty;
			return "";
		}
	}

	private static Dictionary<Guid, DatabaseInfo> getAvailableDBs()
	{
		List<Guid> list = new List<Guid>();
		Dictionary<Guid, DatabaseInfo> dictionary = new Dictionary<Guid, DatabaseInfo>();
		if (!File.Exists(_db_data_path + _unique_instance_id + "dbman_settings.json"))
		{
			return dictionary;
		}
		List<Guid> allActiveDBGUIDs = getAllActiveDBGUIDs(_db_data_path);
		try
		{
			string[] directories = Directory.GetDirectories(_db_data_path);
			foreach (string text in directories)
			{
				if (!Guid.TryParse(Path.GetFileName(text), out var result) || allActiveDBGUIDs.Contains(result))
				{
					continue;
				}
				DirectoryInfo directoryInfo = new DirectoryInfo(text);
				long size = 0L;
				DateTime lastChanged = DateTime.Now;
				DateTime creationTime = DateTime.Now;
				try
				{
					string text2 = text + "\\database.xml";
					if (File.Exists(text2))
					{
						FileInfo fileInfo = new FileInfo(text2);
						size = fileInfo.Length;
						lastChanged = fileInfo.LastWriteTime;
						creationTime = fileInfo.CreationTime;
					}
				}
				catch
				{
					continue;
				}
				string versionString = "unknown";
				string versionNumber = "unknown";
				string description = "";
				HPSDRModel model = HPSDRModel.FIRST;
				bool backupOnStartup = false;
				bool backupOnShutdown = false;
				try
				{
					string path = text + "\\dbman.json";
					if (File.Exists(path))
					{
						DatabaseInfo databaseInfo = JsonConvert.DeserializeObject<DatabaseInfo>(File.ReadAllText(path));
						if (databaseInfo.GUID != result)
						{
							list.Add(result);
							continue;
						}
						description = databaseInfo.Description;
						model = databaseInfo.Model;
						versionString = databaseInfo.VersionString;
						versionNumber = databaseInfo.VersionNumber;
						backupOnStartup = databaseInfo.BackupOnStartup;
						backupOnShutdown = databaseInfo.BackupOnShutdown;
					}
				}
				catch
				{
					continue;
				}
				DatabaseInfo value = new DatabaseInfo
				{
					GUID = result,
					FullPath = directoryInfo.FullName,
					FolderCreationTime = directoryInfo.CreationTime,
					TotalContentsSize = calculateFolderSize(directoryInfo),
					Size = size,
					Description = description,
					Model = model,
					LastChanged = lastChanged,
					CreationTime = creationTime,
					VersionString = versionString,
					VersionNumber = versionNumber,
					BackupOnStartup = backupOnStartup,
					BackupOnShutdown = backupOnShutdown
				};
				dictionary.Add(result, value);
			}
		}
		catch (Exception)
		{
		}
		foreach (Guid item in list)
		{
			moveToBroken(item);
		}
		DatabaseInfo databaseInfo2 = null;
		if (_dbman_settings != null && dictionary.ContainsKey(_dbman_settings.ActiveDB_GUID))
		{
			databaseInfo2 = dictionary[_dbman_settings.ActiveDB_GUID];
			dictionary.Remove(_dbman_settings.ActiveDB_GUID);
		}
		Dictionary<Guid, DatabaseInfo> dictionary2 = dictionary.OrderByDescending((KeyValuePair<Guid, DatabaseInfo> entry) => entry.Value.LastChanged).ToDictionary((KeyValuePair<Guid, DatabaseInfo> entry) => entry.Key, (KeyValuePair<Guid, DatabaseInfo> entry) => entry.Value);
		if (databaseInfo2 != null)
		{
			dictionary2 = dictionary2.Prepend(new KeyValuePair<Guid, DatabaseInfo>(databaseInfo2.GUID, databaseInfo2)).ToDictionary((KeyValuePair<Guid, DatabaseInfo> entry) => entry.Key, (KeyValuePair<Guid, DatabaseInfo> entry) => entry.Value);
		}
		return dictionary2;
	}

	private static List<Guid> getAllActiveDBGUIDs(string path)
	{
		List<Guid> list = new List<Guid>();
		try
		{
			string[] files = Directory.GetFiles(path, "*dbman_settings.json", SearchOption.TopDirectoryOnly);
			string text = _unique_instance_id + "dbman_settings.json";
			string[] array = files;
			foreach (string path2 in array)
			{
				if (Path.GetFileName(path2) == text)
				{
					continue;
				}
				try
				{
					DBSettings dBSettings = JsonConvert.DeserializeObject<DBSettings>(File.ReadAllText(path2));
					if (dBSettings != null && dBSettings.ActiveDB_GUID != Guid.Empty)
					{
						list.Add(dBSettings.ActiveDB_GUID);
					}
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
		return list;
	}

	private static long calculateFolderSize(DirectoryInfo directoryInfo)
	{
		long num = 0L;
		try
		{
			FileInfo[] files = directoryInfo.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				num += fileInfo.Length;
			}
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			foreach (DirectoryInfo directoryInfo2 in directories)
			{
				num += calculateFolderSize(directoryInfo2);
			}
		}
		catch (UnauthorizedAccessException)
		{
		}
		catch (Exception)
		{
		}
		return num;
	}

	public static void MakeActiveDB(Guid guid)
	{
		if (MessageBox.Show("Do you want to activate the selected database? This will cause Thetis to restart.", "Database Manager Issue", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, (MessageBoxOptions)262144) == DialogResult.Yes)
		{
			if (makeDBActive(guid))
			{
				_frm_dbman.Hide();
				Console.getConsole().Restart = true;
				Console.getConsole().Close();
			}
			else
			{
				MessageBox.Show("There was an issue making the database active.", "Database Manager Issue", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			}
		}
	}

	public static void NewDB()
	{
		if (_dbman_settings != null)
		{
			string text = InputBox.Show("Database Description", "Please provide a description for this new database.", "", to_top: true);
			if (!string.IsNullOrEmpty(text))
			{
				createNewDB(check_for_old_db: false, make_active: false, out var _, text);
				Dictionary<Guid, DatabaseInfo> availableDBs = getAvailableDBs();
				_frm_dbman.InitAvailableDBs(availableDBs, _dbman_settings.ActiveDB_GUID, Guid.Empty);
			}
		}
	}

	public static void BackupOnStartUpToggle(Guid guid)
	{
		if (_dbman_settings == null)
		{
			return;
		}
		string path = _db_data_path + guid.ToString() + "\\dbman.json";
		if (!File.Exists(path))
		{
			return;
		}
		try
		{
			string value = File.ReadAllText(path);
			DatabaseInfo? databaseInfo = JsonConvert.DeserializeObject<DatabaseInfo>(value);
			databaseInfo.BackupOnStartup = !databaseInfo.BackupOnStartup;
			value = JsonConvert.SerializeObject(databaseInfo, Newtonsoft.Json.Formatting.Indented);
			File.WriteAllText(path, value);
			Dictionary<Guid, DatabaseInfo> availableDBs = getAvailableDBs();
			_frm_dbman.InitAvailableDBs(availableDBs, _dbman_settings.ActiveDB_GUID, guid);
		}
		catch
		{
		}
	}

	public static void BackupOnShutDownToggle(Guid guid)
	{
		if (_dbman_settings == null)
		{
			return;
		}
		string path = _db_data_path + guid.ToString() + "\\dbman.json";
		if (!File.Exists(path))
		{
			return;
		}
		try
		{
			string value = File.ReadAllText(path);
			DatabaseInfo? databaseInfo = JsonConvert.DeserializeObject<DatabaseInfo>(value);
			databaseInfo.BackupOnShutdown = !databaseInfo.BackupOnShutdown;
			value = JsonConvert.SerializeObject(databaseInfo, Newtonsoft.Json.Formatting.Indented);
			File.WriteAllText(path, value);
			Dictionary<Guid, DatabaseInfo> availableDBs = getAvailableDBs();
			_frm_dbman.InitAvailableDBs(availableDBs, _dbman_settings.ActiveDB_GUID, guid);
		}
		catch
		{
		}
	}

	public static void RemoveDB(Guid guid, bool force = false)
	{
		if (_dbman_settings == null)
		{
			return;
		}
		bool shiftKeyDown = Common.ShiftKeyDown;
		if (shiftKeyDown && MessageBox.Show("Force delete detected. Are you sure?", "Database Manager Issue", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, (MessageBoxOptions)262144) == DialogResult.Yes)
		{
			force = true;
		}
		string text = "";
		if (!force)
		{
			text = InputBox.Show("Database Removal", "Please enter the matching description to remove this database.", "", to_top: true);
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
		}
		bool flag = false;
		string path = _db_data_path + guid.ToString() + "\\dbman.json";
		if (File.Exists(path))
		{
			DatabaseInfo databaseInfo = JsonConvert.DeserializeObject<DatabaseInfo>(File.ReadAllText(path));
			if (force || text == databaseInfo.Description)
			{
				try
				{
					Directory.Delete(_db_data_path + guid.ToString(), recursive: true);
					flag = true;
					Dictionary<Guid, DatabaseInfo> availableDBs = getAvailableDBs();
					_frm_dbman.InitAvailableDBs(availableDBs, _dbman_settings.ActiveDB_GUID, Guid.Empty);
				}
				catch
				{
				}
			}
			else
			{
				flag = true;
			}
		}
		else if (shiftKeyDown && Directory.Exists(_db_data_path + guid.ToString()))
		{
			try
			{
				Directory.Delete(_db_data_path + guid.ToString(), recursive: true);
				flag = true;
				Dictionary<Guid, DatabaseInfo> availableDBs2 = getAvailableDBs();
				_frm_dbman.InitAvailableDBs(availableDBs2, _dbman_settings.ActiveDB_GUID, Guid.Empty);
			}
			catch
			{
			}
		}
		if (!flag)
		{
			MessageBox.Show("There was an issue removing the database.", "Database Manager Issue", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		}
	}

	public static void DuplicateDB(Guid guid)
	{
		if (_dbman_settings == null)
		{
			return;
		}
		string text = InputBox.Show("Database Duplication", "Please enter a description for the duplicate.", "", to_top: true);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		Guid gUID = Guid.NewGuid();
		string sourceFolder = _db_data_path + guid.ToString();
		string text2 = _db_data_path + gUID.ToString();
		bool flag = copyFolder(sourceFolder, text2);
		if (flag)
		{
			try
			{
				string path = text2 + "\\dbman.json";
				if (File.Exists(path))
				{
					string value = File.ReadAllText(path);
					DatabaseInfo? databaseInfo = JsonConvert.DeserializeObject<DatabaseInfo>(value);
					databaseInfo.GUID = gUID;
					databaseInfo.FullPath = text2;
					databaseInfo.CreationTime = DateTime.Now;
					databaseInfo.FolderCreationTime = DateTime.Now;
					databaseInfo.Description = text;
					databaseInfo.LastChanged = DateTime.Now;
					value = JsonConvert.SerializeObject(databaseInfo, Newtonsoft.Json.Formatting.Indented);
					File.WriteAllText(path, value);
				}
			}
			catch
			{
				flag = false;
			}
			if (flag)
			{
				Dictionary<Guid, DatabaseInfo> availableDBs = getAvailableDBs();
				_frm_dbman.InitAvailableDBs(availableDBs, _dbman_settings.ActiveDB_GUID, Guid.Empty);
			}
		}
		if (!flag)
		{
			MessageBox.Show("There was an issue duplicating the database.", "Database Manager Issue", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		}
	}

	public static bool copyFolder(string sourceFolder, string destinationFolder)
	{
		try
		{
			Directory.CreateDirectory(destinationFolder);
			string[] files = Directory.GetFiles(sourceFolder);
			foreach (string obj in files)
			{
				string fileName = Path.GetFileName(obj);
				string destFileName = Path.Combine(destinationFolder, fileName);
				File.Copy(obj, destFileName);
			}
			string[] directories = Directory.GetDirectories(sourceFolder);
			foreach (string obj2 in directories)
			{
				string fileName2 = Path.GetFileName(obj2);
				string destinationFolder2 = Path.Combine(destinationFolder, fileName2);
				copyFolder(obj2, destinationFolder2);
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static void SelectedAvailable(Guid guid)
	{
		if (guid == Guid.Empty)
		{
			if (_dbman_settings == null)
			{
				List<BackupFileInfo> backups = new List<BackupFileInfo>();
				_frm_dbman.InitBackups(backups);
				return;
			}
			guid = _dbman_settings.ActiveDB_GUID;
		}
		List<BackupFileInfo> orderedBackupFiles = getOrderedBackupFiles(_db_data_path + guid.ToString() + "\\backups");
		_frm_dbman.InitBackups(orderedBackupFiles);
	}

	public static bool TakeBackup(Guid highlighted, string description = "", bool auto = false)
	{
		if (_dbman_settings == null)
		{
			return false;
		}
		string text;
		if (string.IsNullOrEmpty(description))
		{
			text = InputBox.Show("Database Backup", "Please enter a description for the backup.", "", to_top: true);
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
		}
		else
		{
			text = description;
		}
		bool flag = false;
		Guid guid = _dbman_settings.ActiveDB_GUID;
		string text2 = _db_data_path + guid.ToString() + "\\backups";
		try
		{
			string text3 = "";
			string text4 = _db_data_path + guid.ToString() + "\\database.xml";
			if (File.Exists(text4) && Directory.Exists(text2))
			{
				text3 = createUniqueFilename(text2);
				File.Copy(text4, text3, overwrite: true);
				flag = true;
			}
			if (flag)
			{
				BackupFileInfo value = new BackupFileInfo
				{
					Auto = auto,
					Description = text
				};
				string path = Path.ChangeExtension(text3, ".json");
				string contents = JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.Indented);
				try
				{
					File.WriteAllText(path, contents);
				}
				catch
				{
					flag = false;
				}
			}
		}
		catch
		{
		}
		if (flag && _prune_backups)
		{
			pruneForGFS(text2);
		}
		if (highlighted != Guid.Empty)
		{
			guid = highlighted;
		}
		getBackups(guid);
		return flag;
	}

	private static void getBackups(Guid guid)
	{
		List<BackupFileInfo> orderedBackupFiles = getOrderedBackupFiles(_db_data_path + guid.ToString() + "\\backups");
		_frm_dbman.InitBackups(orderedBackupFiles);
	}

	private static List<BackupFileInfo> getOrderedBackupFiles(string backupFolderPath)
	{
		List<BackupFileInfo> list = new List<BackupFileInfo>();
		if (!Directory.Exists(backupFolderPath))
		{
			return list;
		}
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		string[] files = Directory.GetFiles(backupFolderPath, "database_backup_*.xml");
		foreach (string text in files)
		{
			try
			{
				string[] array = Path.GetFileNameWithoutExtension(text).Split('_');
				if (array.Length >= 3 && long.TryParse(array[2], out var result))
				{
					DateTime dateTime2 = dateTime.AddSeconds(result);
					DateTime dateTimeOfBackup = dateTime2.ToLocalTime();
					TimeSpan ageSinceBackedUp = DateTime.UtcNow - dateTime2;
					string path = Path.ChangeExtension(text, ".json");
					string description = "Default";
					bool auto = false;
					if (File.Exists(path))
					{
						BackupFileInfo? backupFileInfo = JsonConvert.DeserializeObject<BackupFileInfo>(File.ReadAllText(path));
						description = backupFileInfo.Description;
						auto = backupFileInfo.Auto;
					}
					list.Add(new BackupFileInfo
					{
						FullFilePath = text,
						DateTimeOfBackup = dateTimeOfBackup,
						SecondsSinceEpoch = result,
						AgeSinceBackedUp = ageSinceBackedUp,
						Description = description,
						Auto = auto
					});
				}
			}
			catch
			{
			}
		}
		return list.OrderByDescending((BackupFileInfo f) => f.SecondsSinceEpoch).ToList();
	}

	private static string createUniqueFilename(string directoryPath)
	{
		long num = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		string text = $"database_backup_{num}";
		string text2 = Path.Combine(directoryPath, text + ".xml");
		int num2 = 1;
		while (File.Exists(text2))
		{
			text2 = Path.Combine(directoryPath, $"{text}_{num2}.xml");
			num2++;
		}
		return text2;
	}

	public static void RemoveBackupDB(List<string> file_paths)
	{
		if (file_paths.Count < 1 || MessageBox.Show((file_paths.Count == 1) ? "Do you want to remove this backup?" : $"Do you want to remove these {file_paths.Count} backups?", "Remove Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, (MessageBoxOptions)262144) != DialogResult.Yes)
		{
			return;
		}
		foreach (string file_path in file_paths)
		{
			if (File.Exists(file_path))
			{
				try
				{
					File.Delete(file_path);
				}
				catch
				{
				}
			}
			string path = Path.ChangeExtension(file_path, ".json");
			if (File.Exists(path))
			{
				try
				{
					File.Delete(path);
				}
				catch
				{
				}
			}
		}
	}

	public static void Import()
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "XML files (*.xml)|*.xml";
		openFileDialog.Title = "Select an XML file";
		openFileDialog.Multiselect = false;
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		string fileName = openFileDialog.FileName;
		if (Path.GetExtension(fileName).Equals(".xml", StringComparison.OrdinalIgnoreCase))
		{
			bool flag;
			try
			{
				new XmlDocument().Load(fileName);
				flag = true;
			}
			catch
			{
				flag = false;
			}
			if (flag)
			{
				flag = DB.ImportAndMergeDatabase(fileName, out var log, ignore_merged: false);
				try
				{
					File.WriteAllText(_app_data_path + "ImportLog.txt", log);
				}
				catch
				{
				}
				if (flag)
				{
					MessageBox.Show("The database was imported sucessfully. Thetis will now restart.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
					_frm_dbman.Hide();
					Console.getConsole().Restart = true;
					Console.getConsole().Close();
				}
				else
				{
					MessageBox.Show("There was a problem importing the database. The database file seems to be corrupt.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
				}
			}
			else
			{
				MessageBox.Show("There was a problem importing the database. The xml file seems to be corrupt.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			}
		}
		else
		{
			MessageBox.Show("The database file needs a .xml file extension.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		}
	}

	public static void ImportAsAvailable(Guid selected)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "XML files (*.xml)|*.xml";
		openFileDialog.Title = "Select an XML file";
		openFileDialog.Multiselect = false;
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		string fileName = openFileDialog.FileName;
		if (Path.GetExtension(fileName).Equals(".xml", StringComparison.OrdinalIgnoreCase))
		{
			bool flag;
			try
			{
				new XmlDocument().Load(fileName);
				flag = true;
			}
			catch
			{
				flag = false;
			}
			if (flag)
			{
				string text = InputBox.Show("Database Import", "Please enter a description for the imported database.", "", to_top: true);
				if (string.IsNullOrEmpty(text))
				{
					return;
				}
				_ignore_written = true;
				string text2 = "";
				if (!string.IsNullOrEmpty(DB.FileName))
				{
					text2 = DB.FileName;
					DB.Exit();
				}
				DB.FileName = fileName;
				flag = DB.Init();
				_ignore_written = false;
				HPSDRModel model = HPSDRModel.HERMES;
				string versionString = "";
				string versionNumber = "";
				if (flag)
				{
					Dictionary<string, string> varsDictionary = DB.GetVarsDictionary("Options");
					model = ((!varsDictionary.ContainsKey("comboRadioModel")) ? HPSDRModel.HERMES : HardwareSpecific.StringModelToEnum(varsDictionary["comboRadioModel"]));
					versionString = DB.VersionString;
					versionNumber = DB.VersionNumber;
				}
				if (!string.IsNullOrEmpty(text2))
				{
					_ignore_written = true;
					DB.FileName = text2;
					DB.Init();
					_ignore_written = false;
				}
				if (flag)
				{
					string text3 = "";
					string text4 = createNewDBFolder(out var guid);
					if (!string.IsNullOrEmpty(text4))
					{
						try
						{
							text3 = text4 + "\\database.xml";
							File.Copy(fileName, text3);
							flag = File.Exists(text3);
						}
						catch
						{
							flag = false;
						}
						if (flag)
						{
							DirectoryInfo directoryInfo = new DirectoryInfo(text4);
							FileInfo fileInfo = new FileInfo(text3);
							DatabaseInfo value = new DatabaseInfo
							{
								GUID = guid,
								FullPath = text4,
								FolderCreationTime = DateTime.Now,
								TotalContentsSize = calculateFolderSize(directoryInfo),
								Size = fileInfo.Length,
								Description = text,
								Model = model,
								LastChanged = fileInfo.LastWriteTime,
								CreationTime = fileInfo.CreationTime,
								VersionString = versionString,
								VersionNumber = versionNumber,
								BackupOnStartup = false,
								BackupOnShutdown = false
							};
							string path = text4 + "\\dbman.json";
							string contents = JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.Indented);
							try
							{
								File.WriteAllText(path, contents);
							}
							catch (Exception)
							{
								flag = false;
								MessageBox.Show("There was a problem writing the database info. Unable to copy the source database file.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
							}
							if (flag)
							{
								Dictionary<Guid, DatabaseInfo> availableDBs = getAvailableDBs();
								_frm_dbman.InitAvailableDBs(availableDBs, _dbman_settings.ActiveDB_GUID, Guid.Empty);
							}
						}
						else
						{
							MessageBox.Show("There was a problem importing the database. Unable to copy the source database file.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
						}
					}
					else
					{
						MessageBox.Show("There was a problem importing the database.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
					}
				}
				else
				{
					MessageBox.Show("There was a problem importing the database. The database file seems to be corrupt.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
				}
			}
			else
			{
				MessageBox.Show("There was a problem importing the database. The xml file seems to be corrupt.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			}
		}
		else
		{
			MessageBox.Show("The database file needs a .xml file extension.", "Database Manager", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		}
	}

	public static void Rename(Guid guid)
	{
		if (_dbman_settings == null)
		{
			return;
		}
		string path = string.Concat(_db_data_path + guid.ToString(), "\\dbman.json");
		if (!File.Exists(path))
		{
			return;
		}
		try
		{
			string value = File.ReadAllText(path);
			DatabaseInfo databaseInfo = JsonConvert.DeserializeObject<DatabaseInfo>(value);
			string text = InputBox.Show("Database Change Description", "Please edit the description.", databaseInfo.Description, to_top: true);
			if (!string.IsNullOrEmpty(text) && !(text == databaseInfo.Description))
			{
				databaseInfo.Description = text;
				value = JsonConvert.SerializeObject(databaseInfo, Newtonsoft.Json.Formatting.Indented);
				File.WriteAllText(path, value);
				Dictionary<Guid, DatabaseInfo> availableDBs = getAvailableDBs();
				_frm_dbman.InitAvailableDBs(availableDBs, _dbman_settings.ActiveDB_GUID, guid);
			}
		}
		catch
		{
		}
	}

	public static void RenameBackup(Guid guid, string file_path)
	{
		string path = Path.ChangeExtension(file_path, ".json");
		bool flag = File.Exists(path);
		try
		{
			string text;
			bool auto;
			string value;
			if (flag)
			{
				value = File.ReadAllText(path);
				BackupFileInfo? backupFileInfo = JsonConvert.DeserializeObject<BackupFileInfo>(value);
				text = backupFileInfo.Description;
				auto = backupFileInfo.Auto;
			}
			else
			{
				auto = false;
				text = "Default";
			}
			string text2 = InputBox.Show("Database Change Description", "Please edit the description.", text, to_top: true);
			if (string.IsNullOrEmpty(text2) || text2 == text)
			{
				return;
			}
			value = JsonConvert.SerializeObject(new BackupFileInfo
			{
				Description = text2,
				Auto = auto
			}, Newtonsoft.Json.Formatting.Indented);
			File.WriteAllText(path, value);
			if (guid == Guid.Empty)
			{
				if (_dbman_settings == null)
				{
					List<BackupFileInfo> backups = new List<BackupFileInfo>();
					_frm_dbman.InitBackups(backups);
					return;
				}
				guid = _dbman_settings.ActiveDB_GUID;
			}
			string db_data_path = _db_data_path;
			Guid guid2 = guid;
			List<BackupFileInfo> orderedBackupFiles = getOrderedBackupFiles(db_data_path + guid2.ToString() + "\\backups");
			_frm_dbman.InitBackups(orderedBackupFiles);
		}
		catch
		{
		}
	}

	public static void OpenFolder(Guid guid)
	{
		if (guid == Guid.Empty && _dbman_settings != null)
		{
			guid = _dbman_settings.ActiveDB_GUID;
		}
		if (guid == Guid.Empty)
		{
			return;
		}
		string text = _db_data_path + guid.ToString();
		try
		{
			if (Directory.Exists(text))
			{
				Process.Start("explorer.exe", text);
			}
		}
		catch
		{
		}
	}

	public static void Export(Guid guid)
	{
		string text = _db_data_path + guid.ToString();
		string path = text + "\\dbman.json";
		if (!File.Exists(path))
		{
			return;
		}
		string description = JsonConvert.DeserializeObject<DatabaseInfo>(File.ReadAllText(path)).Description;
		string text2 = Common.DateTimeStringForFile();
		string fileName = "Thetis_database_export_" + description + "_" + text2 + ".xml";
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
			DefaultExt = "xml",
			FileName = fileName,
			Title = "Export Database",
			InitialDirectory = folderPath
		};
		if (saveFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		string fileName2 = saveFileDialog.FileName;
		try
		{
			if (File.Exists(fileName2))
			{
				File.Delete(fileName2);
			}
		}
		catch
		{
		}
		if (guid == _dbman_settings.ActiveDB_GUID)
		{
			try
			{
				DB.WriteDB(fileName2);
				return;
			}
			catch
			{
				return;
			}
		}
		try
		{
			File.Copy(text + "\\database.xml", fileName2);
		}
		catch
		{
		}
	}

	public static void ExportBackup(string desc, string path)
	{
		if (!File.Exists(path))
		{
			return;
		}
		string text = Common.DateTimeStringForFile();
		string fileName = ((!string.IsNullOrEmpty(desc)) ? ("Thetis_database_export_backup_" + desc + "_" + text + ".xml") : ("Thetis_database_export_backup_" + text + ".xml"));
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
			DefaultExt = "xml",
			FileName = fileName,
			Title = "Export Database",
			InitialDirectory = folderPath
		};
		if (saveFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		string fileName2 = saveFileDialog.FileName;
		try
		{
			if (File.Exists(fileName2))
			{
				File.Delete(fileName2);
			}
		}
		catch
		{
		}
		try
		{
			File.Copy(path, fileName2);
		}
		catch
		{
		}
	}

	public static void MakeBackupAvailable(string file_path)
	{
		if (_dbman_settings == null || !File.Exists(file_path))
		{
			return;
		}
		string text = InputBox.Show("Make Database Available", "Please enter a description for the database.", "", to_top: true);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		try
		{
			Guid gUID = Guid.NewGuid();
			string text2 = _db_data_path + gUID.ToString();
			string path = _db_data_path + gUID.ToString() + "\\backups";
			string text3 = text2 + "\\database.xml";
			string path2 = text2 + "\\dbman.json";
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			File.Copy(file_path, text3, overwrite: true);
			HPSDRModel hPSDRModel = HPSDRModel.HERMES;
			string text4 = "unknown";
			string text5 = "unknown";
			_ignore_written = true;
			string text6 = "";
			if (!string.IsNullOrEmpty(DB.FileName))
			{
				text6 = DB.FileName;
				DB.Exit();
			}
			DB.FileName = text3;
			DB.Init();
			_ignore_written = false;
			Dictionary<string, string> varsDictionary = DB.GetVarsDictionary("Options");
			hPSDRModel = ((!varsDictionary.ContainsKey("comboRadioModel")) ? HPSDRModel.HERMES : HardwareSpecific.StringModelToEnum(varsDictionary["comboRadioModel"]));
			text4 = DB.VersionString;
			text5 = DB.VersionNumber;
			if (!string.IsNullOrEmpty(text6))
			{
				_ignore_written = true;
				DB.FileName = text6;
				DB.Init();
				_ignore_written = false;
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(text2);
			FileInfo fileInfo = new FileInfo(text3);
			string contents = JsonConvert.SerializeObject(new DatabaseInfo
			{
				GUID = gUID,
				FullPath = directoryInfo.FullName,
				FolderCreationTime = directoryInfo.CreationTime,
				TotalContentsSize = calculateFolderSize(directoryInfo),
				Size = fileInfo.Length,
				Description = text,
				Model = hPSDRModel,
				LastChanged = fileInfo.LastWriteTime,
				CreationTime = fileInfo.CreationTime,
				VersionString = text4,
				VersionNumber = text5
			}, Newtonsoft.Json.Formatting.Indented);
			try
			{
				File.WriteAllText(path2, contents);
			}
			catch (Exception)
			{
			}
			Dictionary<Guid, DatabaseInfo> availableDBs = getAvailableDBs();
			_frm_dbman.InitAvailableDBs(availableDBs, _dbman_settings.ActiveDB_GUID, Guid.Empty);
		}
		catch
		{
		}
	}

	private static int getWeekOfYear(DateTime date)
	{
		return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
	}

	private static void pruneForGFS(string backup_folder_path)
	{
		if (!_prune_backups || !Directory.Exists(backup_folder_path))
		{
			return;
		}
		bool flag = false;
		List<FileInfo> list = null;
		List<FileInfo> list2 = null;
		try
		{
			list2 = (from f in new DirectoryInfo(backup_folder_path).GetFiles("*.xml")
				orderby f.CreationTime descending
				select f).ToList();
			DateTime now = DateTime.Now;
			List<FileInfo> first = list2.Where((FileInfo f) => (now - f.CreationTime).TotalDays <= 7.0).ToList();
			List<FileInfo> second = (from f in list2
				where (now - f.CreationTime).TotalDays > 7.0
				group f by new
				{
					Year = f.CreationTime.Year,
					Week = getWeekOfYear(f.CreationTime)
				} into g
				select g.OrderByDescending((FileInfo f) => f.CreationTime).First()).ToList();
			List<FileInfo> second2 = (from f in list2
				where (now - f.CreationTime).TotalDays > 30.0
				group f by new
				{
					f.CreationTime.Year,
					f.CreationTime.Month
				} into g
				select g.OrderByDescending((FileInfo f) => f.CreationTime).First()).ToList();
			List<FileInfo> second3 = (from f in list2
				where (now - f.CreationTime).TotalDays > 365.0
				group f by f.CreationTime.Year into g
				select g.OrderByDescending((FileInfo f) => f.CreationTime).First()).ToList();
			list = first.Concat(second).Concat(second2).Concat(second3)
				.Distinct()
				.ToList();
			flag = true;
		}
		catch
		{
		}
		if (!flag)
		{
			return;
		}
		foreach (FileInfo item in list2)
		{
			if (list.Contains(item))
			{
				continue;
			}
			try
			{
				bool flag2 = false;
				string path = Path.ChangeExtension(item.FullName, ".json");
				if (File.Exists(path))
				{
					BackupFileInfo backupFileInfo = JsonConvert.DeserializeObject<BackupFileInfo>(File.ReadAllText(path));
					flag2 = backupFileInfo.Auto || backupFileInfo.Description == "Startup" || backupFileInfo.Description == "Shutdown";
				}
				if (flag2)
				{
					item.Delete();
					if (File.Exists(path))
					{
						File.Delete(path);
					}
				}
			}
			catch
			{
			}
		}
	}
}
