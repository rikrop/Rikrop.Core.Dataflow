using System;
using System.Diagnostics.Contracts;
using Rikrop.Core.Framework.Exceptions;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Фабрика для создания <see cref="BufferingTarget{TItem}"/>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public class BufferingTargetFactory<TKey, TItem> : ITargetFactory<TKey, TItem>
    {
        private readonly ITargetFactory<TKey, TItem> _targetFactory;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly int _bifferConsumersCount;

        /// <summary>
        /// Конструктор для создания <see cref="BufferingTargetFactory{TKey,TItem}"/>.
        /// </summary>
        /// <param name="targetFactory">Фабрика, которая используется для создания целей, куда будут отправляться элементы из буфера.</param>
        /// <param name="exceptionHandler">Обработчик исключения, которые могут возникнуть при отпрвки элементов.</param>
        /// <param name="bifferConsumersCount">Кол-во потребителей буфера.</param>
        public BufferingTargetFactory(ITargetFactory<TKey, TItem> targetFactory, IExceptionHandler exceptionHandler, int bifferConsumersCount = 1)
        {
            Contract.Requires<ArgumentNullException>(targetFactory != null);
            Contract.Requires<ArgumentNullException>(exceptionHandler != null);
            Contract.Requires<ArgumentException>(bifferConsumersCount >= 1);

            _targetFactory = targetFactory;
            _exceptionHandler = exceptionHandler;
            _bifferConsumersCount = bifferConsumersCount;
        }

        public ITarget<TItem> CreateTarget(TKey key)
        {
            return new BufferingTarget<TItem>(_targetFactory.CreateTarget(key), _exceptionHandler, _bifferConsumersCount);
        }
    }
}