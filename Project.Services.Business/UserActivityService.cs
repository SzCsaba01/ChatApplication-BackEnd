using Microsoft.EntityFrameworkCore;
using Project.Data.Data;
using Project.Data.Dto.UserActivity;
using Project.Data.Entities;
using Project.Services.Business.Exceptions;
using Project.Services.Contracts;
using SendGrid.Helpers.Errors.Model;
using System.ComponentModel.DataAnnotations;

namespace Project.Services.Business;
public class UserActivityService : IUserActivityService {
    private readonly DataContext _dataContext;
    public UserActivityService(DataContext dataContext) {
        _dataContext = dataContext;
    }

    public async Task<GetUserActivitiesDto> GetUserActivitiesByUsernameAsync(string username) {
        var activites = await _dataContext.Users
            .Where(u => u.Username.Equals(username))
            .Include(u => u.UserActivities)
            .Select(u => u.UserActivities
                .Select(a => new GetUserActivityDto {
                    LoginDate = a.LoggedIn,
                    LogoutDate = a.LoggedOut,
                }).ToList())
            .FirstOrDefaultAsync();

        if (activites is null) {
            throw new ModelNotFoundException("User not found");
        }

        var userActivities = new GetUserActivitiesDto {
            UserActivities = activites
        };

        return userActivities;
    }

    public async Task UserLoginAsync(Guid userId) {
        var user = await _dataContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.UserActivities)
            .FirstOrDefaultAsync();

        if (user == null) {
            throw new ModelNotFoundException("User not found");
        }

        var existingActivity = user.UserActivities
                            .Where(a => a.LoggedOut is null)
                            .FirstOrDefault();

        if (!(existingActivity is null)) {
            existingActivity.LoggedOut = DateTime.Now;
        }


        var userActivity = new UserActivity {
            LoggedIn = DateTime.Now,
            User = user
        };

        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(userActivity, new ValidationContext(userActivity), results, validateAllProperties: true);
        var errorMessages = results.Select(x => x.ErrorMessage);

        if (!valid) {
            throw new BadRequestException(string.Join(" ", errorMessages));
        }

        await _dataContext.UserActivities.AddAsync(userActivity);
        user.UserActivities.Add(userActivity);

        await _dataContext.SaveChangesAsync();
    }

    public async Task UserLogoutAsync(Guid userId) {
        var user = await _dataContext.Users
         .Where(u => u.Id == userId)
         .Include(u => u.UserActivities)
         .FirstOrDefaultAsync();

        if(user is null) {
            throw new ModelNotFoundException("User not found");
        }

        var userActivity = user.UserActivities
            .Where(a => a.LoggedOut == null)
            .FirstOrDefault();
        
        if(!(userActivity is null)) {
            userActivity.LoggedOut = DateTime.Now;
        }

        await _dataContext.SaveChangesAsync();
    }
}
