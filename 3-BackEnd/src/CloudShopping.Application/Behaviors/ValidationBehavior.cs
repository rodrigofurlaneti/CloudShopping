using CloudShopping.Domain.Primitives.Results;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Result
    {
        // Reflete a assinatura genérica de Result.Failure<TValue>(Error) uma única vez,
        // já que TResponse aqui pode ser tanto "Result" (comandos) quanto "Result<T>"
        // (queries) — não dá pra saber TValue em tempo de compilação dentro desta classe genérica.
        private static readonly MethodInfo GenericFailureMethod = typeof(Result)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == nameof(Result.Failure) && method.IsGenericMethodDefinition);

        private readonly IEnumerable<IValidator<TRequest>> _validators;
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }
            var context = new ValidationContext<TRequest>(request);
            var errors = _validators
                .Select(validator => validator.Validate(context))
                .SelectMany(validationResult => validationResult.Errors)
                .Where(validationFailure => validationFailure != null)
                .Select(failure => new Error(failure.PropertyName, failure.ErrorMessage))
                .Distinct()
                .ToArray();
            if (errors.Any())
            {
                return BuildFailureResponse(errors[0]);
            }
            return await next();
        }

        // Result.Failure(error) devolve uma instância de "Result" pura — que NÃO é o
        // mesmo tipo em runtime de "Result<TValue>" (são classe base/derivada, não
        // conversíveis entre si por cast). Retornar isso via "dynamic" fazia o DLR
        // tentar (e falhar) uma conversão implícita Base -> Derivada em runtime,
        // estourando RuntimeBinderException sempre que a validação de uma query
        // (TResponse = Result<T>) falhava. Aqui construímos o Result<T> de falha
        // correto via reflection, chamando Result.Failure<TValue>(error) com o TValue
        // real de TResponse — e, para comandos (TResponse = Result puro), usamos o
        // Failure(Error) não-genérico diretamente.
        private static TResponse BuildFailureResponse(Error error)
        {
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(error);
            }

            var valueType = typeof(TResponse).GetGenericArguments()[0];
            var typedFailureMethod = GenericFailureMethod.MakeGenericMethod(valueType);
            return (TResponse)typedFailureMethod.Invoke(null, new object[] { error })!;
        }
    }
}