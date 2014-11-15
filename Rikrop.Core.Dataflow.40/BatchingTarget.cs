using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Rikrop.Core.Framework.Exceptions;
using Rikrop.Core.Threading;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="BatchingTarget{TItem}"/> собирает элементы в буфер и по достижении определённого кол-ва отправляет собранную пачку дальше в отдельном потоке.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class BatchingTarget<TItem> : ITarget<TItem>
    {
        private readonly ITarget<IEnumerable<TItem>> _batchTarget;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly BatchPool<TItem> _pool;
        private readonly Task _completedTask;

        /// <summary>
        /// Конструктор для создания <see cref="BatchingTarget{TItem}"/>.
        /// </summary>
        /// <param name="batchTarget">Цель, куда будет отправлена пачка.</param>
        /// <param name="batchSize">Размер пачки.</param>
        /// <param name="forceSendTimeout">Время по ситченеии которого элементы в буфере будут отправлены в цель, даже если <paramref name="batchSize"/> ещё не набран.</param>
        /// <param name="exceptionHandler">Обработчик искючений, которые могут возникнуть во время отправки пачки.</param>
        public BatchingTarget(ITarget<IEnumerable<TItem>> batchTarget, int batchSize, TimeSpan forceSendTimeout, IExceptionHandler exceptionHandler)
        {
            Contract.Requires<ArgumentNullException>(batchTarget != null);
            Contract.Requires<ArgumentException>(batchSize > 0);
            Contract.Requires<ArgumentException>(forceSendTimeout != TimeSpan.Zero);
            Contract.Requires<ArgumentNullException>(exceptionHandler != null);

            _batchTarget = batchTarget;
            _exceptionHandler = exceptionHandler;
            _pool = new BatchPool<TItem>(SendBatch, batchSize, forceSendTimeout);
            _completedTask = TaskEx.FromResult(true);
        }

        public Task SendAsync(TItem item)
        {
            _pool.Add(item);
            return _completedTask;
        }

        private async void SendBatch(IEnumerable<TItem> items)
        {
            try
            {
                await _batchTarget.SendAsync(items);
            }
            catch (Exception ex)
            {
                _exceptionHandler.Handle(ex);
            }
        }
    }
}