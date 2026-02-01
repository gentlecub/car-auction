using CarAuction.Application.DTOs.Common;
using CarAuction.Application.DTOs.User;

namespace CarAuction.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(int id);
    Task<UserDto> GetCurrentUserAsync(int userId);
    Task<UserDto> UpdateAsync(int userId, UpdateUserRequest request);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<PaginatedResult<UserDto>> GetAllAsync(PaginationRequest request);
    Task ActivateUserAsync(int id);
    Task DeactivateUserAsync(int id);
}
