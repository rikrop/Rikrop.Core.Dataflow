using System;
using System.Diagnostics.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="GenericTargetFactory{TKey,TItem}"/> создаёт цели с помощью делегата создания.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public class GenericTargetFactory<TKey, TItem> : ITargetFactory<TKey, TItem>
    {
        private readonly Func<TKey, ITarget<TItem>> _creator;

        /// <summary>
        /// Конструктор для создания <see cref="GenericTargetFactory{TKey,TItem}"/>.
        /// </summary>
        /// <param name="creator">Делегат создания целей.</param>
        public GenericTargetFactory(Func<TKey, ITarget<TItem>> creator)
        {
            Contract.Requires<ArgumentNullException>(creator != null);

            _creator = creator;
        }

        public ITarget<TItem> CreateTarget(TKey key)
        {
            return _creator(key);
        }
    }
}