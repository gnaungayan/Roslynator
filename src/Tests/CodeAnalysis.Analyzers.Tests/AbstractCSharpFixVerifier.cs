﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Roslynator.Testing.CSharp;

namespace Roslynator.CodeAnalysis.CSharp.Tests
{
    public abstract class AbstractCSharpFixVerifier : CSharpFixVerifier
    {
        protected AbstractCSharpFixVerifier() : base(XunitAssert.Instance)
        {
        }
    }
}
