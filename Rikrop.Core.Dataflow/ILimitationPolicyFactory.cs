using System;
using System.Diagnostics.Contracts;
using Rikrop.Core.Dataflow.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Интерфейс, описывающий абстрактную фабрику создания <see cref="ILimitationPolicy"/>.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    [ContractClass(typeof (ContractILimitationPolicyFactory<>))]
    public interface ILimitationPolicyFactory<in TKey>
    {
        ILimitationPolicy CreatePolicyFor(TKey key);
    }

    namespace Contracts
    {
        [ContractClassFor(typeof (ILimitationPolicyFactory<>))]
        public abstract class ContractILimitationPolicyFactory<TKey> : ILimitationPolicyFactory<TKey>
        {
            public ILimitationPolicy CreatePolicyFor(TKey key)
            {
                Contract.Requires<ArgumentNullException>(!Equals(key, null));
                Contract.Ensures(Contract.Result<ILimitationPolicy>() != null);
                return default(ILimitationPolicy);
            }
        }
    }
}