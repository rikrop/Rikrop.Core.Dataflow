using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="GroupingItemsTarget{TKey,TItem}"/> группирует отправку элементов по ключу. 
    /// Т.е. он отсылает все элементы с одинаковым ключом в одну и ту же цель.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public class GroupingItemsTarget<TKey, TItem> : ITarget<TItem>
    {
        private readonly IConverter<TItem, TKey> _itemToKeyConverter;
        private readonly ITargetFactory<TKey, TItem> _targetGroupFactory;
        private readonly ConcurrentDictionary<TKey, ITarget<TItem>> _targets;

        /// <summary>
        /// Конуструктор для создания <see cref="GroupingItemsTarget{TKey,TItem}"/>.
        /// </summary>
        /// <param name="targetGroupFactory">Фабрика создания целей, куда будут отправляться элементы с одинаковым ключом.</param>
        /// <param name="itemToKeyConverter">Извлекатель ключей из элементов. Ключи используются для выбора цели отправки.</param>
        public GroupingItemsTarget(ITargetFactory<TKey, TItem> targetGroupFactory, IConverter<TItem, TKey> itemToKeyConverter)
        {
            Contract.Requires<ArgumentNullException>(targetGroupFactory != null);
            Contract.Requires<ArgumentNullException>(itemToKeyConverter != null);

            _itemToKeyConverter = itemToKeyConverter;
            _targetGroupFactory = targetGroupFactory;

            _targets = new ConcurrentDictionary<TKey, ITarget<TItem>>();
        }

        public async Task SendAsync(TItem item)
        {
            var key = _itemToKeyConverter.Convert(item);

            ITarget<TItem> target;

            if (!_targets.TryGetValue(key, out target))
            {
                target = _targets.AddOrUpdate(key, k => _targetGroupFactory.CreateTarget(k), (k, i) => i);
            }

            await target.SendAsync(item);
        }
    }
}