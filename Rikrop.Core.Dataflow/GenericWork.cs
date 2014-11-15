using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="GenericWork{TItem,TResult}"/> исполняет делегат трансформации.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class GenericWork<TItem, TResult> : IWork<TItem, TResult>
    {
        private readonly Func<TItem, Task<TResult>> _transformFunc;

        /// <summary>
        /// Конструктор для создания синхронного <see cref="GenericWork{TItem,TResult}"/>'а.
        /// </summary>
        /// <param name="transformFunc">Синхронный делегат трансформации.</param>
        public GenericWork(Func<TItem, TResult> transformFunc)
        {
            Contract.Requires<ArgumentNullException>(transformFunc != null);

            _transformFunc = o => Task.FromResult(transformFunc(o));
        }

        /// <summary>
        /// Конструктор для создания асинхронного <see cref="GenericWork{TItem,TResult}"/>'а.
        /// </summary>
        /// <param name="transformFunc">Асинхронный делегат трансформации.</param>
        public GenericWork(Func<TItem, Task<TResult>> transformFunc)
        {
            Contract.Requires<ArgumentNullException>(transformFunc != null);

            _transformFunc = transformFunc;
        }

        public Task<TResult> ExecuteAsync(TItem item)
        {
            return _transformFunc(item);
        }
    }
}