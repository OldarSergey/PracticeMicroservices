using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Shared.Contracts.Kafka;
using Shared.CustomException;
using Shared.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UserMicroservice.Application.Contracts;
using UserMicroservice.Application.CustomException;
using UserMicroservice.Application.DTOs;
using UserMicroservice.Application.DTOs.MessageKafka;
using UserMicroservice.Domain.Entities;
using UserMicroservice.Infrastructure.Data;

namespace UserMicroservice.Infrastructure.Rep
{
    public class UserService : IUserService
    {
        private readonly UserDbContext _userContext;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly IKafkaProducer<string, string> _kafkaProducer;
        private readonly ILogger<UserService> _logger;

        public UserService(

            UserDbContext appDbContext,
            IConfiguration configuration,
            IMemoryCache memoryCache,
            IKafkaProducer<string, string> kafkaProducer,
            ILogger<UserService> logger)

        {
            _userContext = appDbContext;
            _configuration = configuration;
            _memoryCache = memoryCache;
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginUserAsync(LoginDTO loginDTO)
        {
            try
            {
                var getUser = await FindUserByEmail(loginDTO.Email);
                if (getUser == null)
                    throw new UserNotFoundByEmailException(loginDTO.Email);

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDTO.Password, getUser.Password);
                if (isPasswordValid)
                {
                    var user = new UserDTO
                    {
                        Id = getUser.Id,
                        Email = getUser.Email,
                        Name = getUser.Name
                    };
                    return new LoginResponse(true, "Login successfully", GenerateJWTToken(getUser), user);
                }
                else
                    throw new InvalidCredentialsException();
            }
            catch (UserNotFoundByEmailException ex)
            {
                _logger.LogError(ex, "User login attempt failed for email: {Email}", loginDTO.Email);
                return new LoginResponse(false, ex.Message);
            }
            catch (InvalidCredentialsException ex)
            {
                _logger.LogError(ex, "Invalid credentials for email: {Email}", loginDTO.Email);
                return new LoginResponse(false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login attempt for email: {Email}", loginDTO.Email);
                return new LoginResponse(false, "An unexpected error occurred during login");
            }
        }

        private string GenerateJWTToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:audience"],
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<User> FindUserByEmail(string email)
        {
            return await _userContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted && u.IsEmailConfirmed);
        }
        public async Task<RegistrationResponse> RegisterUserAsync(RegisterUserDTO registerUserDTO)
        {
            try
            {
                var existingUser = await _userContext.Users
                    .AnyAsync(u => u.Email == registerUserDTO.Email && !u.IsDeleted && u.IsEmailConfirmed);

                if (existingUser)
                    throw new UserAlreadyExistsException(registerUserDTO.Email);

                var userRoleId = await _userContext.Roles
                    .Where(r => r.Name == "User")
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync();

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Name = registerUserDTO.Name,
                    Email = registerUserDTO.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerUserDTO.Password),
                    RoleId = userRoleId
                };

                _userContext.Users.Add(newUser);
                await _userContext.SaveChangesAsync();

                await SendOtpEmailAsync(registerUserDTO.Email, OtpOperationTypes.ConfirmationAccount, newUser.Id);

                return new RegistrationResponse(true, "Registration completed. Please verify your email using the OTP sent.");
            }
            catch (UserAlreadyExistsException ex)
            {
                _logger.LogError(ex, "Registration attempt failed for email: {Email}", registerUserDTO.Email);
                return new RegistrationResponse(false, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, ex.Message);
                return new RegistrationResponse(false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for email: {Email}", registerUserDTO.Email);
                return new RegistrationResponse(false, "An unexpected error occurred during registration");
            }
        }

