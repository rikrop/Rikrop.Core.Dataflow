using System.Diagnostics.Contracts;
using Rikrop.Core.Dataflow.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Интерфейс, описывающий абстрактную фабрику создания <see cref="IItemsContainer{TItem}"/>.
    /// </summary>
    /// <typeparam name="TKey">Тип ключя создания <see cref="IItemsContainer{TItem}"/>.</typeparam>
    /// <typeparam name="TItem">Тип элементов в <see cref="IItemsContainer{TItem}"/>.</typeparam>
    [ContractClass(typeof(ContractIItemsContainerFactory<,>))]
    public interface IItemsContainerFactory<in TKey, TItem>
    {
        IItemsContainer<TItem> Create(TKey key);
    }

    namespace Contracts
    {
        [ContractClassFor(typeof(IItemsContainerFactory<,>))]
        public abstract class ContractIItemsContainerFactory<TKey, TItem> : IItemsContainerFactory<TKey, TItem>
        {
            public IItemsContainer<TItem> Create(TKey key)
            {
                Contract.Assume(Contract.Result<IItemsContainer<TItem>>() != null);
                return default(IItemsContainer<TItem>);
            }
        }
    }
}