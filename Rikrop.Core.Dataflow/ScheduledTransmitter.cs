using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Rikrop.Core.Framework;
using Rikrop.Core.Framework.Exceptions;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    ///     <see cref="ScheduledTransmitter{TItem}" /> реализует алгоритм передачи элементов по расписанию.
    ///     Получая из источника элемент типа <see cref="ScheduledTransmitterItem{TItem}" />, который содержит информацию о времени и параметрах распиания элемента,
    ///     <see cref="ScheduledTransmitter{TItem}" /> планирует своё распиание и отправляет элементы в цель по заданным меткам времени.
    ///     Время элементов округляется до одной секунды, чтобы снизить накладные расходы на отправку.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class ScheduledTransmitter<TItem> : Transmitter<ScheduledTransmitterItem<TItem>, TItem>
    {
        private readonly TimeSpan _maxTimerInterval = TimeSpan.FromMilliseconds(int.MaxValue);

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ItemsByDateCollection<TItem> _itemsCollection;
        private readonly ITimer _timer;

        private DateTime? _nearestSendDate;

        /// <summary>
        ///     Конструктор для саоздния объекта <see cref="ScheduledTransmitter{TItem}" />.
        /// </summary>
        /// <param name="source">Источник элементов.</param>
        /// <param name="target">Цель для отпрвки.</param>
        /// <param name="dateTimeProvider">Провайдер текущей даты для составления расписания.</param>
        /// <param name="exceptionHandler">Обработчик исключений, возникающих во время получения или отправки.</param>
        public ScheduledTransmitter(ISource<ScheduledTransmitterItem<TItem>> source,
                                    ITarget<TItem> target,
                                    IDateTimeProvider dateTimeProvider,
                                    IExceptionHandler exceptionHandler)
            : this(source, target, dateTimeProvider, new SimpleTimer{AutoReset = true}, exceptionHandler)
        {
        }

        /// <summary>
        ///     Конструктор для саоздния объекта <see cref="ScheduledTransmitter{TItem}" />.
        /// </summary>
        /// <param name="source">Источник элементов.</param>
        /// <param name="target">Цель для отпрвки.</param>
        /// <param name="dateTimeProvider">Провайдер текущей даты для составления расписания.</param>
        /// <param name="timer">Таймер, который используется для составления расписания.</param>
        /// <param name="exceptionHandler">Обработчик исключений, возникающих во время получения или отправки.</param>
        public ScheduledTransmitter(ISource<ScheduledTransmitterItem<TItem>> source,
                                    ITarget<TItem> target,
                                    IDateTimeProvider dateTimeProvider,
                                    ITimer timer,
                                    IExceptionHandler exceptionHandler)
            : base(source, target, exceptionHandler)
        {
            Contract.Requires<ArgumentNullException>(dateTimeProvider != null);
            Contract.Requires<ArgumentNullException>(timer != null);

            _dateTimeProvider = dateTimeProvider;

            _itemsCollection = new ItemsByDateCollection<TItem>();
            _timer = timer;
            _timer.Tick += OnTimerElapsed;
        }

        protected override async Task Transmit()
        {
            var item = await ReceiveAsync();

            ScheduleItem(item);
        }

        protected void ScheduleItem(ScheduledTransmitterItem<TItem> transmitterItem)
        {
            lock (_timer)
            {
                StoreItem(transmitterItem);

                if (IsNearestDateChanged())
                {
                    TrySendItems();
                }
            }
        }

        private void OnTimerElapsed(object sender, EventArgs e)
        {
            lock (_timer)
            {
                _timer.Stop();
                TrySendItems();
            }
        }

        private void TrySendItems()
        {
            StopTimer();

            var now = _dateTimeProvider.Now;
            var itemsToSend = _itemsCollection.TakeItemsTill(now);

            DateTime nextSendDate;
            if (_itemsCollection.TryGetNearestDate(out nextSendDate))
            {
                _nearestSendDate = nextSendDate;
                StartTimer(nextSendDate - now);
            }
            else
            {
                _nearestSendDate = null;
            }

            SendItemsAsync(itemsToSend);
        }

        private bool IsNearestDateChanged()
        {
            DateTime nearestDate;
            if (_itemsCollection.TryGetNearestDate(out nearestDate))
            {
                return !_nearestSendDate.HasValue || _nearestSendDate.Value != nearestDate;
            }
            return _nearestSendDate.HasValue;
        }

        private async void StoreItem(ScheduledTransmitterItem<TItem> transmitterItem)
        {
            if (transmitterItem.StopSchedule)
            {
                _itemsCollection.Remove(transmitterItem.Item);
            }
            else
            {
                var roundedDate = GetRoundedDate(transmitterItem.ScheduleDate);

                if (roundedDate <= _dateTimeProvider.Now)
                {
                    await SendAsync(transmitterItem.Item);
                }
                else
                {
                    _itemsCollection.Add(transmitterItem.Item, roundedDate);
                }
            }
        }

        private DateTime GetRoundedDate(DateTime date)
        {
            return new DateTime(year: date.Year, month: date.Month, day: date.Day, hour: date.Hour, minute: date.Minute, second: date.Second);
        }

        private void StartTimer(TimeSpan sleepInterval)
        {
            Contract.Assume(!_timer.IsEnabled);

            _timer.Interval = sleepInterval > _maxTimerInterval
                                  ? _maxTimerInterval
                                  : sleepInterval;
            _timer.Start();
        }

        private async void SendItemsAsync(IReadOnlyCollection<TItem> itemsToSend)
        {
            if (itemsToSend != null && itemsToSend.Count > 0)
            {
                foreach (var item in itemsToSend)
                {
                    await SendAsync(item);
                }
            }
        }

        private void StopTimer()
        {
            _timer.Stop();
        }
    }
}