using FluentValidation;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Validators;

public class CreateProductionOrderRequestValidator : AbstractValidator<CreateProductionOrderRequest>
{
    public CreateProductionOrderRequestValidator()
    {
        RuleFor(x => x.UniqueCode)
            .NotEmpty().WithMessage("Unique Code is required.")
            .MaximumLength(50).WithMessage("Unique Code cannot exceed 50 characters.");

        RuleFor(x => x.ProductDescription)
            .NotEmpty().WithMessage("Product Description is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.EstimatedDeliveryDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Delivery date must be in the future.");
    }
}
