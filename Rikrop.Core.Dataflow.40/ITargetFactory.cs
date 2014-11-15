namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Интерфейс, описывающий абстрактную фабрику создания целей.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public interface ITargetFactory<in TKey, in TItem>
    {
        /// <summary>
        /// Создаёт цель по ключу <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Ключ для создания цели</param>
        /// <returns>Созданную цель.</returns>
        ITarget<TItem> CreateTarget(TKey key);
    }
}