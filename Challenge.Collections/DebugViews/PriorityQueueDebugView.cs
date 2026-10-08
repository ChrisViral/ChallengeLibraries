using System.Diagnostics;

namespace Challenge.Collections.DebugViews;

internal sealed class PriorityQueueDebugView<T>(PriorityQueue<T>? queue) where T : notnull
{
    private readonly PriorityQueue<T> queue = queue ?? throw new ArgumentNullException(nameof(queue));

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items
    {
        get
        {
            T[] array = new T[this.queue.Count];
            this.queue.CopyTo(array);
            return array;
        }
    }
}
