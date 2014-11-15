using System;
using System.Diagnostics.Contracts;
using Rikrop.Core.Dataflow.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Интерфейс, описывающий абстрактную фабрику создания <see cref="IWork{TItem,TResult}"/>
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    [ContractClass(typeof (ContractIWorkFactory<,>))]
    public interface IWorkFactory<in TItem, TResult>
    {
        /// <summary>
        /// Создаёт объект типа <see cref="IWork{TItem,TResult}"/>
        /// </summary>
        /// <param name="item">Элемент, над которым будет выполняться работа.</param>
        /// <returns>Созданный объект <see cref="IWork{TItem,TResult}"/></returns>
        IWork<TItem, TResult> GetWork(TItem item);
    }

    namespace Contracts
    {
        [ContractClassFor(typeof (IWorkFactory<,>))]
        public abstract class ContractIWorkFactory<TItem, TResult> : IWorkFactory<TItem, TResult>
        {
            public IWork<TItem, TResult> GetWork(TItem item)
            {
                Contract.Requires<ArgumentNullException>(!Equals(item, null));
                Contract.Assume(Contract.Result<IWork<TItem, TResult>>() != null);
                return default(IWork<TItem, TResult>);
            }
        }
    }
}