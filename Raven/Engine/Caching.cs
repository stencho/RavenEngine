using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Raven.Engine;
using Raven.Console;
using Raven.Engine.Universes.Forces;

namespace Raven.Caching {
    public static class CacheCancellation {
        internal static CancellationTokenSource cancellation_token_source = new CancellationTokenSource();
        internal static CancellationToken cancellation_token => cancellation_token_source.Token;

        public static void Cancel() => cancellation_token_source.Cancel(true);        
        public static void Reset() => cancellation_token_source = new CancellationTokenSource();
    }

    public struct cache_item_life {
        internal double birth_time; internal double life_time = 0;

        public cache_item_life(double life_time) {
            this.life_time = life_time;
            refresh();
        }

        public double age => DateTimeOffset.UtcNow.ToUnixTimeSeconds() - birth_time;
        public void refresh() => birth_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        public bool needs_prune() => age > life_time;
    }

    public class ConcurrentCache<I,T> {
        public readonly double MaxAge = 86400; // 1 day

        public string name = "";
        
        public Func<T, bool> prune_rule;
        public int prune_frequency_ms = 5000;

        Type type;
        ConcurrentDictionary<I, (cache_item_life life, T item)> cache = new ConcurrentDictionary<I, (cache_item_life life, T item)>();
        public ConcurrentDictionary<I, (cache_item_life life, T item)> Cache => cache;
        
        bool currently_pruning = false;

        public bool Test(I key) => cache.ContainsKey(key);
        public void Remove(I key) => cache.TryRemove(key, out _);

        public void Clear() => cache.Clear();        
        

        public ConcurrentCache() {
            type = typeof(T);
            StartPruning();
        }
        public ConcurrentCache(Func<T, bool> prune_rule) {
            type = typeof(T);
            StartPruning();
        }

        public ConcurrentCache(double age_seconds) {
            MaxAge = age_seconds;
            type = typeof(T);
            StartPruning();
        }
        
        public ConcurrentCache(string name) {
            this.name = name;
            type = typeof(T);
            StartPruning();
        }
        public ConcurrentCache(string name, Func<T, bool> prune_rule) {
            this.name = name;
            type = typeof(T);
            StartPruning();
        }

        public ConcurrentCache(string name, double age_seconds) {
            this.name = name;
            MaxAge = age_seconds;
            type = typeof(T);
            StartPruning();
        }

        ~ConcurrentCache() {
            currently_pruning = false;            
        }

        public void Store(I key, T item) {
            Store(key, item, MaxAge);
        }
        
        public void Store(I key, T item, double life_time) {
            if (item == null) return;
            if (!item.GetType().IsAssignableFrom(type)) return;
            if (Test(key)) return;
            if (cache.TryAdd(key, (new cache_item_life(life_time), item))) {
                Log.log($"Stored {key}::{item.ToString()} in cache for {life_time} seconds");
            } else {
                Log.log($"Failed cache store on {key}::{item.ToString()}");
            }
        }

        public void Update(I key, T item) {
            if (item == null) return;
            if (!item.GetType().IsAssignableFrom(type)) return;
            cache.AddOrUpdate(key, (new cache_item_life(MaxAge), item), (key, old) => (new cache_item_life(), item));
        }
        public T Request(I key) {
            return cache[key].item;
        }

        public int Count => cache.Count;

        public void StartPruning() {
            Threads.StartTask($"Prune Cache{(name.Length > 0 ? " (" + name + ")" : "") }", Prune, CacheCancellation.cancellation_token)
                .ContinueWith(a => { currently_pruning = false; });
        }

        private async void Prune() {
            currently_pruning = true;
            
            while (currently_pruning && !Threads.cancellation_token.IsCancellationRequested) {
            restart:
                if (cache.Keys.Count > 0) {
                    foreach (var key in cache.Keys.ToList()) {
                        if (prune_rule != null) {
                            if (prune_rule(cache[key].item)) {
                                cache.TryRemove(key, out _);
                                goto restart;

                            }
                        } else if (cache[key].life.life_time != -1 && cache[key].life.needs_prune()) {
                            cache.TryRemove(key, out _);
                            //if (State.LogLevel == Logging.LogLevel.ALL)
                                Debug.WriteLine($"Pruned {key}::{cache[key].item.ToString()} from cache", "Cache", 5);
                            goto restart;
                        }
                    }
                }
                Debug.WriteLine($"Nothing pruned");
                Thread.Sleep(prune_frequency_ms);
            }

            currently_pruning = false;
        }
    }
}
