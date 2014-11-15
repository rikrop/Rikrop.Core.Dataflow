using System.Diagnostics.Contracts;
using Rikrop.Core.Dataflow.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Интерфейс, описывающий абстрактный конвертер объектов.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    [ContractClass(typeof (ContractIConverter<,>))]
    public interface IConverter<in TInput, out TOutput>
    {
        /// <summary>
        /// Конвертирует элемент.
        /// </summary>
        /// <param name="item">Элемент для конвертации.</param>
        /// <returns>Сконвертированный элемент.</returns>
        TOutput Convert(TInput item);
    }

    namespace Contracts
    {
        [ContractClassFor(typeof (IConverter<,>))]
        public abstract class ContractIConverter<TInput, TOutput> : IConverter<TInput, TOutput>
        {
            public TOutput Convert(TInput item)
            {
                Contract.Assume(!Equals(Contract.Result<TOutput>(), null));
                return default(TOutput);
            }
        }
    }
}