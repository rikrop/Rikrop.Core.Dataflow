using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Rikrop.Core.Framework.Exceptions;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Фабрика для создания объектов <see cref="BatchingTarget{TItem}"/>.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public class BatchingTargetFactory<TKey, TItem> : ITargetFactory<TKey, TItem>
    {
        private readonly ITargetFactory<TKey, IReadOnlyCollection<TItem>> _targetFactory;
        private readonly int _batchSize;
        private readonly TimeSpan _forceSendTimeout;
        private readonly IExceptionHandler _exceptionHandler;

        /// <summary>
        /// Конструктор для создания <see cref="BatchingTargetFactory{TKey,TItem}"/>.
        /// </summary>
        /// <param name="targetFactory">Фабрика, которая используется для создания целей, куда будут отправляться пачки</param>
        /// <param name="batchSize">Размер пачки.</param>
        /// <param name="forceSendTimeout">Время по ситченеии которого элементы в буфере будут отправлены в цель, даже если <paramref name="batchSize"/> ещё не набран.</param>
        /// <param name="exceptionHandler">Обработчик искючений, которые могут возникнуть во время отправки пачки.</param>
        public BatchingTargetFactory(ITargetFactory<TKey, IReadOnlyCollection<TItem>> targetFactory, int batchSize, TimeSpan forceSendTimeout, IExceptionHandler exceptionHandler)
        {
            Contract.Requires<ArgumentNullException>(targetFactory != null);
            Contract.Requires<ArgumentException>(batchSize != 0);
            Contract.Requires<ArgumentException>(forceSendTimeout != TimeSpan.MinValue);
            Contract.Requires<ArgumentNullException>(exceptionHandler != null);

            _targetFactory = targetFactory;
            _batchSize = batchSize;
            _forceSendTimeout = forceSendTimeout;
            _exceptionHandler = exceptionHandler;
        }

        public ITarget<TItem> CreateTarget(TKey key)
        {
            return new BatchingTarget<TItem>(_targetFactory.CreateTarget(key), _batchSize, _forceSendTimeout, _exceptionHandler);
        }
    }
}