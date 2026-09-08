using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal abstract class HostAnalysisScope(DiagnosticAnalyzer analyzer)
{
	private StrongBox<AnalyzerActions>? _analyzerActions;

	internal DiagnosticAnalyzer Analyzer { get; } = analyzer;

	public virtual AnalyzerActions GetAnalyzerActions()
	{
		return GetOrCreateAnalyzerActions().Value;
	}

	public void RegisterCompilationAction(Action<CompilationAnalysisContext> action)
	{
		CompilationAnalyzerAction action2 = new CompilationAnalyzerAction(action, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddCompilationAction(action2);
	}

	public void RegisterCompilationEndAction(Action<CompilationAnalysisContext> action)
	{
		CompilationAnalyzerAction action2 = new CompilationAnalyzerAction(action, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddCompilationEndAction(action2);
	}

	public void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action)
	{
		SemanticModelAnalyzerAction action2 = new SemanticModelAnalyzerAction(action, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddSemanticModelAction(action2);
	}

	public void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action)
	{
		SyntaxTreeAnalyzerAction action2 = new SyntaxTreeAnalyzerAction(action, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddSyntaxTreeAction(action2);
	}

	public void RegisterAdditionalFileAction(Action<AdditionalFileAnalysisContext> action)
	{
		AdditionalFileAnalyzerAction action2 = new AdditionalFileAnalyzerAction(action, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddAdditionalFileAction(action2);
	}

	public void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds)
	{
		SymbolAnalyzerAction action2 = new SymbolAnalyzerAction(action, symbolKinds, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddSymbolAction(action2);
		if (!symbolKinds.Contains(SymbolKind.Parameter))
		{
			return;
		}
		RegisterSymbolAction(delegate(SymbolAnalysisContext context)
		{
			ImmutableArray<IParameterSymbol> immutableArray;
			switch (context.Symbol.Kind)
			{
			case SymbolKind.Method:
				immutableArray = ((IMethodSymbol)context.Symbol).Parameters;
				break;
			case SymbolKind.Property:
				immutableArray = ((IPropertySymbol)context.Symbol).Parameters;
				break;
			case SymbolKind.NamedType:
			{
				INamedTypeSymbol namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
				if (namedTypeSymbol.IsExtension)
				{
					IParameterSymbol extensionParameter = namedTypeSymbol.ExtensionParameter;
					immutableArray = ((extensionParameter != null) ? ImmutableCollectionsMarshal.AsImmutableArray(new IParameterSymbol[1] { extensionParameter }) : ImmutableArray<IParameterSymbol>.Empty);
				}
				else
				{
					immutableArray = namedTypeSymbol.DelegateInvokeMethod?.Parameters ?? ImmutableArray.Create<IParameterSymbol>();
				}
				break;
			}
			default:
				throw new ArgumentException($"{context.Symbol.Kind} is not supported.", "context");
			}
			foreach (IParameterSymbol item in immutableArray)
			{
				if (!item.IsImplicitlyDeclared)
				{
					action(new SymbolAnalysisContext(item, context.Compilation, context.Options, ((SymbolAnalysisContext)context).ReportDiagnostic, context.IsSupportedDiagnostic, context.IsGeneratedCode, context.FilterTree, context.FilterSpan, context.CancellationToken));
				}
			}
		}, ImmutableArray.Create(SymbolKind.Method, SymbolKind.Property, SymbolKind.NamedType));
	}

	public void RegisterSymbolStartAction(Action<SymbolStartAnalysisContext> action, SymbolKind symbolKind)
	{
		SymbolStartAnalyzerAction action2 = new SymbolStartAnalyzerAction(action, symbolKind, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddSymbolStartAction(action2);
	}

	public void RegisterSymbolEndAction(Action<SymbolAnalysisContext> action)
	{
		SymbolEndAnalyzerAction action2 = new SymbolEndAnalyzerAction(action, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddSymbolEndAction(action2);
	}

	public void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct
	{
		CodeBlockStartAnalyzerAction<TLanguageKindEnum> action2 = new CodeBlockStartAnalyzerAction<TLanguageKindEnum>(action, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddCodeBlockStartAction(action2);
	}

	public void RegisterCodeBlockEndAction(Action<CodeBlockAnalysisContext> action)
	{
		CodeBlockAnalyzerAction action2 = new CodeBlockAnalyzerAction(action, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddCodeBlockEndAction(action2);
	}

	public void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action)
	{
		CodeBlockAnalyzerAction action2 = new CodeBlockAnalyzerAction(action, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddCodeBlockAction(action2);
	}

	public void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds) where TLanguageKindEnum : struct
	{
		SyntaxNodeAnalyzerAction<TLanguageKindEnum> action2 = new SyntaxNodeAnalyzerAction<TLanguageKindEnum>(action, syntaxKinds, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddSyntaxNodeAction(action2);
	}

	public void RegisterOperationBlockStartAction(Action<OperationBlockStartAnalysisContext> action)
	{
		OperationBlockStartAnalyzerAction action2 = new OperationBlockStartAnalyzerAction(action, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddOperationBlockStartAction(action2);
	}

	public void RegisterOperationBlockEndAction(Action<OperationBlockAnalysisContext> action)
	{
		OperationBlockAnalyzerAction action2 = new OperationBlockAnalyzerAction(action, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddOperationBlockEndAction(action2);
	}

	public void RegisterOperationBlockAction(Action<OperationBlockAnalysisContext> action)
	{
		OperationBlockAnalyzerAction action2 = new OperationBlockAnalyzerAction(action, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddOperationBlockAction(action2);
	}

	public void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		OperationAnalyzerAction action2 = new OperationAnalyzerAction(action, operationKinds, Analyzer);
		GetOrCreateAnalyzerActions().Value.AddOperationAction(action2);
	}

	protected StrongBox<AnalyzerActions> GetOrCreateAnalyzerActions()
	{
		return InterlockedOperations.Initialize(ref _analyzerActions, () => new StrongBox<AnalyzerActions>(AnalyzerActions.Empty));
	}
}
