using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Rikrop.Core.Dataflow
{
    public class SimpleEqualityComparer<TItem, TKey> : IEqualityComparer<TItem>
    {
        private readonly Func<TItem, TKey> _propertyValueExtractor;

        public SimpleEqualityComparer(Func<TItem, TKey> propertyValueExtractor)
        {
            Contract.Requires<ArgumentNullException>(propertyValueExtractor != null);

            _propertyValueExtractor = propertyValueExtractor;
        }

        public bool Equals(TItem x, TItem y)
        {
            var key1 = _propertyValueExtractor(x);
            var key2 = _propertyValueExtractor(y);
            return Equals(key1, key2);
        }

        public int GetHashCode(TItem obj)
        {
            var key = _propertyValueExtractor(obj);
            return key.GetHashCode();
        }
    }
}