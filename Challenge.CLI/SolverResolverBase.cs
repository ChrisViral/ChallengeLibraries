using System.Text.Json;
using Challenge.Solvers;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Challenge.CLI;

/// <summary>
/// SolverResolver base implementation
/// </summary>
/// <param name="logger">Logger instance</param>
/// <param name="settings">Resolver settings</param>
[PublicAPI]
public abstract partial class SolverResolverBase(ILogger logger, ResolverSettings settings) : ISolverResolver
{
    /// <summary>
    /// Input folder name
    /// </summary>
    public const string INPUT_FOLDER = "Input";

    /// <summary>
    /// Session cookie file
    /// </summary>
    public static string SettingsPath { get; } = Path.Combine(INPUT_FOLDER, "settings.json");

    /// <inheritdoc />
    public abstract string ChallengeName { get; }

    /// <summary>
    /// Minimum time between API requests
    /// </summary>
    protected abstract TimeSpan RateLimit { get; }

    /// <summary>
    /// Logger instance
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Resolver settings
    /// </summary>
    protected ResolverSettings Settings { get; } = settings;

    /// <inheritdoc />
    public async Task<Result<string, Exception>> FetchInput(SolverData data, CancellationToken token = default)
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
            return new InvalidOperationException("Request rate limited");
        }

        string fetchedInput;
        try
        {
            // Fetch input and write to file
            fetchedInput = await GetInputFromWebsite(data, token).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // Return exception description in case of failure
            return Result.Failure<string, Exception>(e);
        }

        // Write back settings with new timestamp
        FileInfo settingsFile = new(SettingsPath);
        await using FileStream settingsWriteFileStream = settingsFile.OpenWrite();
        this.Settings.LastRequestTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await JsonSerializer.SerializeAsync(settingsWriteFileStream, this.Settings, ResolverSettingsJsonContext.Default.ResolverSettings, token).ConfigureAwait(false);

        // Write input to file and return
        await using StreamWriter writer = inputFile.CreateText();
        await writer.WriteAsync(fetchedInput).ConfigureAwait(false);
        return fetchedInput;
    }

    /// <inheritdoc />
    public abstract Task<Result> SubmitAnswer(string answer, CancellationToken token = default);

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
    protected abstract Task<string> GetInputFromWebsite(SolverData data, CancellationToken token);
}
