using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VL.IO.Redis
{
    public struct CmdQueue
    {
        private ITransaction _tran;
        private readonly IList<Func<ITransaction, Task<KeyValuePair<Guid, object>>>> _cmds;
        private readonly List<RedisKey> _changedkeys;
        private readonly IList<Task<KeyValuePair<Guid, object>>> _tasks;
        private Task<Task<ImmutableDictionary<Guid, object>>> _result;

        public CmdQueue(IDatabase database)
        {
            _tran = database.CreateTransaction();
            _cmds = new List<Func<ITransaction, Task<KeyValuePair<Guid, object>>>>();
            _changedkeys = new List<RedisKey>();
            _tasks = new List<Task<KeyValuePair<Guid, object>>>();
            _result = null;
        }

        public static CmdQueue Enqueue(CmdQueue queue, Func<ITransaction, Task<KeyValuePair<Guid, object>>> cmd)
        {
            queue._cmds.Add(cmd);

            return queue;
        }

        public static CmdQueue ChangedKeys(CmdQueue queue, IEnumerable<RedisKey> keys)
        {
            queue._changedkeys.AddRange(keys);
            return queue;
        }

        public static CmdQueue AddTransactions(CmdQueue queue, out int Count, out ImmutableList<RedisKey> Changes)
        {
            if (queue._tran != null)
                foreach (var cmd in queue._cmds)
                    queue._tasks.Add(cmd(queue._tran));

            Count = queue._tasks.Count;
            Changes = queue._changedkeys.ToImmutableList();
            return queue;
        }

        public async static Task<ImmutableDictionary<Guid, object>> ExecuteTransactions(CmdQueue queue, CommandFlags commandFlags )
        {
            if (queue._tran.Execute(commandFlags))
            {
                return await Task.WhenAll(queue._tasks).ContinueWith(t => t.Result.ToImmutableDictionary());
            }
            else
            {
                return ImmutableDictionary<Guid, object>.Empty;
            }
        }

        public async static Task<ImmutableDictionary<Guid, object>> TestExecution(CmdQueue queue, CancellationToken ct)
        {
            return await Task.Run<ImmutableDictionary<Guid, object>>(async () =>
            {
                if (await queue._tran.ExecuteAsync())
                    return await Task.WhenAll(queue._tasks).ContinueWith(t => t.Result.ToImmutableDictionary(), ct);
                else 
                    return ImmutableDictionary<Guid, object>.Empty;
            }, ct);
        }

        public static IObservable<ImmutableDictionary<Guid, object>> ExecuteTransactions2(CmdQueue queue)
        {
            return queue._tran.Execute() ? Observable.FromAsync(async () => await Task.WhenAll(queue._tasks).ContinueWith(t => t.Result.ToImmutableDictionary())) : Observable.Empty<ImmutableDictionary<Guid, object>>();
        }



        public static  ImmutableDictionary<Guid, object> Result(Task<ImmutableDictionary<Guid, object>> queue, bool await = false)
        {
            return queue.ConfigureAwait(await).GetAwaiter().GetResult();
        }


    }


    public class RedisCommandQueue
    {
        private ITransaction _tran;
        private readonly IList<Task<KeyValuePair<Guid, object>>> _tasks = new List<Task<KeyValuePair<Guid, object>>>();
        private readonly IList<Func<ITransaction, Task<KeyValuePair<Guid, object>>>> _cmds = new List<Func<ITransaction, Task<KeyValuePair<Guid, object>>>>();
        private readonly List<RedisKey> _changedkeys = new List<RedisKey>();

        private Task<Task<ImmutableDictionary<Guid, object>>> _result;

        

        //private KeyValuePair<Guid, object>[]                   _result2;
        //private ImmutableDictionary<Guid, object>       _output = ImmutableDictionary<Guid, object>.Empty;

        public int Count => _tasks.Count;
        public void Enqueue(Func<ITransaction, Task<KeyValuePair<Guid, object>>> cmd)
        {
            _cmds.Add(cmd);
        }

        public void ChangedKeys(IEnumerable<RedisKey> keys)
        {
            _changedkeys.AddRange(keys);
        }


        public RedisCommandQueue()
        { }

        public void BeginTransaction(IDatabase database)
        {
            _tasks.Clear();
            _cmds.Clear();
            _changedkeys.Clear();
            _tran = database.CreateTransaction();
        }

        public void ExecuteAsync(out int Count)
        {
            
            if (_tran != null)
                foreach (var cmd in _cmds)
                    _tasks.Add(cmd(_tran));

            Count = _tasks.Count;

            _result = _tran.ExecuteAsync().ContinueWith(
            t =>
                Task.WhenAll(_tasks).ContinueWith(t => t.Result.ToImmutableDictionary())
            );

            
        }

        public void AddTransactions(out int Count, out ImmutableList<RedisKey> Changes)
        {
            if (_tran != null)
                foreach (var cmd in _cmds)
                    _tasks.Add(cmd(_tran));

            Count = _tasks.Count;
            Changes = _changedkeys.ToImmutableList();
        }

        public async Task<ImmutableDictionary<Guid, object>> ExecuteTransactions(CancellationToken ct)
        {


            if (await _tran.ExecuteAsync())
            {
                return await Task.WhenAll(_tasks).ContinueWith(t => t.Result.ToImmutableDictionary(), ct);
            }
            else
            {
                return ImmutableDictionary<Guid, object>.Empty;
            }
        }

        private async static Task<ImmutableDictionary<Guid, object>> all(IList<Task<KeyValuePair<Guid, object>>> tasks, CancellationToken ct)
        {
            return await Task.WhenAll(tasks).ContinueWith(t => t.Result.ToImmutableDictionary(), ct);
        }

        public async static Task<ImmutableDictionary<Guid, object>> tran(RedisCommandQueue queue, CancellationToken ct)
        {
            if (await queue._tran.ExecuteAsync())
            {
                return await Task.WhenAll(queue._tasks).ContinueWith(t => t.Result.ToImmutableDictionary(), ct);
            }
            else
            {
                return ImmutableDictionary<Guid, object>.Empty;
            }

        }

        public static IObservable<ImmutableDictionary<Guid, object>> tran2(RedisCommandQueue queue, CancellationToken ct)
        {
            return Observable.FromAsync(() => tran(queue, ct));
        }




        public Task<Task<ImmutableDictionary<Guid, object>>> ExecuteTransactions2(CancellationToken ct)
        {
            var tnp = Observable.FromAsync(() => tran(this, ct));

            var tmp  = _tran.ExecuteAsync().ContinueWith(
            t =>
                Task.WhenAll(_tasks).ContinueWith(t => t.Result.ToImmutableDictionary(), ct)
            ).ToObservable();

            return _tran.ExecuteAsync().ContinueWith(
            t =>
                Task.WhenAll(_tasks).ContinueWith(t => t.Result.ToImmutableDictionary(), ct)
            );
        }








        //public async Task<ValueTuple<ImmutableDictionary<Guid, object>, ImmutableList<RedisKey>>> ExecuteTransactions()
        //{
        //    if (await _tran.ExecuteAsync())
        //    {

        //        return new ValueTuple<ImmutableDictionary<Guid, object>, ImmutableList<RedisKey>>((await Task.WhenAll(_tasks)).ToImmutableDictionary(), _changedkeys.ToImmutableList());

        //    }
        //    else
        //    {
        //        return new ValueTuple<ImmutableDictionary<Guid, object>, ImmutableList<RedisKey>>(ImmutableDictionary<Guid, object>.Empty,ImmutableList<RedisKey>.Empty);
        //    }
        //}

        //public void GetDict(Task<ValueTuple<ImmutableDictionary<Guid, object>, ImmutableList<RedisKey>>> tup , out ImmutableDictionary<Guid, object> Dict, out ImmutableList<RedisKey> Changes)
        //{
        //    var tuple = tup.ConfigureAwait(false).GetAwaiter().GetResult();

        //    Dict = tuple.Item1;
        //    Changes = tuple.Item2;
        //}

        public ImmutableDictionary<Guid, object> Result()
        {
            return _result.ConfigureAwait(false).GetAwaiter().GetResult().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public ImmutableList<RedisKey> Changed()
        {
            return _changedkeys.ToImmutableList();
        }
    }
}
