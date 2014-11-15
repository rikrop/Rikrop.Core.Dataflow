using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="InfinityPipe{TItem}"/> реализует логику потокобезопасной бесконечной трубы, которая гарантирует приём и отправку элементов по запросу, даже если элементов внутри нет.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class InfinityPipe<TItem> : IPipe<TItem, TItem>, IPipe<TItem, IEnumerable<TItem>>
    {
        private readonly IItemsContainer<TItem> _itemsContainer;
        private readonly ConcurrentQueue<TaskCompletionSource<TItem>> _awaitedSources;
        private readonly Task _completedTask;
        private readonly ReaderWriterLockSlim _readerWriterLock;

        public int Count
        {
            get { return _itemsContainer.Count; }
        }

        /// <summary>
        /// Конструктор для создания <see cref="InfinityPipe{TItem}"/>.
        /// </summary>
        /// <param name="itemsContainer">Внутреннее хранилище элементов.</param>
        public InfinityPipe(IItemsContainer<TItem> itemsContainer)
        {
            Contract.Requires<ArgumentNullException>(itemsContainer != null);

            _itemsContainer = itemsContainer;
            _awaitedSources = new ConcurrentQueue<TaskCompletionSource<TItem>>();
            _completedTask = Task.FromResult(true);
            _readerWriterLock = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Возвращает <see cref="Task{TResult}"/> с элементом из контейнера. 
        /// Если элементов в контейнере нет, то возвращает <see cref="Task{TResult}"/> из <see cref="TaskCompletionSource{TResult}"/>, 
        /// чтобы перевести вызывающую сторону в режим ожидания результата. Результат может быть установлен в методах <see cref="SendAsync(TItem)"/> и <see cref="SendAsync(IEnumerable{TItem})"/>.
        /// </summary>
        /// <returns><see cref="Task{TResult}"/> с элементом из контейнера или для ожидания элемента от <see cref="SendAsync(TItem)"/> и <see cref="SendAsync(IEnumerable{TItem})"/>.</returns>
        public Task<TItem> ReceiveAsync()
        {
            TItem item;
            if (_itemsContainer.TryGet(out item))
            {
                return Task.FromResult(item);
            }

            TaskCompletionSource<TItem> taskCompletionSource;
            try
            {
                _readerWriterLock.EnterWriteLock();

                if (_itemsContainer.TryGet(out item))
                {
                    return Task.FromResult(item);
                }

                taskCompletionSource = new TaskCompletionSource<TItem>();
                _awaitedSources.Enqueue(taskCompletionSource);
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Отправляет элемент во внутреннее хранилище элементов. 
        /// Если у <see cref="InfinityPipe{TItem}"/> в данный момент есть ожидающие в методе <see cref="ReceiveAsync"/>, то берёт элемент из внутреннего хранилища и отправляет элемент им.
        /// </summary>
        /// <param name="item">Элемент для отправки во внутреннее хранилище.</param>
        /// <returns><see cref="Task{True}"/></returns>
        public Task SendAsync(TItem item)
        {
            if (_itemsContainer.TryAdd(item))
            {
                TrySendToAwaitedSources();
            }

            return _completedTask;
        }

        /// <summary>
        /// Отправляет пачку элементов во внутреннее хранилище элементов. 
        /// Если у <see cref="InfinityPipe{TItem}"/> в данный момент есть ожидающие в методе <see cref="ReceiveAsync"/>, то после того как вся пачка будет передана во внутреннее хранилище 
        /// <see cref="SendAsync(TItem)"/> переходит в режим отправки элементов ожидающим, забирая элементы из внутреннего хранилища.
        /// </summary>
        /// <param name="items">Пачка элементов для отправки во внутреннее хранилище.</param>
        /// <returns><see cref="Task{True}"/></returns>
        public Task SendAsync(IEnumerable<TItem> items)
        {
            var wasAdded = false;
            foreach (var item in items)
            {
                wasAdded |= _itemsContainer.TryAdd(item);
            }

            if (wasAdded)
            {
                TrySendToAwaitedSources();
            }

            return _completedTask;
        }

        private void TrySendToAwaitedSources()
        {
            while (true)
            {
                bool hasAwaiters;
                TaskCompletionSource<TItem> taskCompletionSource;
                try
                {
                    _readerWriterLock.EnterReadLock();

                    hasAwaiters = _awaitedSources.TryDequeue(out taskCompletionSource);
                }
                finally
                {
                    _readerWriterLock.ExitReadLock();
                }
                
                if (!hasAwaiters)
                {
                    break;
                }

                try
                {
                    TItem newItem;
                    if (_itemsContainer.TryGet(out newItem))
                    {
                        taskCompletionSource.TrySetResult(newItem);
                    }
                    else
                    {
                        _awaitedSources.Enqueue(taskCompletionSource);
                        break;
                    }
                }
                catch (Exception)
                {
                    _awaitedSources.Enqueue(taskCompletionSource);
                    throw;
                }
            }

        }
    }
}