using LLMLab.Dtos.User;
using LLMLab.Server.Data;
using Riok.Mapperly.Abstractions;

namespace LLMLab.Server.Mappers;

[Mapper]
public partial class UserMapper
{
    public static partial UserDto Map(ApplicationUser user);
    
    public static partial void Map(UpdateUserDto updateUserDto, ApplicationUser user);
}