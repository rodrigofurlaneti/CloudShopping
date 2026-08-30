using CloudShopping.Domain.Primitives;
using CloudShopping.Domain.Primitives.Results;
using System.Collections.Generic;

namespace CloudShopping.Domain.ValueObjects
{
    public sealed class Email : ValueObject
    {
        public const int MaxLength = 100;
        public string Value { get; }
        private Email(string value)
        {
            Value = value;
        }
        public static Result<Email> Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result.Failure<Email>(new Error("Email.Empty", "Email não pode estar vazio."));
            if (email.Length > MaxLength)
                return Result.Failure<Email>(new Error("Email.TooLong", $"Email deve ter menos que {MaxLength} caracteres."));
            if (!email.Contains("@")) // Simplificado, usar RegEx no mundo real
                return Result.Failure<Email>(new Error("Email.Invalid", "Formato de email inválido."));
            return Result.Success(new Email(email));
        }
        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return Value;
        }
    }
}
