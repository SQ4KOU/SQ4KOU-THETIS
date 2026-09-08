using System.Runtime.InteropServices;

namespace Thetis;

internal static class Scope
{
	public delegate void createscope(int id);

	public unsafe delegate void Xscope(int state, double* data);

	private static createscope cscDel = createScope;

	private const int nscopes = 16;

	public static DoScope[] dscope = new DoScope[16];

	private static Xscope[] pscope = new Xscope[16];

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SendCBCreateScope(createscope del);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SendCBScope(int id, Xscope del);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetScopeRun(int id, int run);

	public unsafe static void createScope(int id)
	{
		dscope[id] = new DoScope();
		pscope[id] = dscope[id].xscope;
		SendCBScope(id, pscope[id]);
	}

	public static void initScope()
	{
		SendCBCreateScope(cscDel);
	}
}
