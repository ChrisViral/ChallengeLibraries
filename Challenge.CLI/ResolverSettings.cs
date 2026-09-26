using System.Text.Json.Serialization.Metadata;
using JetBrains.Annotations;

namespace Challenge.CLI;

/// <summary>
/// Resolver settings interface
/// </summary>
/// <typeparam name="TSelf">Self type</typeparam>
[PublicAPI]
public interface IResolverSettings<TSelf> where TSelf : class, IResolverSettings<TSelf>
{
    /// <summary>
    /// Default settings value
    /// </summary>
    static abstract TSelf Default { get; }

    /// <summary>
    /// Settings JSON type info
    /// </summary>
    static abstract JsonTypeInfo<TSelf> SettingsTypeInfo { get; }
}

/// <summary>
/// Base resolver settings
/// </summary>
/// <param name="Cookie">Request cookie</param>
/// <param name="LastRequestTimestamp">Last request timestamp</param>
[PublicAPI]
public abstract record ResolverSettings(string Cookie, long LastRequestTimestamp);
