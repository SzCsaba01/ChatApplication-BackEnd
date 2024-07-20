using Microsoft.AspNetCore.Mvc;
using Project.Services.Contracts;
using Project.Data.Dto.User;
using Project.Data.Dto.Email;
using Microsoft.AspNetCore.Authorization;

namespace Project.API.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase {
    private readonly IUserService _userService;

    public UserController(IUserService userService) {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("Registration")]
    public async Task<IActionResult> RegistrationAsync(UserRegistrationDto userDto) {
        await _userService.RegistrationAsync(userDto);
        return Ok("You have successfully registered, verify your email to be able to log in");
    }

    [AllowAnonymous]
    [HttpPost("ConfirmRegistration")]
    public async Task<IActionResult> ConfirmRegistrationAsync(string resetPasswordToken) {
        await _userService.ConfirmRegistrationAsync(resetPasswordToken);
        return Ok("Successfully verified your email");
    }

    [AllowAnonymous]
    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPasswordAsync(SendVerificationOrForgotPasswordEmailDto sendForgotPasswordEmailDto) {
        await _userService.ForgotPasswordAsync(sendForgotPasswordEmailDto);
        return Ok("We have sent you an email to change your password");
    }

    [AllowAnonymous]
    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto) {
        await _userService.ChangePasswordAsync(changePasswordDto);
        return Ok("You have successfully changed your password");
    }

    [AllowAnonymous]
    [HttpGet("GetUserIdByResetPasswordToken")]
    public async Task<IActionResult> GetUserIdByResetPasswordTokenAsync(string token) {
        return Ok(await _userService.GetUserIdByResetPasswordTokenAsync(token));
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPut("ChangeProfilePicture")]
    public async Task<IActionResult> ChangeProfilePictureAsync([FromForm] ChangeProfilePictureDto changeProfilePictureDto) {
        await _userService.ChangeProfilePictureAsync(changeProfilePictureDto);
        return Ok();
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("GetProfilePictureUrl")]
    public async Task<IActionResult> GetProfilePictureByIdAsync(Guid Id) {
        return Ok(await _userService.GetProfilePictureByIdAsync(Id));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("DeleteUser")]
    public async Task<IActionResult> DeleteUserByNameAsync(string username) {
        await _userService.DeleteUserByNameAsync(username);
        return Ok("Successfully deleted a User");
    }

    [HttpGet("GetUserById")]
    public async Task<IActionResult> GetUserByIdAsync(Guid id) {
        return Ok(await _userService.GetUserByIdAsync(id));
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPut("EditUser")]
    public async Task<IActionResult> EditUserAsync(EditUserDto editUserDto) {
        await _userService.EditUserAsync(editUserDto);
        return Ok("Successfully changed your profile details");
    }

    [Authorize(Roles = "Admin, User")]
    [HttpPost("GetSearchedUsersByUserName")]
    public async Task<IActionResult> GetSearchedUsersByUserNameAsync(UserSearchDto userSearchDto) {
        return Ok(await _userService.GetSearchedUsersByUserNameAsync(userSearchDto));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("GetAllSearchedUsersByEmail")]
    public async Task<IActionResult> GetSearchedUsersByEmailAsync(UserSearchDto userSearchDto) {
        return Ok(await _userService.GetAllSearchedUsersByEmailAsync(userSearchDto));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("GetAllSearchedUsersByUsername")]
    public async Task<IActionResult> GetSearchedUsersByUsernameAsync(UserSearchDto userSearchDto) {
        return Ok(await _userService.GetAllSearchedUsersByUserNameAsync(userSearchDto));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("GetUsersPaginated")]
    public async Task<IActionResult> GetUsersPaginatedAsync(int page) {
        return Ok(await _userService.GetUsersPaginatedAsync(page));
    }


    [Authorize(Roles = "Admin, User")]
    [HttpGet("GetFriendList")]
    public async Task<IActionResult> GetFriendListAsync(Guid Id) {
        return Ok(await _userService.GetFriendListAsync(Id));
    }

    [Authorize(Roles = "Admin, User")]
    [HttpPost("DeleteFriend")]
    public async Task<IActionResult> DeleteFriendAsync(DeleteFriendDto deleteFriendDto) {
        await _userService.DeleteFriendAsync(deleteFriendDto);
        return Ok("Successfully deleted a friend");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("GenerateXMLFileForUser")]
    public async Task<IActionResult> GenerateXMLFileForUserAsync(string username) {
        await _userService.GenerateXMLFileForUserAsync(username);
        return Ok($"Succesfully generated XML file for user {username}");
    }
}
