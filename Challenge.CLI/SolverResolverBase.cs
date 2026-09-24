using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Challenge.Solvers;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Challenge.CLI;

/// <summary>
/// SolverResolver base implementation
/// </summary>
/// <param name="logger">Logger instance</param>
[PublicAPI]
public abstract class SolverResolverBase(ILogger logger) : ISolverResolver
{
    /// <summary>
    /// Input folder name
    /// </summary>
    public const string INPUT_FOLDER = "Input";

    /// <summary>
    /// Session cookie file
    /// </summary>
    public static string SettingsPath { get; } = Path.Combine(INPUT_FOLDER, "settings.json");

    /// <summary>
    /// Logger instance
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <inheritdoc />
    public abstract string ChallengeName { get; }

    /// <inheritdoc />
    public abstract Task<Result<string>> FetchInput(SolverData data, CancellationToken token = default);

    /// <inheritdoc />
    public abstract Task<Result> SubmitAnswer(string answer, SolverData data, CancellationToken token = default);

    /// <inheritdoc />
    public abstract Solver? GetSolver(uint year, uint day);
}

/// <summary>
/// SolverResolver base implementation
/// </summary>
/// <param name="logger">Logger instance</param>
/// <param name="settings">Resolver settings</param>
[PublicAPI]
public abstract partial class SolverResolverBase<T>(ILogger logger, T settings) : SolverResolverBase(logger)
    where T : ResolverSettings
{
    /// <summary>
    /// Minimum time between API requests
    /// </summary>
    protected abstract TimeSpan RateLimit { get; }

    /// <summary>
    /// Settings Json type info
    /// </summary>
    protected abstract JsonTypeInfo<T> SettingsTypeInfo { get; }

    /// <summary>
    /// Resolver settings
    /// </summary>
    protected T Settings { get; } = settings;

    /// <inheritdoc />
    public sealed override async Task<Result<string>> FetchInput(SolverData data, CancellationToken token = default)
    {
        // Check for the input file
        FileInfo inputFile = new(GetInputFileName(data));
        if (inputFile.Exists)
        {
            // Read input from file
            using StreamReader reader = inputFile.OpenText();
            string input = await reader.ReadToEndAsync(token).ConfigureAwait(false);
            LogCachedInputLoaded(this.Logger, inputFile.FullName);
            return input;
        }

        // Make sure the directory exists
        if (inputFile.Directory is { Exists: false })
        {
            inputFile.Directory.Create();
        }

        // Validate rate limit
        TimeSpan timeSinceLastRequest = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(this.Settings.LastRequestTimestamp);
        if (timeSinceLastRequest.TotalSeconds < this.RateLimit.TotalSeconds)
        {
            LogRateLimited(this.Logger, timeSinceLastRequest.TotalSeconds, this.RateLimit.TotalSeconds);
            return Result.Failure<string>("Request rate limited");
        }

        string fetchedInput;
        try
        {
            // Fetch input and write to file
            fetchedInput = await GetInputFromAPI(data, token).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // Return exception description in case of failure
            return Result.Failure<string>(e.Message);
        }

        // Write back settings with new timestamp
        this.Settings.LastRequestTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await SaveSettings(token).ConfigureAwait(false);

        // Write input to file and return
        await using StreamWriter writer = inputFile.CreateText();
        await writer.WriteAsync(fetchedInput).ConfigureAwait(false);
        return fetchedInput;
    }

    /// <summary>
    /// Saves the given settings object to the settings file
    /// </summary>
    /// <param name="token">Cancellation token</param>
    protected async Task SaveSettings(CancellationToken token)
    {
        // Write back settings with new timestamp
        FileInfo settingsFile = new(SettingsPath);
        await using FileStream settingsWriteFileStream = settingsFile.OpenWrite();
        await JsonSerializer.SerializeAsync(settingsWriteFileStream, this.Settings, this.SettingsTypeInfo, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the input file name for the given solver data
    /// </summary>
    /// <param name="data">Solver data</param>
    /// <returns>The file at which the input for this solver should be stored/loaded</returns>
    protected abstract string GetInputFileName(in SolverData data);

    /// <summary>
    /// Fetches the input from the challenge website
    /// </summary>
    /// <param name="data">Solver data</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>The input for the problem</returns>
    protected abstract Task<string> GetInputFromAPI(SolverData data, CancellationToken token);
}

/// <summary>
/// SolverResolver with default settings
/// </summary>
/// <param name="logger">Logger instance</param>
/// <param name="settings">Resolver settings</param>
[PublicAPI]
public abstract class DefaultSolverResolverBase(ILogger logger, ResolverSettings settings) : SolverResolverBase<ResolverSettings>(logger, settings)
{
    /// <inheritdoc />
    protected sealed override JsonTypeInfo<ResolverSettings> SettingsTypeInfo => ResolverSettingsJsonContext.Default.ResolverSettings;
}
