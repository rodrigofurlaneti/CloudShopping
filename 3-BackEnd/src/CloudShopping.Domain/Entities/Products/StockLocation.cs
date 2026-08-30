using CloudShopping.Domain.Primitives;
namespace CloudShopping.Domain.Entities.Products
{
    public sealed class StockLocation : ValueObject
    {
        public string Aisle { get; }      // Corredor / Rua
        public string Rack { get; }       // Estante
        public string Level { get; }      // Nível / Prateleira
        public string Position { get; }   // Posição / Vão
        private StockLocation(string aisle, string rack, string level, string position)
        {
            Aisle = aisle;
            Rack = rack;
            Level = level;
            Position = position;
        }

        public static StockLocation Create(string aisle, string rack, string level, string position)
        {
            if (string.IsNullOrWhiteSpace(aisle) || string.IsNullOrWhiteSpace(rack) ||
                string.IsNullOrWhiteSpace(level) || string.IsNullOrWhiteSpace(position))
            {
                throw new ArgumentException("Todos os níveis de endereçamento logístico são obrigatórios.");
            }
            return new StockLocation(aisle.Trim().ToUpper(), rack.Trim().ToUpper(), level.Trim().ToUpper(), position.Trim().ToUpper());
        }
        public override string ToString() => $"{Aisle}-{Rack}-{Level}-{Position}";
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Aisle;
            yield return Rack;
            yield return Level;
            yield return Position;
        }
    }
}
