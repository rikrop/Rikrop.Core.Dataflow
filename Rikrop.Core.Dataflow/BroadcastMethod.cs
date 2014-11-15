namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// ������ �������� �������� � <see cref="BroadcastingTarget{TItem}"/>
    /// </summary>
    public enum BroadcastMethod
    {
        /// <summary>
        /// ������� �������� �������� � ������ ���� ���������������.
        /// </summary>
        AwaitEachTarget,

        /// <summary>
        /// ��������� ������� �� ��� ���� � ����� ����� ���������� �������� �� ��� ����.
        /// </summary>
        AwaitAllTargets,
    }
}