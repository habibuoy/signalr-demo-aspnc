namespace SignalRDemo.Server.Utils;

public class ConcurrentHashSet<T>
{
    private readonly HashSet<T> hashSet = new();
    private readonly object objLock = new();

    public int Count
    {
        get
        {
            lock (objLock)
            {
                return hashSet.Count;
            }
        }
    }

    public bool Contains(T t)
    {
        lock (objLock)
        {
            return hashSet.Contains(t);
        }
    }

    public bool TryAdd(T t, out int count)
    {
        lock (objLock)
        {
            var success = hashSet.Add(t);
            count = hashSet.Count;
            return success;
        }
    }

    public bool TryRemove(T t, out int count)
    {
        lock (objLock)
        {
            var success = hashSet.Remove(t);
            count = hashSet.Count;
            return success;
        }
    }

    public int Clear()
    {
        lock (objLock)
        {
            int count = hashSet.Count;
            hashSet.Clear();
            return count;
        }
    }

    public T[] ToArray()
    {
        lock (objLock)
        {
            return hashSet.ToArray();
        }
    }

    public List<T> ToList()
    {
        lock (objLock)
        {
            return hashSet.ToList();
        }
    }
}