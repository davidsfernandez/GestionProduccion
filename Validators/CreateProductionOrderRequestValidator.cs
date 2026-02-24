using FluentValidation;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Validators;

public class CreateProductionOrderRequestValidator : AbstractValidator<CreateProductionOrderRequest>
{
    public CreateProductionOrderRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required.")
            .GreaterThan(0).WithMessage("Valid Product ID is required.");

        RuleFor(x => x.Quantity)
            .NotEmpty().WithMessage("Quantity is required.")
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.EstimatedCompletionAt)
            .NotEmpty().WithMessage("Estimated completion date is required.")
            .Must(date => date > DateTime.Now).WithMessage("Completion date must be in the future.");
    }
}
