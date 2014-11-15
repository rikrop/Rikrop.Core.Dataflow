using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Rikrop.Core.Framework.Exceptions;
using Rikrop.Core.Framework.Monitoring;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Базовый класс для всех трансмиттеров. Содержит в себе набор счётчиков для мониторинга своего состояния. 
    /// Наследники, переопределяя метод Transmit и используя методы SendAsync и ReceiveAsync, могут строить свою логику передачи элементов из источника в цель. 
    /// Методы Transmit и SendAsync защищены обработчиком исключений. 
    /// Метод ReceiveAsync возвращает исключение, которое обработается обработчиком вокруг Transmit, если в наследнике оно не будет перехвачено.
    /// </summary>
    /// <typeparam name="TInput">Входной тип данных из источника</typeparam>
    /// <typeparam name="TOutput">Выходной тип данных в цель</typeparam>
    public abstract class Transmitter<TInput, TOutput> : ITransmitter
    {
        private readonly ISource<TInput> _source;
        private readonly ITarget<TOutput> _target;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly StateCounters _counters;

        private bool _isStarted;

        public bool IsStarted
        {
            get { return _isStarted; }
        }

        public StateCounters Counters
        {
            get { return _counters; }
        }

        protected Transmitter(ISource<TInput> source,
                              ITarget<TOutput> target,
                              IExceptionHandler exceptionHandler)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentNullException>(exceptionHandler != null);

            _source = source;
            _target = target;
            _exceptionHandler = exceptionHandler;

            _counters = new StateCounters();
        }

        public async Task StartAsync()
        {
            if (_isStarted)
            {
                return;
            }

            try
            {
                _isStarted = true;
                while (_isStarted)
                {
                    try
                    {
                        await Transmit();
                    }
                    catch (Exception ex)
                    {
                        if (!_exceptionHandler.Handle(ex))
                        {
                            break;
                        }
                    }
                }
            }
            finally
            {
                Stop();
            }
        }

        public void Stop()
        {
            _isStarted = false;
        }

        public void InitializeCounters(StateCounters counters)
        {
            Counters.FailedItemsCount = counters.FailedItemsCount;
            Counters.InWorkItemsCount = counters.InWorkItemsCount;
            Counters.ReceivedItemsCount = counters.ReceivedItemsCount;
            Counters.SheduledItemsCount = counters.SheduledItemsCount;
            Counters.SourceCallFailedCount = counters.SourceCallFailedCount;
        }

        protected abstract Task Transmit();

        protected async virtual Task SendAsync(TOutput item)
        {
            _counters.SheduledItemsCount.SafeDecrement();
            _counters.InWorkItemsCount.SafeIncrement();

            try
            {
                await _target.SendAsync(item);
            }
            catch (Exception ex)
            {
                _counters.FailedItemsCount.SafeIncrement();

                if (!_exceptionHandler.Handle(ex))
                {
                    Stop();
                }
            }
            finally
            {
                _counters.InWorkItemsCount.SafeDecrement();
            }
        }

        protected async virtual Task<TInput> ReceiveAsync()
        {
            try
            {
                var result = await _source.ReceiveAsync();
                
                _counters.ReceivedItemsCount.SafeIncrement();
                _counters.SheduledItemsCount.SafeIncrement();

                return result;
            }
            catch (Exception)
            {
                _counters.SourceCallFailedCount.SafeIncrement();
                throw;
            }
        }

        public class StateCounters
        {
            public ICounter ReceivedItemsCount { get; set; }
            public ICounter SourceCallFailedCount { get; set; }
            public IDecrementableCounter SheduledItemsCount { get; set; }
            public IDecrementableCounter InWorkItemsCount { get; set; }
            public ICounter FailedItemsCount { get; set; }
        }
    }
}