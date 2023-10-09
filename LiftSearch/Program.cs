using FluentValidation;
using LiftSearch.Data;
using LiftSearch.Data.Entities;
using LiftSearch.Data.Entities.Enums;
using LiftSearch.Dtos;
using LiftSearch.Endpoints;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using O9d.AspNet.FluentValidation;
using static FluentValidation.DependencyInjectionExtensions;

var builder = WebApplication.CreateBuilder(args);
    //   .AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddDbContext<LsDbContext>();

var services = new ServiceCollection();

builder.Services.AddValidatorsFromAssemblyContaining<UserDto>();
builder.Services.AddValidatorsFromAssemblyContaining<DriverDto>();
builder.Services.AddValidatorsFromAssemblyContaining<TripDto>();
builder.Services.AddValidatorsFromAssemblyContaining<PassengerDto>();

var app = builder.Build();

var usersGroup = app.MapGroup("/api").WithValidationFilter();
UserEndpoints.AddUserApi(usersGroup);

var driversGroup = app.MapGroup("/api").WithValidationFilter();
DriverEndpoints.AddDriverApi(driversGroup);

var tripsGroup = app.MapGroup("/api/drivers/{driverId}").WithValidationFilter();
TripEndpoints.AddTripApi(tripsGroup);

var passengersGroup = app.MapGroup("/api/drivers/{driverId}/trips/{tripId}").WithValidationFilter();
PassengerEndpoints.AddPassengerApi(passengersGroup);

app.Run();