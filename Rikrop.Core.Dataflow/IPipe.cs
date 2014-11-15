namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Интерфейс, описывающий абстрактную трубу, хранящую элементы.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public interface IPipe<TInput, in TOutput> : ISource<TInput>, ITarget<TOutput>
    {
    }
}