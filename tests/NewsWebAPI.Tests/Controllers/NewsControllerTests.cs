using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using Shared;
using NewsMicroservice.Application.DTOs;

namespace NewsWebAPI.Tests.Controllers
{
    public class NewsControllerTests : IClassFixture<NewsWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public NewsControllerTests(NewsWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }


        [Theory]
        [InlineData("C4725F3E-E024-46D9-B93F-4F6AD8BEC02A", true)]
        [InlineData("99999999-9999-9999-9999-999999999999", false)]

        public async Task CreateServiceNews_ReturnsExpectedResult(Guid userId, bool isSuccessExpected)
        {
            // Arrange
            var serviceNewsDto = new CreateServiceNewsDTO
            {
                Title = "Test News",
                Description = "This is a test news content",
                ShortDescription = "Test news",
                Skills = "Program"

            };

            var token = TokenGenerator.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync($"/newswebapi/news/create?userId={userId}", serviceNewsDto);

            // Assert
            if (isSuccessExpected)
            {
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadFromJsonAsync<CreateServiceNewsDTO>();
                Assert.NotNull(responseData);
            }
            else
            {
                Assert.False(response.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }

        }

        [Theory]
        [InlineData("C4725F3E-E024-46D9-B93F-4F6AD8BEC02A", true)]
        public async Task GetUserServiceNews_ReturnsExpectedResult(Guid userId, bool isSuccessExpected)
        {
            // Arrange
            var token = TokenGenerator.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync($"/newswebapi/news/user/{userId}");

            // Assert
            if (isSuccessExpected)
            {
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadFromJsonAsync<IEnumerable<OutputServiceNewsDTO>>();
                Assert.NotNull(responseData);
                Assert.NotEmpty(responseData);
            }
        }

        [Theory]
        [InlineData("Test")]
        public async Task GetAllServiceNewsByFilter_ReturnsOk(string filter)
        {
            // Arrange
            var token = TokenGenerator.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync($"/newswebapi/news/all?filter={filter}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadFromJsonAsync<IEnumerable<OutputServiceNewsDTO>>();
            Assert.NotNull(responseData);
        }


        [Theory]
        [InlineData("C7665F11-30B6-49A3-A635-571186EB591C", true)]
        [InlineData("99999999-9999-9999-9999-999999999999", false)]
        public async Task UpdateServiceNews_ReturnsExpectedResult(Guid newsId, bool isSuccessExpected)
        {
            // Arrange
            var serviceNewsDto = new CreateServiceNewsDTO
            {
                Title = "Test News Update",
                Description = "This is a test news content",
                ShortDescription = "Test news",
                Skills = "Program"

            };

            var token = TokenGenerator.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PutAsJsonAsync($"/newswebapi/news/update/{newsId}", serviceNewsDto);

            // Assert
            if (isSuccessExpected)
            {
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadFromJsonAsync<OutputServiceNewsDTO>();
                Assert.NotNull(responseData);
            }
            else
            {

            }

        }
        [Theory]
        [InlineData("C7665F11-30B6-49A3-A635-571186EB591C", true)]
        [InlineData("99999999-9999-9999-9999-999999999999", false)]
        public async Task DeleteServiceNews_ReturnsExpectedResult(Guid newsId, bool isSuccessExpected)
        {
            // Arrange
            var token = TokenGenerator.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync($"/newswebapi/news/delete/{newsId}");

            // Assert
            if (isSuccessExpected)
            {
                response.EnsureSuccessStatusCode();

            }
            else
            {
                Assert.False(response.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetPendingApprovalNews_ReturnsOk_WhenAuthorized()
        {
            // Arrange
            var token = TokenGenerator.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/newswebapi/news/pending-approval");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadFromJsonAsync<IEnumerable<CreateServiceNewsDTO>>();
            Assert.NotNull(responseData);
        }
        
    }
}

