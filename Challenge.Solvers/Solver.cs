using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Challenge.Utils.Extensions.Enums;
using Challenge.Utils.Extensions.TimeSpans;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using TextCopy;

namespace Challenge.Solvers;

/// <summary>
/// Solver base class
/// </summary>
[PublicAPI]
public abstract partial class Solver : IDisposable
{
    /// <summary>
    /// Default split options
    /// </summary>
    protected const StringSplitOptions DEFAULT_OPTIONS = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
    /// <summary>
    /// Default split characters
    /// </summary>
    private static readonly char[] DefaultSplitters = ['\n'];

    private uint currentPart = 1;
    private readonly Stopwatch partWatch = new();

    /// <summary>
    /// Last answer logged by this solver
    /// </summary>
    public string LastAnswer { get; private set; } = string.Empty;

    /// <summary>
    /// Total solve time
    /// </summary>
    public TimeSpan SolveTime { get; private set; }

    /// <summary>
    /// Input data
    /// </summary>
    protected string[] Data { get; }

    /// <summary>
    /// Logger instance
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Creates a new <see cref="Solver"/> from the specified file
    /// </summary>
    /// <param name="input">Puzzle input</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="splitters">Splitting characters, defaults to newline only</param>
    /// <param name="options">Input parsing options, defaults to removing empty entries and trimming entries</param>
    protected Solver(string input, ILogger logger, char[]? splitters = null, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        // Setup data
        this.Logger = logger;

        splitters ??= DefaultSplitters;
        if (splitters.Length is 0)
        {
            // If no spliter, set data as is
            this.Data = options.HasFlags(StringSplitOptions.TrimEntries)? [input] : [input.Trim()];
        }
        else
        {
            // Else split data
            this.Data = input.Split(splitters, options);
        }
    }

    /// <summary>
    /// Runs the solver and starts the stopwatch
    /// </summary>
    public void RunAndStartStopwatch()
    {
        this.partWatch.Restart();
        Run();
        this.partWatch.Stop();
    }

    /// <summary>
    /// Runs the solver for the given part and starts the stopwatch
    /// </summary>
    /// <param name="part">Part to run</param>
    public void RunAndStartStopwatch(uint part)
    {
        this.currentPart = part;
        this.partWatch.Restart();
        Run(part);
        this.partWatch.Stop();
    }

    /// <summary>
    /// Runs the solver on the problem input
    /// </summary>
    public virtual void Run() => throw new NotSupportedException("This solver does not support no-part solves");

    /// <summary>
    /// Runs the solver on the problem input for the given part
    /// </summary>
    /// <param name="part">Part to run</param>
    public virtual partial void Run(uint part) => throw new NotSupportedException("This solver does not support per-part solves");

    /// <summary>
    /// Logs the answer to Part 1 to the console and results file.<br/>
    /// This also adds the answer to the clipboard.
    /// </summary>
    /// <param name="answer">Answer to log</param>
    public void LogAnswer<T>(T answer) where T : notnull
    {
        // Stop watches
        this.partWatch.Stop();
        this.SolveTime += this.partWatch.Elapsed;

        // Get answer and put into clipboard
        string answerText = answer.ToString() ?? string.Empty;
        this.LastAnswer = answerText;
        if (!string.IsNullOrEmpty(answerText))
        {
            ClipboardService.SetText(answerText);
        }

        // Log answer
        LogPartAnswer(this.Logger, this.currentPart++, answerText);
        LogPartTime(this.Logger, this.partWatch.Elapsed.GetElapsedString());

        // Collect GC and then restart watches
        GC.Collect();
        this.partWatch.Restart();
    }

    /// <summary>
    /// Logs a message to the console and the log file
    /// </summary>
    /// <param name="message">Message to log</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Log<T>(T message) where T : notnull => LogMessage(this.Logger, message);

    /// <inheritdoc />
    public virtual void Dispose() => GC.SuppressFinalize(this);

    /// <summary>
    /// Combines input lines into sequences, separated by empty lines
    /// </summary>
    /// <param name="input">Input lines</param>
    /// <returns>An enumerable of the packed input</returns>
    protected static IEnumerable<List<string>> CombineLines([InstantHandle] IEnumerable<string> input)
    {
        List<string> pack = [];
        foreach (string line in input)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (pack.Count is 0) continue;

                yield return pack;
                pack = [];
            }
            else
            {
                pack.Add(line);
            }
        }

        if (pack.Count is not 0)
        {
            yield return pack;
        }
    }
}

/// <summary>
/// Solver generic class
/// </summary>
/// <typeparam name="T">The fully parse input type</typeparam>
[PublicAPI]
public abstract class Solver<T> : Solver
{
    /// <summary>
    /// Parsed input data
    /// </summary>
    protected new T Data { get; }

    /// <summary>
    /// If the Solver has been disposed or not
    /// </summary>
    private bool IsDisposed { get; set; }

    /// <summary>
    /// Creates a new generic <see cref="Solver{T}"/> with the input data properly parsed
    /// </summary>
    /// <param name="input">Puzzle input</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="splitters">Splitting characters, defaults to newline only</param>
    /// <param name="options">Input parsing options, defaults to removing empty entries and trimming entries</param>
    /// <exception cref="InvalidOperationException">Thrown if the conversion to <typeparamref name="T"/> fails</exception>
    protected Solver(string input, ILogger logger, char[]? splitters = null, StringSplitOptions options = DEFAULT_OPTIONS) : base(input, logger, splitters, options)
    {
#if !DEBUG
        //Convert is intended to be a Pure function, therefore it should be safe to call in the constructor
        //ReSharper disable once VirtualMemberCallInConstructor
        this.Data = Convert(base.Data);
#else
        try
        {
            //ReSharper disable once VirtualMemberCallInConstructor
            this.Data = Convert(base.Data);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Could not convert the string array input to the {typeof(T)} type using the {nameof(Convert)} method.", e);
        }
#endif
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public override void Dispose()
    {
        if (this.IsDisposed) return;

        switch (this.Data)
        {
            case IEnumerable enumerable:
                foreach (object obj in enumerable)
                {
                    (obj as IDisposable)?.Dispose();
                }
                (enumerable as IDisposable)?.Dispose();
                break;

            case IDisposable disposable:
                disposable.Dispose();
                break;
        }

        this.IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Input conversion function<br/>
    /// <b>NOTE</b>: This method <b>must</b> be pure as it initializes the base class
    /// </summary>
    /// <param name="rawInput">Input value</param>
    /// <returns>Target converted value</returns>
    [Pure]
    protected abstract T Convert(string[] rawInput);
}
