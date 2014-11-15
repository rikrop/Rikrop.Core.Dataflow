using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="BatchSplitterTarget{TItem}"/> принимает коллекцию элементов и отправляет их по одному.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class BatchSplitterTarget<TItem> : ITarget<IEnumerable<TItem>>
    {
        private readonly ITarget<TItem> _target;

        /// <summary>
        /// Конструктор для создания <see cref="BatchSplitterTarget{TItem}"/>.
        /// </summary>
        /// <param name="target">Цель, куда будут отправляться элементы по одному.</param>
        public BatchSplitterTarget(ITarget<TItem> target)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            
            _target = target;
        }

        public Task SendAsync(IEnumerable<TItem> items)
        {
            var tasks = items.Select(item => _target.SendAsync(item)).ToList();

            return Task.WhenAll(tasks);
        }
    }
}