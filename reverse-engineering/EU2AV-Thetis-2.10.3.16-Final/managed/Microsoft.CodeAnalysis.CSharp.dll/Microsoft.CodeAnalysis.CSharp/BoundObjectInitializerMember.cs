using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundObjectInitializerMember : BoundExpression, IBoundInvalidNode
{
	public override Symbol? ExpressionSymbol => MemberSymbol;

	ImmutableArray<BoundNode> IBoundInvalidNode.InvalidNodeChildren => StaticCast<BoundNode>.From(Arguments);

	public new TypeSymbol Type => base.Type;

	public Symbol? MemberSymbol { get; }

	public ImmutableArray<BoundExpression> Arguments { get; }

	public ImmutableArray<string?> ArgumentNamesOpt { get; }

	public ImmutableArray<RefKind> ArgumentRefKindsOpt { get; }

	public bool Expanded { get; }

	public ImmutableArray<int> ArgsToParamsOpt { get; }

	public BitVector DefaultArguments { get; }

	public override LookupResultKind ResultKind { get; }

	public AccessorKind AccessorKind { get; }

	public TypeSymbol ReceiverType { get; }

	public BoundObjectInitializerMember(SyntaxNode syntax, Symbol? memberSymbol, ImmutableArray<BoundExpression> arguments, ImmutableArray<string?> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, LookupResultKind resultKind, AccessorKind accessorKind, TypeSymbol receiverType, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ObjectInitializerMember, syntax, type, hasErrors || arguments.HasErrors())
	{
		MemberSymbol = memberSymbol;
		Arguments = arguments;
		ArgumentNamesOpt = argumentNamesOpt;
		ArgumentRefKindsOpt = argumentRefKindsOpt;
		Expanded = expanded;
		ArgsToParamsOpt = argsToParamsOpt;
		DefaultArguments = defaultArguments;
		ResultKind = resultKind;
		AccessorKind = accessorKind;
		ReceiverType = receiverType;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitObjectInitializerMember(this);
	}

	public BoundObjectInitializerMember Update(Symbol? memberSymbol, ImmutableArray<BoundExpression> arguments, ImmutableArray<string?> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, LookupResultKind resultKind, AccessorKind accessorKind, TypeSymbol receiverType, TypeSymbol type)
	{
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(memberSymbol, MemberSymbol) || arguments != Arguments || argumentNamesOpt != ArgumentNamesOpt || argumentRefKindsOpt != ArgumentRefKindsOpt || expanded != Expanded || argsToParamsOpt != ArgsToParamsOpt || defaultArguments != DefaultArguments || resultKind != ResultKind || accessorKind != AccessorKind || !TypeSymbol.Equals(receiverType, ReceiverType, TypeCompareKind.ConsiderEverything) || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundObjectInitializerMember boundObjectInitializerMember = new BoundObjectInitializerMember(Syntax, memberSymbol, arguments, argumentNamesOpt, argumentRefKindsOpt, expanded, argsToParamsOpt, defaultArguments, resultKind, accessorKind, receiverType, type, base.HasErrors);
			boundObjectInitializerMember.CopyAttributes(this);
			return boundObjectInitializerMember;
		}
		return this;
	}
}
