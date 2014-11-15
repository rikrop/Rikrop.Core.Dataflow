using System;
using System.Diagnostics.Contracts;
using System.Threading;
using Rikrop.Core.Framework;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Реализует политику ограничения доступа по времени.
    /// </summary>
    public class TimeoutLimitationPolicy : ILimitationPolicy
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly TimeSpan _lockTimeout;
        private SpinLock _lock;
        private DateTime _nextAvailableDate;

        /// <summary>
        /// Создаёт объект <see cref="TimeoutLimitationPolicy"/>
        /// </summary>
        /// <param name="dateTimeProvider">Провайдер текущей даты и времени.</param>
        /// <param name="lockTimeout">Интервал в течении которого <see cref="IsAvailable"/> будет возвращать false, после первого вызова.</param>
        public TimeoutLimitationPolicy(IDateTimeProvider dateTimeProvider, TimeSpan lockTimeout)
        {
            Contract.Requires<ArgumentNullException>(dateTimeProvider != null);
            Contract.Requires<AggregateException>(lockTimeout != TimeSpan.Zero);

            _dateTimeProvider = dateTimeProvider;
            _lockTimeout = lockTimeout;
            _nextAvailableDate = _dateTimeProvider.Now;

            _lock = new SpinLock();
        }

        /// <summary>
        /// Возвращает true при первом вызове и false на все последующие вызовы в течении времени, указанного при создании <see cref="TimeoutLimitationPolicy"/>.
        /// </summary>
        /// <returns></returns>
        public bool IsAvailable()
        {
            var lockTaken = false;
            try
            {
                if (_nextAvailableDate <= _dateTimeProvider.Now)
                {
                    _lock.Enter(ref lockTaken);
                    if (_nextAvailableDate <= _dateTimeProvider.Now)
                    {
                        _nextAvailableDate = _nextAvailableDate.Add(_lockTimeout);
                        return true;
                    }
                }
                return false;
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit(false);
                }
            }
        }
    }
}