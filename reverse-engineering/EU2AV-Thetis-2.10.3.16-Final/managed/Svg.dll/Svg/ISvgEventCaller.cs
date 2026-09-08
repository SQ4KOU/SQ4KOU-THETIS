using System;

namespace Svg;

public interface ISvgEventCaller
{
	void RegisterAction(string rpcID, Action action);

	void RegisterAction<T1>(string rpcID, Action<T1> action);

	void RegisterAction<T1, T2>(string rpcID, Action<T1, T2> action);

	void RegisterAction<T1, T2, T3>(string rpcID, Action<T1, T2, T3> action);

	void RegisterAction<T1, T2, T3, T4>(string rpcID, Action<T1, T2, T3, T4> action);

	void RegisterAction<T1, T2, T3, T4, T5>(string rpcID, Action<T1, T2, T3, T4, T5> action);

	void RegisterAction<T1, T2, T3, T4, T5, T6>(string rpcID, Action<T1, T2, T3, T4, T5, T6> action);

	void RegisterAction<T1, T2, T3, T4, T5, T6, T7>(string rpcID, Action<T1, T2, T3, T4, T5, T6, T7> action);

	void RegisterAction<T1, T2, T3, T4, T5, T6, T7, T8>(string rpcID, Action<T1, T2, T3, T4, T5, T6, T7, T8> action);

	void UnregisterAction(string rpcID);
}
