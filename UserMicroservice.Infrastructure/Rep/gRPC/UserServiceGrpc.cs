using Grpc.Core;
using UserMicroservice.Application.Contracts;
using UserMicroservice.Protos;

namespace UserMicroservice.Infrastructure.Rep.gRPC
{
    public class UserServiceGrpc : UserMicroservice.Protos.UserService.UserServiceBase
    {
        private readonly IUserService _userService;

        public UserServiceGrpc(IUserService userService)
        {
            _userService = userService;
        }

        public override async Task<GetUserInfoResponse> GetUserInfo(GetUserInfoRequest request, ServerCallContext context)
        {
            var user = await _userService.GetUserByIdAsync(Guid.Parse(request.UserId));

            if(user == null)
            {
                return new GetUserInfoResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }
            return new GetUserInfoResponse
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                Name = user.Name,
                Success = true,
                Message = "User info retrieved successfully"
            };
        }
    }
}