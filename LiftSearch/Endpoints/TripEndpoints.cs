using System.Security.Claims;
using LiftSearch.Auth;
using LiftSearch.Data;
using LiftSearch.Data.Entities;
using LiftSearch.Data.Entities.Enums;
using LiftSearch.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using O9d.AspNet.FluentValidation;

namespace LiftSearch.Endpoints;

public static class TripEndpoints
{
    public static void AddTripApi(RouteGroupBuilder tripsGroup)
    {
        // GET ALL
        tripsGroup.MapGet("trips",
            async (string driverId, LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var driver = await dbContext.Drivers.FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
                if (driver == null)
                    return Results.NotFound("Such driver not found");
                
                return Results.Ok(
                    (await dbContext.Trips.Where(trip => trip.driver.Id == driverId).ToListAsync(cancellationToken))
                    .Select(trip => MakeTripDto(trip)));
            });

        // GET ONE
        tripsGroup.MapGet("trips/{tripId}",
            async (string driverId, string tripId, LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var driver = await dbContext.Drivers.FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
                if (driver == null)
                    return Results.NotFound("Such driver not found");
                
                var trip = await dbContext.Trips.FirstOrDefaultAsync(trip =>
                    trip.Id == tripId && trip.driver.Id == driverId, cancellationToken: cancellationToken);
                if (trip == null) return Results.NotFound("Such trip not found");

                return Results.Ok(MakeTripDto(trip));
            });

        // CREATE
        tripsGroup.MapPost("trips",
            async (string driverId, [Validate] CreateTripDto createTripDto, LsDbContext dbContext, CancellationToken cancellationToken, HttpContext httpContext) =>
            {
                if (createTripDto.startTime >= createTripDto.endTime) return Results.UnprocessableEntity("Start time cannot be later then end time");
                
                var driver = await dbContext.Drivers.FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
                if (driver == null)
                    return Results.NotFound("Such driver not found");

                var claim = httpContext.User;
                if (!claim.IsInRole(UserRoles.Driver) || claim.FindFirstValue(JwtRegisteredClaimNames.Sub) != driverId)
                {
                    return Results.Forbid();
                }
                
                var trip = new Trip
                {
                    tripDate = DateTime.SpecifyKind(createTripDto.tripDate, DateTimeKind.Utc),
                    lastEditTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                    seatsCount = createTripDto.seatsCount,
                    startTime = createTripDto.startTime,
                    endTime = createTripDto.endTime,
                    price = createTripDto.price,
                    description = createTripDto.description,
                    startCity = createTripDto.startCity,
                    endCity = createTripDto.endCity,
                    tripStatus = TripStatus.Active,
                    driverId = driverId
                };

                dbContext.Trips.Add(trip);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Created($"/api/drivers/{driver.Id}/trips/{trip.Id}", MakeTripDto(trip));
            });

        // UPDATE
        tripsGroup.MapPut("trips/{tripId}", async (string driverId, string tripId, [Validate] UpdateTripDto updateTripDto,
            LsDbContext dbContext, CancellationToken cancellationToken, HttpContext httpContext) =>
        {
            if (updateTripDto.startTime >= updateTripDto.endTime) return Results.UnprocessableEntity("Start time cannot be later then end time");
            
            var driver = await dbContext.Drivers.FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
            if (driver == null)
                return Results.NotFound("Such driver not found");
            
            var claim = httpContext.User;
            if (!claim.IsInRole(UserRoles.Driver) || claim.FindFirstValue(JwtRegisteredClaimNames.Sub) != driverId)
            {
                return Results.Forbid();
            }
            
            var trip = await dbContext.Trips.FirstOrDefaultAsync(trip =>
                trip.Id == tripId && trip.driver.Id == driverId, cancellationToken: cancellationToken);
            if (trip == null) return Results.NotFound("Such trip not found");

            trip.seatsCount = updateTripDto.seatsCount ?? trip.seatsCount;
            trip.lastEditTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            trip.startTime = updateTripDto.startTime ?? trip.startTime;
            trip.endTime = updateTripDto.endTime ?? trip.endTime;
            trip.price = updateTripDto.price ?? trip.price;
            trip.description = updateTripDto.description ?? trip.description;
            trip.startCity = updateTripDto.startCity ?? trip.startCity;
            trip.endCity = updateTripDto.endCity ?? trip.endCity;
            trip.tripStatus = updateTripDto.tripStatus ?? trip.tripStatus;

            dbContext.Update(trip);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(MakeTripDto(trip));
        });

        // DELETE
        tripsGroup.MapDelete("trips/{tripId}", async (string driverId, string tripId, LsDbContext dbContext, CancellationToken cancellationToken, HttpContext httpContext) =>
        {
            var driver = await dbContext.Drivers.FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
            if (driver == null)
                return Results.NotFound("Such driver not found");
            
            var claim = httpContext.User;
            if (!(claim.IsInRole(UserRoles.Driver) || claim.IsInRole(UserRoles.Admin))|| claim.FindFirstValue(JwtRegisteredClaimNames.Sub) != driverId)
            {
                return Results.Forbid();
            }
            
            var trip = await dbContext.Trips.FirstOrDefaultAsync(trip =>
                trip.Id == tripId && trip.driver.Id == driverId, cancellationToken: cancellationToken);
            if (trip == null) return Results.NotFound("Such trip not found");

            incrementCancelledTrips(driver, dbContext);
            
            dbContext.Remove(trip);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        });
    }
    
    public static TripDto MakeTripDto (Trip trip)
    {
        return new TripDto(trip.Id, trip.tripDate, trip.lastEditTime, trip.seatsCount, trip.startTime, trip.endTime, trip.price, trip.description, trip.startCity, trip.endCity, trip.tripStatus);
    }
    
    public static void incrementCancelledTrips(Driver driver, LsDbContext dbContext)
    {
        driver.cancelledCountDriver += 1;
        dbContext.Update(driver);
    }
}