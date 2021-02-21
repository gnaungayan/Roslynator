﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    public sealed class CompilerDiagnosticFixTestState : TestState
    {
        public CompilerDiagnosticFixTestState(string diagnosticId, string source, string expectedSource)
            : this(diagnosticId, source, expectedSource, null, null, null)
        {
        }

        public CompilerDiagnosticFixTestState(
            string diagnosticId,
            string source,
            string expectedSource,
            IEnumerable<AdditionalFile> additionalFiles,
            string codeActionTitle,
            string equivalenceKey) : base(source, expectedSource, additionalFiles, codeActionTitle, equivalenceKey)
        {
            DiagnosticId = diagnosticId;
        }

        private CompilerDiagnosticFixTestState(CompilerDiagnosticFixTestState other)
            : this(
                diagnosticId: other.DiagnosticId,
                source: other.Source,
                expectedSource: other.ExpectedSource,
                additionalFiles: other.AdditionalFiles,
                codeActionTitle: other.CodeActionTitle,
                equivalenceKey: other.EquivalenceKey)
        {
        }

        public string DiagnosticId { get; private set; }

        public CompilerDiagnosticFixTestState Update(
            string diagnosticId,
            string source,
            string expectedSource,
            IEnumerable<AdditionalFile> additionalFiles,
            string codeActionTitle,
            string equivalenceKey)
        {
            return new CompilerDiagnosticFixTestState(
                diagnosticId: diagnosticId,
                source: source,
                expectedSource: expectedSource,
                additionalFiles: additionalFiles,
                codeActionTitle: codeActionTitle,
                equivalenceKey: equivalenceKey);
        }

        internal CompilerDiagnosticFixTestState MaybeUpdate(
            string diagnosticId = null,
            string source = null,
            string expectedSource = null,
            IEnumerable<AdditionalFile> additionalFiles = null,
            string codeActionTitle = null,
            string equivalenceKey = null)
        {
            return new CompilerDiagnosticFixTestState(
                diagnosticId: diagnosticId ?? DiagnosticId,
                source: source ?? Source,
                expectedSource: expectedSource ?? ExpectedSource,
                additionalFiles: additionalFiles ?? AdditionalFiles,
                codeActionTitle: codeActionTitle ?? CodeActionTitle,
                equivalenceKey: equivalenceKey ?? EquivalenceKey);
        }

        protected override TestState CommonWithSource(string source) => WithSource(source);

        protected override TestState CommonWithExpectedSource(string expectedSource) => WithExpectedSource(expectedSource);

        protected override TestState CommonWithAdditionalFiles(IEnumerable<AdditionalFile> additionalFiles) => WithAdditionalFiles(additionalFiles);

        protected override TestState CommonWithCodeActionTitle(string codeActionTitle) => WithCodeActionTitle(codeActionTitle);

        protected override TestState CommonWithEquivalenceKey(string equivalenceKey) => WithEquivalenceKey(equivalenceKey);

        public CompilerDiagnosticFixTestState WithDiagnosticId(string diagnosticId)
        {
            return new CompilerDiagnosticFixTestState(this) { DiagnosticId = diagnosticId };
        }

        new public CompilerDiagnosticFixTestState WithSource(string source)
        {
            return new CompilerDiagnosticFixTestState(this) { Source = source };
        }

        new public CompilerDiagnosticFixTestState WithExpectedSource(string expectedSource)
        {
            return new CompilerDiagnosticFixTestState(this) { ExpectedSource = expectedSource };
        }

        new public CompilerDiagnosticFixTestState WithAdditionalFiles(IEnumerable<AdditionalFile> additionalFiles)
        {
            return new CompilerDiagnosticFixTestState(this) { AdditionalFiles = additionalFiles?.ToImmutableArray() ?? ImmutableArray<AdditionalFile>.Empty };
        }

        new public CompilerDiagnosticFixTestState WithCodeActionTitle(string codeActionTitle)
        {
            return new CompilerDiagnosticFixTestState(this) { CodeActionTitle = codeActionTitle };
        }

        new public CompilerDiagnosticFixTestState WithEquivalenceKey(string equivalenceKey)
        {
            return new CompilerDiagnosticFixTestState(this) { EquivalenceKey = equivalenceKey };
        }
    }
}