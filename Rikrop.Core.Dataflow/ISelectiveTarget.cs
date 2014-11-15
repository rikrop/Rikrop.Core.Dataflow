using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Rikrop.Core.Dataflow.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Интерфейс, описывающий абстрактную цель, которая может передавать только ограниченный набор элементов.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    [ContractClass(typeof(ContractISelectiveTarget<>))]
    public interface ISelectiveTarget<in TItem> : ITarget<TItem>
    {
        /// <summary>
        /// Возвращает true, если <paramref name="item"/> может быть отправлен через эту цель. Иначе false.
        /// </summary>
        /// <param name="item">Элемент для проверки перед отправкой.</param>
        /// <returns>True, если <paramref name="item"/> может быть отправлен через эту цель. Иначе false.</returns>
        bool CanSend(TItem item);
    }

    namespace Contracts
    {
        [ContractClassFor(typeof(ISelectiveTarget<>))]
        public abstract class ContractISelectiveTarget<TItem> : ISelectiveTarget<TItem>
        {
            public Task SendAsync(TItem item)
            {
                Contract.Assume(CanSend(item));

                throw new NotSupportedException();
            }

            public abstract bool CanSend(TItem item);
        }
    }
}