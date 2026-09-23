using DotMake.CommandLine;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Challenge.CLI;

/// <summary>
/// Challenge program setup helper
/// </summary>
/// <typeparam name="T">Settings type</typeparam>
[PublicAPI]
public abstract class Setup<T> : IDisposable
    where T : ResolverSettings
{
    private readonly CancellationTokenSource cancellationSource = new();
    /// <summary> Settings instance </summary>
    protected T settings = null!;

    /// <summary>
    /// Creates a new program setup with the given title
    /// </summary>
    /// <param name="title">Program title</param>
    protected Setup(string title) => Console.Title = title;

    /// <summary>
    /// Tries to setup the data for this program
    /// </summary>
    /// <returns><see langword="true"/> when the setup succeeds, otherwise <see langword="false"/></returns>
    public async Task<bool> TrySetup()
    {
        // Setup cancellation token source
        Console.CancelKeyPress += (_, _) =>
        {
            this.cancellationSource.Cancel();
        };

        // Flush existing results file
        string results = Path.Combine("Output", "results.txt");
        if (File.Exists(results))
        {
            File.Delete(results);
        }

        // Create logger
        LoggerConfiguration configuration = new();
        Log.Logger = configuration.WriteTo.Console()
                                  .WriteTo.File(results)
                                  .Enrich.FromLogContext()
#if DEBUG
                                  .MinimumLevel.Debug()
                                  .MinimumLevel.Override(typeof(HttpClient).FullName!, LogEventLevel.Information)
#else
                                  .MinimumLevel.Information()
                                  .MinimumLevel.Override(typeof(HttpClient).FullName!, LogEventLevel.Warning)
#endif
                                  .CreateLogger();

        // Ensure input directory exists
        if (!Directory.Exists(SolverResolverBase.INPUT_FOLDER))
        {
            Directory.CreateDirectory(SolverResolverBase.INPUT_FOLDER);
        }

        // Check if settings exist
        FileInfo settingsFile = new(SolverResolverBase.SettingsPath);
        if (!settingsFile.Exists)
        {
            // Create empty settings file
            await using (FileStream newSettingsStream = settingsFile.Create())
            {
                await CreateDefaultSettings(newSettingsStream, this.cancellationSource.Token).ConfigureAwait(false);
            }

            // Prompt user to add cookie to file
            Log.Error("Could not find the settings file, please add your cookie and to the generated file\n{FileName}", settingsFile.FullName);
            return false;
        }

        // Get settings
        T? loadedSettings;
        await using (FileStream settingsStream = settingsFile.OpenRead())
        {
            loadedSettings = await GetSettings(settingsStream, this.cancellationSource.Token).ConfigureAwait(false);
        }

        if (loadedSettings is null)
        {
            Log.Error("Could not deserialize settings file {FileName}", settingsFile.FullName);
            return false;
        }

        this.settings = loadedSettings;
        Cli.Ext.ConfigureServices(ConfigureServices);
        return true;
    }

    /// <summary>
    /// Runs the program with the given arguments
    /// </summary>
    /// <param name="args">Arguments to run the program with</param>
    /// <returns>The program's return code</returns>
    public async Task<int> RunProgram(string[] args)
    {
        // Default args
        if (args is [])
        {
            args = ["-h"];
        }

#if !DEBUG
        // Don't wrap on debug to allow breakpoints
        return await Cli.RunAsync<ChallengeCommand>(args, cancellationToken: cancellationSource.Token).ConfigureAwait(false);
#else
        try
        {
            // Try running the command
            return await Cli.RunAsync<ChallengeCommand>(args, cancellationToken: this.cancellationSource.Token).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // Log exceptions
            Log.Error(e, "An error occured while executing the command");
            return 1;
        }
#endif
    }

    /// <summary>
    /// Creates the default, uninitialized settings file
    /// </summary>
    /// <param name="fileStream">File stream to which to save the settings to</param>
    /// <param name="token">Cancellation token</param>
    public abstract Task CreateDefaultSettings(FileStream fileStream, CancellationToken token);

    /// <summary>
    /// Gets the settings from the given file
    /// </summary>
    /// <param name="fileStream">File stream from which to load the settings</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>The loaded settings object, if any</returns>
    public abstract ValueTask<T?> GetSettings(FileStream fileStream, CancellationToken token);

    /// <summary>
    /// Configures the Dependency Injection services
    /// </summary>
    /// <param name="services">Service collection instance</param>
    public abstract void ConfigureServices(IServiceCollection services);

    /// <inheritdoc />
    public void Dispose()
    {
        this.cancellationSource.Dispose();
        GC.SuppressFinalize(this);
    }
}
