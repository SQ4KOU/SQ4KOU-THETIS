using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Thetis;

[Serializable]
public class OtherButtonMacroSettings
{
	public enum OB_ButtonState
	{
		OFF,
		ON,
		TOGGLE,
		LED,
		CONT_VIS,
		CAT
	}

	private int _number;

	private string _on_text;

	private string _off_text;

	private string _notes;

	private bool _closes_parent;

	private bool[] _closes_container = new bool[4];

	private bool[] _opens_container = new bool[4];

	private string[] _close_container_id = new string[4];

	private string[] _open_container_id = new string[4];

	private bool[] _open_uses_location = new bool[4];

	private bool[] _send_via_mmio = new bool[4];

	private string[] _mmio_4char = new string[4];

	private string[] _mmio_message_on = new string[4];

	private string[] _mmio_message_off = new string[4];

	private OB_ButtonState _buttonstate_type;

	private string _led_indicator_four_char;

	private string _buttonstate_container_visible_id;

	private string _buttonstate_cat_on_reply;

	private bool _run_state_command_on_visible;

	private bool[] _cat_macro_send = new bool[1];

	private bool _on_state;

	private string _cat_macro;

	public int Number
	{
		get
		{
			return _number;
		}
		set
		{
			_number = value;
		}
	}

	public string OnText
	{
		get
		{
			return _on_text;
		}
		set
		{
			_on_text = value;
		}
	}

	public string OffText
	{
		get
		{
			return _off_text;
		}
		set
		{
			_off_text = value;
		}
	}

	public string Notes
	{
		get
		{
			return _notes;
		}
		set
		{
			_notes = value;
		}
	}

	public bool ClosesParent
	{
		get
		{
			return _closes_parent;
		}
		set
		{
			_closes_parent = value;
		}
	}

	public bool[] ClosesContainer => _closes_container;

	public bool[] OpensContainer => _opens_container;

	public string[] CloseContainerID => _close_container_id;

	public string[] OpenContainerID => _open_container_id;

	public bool[] OpenUsesLocation => _open_uses_location;

	public bool[] SendsViaMMIO => _send_via_mmio;

	public string[] MMICFourChar => _mmio_4char;

	public string[] MMIOMessageON => _mmio_message_on;

	public string[] MMIOMessageOFF => _mmio_message_off;

	public OB_ButtonState ButtonStateType
	{
		get
		{
			return _buttonstate_type;
		}
		set
		{
			_buttonstate_type = value;
		}
	}

	public string LedIndiciatorFourChar
	{
		get
		{
			return _led_indicator_four_char;
		}
		set
		{
			_led_indicator_four_char = value;
		}
	}

	public string ContainerVisibleID
	{
		get
		{
			return _buttonstate_container_visible_id;
		}
		set
		{
			_buttonstate_container_visible_id = value;
		}
	}

	public string ButtonStateCatReply
	{
		get
		{
			return _buttonstate_cat_on_reply;
		}
		set
		{
			_buttonstate_cat_on_reply = value;
		}
	}

	public bool[] CatMacroSend => _cat_macro_send;

	public bool RunStateCommandOnVisible
	{
		get
		{
			return _run_state_command_on_visible;
		}
		set
		{
			_run_state_command_on_visible = value;
		}
	}

	public bool OnState
	{
		get
		{
			return _on_state;
		}
		set
		{
			_on_state = value;
		}
	}

	public string CatMacro
	{
		get
		{
			return _cat_macro;
		}
		set
		{
			_cat_macro = value;
		}
	}

	private static T deep_clone<T>(T obj)
	{
		if (!typeof(T).IsSerializable)
		{
			throw new InvalidOperationException("Type must be serializable");
		}
		using MemoryStream memoryStream = new MemoryStream();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		binaryFormatter.Serialize(memoryStream, obj);
		memoryStream.Position = 0L;
		return (T)binaryFormatter.Deserialize(memoryStream);
	}

	public OtherButtonMacroSettings(OtherButtonMacroSettings settings)
	{
		if (settings != null)
		{
			OtherButtonMacroSettings obj = deep_clone(settings);
			MemberInfo[] serializableMembers = FormatterServices.GetSerializableMembers(typeof(OtherButtonMacroSettings));
			object[] objectData = FormatterServices.GetObjectData(obj, serializableMembers);
			FormatterServices.PopulateObjectMembers(this, serializableMembers, objectData);
		}
	}

	public OtherButtonMacroSettings()
	{
		_buttonstate_type = OB_ButtonState.OFF;
	}
}
