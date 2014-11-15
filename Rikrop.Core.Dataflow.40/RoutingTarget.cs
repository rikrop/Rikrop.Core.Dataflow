using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="RoutingTarget{TItem}"/> маршрутизирует отсылаемый поток основываясь на <see cref="ISelectiveTarget{TItem}"/>.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class RoutingTarget<TItem> : ITarget<TItem>
    {
        private readonly IEnumerable<ISelectiveTarget<TItem>> _targets;

        /// <summary>
        /// Конструктор для создания <see cref="RoutingTarget{TItem}"/>.
        /// </summary>
        /// <param name="targets">Набор целей, каждая из которых предоставляет информацию о том, может ли через неё быть отправлен элемент.</param>
        public RoutingTarget(IEnumerable<ISelectiveTarget<TItem>> targets)
        {
            Contract.Requires<ArgumentNullException>(targets != null);
            Contract.Requires<ArgumentNullException>(targets.Any());

            _targets = targets;
        }

        /// <summary>
        /// Отправляет элемент в одну из целей.
        /// </summary>
        /// <param name="item">Элемент для отправки</param>
        /// <exception cref="InvalidOperationException">Не найдена цель, в которую можно отправить элемент.</exception>
        /// <returns><see cref="Task"/> той цели, куда отправляется элемент.</returns>
        public Task SendAsync(TItem item)
        {
            foreach (var selectiveTarget in _targets)
            {
                if (selectiveTarget.CanSend(item))
                {
                    return selectiveTarget.SendAsync(item);
                }
            }

            throw new InvalidOperationException(string.Format("RouterTarget can't find target for item {0}", item));
        }
    }
}