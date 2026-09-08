using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Thetis;

public class Skin
{
	private enum ImageState
	{
		NormalUp,
		NormalDown,
		DisabledUp,
		DisabledDown,
		FocusedUp,
		FocusedDown,
		MouseOverUp,
		MouseOverDown
	}

	private static string name;

	private static string path;

	private static string apf_skin_path = "";

	private const string pic_file_ext = ".png";

	private static Console m_objConsole;

	private static string app_data_path = "";

	private static Dictionary<string, ImageList> _shared_image_lists = new Dictionary<string, ImageList>();

	private static Dictionary<string, Image> _image_cache = new Dictionary<string, Image>();

	private static Dictionary<string, string> _image_cache_map = new Dictionary<string, string>();

	public static string AppDataPath
	{
		set
		{
			app_data_path = value;
		}
	}

	public static void SetConsole(Console objConsole)
	{
		m_objConsole = objConsole;
	}

	public static void Save(string name, string p, Form f)
	{
		path = p + "\\" + name;
		Skin.name = name;
		XmlTextWriter xmlTextWriter = new XmlTextWriter(path + "\\" + name + ".xml", Encoding.UTF8);
		xmlTextWriter.Formatting = Formatting.Indented;
		xmlTextWriter.WriteStartDocument();
		xmlTextWriter.WriteStartElement("Form");
		SaveForm(f, xmlTextWriter);
		xmlTextWriter.WriteStartElement("Controls");
		foreach (Control control in f.Controls)
		{
			Save(control, xmlTextWriter);
		}
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndDocument();
		xmlTextWriter.Close();
	}

	public static bool Restore(string name, string p, Form f)
	{
		path = p + "\\" + name;
		Skin.name = name;
		try
		{
			string directoryName = Path.GetDirectoryName(Path.GetDirectoryName(path));
			if (!string.IsNullOrEmpty(directoryName))
			{
				apf_skin_path = directoryName + "\\SkinsAPF";
			}
		}
		catch
		{
		}
		if (File.Exists(path + "\\" + f.Name + "\\" + f.Name + ".png"))
		{
			if (!(f is Console))
			{
				f.BackgroundImage = loadImage(path + "\\" + f.Name + "\\" + f.Name + ".png");
			}
			else
			{
				m_objConsole.CachedBackgroundImage = loadImage(path + "\\" + f.Name + "\\" + f.Name + ".png");
			}
		}
		else if (File.Exists(path + "\\Console\\Console.png"))
		{
			if (!(f is Console))
			{
				f.BackgroundImage = loadImage(path + "\\Console\\Console.png");
			}
			else
			{
				m_objConsole.CachedBackgroundImage = loadImage(path + "\\Console\\Console.png");
			}
		}
		else if (!(f is Console))
		{
			f.BackgroundImage = null;
		}
		else
		{
			m_objConsole.CachedBackgroundImage = null;
		}
		foreach (Control control in f.Controls)
		{
			ReadImages(control);
		}
		if (!File.Exists(path + "\\" + name + ".xml"))
		{
			return true;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(path + "\\" + name + ".xml");
		}
		catch (Exception ex)
		{
			TextWriter textWriter = new StreamWriter(app_data_path + "\\xml_error.log", append: true);
			textWriter.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " " + ex.Message + "\n\n" + ex.StackTrace + "\n");
			textWriter.Close();
			MessageBox.Show("Error reading Skin file.\n\n" + ex.Message + "\n\n" + ex.StackTrace, "Skin file error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return false;
		}
		xmlDocument.GetElementsByTagName("Form");
		RestoreForm(f, xmlDocument);
		foreach (Control control2 in f.Controls)
		{
			Restore(control2, xmlDocument);
		}
		return true;
	}

