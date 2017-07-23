using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace Revalee.NetCore.Recurring
{
    internal sealed class ImmutableTaskCollection : ITaskCollection
    {
        private Dictionary<string, ConfiguredTask> _Tasks = new Dictionary<string, ConfiguredTask>();

        internal ImmutableTaskCollection()
        {
        }

        internal ImmutableTaskCollection(IEnumerable<ConfiguredTask> tasks)
        {
            foreach (ConfiguredTask taskConfig in tasks)
            {
                if (!_Tasks.ContainsKey(taskConfig.Identifier))
                {
                    _Tasks.Add(taskConfig.Identifier, taskConfig);
                }
            }
        }

        public int Count => _Tasks.Count;

        public IEnumerable<ConfiguredTask> Tasks => _Tasks.Values;

        public bool TryGetTask(string identifier, out ConfiguredTask taskConfig) => _Tasks.TryGetValue(identifier, out taskConfig);

        public async Task<bool> AddAsync(ConfiguredTask taskConfig)
        {
            // A proper immutable dictionary class would simplify this routine
            RandomWaitScheduler retryScheduler = null;

            do
            {
                Dictionary<string, ConfiguredTask> original = _Tasks;

                if (!original.ContainsKey(taskConfig.Identifier))
                {
                    var clone = new Dictionary<string, ConfiguredTask>(original);

                    if (!clone.ContainsKey(taskConfig.Identifier))
                    {
                        clone.Add(taskConfig.Identifier, taskConfig);

                        if (!object.ReferenceEquals(Interlocked.CompareExchange(ref _Tasks, clone, original), original))
                        {
                            if (retryScheduler == null)
                            {
                                retryScheduler = new RandomWaitScheduler();
                            }

                            await retryScheduler.Wait();
                            continue;
                        }

                        return true;
                    }
                }

                return false;
            } while (true);
        }

        public async Task<bool> RemoveAsync(string identifier)
        {
            // A proper immutable dictionary class would simplify this routine
            RandomWaitScheduler retryScheduler = null;

            do
            {
                Dictionary<string, ConfiguredTask> original = _Tasks;

                if (original.ContainsKey(identifier))
                {
                    var clone = new Dictionary<string, ConfiguredTask>(original);

                    if (clone.ContainsKey(identifier))
                    {
                        clone.Remove(identifier);

                        if (!object.ReferenceEquals(Interlocked.CompareExchange(ref _Tasks, clone, original), original))
                        {
                            if (retryScheduler == null)
                            {
                                retryScheduler = new RandomWaitScheduler();
                            }

                            await retryScheduler.Wait();
                            continue;
                        }

                        return true;
                    }
                }

                return false;
            } while (true);
        }

        public async Task<bool> RemoveAsync(ConfiguredTask taskConfig) => await RemoveAsync(taskConfig.Identifier);

        private class RandomWaitScheduler
        {
            private readonly Random _RandomNumberGenerator = new Random();

            public Task Wait() => Task.Delay(_RandomNumberGenerator.Next(15));
        }
    }
}
