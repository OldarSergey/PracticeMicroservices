using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Kafka;
using System.Security.Claims;
using System.Text.Json;
using UserMicroservice.Application.Contracts;
using UserMicroservice.Application.DTOs;
namespace UserWebAPI.Controllers
{
    [ApiController]
    [Route("userwebapi/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IKafkaProducer<string, string> _kafkaProducer;

        public AccountController(IUserService user, IKafkaProducer<string, string> kafkaProducer)
        {
            _user = user;
            _kafkaProducer = kafkaProducer;
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> LoginUser(LoginDTO loginDTO)
        {
            var result = await _user.LoginUserAsync(loginDTO);

            if (!result.Flag)
            {
                return result.Message == "User not found" ? NotFound(result) : Unauthorized(result);
            }

            return Ok(result);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> RegisterUser(RegisterUserDTO registerUserDTO)
        {
            var result = await _user.RegisterUserAsync(registerUserDTO);

            return Ok(result);
        }
        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
                return BadRequest("Invalid request.");

            var response = await _user.VerifyOtpRegistrationAsync(request);

            if (response.Flag)
                return Ok(response.Message);
            else
                return BadRequest(response.Message);

        }

        [HttpPut("update/{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserDTO updateUserDTO)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || Guid.Parse(userIdClaim) != userId)
                return Unauthorized("Insufficient permissions to perform the operation");

            if (updateUserDTO == null)
                return BadRequest("UpdateUserDTO cannot be null.");

            var user = await _user.UpdateUserAsync(userId, updateUserDTO);
            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }

        [HttpGet("Users")]
        public async Task<IActionResult> GetAllActiveUsers()
        {
            var users = await _user.GetAllActiveUsers();
            return Ok(users);
        }
        [HttpGet("user-info/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var user = await _user.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            return Ok(new { user.Id, user.Email, user.Name });
        }

        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || Guid.Parse(userIdClaim) != userId)
                return Unauthorized("Insufficient permissions to perform the operation");

            var response = await _user.DeleteUserAsync(userId);

            if (response.Flag)
                return Ok(response);
            return BadRequest(response);
        }
        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset(string email)
        {

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var response = await _user.RequestPasswordResetAsync(Guid.Parse(userIdClaim), email);
            if (response.Flag)
                return Ok(response.Message);
            else
                return BadRequest(response);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            var response = await _user.ResetPasswordAsync(resetPasswordDTO);
            if (response.Flag)
                return Ok(response.Message);
            else
                return BadRequest(response.Message);
        }

        [HttpPost("request-email-reset")]
        public async Task<IActionResult> RequestEmailReset([FromBody] UpdateEmailUserDTO updateEmailUserDTO)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var response = await _user.RequestEmailResetAsync(Guid.Parse(userIdClaim), updateEmailUserDTO);

            if (response.Flag)
                return Ok(response.Message);

            return BadRequest(response.Message);
        }

        [HttpPost("confirm-email-reset")]
        public async Task<IActionResult> ConfirmEmailReset(string otpCode)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var response = await _user.ConfirmEmailChangeAsync(otpCode, Guid.Parse(userIdClaim));

            if (response.Flag)
            {
                return Ok(response.Message);
            }

            return BadRequest(response.Message);
        }
    }
}
