// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    public class TestOptions
    {
        internal static TestOptions Default { get; } = new TestOptions(
            DiagnosticSeverity.Info,
            ImmutableArray<string>.Empty,
            ImmutableDictionary<string, ReportDiagnostic>.Empty);

        public TestOptions(
            DiagnosticSeverity allowedCompilerDiagnosticSeverity,
            ImmutableArray<string> allowedCompilerDiagnosticIds,
            IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions)
        {
            AllowedCompilerDiagnosticSeverity = allowedCompilerDiagnosticSeverity;
            AllowedCompilerDiagnosticIds = allowedCompilerDiagnosticIds;
            SpecificDiagnosticOptions = specificDiagnosticOptions?.ToImmutableDictionary() ?? ImmutableDictionary<string, ReportDiagnostic>.Empty;
        }

        public TestOptions(TestOptions other)
            : this(
                allowedCompilerDiagnosticSeverity: other.AllowedCompilerDiagnosticSeverity,
                allowedCompilerDiagnosticIds: other.AllowedCompilerDiagnosticIds,
                specificDiagnosticOptions: other.SpecificDiagnosticOptions)
        {
        }

        public DiagnosticSeverity AllowedCompilerDiagnosticSeverity { get; private set; }

        public ImmutableArray<string> AllowedCompilerDiagnosticIds { get; private set; }

        public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; private set; }

        /// <summary>
        /// Adds specified compiler diagnostic ID to the list of allowed compiler diagnostic IDs.
        /// </summary>
        /// <param name="diagnosticId"></param>
        public TestOptions AddAllowedCompilerDiagnosticId(string diagnosticId)
        {
            return WithAllowedCompilerDiagnosticIds(AllowedCompilerDiagnosticIds.Add(diagnosticId));
        }

        /// <summary>
        /// Adds a list of specified compiler diagnostic IDs to the list of allowed compiler diagnostic IDs.
        /// </summary>
        /// <param name="diagnosticIds"></param>
        public TestOptions AddAllowedCompilerDiagnosticIds(IEnumerable<string> diagnosticIds)
        {
            return WithAllowedCompilerDiagnosticIds(AllowedCompilerDiagnosticIds.AddRange(diagnosticIds));
        }

        public TestOptions EnableDiagnostic(DiagnosticDescriptor descriptor)
        {
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = SpecificDiagnosticOptions.SetItem(
                descriptor.Id,
                descriptor.DefaultSeverity.ToReportDiagnostic());

            return WithSpecificDiagnosticOptions(specificDiagnosticOptions);
        }

        internal TestOptions EnableDiagnostic(DiagnosticDescriptor descriptor1, DiagnosticDescriptor descriptor2)
        {
            ImmutableDictionary<string, ReportDiagnostic> options = SpecificDiagnosticOptions;

            options = options
                .SetItem(descriptor1.Id, descriptor1.DefaultSeverity.ToReportDiagnostic())
                .SetItem(descriptor2.Id, descriptor2.DefaultSeverity.ToReportDiagnostic());

            return WithSpecificDiagnosticOptions(options);
        }

        public TestOptions DisableDiagnostic(DiagnosticDescriptor descriptor)
        {
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = SpecificDiagnosticOptions.SetItem(
                descriptor.Id,
                ReportDiagnostic.Suppress);

            return WithSpecificDiagnosticOptions(specificDiagnosticOptions);
        }

        public TestOptions WithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity allowedCompilerDiagnosticSeverity)
        {
            return new TestOptions(this) { AllowedCompilerDiagnosticSeverity = allowedCompilerDiagnosticSeverity };
        }

        public TestOptions WithAllowedCompilerDiagnosticIds(IEnumerable<string> allowedCompilerDiagnosticIds)
        {
            return new TestOptions(this) { AllowedCompilerDiagnosticIds = allowedCompilerDiagnosticIds?.ToImmutableArray() ?? ImmutableArray<string>.Empty };
        }

        public TestOptions WithSpecificDiagnosticOptions(IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions)
        {
            return new TestOptions(this) { SpecificDiagnosticOptions = specificDiagnosticOptions?.ToImmutableDictionary() ?? ImmutableDictionary<string, ReportDiagnostic>.Empty };
        }
    }
}
