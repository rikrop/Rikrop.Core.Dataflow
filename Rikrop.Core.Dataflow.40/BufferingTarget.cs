using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Rikrop.Core.Framework.Exceptions;
using Rikrop.Core.Threading;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="BufferingTarget{TItem}"/> является элементом буфера в который можно быстро писать и буфер которого постепенно разбирается одним или множеством потребителей. 
    /// Применяется там, где важно обеспечить быструю, неблокирующую отправку, когда результат отправки не важен./>
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class BufferingTarget<TItem> : ITarget<TItem>
    {
        private readonly ITarget<TItem> _target;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly int _bifferConsumersCount;
        private readonly ConcurrentQueue<TItem> _itemsBuffer;
        private readonly ManualResetEventAsync _event;
        private readonly Task _completedTask = TaskEx.FromResult(true);

        /// <summary>
        /// Конструктор для создания <see cref="BufferingTarget{TItem}"/>.
        /// </summary>
        /// <param name="target">Цель, куда будут отправляться элементы из буфера.</param>
        /// <param name="exceptionHandler">Обработчик исключения, которые могут возникнуть при отпрвки элементов.</param>
        /// <param name="bifferConsumersCount">Кол-во потребителей буфера.</param>
        public BufferingTarget(ITarget<TItem> target, IExceptionHandler exceptionHandler, int bifferConsumersCount = 1)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentNullException>(exceptionHandler != null);
            Contract.Requires<ArgumentException>(bifferConsumersCount >= 1);

            _target = target;
            _exceptionHandler = exceptionHandler;
            _bifferConsumersCount = bifferConsumersCount;
            _event = new ManualResetEventAsync(allowToStopWaitBeforeAwaited: true);
            _itemsBuffer = new ConcurrentQueue<TItem>();

            BeginAwaitForConsumtion();
        }

        private async void BeginAwaitForConsumtion()
        {
            while (true)
            {
                await _event;

                var tasks = new List<Task>(_bifferConsumersCount);
                for (int i = 0; i < _bifferConsumersCount; i++)
                {
                    var task = TaskEx.Run(() => ConsumeAll());
                    tasks.Add(task);
                }
                
                await TaskEx.WhenAll(tasks);
            }
        }

        private async Task ConsumeAll()
        {
            TItem item;
            while (_itemsBuffer.TryDequeue(out item))
            {
                try
                {
                    await _target.SendAsync(item);
                }
                catch (Exception ex)
                {
                    if (!_exceptionHandler.Handle(ex))
                    {
                        break;
                    }
                }
            }
        }

        public Task SendAsync(TItem item)
        {
            Send(item);
            return _completedTask;
        }

        public void Send(TItem item)
        {
            _itemsBuffer.Enqueue(item);
            _event.StopAwait();
        }
    }
}