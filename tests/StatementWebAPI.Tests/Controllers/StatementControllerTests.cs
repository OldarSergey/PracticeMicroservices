using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using Shared;
using StatementMicroservice.Application.DTOs;

namespace StatementWebAPI.Tests.Controllers
{
    public class StatementControllerTests : IClassFixture<StatementWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public StatementControllerTests(StatementWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateStatement_ReturnsOk_WhenValid()
        {
            // Arrange
            var statement = new StatementDTO
            {
                Id = Guid.NewGuid(),
                SenderId = Guid.Parse("C4725F3E-E024-46D9-B93F-4F6AD8BEC02A"),
                ReceiverId = Guid.Parse("0592A7D8-1009-452E-9680-27F6525C90A6"),
                ServiceNewsId = Guid.Parse("C7665F11-30B6-49A3-A635-571186EB591C"),
            };
            var token = TokenGenerator.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/statementwebapi/statement/create", statement);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadFromJsonAsync<object>();
            Assert.NotNull(responseData);
        }

        [Theory]
        [InlineData("C4725F3E-E024-46D9-B93F-4F6AD8BEC02A", true)]
        [InlineData("99999999-9999-9999-9999-999999999999", false)]
        public async Task GetSentStatements_ReturnsExpectedResult(Guid userId, bool isSuccessExpected)
        {
            // Arrange
            var token = TokenGenerator.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync($"/statementwebapi/statement/sent/{userId}");

            // Assert
            if (isSuccessExpected)
            {
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadFromJsonAsync<IEnumerable<StatementDTO>>();
                Assert.NotNull(responseData);
                Assert.NotEmpty(responseData);
            }
            else
            {
                Assert.False(response.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [Theory]
        [InlineData("C4725F3E-E024-46D9-B93F-4F6AD8BEC02A", true)]
        [InlineData("99999999-9999-9999-9999-999999999999", false)]
        public async Task GetReceivedStatements_ReturnsExpectedResult(Guid userId, bool isSuccessExpected)
        {
            // Arrange
            var token = TokenGenerator.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync($"/statementwebapi/statement/received/{userId}");

            // Assert
            if (isSuccessExpected)
            {
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadFromJsonAsync<IEnumerable<StatementDTO>>();
                Assert.NotNull(responseData);
                Assert.NotEmpty(responseData);
            }
            else
            {
                Assert.False(response.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        

        [Theory]
        [InlineData("8845AB1A-E45D-4C25-8470-3E673E5B3C68", true)]
        [InlineData("99999999-9999-9999-9999-999999999999", false)]
        public async Task CopyArchivedStatement_ReturnsExpectedResult(Guid statementId, bool isSuccessExpected)
        {
            // Arrange
            var token = TokenGenerator.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsync($"/statementwebapi/statement/copy/{statementId}", null);

            // Assert
            if (isSuccessExpected)
            {
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadFromJsonAsync<object>();
                Assert.NotNull(responseData);
            }
            else
            {
                Assert.False(response.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Theory]
        [InlineData("2D0DF9DA-0491-4CD0-9446-557D91465AC2", true)]
        [InlineData("99999999-9999-9999-9999-999999999999", false)]
        public async Task DeleteStatement_ReturnsExpectedResult(Guid statementId, bool isSuccessExpected)
        {
            // Arrange
            var token = TokenGenerator.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync($"/statementwebapi/statement/delete/{statementId}");

            // Assert
            if (isSuccessExpected)
            {
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadFromJsonAsync<object>();
                Assert.NotNull(responseData);
            }
            else
            {
                Assert.False(response.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }
    }
}
