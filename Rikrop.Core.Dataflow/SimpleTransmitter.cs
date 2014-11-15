using System.Threading.Tasks;
using Rikrop.Core.Framework.Exceptions;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="SimpleTransmitter{TItem}"/> просто передаёт элементы из источника в цель без какой-либо дополнительной логики.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class SimpleTransmitter<TItem> : Transmitter<TItem, TItem>
    {
        /// <summary>
        /// Конструктор для саоздния объекта <see cref="SimpleTransmitter{TItem}"/>.
        /// </summary>
        /// <param name="source">Источник элементов.</param>
        /// <param name="target">Цель для отпрвки.</param>
        /// <param name="exceptionHandler">Обработчик исключений, возникающих во время получения или отправки.</param>
        public SimpleTransmitter(ISource<TItem> source,
                                 ITarget<TItem> target,
                                 IExceptionHandler exceptionHandler)
            : base(source, target, exceptionHandler)
        {
        }

        protected override async Task Transmit()
        {
            var item = await ReceiveAsync();

            await SendAsync(item);
        }
    }
}