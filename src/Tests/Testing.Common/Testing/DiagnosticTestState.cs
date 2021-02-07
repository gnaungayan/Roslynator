// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    public sealed class DiagnosticTestState : TestState
    {
        internal static DiagnosticTestState Empty { get; } = new DiagnosticTestState(null, null, null);

        public DiagnosticTestState(string source, string expectedSource, IEnumerable<Diagnostic> diagnostics)
            : this(source, expectedSource, diagnostics, null, null, null, null, null)
        {
        }

        public DiagnosticTestState(
            string source,
            string expectedSource,
            IEnumerable<Diagnostic> diagnostics,
            IEnumerable<AdditionalFile> additionalFiles,
            string message,
            IFormatProvider formatProvider,
            string codeActionTitle,
            string equivalenceKey) : base(source, expectedSource, additionalFiles, codeActionTitle, equivalenceKey)
        {
            Diagnostics = diagnostics?.ToImmutableArray() ?? ImmutableArray<Diagnostic>.Empty;
            Message = message;
            FormatProvider = formatProvider;
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; private set; }

        public string Message { get; private set; }

        public IFormatProvider FormatProvider { get; private set; }

        private DiagnosticTestState(DiagnosticTestState other)
            : this(
                source: other.Source,
                expectedSource: other.ExpectedSource,
                diagnostics: other.Diagnostics,
                additionalFiles: other.AdditionalFiles,
                message: other.Message,
                formatProvider: other.FormatProvider,
                codeActionTitle: other.CodeActionTitle,
                equivalenceKey: other.EquivalenceKey)
        {
        }

        public DiagnosticTestState Update(
            string source,
            string expectedSource,
            IEnumerable<Diagnostic> diagnostics,
            IEnumerable<AdditionalFile> additionalFiles,
            string message,
            IFormatProvider formatProvider,
            string codeActionTitle,
            string equivalenceKey)
        {
            return new DiagnosticTestState(
                source: source,
                expectedSource: expectedSource,
                diagnostics: diagnostics,
                additionalFiles: additionalFiles,
                message: message,
                formatProvider: formatProvider,
                codeActionTitle: codeActionTitle,
                equivalenceKey: equivalenceKey);
        }

        public DiagnosticTestState MaybeUpdate(
            string source = null,
            string expectedSource = null,
            IEnumerable<Diagnostic> diagnostics = null,
            IEnumerable<AdditionalFile> additionalFiles = null,
            string message = null,
            IFormatProvider formatProvider = null,
            string codeActionTitle = null,
            string equivalenceKey = null)
        {
            return new DiagnosticTestState(
                source: source ?? Source,
                expectedSource: expectedSource ?? ExpectedSource,
                diagnostics: diagnostics ?? Diagnostics,
                additionalFiles: additionalFiles ?? AdditionalFiles,
                message: message ?? Message,
                formatProvider: formatProvider ?? FormatProvider,
                codeActionTitle: codeActionTitle ?? CodeActionTitle,
                equivalenceKey: equivalenceKey ?? EquivalenceKey);
        }

        protected override TestState CommonWithSource(string source)
        {
            return WithSource(source);
        }

        protected override TestState CommonWithExpectedSource(string expectedSource)
        {
            return WithExpectedSource(expectedSource);
        }

        protected override TestState CommonWithAdditionalFiles(IEnumerable<AdditionalFile> additionalFiles)
        {
            return WithAdditionalFiles(additionalFiles);
        }

        protected override TestState CommonWithCodeActionTitle(string codeActionTitle)
        {
            return WithCodeActionTitle(codeActionTitle);
        }

        protected override TestState CommonWithEquivalenceKey(string equivalenceKey)
        {
            return WithEquivalenceKey(equivalenceKey);
        }

        new public DiagnosticTestState WithSource(string source)
        {
            return new DiagnosticTestState(this) { Source = source };
        }

        new public DiagnosticTestState WithExpectedSource(string expectedSource)
        {
            return new DiagnosticTestState(this) { ExpectedSource = expectedSource };
        }

        public DiagnosticTestState WithDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
            return new DiagnosticTestState(this) { Diagnostics = diagnostics?.ToImmutableArray() ?? ImmutableArray<Diagnostic>.Empty };
        }

        new public DiagnosticTestState WithAdditionalFiles(IEnumerable<AdditionalFile> additionalFiles)
        {
            return new DiagnosticTestState(this) { AdditionalFiles = additionalFiles?.ToImmutableArray() ?? ImmutableArray<AdditionalFile>.Empty };
        }

        public DiagnosticTestState WithMessage(string message)
        {
            return new DiagnosticTestState(this) { Message = message };
        }

        public DiagnosticTestState WithFormatProvider(IFormatProvider formatProvider)
        {
            return new DiagnosticTestState(this) { FormatProvider = formatProvider };
        }

        new public DiagnosticTestState WithCodeActionTitle(string codeActionTitle)
        {
            return new DiagnosticTestState(this) { CodeActionTitle = codeActionTitle };
        }

        new public DiagnosticTestState WithEquivalenceKey(string equivalenceKey)
        {
            return new DiagnosticTestState(this) { EquivalenceKey = equivalenceKey };
        }
    }
}
