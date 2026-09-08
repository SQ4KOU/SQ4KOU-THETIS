using System;

namespace Microsoft.CodeAnalysis.Emit;

internal readonly struct EmitContext
{
	[Flags]
	private enum Flags
	{
		None = 0,
		MetadataOnly = 1,
		IncludePrivateMembers = 2
	}

	public readonly CommonPEModuleBuilder Module;

	private readonly SyntaxNode? _syntaxNode;

	public readonly SyntaxReference? SyntaxReference;

	public readonly RebuildData? RebuildData;

	public readonly DiagnosticBag Diagnostics;

	private readonly Flags _flags;

	public bool IncludePrivateMembers => (_flags & Flags.IncludePrivateMembers) != 0;

	public bool MetadataOnly => (_flags & Flags.MetadataOnly) != 0;

	public bool IsRefAssembly
	{
		get
		{
			if (MetadataOnly)
			{
				return !IncludePrivateMembers;
			}
			return false;
		}
	}

	public SyntaxNode? SyntaxNode
	{
		get
		{
			SyntaxNode syntaxNode = _syntaxNode;
			if (syntaxNode == null)
			{
				SyntaxReference? syntaxReference = SyntaxReference;
				if (syntaxReference == null)
				{
					return null;
				}
				syntaxNode = syntaxReference.GetSyntax();
			}
			return syntaxNode;
		}
	}

	public Location? Location
	{
		get
		{
			object obj = _syntaxNode?.Location;
			if (obj == null)
			{
				SyntaxReference? syntaxReference = SyntaxReference;
				if (syntaxReference == null)
				{
					return null;
				}
				obj = syntaxReference.GetLocation();
			}
			return (Location?)obj;
		}
	}

	public EmitContext(CommonPEModuleBuilder module, SyntaxNode? syntaxNode, DiagnosticBag diagnostics, bool metadataOnly, bool includePrivateMembers)
		: this(module, diagnostics, metadataOnly, includePrivateMembers, syntaxNode)
	{
	}

	public EmitContext(CommonPEModuleBuilder module, DiagnosticBag diagnostics, bool metadataOnly, bool includePrivateMembers, SyntaxNode? syntaxNode = null, RebuildData? rebuildData = null, SyntaxReference? syntaxReference = null)
	{
		RebuildData = rebuildData;
		Module = module;
		_syntaxNode = syntaxNode;
		SyntaxReference = syntaxReference;
		RebuildData = rebuildData;
		Diagnostics = diagnostics;
		Flags flags = Flags.None;
		if (metadataOnly)
		{
			flags |= Flags.MetadataOnly;
		}
		if (includePrivateMembers)
		{
			flags |= Flags.IncludePrivateMembers;
		}
		_flags = flags;
	}
}
