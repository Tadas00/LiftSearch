using FluentValidation;
using LiftSearch.Data.Entities;
using LiftSearch.Data.Entities.Enums;

namespace LiftSearch.Dtos;

public record UserDto(int Id, string name, string lastname, string email, string phone, DriverStatus driverStatus, DateTime registrationDate, int tripsCountTraveler, int cancelledCountTraveler);
public record CreateUserDto(string name, string lastname, string email, string phone);
public record UpdateUserDto(string name, string lastname, string email, string phone);

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(dto => dto.name).NotEmpty().NotNull().Length(min: 2, max: 20);
        RuleFor(dto => dto.lastname).NotEmpty().NotNull().Length(min: 2, max: 30);
        RuleFor(dto => dto.email).NotEmpty().NotNull().EmailAddress().Length(min: 5, max: 30);
        RuleFor(dto => dto.phone).NotEmpty().NotNull().Length(min: 4, max: 12);
    }
}

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(dto => dto.name).NotEmpty().Length(min: 2, max: 20);
        RuleFor(dto => dto.lastname).NotEmpty().Length(min: 2, max: 30);
        RuleFor(dto => dto.email).NotEmpty().EmailAddress().Length(min: 5, max: 30);
        RuleFor(dto => dto.phone).NotEmpty().Length(min: 4, max: 12);
    }
}



