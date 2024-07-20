using Project.Data.Dto.User;
using Project.Data.Entities;

namespace Project.Data.Mappers;
public static class UserMapper {
    public static User ToUser(this UserRegistrationDto userDto) {
        return new User {
            Email = userDto.Email,
            Username = userDto.Username,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Password = userDto.Password,
        };
    }

    public static void EditUser(this User user, EditUserDto editUserDto) {
        user.FirstName = editUserDto.FirstName;
        user.LastName = editUserDto.LastName;
        user.Email = editUserDto.Email;
        user.Username = editUserDto.Username;
    }
}
