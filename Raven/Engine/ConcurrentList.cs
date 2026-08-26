using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raven.Engine;

public class ConcurrentList<T> : IEnumerable {
    private readonly List<T> list = new();
    private System.Threading.Lock _lock = new();

    public ConcurrentList() {}

    public void Add(T item) {
        lock (_lock) {
            list.Add(item);
        }
    }
    
    public void Remove(T item) {
        lock (_lock) {
            list.Remove(item);
        }
    }

    public void Clear() {
        lock (_lock) {
            list.Clear();
        }
    }
    
    public T this[int index] {
        get {
            lock (_lock) {
                return list[index];
            }
        }
        set {
            lock (_lock) {
                list[index] = value;
            }
        }
    }

    public int Count { get { lock (_lock) return list.Count; } }
    
    public void ForEach(Action<T> action) {
        lock (_lock) {
            list.ForEach(action);
        }
    }
    
    
    
    public IEnumerator<T> GetEnumerator()
    {
        T[] snapshot;

        lock (_lock)
            snapshot = list.ToArray();

        return ((IEnumerable<T>)snapshot).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}