namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Способ отправки элемента в <see cref="BroadcastingTarget{TItem}"/>
    /// </summary>
    public enum BroadcastMethod
    {
        /// <summary>
        /// Ожидать отправки элемента в каждую цель последовательно.
        /// </summary>
        AwaitEachTarget,

        /// <summary>
        /// Отправить элемент во все цели и потом ждать завершения отправки во все цели.
        /// </summary>
        AwaitAllTargets,
    }
}