using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundUnconvertedObjectCreationExpression : BoundExpression
{
	public override object Display
	{
		get
		{
			ImmutableArray<BoundExpression> arguments = Arguments;
			if (arguments.Length == 0)
			{
				return "new()";
			}
			PooledStringBuilder instance = PooledStringBuilder.GetInstance();
			StringBuilder builder = instance.Builder;
			object[] array = new object[arguments.Length];
			builder.Append("new");
			builder.Append('(');
			builder.Append("{0}");
			array[0] = arguments[0].Display;
			for (int i = 1; i < arguments.Length; i++)
			{
				builder.Append(", {" + i + "}");
				array[i] = arguments[i].Display;
			}
			builder.Append(')');
			return FormattableStringFactory.Create(instance.ToStringAndFree(), array);
		}
	}

	public new TypeSymbol? Type => base.Type;

	public ImmutableArray<BoundExpression> Arguments { get; }

	public ImmutableArray<(string Name, Location Location)?> ArgumentNamesOpt { get; }

	public ImmutableArray<RefKind> ArgumentRefKindsOpt { get; }

	public InitializerExpressionSyntax? InitializerOpt { get; }

	public Binder Binder { get; }

	public BoundUnconvertedObjectCreationExpression(SyntaxNode syntax, ImmutableArray<BoundExpression> arguments, ImmutableArray<(string Name, Location Location)?> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, InitializerExpressionSyntax? initializerOpt, Binder binder, bool hasErrors = false)
		: base(BoundKind.UnconvertedObjectCreationExpression, syntax, null, hasErrors || arguments.HasErrors())
	{
		Arguments = arguments;
		ArgumentNamesOpt = argumentNamesOpt;
		ArgumentRefKindsOpt = argumentRefKindsOpt;
		InitializerOpt = initializerOpt;
		Binder = binder;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitUnconvertedObjectCreationExpression(this);
	}

	public BoundUnconvertedObjectCreationExpression Update(ImmutableArray<BoundExpression> arguments, ImmutableArray<(string Name, Location Location)?> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, InitializerExpressionSyntax? initializerOpt, Binder binder)
	{
		if (arguments != Arguments || argumentNamesOpt != ArgumentNamesOpt || argumentRefKindsOpt != ArgumentRefKindsOpt || initializerOpt != InitializerOpt || binder != Binder)
		{
			BoundUnconvertedObjectCreationExpression boundUnconvertedObjectCreationExpression = new BoundUnconvertedObjectCreationExpression(Syntax, arguments, argumentNamesOpt, argumentRefKindsOpt, initializerOpt, binder, base.HasErrors);
			boundUnconvertedObjectCreationExpression.CopyAttributes(this);
			return boundUnconvertedObjectCreationExpression;
		}
		return this;
	}
}
