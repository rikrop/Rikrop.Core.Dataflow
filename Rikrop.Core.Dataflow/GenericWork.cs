using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="GenericWork{TItem,TResult}"/> ��������� ������� �������������.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class GenericWork<TItem, TResult> : IWork<TItem, TResult>
    {
        private readonly Func<TItem, Task<TResult>> _transformFunc;

        /// <summary>
        /// ����������� ��� �������� ����������� <see cref="GenericWork{TItem,TResult}"/>'�.
        /// </summary>
        /// <param name="transformFunc">���������� ������� �������������.</param>
        public GenericWork(Func<TItem, TResult> transformFunc)
        {
            Contract.Requires<ArgumentNullException>(transformFunc != null);

            _transformFunc = o => Task.FromResult(transformFunc(o));
        }

        /// <summary>
        /// ����������� ��� �������� ������������ <see cref="GenericWork{TItem,TResult}"/>'�.
        /// </summary>
        /// <param name="transformFunc">����������� ������� �������������.</param>
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