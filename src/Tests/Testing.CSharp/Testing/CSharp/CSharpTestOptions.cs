// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

#pragma warning disable RCS1223

namespace Roslynator.Testing.CSharp
{
    public sealed class CSharpTestOptions : TestOptions
    {
        public CSharpTestOptions(
            CSharpCompilationOptions compilationOptions,
            CSharpParseOptions parseOptions,
            DiagnosticSeverity allowedCompilerDiagnosticSeverity,
            IEnumerable<string> allowedCompilerDiagnosticIds,
            IEnumerable<MetadataReference> metadataReferences)
            : base(allowedCompilerDiagnosticSeverity, allowedCompilerDiagnosticIds, metadataReferences)
        {
            CompilationOptions = compilationOptions;
            ParseOptions = parseOptions;
        }

        private CSharpTestOptions(CSharpTestOptions other)
            : base(
                other.AllowedCompilerDiagnosticSeverity,
                other.AllowedCompilerDiagnosticIds,
                other.MetadataReferences)
        {
            CompilationOptions = other.CompilationOptions;
            ParseOptions = other.ParseOptions;
        }

        public override string Language => LanguageNames.CSharp;

        public override string DocumentName => "Test.cs";

        /// <summary>
        /// Gets a parse options that should be used to parse tested source code.
        /// </summary>
        new public CSharpParseOptions ParseOptions { get; private set; }

        /// <summary>
        /// Gets a compilation options that should be used to compile test project.
        /// </summary>
        new public CSharpCompilationOptions CompilationOptions { get; private set; }

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
        public static CSharpTestOptions Default { get; } = CreateDefault();

        private static CSharpTestOptions CreateDefault()
        {
            var parseOptions = new CSharpParseOptions(LanguageVersion.LatestMajor);

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            return new CSharpTestOptions(
                compilationOptions: compilationOptions,
                parseOptions: parseOptions,
                allowedCompilerDiagnosticSeverity: DiagnosticSeverity.Info,
                allowedCompilerDiagnosticIds: null,
                metadataReferences: RuntimeMetadataReference.MetadataReferences.Select(f => f.Value).ToImmutableArray()
            );
        }

        /// <summary>
        /// Adds specified compiler diagnostic ID to the list of allowed compiler diagnostic IDs.
        /// </summary>
        /// <param name="diagnosticId"></param>
        public CSharpTestOptions AddAllowedCompilerDiagnosticId(string diagnosticId)
        {
            return WithAllowedCompilerDiagnosticIds(AllowedCompilerDiagnosticIds.Add(diagnosticId));
        }

        /// <summary>
        /// Adds a list of specified compiler diagnostic IDs to the list of allowed compiler diagnostic IDs.
        /// </summary>
        /// <param name="diagnosticIds"></param>
        public CSharpTestOptions AddAllowedCompilerDiagnosticIds(IEnumerable<string> diagnosticIds)
        {
            return WithAllowedCompilerDiagnosticIds(AllowedCompilerDiagnosticIds.AddRange(diagnosticIds));
        }

        public CSharpTestOptions EnableDiagnostic(DiagnosticDescriptor descriptor)
        {
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = CompilationOptions.SpecificDiagnosticOptions.SetItem(
                descriptor.Id,
                descriptor.DefaultSeverity.ToReportDiagnostic());

            return WithSpecificDiagnosticOptions(specificDiagnosticOptions);
        }

        internal CSharpTestOptions EnableDiagnostic(DiagnosticDescriptor descriptor1, DiagnosticDescriptor descriptor2)
        {
            ImmutableDictionary<string, ReportDiagnostic> options = CompilationOptions.SpecificDiagnosticOptions;

            options = options
                .SetItem(descriptor1.Id, descriptor1.DefaultSeverity.ToReportDiagnostic())
                .SetItem(descriptor2.Id, descriptor2.DefaultSeverity.ToReportDiagnostic());

            return WithSpecificDiagnosticOptions(options);
        }

        public CSharpTestOptions DisableDiagnostic(DiagnosticDescriptor descriptor)
        {
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = CompilationOptions.SpecificDiagnosticOptions.SetItem(
                descriptor.Id,
                ReportDiagnostic.Suppress);

            return WithSpecificDiagnosticOptions(specificDiagnosticOptions);
        }

        /// <summary>
        /// Adds specified assembly name to the list of assembly names.
        /// </summary>
        /// <param name="metadataReference"></param>
        internal CSharpTestOptions AddMetadataReference(MetadataReference metadataReference)
        {
            return WithMetadataReferences(MetadataReferences.Add(metadataReference));
        }

        public CSharpTestOptions WithSpecificDiagnosticOptions(IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions)
        {
            return WithCompilationOptions(CompilationOptions.WithSpecificDiagnosticOptions(specificDiagnosticOptions));
        }

        internal CSharpTestOptions WithAllowUnsafe(bool enabled)
        {
            return WithCompilationOptions(CompilationOptions.WithAllowUnsafe(enabled));
        }

        internal CSharpTestOptions WithDebugPreprocessorSymbol()
        {
            return WithParseOptions(
                ParseOptions.WithPreprocessorSymbols(
                    ParseOptions.PreprocessorSymbolNames.Concat(new[] { "DEBUG" })));
        }

#pragma warning disable CS1591
        protected override TestOptions CommonWithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity value)
        {
            return new CSharpTestOptions(this) { AllowedCompilerDiagnosticSeverity = value };
        }

        protected override TestOptions CommonWithAllowedCompilerDiagnosticIds(IEnumerable<string> values)
        {
        return new CSharpTestOptions(this) { AllowedCompilerDiagnosticIds = values?.ToImmutableArray() ?? ImmutableArray<string>.Empty };
        }

        protected override TestOptions CommonWithMetadataReferences(IEnumerable<MetadataReference> values)
        {
            return new CSharpTestOptions(this) { MetadataReferences = values?.ToImmutableArray() ?? ImmutableArray<MetadataReference>.Empty };
        }

        new public CSharpTestOptions WithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity value)
        {
            return (CSharpTestOptions)base.WithAllowedCompilerDiagnosticSeverity(value);
        }

        new public CSharpTestOptions WithAllowedCompilerDiagnosticIds(IEnumerable<string> values)
        {
            return (CSharpTestOptions)base.WithAllowedCompilerDiagnosticIds(values);
        }

        new public CSharpTestOptions WithMetadataReferences(IEnumerable<MetadataReference> values)
        {
            return (CSharpTestOptions)base.WithMetadataReferences(values);
        }

        public CSharpTestOptions WithParseOptions(CSharpParseOptions parseOptions)
        {
            return new CSharpTestOptions(this) { ParseOptions = parseOptions ?? throw new ArgumentNullException(nameof(parseOptions)) };
        }

        public CSharpTestOptions WithCompilationOptions(CSharpCompilationOptions compilationOptions)
        {
            return new CSharpTestOptions(this) { CompilationOptions = compilationOptions ?? throw new ArgumentNullException(nameof(compilationOptions)) };
        }
#pragma warning restore CS1591
    }
}