	private static void Save(Control c, XmlTextWriter writer)
	{
		if (c is Button)
		{
			SaveButton((Button)c, writer);
		}
		else if (c is CheckBox)
		{
			SaveCheckBox((CheckBox)c, writer);
		}
		else if (c is ComboBox)
		{
			SaveComboBox((ComboBox)c, writer);
		}
		else if (c is Label)
		{
			SaveLabel((Label)c, writer);
		}
		else if (c is NumericUpDown)
		{
			SaveNumericUpDown((NumericUpDown)c, writer);
		}
		else if (c is PrettyTrackBar)
		{
			SavePrettyTrackBar((PrettyTrackBar)c, writer);
		}
		else if (c is PictureBox)
		{
			SavePictureBox((PictureBox)c, writer);
		}
		else if (c is RadioButton)
		{
			SaveRadioButton((RadioButton)c, writer);
		}
		else if (c is TextBox)
		{
			SaveTextBox((TextBox)c, writer);
		}
		else if (c is GroupBox)
		{
			GroupBox obj = (GroupBox)c;
			writer.WriteStartElement(c.Name);
			SaveGroupBox(obj, writer);
			writer.WriteStartElement("Controls");
			foreach (Control control in obj.Controls)
			{
				Save(control, writer);
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		else
		{
			if (!(c is Panel))
			{
				return;
			}
			Panel obj2 = (Panel)c;
			writer.WriteStartElement(c.Name);
			SavePanel(obj2, writer);
			writer.WriteStartElement("Controls");
			foreach (Control control2 in obj2.Controls)
			{
				Save(control2, writer);
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}

	private static void Restore(Control c, XmlDocument doc)
	{
		if (c is GroupBox)
		{
			GroupBox obj = (GroupBox)c;
			RestoreGroupBox(obj, doc);
			{
				foreach (Control control in obj.Controls)
				{
					Restore(control, doc);
				}
				return;
			}
		}
		if (c is Panel)
		{
			Panel obj2 = (Panel)c;
			RestorePanel(obj2, doc);
			{
				foreach (Control control2 in obj2.Controls)
				{
					Restore(control2, doc);
				}
				return;
			}
		}
		if (c is Button)
		{
			RestoreButton((Button)c, doc);
		}
		else if (c is CheckBox)
		{
			RestoreCheckBox((CheckBox)c, doc);
		}
		else if (c is ComboBox)
		{
			RestoreComboBox((ComboBox)c, doc);
		}
		else if (c is Label)
		{
			RestoreLabel((Label)c, doc);
		}
		else if (c is NumericUpDown)
		{
			RestoreNumericUpDown((NumericUpDown)c, doc);
		}
		else if (c is PrettyTrackBar)
		{
			RestorePrettyTrackBar((PrettyTrackBar)c, doc);
		}
		else if (c is PictureBox)
		{
			RestorePictureBox((PictureBox)c, doc);
		}
		else if (c is RadioButton)
		{
			RestoreRadioButton((RadioButton)c, doc);
		}
		else if (c is TextBox)
		{
			RestoreTextBox((TextBox)c, doc);
		}
	}

	private static void ReadImages(Control c)
	{
		if (c is GroupBox)
		{
			GroupBox obj = (GroupBox)c;
			SetBackgroundImage(c);
			{
				foreach (Control control in obj.Controls)
				{
					ReadImages(control);
				}
				return;
			}
		}
		if (c is Panel)
		{
			Panel obj2 = (Panel)c;
			SetBackgroundImage(c);
			{
				foreach (Control control2 in obj2.Controls)
				{
					ReadImages(control2);
				}
				return;
			}
		}
		if (c is Button)
		{
			SetupButtonImages((Button)c);
		}
		else if (c is CheckBox)
		{
			if (((CheckBox)c).Appearance == Appearance.Button)
			{
				SetupCheckBoxImages((CheckBox)c);
			}
		}
		else if (c is Label)
		{
			SetBackgroundImage(c);
		}
		else if (c is PrettyTrackBar)
		{
			SetupPrettyTrackBarImages((PrettyTrackBar)c);
		}
		else if (c is PictureBox)
		{
			SetBackgroundImage((PictureBox)c);
		}
		else if (c is RadioButton)
		{
			if (((RadioButton)c).Appearance == Appearance.Button)
			{
				SetupRadioButtonImages((RadioButton)c);
			}
		}
		else if (c is ucQuickRecall)
		{
			SetupQuickRecallImages((ucQuickRecall)c);
		}
		else if (c is ucInfoBar)
		{
			SetupInfoBar((ucInfoBar)c);
		}
	}

	private static void SaveForm(Form ctrl, XmlTextWriter writer)
	{
		writer.WriteElementString("Name", ctrl.Name);
		writer.WriteElementString("BackColor", ctrl.BackColor.Name);
		writer.WriteElementString("BackgroundImageLayout", ctrl.BackgroundImageLayout.ToString());
		SaveFont(ctrl.Font, writer);
		writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
		SaveSize(ctrl.Size, writer);
		writer.WriteElementString("Text", ctrl.Text);
		writer.WriteElementString("TransparencyKey", ctrl.TransparencyKey.Name);
	}

	private static void RestoreForm(Form ctrl, XmlDocument doc)
	{
		XmlNodeList elementsByTagName = doc.GetElementsByTagName("Form");
		if (elementsByTagName.Count == 0)
		{
			return;
		}
		foreach (XmlNode childNode in elementsByTagName[0].ChildNodes)
		{
			switch (childNode.LocalName)
			{
			case "BackColor":
				ctrl.BackColor = StringToColor(childNode.InnerText);
				break;
			case "BackgroundImageLayout":
				ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), childNode.InnerText);
				break;
			case "Font":
				ctrl.Font = RestoreFont(childNode);
				break;
			case "ForeColor":
				ctrl.ForeColor = StringToColor(childNode.InnerText);
				break;
			case "Size":
				ctrl.Size = RestoreSize(childNode);
				break;
			case "Text":
				ctrl.Text = childNode.InnerText;
				break;
			}
		}
	}

	private static void SaveGroupBox(GroupBox ctrl, XmlTextWriter writer)
	{
		writer.WriteElementString("Type", "GroupBox");
		writer.WriteElementString("BackColor", ctrl.BackColor.Name);
		writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
		SaveFont(ctrl.Font, writer);
		writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
		SaveLocation(ctrl.Location, writer);
		SaveSize(ctrl.Size, writer);
		writer.WriteElementString("Text", ctrl.Text);
	}

	private static void RestoreGroupBox(GroupBox ctrl, XmlDocument doc)
	{
		XmlNodeList elementsByTagName = doc.GetElementsByTagName(ctrl.Name);
		if (elementsByTagName.Count == 0)
		{
			return;
		}
		foreach (XmlNode childNode in elementsByTagName[0].ChildNodes)
		{
			switch (childNode.LocalName)
			{
			case "BackColor":
				ctrl.BackColor = StringToColor(childNode.InnerText);
				break;
			case "BackgroundImageLayout":
				ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), childNode.InnerText);
				break;
			case "Font":
				ctrl.Font = RestoreFont(childNode);
				break;
			case "ForeColor":
				ctrl.ForeColor = StringToColor(childNode.InnerText);
				break;
			case "Location":
				ctrl.Location = RestoreLocation(childNode);
				break;
			case "Size":
				ctrl.Size = RestoreSize(childNode);
				break;
			case "Text":
				ctrl.Text = childNode.InnerText;
				break;
			}
		}
	}

	private static void SavePanel(Panel ctrl, XmlTextWriter writer)
	{
		writer.WriteElementString("Type", "Panel");
		writer.WriteElementString("BackColor", ctrl.BackColor.Name);
		writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
		writer.WriteElementString("BorderStyle", ctrl.BorderStyle.ToString());
		SaveLocation(ctrl.Location, writer);
		SaveSize(ctrl.Size, writer);
	}

