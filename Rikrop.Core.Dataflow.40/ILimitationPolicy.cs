namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Интерфейс, описывающий политику ограничения доступа.
    /// </summary>
    public interface ILimitationPolicy
    {
        /// <summary>
        /// Возвращает true, если доступ разрешён. Иначе false.
        /// </summary>
        /// <returns>True, если доступ разрешён. Иначе false.</returns>
        bool IsAvailable();
    }
}