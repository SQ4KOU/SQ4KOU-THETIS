using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public abstract class SyntaxTree
{
	protected internal static readonly ImmutableDictionary<string, ReportDiagnostic> EmptyDiagnosticOptions = ImmutableDictionary.Create<string, ReportDiagnostic>(CaseInsensitiveComparison.Comparer);

	private ImmutableArray<byte> _lazyChecksum;

	private SourceHashAlgorithm _lazyHashAlgorithm;

	private SourceGeneratorSyntaxTreeInfo _sourceGeneratorInfo;

	public abstract string FilePath { get; }

	public abstract bool HasCompilationUnitRoot { get; }

	public ParseOptions Options => OptionsCore;

	protected abstract ParseOptions OptionsCore { get; }

	[Obsolete("Obsolete due to performance problems, use CompilationOptions.SyntaxTreeOptionsProvider instead", false)]
	public virtual ImmutableDictionary<string, ReportDiagnostic> DiagnosticOptions => EmptyDiagnosticOptions;

	public abstract int Length { get; }

	public abstract Encoding? Encoding { get; }

	internal virtual bool SupportsLocations => HasCompilationUnitRoot;

	public abstract bool TryGetText([NotNullWhen(true)] out SourceText? text);

	public abstract SourceText GetText(CancellationToken cancellationToken = default(CancellationToken));

	public virtual Task<SourceText> GetTextAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		SourceText text;
		return Task.FromResult(TryGetText(out text) ? text : GetText(cancellationToken));
	}

	public bool TryGetRoot([NotNullWhen(true)] out SyntaxNode? root)
	{
		return TryGetRootCore(out root);
	}

	protected abstract bool TryGetRootCore([NotNullWhen(true)] out SyntaxNode? root);

	public SyntaxNode GetRoot(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetRootCore(cancellationToken);
	}

	protected abstract SyntaxNode GetRootCore(CancellationToken cancellationToken);

	public Task<SyntaxNode> GetRootAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetRootAsyncCore(cancellationToken);
	}

	protected abstract Task<SyntaxNode> GetRootAsyncCore(CancellationToken cancellationToken);

	public abstract SyntaxTree WithChangedText(SourceText newText);

	public abstract IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default(CancellationToken));

	public abstract IEnumerable<Diagnostic> GetDiagnostics(SyntaxNode node);

	public abstract IEnumerable<Diagnostic> GetDiagnostics(SyntaxToken token);

	public abstract IEnumerable<Diagnostic> GetDiagnostics(SyntaxTrivia trivia);

	public abstract IEnumerable<Diagnostic> GetDiagnostics(SyntaxNodeOrToken nodeOrToken);

	public abstract FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken));

	public abstract FileLinePositionSpan GetMappedLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IEnumerable<LineMapping> GetLineMappings(CancellationToken cancellationToken = default(CancellationToken));

	public virtual LineVisibility GetLineVisibility(int position, CancellationToken cancellationToken = default(CancellationToken))
	{
		return LineVisibility.Visible;
	}

	internal virtual FileLinePositionSpan GetMappedLineSpanAndVisibility(TextSpan span, out bool isHiddenPosition)
	{
		isHiddenPosition = GetLineVisibility(span.Start) == LineVisibility.Hidden;
		return GetMappedLineSpan(span);
	}

	internal string GetDisplayPath(TextSpan span, SourceReferenceResolver? resolver)
	{
		FileLinePositionSpan mappedLineSpan = GetMappedLineSpan(span);
		if (resolver == null || mappedLineSpan.Path.IsEmpty())
		{
			return mappedLineSpan.Path;
		}
		return resolver.NormalizePath(mappedLineSpan.Path, mappedLineSpan.HasMappedPath ? FilePath : null) ?? mappedLineSpan.Path;
	}

	internal string GetNormalizedPath(SourceReferenceResolver? resolver)
	{
		if (resolver == null)
		{
			return FilePath;
		}
		return resolver.NormalizePath(FilePath, null) ?? FilePath;
	}

	internal int GetDisplayLineNumber(TextSpan span)
	{
		return GetMappedLineSpan(span).StartLinePosition.Line + 1;
	}

	public abstract bool HasHiddenRegions();

	public abstract IList<TextSpan> GetChangedSpans(SyntaxTree syntaxTree);

	public abstract Location GetLocation(TextSpan span);

	public abstract bool IsEquivalentTo(SyntaxTree tree, bool topLevel = false);

	public abstract SyntaxReference GetReference(SyntaxNode node);

	public abstract IList<TextChange> GetChanges(SyntaxTree oldTree);

	internal DebugSourceInfo GetDebugSourceInfo()
	{
		if (RoslynImmutableInterlocked.VolatileRead(in _lazyChecksum).IsDefault)
		{
			SourceText text = GetText();
			_lazyHashAlgorithm = text.ChecksumAlgorithm;
			ImmutableInterlocked.InterlockedInitialize(ref _lazyChecksum, text.GetChecksum());
		}
		return new DebugSourceInfo(_lazyChecksum, _lazyHashAlgorithm);
	}

	public abstract SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options);

	public abstract SyntaxTree WithFilePath(string path);

	[Obsolete("Obsolete due to performance problems, use CompilationOptions.SyntaxTreeOptionsProvider instead", false)]
	public virtual SyntaxTree WithDiagnosticOptions(ImmutableDictionary<string, ReportDiagnostic> options)
	{
		throw new NotImplementedException();
	}

	public override string ToString()
	{
		return GetText(CancellationToken.None).ToString();
	}

	internal SourceGeneratorSyntaxTreeInfo GetSourceGeneratorInfo(ISyntaxHelper syntaxHelper, CancellationToken cancellationToken)
	{
		if (_sourceGeneratorInfo == SourceGeneratorSyntaxTreeInfo.NotComputedYet)
		{
			SyntaxNode root = GetRoot(cancellationToken);
			SourceGeneratorSyntaxTreeInfo sourceGeneratorSyntaxTreeInfo = SourceGeneratorSyntaxTreeInfo.None;
			if (syntaxHelper.ContainsGlobalAliases(root))
			{
				sourceGeneratorSyntaxTreeInfo |= SourceGeneratorSyntaxTreeInfo.ContainsGlobalAliases;
			}
			if (root.ContainsAttributes)
			{
				sourceGeneratorSyntaxTreeInfo |= SourceGeneratorSyntaxTreeInfo.ContainsAttributeList;
			}
			_sourceGeneratorInfo = sourceGeneratorSyntaxTreeInfo;
		}
		return _sourceGeneratorInfo;
	}
}
