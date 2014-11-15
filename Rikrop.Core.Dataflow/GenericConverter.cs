using System;
using System.Diagnostics.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Конвертер объектов основанный на делегате конвертирования.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class GenericConverter<TInput, TOutput> : IConverter<TInput, TOutput>
    {
        private readonly Func<TInput, TOutput> _keyExtractDelegate;

        /// <summary>
        /// Конструктор для создания <see cref="GenericConverter{TInput,TOutput}"/>.
        /// </summary>
        /// <param name="keyExtractDelegate">Делегат конвертирования объектов.</param>
        public GenericConverter(Func<TInput, TOutput> keyExtractDelegate)
        {
            Contract.Requires<ArgumentNullException>(keyExtractDelegate != null);

            _keyExtractDelegate = keyExtractDelegate;
        }

        public TOutput Convert(TInput item)
        {
            return _keyExtractDelegate(item);
        }
    }
}