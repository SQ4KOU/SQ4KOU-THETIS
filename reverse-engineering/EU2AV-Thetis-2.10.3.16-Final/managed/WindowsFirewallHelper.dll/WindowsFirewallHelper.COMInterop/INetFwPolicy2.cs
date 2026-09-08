using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WindowsFirewallHelper.InternalHelpers;

namespace WindowsFirewallHelper.COMInterop;

[ComImport]
[Guid("98325047-C671-4174-8D81-DEFCD3F03186")]
[ComClassProgId("HNetCfg.FwPolicy2")]
internal interface INetFwPolicy2
{
	[DispId(1)]
	int CurrentProfileTypes
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	bool get_FirewallEnabled([In] NetFwProfileType2 profileType);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	void set_FirewallEnabled([In] NetFwProfileType2 profileType, [In] bool enabled);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	[return: MarshalAs(UnmanagedType.Struct)]
	object get_ExcludedInterfaces([In] NetFwProfileType2 profileType);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	void set_ExcludedInterfaces([In] NetFwProfileType2 profileType, [In][MarshalAs(UnmanagedType.Struct)] object interfaces);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(4)]
	bool get_BlockAllInboundTraffic([In] NetFwProfileType2 profileType);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(4)]
	void set_BlockAllInboundTraffic([In] NetFwProfileType2 profileType, [In] bool block);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(5)]
	bool get_NotificationsDisabled([In] NetFwProfileType2 profileType);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(5)]
	void set_NotificationsDisabled([In] NetFwProfileType2 profileType, [In] bool disabled);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(6)]
	bool get_UnicastResponsesToMulticastBroadcastDisabled([In] NetFwProfileType2 profileType);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(6)]
	void set_UnicastResponsesToMulticastBroadcastDisabled([In] NetFwProfileType2 profileType, [In] bool disabled);

	[DispId(7)]
	INetFwRules Rules
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(7)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(8)]
	INetFwServiceRestriction ServiceRestriction
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(8)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(9)]
	void EnableRuleGroup([In] int profileTypesBitmask, [In][MarshalAs(UnmanagedType.BStr)] string group, [In] bool enable);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(10)]
	bool IsRuleGroupEnabled([In] int profileTypesBitmask, [In][MarshalAs(UnmanagedType.BStr)] string group);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(11)]
	void RestoreLocalFirewallDefaults();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(12)]
	NetFwAction get_DefaultInboundAction([In] NetFwProfileType2 profileType);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(12)]
	void set_DefaultInboundAction([In] NetFwProfileType2 profileType, [In] NetFwAction action);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(13)]
	NetFwAction get_DefaultOutboundAction([In] NetFwProfileType2 profileType);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(13)]
	void set_DefaultOutboundAction([In] NetFwProfileType2 profileType, [In] NetFwAction action);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(14)]
	bool get_IsRuleGroupCurrentlyEnabled([In][MarshalAs(UnmanagedType.BStr)] string group);

	[DispId(15)]
	NetFwModifyState LocalPolicyModifyState
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(15)]
		get;
	}
}
