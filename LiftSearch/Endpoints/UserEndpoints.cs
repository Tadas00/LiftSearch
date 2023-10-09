using LiftSearch.Data;
using LiftSearch.Data.Entities;
using LiftSearch.Data.Entities.Enums;
using LiftSearch.Dtos;
using Microsoft.EntityFrameworkCore;
using O9d.AspNet.FluentValidation;

namespace LiftSearch.Endpoints;

public static class UserEndpoints
{
    public static void AddUserApi(RouteGroupBuilder usersGroup)
    {
        // GET ALL
        usersGroup.MapGet("users",
            async (LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                return Results.Ok((await dbContext.Users.ToListAsync(cancellationToken)).Select(user => MakeUserDto(user, dbContext)));
            });

        // GET ONE
        usersGroup.MapGet("users/{userId}",
            async (int userId, LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken: cancellationToken);
                if (user == null) return Results.NotFound("Such user not found");

                return Results.Ok(MakeUserDto(user, dbContext));
            });

        // CREATE
        usersGroup.MapPost("users", async ([Validate] CreateUserDto createUserDto, LsDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var user = new User()
            {
                name = createUserDto.name,
                lastname = createUserDto.lastname,
                email = createUserDto.email,
                phone = createUserDto.phone,
                driverStatus = DriverStatus.No,
                registrationDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                cancelledCountTraveler = 0
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/users/{user.Id}", MakeUserDto(user, dbContext));
        });
        
        // UPDATE
        usersGroup.MapPut("users/{userId}",
            async (int userId, [Validate] UpdateUserDto updateUserDto, LsDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken: cancellationToken);
                if (user == null) return Results.NotFound();

                user.name = updateUserDto.name;
                user.lastname = updateUserDto.lastname;
                user.email = updateUserDto.email;
                user.phone = updateUserDto.phone;

                dbContext.Update(user);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Ok(MakeUserDto(user, dbContext));
            });

        // DELETE
        usersGroup.MapDelete("users/{userId}", async (int userId, LsDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken: cancellationToken);
            if (user == null) return Results.NotFound();

            dbContext.Remove(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        });
    }
    
    public static UserDto MakeUserDto(User user, LsDbContext dbContext)
    {
        return new UserDto(user.Id, user.name, user.lastname, user.email, user.phone, user.driverStatus, user.registrationDate, GetCompletedTripsCount(user, dbContext), user.cancelledCountTraveler);
    }

    public static int GetCompletedTripsCount(User user, LsDbContext dbContext)
    {
        return dbContext.Passengers.Include(p => p.trip).Include(p => p.traveler)
            .Count(p => p.traveler.Id == user.Id && p.trip.tripStatus == TripStatus.Finished);
    }
}