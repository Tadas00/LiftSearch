using System.Security.Claims;
using LiftSearch.Auth;
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
            async (LsDbContext dbContext, CancellationToken cancellationToken, HttpContext httpContext, UserManager<User> userManager) =>
            {
                var claim = httpContext.User;
                if (!claim.IsInRole(UserRoles.Admin))
                {
                    return Results.Forbid();
                }
                
                var user = await userManager.FindByIdAsync(claim.FindFirstValue(JwtRegisteredClaimNames.Sub));
                if (user == null) return Results.UnprocessableEntity("Invalid token");
                if (user.forceRelogin) return Results.Forbid();
                
                return Results.Ok(
                    (await dbContext.Travelers.Include(traveler => traveler.User).ToListAsync(cancellationToken)).Select(
                        traveler =>
                            MakeTravelerDto(traveler, dbContext)));
            });

        // GET ONE
        travelersGroup.MapGet("travelers/{travelerId}",
            async (int travelerId, LsDbContext dbContext, CancellationToken cancellationToken, HttpContext httpContext, UserManager<User> userManager) =>
            {
                var traveler = await dbContext.Travelers.Include(traveler => traveler.User).FirstOrDefaultAsync(traveler => traveler.Id == travelerId, cancellationToken: cancellationToken);
                if (traveler == null)
                    return Results.NotFound(new { error = "Such traveler not found" });

                var claim = httpContext.User;
                if (!claim.IsInRole(UserRoles.Admin) && !claim.IsInRole(UserRoles.Driver) && (!claim.IsInRole(UserRoles.Traveler) || claim.FindFirstValue(JwtRegisteredClaimNames.Sub) != traveler.UserId))
                {
                    return Results.Forbid();
                }
                
                var user = await userManager.FindByIdAsync(claim.FindFirstValue(JwtRegisteredClaimNames.Sub));
                if (user == null) return Results.UnprocessableEntity("Invalid token");
                if (user.forceRelogin) return Results.Forbid();
                
                return Results.Ok(MakeTravelerDto(traveler, dbContext));
            });
        
        // CREATE
        travelersGroup.MapPost("travelers", async ([Validate] CreateTravelerDto createTravelerDto, LsDbContext dbContext, CancellationToken cancellationToken, HttpContext httpContext) =>
        {
            var claim = httpContext.User;
            if (!claim.IsInRole(UserRoles.Admin))
            {
                return Results.Forbid();
            }
            
            var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == createTravelerDto.userId, cancellationToken: cancellationToken);
            if (user == null) return Results.NotFound(new { error = "Such user not found" });
            if (user.forceRelogin) return Results.Forbid();
            
            var travelerCheck = await dbContext.Travelers.FirstOrDefaultAsync(t => t.UserId == createTravelerDto.userId, cancellationToken: cancellationToken);
            if (travelerCheck != null) return Results.UnprocessableEntity(new { error = "This user is already a traveler" });
            
            var traveler = new Traveler()
            {
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
            async (int travelerId, [Validate] UpdateTravelerDto updateTravelerDto, LsDbContext dbContext, CancellationToken cancellationToken, HttpContext httpContext, UserManager<User> userManager) =>
            {
                var traveler = await dbContext.Travelers.Include(traveler => traveler.User).FirstOrDefaultAsync(traveler => traveler.Id == travelerId, cancellationToken: cancellationToken);
                if (traveler == null)
                    return Results.NotFound(new { error = "Such traveler not found" });
                
                var userId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (traveler.UserId != userId)
                {
                    return Results.Forbid();
                }
                
                var claim = httpContext.User;
                var user = await userManager.FindByIdAsync(claim.FindFirstValue(JwtRegisteredClaimNames.Sub));
                if (user == null) return Results.UnprocessableEntity("Invalid token");
                if (user.forceRelogin) return Results.Forbid();
                
                traveler.travelerBio = updateTravelerDto.travelerBio ?? traveler.travelerBio;

                dbContext.Update(traveler);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Ok(MakeTravelerDto(traveler, dbContext));
            });

        // DELETE
        travelersGroup.MapDelete("travelers/{travelerId}", async (int travelerId, LsDbContext dbContext, CancellationToken cancellationToken, UserManager<User> userManager, HttpContext httpContext) =>
        {
            
            var traveler = await dbContext.Travelers.Include(traveler => traveler.User).FirstOrDefaultAsync(traveler => traveler.Id == travelerId, cancellationToken: cancellationToken);
            if (traveler == null)
                return Results.NotFound(new { error = "Such traveler not found" });
            
            var claim = httpContext.User;
            if (!claim.IsInRole(UserRoles.Admin) && (!claim.IsInRole(UserRoles.Traveler) || claim.FindFirstValue(JwtRegisteredClaimNames.Sub) != traveler.UserId))
            {
                return Results.Forbid();
            }
            var user = await userManager.FindByIdAsync(claim.FindFirstValue(JwtRegisteredClaimNames.Sub));
            if (user == null) return Results.UnprocessableEntity("Invalid token");
            if (user.forceRelogin) return Results.Forbid();
            
            var countActiveTrips = dbContext.Passengers.Include(t => t.trip).Include(t => t.Traveler).Count(t => t.Traveler.Id == travelerId && t.trip.tripStatus == TripStatus.Active);
            if (countActiveTrips != 0)
                return Results.UnprocessableEntity(new { error = "Traveler can't be removed because he has active trips" });
            
            var countActiveDrives = dbContext.Trips.Include(t => t.Driver).Count(t => t.Driver.Id == travelerId && t.tripStatus == TripStatus.Active);
            if (countActiveDrives != 0)
                return Results.UnprocessableEntity(new { error = "Driver can't be removed because he has active trips" });

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