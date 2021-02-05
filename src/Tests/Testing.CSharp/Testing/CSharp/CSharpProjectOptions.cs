// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    public class CSharpProjectOptions : ProjectOptions
    {
        private static CSharpProjectOptions _default_CSharp5;
        private static CSharpProjectOptions _default_CSharp6;
        private static CSharpProjectOptions _default_CSharp7;
        private static CSharpProjectOptions _default_CSharp7_3;
        private static CSharpProjectOptions _default_NullableReferenceTypes;

        public CSharpProjectOptions(
            CSharpCompilationOptions compilationOptions,
            CSharpParseOptions parseOptions,
            IEnumerable<MetadataReference> metadataReferences,
            DiagnosticSeverity allowedCompilerDiagnosticSeverity = DiagnosticSeverity.Info,
            IEnumerable<string> allowedCompilerDiagnosticIds = null)
            : base(metadataReferences, allowedCompilerDiagnosticSeverity, allowedCompilerDiagnosticIds)
        {
            CompilationOptions = compilationOptions;
            ParseOptions = parseOptions;
        }

        public override string Language => LanguageNames.CSharp;

        public override string DefaultDocumentName => "Test.cs";

        /// <summary>
        /// Gets a parse options that should be used to parse tested source code.
        /// </summary>
        new public CSharpParseOptions ParseOptions { get; }

        /// <summary>
        /// Gets a compilation options that should be used to compile test project.
        /// </summary>
        new public CSharpCompilationOptions CompilationOptions { get; }

        /// <summary>
        /// Gets a common parse options.
        /// </summary>
        protected override ParseOptions CommonParseOptions => ParseOptions;

        /// <summary>
        /// Gets a common compilation options.
        /// </summary>
        protected override CompilationOptions CommonCompilationOptions => CompilationOptions;

        /// <summary>
        /// Gets a default code verification options.
        /// </summary>
        public static CSharpProjectOptions Default { get; } = CreateDefault();

        private static CSharpProjectOptions CreateDefault()
        {
            CSharpParseOptions parseOptions = null;
            CSharpCompilationOptions compilationOptions = null;

            using (var workspace = new AdhocWorkspace())
            {
                Project project = workspace
                    .CurrentSolution
                    .AddProject("TestProject", "TestProject", LanguageNames.CSharp);

                compilationOptions = ((CSharpCompilationOptions)project.CompilationOptions)
                    .WithAllowUnsafe(true)
                    .WithOutputKind(OutputKind.DynamicallyLinkedLibrary);

                parseOptions = ((CSharpParseOptions)project.ParseOptions);

                parseOptions = parseOptions
                    .WithLanguageVersion(LanguageVersion.CSharp9)
                    .WithPreprocessorSymbols(parseOptions.PreprocessorSymbolNames.Concat(new[] { "DEBUG" }));
            }

            return new CSharpProjectOptions(
                parseOptions: parseOptions,
                compilationOptions: compilationOptions,
                metadataReferences: RuntimeMetadataReference.MetadataReferences.Select(f => f.Value).ToImmutableArray(),
                allowedCompilerDiagnosticIds: ImmutableArray.Create(
                    "CS0067", // Event is never used
                    "CS0168", // Variable is declared but never used
                    "CS0169", // Field is never used
                    "CS0219", // Variable is assigned but its value is never used
                    "CS0414", // Field is assigned but its value is never used
                    "CS0649", // Field is never assigned to, and will always have its default value null
                    "CS0660", // Type defines operator == or operator != but does not override Object.Equals(object o)
                    "CS0661", // Type defines operator == or operator != but does not override Object.GetHashCode()
                    "CS8019", // Unnecessary using directive
                    "CS8321" // The local function is declared but never used
                ));
        }

        internal static CSharpProjectOptions Default_CSharp5
        {
            get
            {
                if (_default_CSharp5 == null)
                    Interlocked.CompareExchange(ref _default_CSharp5, Create(), null);

                return _default_CSharp5;

                static CSharpProjectOptions Create() => Default.WithParseOptions(Default.ParseOptions.WithLanguageVersion(LanguageVersion.CSharp5));
            }
        }

        internal static CSharpProjectOptions Default_CSharp6
        {
            get
            {
                if (_default_CSharp6 == null)
                    Interlocked.CompareExchange(ref _default_CSharp6, Create(), null);

                return _default_CSharp6;

                static CSharpProjectOptions Create() => Default.WithParseOptions(Default.ParseOptions.WithLanguageVersion(LanguageVersion.CSharp6));
            }
        }

        internal static CSharpProjectOptions Default_CSharp7
        {
            get
            {
                if (_default_CSharp7 == null)
                    Interlocked.CompareExchange(ref _default_CSharp7, Create(), null);

                return _default_CSharp7;

                static CSharpProjectOptions Create() => Default.WithParseOptions(Default.ParseOptions.WithLanguageVersion(LanguageVersion.CSharp7));
            }
        }

        internal static CSharpProjectOptions Default_CSharp7_3
        {
            get
            {
                if (_default_CSharp7_3 == null)
                    Interlocked.CompareExchange(ref _default_CSharp7_3, Create(), null);

                return _default_CSharp7_3;

                static CSharpProjectOptions Create() => Default.WithParseOptions(Default.ParseOptions.WithLanguageVersion(LanguageVersion.CSharp7_3));
            }
        }

        internal static CSharpProjectOptions Default_NullableReferenceTypes
        {
            get
            {
                if (_default_NullableReferenceTypes == null)
                    Interlocked.CompareExchange(ref _default_NullableReferenceTypes, Create(), null);

                return _default_NullableReferenceTypes;

                static CSharpProjectOptions Create() => Default.WithCompilationOptions(Default.CompilationOptions.WithNullableContextOptions(NullableContextOptions.Enable));
            }
        }

        /// <summary>
        /// Adds specified compiler diagnostic ID to the list of allowed compiler diagnostic IDs.
        /// </summary>
        /// <param name="diagnosticId"></param>
        public CSharpProjectOptions AddAllowedCompilerDiagnosticId(string diagnosticId)
        {
            return WithAllowedCompilerDiagnosticIds(AllowedCompilerDiagnosticIds.Add(diagnosticId));
        }

        /// <summary>
        /// Adds a list of specified compiler diagnostic IDs to the list of allowed compiler diagnostic IDs.
        /// </summary>
        /// <param name="diagnosticIds"></param>
        public CSharpProjectOptions AddAllowedCompilerDiagnosticIds(IEnumerable<string> diagnosticIds)
        {
            return WithAllowedCompilerDiagnosticIds(AllowedCompilerDiagnosticIds.AddRange(diagnosticIds));
        }

        /// <summary>
        /// Adds specified assembly name to the list of assembly names.
        /// </summary>
        /// <param name="assemblyName"></param>
        public CSharpProjectOptions AddMetadataReferences(MetadataReference metadataReference)
        {
            return WithMetadataReferences(MetadataReferences.Add(metadataReference));
        }

        internal CSharpProjectOptions WithEnabled(DiagnosticDescriptor descriptor)
        {
            var compilationOptions = (CSharpCompilationOptions)CompilationOptions.EnsureEnabled(descriptor);

            return WithCompilationOptions(compilationOptions);
        }

        internal CSharpProjectOptions WithEnabled(DiagnosticDescriptor descriptor1, DiagnosticDescriptor descriptor2)
        {
            ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions = CompilationOptions.SpecificDiagnosticOptions;

            diagnosticOptions = diagnosticOptions
                .SetItem(descriptor1.Id, descriptor1.DefaultSeverity.ToReportDiagnostic())
                .SetItem(descriptor2.Id, descriptor2.DefaultSeverity.ToReportDiagnostic());

            CSharpCompilationOptions compilationOptions = CompilationOptions.WithSpecificDiagnosticOptions(diagnosticOptions);

            return WithCompilationOptions(compilationOptions);
        }

        internal CSharpProjectOptions WithDisabled(DiagnosticDescriptor descriptor)
        {
            var compilationOptions = (CSharpCompilationOptions)CompilationOptions.EnsureSuppressed(descriptor);

            return WithCompilationOptions(compilationOptions);
        }
#pragma warning disable CS1591
        public CSharpProjectOptions WithAllowedCompilerDiagnosticIds(IEnumerable<string> allowedCompilerDiagnosticIds)
        {
            return new CSharpProjectOptions(
                compilationOptions: CompilationOptions,
                parseOptions: ParseOptions,
                metadataReferences: MetadataReferences,
                allowedCompilerDiagnosticSeverity: AllowedCompilerDiagnosticSeverity,
                allowedCompilerDiagnosticIds: allowedCompilerDiagnosticIds);
        }

        public CSharpProjectOptions WithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity allowedCompilerDiagnosticSeverity)
        {
            return new CSharpProjectOptions(
                compilationOptions: CompilationOptions,
                parseOptions: ParseOptions,
                metadataReferences: MetadataReferences,
                allowedCompilerDiagnosticSeverity: allowedCompilerDiagnosticSeverity,
                allowedCompilerDiagnosticIds: AllowedCompilerDiagnosticIds);
        }

        public CSharpProjectOptions WithParseOptions(CSharpParseOptions parseOptions)
        {
            return new CSharpProjectOptions(
                compilationOptions: CompilationOptions,
                parseOptions: parseOptions,
                metadataReferences: MetadataReferences,
                allowedCompilerDiagnosticSeverity: AllowedCompilerDiagnosticSeverity,
                allowedCompilerDiagnosticIds: AllowedCompilerDiagnosticIds);
        }

        public CSharpProjectOptions WithCompilationOptions(CSharpCompilationOptions compilationOptions)
        {
            return new CSharpProjectOptions(
                compilationOptions: compilationOptions,
                parseOptions: ParseOptions,
                metadataReferences: MetadataReferences,
                allowedCompilerDiagnosticSeverity: AllowedCompilerDiagnosticSeverity,
                allowedCompilerDiagnosticIds: AllowedCompilerDiagnosticIds);
        }

        public CSharpProjectOptions WithMetadataReferences(IEnumerable<MetadataReference> metadataReferences)
        {
            return new CSharpProjectOptions(
                compilationOptions: CompilationOptions,
                parseOptions: ParseOptions,
                metadataReferences: metadataReferences,
                allowedCompilerDiagnosticSeverity: AllowedCompilerDiagnosticSeverity,
                allowedCompilerDiagnosticIds: AllowedCompilerDiagnosticIds);
        }
#pragma warning restore CS1591
    }
}
