using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="RoutingTarget{TItem}"/> �������������� ���������� ����� ����������� �� <see cref="ISelectiveTarget{TItem}"/>.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class RoutingTarget<TItem> : ITarget<TItem>
    {
        private readonly IEnumerable<ISelectiveTarget<TItem>> _targets;

        /// <summary>
        /// ����������� ��� �������� <see cref="RoutingTarget{TItem}"/>.
        /// </summary>
        /// <param name="targets">����� �����, ������ �� ������� ������������� ���������� � ���, ����� �� ����� �� ���� ��������� �������.</param>
        public RoutingTarget(IEnumerable<ISelectiveTarget<TItem>> targets)
        {
            Contract.Requires<ArgumentNullException>(targets != null);
            Contract.Requires<ArgumentNullException>(targets.Any());

            _targets = targets;
        }

        /// <summary>
        /// ���������� ������� � ���� �� �����.
        /// </summary>
        /// <param name="item">������� ��� ��������</param>
        /// <exception cref="InvalidOperationException">�� ������� ����, � ������� ����� ��������� �������.</exception>
        /// <returns><see cref="Task"/> ��� ����, ���� ������������ �������.</returns>
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