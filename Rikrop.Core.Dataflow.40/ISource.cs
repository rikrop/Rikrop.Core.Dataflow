using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Rikrop.Core.Dataflow.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Интерфейс, описывающий абстрактный источник элементов
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ContractClass(typeof(ContractISource<>))]
    public interface ISource<T>
    {
        /// <summary>
        /// Возвращает элемент из источника.
        /// </summary>
        /// <returns><see cref="Task{TResult}"/> для получения элемента.</returns>
        Task<T> ReceiveAsync();
    }

    /// <summary>
    /// Интерфейс, описывающий абстрактный источник элементов
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TParam"></typeparam>
    [ContractClass(typeof(ContractISource<,>))]
    public interface ISource<TItem, in TParam>
    {
        /// <summary>
        /// Возвращает элемент из источника используя параметр.
        /// </summary>
        /// <param name="param">Параметр, на основе которого возвращается элемент.</param>
        /// <returns><see cref="Task{TResult}"/> для получения элемента.</returns>
        Task<TItem> ReceiveAsync(TParam param);
    }

    namespace Contracts
    {
        [ContractClassFor(typeof(ISource<>))]
        public abstract class ContractISource<T> : ISource<T>
        {
            public Task<T> ReceiveAsync()
            {
                Contract.Assume(Contract.Result<Task<T>>() != null);
                return default(Task<T>);
            }
        }

        [ContractClassFor(typeof(ISource<,>))]
        public abstract class ContractISource<T, TParam> : ISource<T, TParam>
        {
            public Task<T> ReceiveAsync(TParam param)
            {
                Contract.Assume(Contract.Result<Task<T>>() != null);
                return default(Task<T>);
            }
        }
    }
}