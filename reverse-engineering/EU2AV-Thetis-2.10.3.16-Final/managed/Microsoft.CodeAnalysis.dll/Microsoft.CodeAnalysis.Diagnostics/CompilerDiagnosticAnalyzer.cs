using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal abstract class CompilerDiagnosticAnalyzer : DiagnosticAnalyzer
{
	private class CompilationAnalyzer
	{
		private sealed class CompilerDiagnostic : Diagnostic
		{
			private readonly Diagnostic _original;

			private readonly ImmutableDictionary<string, string?> _properties;

			public override DiagnosticDescriptor Descriptor => _original.Descriptor;

			internal override int Code => _original.Code;

			internal override IReadOnlyList<object?> Arguments => _original.Arguments;

			public override string Id => _original.Id;

			public override DiagnosticSeverity Severity => _original.Severity;

			public override int WarningLevel => _original.WarningLevel;

			public override Location Location => _original.Location;

			public override IReadOnlyList<Location> AdditionalLocations => _original.AdditionalLocations;

			public override bool IsSuppressed => _original.IsSuppressed;

			public override ImmutableDictionary<string, string?> Properties => _properties;

			public CompilerDiagnostic(Diagnostic original, ImmutableDictionary<string, string?> properties)
			{
				_original = original;
				_properties = properties;
			}

			public override string GetMessage(IFormatProvider? formatProvider = null)
			{
				return _original.GetMessage(formatProvider);
			}

			public override int GetHashCode()
			{
				return _original.GetHashCode();
			}

			public override bool Equals(Diagnostic? obj)
			{
				if (obj is CompilerDiagnostic compilerDiagnostic)
				{
					return _original.Equals(compilerDiagnostic._original);
				}
				return false;
			}

			internal override Diagnostic WithLocation(Location location)
			{
				return new CompilerDiagnostic(_original.WithLocation(location), _properties);
			}

			internal override Diagnostic WithSeverity(DiagnosticSeverity severity)
			{
				return new CompilerDiagnostic(_original.WithSeverity(severity), _properties);
			}

			internal override Diagnostic WithIsSuppressed(bool isSuppressed)
			{
				return new CompilerDiagnostic(_original.WithIsSuppressed(isSuppressed), _properties);
			}
		}

		private readonly Compilation _compilation;

		public CompilationAnalyzer(Compilation compilation)
		{
			_compilation = compilation;
		}

		public void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
		{
			ReportDiagnostics(_compilation.GetSemanticModel(context.Tree).GetSyntaxDiagnostics(context.FilterSpan, context.CancellationToken), ((SyntaxTreeAnalysisContext)context).ReportDiagnostic, IsSourceLocation, s_syntactic);
		}

		public static void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
		{
			ImmutableArray<Diagnostic> declarationDiagnostics = context.SemanticModel.GetDeclarationDiagnostics(context.FilterSpan, context.CancellationToken);
			ImmutableArray<Diagnostic> methodBodyDiagnostics = context.SemanticModel.GetMethodBodyDiagnostics(context.FilterSpan, context.CancellationToken);
			ReportDiagnostics(declarationDiagnostics, ((SemanticModelAnalysisContext)context).ReportDiagnostic, IsSourceLocation, s_declaration);
			ReportDiagnostics(methodBodyDiagnostics, ((SemanticModelAnalysisContext)context).ReportDiagnostic, IsSourceLocation);
		}

		public static void AnalyzeCompilation(CompilationAnalysisContext context)
		{
			ReportDiagnostics(context.Compilation.GetDeclarationDiagnostics(context.CancellationToken), ((CompilationAnalysisContext)context).ReportDiagnostic, (Location location) => !IsSourceLocation(location), s_declaration);
		}

		private static bool IsSourceLocation(Location location)
		{
			if (location != null)
			{
				return location.Kind == LocationKind.SourceFile;
			}
			return false;
		}

		private static void ReportDiagnostics(ImmutableArray<Diagnostic> diagnostics, Action<Diagnostic> reportDiagnostic, Func<Location, bool> locationFilter, ImmutableDictionary<string, string?>? properties = null)
		{
			foreach (Diagnostic item in diagnostics)
			{
				if (locationFilter(item.Location) && item.Severity != DiagnosticSeverity.Hidden)
				{
					Diagnostic obj = ((properties == null) ? item : new CompilerDiagnostic(item, properties));
					reportDiagnostic(obj);
				}
			}
		}
	}

	private const string Origin = "Origin";

	private const string Syntactic = "Syntactic";

	private const string Declaration = "Declaration";

	private static readonly ImmutableDictionary<string, string?> s_syntactic = ImmutableDictionary<string, string>.Empty.Add("Origin", "Syntactic");

	private static readonly ImmutableDictionary<string, string?> s_declaration = ImmutableDictionary<string, string>.Empty.Add("Origin", "Declaration");

	private ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

	protected abstract CommonMessageProvider MessageProvider { get; }

	public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => InterlockedOperations.Initialize(ref _supportedDiagnostics, delegate(CompilerDiagnosticAnalyzer @this)
	{
		CommonMessageProvider messageProvider = @this.MessageProvider;
		ImmutableArray<int> supportedErrorCodes = @this.GetSupportedErrorCodes();
		ArrayBuilder<DiagnosticDescriptor> instance = ArrayBuilder<DiagnosticDescriptor>.GetInstance(supportedErrorCodes.Length);
		foreach (int item in supportedErrorCodes)
		{
			instance.Add(DiagnosticInfo.GetDescriptor(item, messageProvider));
		}
		instance.Add(AnalyzerExecutor.GetAnalyzerExceptionDiagnosticDescriptor());
		return instance.ToImmutableAndFree();
	}, this);

	internal abstract ImmutableArray<int> GetSupportedErrorCodes();

	public sealed override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
		context.RegisterCompilationStartAction(delegate(CompilationStartAnalysisContext c)
		{
			CompilationAnalyzer compilationAnalyzer = new CompilationAnalyzer(c.Compilation);
			c.RegisterSyntaxTreeAction(compilationAnalyzer.AnalyzeSyntaxTree);
			c.RegisterSemanticModelAction(CompilationAnalyzer.AnalyzeSemanticModel);
		});
	}
}
