// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Roslynator.Testing
{
    public sealed class VisualBasicTestOptions : TestOptions
    {
        public VisualBasicTestOptions(
            VisualBasicCompilationOptions compilationOptions,
            VisualBasicParseOptions parseOptions,
            DiagnosticSeverity allowedCompilerDiagnosticSeverity,
            IEnumerable<string> allowedCompilerDiagnosticIds,
            IEnumerable<MetadataReference> metadataReferences)
            : base(allowedCompilerDiagnosticSeverity, allowedCompilerDiagnosticIds, metadataReferences)
        {
            CompilationOptions = compilationOptions;
            ParseOptions = parseOptions;
        }

        private VisualBasicTestOptions(VisualBasicTestOptions other)
            : base(
                other.AllowedCompilerDiagnosticSeverity,
                other.AllowedCompilerDiagnosticIds,
                other.MetadataReferences)
        {
            CompilationOptions = other.CompilationOptions;
            ParseOptions = other.ParseOptions;
        }

        public override string Language => LanguageNames.VisualBasic;

        public override string DocumentName => "Test.vb";

        new public VisualBasicParseOptions ParseOptions { get; private set; }

        new public VisualBasicCompilationOptions CompilationOptions { get; private set; }

        protected override ParseOptions CommonParseOptions => ParseOptions;

        protected override CompilationOptions CommonCompilationOptions => CompilationOptions;

        /// <summary>
        /// Gets a default code verification options.
        /// </summary>
        public static VisualBasicTestOptions Default { get; } = CreateDefault();

        private static VisualBasicTestOptions CreateDefault()
        {
            var parseOptions = new VisualBasicParseOptions(LanguageVersion.Default);

            var compilationOptions = new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            return new VisualBasicTestOptions(
                compilationOptions: compilationOptions,
                parseOptions: parseOptions,
                allowedCompilerDiagnosticSeverity: DiagnosticSeverity.Info,
                allowedCompilerDiagnosticIds: null,
                metadataReferences: RuntimeMetadataReference.MetadataReferences.Select(f => f.Value).ToImmutableArray());
        }

        /// <summary>
        /// Adds specified assembly name to the list of assembly names.
        /// </summary>
        /// <param name="metadataReference"></param>
        public VisualBasicTestOptions AddMetadataReference(MetadataReference metadataReference)
        {
            return WithMetadataReferences(MetadataReferences.Add(metadataReference));
        }

#pragma warning disable CS1591
        protected override TestOptions CommonWithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity value)
        {
            return new VisualBasicTestOptions(this) { AllowedCompilerDiagnosticSeverity = value };
        }

        protected override TestOptions CommonWithAllowedCompilerDiagnosticIds(IEnumerable<string> values)
        {
            return new VisualBasicTestOptions(this) { AllowedCompilerDiagnosticIds = values?.ToImmutableArray() ?? ImmutableArray<string>.Empty };
        }

        protected override TestOptions CommonWithMetadataReferences(IEnumerable<MetadataReference> values)
        {
            return new VisualBasicTestOptions(this) { MetadataReferences = values?.ToImmutableArray() ?? ImmutableArray<MetadataReference>.Empty };
        }

        new public VisualBasicTestOptions WithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity value)
        {
            return (VisualBasicTestOptions)base.WithAllowedCompilerDiagnosticSeverity(value);
        }

        new public VisualBasicTestOptions WithAllowedCompilerDiagnosticIds(IEnumerable<string> values)
        {
            return (VisualBasicTestOptions)base.WithAllowedCompilerDiagnosticIds(values);
        }

        new public VisualBasicTestOptions WithMetadataReferences(IEnumerable<MetadataReference> values)
        {
            return (VisualBasicTestOptions)base.WithMetadataReferences(values);
        }

        public VisualBasicTestOptions WithParseOptions(VisualBasicParseOptions parseOptions)
        {
            return new VisualBasicTestOptions(this) { ParseOptions = parseOptions ?? throw new ArgumentNullException(nameof(parseOptions)) };
        }

        public VisualBasicTestOptions WithCompilationOptions(VisualBasicCompilationOptions compilationOptions)
        {
            return new VisualBasicTestOptions(this) { CompilationOptions = compilationOptions ?? throw new ArgumentNullException(nameof(compilationOptions)) };
        }
#pragma warning restore CS1591
    }
}
