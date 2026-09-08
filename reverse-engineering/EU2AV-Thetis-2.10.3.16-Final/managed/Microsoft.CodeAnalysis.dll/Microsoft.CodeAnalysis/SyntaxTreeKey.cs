using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis;

internal abstract class SyntaxTreeKey
{
	private sealed class DefaultSyntaxTreeKey : SyntaxTreeKey
	{
		private readonly SyntaxTree _tree;

		public override string FilePath => _tree.FilePath;

		public override ParseOptions Options => _tree.Options;

		public DefaultSyntaxTreeKey(SyntaxTree tree)
		{
			_tree = tree;
		}

		public override SourceText GetText(CancellationToken cancellationToken = default(CancellationToken))
		{
			return _tree.GetText(cancellationToken);
		}
	}

	public abstract string FilePath { get; }

	public abstract ParseOptions Options { get; }

	public abstract SourceText GetText(CancellationToken cancellationToken = default(CancellationToken));

	public static SyntaxTreeKey Create(SyntaxTree tree)
	{
		return new DefaultSyntaxTreeKey(tree);
	}
}