        public async Task<OperationResponse> UpdateUserAsync(Guid userId, UpdateUserDTO updateUserDTO)
        {
            try
            {
                var user = await _userContext.Users
                  .Where(u => !u.IsDeleted && u.Id == userId)
                  .FirstOrDefaultAsync();
                if (user == null)
                    throw new UserNotFoundException(userId);

                user.Name = updateUserDTO.Name;

                _userContext.Users.Update(user);

                await _userContext.SaveChangesAsync();

                await _kafkaProducer.ProduceAsync("UpdateUser", user.Id.ToString(), JsonSerializer.Serialize(user));

                return new OperationResponse(true, "Update completed successfully", user);
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogError(ex, "User update failed for user ID: {UserId}", userId);
                return new OperationResponse(false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user update for user ID: {UserId}", userId);
                return new OperationResponse(false, "An unexpected error occurred during user update");
            }
        }

        public async Task<UserDTO> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await _userContext.Users
                .Where(u => u.Id == userId && !u.IsDeleted && u.IsEmailConfirmed)
                .Select(u => new UserDTO { Id = u.Id, Name = u.Name, Email = u.Email })
                .FirstOrDefaultAsync();
                if (user == null)
                    throw new UserNotFoundException(userId);
                return user;
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<List<UserDTO>> GetAllActiveUsers()
        {
            try
            {
                var users = await _userContext.Users
                     .Where(u => !u.IsDeleted && u.IsEmailConfirmed)
                     .Select(u => new UserDTO
                     {
                         Id = u.Id,
                         Name = u.Name,
                         Email = u.Email
                     })
                     .ToListAsync();
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving active users.");
                throw new DataRetrievalException("An unexpected error occurred while retrieving active users.", ex);
            }
        }
        public async Task<OperationResponse> DeleteUserAsync(Guid userId)
        {
            try
            {
                var user = await _userContext.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

                if (user == null)
                    throw new UserNotFoundException(userId);

                user.IsDeleted = true;
                var userDeleteDto = new UserDeleteDTO()
                {
                    UserId = user.Id,
                    UserName = user.Name
                };
                _userContext.Users.Update(user);
                await _userContext.SaveChangesAsync();
                await _kafkaProducer.ProduceAsync("user-delete", user.Id.ToString(), JsonSerializer.Serialize(userDeleteDto));
                return new OperationResponse(true, "User deleted successfully");
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogError(ex, "User delete failed for user ID: {UserId}", userId);
                return new OperationResponse(false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user delete for user ID: {UserId}", userId);
                return new OperationResponse(false, "An unexpected error occurred during user delete");
            }
        }


        public async Task<OperationResponse> RequestPasswordResetAsync(Guid userId, string email)
        {
            try
            {
                var user = await _userContext.Users.FirstOrDefaultAsync(u => u.Email == email && u.Id == userId && !u.IsDeleted);
                if (user == null)
                    throw new UserNotFoundByEmailException(email);

                await SendOtpEmailAsync(email, OtpOperationTypes.PasswordReset, user.Id);

                return new OperationResponse(true, "The OTP code for password reset has been sent to your email");
            }
            catch (UserNotFoundByEmailException ex)
            {
                _logger.LogError(ex, $"User with email '{email}' not found");
                return new OperationResponse(false, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, ex.Message);
                return new OperationResponse(false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while requesting password reset");
                return new OperationResponse(false, "An unexpected error occurred. Please try again later");
            }
        }
        public async Task<OperationResponse> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
        {
            try
            {
                var user = await _userContext.Users.FirstOrDefaultAsync(u => u.Email == resetPasswordDTO.Email && !u.IsDeleted);
                if (user == null)
                    throw new UserNotFoundByEmailException(resetPasswordDTO.Email);

                bool isSamePassword = BCrypt.Net.BCrypt.Verify(resetPasswordDTO.NewPassword, user.Password);
                if (isSamePassword)
                    return new OperationResponse(false, "The new password cannot be the same as the old password");

                var cacheKey = $"otp_{resetPasswordDTO.Email}_{OtpOperationTypes.PasswordReset}";

                if (!_memoryCache.TryGetValue(cacheKey, out string cachedOtp) || cachedOtp != resetPasswordDTO.OtpCode)
                    return new OperationResponse(false, "Invalid or expired OTP code");

                user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDTO.NewPassword);

                _memoryCache.Remove(cacheKey);

                await _userContext.SaveChangesAsync();

                return new OperationResponse(true, "Password has been reset successfully");
            }
            catch (UserNotFoundByEmailException ex)
            {
                _logger.LogError(ex, $"User with email '{resetPasswordDTO.Email}' not found");
                return new OperationResponse(false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Password reset failed for email '{resetPasswordDTO.Email}'");
                return new OperationResponse(false, "An unexpected error occurred while resetting the password");
            }
        }




        public async Task<OperationResponse> RequestEmailResetAsync(Guid userId, UpdateEmailUserDTO updateEmailUserDTO)
        {
            try
            {
                var user = await _userContext.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
                if (user == null)
                    throw new UserNotFoundException(userId);

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(updateEmailUserDTO.Password, user.Password);
                if (!isPasswordValid)
                    return new OperationResponse(false, "Invalid password");

                var emailExists = await _userContext.Users.AnyAsync(u => (u.Email == updateEmailUserDTO.NewEmail || u.NewEmail == updateEmailUserDTO.NewEmail) && u.IsEmailConfirmed);
                if (emailExists)
                    return new OperationResponse(false, "The new email address is already in use by another user.");

                user.NewEmail = updateEmailUserDTO.NewEmail;
                await _userContext.SaveChangesAsync();

                await SendOtpEmailAsync(user.NewEmail, OtpOperationTypes.EmailChange, user.Id);

                var messageNotificationKafka = new NotificationDTO(user.Email, "Notification", "You have requested an email change. Please verify your new email to complete the process");
                await _kafkaProducer.ProduceAsync("send-notification", userId.ToString(), JsonSerializer.Serialize(messageNotificationKafka));

                return new OperationResponse(true, "OTP code has been sent to the new email address.");
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogError(ex, $"User with ID '{userId}' not found");
                return new OperationResponse(false, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, ex.Message);
                return new OperationResponse(false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error during email reset request for user ID: {userId}");
                return new OperationResponse(false, "An unexpected error occurred");
            }
        }

        public async Task<OperationResponse> ConfirmEmailChangeAsync(string confirmationCode, Guid userId)
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
                throw new UserNotFoundException(userId);


            return await HandleOtpAsync(user.NewEmail, confirmationCode, OtpOperationTypes.EmailChange, user);
        }

        private async Task<OperationResponse> SendOtpEmailAsync(string toEmail, string operationType, Guid userId)
        {
            try
            {
                var cacheKey = $"otp_{toEmail}_{operationType}";
                var lastRequestKey = $"{cacheKey}_lastRequest";

                if (_memoryCache.TryGetValue(lastRequestKey, out DateTime lastRequestTime))
                {
                    var timeSinceLastRequest = DateTime.UtcNow - lastRequestTime;
                    const int otpRequestIntervalInSeconds = 30;
                    if (timeSinceLastRequest < TimeSpan.FromSeconds(otpRequestIntervalInSeconds))
                    {
                        var waitTime = TimeSpan.FromSeconds(otpRequestIntervalInSeconds) - timeSinceLastRequest;
                        throw new InvalidOperationException($"OTP request limit exceeded. Please wait {waitTime.Seconds} seconds before requesting another OTP.");
                    }
                }

                var otp = GenerateOtp();

                _memoryCache.Set(cacheKey, otp, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
                _memoryCache.Set(lastRequestKey, DateTime.UtcNow, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

                var subject = "Your OTP Code";
                var message = $"Your OTP code is {otp}. It will expire in 10 minutes";

                var messageNotificationKafka = new NotificationDTO(toEmail, subject, message);

                await _kafkaProducer.ProduceAsync("send-notification", userId.ToString(), JsonSerializer.Serialize(messageNotificationKafka));

                return new OperationResponse(true, $"OTP code has been successfully sent to the email {toEmail}");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during OTP sending");
                throw;
            }
        }

        public async Task<OperationResponse> VerifyOtpRegistrationAsync(VerifyOtpRequest request)
        {
            var user = await _userContext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);

            if (user == null)
                return new OperationResponse(false, "User not found");

            return await HandleOtpAsync(request.Email, request.Otp, OtpOperationTypes.ConfirmationAccount, user);
        }


        private async Task<OperationResponse> HandleOtpAsync(string email, string otp, string operationType, User user)
        {
            try
            {
                var cacheKey = $"otp_{email}_{operationType}";

                if (_memoryCache.TryGetValue(cacheKey, out string cachedOtp) && cachedOtp == otp)
                {


                    switch (operationType)
                    {
                        case OtpOperationTypes.ConfirmationAccount:
                            user.IsEmailConfirmed = true;
                            _memoryCache.Remove(cacheKey);
                            break;
                        case OtpOperationTypes.EmailChange:
                            user.Email = user.NewEmail;
                            user.NewEmail = null;
                            _memoryCache.Remove(cacheKey);
                            break;
                        default:
                            return new OperationResponse(false, "Invalid operation type.");
                    }

                    await _userContext.SaveChangesAsync();
                    return new OperationResponse(true, $"{operationType} completed successfully.");
                }

                return new OperationResponse(false, "Invalid or expired OTP.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error during {operationType} for email: {email}");
                return new OperationResponse(false, $"An unexpected error occurred during {operationType}.");
            }
        }
        private string GenerateOtp()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var tokenData = new byte[4];
                rng.GetBytes(tokenData);
                return BitConverter.ToUInt32(tokenData, 0).ToString("D6");
            }
        }

    }
}
