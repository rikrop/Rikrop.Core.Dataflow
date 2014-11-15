using System;
using System.Diagnostics.Contracts;
using Rikrop.Core.Framework;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Фабрика для создания <see cref="TimeoutLimitationPolicy"/>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TimeoutLimitationPolicyFactory<TKey> : ILimitationPolicyFactory<TKey>
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly TimeSpan _lockTimeout;

        public TimeoutLimitationPolicyFactory(IDateTimeProvider dateTimeProvider, TimeSpan lockTimeout)
        {
            Contract.Requires<ArgumentNullException>(dateTimeProvider != null);
            Contract.Requires<ArgumentException>(lockTimeout != TimeSpan.Zero);

            _dateTimeProvider = dateTimeProvider;
            _lockTimeout = lockTimeout;
        }

        public ILimitationPolicy CreatePolicyFor(TKey key)
        {
            return new TimeoutLimitationPolicy(_dateTimeProvider, _lockTimeout);
        }
    }
}