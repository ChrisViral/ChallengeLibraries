using Challenge.Solvers;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;

namespace Challenge.CLI;

/// <summary>
/// Challenge input fetcher interface
/// </summary>
[PublicAPI]
public interface ISolverResolver
{
    /// <summary>
    /// Challenge name
    /// </summary>
    string ChallengeName { get; }

    /// <summary>
    /// Fetch the challenge input for a given problem
    /// </summary>
    /// <param name="year">Challenge year</param>
    /// <param name="day">Challenge day</param>
    /// <param name="module">Challenge module</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>A <see cref="Result{T}"/> object containing the fetched input, or an error if failed</returns>
    Task<Result<string>> FetchInput(uint year, uint day, string module, CancellationToken token = default);
}
