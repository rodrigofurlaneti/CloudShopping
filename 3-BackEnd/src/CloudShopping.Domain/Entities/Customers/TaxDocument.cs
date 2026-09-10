namespace CloudShopping.Domain.Entities.Customers;
public static class TaxDocument
{
    public static bool IsValid(string value)
    {
        if (value.Length is not (11 or 14) || !value.All(char.IsAsciiDigit) || value.Distinct().Count() == 1) return false;
        int Calculate(int length) {
            var sum = 0;
            for (int i = 0; i < length; i++) {
                int weight = value.Length == 11 ? length + 1 - i : (length - 1 - i) % 8 + 2;
                sum += (value[i] - '0') * weight;
            }
            int rest = sum % 11; return rest < 2 ? 0 : 11 - rest;
        }
        return Calculate(value.Length - 2) == value[^2] - '0' && Calculate(value.Length - 1) == value[^1] - '0';
    }
}
