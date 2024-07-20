using Azure;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Data.Data;
using Project.Data.Dto.Email;
using Project.Data.Dto.User;
using Project.Data.Entities;
using Project.Data.Mappers;
using Project.Services.Business.Exceptions;
using Project.Services.Contracts;
using SendGrid.Helpers.Errors.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace Project.Services.Business;
public class UserService :IUserService{
    private readonly IEncryptionService _encryptionService;
    private readonly DataContext _dataContext;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public UserService(DataContext dataContext, IEncryptionService encryptionService, ITokenService tokenService, IEmailService emailService) {
        _dataContext = dataContext;
        _encryptionService = encryptionService;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<bool> VerifyExistingEmailAsync(string email) {

        var user = await _dataContext.Users.Select(u => u.Email).FirstOrDefaultAsync(u => u.Equals(email));

        if (user is null) {
            return false;
        }

        return true;
    }

    public async Task<bool> VerifyExistingUsernameAsync(string userName) {

        var user = await _dataContext.Users.Select(u => u.Username).FirstOrDefaultAsync(u => u.Equals(userName));

        if (user is null) {
            return false;
        }

        return true;
    }

    public async Task RegistrationAsync(UserRegistrationDto userDto) {
        if (await VerifyExistingUsernameAsync(userDto.Username)) {
            throw new AlreadyExistsException("Username already taken");
        }

        if (await VerifyExistingEmailAsync(userDto.Email)) {
            throw new AlreadyExistsException("Email already taken");
        }

        var role = await _dataContext.Roles.FirstOrDefaultAsync(r => r.RoleType == 0);

        if (role is null) {
            throw new ModelNotFoundException("Something wrong happend...");
        }

        if (!userDto.Password.Equals(userDto.RepeatPassword)) {
            throw new PasswordException("Passwords doesn't match");
        }

        var user = UserMapper.ToUser(userDto);
        user.Role = role;

        user.RegistrationToken = await _tokenService.GenerateRandomTokenAsync() + user.Username;

        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(user, new ValidationContext(user), results, validateAllProperties: true);
        var errorMessages = results.Select(x => x.ErrorMessage);

        if (!valid) {
            throw new BadRequestException(string.Join(" ", errorMessages));
        }

        user.Password = _encryptionService.GeneratedHashedPassword(userDto.Password);

        await _dataContext.Users.AddAsync(user);
        await _dataContext.SaveChangesAsync();

        var sendVerificationEmail = new SendVerificationOrForgotPasswordEmailDto {
            Email = user.Email,
            Token = user.RegistrationToken
        };

        await _emailService.SendVerificationEmail(sendVerificationEmail);
    }

    public async Task ForgotPasswordAsync(SendVerificationOrForgotPasswordEmailDto sendForgotPasswordEmailDto) {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(sendForgotPasswordEmailDto.Email));

        if (user is null) {
            throw new ModelNotFoundException("Email not found");
        }

        sendForgotPasswordEmailDto.Token = await _tokenService.GenerateRandomTokenAsync() + sendForgotPasswordEmailDto.Email;

        user.ResetPasswordTokenGenerationTime = DateTime.UtcNow;
        user.ResetPasswordToken = sendForgotPasswordEmailDto.Token;

        await _emailService.SendForgotPasswordEmailAsync(sendForgotPasswordEmailDto);

        await _dataContext.SaveChangesAsync();
    }

    public async Task ConfirmRegistrationAsync(string resetPasswordToken) {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.RegistrationToken.Equals(resetPasswordToken));

        if (user is null) {
            throw new ModelNotFoundException("User not found");
        }

        user.IsConfirmedRegistrationToken = true;

        await _dataContext.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(ChangePasswordDto changePasswordDto) {
        if (!changePasswordDto.newPassword.Equals(changePasswordDto.repeatNewPassword)) {
            throw new PasswordException("Passwords doesn't match");
        }

        var user = await _dataContext.Users.FindAsync(changePasswordDto.Id);

        if (user is null) {
            throw new PasswordException("User not found");
        }

        user.Password = _encryptionService.GeneratedHashedPassword(changePasswordDto.newPassword);

        await _dataContext.SaveChangesAsync();
    }

    public async Task<Guid> GetUserIdByResetPasswordTokenAsync(string token) {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken.Equals(token));

        if (user is null) {
            throw new ModelNotFoundException("User not found");
        }

