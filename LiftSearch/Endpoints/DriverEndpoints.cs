using LiftSearch.Data;
using LiftSearch.Data.Entities;
using LiftSearch.Data.Entities.Enums;
using LiftSearch.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
                return Results.Ok((await dbContext.Drivers.Include(driver => driver.user).ToListAsync(cancellationToken)).Select(driver =>
                    MakeDriverDto(driver, dbContext)));
            });

        // GET ONE
        driversGroup.MapGet("drivers/{driverId}",
            async (int driverId, LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var driver = await dbContext.Drivers.Include(driver => driver.user).FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
                if (driver == null)
                    return Results.NotFound("Such driver not found");

                return Results.Ok(MakeDriverDto(driver, dbContext));
            });
        
        // GET PASSENGERS
        driversGroup.MapGet("drivers/{driverId}/passengers",
            async (int driverId, LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var driver = await dbContext.Drivers.Include(driver => driver.user).FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
                if (driver == null)
                    return Results.NotFound("Such driver not found");

                return Results.Ok((await dbContext.Passengers.Include(p => p.trip.driver).Include(p => p.traveler).Where(p => p.trip.driver.Id == driverId).ToListAsync(cancellationToken)).Select(passenger => PassengerEndpoints.MakePassengerDto(passenger)));
            });
        
        // CREATE
        driversGroup.MapPost("drivers", async ([Validate] CreateDriverDto createDriverDto, LsDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == createDriverDto.userId, cancellationToken: cancellationToken);
            if (user == null) return Results.NotFound("Such user not found");
            
            var driverCheck = await dbContext.Drivers.FirstOrDefaultAsync(d => d.user.Id == createDriverDto.userId, cancellationToken: cancellationToken);
            if (driverCheck != null) return Results.UnprocessableEntity("This user is already a driver");
            
            var driver = new Driver()
            {
                cancelledCountDriver = 0,
                registeredDriverDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                lastTripDate = null,
                driverBio = null,
                user = user
            };
            
            user.driverStatus = DriverStatus.Yes;
            dbContext.Update(user);

            dbContext.Drivers.Add(driver);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/drivers/{driver.Id}",MakeDriverDto(driver, dbContext));
        });
        
        // UPDATE
        driversGroup.MapPut("drivers/{driverId}",
            async (int driverId, [Validate] UpdateDriverDto updateDriverDto, LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var driver = await dbContext.Drivers.Include(driver => driver.user).FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
                if (driver == null)
                    return Results.NotFound("Such driver not found");

                driver.driverBio = updateDriverDto.driverBio ?? driver.driverBio;

                dbContext.Update(driver);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Ok(MakeDriverDto(driver, dbContext));
            });

        // DELETE
        driversGroup.MapDelete("drivers/{driverId}", async (int driverId, LsDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var driver = await dbContext.Drivers.Include(driver => driver.user).FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
            if (driver == null)
                return Results.NotFound("Such driver not found");
            
            var countActiveTrips = dbContext.Trips.Include(t => t.driver).Count(t => t.driver.Id == driverId && t.tripStatus == TripStatus.Active);
            if (countActiveTrips != 0)
                return Results.UnprocessableEntity("Driver can't be removed because he has active trips");
            
            driver.user.driverStatus = DriverStatus.No;
            dbContext.Update(driver.user);

            dbContext.Remove(driver);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        });
    }
    
    public static DriverDto MakeDriverDto (Driver driver, LsDbContext dbContext)
    {
        return new DriverDto(driver.Id, GetCompletedTripsCount(driver, dbContext), driver.cancelledCountDriver, driver.registeredDriverDate, driver.lastTripDate, driver.driverBio, driver.user.name, driver.user.lastname, driver.user.email, driver.user.phone);
    }
    
    public static int GetCompletedTripsCount(Driver driver, LsDbContext dbContext)
    {
        return dbContext.Trips.Include(t => t.driver).Count(t => t.driver.Id == driver.Id && t.tripStatus == TripStatus.Finished);
    }
}