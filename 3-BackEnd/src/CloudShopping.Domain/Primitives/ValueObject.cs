using System.Collections.Generic;
using System.Linq;

namespace CloudShopping.Domain.Primitives
{
    /// <summary>
    /// Classe base para Value Objects: objetos sem identidade própria,
    /// cuja igualdade é determinada pela igualdade de todos os seus valores atômicos.
    /// </summary>
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object?> GetAtomicValues();

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType())
                return false;

            var other = (ValueObject)obj;
            return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
        }

        public override int GetHashCode()
        {
            return GetAtomicValues()
                .Select(x => x?.GetHashCode() ?? 0)
                .Aggregate(17, (current, hash) => current * 31 + hash);
        }

        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
    }
}
