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

public static class PassengerEndpoints
{
    public static void AddPassengerApi(RouteGroupBuilder passengerGroup)
    {
        // GET ALL
        passengerGroup.MapGet("passengers",
            async (string driverId, string tripId, LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var driver = await dbContext.Drivers.FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
                if (driver == null)
                    return Results.NotFound("Such driver not found");
            
                var trip = await dbContext.Trips.FirstOrDefaultAsync(trip =>
                    trip.Id == tripId && trip.driver.Id == driverId, cancellationToken: cancellationToken);
                if (trip == null) return Results.NotFound("Such trip not found");
                
                return Results.Ok(
                    (await dbContext.Passengers
                        .Where(passenger => passenger.trip.Id == tripId && passenger.trip.driver.Id == driverId)
                        .Include(passenger => passenger.trip).Include(passenger => passenger.Traveler).ToListAsync(cancellationToken))
                        .Select(passenger => MakePassengerDto(passenger)));
            });

        // GET ONE
        passengerGroup.MapGet("passengers/{passengerId}",
            async (string driverId, string tripId, string passengerId, LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var driver = await dbContext.Drivers.FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
                if (driver == null)
                    return Results.NotFound("Such driver not found");
            
                var trip = await dbContext.Trips.FirstOrDefaultAsync(trip =>
                    trip.Id == tripId && trip.driver.Id == driverId, cancellationToken: cancellationToken);
                if (trip == null) return Results.NotFound("Such trip not found");
                
                var passenger = await dbContext.Passengers.Include(passenger => passenger.trip)
                    .Include(passenger => passenger.Traveler).FirstOrDefaultAsync(passenger =>
                    passenger.Id == passengerId && passenger.trip.Id == tripId && passenger.trip.driver.Id == driverId, cancellationToken: cancellationToken);
                if (passenger == null) return Results.NotFound("Such passenger not found");

                return Results.Ok(MakePassengerDto(passenger));
            });
        
        // CREATE
        passengerGroup.MapPost("passengers",
            async (string driverId, string tripId, [Validate] CreatePassengerDto createPassengerDto, LsDbContext dbContext, CancellationToken cancellationToken, HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                
                var driver = await dbContext.Drivers.Include(driver => driver.User).FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
                if (driver == null) return Results.NotFound("Such driver not found");
            
                var trip = await dbContext.Trips.FirstOrDefaultAsync(trip => trip.Id == tripId && trip.driver.Id == driverId, cancellationToken: cancellationToken);
                if (trip == null) return Results.NotFound("Such trip not found");
                //TODO trip validation
                
              //  var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken: cancellationToken);
              //  if (user == null) return Results.NotFound("Such user not found");
                
                if(driver.User.Id == userId) return Results.UnprocessableEntity("Driver cannot register to it's own trip");
                
                var passengerCheck = await dbContext.Passengers.FirstOrDefaultAsync(p => p.trip.Id == tripId && p.Traveler.Id == userId, cancellationToken: cancellationToken);
                if (passengerCheck != null) return Results.UnprocessableEntity("This user has already registered to this trip");

                var passenger = new Passenger
                {
                    registrationStatus = false,
                    startCity = createPassengerDto.startCity,
                    endCity = createPassengerDto.endCity,
                    startAdress = createPassengerDto.startAdress,
                    endAdress = createPassengerDto.endAdress,
                    comment = createPassengerDto.comment,
                    trip = trip,
                    TravelerId = userId
                };

                dbContext.Passengers.Add(passenger);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Created($"/api/drivers/{driver.Id}/trips/{trip.Id}/passenger/{passenger.Id}", MakePassengerDto(passenger));
            });
        
        // UPDATE
        passengerGroup.MapPut("passengers/{passengerId}", async (string driverId, string tripId, string passengerId, [Validate] UpdatePassengerDto updatePassengerDto,
            LsDbContext dbContext, CancellationToken cancellationToken, HttpContext httpContext) =>
        {
            var driver = await dbContext.Drivers.FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
            if (driver == null) return Results.NotFound("Such driver not found");
            
            var trip = await dbContext.Trips.FirstOrDefaultAsync(trip =>
                trip.Id == tripId && trip.driver.Id == driverId, cancellationToken: cancellationToken);
            if (trip == null) return Results.NotFound("Such trip not found");
            //TODO trip validation
            
            var passenger = await dbContext.Passengers.Include(passenger => passenger.trip)
                .Include(passenger => passenger.Traveler).FirstOrDefaultAsync(passenger =>
                passenger.Id == passengerId && passenger.trip.Id == tripId && passenger.trip.driver.Id == driverId, cancellationToken: cancellationToken);
            if (passenger == null) return Results.NotFound("Such passenger not found");
            
            
            var userId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (passenger.TravelerId != userId)
            {
                return Results.Forbid();
            }
            

            passenger.registrationStatus = updatePassengerDto.registrationStatus ?? passenger.registrationStatus;
            passenger.startCity = updatePassengerDto.startCity ?? passenger.startCity;
            passenger.endCity = updatePassengerDto.endCity ?? passenger.endCity;
            passenger.startAdress = updatePassengerDto.startAdress ?? passenger.startAdress;
            passenger.endAdress = updatePassengerDto.endAdress ?? passenger.endAdress;
            passenger.comment = updatePassengerDto.comment ?? passenger.comment;

            dbContext.Update(passenger);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(MakePassengerDto(passenger));
        });

        // DELETE
        passengerGroup.MapDelete("passengers/{passengerId}", async (string driverId, string tripId, string passengerId, LsDbContext dbContext, CancellationToken cancellationToken, HttpContext httpContext) =>
        {
            var driver = await dbContext.Drivers.FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
            if (driver == null)
                return Results.NotFound("Such driver not found");
            
            var trip = await dbContext.Trips.FirstOrDefaultAsync(trip =>
                trip.Id == tripId && trip.driver.Id == driverId, cancellationToken: cancellationToken);
            if (trip == null) return Results.NotFound("Such trip not found");
            
            var passenger = await dbContext.Passengers.Include(passenger => passenger.Traveler).FirstOrDefaultAsync(passenger =>
                passenger.Id == passengerId && passenger.trip.Id == tripId && passenger.trip.driverId == driverId, cancellationToken: cancellationToken);
            if (passenger == null) return Results.NotFound("Such passenger not found");
            
            var userId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (passenger.TravelerId != userId)
            {
                return Results.Forbid();
            }

            incrementCancelledTrips(passenger.Traveler, dbContext);
            
            dbContext.Remove(passenger);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        });
    }
    
    public static PassengerDto MakePassengerDto (Passenger passenger)
    {
        return new PassengerDto(passenger.Id, passenger.registrationStatus, passenger.startCity, passenger.endCity, passenger.startAdress, passenger.endAdress, passenger.comment, passenger.Traveler.Id, passenger.trip.Id);
    }
    
    public static void incrementCancelledTrips(Traveler traveler, LsDbContext dbContext)
    {
        traveler.cancelledCountTraveler += 1;
        dbContext.Update(traveler);
    }
}