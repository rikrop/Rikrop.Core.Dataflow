using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="ConvertingSource{TInput,TOutput}"/> преобразует элементы источника с помощью <see cref="IConverter{TInput,TOutput}"/>
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class ConvertingSource<TInput, TOutput> : ISource<TOutput>
    {
        private readonly ISource<TInput> _inputSource;
        private readonly IConverter<TInput, TOutput> _converter;

        /// <summary>
        /// Конструктор для создания <see cref="ConvertingSource{TInput,TOutput}"/>
        /// </summary>
        /// <param name="inputSource">Источник элементов для конвертации.</param>
        /// <param name="converter">Конвертер, с помощью которого происходит ковертирование элементов.</param>
        public ConvertingSource(ISource<TInput> inputSource, IConverter<TInput, TOutput> converter)
        {
            Contract.Requires<ArgumentNullException>(inputSource != null);
            Contract.Requires<ArgumentNullException>(converter != null);

            _inputSource = inputSource;
            _converter = converter;
        }

        public async Task<TOutput> ReceiveAsync()
        {
            var item = await _inputSource.ReceiveAsync();
            return _converter.Convert(item);
        }
    }
}