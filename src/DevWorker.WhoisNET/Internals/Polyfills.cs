// Polyfills for netstandard2.0 and net6.0 to support modern C# language features.

#if !NET7_0_OR_GREATER

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal static class IsExternalInit { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName) => FeatureName = featureName;
        public string FeatureName { get; }
        public bool IsOptional { get; init; }
    }
}

// ReSharper disable once CheckNamespace
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : Attribute { }
}

#endif

#if NETSTANDARD2_0

// ReSharper disable once CheckNamespace
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;
        public bool ReturnValue { get; }
    }
}

// ReSharper disable once CheckNamespace
namespace DevWorker.WhoisNET.Internals
{
    internal static class StringPolyfills
    {
        internal static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source.IndexOf(value, comparisonType) >= 0;
        }

        internal static bool StartsWith(this string source, char value)
        {
            return source.Length > 0 && source[0] == value;
        }

        internal static bool EndsWith(this string source, char value)
        {
            return source.Length > 0 && source[source.Length - 1] == value;
        }
    }
}

#endif
