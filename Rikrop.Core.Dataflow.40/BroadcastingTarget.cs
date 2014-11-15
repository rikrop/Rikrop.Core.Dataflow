using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="BroadcastingTarget{TItem}"/> рассылает полученный элемент во множество целей./>
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class BroadcastingTarget<TItem> : ITarget<TItem>
    {
        private readonly IEnumerable<ITarget<TItem>> _targets;
        private readonly Func<TItem, Task> _sendDelegate;

        /// <summary>
        /// Конструктор для создания <see cref="BroadcastingTarget{TItem}"/>.
        /// </summary>
        /// <param name="targets">Набор целей, куда будет происходить отправка.</param>
        /// <param name="broadcastMethod">Способ отправки.</param>
        /// <param name="sendInThreadPool">Запускать ли отправку в потоке <see cref="ThreadPool"/>'a.</param>
        public BroadcastingTarget(IEnumerable<ITarget<TItem>> targets, BroadcastMethod broadcastMethod = BroadcastMethod.AwaitAllTargets, bool sendInThreadPool = false)
        {
            Contract.Requires<ArgumentNullException>(targets != null);

            _targets = targets;

            switch (broadcastMethod)
            {
                case BroadcastMethod.AwaitEachTarget:
                    if (sendInThreadPool)
                    {
                        _sendDelegate = SendEachInThreadPoolWithAwaitEach;
                    }
                    else
                    {
                        _sendDelegate = SendWithAwaitEach;
                    }
                    break;
                case BroadcastMethod.AwaitAllTargets:
                    if (sendInThreadPool)
                    {
                        _sendDelegate = SendEachInThreadPoolWithAwaitAll;
                    }
                    else
                    {
                        _sendDelegate = SendWithAwaitAll;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("broadcastMethod", string.Format("Unknown type of broadcast method: {0}", broadcastMethod));
            }
        }

        public Task SendAsync(TItem item)
        {
            return _sendDelegate(item);
        }

        private Task SendEachInThreadPoolWithAwaitAll(TItem item)
        {
            var tasks = new Task[_targets.Count()];
            var targetNum = 0;
            foreach (var target in _targets)
            {
                var target1 = target;
                tasks[targetNum++] = TaskEx.Run(() => target1.SendAsync(item));
            }
            return TaskEx.WhenAll(tasks);
        }

        private async Task SendEachInThreadPoolWithAwaitEach(TItem item)
        {
            foreach (var target in _targets)
            {
                var target1 = target;
                await TaskEx.Run(() => target1.SendAsync(item));
            }
        }

        private Task SendWithAwaitAll(TItem item)
        {
            var tasks = new Task[_targets.Count()];
            var targetNum = 0;
            foreach (var target in _targets)
            {
                tasks[targetNum++] = target.SendAsync(item);
            }
            return TaskEx.WhenAll(tasks);
        }

        private async Task SendWithAwaitEach(TItem item)
        {
            foreach (var target in _targets)
            {
                await target.SendAsync(item);
            }
        }
    }
}