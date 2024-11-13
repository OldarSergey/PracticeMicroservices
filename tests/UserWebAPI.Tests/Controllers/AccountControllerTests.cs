using Shared;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UserMicroservice.Application.DTOs;

namespace UserWebAPI.Tests.Controllers
{
    public class AccountControllerTests : IClassFixture<UserWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AccountControllerTests(UserWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }
        public static IEnumerable<object[]> GetTokenData()
        {
            yield return new object[] { TokenGenerator.GenerateTestToken() };
        }

        [Fact]
        public async Task RegisterUser_ReturnsOk()
        {
            // Arrange
            var registerUserDTO = new RegisterUserDTO
            {
                Name = "testuser",
                Email = "testuser@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/userwebapi/account/register", registerUserDTO);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadFromJsonAsync<RegistrationResponse>();
            Assert.NotNull(responseData);
        }

        [Theory]
        [MemberData(nameof(GetTokenData))]
        public async Task GetAllUser_ReturnsOk_WhenAuthorized(string token)
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/userwebapi/account/Users");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadFromJsonAsync<IEnumerable<UserDTO>>();
            Assert.NotNull(responseData);
        }
        [Theory]
        [InlineData("testuser@example.com", "Test@1234", true)]
        [InlineData("invaliduser@example.com", "password", false)]
        public async Task LoginUser_ReturnsExpectedResult(string email, string password, bool isSuccessExpected)
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                Email = email,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/userwebapi/account/login", loginDTO);

            // Assert
            if (isSuccessExpected)
            {
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();
                Assert.NotNull(responseData);
                Assert.NotNull(responseData.Token);
            }
            else
            {
                Assert.False(response.IsSuccessStatusCode);

                var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();
                Assert.NotNull(responseData);
                Assert.False(responseData.Flag);
                Assert.NotNull(responseData.Message);
            }
        }




    }

}
