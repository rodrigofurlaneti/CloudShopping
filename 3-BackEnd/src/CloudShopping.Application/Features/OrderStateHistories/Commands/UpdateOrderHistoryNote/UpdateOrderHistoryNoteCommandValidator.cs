using FluentValidation;
namespace CloudShopping.Application.Features.OrderStateHistories.Commands.UpdateOrderHistoryNote
{
    public sealed class UpdateOrderHistoryNoteCommandValidator : AbstractValidator<UpdateOrderHistoryNoteCommand>
    {
        public UpdateOrderHistoryNoteCommandValidator()
        {
            RuleFor(x => x.HistoryId).GreaterThan(0);

            RuleFor(x => x.NewNote)
                .NotEmpty().WithMessage("A anotação não pode ser vazia.")
                .MaximumLength(255).WithMessage("A anotação deve ter no máximo 255 caracteres.");
        }
    }
}
