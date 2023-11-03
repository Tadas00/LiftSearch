using System.Security.Claims;
using LiftSearch.Data;
using LiftSearch.Data.Entities;
using LiftSearch.Data.Entities.Enums;
using LiftSearch.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using O9d.AspNet.FluentValidation;

namespace LiftSearch.Endpoints;

public class TravelerEndpoint
{
    public static void AddTravelerApi(RouteGroupBuilder travelersGroup)
    {
        // GET ALL
        travelersGroup.MapGet("travelers",
            async (LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                return Results.Ok(
                    (await dbContext.Travelers.Include(traveler => traveler.User).ToListAsync(cancellationToken)).Select(
                        traveler =>
                            MakeTravelerDto(traveler, dbContext)));
            });

        // GET ONE
        travelersGroup.MapGet("travelers/{travelerId}",
            async (string travelerId, LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var traveler = await dbContext.Travelers.Include(traveler => traveler.User).FirstOrDefaultAsync(traveler => traveler.Id == travelerId, cancellationToken: cancellationToken);
                if (traveler == null)
                    return Results.NotFound(new { error = "Such traveler not found" });

                return Results.Ok(MakeTravelerDto(traveler, dbContext));
            });
        
        // CREATE
        travelersGroup.MapPost("travelers", async ([Validate] CreateTravelerDto createTravelerDto, LsDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == createTravelerDto.userId, cancellationToken: cancellationToken);
            if (user == null) return Results.NotFound("Such user not found");
            
            var travelerCheck = await dbContext.Travelers.FirstOrDefaultAsync(t => t.Id == createTravelerDto.userId, cancellationToken: cancellationToken);
            if (travelerCheck != null) return Results.UnprocessableEntity("This user is already a traveler");
            
            var traveler = new Traveler()
            {
                Id = user.Id,
                cancelledCountTraveler = 0,
                registrationDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                lastTripDate = null,
                travelerBio = null,
                UserId = user.Id
            };
            
        //    dbContext.Update(user);

            dbContext.Travelers.Add(traveler);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/travelers/{traveler.Id}",MakeTravelerDto(traveler, dbContext));
        });
        
        // UPDATE
        travelersGroup.MapPut("travelers/{travelerId}",
            async (string travelerId, [Validate] UpdateTravelerDto updateTravelerDto, LsDbContext dbContext, CancellationToken cancellationToken, HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (travelerId != userId)
                {
                    return Results.Forbid();
                }
                
                var traveler = await dbContext.Travelers.Include(traveler => traveler.User).FirstOrDefaultAsync(traveler => traveler.Id == travelerId, cancellationToken: cancellationToken);
                if (traveler == null)
                    return Results.NotFound("Such traveler not found");

                traveler.travelerBio = updateTravelerDto.travelerBio ?? traveler.travelerBio;

                dbContext.Update(traveler);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Ok(MakeTravelerDto(traveler, dbContext));
            });

        // DELETE
        travelersGroup.MapDelete("travelers/{travelerId}", async (string travelerId, LsDbContext dbContext, CancellationToken cancellationToken, UserManager<User> userManager, HttpContext httpContext) =>
        {
            var userId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (travelerId != userId)
            {
                return Results.Forbid();
            }
            
            var traveler = await dbContext.Travelers.Include(traveler => traveler.User).FirstOrDefaultAsync(traveler => traveler.Id == travelerId, cancellationToken: cancellationToken);
            if (traveler == null)
                return Results.NotFound("Such traveler not found");
            
            var countActiveTrips = dbContext.Passengers.Include(t => t.trip).Include(t => t.Traveler).Count(t => t.Traveler.Id == travelerId && t.trip.tripStatus == TripStatus.Active);
            if (countActiveTrips != 0)
                return Results.UnprocessableEntity("Traveler can't be removed because he has active trips");
            
            var countActiveDrives = dbContext.Trips.Include(t => t.driver).Count(t => t.driver.Id == travelerId && t.tripStatus == TripStatus.Active);
            if (countActiveDrives != 0)
                return Results.UnprocessableEntity("Driver can't be removed because he has active trips");

            dbContext.Remove(traveler);
            await userManager.DeleteAsync(traveler.User);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        });
    }
    
    public static TravelerDto MakeTravelerDto (Traveler traveler, LsDbContext dbContext)
    {
        return new TravelerDto(traveler.Id, GetCompletedTripsCount(traveler, dbContext), traveler.cancelledCountTraveler, traveler.registrationDate, traveler.lastTripDate, traveler.travelerBio, traveler.User.UserName, traveler.User.Email);
    }
    
    public static int GetCompletedTripsCount(Traveler traveler, LsDbContext dbContext)
    {
        return dbContext.Passengers.Include(t => t.trip).Count(t => t.Traveler.Id == traveler.Id && t.trip.tripStatus == TripStatus.Finished);
    }
}