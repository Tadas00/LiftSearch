﻿using System.Security.Claims;
using LiftSearch.Auth;
using LiftSearch.Data;
using LiftSearch.Data.Entities;
using LiftSearch.Data.Entities.Enums;
using LiftSearch.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.IdentityModel.JsonWebTokens;
using O9d.AspNet.FluentValidation;

namespace LiftSearch.Endpoints;

public static class DriverEndpoints
{
    
    public static void AddDriverApi(RouteGroupBuilder driversGroup)
    {
        // GET ALL
        driversGroup.MapGet("drivers",
            async (LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                return Results.Ok(
                    (await dbContext.Drivers.Include(driver => driver.User).ToListAsync(cancellationToken)).Select(
                        driver =>
                            MakeDriverDto(driver, dbContext)));
            });

        // GET ONE
        driversGroup.MapGet("drivers/{driverId}",
            async (int driverId, LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var driver = await dbContext.Drivers.Include(driver => driver.User).FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
                if (driver == null)
                    return Results.NotFound(new { error = "Such driver not found" });

                return Results.Ok(MakeDriverDto(driver, dbContext));
            });
        
        // GET PASSENGERS
        driversGroup.MapGet("drivers/{driverId}/passengers",
            async (int driverId, LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var driver = await dbContext.Drivers.Include(driver => driver.User).FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
                if (driver == null)
                    return Results.NotFound("Such driver not found");

                return Results.Ok((await dbContext.Passengers.Include(p => p.trip.Driver).Include(p => p.Traveler).Where(p => p.trip.DriverId == driverId).ToListAsync(cancellationToken)).Select(passenger => PassengerEndpoints.MakePassengerDto(passenger)));
            });
        
        // CREATE
        driversGroup.MapPost("drivers", async ([Validate] CreateDriverDto createDriverDto, LsDbContext dbContext, CancellationToken cancellationToken, UserManager<User> userManager, HttpContext httpContext) =>
        {
            var claim = httpContext.User;
            string? userId;
            if (claim.IsInRole(UserRoles.Admin) && createDriverDto.userId != null)
                userId = createDriverDto.userId;
            else if (claim.IsInRole(UserRoles.Traveler))
                userId = claim.FindFirstValue(JwtRegisteredClaimNames.Sub);
            else
                return Results.Forbid();
            
            var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken: cancellationToken);
            if (user == null) return Results.NotFound("Such user not found");
            
            var driverCheck = await dbContext.Drivers.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken: cancellationToken);
            if (driverCheck != null) return Results.UnprocessableEntity("This user is already a driver");
            
            var driver = new Driver()
            {
                cancelledCountDriver = 0,
                registeredDriverDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                lastTripDate = null,
                driverBio = null,
                UserId = user.Id
            };

            dbContext.Drivers.Add(driver);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            await userManager.AddToRoleAsync(user, UserRoles.Driver);

            return Results.Created($"/api/drivers/{driver.Id}",MakeDriverDto(driver, dbContext));
        });
        
        // UPDATE
        driversGroup.MapPut("drivers/{driverId}",
            async (int driverId, [Validate] UpdateDriverDto updateDriverDto, LsDbContext dbContext, CancellationToken cancellationToken, HttpContext httpContext) =>
            {
                
                var driver = await dbContext.Drivers.Include(driver => driver.User).FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
                if (driver == null)
                    return Results.NotFound("Such driver not found");
                
                var claim = httpContext.User;
                if (!claim.IsInRole(UserRoles.Driver) || claim.FindFirstValue(JwtRegisteredClaimNames.Sub) != driver.UserId)
                {
                    return Results.Forbid();
                }

                driver.driverBio = updateDriverDto.driverBio ?? driver.driverBio;

                dbContext.Update(driver);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Ok(MakeDriverDto(driver, dbContext));
            });

        // DELETE
        driversGroup.MapDelete("drivers/{driverId}", async (int driverId, LsDbContext dbContext, CancellationToken cancellationToken, UserManager<User> userManager, HttpContext httpContext) =>
        {
            
            var driver = await dbContext.Drivers.Include(driver => driver.User).FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
            if (driver == null)
                return Results.NotFound("Such driver not found");
            
            var claim = httpContext.User;
            if (!claim.IsInRole(UserRoles.Driver) || claim.FindFirstValue(JwtRegisteredClaimNames.Sub) != driver.UserId)
            {
                return Results.Forbid();
            }
            
            var countActiveTrips = dbContext.Trips.Include(t => t.Driver).Count(t => t.DriverId == driverId && t.tripStatus == TripStatus.Active);
            if (countActiveTrips != 0)
                return Results.UnprocessableEntity("Driver can't be removed because he has active trips");
            
            await userManager.RemoveFromRoleAsync(driver.User, UserRoles.Driver);

            dbContext.Remove(driver);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        });
    }
    
    public static DriverDto MakeDriverDto (Driver driver, LsDbContext dbContext)
    {
        return new DriverDto(driver.Id, GetCompletedTripsCount(driver, dbContext), driver.cancelledCountDriver, driver.registeredDriverDate, driver.lastTripDate, driver.driverBio, driver.User.UserName, driver.User.Email);
    }
    
    public static int GetCompletedTripsCount(Driver driver, LsDbContext dbContext)
    {
        return dbContext.Trips.Include(t => t.Driver).Count(t => t.DriverId == driver.Id && t.tripStatus == TripStatus.Finished);
    }
}