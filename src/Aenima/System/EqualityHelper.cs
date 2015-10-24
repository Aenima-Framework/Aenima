using System;
using System.Collections.Generic;

namespace Aenima.System
{
    /// <summary>A static class that exposes methods for creating generic comparers.</summary>
    public static class EqualityHelper<T>
    {
        public static IEqualityComparer<T> CreateComparer<TV>(Func<T, TV> keySelector)
        {
            return CreateComparer(keySelector, null);
        }

        public static IEqualityComparer<T> CreateComparer<TV>(Func<T, TV> keySelector, IEqualityComparer<TV> comparer)
        {
            return new KeyEqualityComparer<TV>(keySelector, comparer);
        }

        private class KeyEqualityComparer<TV> : IEqualityComparer<T>
        {
            private readonly IEqualityComparer<TV> _comparer;
            private readonly Func<T, TV> _keySelector;

            public KeyEqualityComparer(Func<T, TV> keySelector, IEqualityComparer<TV> comparer)
            {
                if(keySelector == null) {
                    throw new ArgumentNullException(nameof(keySelector));
                }

                _keySelector = keySelector;
                _comparer = comparer ?? EqualityComparer<TV>.Default;
            }

            public bool Equals(T x, T y)
            {
                return _comparer.Equals(_keySelector(x), _keySelector(y));
            }

            public int GetHashCode(T obj)
            {
                return _comparer.GetHashCode(_keySelector(obj));
            }
        }
    }
}