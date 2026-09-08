using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class AnalyzedArguments
{
	public readonly ArrayBuilder<BoundExpression> Arguments;

	public readonly ArrayBuilder<(string Name, Location Location)?> Names;

	public readonly ArrayBuilder<RefKind> RefKinds;

	public bool IncludesReceiverAsArgument;

	private ThreeState _lazyHasDynamicArgument;

	public static readonly ObjectPool<AnalyzedArguments> Pool = CreatePool();

	public bool HasDynamicArgument
	{
		get
		{
			if (_lazyHasDynamicArgument.HasValue())
			{
				return _lazyHasDynamicArgument.Value();
			}
			bool flag = RefKinds.Count > 0;
			for (int i = 0; i < Arguments.Count; i++)
			{
				BoundExpression boundExpression = Arguments[i];
				if ((object)boundExpression.Type != null && boundExpression.Type.IsDynamic() && (!flag || RefKinds[i] == Microsoft.CodeAnalysis.RefKind.None))
				{
					_lazyHasDynamicArgument = ThreeState.True;
					return true;
				}
			}
			_lazyHasDynamicArgument = ThreeState.False;
			return false;
		}
	}

	public bool HasErrors
	{
		get
		{
			foreach (BoundExpression argument in Arguments)
			{
				if (argument.HasAnyErrors)
				{
					return true;
				}
			}
			return false;
		}
	}

	internal AnalyzedArguments()
	{
		Arguments = new ArrayBuilder<BoundExpression>(32);
		Names = new ArrayBuilder<(string, Location)?>(32);
		RefKinds = new ArrayBuilder<RefKind>(32);
	}

	public void Clear()
	{
		Arguments.Clear();
		Names.Clear();
		RefKinds.Clear();
		IncludesReceiverAsArgument = false;
		_lazyHasDynamicArgument = ThreeState.Unknown;
	}

	public BoundExpression Argument(int i)
	{
		return Arguments[i];
	}

	public void AddName(IdentifierNameSyntax name)
	{
		Names.Add((name.Identifier.ValueText, name.Location));
	}

	public string? Name(int i)
	{
		if (Names.Count == 0)
		{
			return null;
		}
		return Names[i]?.Name;
	}

	public ImmutableArray<string?> GetNames()
	{
		if (Names.Count == 0)
		{
			return default(ImmutableArray<string>);
		}
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance(Names.Count);
		for (int i = 0; i < Names.Count; i++)
		{
			instance.Add(Name(i));
		}
		return instance.ToImmutableAndFree();
	}

	public RefKind RefKind(int i)
	{
		if (RefKinds.Count <= 0)
		{
			return Microsoft.CodeAnalysis.RefKind.None;
		}
		return RefKinds[i];
	}

	public bool IsExtensionMethodReceiverArgument(int i)
	{
		if (i == 0)
		{
			return IncludesReceiverAsArgument;
		}
		return false;
	}

	public static AnalyzedArguments GetInstance()
	{
		return Pool.Allocate();
	}

	public static AnalyzedArguments GetInstance(AnalyzedArguments original)
	{
		AnalyzedArguments instance = GetInstance();
		instance.Arguments.AddRange(original.Arguments);
		instance.Names.AddRange(original.Names);
		instance.RefKinds.AddRange(original.RefKinds);
		instance.IncludesReceiverAsArgument = original.IncludesReceiverAsArgument;
		instance._lazyHasDynamicArgument = original._lazyHasDynamicArgument;
		return instance;
	}

	public static AnalyzedArguments GetInstance(ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> argumentRefKindsOpt, ImmutableArray<(string, Location)?> argumentNamesOpt)
	{
		AnalyzedArguments instance = GetInstance();
		instance.Arguments.AddRange(arguments);
		if (!argumentRefKindsOpt.IsDefault)
		{
			instance.RefKinds.AddRange(argumentRefKindsOpt);
		}
		if (!argumentNamesOpt.IsDefault)
		{
			instance.Names.AddRange(argumentNamesOpt);
		}
		return instance;
	}

	public void Free()
	{
		Clear();
		Pool.Free(this);
	}

	private static ObjectPool<AnalyzedArguments> CreatePool()
	{
		return new ObjectPool<AnalyzedArguments>(() => new AnalyzedArguments(), 10);
	}
}
