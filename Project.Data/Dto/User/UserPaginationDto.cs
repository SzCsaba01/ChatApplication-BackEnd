﻿namespace Project.Data.Dto.User;
public class UserPaginationDto {
    public ICollection<GetSearchedUserDto> getSearchedUserDto { get; set; }
    public int NumberOfUsers { get; set; }
    public int NumberOfPages { get; set; }
}
