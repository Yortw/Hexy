// Licensed to the .NET Foundation under one or more agreements.
// Polyfill attributes for netstandard2.0 that are built-in on .NET 5+.
// The C# compiler recognizes these by namespace-qualified name, so
// defining them internally is sufficient for flow analysis.

#if !NET5_0_OR_GREATER

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Applied to a method that will never return under any circumstance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class DoesNotReturnAttribute : Attribute { }
}

#endif
