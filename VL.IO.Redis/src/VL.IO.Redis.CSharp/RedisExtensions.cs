﻿using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Collections.Immutable;
using System.Reactive.Concurrency;

namespace VL.IO.Redis
{
    
    public sealed class ThreadSafeToggle
    {
        public ThreadSafeToggle() { }

        private bool enabled = true;
        //private object syncObj = new object();

        public void Enable()
        {
            //lock (syncObj)
            {
                enabled = true;
            }
        }
        public void Disable()
        {
            //lock (syncObj)
            {
                enabled = false;
            }
        }
        public bool Enabled()
        {
            //lock (syncObj)
            {
                return enabled;
            }
        }
    }

    public static class RedisExtensions
    {
        //public static ImmutableDictionary<Guid, object> Result(this Task<Task<ImmutableDictionary<Guid, object>>> keyValuePairs)
        //{
        //    return keyValuePairs.ConfigureAwait(false).GetAwaiter().GetResult().ConfigureAwait(false).GetAwaiter().GetResult();
        //}

        //public static void GetDict(this Task<ValueTuple<ImmutableDictionary<Guid, object>, ImmutableList<RedisKey>>> tup, out ImmutableDictionary<Guid, object> Dict, out ImmutableList<RedisKey> Changes)
        //{
        //    var tuple = tup.ConfigureAwait(false).GetAwaiter().GetResult();

        //    Dict = tuple.Item1;
        //    Changes = tuple.Item2;
        //}

        //public static ValueTuple<ImmutableDictionary<Guid, object>, ImmutableList<RedisKey>> Wait(this Task<ValueTuple<ImmutableDictionary<Guid, object>, ImmutableList<RedisKey>>> tup)
        //{
        //    return tup.ConfigureAwait(false).GetAwaiter().GetResult();
        //}

        public static Task<KeyValuePair<Guid, object>> Cast<T>(this Task<T> task, Guid guid)
        {
            return task.ContinueWith(t => new KeyValuePair<Guid,object>(guid, (object)t.Result));
        }

        public static IObservable<RedisValue> WhenMessageReceived(this ISubscriber subscriber, RedisChannel channel)
        {
            return Observable.Create<RedisValue>(async (obs, ct) =>
            {
                // as the SubscribeAsync callback can be invoked concurrently
                // a thread-safe wrapper for OnNext is needed
                var syncObs = Observer.Synchronize(obs);
                await subscriber.SubscribeAsync(channel, (_, message) =>
                {
                    syncObs.OnNext(message);
                }).ConfigureAwait(false);

                return Disposable.Create(() => subscriber.Unsubscribe(channel));
            });
        }
    }

   
}
