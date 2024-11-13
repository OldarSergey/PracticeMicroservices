using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMicroservice.Application.DTOs;

namespace UserMicroservice.Application.Contracts
{
    public interface IUserService
    {
        Task<RegistrationResponse> RegisterUserAsync(RegisterUserDTO registerUserDTO);
        Task<LoginResponse> LoginUserAsync(LoginDTO loginDTO);
        Task<OperationResponse> VerifyOtpRegistrationAsync(VerifyOtpRequest request);
        Task<OperationResponse> UpdateUserAsync(Guid userId, UpdateUserDTO updateUserDTO);
        Task<List<UserDTO>> GetAllActiveUsers();
        Task<OperationResponse> DeleteUserAsync(Guid userId);
        Task<OperationResponse> RequestPasswordResetAsync(Guid userId, string email);
        Task<OperationResponse> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO);
        Task<OperationResponse> RequestEmailResetAsync(Guid userId, UpdateEmailUserDTO updateEmailUserDTO);
        Task<OperationResponse> ConfirmEmailChangeAsync(string confirmationCode, Guid userId);
        Task<UserDTO> GetUserByIdAsync(Guid userId);

    }
}
