using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Challenge.CLI;

/// <summary>
/// <see cref="ResolverSettings"/> JSON source generation context
/// </summary>
[PublicAPI, JsonSerializable(typeof(ResolverSettings)), JsonSourceGenerationOptions(WriteIndented = true)]
public sealed partial class ResolverSettingsJsonContext : JsonSerializerContext;

/// <summary>
/// Resolver settings
/// </summary>
/// <param name="Cookie">Request cookie</param>
/// <param name="LastRequestTimestamp">Last request timestamp</param>
[PublicAPI]
[method: JsonConstructor]
public record ResolverSettings(string Cookie, long LastRequestTimestamp)
{
    /// <summary>
    /// Last request timestamp
    /// </summary>
    public long LastRequestTimestamp { get; set; } = LastRequestTimestamp;
}
