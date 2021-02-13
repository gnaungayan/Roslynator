// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    public abstract class ProjectOptions
    {
        protected ProjectOptions(
            IEnumerable<MetadataReference> metadataReferences,
            DiagnosticSeverity allowedCompilerDiagnosticSeverity = DiagnosticSeverity.Info,
            IEnumerable<string> allowedCompilerDiagnosticIds = null)
        {
            MetadataReferences = metadataReferences?.ToImmutableArray() ?? ImmutableArray<MetadataReference>.Empty;
            AllowedCompilerDiagnosticSeverity = allowedCompilerDiagnosticSeverity;
            AllowedCompilerDiagnosticIds = allowedCompilerDiagnosticIds?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
        }

        public abstract string Language { get; }

        public abstract string DefaultDocumentName { get; }

        /// <summary>
        /// Gets a common parse options.
        /// </summary>
        protected abstract ParseOptions CommonParseOptions { get; }

        /// <summary>
        /// Gets a common compilation options.
        /// </summary>
        protected abstract CompilationOptions CommonCompilationOptions { get; }

        /// <summary>
        /// Gets a parse options that should be used to parse tested source code.
        /// </summary>
        public ParseOptions ParseOptions => CommonParseOptions;

        /// <summary>
        /// Gets a compilation options that should be used to compile test project.
        /// </summary>
        public CompilationOptions CompilationOptions => CommonCompilationOptions;

        public ImmutableArray<MetadataReference> MetadataReferences { get; }

        //TODO: del
        /// <summary>
        /// Gets a diagnostic severity that is allowed. Default value is <see cref="DiagnosticSeverity.Info"/>
        /// which means that compiler diagnostics with severity <see cref="DiagnosticSeverity.Hidden"/>
        /// and <see cref="DiagnosticSeverity.Info"/> are allowed.
        /// </summary>
        public DiagnosticSeverity AllowedCompilerDiagnosticSeverity { get; }

        //TODO: del
        /// <summary>
        /// Gets a list of compiler diagnostic IDs that are allowed.
        /// </summary>
        public ImmutableArray<string> AllowedCompilerDiagnosticIds { get; }
    }
}
