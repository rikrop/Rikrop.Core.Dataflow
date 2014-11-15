using System.Threading.Tasks;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Интерфейс, описывающий абстрактный передатчик элементов.
    /// </summary>
    public interface ITransmitter
    {
        /// <summary>
        /// Запусткает передатчик. Возвращает <see cref="Task"/>, который завершится после вызова метода <see cref="Stop"/>.
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// Останавливает передатчик.
        /// </summary>
        void Stop();
    }
}