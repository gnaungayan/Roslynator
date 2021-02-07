// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Roslynator.Testing;

namespace Roslynator.VisualBasic.Testing
{
    public abstract class VisualBasicDiagnosticVerifier : DiagnosticVerifier
    {
        protected VisualBasicDiagnosticVerifier(IAssert assert) : base(assert)
        {
        }

        new public virtual VisualBasicProjectOptions ProjectOptions => VisualBasicProjectOptions.Default;

        protected override ProjectOptions CommonProjectOptions => ProjectOptions;
    }
}
