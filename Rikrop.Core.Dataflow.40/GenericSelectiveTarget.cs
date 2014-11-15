using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="GenericSelectiveTarget{TItem}"/> ������� �� �������� �������� �������� ����� ���������.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class GenericSelectiveTarget<TItem> : ISelectiveTarget<TItem>
    {
        private readonly Func<TItem, bool> _condition;
        private readonly ITarget<TItem> _target;

        /// <summary>
        /// ����������� ��� �������� <see cref="GenericSelectiveTarget{TItem}"/>.
        /// </summary>
        /// <param name="condition">������� ������� �������� ����� ��� ����.</param>
        /// <param name="target">����, ���� ����� ��������� �������, ���� <paramref name="condition"/> ���������� true.</param>
        public GenericSelectiveTarget(Func<TItem, bool> condition, ITarget<TItem> target)
        {
            Contract.Requires<ArgumentNullException>(condition != null);
            Contract.Requires<ArgumentNullException>(target != null);

            _condition = condition;
            _target = target;
        }

        public bool CanSend(TItem item)
        {
            return _condition(item);
        }

        public Task SendAsync(TItem item)
        {
            return _target.SendAsync(item);
        }
    }
}