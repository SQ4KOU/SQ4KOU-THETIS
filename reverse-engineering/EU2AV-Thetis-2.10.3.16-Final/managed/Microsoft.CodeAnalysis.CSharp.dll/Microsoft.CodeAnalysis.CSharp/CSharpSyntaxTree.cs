using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public abstract class CSharpSyntaxTree : SyntaxTree
{
	private sealed class DebuggerSyntaxTree : ParsedSyntaxTree
	{
		internal override bool SupportsLocations => true;

		public DebuggerSyntaxTree(CSharpSyntaxNode root, SourceText text, CSharpParseOptions options)
			: base(text, text.Encoding, text.ChecksumAlgorithm, "", options, root, DirectiveStack.Empty, null, cloneRoot: true)
		{
		}
	}

	internal sealed class DummySyntaxTree : CSharpSyntaxTree
	{
		private const SourceHashAlgorithm ChecksumAlgorithm = SourceHashAlgorithm.Sha1;

		private readonly Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax _node;

		public override Encoding Encoding => System.Text.Encoding.UTF8;

		public override int Length => 0;

		public override CSharpParseOptions Options => CSharpParseOptions.Default;

		[Obsolete("Obsolete due to performance problems, use CompilationOptions.SyntaxTreeOptionsProvider instead", false)]
		public override ImmutableDictionary<string, ReportDiagnostic> DiagnosticOptions
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Syntax/CSharpSyntaxTree.Dummy.cs", 61);
			}
		}

		public override string FilePath => string.Empty;

		public override bool HasCompilationUnitRoot => true;

		public DummySyntaxTree()
		{
			_node = CloneNodeAsRoot(SyntaxFactory.ParseCompilationUnit(string.Empty));
		}

		public override string ToString()
		{
			return string.Empty;
		}

		public override SourceText GetText(CancellationToken cancellationToken)
		{
			return SourceText.From(string.Empty, Encoding);
		}

		public override bool TryGetText(out SourceText text)
		{
			text = SourceText.From(string.Empty, Encoding);
			return true;
		}

		public override SyntaxReference GetReference(SyntaxNode node)
		{
			return new SimpleSyntaxReference(node);
		}

		public override CSharpSyntaxNode GetRoot(CancellationToken cancellationToken)
		{
			return _node;
		}

		public override bool TryGetRoot(out CSharpSyntaxNode root)
		{
			root = _node;
			return true;
		}

		public override FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
		{
			return default(FileLinePositionSpan);
		}

		public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
		{
			return Create((CSharpSyntaxNode)root, (CSharpParseOptions)options, FilePath, Encoding, SourceHashAlgorithm.Sha1);
		}

		public override SyntaxTree WithFilePath(string path)
		{
			return Create(_node, Options, path, Encoding, SourceHashAlgorithm.Sha1);
		}
	}

	private sealed class LazySyntaxTree : CSharpSyntaxTree
	{
		private readonly SourceText _text;

		private readonly CSharpParseOptions _options;

		private readonly string _path;

		private readonly ImmutableDictionary<string, ReportDiagnostic> _diagnosticOptions;

		private CSharpSyntaxNode? _lazyRoot;

		public override string FilePath => _path;

		public override Encoding? Encoding => _text.Encoding;

		public override int Length => _text.Length;

		public override bool HasCompilationUnitRoot => true;

		public override CSharpParseOptions Options => _options;

		[Obsolete("Obsolete due to performance problems, use CompilationOptions.SyntaxTreeOptionsProvider instead", false)]
		public override ImmutableDictionary<string, ReportDiagnostic> DiagnosticOptions => _diagnosticOptions;

		internal LazySyntaxTree(SourceText text, CSharpParseOptions options, string path, ImmutableDictionary<string, ReportDiagnostic>? diagnosticOptions)
		{
			_text = text;
			_options = options;
			_path = path ?? string.Empty;
			_diagnosticOptions = diagnosticOptions ?? SyntaxTree.EmptyDiagnosticOptions;
		}

		public override SourceText GetText(CancellationToken cancellationToken)
		{
			return _text;
		}

		public override bool TryGetText([NotNullWhen(true)] out SourceText? text)
		{
			text = _text;
			return true;
		}

		public override CSharpSyntaxNode GetRoot(CancellationToken cancellationToken)
		{
			if (_lazyRoot == null)
			{
				SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(_text, _options, _path, cancellationToken);
				CSharpSyntaxNode value = CloneNodeAsRoot((CSharpSyntaxNode)syntaxTree.GetRoot(cancellationToken));
				Interlocked.CompareExchange(ref _lazyRoot, value, null);
			}
			return _lazyRoot;
		}

		public override bool TryGetRoot([NotNullWhen(true)] out CSharpSyntaxNode? root)
		{
			root = _lazyRoot;
			return root != null;
		}

		public override SyntaxReference GetReference(SyntaxNode node)
		{
			return new SimpleSyntaxReference(node);
		}

		public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
		{
			if (_lazyRoot == root && (object)_options == options)
			{
				return this;
			}
			return new ParsedSyntaxTree(null, _text.Encoding, _text.ChecksumAlgorithm, _path, (CSharpParseOptions)options, (CSharpSyntaxNode)root, _lazyDirectives, _diagnosticOptions, cloneRoot: true);
		}

		public override SyntaxTree WithFilePath(string path)
		{
			if (_path == path)
			{
				return this;
			}
			if (TryGetRoot(out CSharpSyntaxNode root))
			{
				return new ParsedSyntaxTree(_text, _text.Encoding, _text.ChecksumAlgorithm, path, _options, root, default(DirectiveStack), _diagnosticOptions, cloneRoot: true);
			}
			return new LazySyntaxTree(_text, _options, path, _diagnosticOptions);
		}

		[Obsolete("Obsolete due to performance problems, use CompilationOptions.SyntaxTreeOptionsProvider instead", false)]
		public override SyntaxTree WithDiagnosticOptions(ImmutableDictionary<string, ReportDiagnostic> options)
		{
			if (options == null)
			{
				options = SyntaxTree.EmptyDiagnosticOptions;
			}
			if (_diagnosticOptions == options)
			{
				return this;
			}
			if (TryGetRoot(out CSharpSyntaxNode root))
			{
				return new ParsedSyntaxTree(_text, _text.Encoding, _text.ChecksumAlgorithm, _path, _options, root, default(DirectiveStack), options, cloneRoot: true);
			}
			return new LazySyntaxTree(_text, _options, _path, options);
		}
	}

	private class ParsedSyntaxTree : CSharpSyntaxTree
	{
		private readonly CSharpParseOptions _options;

		private readonly string _path;

		private readonly CSharpSyntaxNode _root;

		private readonly bool _hasCompilationUnitRoot;

		private readonly Encoding? _encodingOpt;

		private readonly SourceHashAlgorithm _checksumAlgorithm;

		private readonly ImmutableDictionary<string, ReportDiagnostic> _diagnosticOptions;

		private SourceText? _lazyText;

		public override string FilePath => _path;

		public override Encoding? Encoding => _encodingOpt;

		public override int Length => _root.FullSpan.Length;

		public override bool HasCompilationUnitRoot => _hasCompilationUnitRoot;

		public override CSharpParseOptions Options => _options;

		[Obsolete("Obsolete due to performance problems, use CompilationOptions.SyntaxTreeOptionsProvider instead", false)]
		public override ImmutableDictionary<string, ReportDiagnostic> DiagnosticOptions => _diagnosticOptions;

		internal ParsedSyntaxTree(SourceText? textOpt, Encoding? encodingOpt, SourceHashAlgorithm checksumAlgorithm, string? path, CSharpParseOptions options, CSharpSyntaxNode root, DirectiveStack directives, ImmutableDictionary<string, ReportDiagnostic>? diagnosticOptions, bool cloneRoot)
			: base(directives)
		{
			_lazyText = textOpt;
			_encodingOpt = encodingOpt ?? textOpt?.Encoding;
			_checksumAlgorithm = checksumAlgorithm;
			_options = options;
			_path = path ?? string.Empty;
			_root = (cloneRoot ? CloneNodeAsRoot(root) : root);
			_hasCompilationUnitRoot = root.Kind() == SyntaxKind.CompilationUnit;
			_diagnosticOptions = diagnosticOptions ?? SyntaxTree.EmptyDiagnosticOptions;
		}

		public override SourceText GetText(CancellationToken cancellationToken)
		{
			if (_lazyText == null)
			{
				Interlocked.CompareExchange(ref _lazyText, GetRoot(cancellationToken).GetText(_encodingOpt, _checksumAlgorithm), null);
			}
			return _lazyText;
		}

		public override bool TryGetText([NotNullWhen(true)] out SourceText? text)
		{
			text = _lazyText;
			return text != null;
		}

		public override CSharpSyntaxNode GetRoot(CancellationToken cancellationToken)
		{
			return _root;
		}

		public override bool TryGetRoot(out CSharpSyntaxNode root)
		{
			root = _root;
			return true;
		}

		public override SyntaxReference GetReference(SyntaxNode node)
		{
			return new SimpleSyntaxReference(node);
		}

		public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
		{
			if (_root == root && (object)_options == options)
			{
				return this;
			}
			return new ParsedSyntaxTree(null, _encodingOpt, _checksumAlgorithm, _path, (CSharpParseOptions)options, (CSharpSyntaxNode)root, (_root == root) ? _lazyDirectives : default(DirectiveStack), _diagnosticOptions, cloneRoot: true);
		}

		public override SyntaxTree WithFilePath(string path)
		{
			if (_path == path)
			{
				return this;
			}
			return new ParsedSyntaxTree(_lazyText, _encodingOpt, _checksumAlgorithm, path, _options, _root, _lazyDirectives, _diagnosticOptions, cloneRoot: true);
		}

		[Obsolete("Obsolete due to performance problems, use CompilationOptions.SyntaxTreeOptionsProvider instead", false)]
		public override SyntaxTree WithDiagnosticOptions(ImmutableDictionary<string, ReportDiagnostic> options)
		{
			if (options == null)
			{
				options = SyntaxTree.EmptyDiagnosticOptions;
			}
			if (_diagnosticOptions == options)
			{
				return this;
			}
			return new ParsedSyntaxTree(_lazyText, _encodingOpt, _checksumAlgorithm, _path, _options, _root, _lazyDirectives, options, cloneRoot: true);
		}
	}

	internal static readonly SyntaxTree Dummy = new DummySyntaxTree();

	private DirectiveStack _lazyDirectives;

	private ImmutableArray<int> _preprocessorStateChangePositions;

	private ImmutableArray<DirectiveStack> _preprocessorStates;

	private CSharpLineDirectiveMap? _lazyLineDirectiveMap;

	private CSharpPragmaWarningStateMap? _lazyPragmaWarningStateMap;

	private SingleInitNullable<NullableContextStateMap> _lazyNullableContextStateMap;

	private GeneratedKind _lazyIsGeneratedCode;

	public new abstract CSharpParseOptions Options { get; }

	internal bool HasReferenceDirectives
	{
		get
		{
			if (Options.Kind == SourceCodeKind.Script)
			{
				return GetCompilationUnitRoot().HasReferenceDirectives;
			}
			return false;
		}
	}

	internal bool HasReferenceOrLoadDirectives
	{
		get
		{
			if (Options.Kind == SourceCodeKind.Script)
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax compilationUnitRoot = GetCompilationUnitRoot();
				if (!compilationUnitRoot.HasReferenceDirectives)
				{
					return compilationUnitRoot.HasLoadDirectives;
				}
				return true;
			}
			return false;
		}
	}

	protected override ParseOptions OptionsCore => Options;

	public CSharpSyntaxTree()
		: this(default(DirectiveStack))
	{
	}

	internal CSharpSyntaxTree(DirectiveStack directives)
	{
		_lazyDirectives = directives;
	}

	protected T CloneNodeAsRoot<T>(T node) where T : CSharpSyntaxNode
	{
		return SyntaxNode.CloneNodeAsRoot(node, this);
	}

	public new abstract CSharpSyntaxNode GetRoot(CancellationToken cancellationToken = default(CancellationToken));

	public abstract bool TryGetRoot([NotNullWhen(true)] out CSharpSyntaxNode? root);

	public new virtual Task<CSharpSyntaxNode> GetRootAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		CSharpSyntaxNode root;
		return Task.FromResult(TryGetRoot(out root) ? root : GetRoot(cancellationToken));
	}

	public Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax GetCompilationUnitRoot(CancellationToken cancellationToken = default(CancellationToken))
	{
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)GetRoot(cancellationToken);
	}

	public override bool IsEquivalentTo(SyntaxTree tree, bool topLevel = false)
	{
		return SyntaxFactory.AreEquivalent(this, tree, topLevel);
	}

	internal DirectiveStack GetDirectives()
	{
		if (_lazyDirectives.IsNull)
		{
			DirectiveStack.InterlockedInitialize(ref _lazyDirectives, GetRoot().CsGreen.ApplyDirectives(DirectiveStack.Empty));
		}
		return _lazyDirectives;
	}

	internal bool IsAnyPreprocessorSymbolDefined(ImmutableArray<string> conditionalSymbols)
	{
		DirectiveStack directives = GetDirectives();
		foreach (string item in conditionalSymbols)
		{
			if (IsPreprocessorSymbolDefined(directives, item))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsPreprocessorSymbolDefined(DirectiveStack directives, string symbolName)
	{
		return directives.IsDefined(symbolName) switch
		{
			DefineState.Defined => true, 
			DefineState.Undefined => false, 
			_ => Options.PreprocessorSymbols.Contains(symbolName), 
		};
	}

	internal bool IsPreprocessorSymbolDefined(string symbolName, int position)
	{
		if (_preprocessorStateChangePositions.IsDefault)
		{
			BuildPreprocessorStateChangeMap();
		}
		int num = _preprocessorStateChangePositions.BinarySearch(position);
		DirectiveStack directives;
		if (num < 0)
		{
			num = ~num - 1;
			directives = ((num < 0) ? DirectiveStack.Empty : _preprocessorStates[num]);
		}
		else
		{
			directives = _preprocessorStates[num];
		}
		return IsPreprocessorSymbolDefined(directives, symbolName);
	}

	private void BuildPreprocessorStateChangeMap()
	{
		DirectiveStack directiveStack = DirectiveStack.Empty;
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
		ArrayBuilder<DirectiveStack> instance2 = ArrayBuilder<DirectiveStack>.GetInstance();
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax directive in GetRoot().GetDirectives(delegate(Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax d)
		{
			SyntaxKind syntaxKind = d.Kind();
			return (syntaxKind - 8548 <= (SyntaxKind)3 || syntaxKind - 8554 <= SyntaxKind.List) ? true : false;
		}))
		{
			directiveStack = directive.ApplyDirectives(directiveStack);
			switch (directive.Kind())
			{
			case SyntaxKind.ElifDirectiveTrivia:
				instance2.Add(directiveStack);
				instance.Add(((Microsoft.CodeAnalysis.CSharp.Syntax.ElifDirectiveTriviaSyntax)directive).ElifKeyword.SpanStart);
				break;
			case SyntaxKind.ElseDirectiveTrivia:
				instance2.Add(directiveStack);
				instance.Add(((Microsoft.CodeAnalysis.CSharp.Syntax.ElseDirectiveTriviaSyntax)directive).ElseKeyword.SpanStart);
				break;
			case SyntaxKind.EndIfDirectiveTrivia:
				instance2.Add(directiveStack);
				instance.Add(((Microsoft.CodeAnalysis.CSharp.Syntax.EndIfDirectiveTriviaSyntax)directive).EndIfKeyword.SpanStart);
				break;
			case SyntaxKind.DefineDirectiveTrivia:
				instance2.Add(directiveStack);
				instance.Add(((Microsoft.CodeAnalysis.CSharp.Syntax.DefineDirectiveTriviaSyntax)directive).Name.SpanStart);
				break;
			case SyntaxKind.UndefDirectiveTrivia:
				instance2.Add(directiveStack);
				instance.Add(((Microsoft.CodeAnalysis.CSharp.Syntax.UndefDirectiveTriviaSyntax)directive).Name.SpanStart);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(directive.Kind());
			case SyntaxKind.IfDirectiveTrivia:
				break;
			}
		}
		ImmutableInterlocked.InterlockedInitialize(ref _preprocessorStates, instance2.ToImmutableAndFree());
		ImmutableInterlocked.InterlockedInitialize(ref _preprocessorStateChangePositions, instance.ToImmutableAndFree());
	}

	public static SyntaxTree Create(CSharpSyntaxNode root, CSharpParseOptions? options = null, string? path = "", Encoding? encoding = null)
	{
		return Create(root, options, path, encoding, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The diagnosticOptions and isGeneratedCode parameters are obsolete due to performance problems, if you are using them use CompilationOptions.SyntaxTreeOptionsProvider instead", false)]
	public static SyntaxTree Create(CSharpSyntaxNode root, CSharpParseOptions? options, string? path, Encoding? encoding, ImmutableDictionary<string, ReportDiagnostic>? diagnosticOptions, bool? isGeneratedCode)
	{
		if (root == null)
		{
			throw new ArgumentNullException("root");
		}
		return new ParsedSyntaxTree(null, encoding, SourceHashAlgorithm.Sha1, path, options ?? CSharpParseOptions.Default, root, default(DirectiveStack), diagnosticOptions, cloneRoot: true);
	}

	internal static SyntaxTree Create(CSharpSyntaxNode root, CSharpParseOptions options, string? path, Encoding? encoding, SourceHashAlgorithm checksumAlgorithm)
	{
		return new ParsedSyntaxTree(null, encoding, checksumAlgorithm, path, options, root, default(DirectiveStack), null, cloneRoot: true);
	}

	internal static SyntaxTree CreateForDebugger(CSharpSyntaxNode root, SourceText text, CSharpParseOptions options)
	{
		return new DebuggerSyntaxTree(root, text, options);
	}

	internal static SyntaxTree CreateWithoutClone(CSharpSyntaxNode root, CSharpParseOptions options)
	{
		return new ParsedSyntaxTree(null, null, SourceHashAlgorithm.Sha1, "", options, root, default(DirectiveStack), null, cloneRoot: false);
	}

	internal static SyntaxTree ParseTextLazy(SourceText text, CSharpParseOptions? options = null, string path = "")
	{
		return new LazySyntaxTree(text, options ?? CSharpParseOptions.Default, path, null);
	}

	public static SyntaxTree ParseText(string text, CSharpParseOptions? options = null, string path = "", Encoding? encoding = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ParseText(text, options, path, encoding, null, cancellationToken);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The diagnosticOptions and isGeneratedCode parameters are obsolete due to performance problems, if you are using them use CompilationOptions.SyntaxTreeOptionsProvider instead", false)]
	public static SyntaxTree ParseText(string text, CSharpParseOptions? options, string path, Encoding? encoding, ImmutableDictionary<string, ReportDiagnostic>? diagnosticOptions, bool? isGeneratedCode, CancellationToken cancellationToken)
	{
		return ParseText(SourceText.From(text, encoding), options, path, diagnosticOptions, isGeneratedCode, cancellationToken);
	}

	public static SyntaxTree ParseText(SourceText text, CSharpParseOptions? options = null, string path = "", CancellationToken cancellationToken = default(CancellationToken))
	{
		return ParseText(text, options, path, null, cancellationToken);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The diagnosticOptions and isGeneratedCode parameters are obsolete due to performance problems, if you are using them use CompilationOptions.SyntaxTreeOptionsProvider instead", false)]
	public static SyntaxTree ParseText(SourceText text, CSharpParseOptions? options, string path, ImmutableDictionary<string, ReportDiagnostic>? diagnosticOptions, bool? isGeneratedCode, CancellationToken cancellationToken)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		options = options ?? CSharpParseOptions.Default;
		using Lexer lexer = new Lexer(text, options);
		using LanguageParser languageParser = new LanguageParser(lexer, null, null, LexerMode.Syntax, cancellationToken);
		Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax compilationUnitSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)languageParser.ParseCompilationUnit().CreateRed();
		return compilationUnitSyntax._syntaxTree = new ParsedSyntaxTree(text, text.Encoding, text.ChecksumAlgorithm, path, options, compilationUnitSyntax, languageParser.Directives, diagnosticOptions, cloneRoot: false);
	}

	public override SyntaxTree WithChangedText(SourceText newText)
	{
		if (TryGetText(out SourceText text))
		{
			IReadOnlyList<TextChangeRange> changeRanges = newText.GetChangeRanges(text);
			if (changeRanges.Count == 0 && newText == text)
			{
				return this;
			}
			return WithChanges(newText, changeRanges);
		}
		return WithChanges(newText, new TextChangeRange[1]
		{
			new TextChangeRange(new TextSpan(0, Length), newText.Length)
		});
	}

	private SyntaxTree WithChanges(SourceText newText, IReadOnlyList<TextChangeRange> changes)
	{
		if (changes == null)
		{
			throw new ArgumentNullException("changes");
		}
		IReadOnlyList<TextChangeRange> readOnlyList = changes;
		CSharpSyntaxTree cSharpSyntaxTree = this;
		if (readOnlyList.Count == 1 && readOnlyList[0].Span == new TextSpan(0, Length) && readOnlyList[0].NewLength == newText.Length)
		{
			readOnlyList = null;
			cSharpSyntaxTree = null;
		}
		using Lexer lexer = new Lexer(newText, Options);
		using LanguageParser languageParser = new LanguageParser(lexer, cSharpSyntaxTree?.GetRoot(), readOnlyList);
		Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)languageParser.ParseCompilationUnit().CreateRed();
		return new ParsedSyntaxTree(newText, newText.Encoding, newText.ChecksumAlgorithm, FilePath, Options, root, languageParser.Directives, DiagnosticOptions, cloneRoot: true);
	}

	public override IList<TextSpan> GetChangedSpans(SyntaxTree oldTree)
	{
		if (oldTree == null)
		{
			throw new ArgumentNullException("oldTree");
		}
		return SyntaxDiffer.GetPossiblyDifferentTextSpans(oldTree, this);
	}

	public override IList<TextChange> GetChanges(SyntaxTree oldTree)
	{
		if (oldTree == null)
		{
			throw new ArgumentNullException("oldTree");
		}
		return SyntaxDiffer.GetTextChanges(oldTree, this);
	}

	private CSharpLineDirectiveMap GetDirectiveMap()
	{
		if (_lazyLineDirectiveMap == null)
		{
			Interlocked.CompareExchange(ref _lazyLineDirectiveMap, new CSharpLineDirectiveMap(this), null);
		}
		return _lazyLineDirectiveMap;
	}

	public override FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
	{
		return new FileLinePositionSpan(FilePath, GetLinePosition(span.Start, cancellationToken), GetLinePosition(span.End, cancellationToken));
	}

	public override FileLinePositionSpan GetMappedLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDirectiveMap().TranslateSpan(GetText(cancellationToken), FilePath, span);
	}

	public override LineVisibility GetLineVisibility(int position, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDirectiveMap().GetLineVisibility(GetText(cancellationToken), position);
	}

	public override IEnumerable<LineMapping> GetLineMappings(CancellationToken cancellationToken = default(CancellationToken))
	{
		CSharpLineDirectiveMap directiveMap = GetDirectiveMap();
		if (directiveMap.Entries.Length != 1)
		{
			return directiveMap.GetLineMappings(GetText(cancellationToken).Lines);
		}
		return Array.Empty<LineMapping>();
	}

	internal override FileLinePositionSpan GetMappedLineSpanAndVisibility(TextSpan span, out bool isHiddenPosition)
	{
		return GetDirectiveMap().TranslateSpanAndVisibility(GetText(), FilePath, span, out isHiddenPosition);
	}

	public override bool HasHiddenRegions()
	{
		return GetDirectiveMap().HasAnyHiddenRegions();
	}

	internal PragmaWarningState GetPragmaDirectiveWarningState(string id, int position)
	{
		if (_lazyPragmaWarningStateMap == null)
		{
			Interlocked.CompareExchange(ref _lazyPragmaWarningStateMap, new CSharpPragmaWarningStateMap(this), null);
		}
		return _lazyPragmaWarningStateMap.GetWarningState(id, position);
	}

	private NullableContextStateMap GetNullableContextStateMap()
	{
		return _lazyNullableContextStateMap.Initialize((CSharpSyntaxTree @this) => NullableContextStateMap.Create(@this), this);
	}

	internal NullableContextState GetNullableContextState(int position)
	{
		return GetNullableContextStateMap().GetContextState(position);
	}

	internal bool? IsNullableAnalysisEnabled(TextSpan span)
	{
		return GetNullableContextStateMap().IsNullableAnalysisEnabled(span);
	}

	internal bool IsGeneratedCode(SyntaxTreeOptionsProvider? provider, CancellationToken cancellationToken)
	{
		GeneratedKind? generatedKind = provider?.IsGenerated(this, cancellationToken);
		if (generatedKind.HasValue)
		{
			GeneratedKind valueOrDefault = generatedKind.GetValueOrDefault();
			if (valueOrDefault != GeneratedKind.Unknown)
			{
				return valueOrDefault != GeneratedKind.NotGenerated;
			}
		}
		return isGeneratedHeuristic();
		bool isGeneratedHeuristic()
		{
			if (_lazyIsGeneratedCode == GeneratedKind.Unknown)
			{
				bool flag = GeneratedCodeUtilities.IsGeneratedCode(this, (SyntaxTrivia trivia) => trivia.Kind() == SyntaxKind.SingleLineCommentTrivia || trivia.Kind() == SyntaxKind.MultiLineCommentTrivia, default(CancellationToken));
				_lazyIsGeneratedCode = ((!flag) ? GeneratedKind.NotGenerated : GeneratedKind.MarkedGenerated);
			}
			return _lazyIsGeneratedCode == GeneratedKind.MarkedGenerated;
		}
	}

	private LinePosition GetLinePosition(int position, CancellationToken cancellationToken)
	{
		return GetText(cancellationToken).Lines.GetLinePosition(position);
	}

	public override Location GetLocation(TextSpan span)
	{
		return new SourceLocation(this, span);
	}

	public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		return GetDiagnostics(node.Green, node.Position);
	}

	private IEnumerable<Diagnostic> GetDiagnostics(GreenNode greenNode, int position)
	{
		if (greenNode == null)
		{
			throw new InvalidOperationException();
		}
		if (greenNode.ContainsDiagnostics)
		{
			return SyntaxTreeDiagnosticEnumerator.EnumerateDiagnostics(this, greenNode, position);
		}
		return SpecializedCollections.EmptyEnumerable<Diagnostic>();
	}

	public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxToken token)
	{
		if (token.Node == null)
		{
			throw new InvalidOperationException();
		}
		return GetDiagnostics(token.Node, token.Position);
	}

	public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxTrivia trivia)
	{
		if (trivia.UnderlyingNode == null)
		{
			throw new InvalidOperationException();
		}
		return GetDiagnostics(trivia.UnderlyingNode, trivia.Position);
	}

	public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNodeOrToken nodeOrToken)
	{
		if (nodeOrToken.UnderlyingNode == null)
		{
			throw new InvalidOperationException();
		}
		return GetDiagnostics(nodeOrToken.UnderlyingNode, nodeOrToken.Position);
	}

	public override IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(GetRoot(cancellationToken));
	}

	protected override SyntaxNode GetRootCore(CancellationToken cancellationToken)
	{
		return GetRoot(cancellationToken);
	}

	protected override async Task<SyntaxNode> GetRootAsyncCore(CancellationToken cancellationToken)
	{
		return await GetRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected override bool TryGetRootCore([NotNullWhen(true)] out SyntaxNode? root)
	{
		if (TryGetRoot(out CSharpSyntaxNode root2))
		{
			root = root2;
			return true;
		}
		root = null;
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The diagnosticOptions parameter is obsolete due to performance problems, if you are passing non-null use CompilationOptions.SyntaxTreeOptionsProvider instead", false)]
	public static SyntaxTree ParseText(SourceText text, CSharpParseOptions? options, string path, ImmutableDictionary<string, ReportDiagnostic>? diagnosticOptions, CancellationToken cancellationToken)
	{
		return ParseText(text, options, path, diagnosticOptions, null, cancellationToken);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The diagnosticOptions parameter is obsolete due to performance problems, if you are passing non-null use CompilationOptions.SyntaxTreeOptionsProvider instead", false)]
	public static SyntaxTree ParseText(string text, CSharpParseOptions? options, string path, Encoding? encoding, ImmutableDictionary<string, ReportDiagnostic>? diagnosticOptions, CancellationToken cancellationToken)
	{
		return ParseText(SourceText.From(text, encoding), options, path, diagnosticOptions, null, cancellationToken);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The diagnosticOptions parameter is obsolete due to performance problems, if you are passing non-null use CompilationOptions.SyntaxTreeOptionsProvider instead", false)]
	public static SyntaxTree Create(CSharpSyntaxNode root, CSharpParseOptions? options, string? path, Encoding? encoding, ImmutableDictionary<string, ReportDiagnostic>? diagnosticOptions)
	{
		return Create(root, options, path, encoding, diagnosticOptions, null);
	}

	internal string Dump()
	{
		return GetRoot().Dump();
	}
}
