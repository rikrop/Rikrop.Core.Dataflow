using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Rikrop.Core.Dataflow.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// ���������, ����������� ����������� ����, ������� ����� ���������� ������ ������������ ����� ���������.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    [ContractClass(typeof(ContractISelectiveTarget<>))]
    public interface ISelectiveTarget<in TItem> : ITarget<TItem>
    {
        /// <summary>
        /// ���������� true, ���� <paramref name="item"/> ����� ���� ��������� ����� ��� ����. ����� false.
        /// </summary>
        /// <param name="item">������� ��� �������� ����� ���������.</param>
        /// <returns>True, ���� <paramref name="item"/> ����� ���� ��������� ����� ��� ����. ����� false.</returns>
        bool CanSend(TItem item);
    }

    namespace Contracts
    {
        [ContractClassFor(typeof(ISelectiveTarget<>))]
        public abstract class ContractISelectiveTarget<TItem> : ISelectiveTarget<TItem>
        {
            public Task SendAsync(TItem item)
            {
                Contract.Assume(CanSend(item));

                throw new NotSupportedException();
            }

            public abstract bool CanSend(TItem item);
        }
    }
}