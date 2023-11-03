using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LiftSearch.Data;
using LiftSearch.Data.Entities;
using LiftSearch.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LiftSearch.Auth;

public static class AuthEndpoints
{
    public static void AddAutApi(this WebApplication app)
    {
        /*
        // register driver
        app.MapPost("api/registerDriver", async (UserManager<User> userManager, RegisterUserDto registerUserDto, LsDbContext dbContext, CancellationToken cancellationToken) =>
        {
            //check user exists
            var user = await userManager.FindByNameAsync(registerUserDto.Username);
            if (user != null)
            {
                if (userManager.GetRolesAsync(user).Result.Contains(UserRoles.Driver))
                {
                    return Results.UnprocessableEntity("User name already taken");
                }
                else
                {
                    //TODO jau yra traveleris
                    //TODO redirects
                }
                
            }

            var newUser = new User()
            {
                Email = registerUserDto.Email,
                UserName = registerUserDto.Username
            };

            var createUserResults = await userManager.CreateAsync(newUser, registerUserDto.Password);
            if (!createUserResults.Succeeded) return Results.UnprocessableEntity();

            await userManager.AddToRoleAsync(newUser, UserRoles.Driver);
            
            //create
            var newdriver = new Driver()
            {
                Id = newUser.Id,
                cancelledCountDriver = 0,
                registeredDriverDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                lastTripDate = null,
                driverBio = null,
                UserId = newUser.Id
            };

            dbContext.Drivers.Add(newdriver);
            await dbContext.SaveChangesAsync(cancellationToken);
            //create

            return Results.Created("api/login", new UserDto(newUser.Id, newUser.UserName, newUser.Email));

        });
        */
        
        // register traveler
        app.MapPost("api/register", async (UserManager<User> userManager, RegisterUserDto registerUserDto, LsDbContext dbContext, CancellationToken cancellationToken) =>
        {
            //check user exists
            var user = await userManager.FindByNameAsync(registerUserDto.Username);
            if (user != null) return Results.UnprocessableEntity("User name already taken");

            var newUser = new User()
            {
                Email = registerUserDto.Email,
                UserName = registerUserDto.Username
            };

            var createUserResults = await userManager.CreateAsync(newUser, registerUserDto.Password);
            if (!createUserResults.Succeeded) return Results.UnprocessableEntity();

            await userManager.AddToRoleAsync(newUser, UserRoles.Traveler);
            
            //create
            var traveler = new Traveler()
            {
                Id = newUser.Id,
                cancelledCountTraveler = 0,
                registrationDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                lastTripDate = null,
                travelerBio = null,
                UserId = newUser.Id
            };

            dbContext.Travelers.Add(traveler);
            await dbContext.SaveChangesAsync(cancellationToken);
            //create

            return Results.Created("api/login", new UserDto(newUser.Id, newUser.UserName, newUser.Email));
        });


        // login
        app.MapPost("api/login", async (UserManager<User> userManager, JwtTokenService jwtTokenService, LoginUserDto loginUserDto) =>
        {
            //check user exists
            var user = await userManager.FindByNameAsync(loginUserDto.Username);
            if (user == null) return Results.UnprocessableEntity("Username or password was incorrect");

            var isPasswordValid = await userManager.CheckPasswordAsync(user, loginUserDto.Password);
            if (!isPasswordValid) Results.UnprocessableEntity("Username or password was incorrect");

            user.forceRelogin = false;
            await userManager.UpdateAsync(user);
            
            var roles = await userManager.GetRolesAsync(user);
            var accessToken = jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
            var refreshToken = jwtTokenService.CreateRefreshToken(user.Id);

            return Results.Ok(new SuccesfullLoginDto(accessToken, refreshToken));
        });

        // accessToken
        app.MapPost("api/accessToken",
            async (UserManager<User> userManager, JwtTokenService jwtTokenService,
                RefreshAccessTokenDto refreshAccessTokenDto) =>
            {
                if (!jwtTokenService.TryParseRefreshToken(refreshAccessTokenDto.RefreshToken, out var claims))
                {
                    return Results.UnprocessableEntity();
                }

                var userId = claims.FindFirstValue(JwtRegisteredClaimNames.Sub);

                var user = await userManager.FindByIdAsync(userId);
                if (user == null) return Results.UnprocessableEntity("Invalid token");

                if (user.forceRelogin) return Results.UnprocessableEntity();
                
                var roles = await userManager.GetRolesAsync(user);
                var accessToken = jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
                var refreshToken = jwtTokenService.CreateRefreshToken(user.Id);
                
                return Results.Ok(new SuccesfullLoginDto(accessToken, refreshToken));
            });
    }
}


