using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raven.Engine;

public class ConcurrentList<T> {
    private readonly List<T> list = new();
    private System.Threading.Lock interaction_lock = new();

    public ConcurrentList() {}

    public void Add(T item) {
        lock (interaction_lock) {
            list.Add(item);
        }
    }
    
    public void Remove(T item) {
        lock (interaction_lock) {
            list.Remove(item);
        }
    }

    public void ForEach(Func<T, bool> func) {
        
    }
}