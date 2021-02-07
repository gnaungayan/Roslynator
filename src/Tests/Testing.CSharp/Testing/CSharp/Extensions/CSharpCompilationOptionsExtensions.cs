// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslynator.Testing.CSharp
{
    internal static class CSharpCompilationOptionsExtensions
    {
        public static CSharpCompilationOptions EnsureEnabled(this CSharpCompilationOptions compilationOptions, DiagnosticDescriptor descriptor)
        {
            return SetSeverity(compilationOptions, descriptor, descriptor.DefaultSeverity.ToReportDiagnostic());
        }

        private static CSharpCompilationOptions SetSeverity(
            this CSharpCompilationOptions compilationOptions,
            DiagnosticDescriptor descriptor,
            ReportDiagnostic reportDiagnostic)
        {
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = compilationOptions.SpecificDiagnosticOptions;

            specificDiagnosticOptions = specificDiagnosticOptions.SetItem(
                descriptor.Id,
                reportDiagnostic);

            return compilationOptions.WithSpecificDiagnosticOptions(specificDiagnosticOptions);
        }
    }
}
