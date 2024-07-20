using Microsoft.Identity.Client;
using Project.Data.Dto.UserActivity;

namespace Project.Services.Contracts;
public interface IUserActivityService {
    public Task<GetUserActivitiesDto> GetUserActivitiesByUsernameAsync(string username);
    public Task UserLoginAsync(Guid userId);
    public Task UserLogoutAsync(Guid userId);
}
