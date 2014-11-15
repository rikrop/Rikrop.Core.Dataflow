using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Rikrop.Core.Dataflow.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Интерфейс, описывающий абстрактную трансформацию элемента в результат.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    [ContractClass(typeof(ContractIWork<,>))]
    public interface IWork<in TItem, TResult>
    {
        /// <summary>
        /// Выполняет работу над элементом.
        /// </summary>
        /// <param name="item">Элемент, который содержит данные для выполнения работы.</param>
        /// <returns><see cref="Task{TResult}"/> для ожидания результатов работы.</returns>
        Task<TResult> ExecuteAsync(TItem item);
    }

    namespace Contracts
    {
        [ContractClassFor(typeof(IWork<,>))]
        public abstract class ContractIWork<TItem, TResult> : IWork<TItem, TResult>
        {
            public Task<TResult> ExecuteAsync(TItem item)
            {
                Contract.Requires<ArgumentNullException>(!Equals(item, null));
                Contract.Assume(Contract.Result<Task<TResult>>() != null);
                return default(Task<TResult>);
            }
        }
    }
}