using FluentValidation;
using LiftSearch.Data;
using LiftSearch.Data.Entities;
using LiftSearch.Data.Entities.Enums;
using LiftSearch.Dtos;
using LiftSearch.Endpoints;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using O9d.AspNet.FluentValidation;
using static FluentValidation.DependencyInjectionExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LsDbContext>();

// builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = false);



var services = new ServiceCollection();

builder.Services.AddValidatorsFromAssemblyContaining<UserDto>();
builder.Services.AddValidatorsFromAssemblyContaining<DriverDto>();
builder.Services.AddValidatorsFromAssemblyContaining<TripDto>();
builder.Services.AddValidatorsFromAssemblyContaining<PassengerDto>();

var app = builder.Build();

app.UseExceptionHandler(c => c.Run(async context =>
{
    var exception = context.Features
        .Get<IExceptionHandlerFeature>()
        ?.Error;
    if (exception is not null)
    {
        var response = new { error = exception.Message };
        context.Response.StatusCode = 400;

        await context.Response.WriteAsJsonAsync(response);
    }
}));

var usersGroup = app.MapGroup("/api").WithValidationFilter();
UserEndpoints.AddUserApi(usersGroup);

var driversGroup = app.MapGroup("/api").WithValidationFilter();
DriverEndpoints.AddDriverApi(driversGroup);

var tripsGroup = app.MapGroup("/api/drivers/{driverId}").WithValidationFilter();
TripEndpoints.AddTripApi(tripsGroup);

var passengersGroup = app.MapGroup("/api/drivers/{driverId}/trips/{tripId}").WithValidationFilter();
PassengerEndpoints.AddPassengerApi(passengersGroup);

app.Run();