using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Data.Data;
using Project.Data.Dto.Chat;
using Project.Data.Dto.User;
using Project.Data.Entities;
using Project.Services.Business.Exceptions;
using Project.Services.Contracts;
using SendGrid.Helpers.Errors.Model;
using System.ComponentModel.DataAnnotations;

namespace Project.Services.Business;
public class ChatService : IChatService {
    private readonly DataContext _dataContext;
    private readonly IUserService _userService;
    public ChatService(DataContext dataContext, IUserService userService) {
        _dataContext = dataContext;
        _userService = userService;
    }
    public async Task CreateChatAsync(CreateChatDto createChatDto) {
        var user1 = await _dataContext.Users
            .Include(u => u.ChatsAdmin)
            .FirstOrDefaultAsync(u => u.Username.Equals(createChatDto.Username1));

        if(user1 == null) {
            throw new ModelNotFoundException("User not found");
        }

        var user2 = await _dataContext.Users
            .Include (u => u.ChatsAdmin)
            .FirstOrDefaultAsync(u => u.Username.Equals(createChatDto.Username2));

        if(user2 == null) {
            throw new ModelNotFoundException("User not found");
        }

        var chat = new Chat {
            ChatAdmins = new HashSet<User>(),
            Users = new HashSet<User>(),
            Messages = new List<Message>(),
            Name = "",
        };

        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(chat, new ValidationContext(chat), results, validateAllProperties: true);
        var errorMessages = results.Select(x => x.ErrorMessage);

        if (!valid) {
            throw new BadRequestException(string.Join(" ", errorMessages));
        }

        await _dataContext.Chats.AddAsync(chat);

        chat.ChatAdmins.Add(user1);
        chat.ChatAdmins.Add(user2);

        user1.ChatsAdmin.Add(chat);
        user2.ChatsAdmin.Add(chat);

        await _dataContext.SaveChangesAsync();
    }

    public async Task LeaveChatAsync(LeaveChatDto leaveChatDto) {
        var user = await _dataContext.Users
            .Include(u => u.Chats)
            .Include(u => u.ChatsAdmin)
            .FirstOrDefaultAsync(u => u.Id == leaveChatDto.UserId);

        if(user == null) {
            throw new ModelNotFoundException("User not found");
        }

        var chat = await _dataContext.Chats
            .Include(c => c.Users)
            .ThenInclude(u => u.Chats)
            .Include(c => c.Users)
            .ThenInclude(u => u.ChatsAdmin)
            .Include(c => c.ChatAdmins)
            .FirstOrDefaultAsync(c => c.Id == leaveChatDto.ChatId);

        if(chat == null) {
            throw new ModelNotFoundException("Chat not found");
        }

        if(user.ChatsAdmin.Contains(chat) && chat.ChatAdmins.Count() == 2 && chat.Users.Count() == 0) {
            _dataContext.Remove(chat);

            var friend = chat.ChatAdmins
                .Where(u => u.Id != user.Id)
                .Select(u => u.Username)
                .FirstOrDefault();

            var deleteFriendDto = new DeleteFriendDto {
                UserId = user.Id,
                FriendUsername = friend
            };

            await _userService.DeleteFriendAsync(deleteFriendDto);
            await _dataContext.SaveChangesAsync();
            return;
        }

        if(user.ChatsAdmin.Contains(chat) && chat.ChatAdmins.Count() == 1 && chat.Users.Count() > 0) {
            var newAdmin = chat.Users.FirstOrDefault();

            chat.ChatAdmins.Remove(user);
            chat.ChatAdmins.Add(newAdmin);

            user.ChatsAdmin.Remove(chat);

            newAdmin.Chats.Remove(chat);
            newAdmin.ChatsAdmin.Add(chat);

            await _dataContext.SaveChangesAsync();
            return;
        }

        if(user.ChatsAdmin.Contains(chat) && chat.ChatAdmins.Count() == 1 && chat.Users.Count() == 0) {
            var oldFile = chat.ChatPicture;

            if (oldFile is not null) {

                oldFile = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, AppConstants.RESOURCE_FOLDER, oldFile);

                if (File.Exists(oldFile)) {
                    File.Delete(oldFile);
                }
            }

            _dataContext.Chats.Remove(chat);
            await _dataContext.SaveChangesAsync();
            return;
        }

