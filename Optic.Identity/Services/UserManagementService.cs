using Microsoft.AspNetCore.Identity;
using Optic.Identity.Exceptions;
using Optic.Identity.Models;
using Optic.Identity.Models.Requests;
using Optic.Identity.Repositories;
using Optic.Identity.Services.FileStorageProviders;
using Optic.Identity.Services.Interfaces;
using Optic.Identity.Services.TimeZone;

namespace Optic.Identity.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly ITimeZoneService _timeZoneService;

    public UserManagementService(
        IUserRepository userRepository,
        IFileStorageProvider fileStorageProvider,
        ITimeZoneService timeZoneService,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _fileStorageProvider = fileStorageProvider;
        _timeZoneService = timeZoneService;
        _passwordHasher = passwordHasher;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();

        foreach (var user in users)
        {
            ApplyUserTimeZone(user);
        }

        return users;
    }
    public async Task<User?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new BadRequestException("User ID must be greater than zero.");

        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            throw new NotFoundException("User was not found.");

        ApplyUserTimeZone(user);

        return user;
    }

    public async Task<int> CreateAsync(
        CreateUserRequest request,
        string createdBy)
    {
        if (request == null)
            throw new BadRequestException("User details are required.");

        if (string.IsNullOrWhiteSpace(request.EmployeeCode))
            throw new BadRequestException("Employee Code is required.");

        if (string.IsNullOrWhiteSpace(request.Username))
            throw new BadRequestException("Username is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BadRequestException("Password is required.");

        string? avatarUrl = null;

        if (request.ProfilePicture != null &&
            request.ProfilePicture.Length > 0)
        {
            avatarUrl = await _fileStorageProvider.SaveFileAsync(
                request.ProfilePicture,
                "Uploads");
        }

        var user = new User
        {
            EmployeeCode = request.EmployeeCode.Trim(),
            Username = request.Username.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            Mobile = request.Mobile?.Trim(),
            EmployeeTypeId = request.EmployeeTypeId,
            IsActive = true,
            IsDeleted = false,
            AvatarUrl = avatarUrl,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        return await _userRepository.CreateAsync(user, createdBy);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateUserRequest request,
        string updatedBy)
    {
        if (id <= 0)
            throw new BadRequestException("User ID must be greater than zero.");

        if (request == null)
            throw new BadRequestException("User details are required.");

        var existingUser = await _userRepository.GetByIdAsync(id);

        if (existingUser == null)
            throw new NotFoundException("User was not found.");

        var oldAvatarUrl = existingUser.AvatarUrl;
        var newAvatarUploaded = false;

        if (request.ProfilePicture != null &&
            request.ProfilePicture.Length > 0)
        {
            existingUser.AvatarUrl = await _fileStorageProvider.SaveFileAsync(
                request.ProfilePicture,
                "Uploads");

            newAvatarUploaded = true;
        }

        existingUser.EmployeeCode = request.EmployeeCode.Trim();
        existingUser.Username = request.Username.Trim();
        existingUser.FirstName = request.FirstName.Trim();
        existingUser.LastName = request.LastName.Trim();
        existingUser.Email = request.Email.Trim();
        existingUser.Mobile = request.Mobile?.Trim();
        existingUser.EmployeeTypeId = request.EmployeeTypeId;
        existingUser.IsActive = request.IsActive;
        existingUser.UpdatedAt = DateTime.UtcNow;
        existingUser.UpdatedBy = updatedBy;

        var updated = await _userRepository.UpdateAsync(
            existingUser,
            updatedBy);

        if (!updated)
        {
            if (newAvatarUploaded && existingUser.AvatarUrl != null)
            {
                await _fileStorageProvider.DeleteFileAsync(
                    existingUser.AvatarUrl);
            }

            throw new NotFoundException(
                "User was not found or could not be updated.");
        }

        if (newAvatarUploaded &&
            !string.IsNullOrWhiteSpace(oldAvatarUrl))
        {
            await _fileStorageProvider.DeleteFileAsync(oldAvatarUrl);
        }

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
            throw new BadRequestException("User ID must be greater than zero.");

        var existingUser = await _userRepository.GetByIdAsync(id);

        if (existingUser == null)
            throw new NotFoundException("User was not found or is already deleted.");

        var deleted = await _userRepository.DeleteAsync(id, null);

        if (!deleted)
            throw new NotFoundException("User was not found or is already deleted.");

        return true;
    }

    private void ApplyUserTimeZone(User user)
    {
        user.CreatedAt = _timeZoneService.ToLocalTime(user.CreatedAt);
        user.UpdatedAt = _timeZoneService.ToLocalTime(user.UpdatedAt);
        user.LastLoginAt = _timeZoneService.ToLocalTime(user.LastLoginAt);
        user.PasswordChangedAt = _timeZoneService.ToLocalTime(user.PasswordChangedAt);
    }
}
