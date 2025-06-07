using FluentValidation;
using Microsoft.Extensions.Localization;
using Localization.Resources;
using Domain.Dtos.Authentication;
using Domain.Exceptions;
using Common;

namespace Domain.Validations;

public class UserValidator : AbstractValidator<LoginRequest>
{

    IStringLocalizer<ErrorMessages> _localizer;
    public UserValidator(IStringLocalizer<ErrorMessages> localizer){
        _localizer = localizer;
        RuleFor(x=> x.Email)
                .NotEmpty().Custom((email, context) =>
                {
                    if (string.IsNullOrEmpty(email))
                    {
                        throw ApiException.NotFound(_localizer["RequiredField"].Value);
                    }
                })
                .EmailAddress().Custom((email, context) =>
                {
                    if (!ValidationHelper.IsValidEmail(email))
                    {
                        throw ApiException.NotFound(_localizer["InvalidEmail"].Value);
                    }
                });
    }
}