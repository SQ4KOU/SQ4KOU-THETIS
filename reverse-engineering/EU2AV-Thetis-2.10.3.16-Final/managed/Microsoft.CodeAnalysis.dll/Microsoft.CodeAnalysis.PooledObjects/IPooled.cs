namespace Microsoft.CodeAnalysis.PooledObjects;

internal interface IPooled
{
	void Free(bool discardLargeInstances);
}