        if (user.Chats.Contains(chat)) {
            user.Chats.Remove(chat);
            chat.Users.Remove(user);
            await _dataContext.SaveChangesAsync();
            return;
        }

        throw new ModelNotFoundException("Chat not found");
    }

    public async Task<GetChatsDto> GetChatsByUserIdAsync(Guid UserId) {
        var user = await _dataContext.Users
            .Include(u => u.Chats)
            .ThenInclude(c => c.Messages)
            .ThenInclude(m => m.User)
            .Include(u => u.Chats)
            .ThenInclude(c => c.ChatAdmins)
            .Include(u => u.Chats)
            .ThenInclude(c => c.Users)
            .Include(u => u.ChatsAdmin)
            .ThenInclude(c => c.Messages)
            .ThenInclude(m => m.User)
            .Include(u => u.ChatsAdmin)
            .ThenInclude(c => c.ChatAdmins)
            .Include(u => u.ChatsAdmin)
            .ThenInclude(c => c.Users)
            .FirstOrDefaultAsync(u => u.Id == UserId);

        if(user == null) {
            throw new ModelNotFoundException("User not found");
        }
        var simpleChats = user.Chats.Select(c => new GetChatDto {
            User = new GetUserDto {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
            },
            ChatId = c.Id,
            ChatName = c.Name,
            ChatPicture = Path.Combine(AppConstants.APP_URL, c.ChatPicture ?? ""),
            ChatAdmins = c.ChatAdmins.Select(c => new GetUserDto {
                Username = c.Username,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                ProfilePicture = Path.Combine(AppConstants.APP_URL, c.ProfilePicture ?? ""),
    }).ToList(),
            Users = c.Users.Select(u => new GetUserDto {
                Username = u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                ProfilePicture = Path.Combine(AppConstants.APP_URL, u.ProfilePicture ?? ""),
            }).ToList(),
        });

        var adminChats = user.ChatsAdmin.Select(c => new GetChatDto {
            User = new GetUserDto {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
            },
            ChatId = c.Id,
            ChatName = c.Name,
            ChatPicture = Path.Combine(AppConstants.APP_URL, c.ChatPicture ?? ""),
            ChatAdmins = c.ChatAdmins.Select(c => new GetUserDto {
                Username = c.Username,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                ProfilePicture = Path.Combine(AppConstants.APP_URL, c.ProfilePicture ?? ""),
            }).ToList(),
            Users = c.Users.Select(u => new GetUserDto {
                Username = u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                ProfilePicture = Path.Combine(AppConstants.APP_URL, u.ProfilePicture ?? ""),
            }).ToList(),
        });

        return new GetChatsDto {
            Chats = simpleChats.Concat(adminChats).ToList(),
        };
    }

    public async Task CreatGroupChatAsync(CreateGroupChatDto createGroupChatDto) {
        if(createGroupChatDto.Usernames.Count() < 2) {
            throw new ModelNotFoundException("You need more users to create a group chat");
        }

        var admin = await _dataContext.Users
            .Include(u => u.ChatsAdmin)
            .FirstOrDefaultAsync(u => u.Id == createGroupChatDto.UserId);

        if(admin == null) {
            throw new ModelNotFoundException("User not found");
        }

        var chat = new Chat {
            Name = createGroupChatDto.ChatName,
            ChatAdmins = new HashSet<User> { admin },
            Users = new HashSet<User>(),
        };

        var file = createGroupChatDto.File;
        var folderName = Path.Combine(AppConstants.RESOURCES, AppConstants.GROUP_CHAT_IMAGES);
        var pathToSave = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, AppConstants.RESOURCE_FOLDER, folderName);

        if (file.Length <= 0) {
            throw new ModelNotFoundException("File not found");
        }

        var fileName = file.FileName;
        var fullPath = Path.Combine(pathToSave, fileName);
        var dbPath = Path.Combine(folderName, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create)) {
            file.CopyTo(stream);
        }

        chat.ChatPicture = dbPath;

        await _dataContext.Chats.AddAsync(chat);


        admin.ChatsAdmin.Add(chat);

        foreach(string username in createGroupChatDto.Usernames) {
            var user = await _dataContext.Users
                .Include(u => u.Chats)
                .FirstOrDefaultAsync(u => u.Username.Equals(username));

            if(user == null) {
                throw new ModelNotFoundException("User not found");
            }

            chat.Users.Add(user);
            user.Chats.Add(chat);
        }

        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(chat, new ValidationContext(chat), results, validateAllProperties: true);
        var errorMessages = results.Select(x => x.ErrorMessage);

        if (!valid) {
            throw new BadRequestException(string.Join(" ", errorMessages));
        }

        await _dataContext.SaveChangesAsync();
    }

    public async Task EditGroupChatAsync(EditGroupChatDto editGroupChatDto) {
        var user = await _dataContext.Users.FindAsync(editGroupChatDto.UserId);

        if(user == null) {
            throw new ModelNotFoundException("User not found");
        }

        var chat = await _dataContext.Chats
            .Include(c => c.ChatAdmins)
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == editGroupChatDto.ChatId);

        if(chat == null) {
            throw new ModelNotFoundException("Chat not found");
        }

        if(!chat.ChatAdmins.Contains(user)) {
            throw new ModelNotFoundException("You are not an admin of this chat");
        }

        chat.Name = editGroupChatDto.ChatName;
        
        foreach(var userToBeAdded in editGroupChatDto.UsersToBeAdded) {
            var userToAdd = await _dataContext.Users
                .Include(u => u.Chats)
                .FirstOrDefaultAsync(u => u.Username.Equals(userToBeAdded));
            if(userToAdd == null) {
                throw new ModelNotFoundException("User not found");
            }
            if(!chat.Users.Contains(userToAdd)) {
                chat.Users.Add(userToAdd);
                userToAdd.Chats.Add(chat);
            }
        }

        foreach(var adminsToBeAdded in editGroupChatDto.AdminsToBeAdded) {
            var adminToAdd = await _dataContext.Users
                .Include(u => u.ChatsAdmin)
                .Include(u => u.Chats)
                .FirstOrDefaultAsync(u => u.Username.Equals(adminsToBeAdded));

            if(adminToAdd == null) {
                throw new ModelNotFoundException("User not found");
            }

            if (!chat.Users.Contains(adminToAdd)) {
                throw new ModelNotFoundException("User not in the group");
            }

            if(!chat.ChatAdmins.Contains(adminToAdd)) {
                chat.ChatAdmins.Add(adminToAdd);
                adminToAdd.ChatsAdmin.Add(chat);
                adminToAdd.Chats.Remove(chat);
            }
        }

        foreach(var memberToBeRemoved in editGroupChatDto.MembersToBeRemoved) {
            var memberToRemove = await _dataContext.Users
                .Include(u => u.Chats)
                .FirstOrDefaultAsync(u => u.Username.Equals(memberToBeRemoved));
            if(memberToRemove == null) {
                throw new ModelNotFoundException("User not found");
            }
            if(!chat.Users.Contains(memberToRemove)) {
                throw new ModelNotFoundException("User not in the group");
            }
            chat.Users.Remove(memberToRemove);
            memberToRemove.Chats.Remove(chat);
        }

        await _dataContext.SaveChangesAsync();
    }

    public async Task  ChangeGroupChatProfilePictureAsync(ChangeGroupChatProfilePictureDto changeGroupChatProfilePictureDto) {
        var file = changeGroupChatProfilePictureDto.File;
        var folderName = Path.Combine(AppConstants.RESOURCES, AppConstants.GROUP_CHAT_IMAGES);
        var pathToSave = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, AppConstants.RESOURCE_FOLDER, folderName);

        if (file.Length <= 0) {
            throw new ModelNotFoundException("File not found");
        }

        var fileName = file.FileName;
        var fullPath = Path.Combine(pathToSave, fileName);
        var dbPath = Path.Combine(folderName, fileName);

        var oldFile = await _dataContext.Chats
            .Where(u => u.Id == changeGroupChatProfilePictureDto.ChatId)
            .Select(u => u.ChatPicture)
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

        var chat = await _dataContext.Chats.FindAsync(changeGroupChatProfilePictureDto.ChatId);

        if (chat is null) {
            throw new ModelNotFoundException("User not found");
        }

        chat.ChatPicture = dbPath;
        await _dataContext.SaveChangesAsync();
    }

}
