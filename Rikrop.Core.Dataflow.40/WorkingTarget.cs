using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Цель, которая выполняет <see cref="IWork{TItem,TResult}"/> над <typeparamref name="TItem"/> и посылает <typeparamref name="TResult"/> в следующую цель.
    /// </summary>
    /// <typeparam name="TItem">Тип элемента, над которым будет производиться работа.</typeparam>
    /// <typeparam name="TResult">Тип результата работы.</typeparam>
    public class WorkingTarget<TItem, TResult> : ITarget<TItem>
    {
        private readonly IWork<TItem, TResult> _work;
        private readonly ITarget<TResult> _target;

        /// <summary>
        /// Создаёт объект <see cref="WorkingTarget{TItem,TResult}"/>.
        /// </summary>
        /// <param name="work">Объект работы, которая будет выполняться перед отправкой в <paramref name="target"/>.</param>
        /// <param name="target">Цель, в которую будет отправлен результат работы.</param>
        public WorkingTarget(IWork<TItem, TResult> work, ITarget<TResult> target)
        {
            Contract.Requires<ArgumentNullException>(work != null);
            Contract.Requires<ArgumentNullException>(target != null);

            _work = work;
            _target = target;
        }

        /// <summary>
        /// Выполняет над <paramref name="item"/> работу. Результат отправляет в цель, указанную при создании <see cref="WorkingTarget{TItem,TResult}"/>.
        /// </summary>
        /// <param name="item">Элемент, над которым будет выполняться работа.</param>
        /// <returns><see cref="Task"/> для ожидания выполнения работы и отправки результата в цель.</returns>
        public async Task SendAsync(TItem item)
        {
            var result = await _work.ExecuteAsync(item);
            await _target.SendAsync(result);
        }
    }
}