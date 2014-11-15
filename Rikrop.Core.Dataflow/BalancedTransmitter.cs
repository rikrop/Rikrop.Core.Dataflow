using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Rikrop.Core.Framework.Exceptions;
using Rikrop.Core.Threading;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="BalancedTransmitter{TItem}"/> реализует алгоритм передачи элементов следя за кол-вом отсылаемых элементов. Каждый элемент он отправляет в цель в потоке <see cref="ThreadPool"/>'а.
    /// <see cref="BalancedTransmitter{TItem}"/> начинает набирать элементы из источника до тех пор, пока не достигнет maximumThreshold. 
    /// Далее он будет ожидать, пока кол-во отсылаемых элементов не упадёт до minimumThreshold, после чего он опять начнёт набирать элементы из источника.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class BalancedTransmitter<TItem> : Transmitter<TItem, TItem>
    {
        private readonly int _maximumThreshold;
        private readonly CountdownTaskScheduler _scheduler;
        private readonly TaskFactory _taskFactory;

        /// <summary>
        /// Конструктор для саоздния объекта <see cref="BalancedTransmitter{TItem}"/>.
        /// </summary>
        /// <param name="source">Источник элементов.</param>
        /// <param name="target">Цель для отпрвки.</param>
        /// <param name="exceptionHandler">Обработчик исключений, возникающих во время получения или отправки.</param>
        /// <param name="minimumThreshold">Минимальное кол-во элементов находящихся в отпрвке, по достижении которого <see cref="BalancedTransmitter{TItem}"/> начинает опрашивать <param name="source"/>.</param>
        /// <param name="maximumThreshold">Максимальное кол-во элементов находящихся в отпрвке, по достижении которого <see cref="BalancedTransmitter{TItem}"/> перестаёт опрашивать <param name="source"/>.</param>
        public BalancedTransmitter(ISource<TItem> source,
                                   ITarget<TItem> target,
                                   IExceptionHandler exceptionHandler,
                                   int minimumThreshold,
                                   int maximumThreshold)
            : base(source, target, exceptionHandler)
        {
            Contract.Requires<ArgumentException>(minimumThreshold >= 0);
            Contract.Requires<ArgumentException>(minimumThreshold < maximumThreshold);

            _maximumThreshold = maximumThreshold;

            _scheduler = new CountdownTaskScheduler(minimumThreshold);
            _taskFactory = new TaskFactory(_scheduler);
        }

        protected override async Task Transmit()
        {
            await _scheduler.WaitAsync();

            while (_scheduler.TrackedTasksCount < _maximumThreshold && IsStarted)
            {
                var item = await ReceiveAsync();

                ScheduleItemSend(item);
            }
        }

        private void ScheduleItemSend(TItem item)
        {
            _taskFactory.StartNew(() => SendAsync(item));
        }
    }
}