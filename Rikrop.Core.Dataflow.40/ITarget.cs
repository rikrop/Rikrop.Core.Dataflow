using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Rikrop.Core.Dataflow.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Интерфейс, описывающий абстрактную цель, куда можно отправлять элементы.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ContractClass(typeof(ContractITarget<>))]
    public interface ITarget<in T>
    {
        /// <summary>
        /// Отправляет элемент в цель.
        /// </summary>
        /// <param name="item">Элемент для отправки.</param>
        /// <returns><see cref="Task"/> для ожидания отправки.</returns>
        Task SendAsync(T item);
    }

    namespace Contracts
    {
        [ContractClassFor(typeof(ITarget<>))]
        public abstract class ContractITarget<T> : ITarget<T>
        {
            public Task SendAsync(T item)
            {
                Contract.Requires<ArgumentNullException>(!Equals(item, null));
                Contract.Assume(Contract.Result<Task>() != null);
                return default(Task);
            }
        }
    }
}