	private static void RestorePanel(Panel ctrl, XmlDocument doc)
	{
		XmlNodeList elementsByTagName = doc.GetElementsByTagName(ctrl.Name);
		if (elementsByTagName.Count == 0)
		{
			return;
		}
		foreach (XmlNode childNode in elementsByTagName[0].ChildNodes)
		{
			switch (childNode.LocalName)
			{
			case "BackColor":
				ctrl.BackColor = StringToColor(childNode.InnerText);
				break;
			case "BackgroundImageLayout":
				ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), childNode.InnerText);
				break;
			case "Location":
				ctrl.Location = RestoreLocation(childNode);
				break;
			case "Size":
				ctrl.Size = RestoreSize(childNode);
				break;
			case "Text":
				ctrl.Text = childNode.InnerText;
				break;
			}
		}
	}

	private static void SaveButton(Button ctrl, XmlTextWriter writer)
	{
		writer.WriteStartElement(ctrl.Name);
		writer.WriteElementString("Type", "Button");
		writer.WriteElementString("BackColor", ctrl.BackColor.Name);
		writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
		SaveFlatAppearance(ctrl.FlatAppearance, writer);
		writer.WriteElementString("FlatStyle", ctrl.FlatStyle.ToString());
		SaveFont(ctrl.Font, writer);
		writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
		SaveLocation(ctrl.Location, writer);
		SaveSize(ctrl.Size, writer);
		writer.WriteElementString("Text", ctrl.Text);
		writer.WriteElementString("UseVisualStyleBackColor", ctrl.UseVisualStyleBackColor.ToString());
		writer.WriteEndElement();
	}

	private static void RestoreButton(Button ctrl, XmlDocument doc)
	{
		XmlNodeList elementsByTagName = doc.GetElementsByTagName(ctrl.Name);
		if (elementsByTagName.Count == 0)
		{
			return;
		}
		foreach (XmlNode childNode in elementsByTagName[0].ChildNodes)
		{
			switch (childNode.LocalName)
			{
			case "BackColor":
				ctrl.BackColor = StringToColor(childNode.InnerText);
				break;
			case "BackgroundImageLayout":
				ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), childNode.InnerText);
				break;
			case "FlatAppearance":
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					string localName = childNode2.LocalName;
					if (!(localName == "BorderColor"))
					{
						if (localName == "BorderSize")
						{
							ctrl.FlatAppearance.BorderSize = int.Parse(childNode2.InnerText);
						}
					}
					else
					{
						ctrl.FlatAppearance.BorderColor = StringToColor(childNode2.InnerText);
					}
				}
				break;
			case "FlatStyle":
				ctrl.FlatStyle = (FlatStyle)Enum.Parse(typeof(FlatStyle), childNode.InnerText);
				break;
			case "Font":
				ctrl.Font = RestoreFont(childNode);
				break;
			case "ForeColor":
				ctrl.ForeColor = StringToColor(childNode.InnerText);
				break;
			case "Location":
				ctrl.Location = RestoreLocation(childNode);
				break;
			case "Size":
				ctrl.Size = RestoreSize(childNode);
				break;
			case "Text":
				ctrl.Text = childNode.InnerText;
				break;
			case "UseVisualStyleBackColor":
				ctrl.UseVisualStyleBackColor = bool.Parse(childNode.InnerText);
				break;
			}
		}
	}

	private static void SetupQuickRecallImages(ucQuickRecall ctrl)
	{
		for (int i = 0; i < 3; i++)
		{
			string text;
			Button button;
			switch (i)
			{
			case 0:
				text = "_previous";
				button = ctrl.PreviousButton;
				break;
			case 1:
				text = "_list";
				button = ctrl.ListButton;
				break;
			case 2:
				text = "_next";
				button = ctrl.NextButton;
				break;
			default:
				text = "_previous";
				button = ctrl.PreviousButton;
				break;
			}
			if (button.ImageList == null)
			{
				button.ImageList = new ImageList();
			}
			else
			{
				button.ImageList.Images.Clear();
			}
			button.ImageList.ImageSize = button.Size;
			button.ImageList.ColorDepth = ColorDepth.Depth32Bit;
			for (int j = 0; j < 8; j++)
			{
				if (File.Exists(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + text + "-" + j + ".png"))
				{
					Image image = loadImage(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + text + "-" + j + ".png");
					if (image != null)
					{
						ImageList.ImageCollection images = button.ImageList.Images;
						ImageState imageState = (ImageState)j;
						images.Add(imageState.ToString(), image);
					}
				}
				else if (File.Exists(path + "\\Console\\" + ctrl.Name + text + "-" + j + ".png"))
				{
					Image image2 = loadImage(path + "\\Console\\" + ctrl.Name + text + "-" + j + ".png");
					if (image2 != null)
					{
						ImageList.ImageCollection images2 = button.ImageList.Images;
						ImageState imageState = (ImageState)j;
						images2.Add(imageState.ToString(), image2);
					}
				}
			}
			if (button.ImageList.Images.Count > 0)
			{
				button.Text = "";
			}
			setupButtonHandlers(button);
			Button_StateChanged(button, EventArgs.Empty);
		}
	}

	private static void setupButtonHandlers(Button ctrl)
	{
		EventHandler value = Button_StateChanged;
		ctrl.Click -= value;
		ctrl.Click += value;
		ctrl.EnabledChanged -= value;
		ctrl.EnabledChanged += value;
		ctrl.MouseEnter -= Button_MouseEnter;
		ctrl.MouseEnter += Button_MouseEnter;
		ctrl.MouseLeave -= value;
		ctrl.MouseLeave += value;
		ctrl.MouseDown -= Button_MouseDown;
		ctrl.MouseDown += Button_MouseDown;
		ctrl.MouseUp -= Button_MouseUp;
		ctrl.MouseUp += Button_MouseUp;
		ctrl.GotFocus -= value;
		ctrl.GotFocus += value;
		ctrl.LostFocus -= value;
		ctrl.LostFocus += value;
		ctrl.BackgroundImage = null;
	}

	private static void SetupButtonImages(Button ctrl)
	{
		string text = "";
		for (int i = 0; i < 8; i++)
		{
			string key = path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + i + ".png";
			if (!File.Exists(key))
			{
				key = path + "\\Console\\" + ctrl.Name + "-" + i + ".png";
			}
			if (File.Exists(key))
			{
				loadImage(key);
				if (_image_cache_map.ContainsKey(key))
				{
					text += _image_cache_map[key];
				}
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = text + "_" + ctrl.Size.Width + "_" + ctrl.Size.Height;
			if (!_shared_image_lists.ContainsKey(text))
			{
				_shared_image_lists.Add(text, new ImageList());
				_shared_image_lists[text].ImageSize = ctrl.Size;
				_shared_image_lists[text].ColorDepth = ColorDepth.Depth32Bit;
			}
			ctrl.ImageList = _shared_image_lists[text];
			for (int j = 0; j < 8; j++)
			{
				ImageState imageState = (ImageState)j;
				string key2 = imageState.ToString();
				Image imageFromFilePath = getImageFromFilePath(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + j + ".png");
				if (imageFromFilePath == null)
				{
					imageFromFilePath = getImageFromFilePath(path + "\\Console\\" + ctrl.Name + "-" + j + ".png");
				}
				if (imageFromFilePath != null && !_shared_image_lists[text].Images.ContainsKey(key2))
				{
					imageFromFilePath = resizeImage(imageFromFilePath, ctrl);
					_shared_image_lists[text].Images.Add(key2, imageFromFilePath);
				}
			}
		}
		if (ctrl.ImageList == null)
		{
			ctrl.ImageList = new ImageList();
		}
		setupButtonHandlers(ctrl);
		Button_StateChanged(ctrl, EventArgs.Empty);
	}

	private static void Button_StateChanged(object sender, EventArgs e)
	{
		Button button = (Button)sender;
		ImageState imageState = ImageState.NormalUp;
		imageState = ((!button.Enabled && button.ImageList.Images.IndexOfKey(ImageState.DisabledUp.ToString()) >= 0) ? ImageState.DisabledUp : ((button.Focused && button.ImageList.Images.IndexOfKey(ImageState.FocusedUp.ToString()) >= 0) ? ImageState.FocusedUp : ImageState.NormalUp));
		SetButtonImageState(button, imageState);
	}

	private static void Button_MouseEnter(object sender, EventArgs e)
	{
		Button button = (Button)sender;
		if (button.Enabled)
		{
			ImageState state = ImageState.MouseOverUp;
			SetButtonImageState(button, state);
		}
	}

	private static void Button_MouseDown(object sender, MouseEventArgs e)
	{
		Button button = (Button)sender;
		if (button.Enabled)
		{
			ImageState state = ImageState.NormalDown;
			SetButtonImageState(button, state);
		}
	}

	private static void Button_MouseUp(object sender, MouseEventArgs e)
	{
		Button_StateChanged(sender, EventArgs.Empty);
	}

	private static void SetButtonImageState(Button ctrl, ImageState state)
	{
		if (ctrl.ImageList != null)
		{
			int num = ctrl.ImageList.Images.IndexOfKey(state.ToString());
			if (num >= 0)
			{
				ctrl.BackgroundImage = ctrl.ImageList.Images[num];
			}
		}
	}

	private static void SaveCheckBox(CheckBox ctrl, XmlTextWriter writer)
	{
		writer.WriteStartElement(ctrl.Name);
		writer.WriteElementString("Type", "CheckBox");
		writer.WriteElementString("Appearance", ctrl.Appearance.ToString());
		writer.WriteElementString("AutoSize", ctrl.AutoSize.ToString());
		writer.WriteElementString("BackColor", ctrl.BackColor.Name);
		writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
		SaveFlatAppearance(ctrl.FlatAppearance, writer);
		writer.WriteElementString("FlatStyle", ctrl.FlatStyle.ToString());
		SaveFont(ctrl.Font, writer);
		writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
		SaveLocation(ctrl.Location, writer);
		SaveSize(ctrl.Size, writer);
		writer.WriteElementString("Text", ctrl.Text);
		writer.WriteElementString("UseVisualStyleBackColor", ctrl.UseVisualStyleBackColor.ToString());
		writer.WriteEndElement();
	}

	private static void RestoreCheckBox(CheckBox ctrl, XmlDocument doc)
	{
		XmlNodeList elementsByTagName = doc.GetElementsByTagName(ctrl.Name);
		if (elementsByTagName.Count == 0)
		{
			return;
		}
		foreach (XmlNode childNode in elementsByTagName[0].ChildNodes)
		{
			switch (childNode.LocalName)
			{
			case "Appearance":
				ctrl.Appearance = (Appearance)Enum.Parse(typeof(Appearance), childNode.InnerText);
				break;
			case "AutoSize":
				ctrl.AutoSize = bool.Parse(childNode.InnerText);
				break;
			case "BackColor":
				ctrl.BackColor = StringToColor(childNode.InnerText);
				break;
			case "BackgroundImageLayout":
				ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), childNode.InnerText);
				break;
			case "FlatAppearance":
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					string localName = childNode2.LocalName;
					if (!(localName == "BorderColor"))
					{
						if (localName == "BorderSize")
						{
							ctrl.FlatAppearance.BorderSize = int.Parse(childNode2.InnerText);
						}
					}
					else
					{
						ctrl.FlatAppearance.BorderColor = StringToColor(childNode2.InnerText);
					}
				}
				break;
			case "FlatStyle":
				ctrl.FlatStyle = (FlatStyle)Enum.Parse(typeof(FlatStyle), childNode.InnerText);
				break;
			case "Font":
				ctrl.Font = RestoreFont(childNode);
				break;
			case "ForeColor":
				ctrl.ForeColor = StringToColor(childNode.InnerText);
				break;
			case "Location":
				ctrl.Location = RestoreLocation(childNode);
				break;
			case "Size":
				ctrl.Size = RestoreSize(childNode);
				break;
			case "Text":
				ctrl.Text = childNode.InnerText;
				break;
			case "UseVisualStyleBackColor":
				ctrl.UseVisualStyleBackColor = bool.Parse(childNode.InnerText);
				break;
			}
		}
	}

	private static string getCheckBoxImagePath(CheckBox ctrl, int stateIndex)
	{
		string result = path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + stateIndex + ".png";
		if (File.Exists(result))
		{
			return result;
		}
		result = path + "\\Console\\" + ctrl.Name + "-" + stateIndex + ".png";
		if (File.Exists(result))
		{
			return result;
		}
		if (ctrl.Name == "btnAPF_type")
		{
			if (!string.IsNullOrEmpty(apf_skin_path))
			{
				result = apf_skin_path + "\\" + ctrl.Name + "-" + stateIndex + ".png";
				if (File.Exists(result))
				{
					return result;
				}
			}
			string text = Application.StartupPath + "\\SkinsAPF";
			if (text != apf_skin_path)
			{
				result = text + "\\" + ctrl.Name + "-" + stateIndex + ".png";
				if (File.Exists(result))
				{
					return result;
				}
			}
		}
		return null;
	}

	private static void GenerateAPFButtonImage(string folder, int stateIndex, Color backColor, Color borderColor)
	{
		int num = 36;
		int num2 = 20;
		using Bitmap bitmap = new Bitmap(num, num2);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear(Color.Transparent);
		using (Brush brush = new SolidBrush(backColor))
		{
			graphics.FillRectangle(brush, 0, 0, num - 1, num2 - 1);
		}
		using (Pen pen = new Pen(borderColor, 1f))
		{
			graphics.DrawRectangle(pen, 0, 0, num - 1, num2 - 1);
		}
		bitmap.Save(folder + "\\btnAPF_type-" + stateIndex + ".png", ImageFormat.Png);
	}

	private static void EnsureAPFSkinImages()
	{
		if (string.IsNullOrEmpty(apf_skin_path))
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < 8; i++)
		{
			if (!File.Exists(apf_skin_path + "\\btnAPF_type-" + i + ".png"))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return;
		}
		try
		{
			if (!Directory.Exists(apf_skin_path))
			{
				Directory.CreateDirectory(apf_skin_path);
			}
			GenerateAPFButtonImage(apf_skin_path, 0, Color.FromArgb(40, 40, 40), Color.FromArgb(80, 80, 80));
			GenerateAPFButtonImage(apf_skin_path, 1, Color.FromArgb(30, 30, 30), Color.FromArgb(100, 100, 100));
			GenerateAPFButtonImage(apf_skin_path, 2, Color.FromArgb(50, 50, 50), Color.FromArgb(70, 70, 70));
			GenerateAPFButtonImage(apf_skin_path, 3, Color.FromArgb(40, 40, 40), Color.FromArgb(80, 80, 80));
			GenerateAPFButtonImage(apf_skin_path, 4, Color.FromArgb(45, 45, 45), Color.FromArgb(90, 90, 90));
			GenerateAPFButtonImage(apf_skin_path, 5, Color.FromArgb(35, 35, 35), Color.FromArgb(100, 100, 100));
			GenerateAPFButtonImage(apf_skin_path, 6, Color.FromArgb(55, 55, 55), Color.FromArgb(100, 100, 100));
			GenerateAPFButtonImage(apf_skin_path, 7, Color.FromArgb(35, 35, 35), Color.FromArgb(100, 100, 100));
		}
		catch
		{
		}
	}

	private static void SetupCheckBoxImages(CheckBox ctrl)
	{
		if (ctrl.Name == "btnAPF_type")
		{
			EnsureAPFSkinImages();
		}
		string text = "";
		for (int i = 0; i < 8; i++)
		{
			string checkBoxImagePath = getCheckBoxImagePath(ctrl, i);
			if (!string.IsNullOrEmpty(checkBoxImagePath))
			{
				loadImage(checkBoxImagePath);
				if (_image_cache_map.ContainsKey(checkBoxImagePath))
				{
					text += _image_cache_map[checkBoxImagePath];
				}
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = text + "_" + ctrl.Size.Width + "_" + ctrl.Size.Height;
			if (!_shared_image_lists.ContainsKey(text))
			{
				_shared_image_lists.Add(text, new ImageList());
				_shared_image_lists[text].ImageSize = ctrl.Size;
				_shared_image_lists[text].ColorDepth = ColorDepth.Depth32Bit;
			}
			ctrl.ImageList = _shared_image_lists[text];
			for (int j = 0; j < 8; j++)
			{
				ImageState imageState = (ImageState)j;
				string key = imageState.ToString();
				string checkBoxImagePath2 = getCheckBoxImagePath(ctrl, j);
				Image image = null;
				if (!string.IsNullOrEmpty(checkBoxImagePath2))
				{
					image = getImageFromFilePath(checkBoxImagePath2);
				}
				if (image != null && !_shared_image_lists[text].Images.ContainsKey(key))
				{
					image = resizeImage(image, ctrl);
					_shared_image_lists[text].Images.Add(key, image);
				}
			}
		}
		if (ctrl.ImageList == null)
		{
			ctrl.ImageList = new ImageList();
		}
		if (ctrl.ImageList.Images.Count > 0)
		{
			ctrl.BackColor = Color.Transparent;
			ctrl.FlatAppearance.CheckedBackColor = Color.Transparent;
			ctrl.FlatAppearance.MouseOverBackColor = Color.Transparent;
			ctrl.FlatAppearance.MouseDownBackColor = Color.Transparent;
		}
		setupCheckBoxHandlers(ctrl);
		CheckBox_StateChanged(ctrl, EventArgs.Empty);
	}

	private static void setupCheckBoxHandlers(CheckBox ctrl)
	{
		EventHandler value = CheckBox_StateChanged;
		ctrl.CheckedChanged -= value;
		ctrl.CheckedChanged += value;
		ctrl.EnabledChanged -= value;
		ctrl.EnabledChanged += value;
		ctrl.MouseEnter -= CheckBox_MouseEnter;
		ctrl.MouseEnter += CheckBox_MouseEnter;
		ctrl.MouseLeave -= value;
		ctrl.MouseLeave += value;
		ctrl.GotFocus -= value;
		ctrl.GotFocus += value;
		ctrl.LostFocus -= value;
		ctrl.LostFocus += value;
		ctrl.BackgroundImage = null;
	}

	private static void SetupInfoBar(ucInfoBar ctrl)
	{
		for (int i = 0; i < 2; i++)
		{
			string text = "_button" + (i + 1);
			CheckBox checkBox = ((i != 0) ? ctrl.Button2 : ctrl.Button1);
			if (checkBox.ImageList == null)
			{
				checkBox.ImageList = new ImageList();
			}
			else
			{
				checkBox.ImageList.Images.Clear();
			}
			checkBox.ImageList.ImageSize = checkBox.Size;
			checkBox.ImageList.ColorDepth = ColorDepth.Depth32Bit;
			for (int j = 0; j < 8; j++)
			{
				if (File.Exists(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + text + "-" + j + ".png"))
				{
					Image image = loadImage(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + text + "-" + j + ".png");
					if (image != null)
					{
						ImageList.ImageCollection images = checkBox.ImageList.Images;
						ImageState imageState = (ImageState)j;
						images.Add(imageState.ToString(), image);
					}
				}
				else if (File.Exists(path + "\\Console\\" + ctrl.Name + text + "-" + j + ".png"))
				{
					Image image2 = loadImage(path + "\\Console\\" + ctrl.Name + text + "-" + j + ".png");
					if (image2 != null)
					{
						ImageList.ImageCollection images2 = checkBox.ImageList.Images;
						ImageState imageState = (ImageState)j;
						images2.Add(imageState.ToString(), image2);
					}
				}
			}
			if (checkBox.ImageList.Images.Count > 0)
			{
				checkBox.BackColor = Color.Transparent;
				checkBox.FlatAppearance.CheckedBackColor = Color.Transparent;
				checkBox.FlatAppearance.MouseOverBackColor = Color.Transparent;
				checkBox.FlatAppearance.MouseDownBackColor = Color.Transparent;
			}
			else
			{
				checkBox.BackColor = Color.Transparent;
				checkBox.FlatAppearance.CheckedBackColor = Color.Silver;
				checkBox.FlatAppearance.MouseOverBackColor = Color.Gray;
				checkBox.FlatAppearance.MouseDownBackColor = Color.Gray;
			}
			setupCheckBoxHandlers(checkBox);
			CheckBox_StateChanged(checkBox, EventArgs.Empty);
			for (int k = 0; k < 8; k++)
			{
				CheckBox popupButton = ctrl.GetPopupButton(i + 1, k);
				if (popupButton == null)
				{
					continue;
				}
				if (popupButton.ImageList == null)
				{
					popupButton.ImageList = new ImageList();
				}
				else
				{
					popupButton.ImageList.Images.Clear();
				}
				popupButton.ImageList.ImageSize = popupButton.Size;
				popupButton.ImageList.ColorDepth = ColorDepth.Depth32Bit;
				if (checkBox.ImageList.Images.Count > 0)
				{
					for (int l = 0; l < checkBox.ImageList.Images.Count; l++)
					{
						popupButton.ImageList.Images.Add(checkBox.ImageList.Images.Keys[l], checkBox.ImageList.Images[l]);
					}
					popupButton.BackColor = Color.Transparent;
					popupButton.FlatAppearance.CheckedBackColor = Color.Transparent;
					popupButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
					popupButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
				}
				else
				{
					popupButton.BackColor = Color.Transparent;
					popupButton.FlatAppearance.CheckedBackColor = Color.Silver;
					popupButton.FlatAppearance.MouseOverBackColor = Color.Gray;
					popupButton.FlatAppearance.MouseDownBackColor = Color.Gray;
				}
				setupCheckBoxHandlers(popupButton);
				CheckBox_StateChanged(popupButton, EventArgs.Empty);
			}
		}
	}

	private static void CheckBox_StateChanged(object sender, EventArgs e)
	{
		CheckBox checkBox = (CheckBox)sender;
		ImageState imageState = ImageState.NormalUp;
		imageState = ((!checkBox.Enabled && checkBox.ImageList.Images.IndexOfKey(ImageState.DisabledDown.ToString()) >= 0 && checkBox.ImageList.Images.IndexOfKey(ImageState.DisabledUp.ToString()) >= 0) ? (checkBox.Checked ? ImageState.DisabledDown : ImageState.DisabledUp) : ((!checkBox.Focused || checkBox.ImageList.Images.IndexOfKey(ImageState.FocusedDown.ToString()) < 0 || checkBox.ImageList.Images.IndexOfKey(ImageState.FocusedUp.ToString()) < 0) ? (checkBox.Checked ? ImageState.NormalDown : ImageState.NormalUp) : (checkBox.Checked ? ImageState.FocusedDown : ImageState.FocusedUp)));
		SetCheckBoxImageState(checkBox, imageState);
	}

	private static void CheckBox_MouseEnter(object sender, EventArgs e)
	{
		CheckBox checkBox = (CheckBox)sender;
		if (checkBox.Enabled)
		{
			ImageState state = ImageState.MouseOverUp;
			if (checkBox.Checked)
			{
				state = ImageState.MouseOverDown;
			}
			SetCheckBoxImageState(checkBox, state);
		}
	}

	private static void SetCheckBoxImageState(CheckBox ctrl, ImageState state)
	{
		if (ctrl.ImageList != null)
		{
			int num = ctrl.ImageList.Images.IndexOfKey(state.ToString());
			if (num >= 0)
			{
				ctrl.BackgroundImage = ctrl.ImageList.Images[num];
			}
		}
	}

	private static void SaveComboBox(ComboBox ctrl, XmlTextWriter writer)
	{
		writer.WriteStartElement(ctrl.Name);
		writer.WriteElementString("Type", "ComboBox");
		writer.WriteElementString("BackColor", ctrl.BackColor.Name);
		writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
		writer.WriteElementString("FlatStyle", ctrl.FlatStyle.ToString());
		SaveFont(ctrl.Font, writer);
		writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
		SaveLocation(ctrl.Location, writer);
		SaveSize(ctrl.Size, writer);
		writer.WriteEndElement();
	}

	private static void RestoreComboBox(ComboBox ctrl, XmlDocument doc)
	{
		XmlNodeList elementsByTagName = doc.GetElementsByTagName(ctrl.Name);
		if (elementsByTagName.Count == 0)
		{
			return;
		}
		foreach (XmlNode childNode in elementsByTagName[0].ChildNodes)
		{
			switch (childNode.LocalName)
			{
			case "BackColor":
				ctrl.BackColor = StringToColor(childNode.InnerText);
				break;
			case "BackgroundImageLayout":
				ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), childNode.InnerText);
				break;
			case "FlatStyle":
				ctrl.FlatStyle = (FlatStyle)Enum.Parse(typeof(FlatStyle), childNode.InnerText);
				break;
			case "Font":
				ctrl.Font = RestoreFont(childNode);
				break;
			case "ForeColor":
				ctrl.ForeColor = StringToColor(childNode.InnerText);
				break;
			case "Location":
				ctrl.Location = RestoreLocation(childNode);
				break;
			case "Size":
				ctrl.Size = RestoreSize(childNode);
				break;
			}
		}
	}

	private static void SaveLabel(Label ctrl, XmlTextWriter writer)
	{
		writer.WriteStartElement(ctrl.Name);
		writer.WriteElementString("Type", "Label");
		writer.WriteElementString("AutoSize", ctrl.AutoSize.ToString());
		writer.WriteElementString("BackColor", ctrl.BackColor.Name);
		writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
		SaveFont(ctrl.Font, writer);
		writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
		SaveLocation(ctrl.Location, writer);
		SaveSize(ctrl.Size, writer);
		writer.WriteElementString("Text", ctrl.Text);
		writer.WriteEndElement();
	}

	private static void RestoreLabel(Label ctrl, XmlDocument doc)
	{
		XmlNodeList elementsByTagName = doc.GetElementsByTagName(ctrl.Name);
		if (elementsByTagName.Count == 0)
		{
			return;
		}
		foreach (XmlNode childNode in elementsByTagName[0].ChildNodes)
		{
			switch (childNode.LocalName)
			{
			case "AutoSize":
				ctrl.AutoSize = bool.Parse(childNode.InnerText);
				break;
			case "BackColor":
				ctrl.BackColor = StringToColor(childNode.InnerText);
				break;
			case "BackgroundImageLayout":
				ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), childNode.InnerText);
				break;
			case "Font":
				ctrl.Font = RestoreFont(childNode);
				break;
			case "ForeColor":
				ctrl.ForeColor = StringToColor(childNode.InnerText);
				break;
			case "Location":
				ctrl.Location = RestoreLocation(childNode);
				break;
			case "Size":
				ctrl.Size = RestoreSize(childNode);
				break;
			case "Text":
				ctrl.Text = childNode.InnerText;
				break;
			}
		}
	}

	private static void SaveNumericUpDown(NumericUpDown ctrl, XmlTextWriter writer)
	{
		writer.WriteStartElement(ctrl.Name);
		writer.WriteElementString("Type", "NumericUpDown");
		writer.WriteElementString("AutoSize", ctrl.AutoSize.ToString());
		writer.WriteElementString("BackColor", ctrl.BackColor.Name);
		writer.WriteElementString("BorderStyle", ctrl.BorderStyle.ToString());
		SaveFont(ctrl.Font, writer);
		writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
		SaveLocation(ctrl.Location, writer);
		SaveSize(ctrl.Size, writer);
		writer.WriteEndElement();
	}

	private static void RestoreNumericUpDown(NumericUpDown ctrl, XmlDocument doc)
	{
		XmlNodeList elementsByTagName = doc.GetElementsByTagName(ctrl.Name);
		if (elementsByTagName.Count == 0)
		{
			return;
		}
		foreach (XmlNode childNode in elementsByTagName[0].ChildNodes)
		{
			switch (childNode.LocalName)
			{
			case "AutoSize":
				ctrl.AutoSize = bool.Parse(childNode.InnerText);
				break;
			case "BackColor":
				ctrl.BackColor = StringToColor(childNode.InnerText);
				break;
			case "BorderStyle":
				ctrl.BorderStyle = (BorderStyle)Enum.Parse(typeof(BorderStyle), childNode.InnerText);
				break;
			case "Font":
				ctrl.Font = RestoreFont(childNode);
				break;
			case "ForeColor":
				ctrl.ForeColor = StringToColor(childNode.InnerText);
				break;
			case "Location":
				ctrl.Location = RestoreLocation(childNode);
				break;
			case "Size":
				ctrl.Size = RestoreSize(childNode);
				break;
			}
		}
	}

	private static void SavePictureBox(PictureBox ctrl, XmlTextWriter writer)
	{
		writer.WriteStartElement(ctrl.Name);
		writer.WriteElementString("Type", "PictureBox");
		writer.WriteElementString("BackColor", ctrl.BackColor.Name);
		writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
		writer.WriteElementString("BorderStyle", ctrl.BorderStyle.ToString());
		SaveLocation(ctrl.Location, writer);
		SaveSize(ctrl.Size, writer);
		writer.WriteEndElement();
	}

	private static void RestorePictureBox(PictureBox ctrl, XmlDocument doc)
	{
		XmlNodeList elementsByTagName = doc.GetElementsByTagName(ctrl.Name);
		if (elementsByTagName.Count == 0)
		{
			return;
		}
		foreach (XmlNode childNode in elementsByTagName[0].ChildNodes)
		{
			switch (childNode.LocalName)
			{
			case "BackColor":
				ctrl.BackColor = StringToColor(childNode.InnerText);
				break;
			case "BackgroundImageLayout":
				ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), childNode.InnerText);
				break;
			case "Location":
				ctrl.Location = RestoreLocation(childNode);
				break;
			case "Size":
				ctrl.Size = RestoreSize(childNode);
				break;
			case "Text":
				ctrl.Text = childNode.InnerText;
				break;
			}
		}
	}

	private static void SaveRadioButton(RadioButton ctrl, XmlTextWriter writer)
	{
		writer.WriteStartElement(ctrl.Name);
		writer.WriteElementString("Type", "RadioButton");
		writer.WriteElementString("Appearance", ctrl.Appearance.ToString());
		writer.WriteElementString("AutoSize", ctrl.AutoSize.ToString());
		writer.WriteElementString("BackColor", ctrl.BackColor.Name);
		writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
		SaveFlatAppearance(ctrl.FlatAppearance, writer);
		writer.WriteElementString("FlatStyle", ctrl.FlatStyle.ToString());
		SaveFont(ctrl.Font, writer);
		writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
		SaveLocation(ctrl.Location, writer);
		SaveSize(ctrl.Size, writer);
		writer.WriteElementString("Text", ctrl.Text);
		writer.WriteElementString("UseVisualStyleBackColor", ctrl.UseVisualStyleBackColor.ToString());
		writer.WriteEndElement();
	}

	private static void RestoreRadioButton(RadioButton ctrl, XmlDocument doc)
	{
		XmlNodeList elementsByTagName = doc.GetElementsByTagName(ctrl.Name);
		if (elementsByTagName.Count == 0)
		{
			return;
		}
		foreach (XmlNode childNode in elementsByTagName[0].ChildNodes)
		{
			switch (childNode.LocalName)
			{
			case "Appearance":
				ctrl.Appearance = (Appearance)Enum.Parse(typeof(Appearance), childNode.InnerText);
				break;
			case "AutoSize":
				ctrl.AutoSize = bool.Parse(childNode.InnerText);
				break;
			case "BackColor":
				ctrl.BackColor = StringToColor(childNode.InnerText);
				break;
			case "BackgroundImageLayout":
				ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), childNode.InnerText);
				break;
			case "FlatAppearance":
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					string localName = childNode2.LocalName;
					if (!(localName == "BorderColor"))
					{
						if (localName == "BorderSize")
						{
							ctrl.FlatAppearance.BorderSize = int.Parse(childNode2.InnerText);
						}
					}
					else
					{
						ctrl.FlatAppearance.BorderColor = StringToColor(childNode2.InnerText);
					}
				}
				break;
			case "FlatStyle":
				ctrl.FlatStyle = (FlatStyle)Enum.Parse(typeof(FlatStyle), childNode.InnerText);
				break;
			case "Font":
				ctrl.Font = RestoreFont(childNode);
				break;
			case "ForeColor":
				ctrl.ForeColor = StringToColor(childNode.InnerText);
				break;
			case "Location":
				ctrl.Location = RestoreLocation(childNode);
				break;
			case "Size":
				ctrl.Size = RestoreSize(childNode);
				break;
			case "Text":
				ctrl.Text = childNode.InnerText;
				break;
			case "UseVisualStyleBackColor":
				ctrl.UseVisualStyleBackColor = bool.Parse(childNode.InnerText);
				break;
			}
		}
	}

	private static void SetupRadioButtonImages(RadioButton ctrl)
	{
		string text = "";
		for (int i = 0; i < 8; i++)
		{
			string key = path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + i + ".png";
			if (!File.Exists(key))
			{
				key = path + "\\Console\\" + ctrl.Name + "-" + i + ".png";
			}
			if (File.Exists(key))
			{
				loadImage(key);
				if (_image_cache_map.ContainsKey(key))
				{
					text += _image_cache_map[key];
				}
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = text + "_" + ctrl.Size.Width + "_" + ctrl.Size.Height;
			if (!_shared_image_lists.ContainsKey(text))
			{
				_shared_image_lists.Add(text, new ImageList());
				_shared_image_lists[text].ImageSize = ctrl.Size;
				_shared_image_lists[text].ColorDepth = ColorDepth.Depth32Bit;
			}
			ctrl.ImageList = _shared_image_lists[text];
			for (int j = 0; j < 8; j++)
			{
				ImageState imageState = (ImageState)j;
				string key2 = imageState.ToString();
				Image imageFromFilePath = getImageFromFilePath(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + j + ".png");
				if (imageFromFilePath == null)
				{
					imageFromFilePath = getImageFromFilePath(path + "\\Console\\" + ctrl.Name + "-" + j + ".png");
				}
				if (imageFromFilePath != null && !_shared_image_lists[text].Images.ContainsKey(key2))
				{
					imageFromFilePath = resizeImage(imageFromFilePath, ctrl);
					_shared_image_lists[text].Images.Add(key2, imageFromFilePath);
				}
			}
		}
		if (ctrl.ImageList == null)
		{
			ctrl.ImageList = new ImageList();
		}
		EventHandler value = RadioButton_StateChanged;
		ctrl.CheckedChanged -= value;
		ctrl.CheckedChanged += value;
		ctrl.EnabledChanged -= value;
		ctrl.EnabledChanged += value;
		ctrl.MouseEnter -= RadioButton_MouseEnter;
		ctrl.MouseEnter += RadioButton_MouseEnter;
		ctrl.GotFocus -= value;
		ctrl.GotFocus += value;
		ctrl.LostFocus -= value;
		ctrl.LostFocus += value;
		ctrl.BackgroundImage = null;
		RadioButton_StateChanged(ctrl, EventArgs.Empty);
	}

	private static void RadioButton_StateChanged(object sender, EventArgs e)
	{
		RadioButton radioButton = (RadioButton)sender;
		ImageState imageState = ImageState.NormalUp;
		imageState = ((!radioButton.Enabled && radioButton.ImageList.Images.IndexOfKey(ImageState.DisabledDown.ToString()) >= 0 && radioButton.ImageList.Images.IndexOfKey(ImageState.DisabledUp.ToString()) >= 0) ? (radioButton.Checked ? ImageState.DisabledDown : ImageState.DisabledUp) : ((!radioButton.Focused || radioButton.ImageList.Images.IndexOfKey(ImageState.FocusedDown.ToString()) < 0 || radioButton.ImageList.Images.IndexOfKey(ImageState.FocusedUp.ToString()) < 0) ? (radioButton.Checked ? ImageState.NormalDown : ImageState.NormalUp) : ((!radioButton.Checked) ? ImageState.FocusedUp : ImageState.FocusedDown)));
		SetRadioButtonImageState(radioButton, imageState);
	}

	private static void RadioButton_MouseEnter(object sender, EventArgs e)
	{
		RadioButton radioButton = (RadioButton)sender;
		if (radioButton.Enabled)
		{
			ImageState state = ImageState.MouseOverUp;
			if (radioButton.Checked)
			{
				state = ImageState.MouseOverDown;
			}
			SetRadioButtonImageState(radioButton, state);
		}
	}

	private static void SetRadioButtonImageState(RadioButton ctrl, ImageState state)
	{
		if (ctrl.ImageList != null)
		{
			int num = ctrl.ImageList.Images.IndexOfKey(state.ToString());
			if (num >= 0)
			{
				ctrl.BackgroundImage = ctrl.ImageList.Images[num];
			}
		}
	}

	private static void SaveTextBox(TextBox ctrl, XmlTextWriter writer)
	{
		writer.WriteStartElement(ctrl.Name);
		writer.WriteElementString("Type", "CheckBox");
		writer.WriteElementString("AutoSize", ctrl.AutoSize.ToString());
		writer.WriteElementString("BackColor", ctrl.BackColor.Name);
		writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
		writer.WriteElementString("BorderStyle", ctrl.BorderStyle.ToString());
		SaveFont(ctrl.Font, writer);
		writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
		SaveLocation(ctrl.Location, writer);
		SaveSize(ctrl.Size, writer);
		writer.WriteElementString("Text", ctrl.Text);
		writer.WriteEndElement();
	}

	private static void RestoreTextBox(TextBox ctrl, XmlDocument doc)
	{
		XmlNodeList elementsByTagName = doc.GetElementsByTagName(ctrl.Name);
		if (elementsByTagName.Count == 0)
		{
			return;
		}
		foreach (XmlNode childNode in elementsByTagName[0].ChildNodes)
		{
			switch (childNode.LocalName)
			{
			case "AutoSize":
				ctrl.AutoSize = bool.Parse(childNode.InnerText);
				break;
			case "BackColor":
				ctrl.BackColor = StringToColor(childNode.InnerText);
				break;
			case "BackgroundImageLayout":
				ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), childNode.InnerText);
				break;
			case "BorderStyle":
				ctrl.BorderStyle = (BorderStyle)Enum.Parse(typeof(BorderStyle), childNode.InnerText);
				break;
			case "Font":
				ctrl.Font = RestoreFont(childNode);
				break;
			case "ForeColor":
				ctrl.ForeColor = StringToColor(childNode.InnerText);
				break;
			case "Location":
				ctrl.Location = RestoreLocation(childNode);
				break;
			case "Size":
				ctrl.Size = RestoreSize(childNode);
				break;
			case "Text":
				ctrl.Text = childNode.InnerText;
				break;
			}
		}
	}

	private static void SavePrettyTrackBar(PrettyTrackBar ctrl, XmlTextWriter writer)
	{
		writer.WriteStartElement(ctrl.Name);
		writer.WriteElementString("Type", "PrettyTrackBar");
		writer.WriteElementString("BackColor", ctrl.BackColor.Name);
		writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
		SaveLocation(ctrl.Location, writer);
		SaveSize(ctrl.Size, writer);
		writer.WriteEndElement();
	}

	private static void RestorePrettyTrackBar(PrettyTrackBar ctrl, XmlDocument doc)
	{
		XmlNodeList elementsByTagName = doc.GetElementsByTagName(ctrl.Name);
		if (elementsByTagName.Count == 0)
		{
			return;
		}
		foreach (XmlNode childNode in elementsByTagName[0].ChildNodes)
		{
			switch (childNode.LocalName)
			{
			case "BackColor":
				ctrl.BackColor = StringToColor(childNode.InnerText);
				break;
			case "BackgroundImageLayout":
				ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), childNode.InnerText);
				break;
			case "Location":
				ctrl.Location = RestoreLocation(childNode);
				break;
			case "Size":
				ctrl.Size = RestoreSize(childNode);
				break;
			}
		}
	}

	private static void SetupPrettyTrackBarImages(PrettyTrackBar ctrl)
	{
		if (File.Exists(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-back.png"))
		{
			ctrl.BackgroundImage = loadImage(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-back.png");
		}
		else if (File.Exists(path + "\\Console\\" + ctrl.Name + "-back.png"))
		{
			ctrl.BackgroundImage = loadImage(path + "\\Console\\" + ctrl.Name + "-back.png");
		}
		else
		{
			ctrl.BackgroundImage = null;
		}
		if (File.Exists(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-head.png"))
		{
			ctrl.HeadImage = loadImage(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-head.png");
		}
		else if (File.Exists(path + "\\Console\\" + ctrl.Name + "-head.png"))
		{
			ctrl.HeadImage = loadImage(path + "\\Console\\" + ctrl.Name + "-head.png");
		}
		else
		{
			ctrl.HeadImage = null;
		}
		ctrl.Invalidate();
	}

	private static void SaveSize(Size s, XmlTextWriter writer)
	{
		writer.WriteStartElement("Size");
		writer.WriteElementString("Width", s.Width.ToString());
		writer.WriteElementString("Height", s.Height.ToString());
		writer.WriteEndElement();
	}

	private static Size RestoreSize(XmlNode node)
	{
		Size result = new Size(0, 0);
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string localName = childNode.LocalName;
			if (!(localName == "Width"))
			{
				if (localName == "Height")
				{
					result.Height = int.Parse(childNode.InnerText);
				}
			}
			else
			{
				result.Width = int.Parse(childNode.InnerText);
			}
		}
		return result;
	}

	private static void SaveFont(Font f, XmlTextWriter writer)
	{
		writer.WriteStartElement("Font");
		writer.WriteElementString("FontFamily", f.FontFamily.Name);
		writer.WriteElementString("Size", f.Size.ToString());
		writer.WriteElementString("Bold", f.Bold.ToString());
		writer.WriteElementString("Italic", f.Italic.ToString());
		writer.WriteElementString("Underline", f.Underline.ToString());
		writer.WriteEndElement();
	}

	private static Font RestoreFont(XmlNode node)
	{
		string familyName = "Arial";
		float emSize = 8.25f;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			switch (childNode.LocalName)
			{
			case "FontFamily":
				familyName = childNode.InnerText;
				break;
			case "Size":
				emSize = float.Parse(childNode.InnerText);
				break;
			case "Bold":
				flag = bool.Parse(childNode.InnerText);
				break;
			case "Italic":
				flag2 = bool.Parse(childNode.InnerText);
				break;
			case "Underline":
				flag3 = bool.Parse(childNode.InnerText);
				break;
			}
		}
		FontStyle fontStyle = FontStyle.Regular;
		if (flag)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (flag2)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (flag3)
		{
			fontStyle |= FontStyle.Underline;
		}
		return new Font(familyName, emSize, fontStyle);
	}

	private static void SaveLocation(Point p, XmlTextWriter writer)
	{
		writer.WriteStartElement("Location");
		writer.WriteElementString("X", p.X.ToString());
		writer.WriteElementString("Y", p.Y.ToString());
		writer.WriteEndElement();
	}

	private static Point RestoreLocation(XmlNode node)
	{
		Point result = default(Point);
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string localName = childNode.LocalName;
			if (!(localName == "X"))
			{
				if (localName == "Y")
				{
					result.Y = int.Parse(childNode.InnerText);
				}
			}
			else
			{
				result.X = int.Parse(childNode.InnerText);
			}
		}
		return result;
	}

	private static void SaveFlatAppearance(FlatButtonAppearance fa, XmlTextWriter writer)
	{
		writer.WriteStartElement("FlatAppearance");
		writer.WriteElementString("BorderColor", fa.BorderColor.Name);
		writer.WriteElementString("BorderSize", fa.BorderSize.ToString());
		writer.WriteEndElement();
	}

	private static Color StringToColor(string s)
	{
		Color result = Color.FromName(s);
		if (!result.IsKnownColor)
		{
			result = Color.FromArgb(int.Parse(s, NumberStyles.HexNumber));
		}
		return result;
	}

	private static void SetBackgroundImage(Control c)
	{
		string text = c.Name;
		Image image = (File.Exists(path + "\\" + c.TopLevelControl.Name + "\\" + text + ".png") ? loadImage(path + "\\" + c.TopLevelControl.Name + "\\" + text + ".png") : ((!File.Exists(path + "\\Console\\" + text + ".png")) ? null : loadImage(path + "\\Console\\" + text + ".png")));
		bool flag = text.Equals("pnlDisplay");
		if (flag && image == null)
		{
			text = "picDisplay";
			image = (File.Exists(path + "\\" + c.TopLevelControl.Name + "\\" + text + ".png") ? loadImage(path + "\\" + c.TopLevelControl.Name + "\\" + text + ".png") : ((!File.Exists(path + "\\Console\\" + text + ".png")) ? null : loadImage(path + "\\Console\\" + text + ".png")));
		}
		if (flag)
		{
			m_objConsole.PnlDisplayBackgroundImage = image;
		}
		else
		{
			c.BackgroundImage = image;
		}
	}

	private static Image loadImage(string path)
	{
		Image value;
		try
		{
			using Image image = Image.FromFile(path);
			string text = computeHashFromImage(image);
			string value2;
			if (_image_cache.TryGetValue(text, out value))
			{
				if (!_image_cache_map.TryGetValue(path, out value2))
				{
					_image_cache_map[path] = text;
				}
				return value;
			}
			value = new Bitmap(image);
			_image_cache[text] = value;
			if (!_image_cache_map.TryGetValue(path, out value2))
			{
				_image_cache_map[path] = text;
			}
		}
		catch (Exception)
		{
			value = null;
		}
		return value;
	}

	private static Image getImageFromFilePath(string path)
	{
		if (!_image_cache_map.ContainsKey(path))
		{
			return null;
		}
		string key = _image_cache_map[path];
		if (!_image_cache.ContainsKey(key))
		{
			return null;
		}
		return _image_cache[key];
	}

	private static string computeHashFromImage(Image image)
	{
		using MemoryStream memoryStream = new MemoryStream();
		image.Save(memoryStream, image.RawFormat);
		memoryStream.Position = 0L;
		using MD5 mD = MD5.Create();
		return BitConverter.ToString(mD.ComputeHash(memoryStream)).Replace("-", "").ToLowerInvariant();
	}

	private static Image resizeImage(Image image, Control c)
	{
		if (c.ClientSize.Width == 0 || c.ClientSize.Width == 0)
		{
			return null;
		}
		if (c.ClientSize.Width == image.Width && c.ClientSize.Width == image.Height)
		{
			return image;
		}
		Graphics graphics = null;
		try
		{
			Image image2 = new Bitmap(c.ClientSize.Width, c.ClientSize.Height);
			graphics = Graphics.FromImage(image2);
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.DrawImage(image, 0, 0, image2.Width, image2.Height);
			return image;
		}
		catch
		{
			return null;
		}
		finally
		{
			graphics?.Dispose();
		}
	}
}
