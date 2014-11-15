using System;
using System.Diagnostics.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="ScheduledTransmitterItem{TItem}"/> - элемент, содержащий всю необходимую информацию для работы <see cref="ScheduledTransmitter{TItem}"/>'а.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public struct ScheduledTransmitterItem<TItem>
    {
        private readonly TItem _item;
        private readonly bool _stopSchedule;
        private readonly DateTime _scheduleDate;

        /// <summary>
        /// Возвращает реальный элемент, который нужно отправить по расписанию.
        /// </summary>
        public TItem Item
        {
            get { return _item; }
        }

        /// <summary>
        /// Возвращает метку времени, в момент которой необходимо отправить элемент.
        /// </summary>
        public DateTime ScheduleDate
        {
            get { return _scheduleDate; }
        }

        /// <summary>
        /// Возвращает флаг, означающий что элемент, если он есть в расписании, необходимо удалить из расписания.
        /// </summary>
        public bool StopSchedule
        {
            get { return _stopSchedule; }
        }

        /// <summary>
        /// Создаёт новый объект <see cref="ScheduledTransmitterItem{TItem}"/>.
        /// </summary>
        /// <param name="scheduleDate">Метка времени, в момент которой необходимо отправить элемент.</param>
        /// <param name="item">Элемент, который нужно отправить по расписанию.</param>
        /// <param name="stopSchedule">Флаг, означающий что элемент, если он есть в расписании, необходимо удалить из расписания.</param>
        public ScheduledTransmitterItem(DateTime scheduleDate, TItem item, bool stopSchedule = false)
        {
            Contract.Requires<ArgumentNullException>(!Equals(item, null));

            _scheduleDate = scheduleDate;
            _item = item;
            _stopSchedule = stopSchedule;
        }
    }
}