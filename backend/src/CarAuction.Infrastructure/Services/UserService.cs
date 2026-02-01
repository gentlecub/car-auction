using AutoMapper;
using CarAuction.Application.DTOs.Common;
using CarAuction.Application.DTOs.User;
using CarAuction.Application.Interfaces;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Enums;
using CarAuction.Domain.Exceptions;
using CarAuction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarAuction.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UserService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserDto> GetByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), id);
        }

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> GetCurrentUserAsync(int userId)
    {
        return await GetByIdAsync(userId);
    }

    public async Task<UserDto> UpdateAsync(int userId, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        if (!string.IsNullOrEmpty(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrEmpty(request.LastName))
            user.LastName = request.LastName;

        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(userId);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new BadRequestException("La contraseña actual es incorrecta");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 12);
        await _context.SaveChangesAsync();
    }

    public async Task<PaginatedResult<UserDto>> GetAllAsync(PaginationRequest request)
    {
        var query = _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

        var totalItems = await query.CountAsync();

        query = request.SortBy?.ToLower() switch
        {
            "email" => request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "name" => request.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "status" => request.SortDescending ? query.OrderByDescending(u => u.Status) : query.OrderBy(u => u.Status),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        var users = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PaginatedResult<UserDto>
        {
            Items = _mapper.Map<IEnumerable<UserDto>>(users),
            TotalItems = totalItems,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task ActivateUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), id);
        }

        user.Status = UserStatus.Active;
        await _context.SaveChangesAsync();
    }

    public async Task DeactivateUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), id);
        }

        user.Status = UserStatus.Inactive;
        await _context.SaveChangesAsync();
    }
}