        return user.Id;
    }

    public async Task ChangeProfilePictureAsync(ChangeProfilePictureDto changeProfilePictureDto) {
        var file = changeProfilePictureDto.File;
        var folderName = Path.Combine(AppConstants.RESOURCES, AppConstants.USER_PROFILE_IMAGES);
        var pathToSave = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, AppConstants.RESOURCE_FOLDER, folderName);

        if (file.Length <= 0) {
            throw new ModelNotFoundException("File not found");
        }

        var fileName = file.FileName;
        var fullPath = Path.Combine(pathToSave, fileName);
        var dbPath = Path.Combine(folderName, fileName);

        var oldFile = await _dataContext.Users
            .Where(u => u.Id == changeProfilePictureDto.UserId)
            .Select(u => u.ProfilePicture)
            .FirstOrDefaultAsync();

        if (oldFile is not null) {

            oldFile = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, AppConstants.RESOURCE_FOLDER, oldFile);

            if (File.Exists(oldFile)) {
                File.Delete(oldFile);
            }
        }

        using (var stream = new FileStream(fullPath, FileMode.Create)) {
            file.CopyTo(stream);
        }

        var user = await _dataContext.Users.FindAsync(changeProfilePictureDto.UserId);

        if (user is null) {
            throw new ModelNotFoundException("User not found");
        }

        user.ProfilePicture = dbPath;
        await _dataContext.SaveChangesAsync();
    }

    public async Task<string> GetProfilePictureByIdAsync(Guid Id) {
        var profilePictureUrl = await _dataContext.Users
            .Where(u => u.Id == Id)
            .Select(u => u.ProfilePicture)
            .FirstOrDefaultAsync();

        if(profilePictureUrl is null) {
            return "";
        }

        var fullPath = Path.Combine(AppConstants.APP_URL, profilePictureUrl);

        return fullPath;
    }

    public async Task<UserPaginationDto> GetUsersPaginatedAsync(int page) {
        var pageResults = 10d;
        var pageCount = Math.Ceiling(_dataContext.Users.Count() / pageResults);

        if (page < 1) {
            page = 1;
        }

        if (page > (int)pageCount) {
            page = (int)pageCount;
        }

        var users = await _dataContext.Users
            .Skip((page - 1) * (int)pageResults)
            .Take((int)pageResults)
            .Select(u => new GetSearchedUserDto() {
                Username = u.Username,
                Email = u.Email,
                LastName = u.LastName,
                FirstName = u.FirstName,
            }).ToListAsync();

        if (users is null) {
            throw new ModelNotFoundException("Users not found");
        }

        var userPaginationDto = new UserPaginationDto {
            getSearchedUserDto = users,
            NumberOfUsers = _dataContext.Users.Count(),
            NumberOfPages = (int)pageCount,
        };

        return userPaginationDto;
    }

    public async Task<UserPaginationDto> GetAllSearchedUsersByEmailAsync(UserSearchDto userSearchDto){
        var pageResults = 10d;

        if (userSearchDto.Page < 1) {
            userSearchDto.Page = 1;
        }

        var usersQuery = _dataContext.Users
            .Where(u => u.Email.ToLower().Contains(userSearchDto.Search.ToLower()));

        var numberOfUsers = usersQuery.Count();

        var pageCount = Math.Ceiling(numberOfUsers / pageResults);

        if (userSearchDto.Page > (int)pageCount) {
            userSearchDto.Page = (int)pageCount;
        }

        if (numberOfUsers < pageResults) {
            pageResults = numberOfUsers;
        }

        var searchedUsers = await usersQuery
            .Skip((userSearchDto.Page - 1) * (int)pageResults)
            .Take((int)pageResults)
            .Select(u => new GetSearchedUserDto {
                Username = u.Username,
                Email = u.Email,
                LastName = u.LastName,
                FirstName = u.FirstName,
            }).ToListAsync();

        var userPaginationDto = new UserPaginationDto {
            getSearchedUserDto = searchedUsers,
            NumberOfUsers = numberOfUsers,
            NumberOfPages = (int)pageCount,
        };

        return userPaginationDto;
    }

    public async Task<UserPaginationDto> GetAllSearchedUsersByUserNameAsync(UserSearchDto userSearchDto) {
        var pageResults = 10d;

        if (userSearchDto.Page < 1) {
            userSearchDto.Page = 1;
        }

        var usersQuery = _dataContext.Users
            .Where(u => u.Username.ToLower().Contains(userSearchDto.Search.ToLower()));

        var numberOfUsers = usersQuery.Count();

        var pageCount = Math.Ceiling(numberOfUsers / pageResults);

        if (userSearchDto.Page > (int)pageCount) {
            userSearchDto.Page = (int)pageCount;
        }

        if (numberOfUsers < pageResults) {
            pageResults = numberOfUsers;
        }

        var searchedUsers = await usersQuery
            .Skip((userSearchDto.Page - 1) * (int)pageResults)
            .Take((int)pageResults)
            .Select(u => new GetSearchedUserDto {
                Username = u.Username,
                Email = u.Email,
                LastName = u.LastName,
                FirstName = u.FirstName,
            }).ToListAsync();

        var userPaginationDto = new UserPaginationDto {
            getSearchedUserDto = searchedUsers,
            NumberOfUsers = numberOfUsers,
            NumberOfPages = (int)pageCount,
        };

        return userPaginationDto;
    }

    private static FriendRequestFlag CheckIfUserSentFriendRequest(User searcher, string searchedName) {
        var searchedUser = searcher.FriendList
            .Where(u => u.Username.Equals(searchedName))
            .FirstOrDefault();

        if(searchedUser is not null) {
            return FriendRequestFlag.Approved;
        }

        var friendRequest = searcher.RecievedFriendRequest
            .Where(u => u.Sender.Username.Equals(searchedName))
            .FirstOrDefault();

        if(friendRequest is not null) {
            return FriendRequestFlag.Sent;
        }

        var friendRequestSent = searcher.SentFriendRequests
            .Where(u => u.Receiver.Username.Equals(searchedName))
            .FirstOrDefault();

        if(friendRequestSent is not null) {
            return FriendRequestFlag.Sent;
        }

        return FriendRequestFlag.None;
    }

    public async Task<UserPaginationDto> GetSearchedUsersByUserNameAsync(UserSearchDto userSearchDto) {
        var searcher = await _dataContext.Users
            .Where(u => u.Id == userSearchDto.SearcherId)
            .Include(u => u.RecievedFriendRequest)
            .ThenInclude(u => u.Sender)
            .Include(u => u.SentFriendRequests)
            .ThenInclude(u => u.Receiver)
            .Include(u => u.FriendList)
            .FirstOrDefaultAsync();

        if (searcher is null)
        {
            throw new ModelNotFoundException("User not found");
        }

        var pageResults = 5d;

        if (userSearchDto.Page < 1) {
            userSearchDto.Page = 1;
        }

        var usersQuery = _dataContext.Users
            .Where(u => u.Username.ToLower().Contains(userSearchDto.Search.ToLower()))
            .Include(u => u.RecievedFriendRequest);

        var numberOfUsers = usersQuery.Count();

        var pageCount = Math.Ceiling(numberOfUsers / pageResults);

        if (userSearchDto.Page > (int)pageCount) {
            userSearchDto.Page = (int)pageCount;
        }

        if (numberOfUsers < pageResults) {
            pageResults = numberOfUsers;
        }

        var searchedUsers = await usersQuery
            .Skip((userSearchDto.Page - 1) * (int)pageResults)
            .Take((int)pageResults)
            .Select(u => new GetSearchedUserDto() {
                Username = u.Username,
                Email = u.Email,
                LastName = u.LastName,
                FirstName = u.FirstName,
                FriendRequestFlag = CheckIfUserSentFriendRequest(searcher, u.Username),
            }).ToListAsync();

        var userPaginationDto = new UserPaginationDto {
            getSearchedUserDto = searchedUsers,
            NumberOfUsers = numberOfUsers,
            NumberOfPages = (int)pageCount,
        };

        return userPaginationDto;
    }

    public async Task<GetUserDto> GetUserByIdAsync(Guid id) {
        var user = await _dataContext.Users
            .Where(u => u.Id == id)
            .Select(u => new GetUserDto {
                Username = u.Username,
                Email = u.Email,
                LastName = u.LastName,
                FirstName = u.FirstName,
                ProfilePicture = Path.Combine(AppConstants.APP_URL, u.ProfilePicture ?? ""),
            }).FirstOrDefaultAsync();

        if (user == null) {
            throw new ModelNotFoundException("User not found");
        }

        return user;
    }


    public async Task EditUserAsync(EditUserDto editUserDto) {
        var user = await _dataContext.Users.FindAsync(editUserDto.UserId);

        if (user == null) {
            throw new ModelNotFoundException("User not found");
        }

        if (!user.Email.Equals(editUserDto.Email)) {
            if (VerifyExistingEmailAsync(editUserDto.Email).Result) {
                throw new AlreadyExistsException("Email already exists");
            }
        }

        if (!user.Username.Equals(editUserDto.Username)) {
            if (VerifyExistingUsernameAsync(editUserDto.Username).Result) {
                throw new AlreadyExistsException("Username already exists");
            }
        }

        UserMapper.EditUser(user, editUserDto);

        await _dataContext.SaveChangesAsync();
    }

    public async Task DeleteUserByNameAsync(string userName) {
        var user = await _dataContext.Users
            .Where(u => u.Username.Equals(userName))
            .FirstOrDefaultAsync();
        
        if (user == null) {
            throw new ModelNotFoundException("User not found");
        }

        var sentFriendRequests = await _dataContext.FriendRequests
            .Include(fr => fr.Sender)
            .Where(fr => fr.Sender.Id == user.Id)
            .ToListAsync();

        var receivedFriendRequests = await _dataContext.FriendRequests
            .Include(fr => fr.Receiver)
            .Where(fr => fr.Receiver.Id == user.Id)
            .ToListAsync();

        _dataContext.FriendRequests.RemoveRange(sentFriendRequests);
        _dataContext.FriendRequests.RemoveRange(receivedFriendRequests);

        _dataContext.Users.Remove(user);
        await _dataContext.SaveChangesAsync();
    }

    public async Task<GetFriendListDto> GetFriendListAsync(Guid Id) {
        var friendList = await _dataContext.Users
            .Where(u => u.Id == Id)
            .Include(u => u.FriendList)
            .Select(u => u.FriendList.Select(f => new GetUserDto {
                Username = f.Username,
                Email = f.Email,
                LastName = f.LastName,
                FirstName = f.FirstName,
                ProfilePicture = f.ProfilePicture,
            }).ToList()).FirstOrDefaultAsync();

        var getFriendListDto = new GetFriendListDto {
            Friends = friendList,
        };

        return getFriendListDto;
    }

    public async Task DeleteFriendAsync(DeleteFriendDto DeleteFriendDto) {
        var user = await _dataContext.Users
            .Where(u => u.Id == DeleteFriendDto.UserId)
            .Include(u => u.FriendList)
            .ThenInclude(u => u.FriendList)
            .Include(u => u.SentFriendRequests)
            .ThenInclude(u => u.Receiver)
            .Include(u => u.RecievedFriendRequest)
            .ThenInclude(u => u.Sender)
            .FirstOrDefaultAsync();

        if (user is null) {
            throw new ModelNotFoundException("User not found");
        }

        var friend = user.FriendList
            .Where(f => f.Username.Equals(DeleteFriendDto.FriendUsername))
            .FirstOrDefault();

        if (friend is null) {
            throw new ModelNotFoundException("Friend not found");
        }

        var friendRequestSent = user.SentFriendRequests
            .Where(u => u.Receiver.Username.Equals(DeleteFriendDto.FriendUsername))
            .FirstOrDefault();

        if (friendRequestSent is not null) {
            user.SentFriendRequests.Remove(friendRequestSent);
            _dataContext.FriendRequests.Remove(friendRequestSent);
        }

        var friendRequestReceived = user.RecievedFriendRequest
            .Where(u => u.Sender.Username.Equals(DeleteFriendDto.FriendUsername))
            .FirstOrDefault();

        if (friendRequestReceived is not null) {
            user.SentFriendRequests.Remove(friendRequestReceived);
            _dataContext.FriendRequests.Remove(friendRequestReceived);
        }

        user.FriendList.Remove(friend);
        friend.FriendList.Remove(user);

        await _dataContext.SaveChangesAsync();
    }

    public async Task GenerateXMLFileForUserAsync(string username) {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Username.Equals(username));

        if(user is null) {
            throw new ModelNotFoundException("User not found");
        }

        var folderName = Path.Combine(AppConstants.RESOURCES, AppConstants.XML_FILES);
        var pathToSave = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, AppConstants.RESOURCE_FOLDER, folderName, username);

        XmlSerializer ser = new XmlSerializer(typeof(User));

        using (StreamWriter wr = new StreamWriter(pathToSave, false, Encoding.UTF8)) {
            ser.Serialize(wr, user);
        }
    }
}
