// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Roslynator.Testing
{
    public class VisualBasicProjectOptions : ProjectOptions
    {
        public VisualBasicProjectOptions(
            VisualBasicCompilationOptions compilationOptions,
            VisualBasicParseOptions parseOptions,
            IEnumerable<MetadataReference> metadataReferences)
            : base(metadataReferences)
        {
            CompilationOptions = compilationOptions;
            ParseOptions = parseOptions;
        }

        public override string Language => LanguageNames.VisualBasic;

        public override string DefaultDocumentName => "Test.vb";

        new public VisualBasicParseOptions ParseOptions { get; }

        new public VisualBasicCompilationOptions CompilationOptions { get; }

        protected override ParseOptions CommonParseOptions => ParseOptions;

        protected override CompilationOptions CommonCompilationOptions => CompilationOptions;

        /// <summary>
        /// Gets a default code verification options.
        /// </summary>
        public static VisualBasicProjectOptions Default { get; } = CreateDefault();

        private static VisualBasicProjectOptions CreateDefault()
        {
            VisualBasicParseOptions parseOptions = null;
            VisualBasicCompilationOptions compilationOptions = null;

            using (var workspace = new AdhocWorkspace())
            {
                Project project = workspace
                    .CurrentSolution
                    .AddProject("TestProject", "TestProject", LanguageNames.VisualBasic);

                compilationOptions = ((VisualBasicCompilationOptions)project.CompilationOptions)
                    .WithOutputKind(OutputKind.DynamicallyLinkedLibrary);

                parseOptions = ((VisualBasicParseOptions)project.ParseOptions);

                parseOptions = parseOptions
                    .WithLanguageVersion(LanguageVersion.Latest);
            }

            return new VisualBasicProjectOptions(
                compilationOptions: compilationOptions,
                parseOptions: parseOptions,
                metadataReferences: RuntimeMetadataReference.MetadataReferences.Select(f => f.Value).ToImmutableArray());
        }

        /// <summary>
        /// Adds specified assembly name to the list of assembly names.
        /// </summary>
        /// <param name="metadataReference"></param>
        public VisualBasicProjectOptions AddMetadataReferences(MetadataReference metadataReference)
        {
            return WithMetadataReferences(MetadataReferences.Add(metadataReference));
        }

        internal VisualBasicProjectOptions WithEnabled(DiagnosticDescriptor descriptor)
        {
            var compilationOptions = (VisualBasicCompilationOptions)CompilationOptions.EnsureEnabled(descriptor);

            return WithCompilationOptions(compilationOptions);
        }

        internal VisualBasicProjectOptions WithEnabled(DiagnosticDescriptor descriptor1, DiagnosticDescriptor descriptor2)
        {
            ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions = CompilationOptions.SpecificDiagnosticOptions;

            diagnosticOptions = diagnosticOptions
                .SetItem(descriptor1.Id, descriptor1.DefaultSeverity.ToReportDiagnostic())
                .SetItem(descriptor2.Id, descriptor2.DefaultSeverity.ToReportDiagnostic());

            VisualBasicCompilationOptions compilationOptions = CompilationOptions.WithSpecificDiagnosticOptions(diagnosticOptions);

            return WithCompilationOptions(compilationOptions);
        }

        internal VisualBasicProjectOptions WithDisabled(DiagnosticDescriptor descriptor)
        {
            var compilationOptions = (VisualBasicCompilationOptions)CompilationOptions.EnsureSuppressed(descriptor);

            return WithCompilationOptions(compilationOptions);
        }
#pragma warning disable CS1591

        public VisualBasicProjectOptions WithParseOptions(VisualBasicParseOptions parseOptions)
        {
            return new VisualBasicProjectOptions(
                compilationOptions: CompilationOptions,
                parseOptions: parseOptions,
                metadataReferences: MetadataReferences);
        }

        public VisualBasicProjectOptions WithCompilationOptions(VisualBasicCompilationOptions compilationOptions)
        {
            return new VisualBasicProjectOptions(
                compilationOptions: compilationOptions,
                parseOptions: ParseOptions,
                metadataReferences: MetadataReferences);
        }

        public VisualBasicProjectOptions WithMetadataReferences(IEnumerable<MetadataReference> metadataReferences)
        {
            return new VisualBasicProjectOptions(
                compilationOptions: CompilationOptions,
                parseOptions: ParseOptions,
                metadataReferences: metadataReferences);
        }
#pragma warning restore CS1591
    }
}
