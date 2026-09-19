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
    /// <param name="data">Solver data</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>A <see cref="Result{T}"/> object containing the fetched input, or an error if failed</returns>
    Task<Result<string, Exception>> FetchInput(SolverData data, CancellationToken token = default);

    /// <summary>
    /// Submits the answer for verification
    /// </summary>
    /// <param name="answer">Answer to submit</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>A <see cref="Result"/> object indicating if the answer was correct or not</returns>
    Task<Result> SubmitAnswer(string answer, CancellationToken token = default);
}
