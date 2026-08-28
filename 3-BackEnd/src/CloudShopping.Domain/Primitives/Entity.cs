namespace CloudShopping.Domain.Primitives
{
    public abstract class Entity<TId> : IEquatable<Entity<TId>>
    {
        public TId Id { get; protected set; }

        protected Entity(TId id)
        {
            Id = id;
        }

        protected Entity() { } // Construtor vazio para ORM (Entity Framework)

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType())
                return false;

            var other = (Entity<TId>)obj;
            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        public override int GetHashCode() => Id!.GetHashCode();

        public bool Equals(Entity<TId>? other)
        {
            if (other is null || other.GetType() != GetType())
                return false;

            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }
    }
}
