// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Roslynator.Testing
{
    public sealed class VisualBasicProjectOptions : ProjectOptions
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

        public override string DocumentName => "Test.vb";

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
            var parseOptions = new VisualBasicParseOptions(LanguageVersion.Default);

            var compilationOptions = new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            return new VisualBasicProjectOptions(
                compilationOptions: compilationOptions,
                parseOptions: parseOptions,
                metadataReferences: RuntimeMetadataReference.MetadataReferences.Select(f => f.Value).ToImmutableArray());
        }

        /// <summary>
        /// Adds specified assembly name to the list of assembly names.
        /// </summary>
        /// <param name="metadataReference"></param>
        public VisualBasicProjectOptions AddMetadataReference(MetadataReference metadataReference)
        {
            return WithMetadataReferences(MetadataReferences.Add(metadataReference));
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
