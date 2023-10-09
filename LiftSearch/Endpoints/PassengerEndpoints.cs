using LiftSearch.Data;
using LiftSearch.Data.Entities;
using LiftSearch.Data.Entities.Enums;
using LiftSearch.Dtos;
using Microsoft.EntityFrameworkCore;
using O9d.AspNet.FluentValidation;

namespace LiftSearch.Endpoints;

public static class PassengerEndpoints
{
    public static void AddPassengerApi(RouteGroupBuilder passengerGroup)
    {
        // GET ALL
        passengerGroup.MapGet("passengers",
            async (int driverId, int tripId, LsDbContext dbContext, CancellationToken cancellationToken) =>
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
                        .Include(passenger => passenger.trip).Include(passenger => passenger.traveler).ToListAsync(cancellationToken))
                        .Select(passenger => MakePassengerDto(passenger)));
            });

        // GET ONE
        passengerGroup.MapGet("passengers/{passengerId}",
            async (int driverId, int tripId, int passengerId, LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var driver = await dbContext.Drivers.FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
                if (driver == null)
                    return Results.NotFound("Such driver not found");
            
                var trip = await dbContext.Trips.FirstOrDefaultAsync(trip =>
                    trip.Id == tripId && trip.driver.Id == driverId, cancellationToken: cancellationToken);
                if (trip == null) return Results.NotFound("Such trip not found");
                
                var passenger = await dbContext.Passengers.Include(passenger => passenger.trip)
                    .Include(passenger => passenger.traveler).FirstOrDefaultAsync(passenger =>
                    passenger.Id == passengerId && passenger.trip.Id == tripId && passenger.trip.driver.Id == driverId, cancellationToken: cancellationToken);
                if (passenger == null) return Results.NotFound("Such passenger not found");

                return Results.Ok(MakePassengerDto(passenger));
            });
        
        // CREATE
        passengerGroup.MapPost("passengers",
            async (int driverId, int tripId, [Validate] CreatePassengerDto createPassengerDto, LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var driver = await dbContext.Drivers.Include(driver => driver.user).FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
                if (driver == null) return Results.NotFound("Such driver not found");
            
                var trip = await dbContext.Trips.FirstOrDefaultAsync(trip => trip.Id == tripId && trip.driver.Id == driverId, cancellationToken: cancellationToken);
                if (trip == null) return Results.NotFound("Such trip not found");
                
                var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == createPassengerDto.userId, cancellationToken: cancellationToken);
                if (user == null) return Results.NotFound("Such user not found");
                
                if(driver.user.Id == createPassengerDto.userId) return Results.UnprocessableEntity("Driver cannot register to it's own trip");
                
                var passengerCheck = await dbContext.Passengers.FirstOrDefaultAsync(p => p.trip.Id == tripId && p.traveler.Id == createPassengerDto.userId, cancellationToken: cancellationToken);
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
                    traveler = user
                };

                dbContext.Passengers.Add(passenger);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Created($"/api/drivers/{driver.Id}/trips/{trip.Id}/passenger/{passenger.Id}", MakePassengerDto(passenger));
            });
        
        // UPDATE
        passengerGroup.MapPut("passengers/{passengerId}", async (int driverId, int tripId, int passengerId, [Validate] UpdatePassengerDto updatePassengerDto,
            LsDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var driver = await dbContext.Drivers.FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
            if (driver == null) return Results.NotFound("Such driver not found");
            
            var trip = await dbContext.Trips.FirstOrDefaultAsync(trip =>
                trip.Id == tripId && trip.driver.Id == driverId, cancellationToken: cancellationToken);
            if (trip == null) return Results.NotFound("Such trip not found");
            
            var passenger = await dbContext.Passengers.Include(passenger => passenger.trip)
                .Include(passenger => passenger.traveler).FirstOrDefaultAsync(passenger =>
                passenger.Id == passengerId && passenger.trip.Id == tripId && passenger.trip.driver.Id == driverId, cancellationToken: cancellationToken);
            if (passenger == null) return Results.NotFound("Such passenger not found");

            passenger.registrationStatus = updatePassengerDto.registrationStatus;
            passenger.startCity = updatePassengerDto.startCity;
            passenger.endCity = updatePassengerDto.endCity;
            passenger.startAdress = updatePassengerDto.startAdress;
            passenger.endAdress = updatePassengerDto.endAdress;
            passenger.comment = updatePassengerDto.comment;

            dbContext.Update(passenger);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(MakePassengerDto(passenger));
        });

        // DELETE
        passengerGroup.MapDelete("passengers/{passengerId}", async (int driverId, int tripId, int passengerId, LsDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var driver = await dbContext.Drivers.FirstOrDefaultAsync(driver => driver.Id == driverId, cancellationToken: cancellationToken);
            if (driver == null)
                return Results.NotFound("Such driver not found");
            
            var trip = await dbContext.Trips.FirstOrDefaultAsync(trip =>
                trip.Id == tripId && trip.driver.Id == driverId, cancellationToken: cancellationToken);
            if (trip == null) return Results.NotFound("Such trip not found");
            
            var passenger = await dbContext.Passengers.Include(passenger => passenger.traveler).FirstOrDefaultAsync(passenger =>
                passenger.Id == passengerId && passenger.trip.Id == tripId && passenger.trip.driver.Id == driverId, cancellationToken: cancellationToken);
            if (passenger == null) return Results.NotFound("Such passenger not found");

            incrementCancelledTrips(passenger.traveler, dbContext);
            
            dbContext.Remove(passenger);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        });
    }
    
    public static PassengerDto MakePassengerDto (Passenger passenger)
    {
        return new PassengerDto(passenger.Id, passenger.registrationStatus, passenger.startCity, passenger.endCity, passenger.startAdress, passenger.endAdress, passenger.comment, passenger.traveler.Id, passenger.trip.Id);
    }
    
    public static void incrementCancelledTrips(User user, LsDbContext dbContext)
    {
        user.cancelledCountTraveler += 1;
        dbContext.Update(user);
    }
}