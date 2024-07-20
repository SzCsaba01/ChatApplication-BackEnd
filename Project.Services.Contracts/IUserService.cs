using Project.Data.Dto.Email;
using Project.Data.Dto.User;

namespace Project.Services.Contracts;
public interface IUserService {
    public Task RegistrationAsync(UserRegistrationDto userDto);
    public Task ForgotPasswordAsync(SendVerificationOrForgotPasswordEmailDto sendForgotPasswordEmailDto);
    public Task ConfirmRegistrationAsync(string resetPasswordToken);
    public Task ChangePasswordAsync(ChangePasswordDto changePasswordDto);
    public Task<Guid> GetUserIdByResetPasswordTokenAsync(string token);
    public Task ChangeProfilePictureAsync(ChangeProfilePictureDto changeProfilePictureDto);
    public Task<string> GetProfilePictureByIdAsync(Guid Id);
    public Task<UserPaginationDto> GetUsersPaginatedAsync(int page);
    public Task<UserPaginationDto> GetSearchedUsersByUserNameAsync(UserSearchDto userSearchDto);
    public Task<UserPaginationDto> GetAllSearchedUsersByUserNameAsync(UserSearchDto userSearchDto);
    public Task<UserPaginationDto> GetAllSearchedUsersByEmailAsync(UserSearchDto userSearchDto);
    public Task<GetUserDto> GetUserByIdAsync(Guid id);
    public Task EditUserAsync(EditUserDto editUserDto);
    public Task DeleteUserByNameAsync(string username);
    public Task<GetFriendListDto> GetFriendListAsync(Guid Id);
    public Task DeleteFriendAsync(DeleteFriendDto FriendName);
    public Task GenerateXMLFileForUserAsync(string username);
}
