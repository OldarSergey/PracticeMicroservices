using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using Shared;
using ArchiveMicroservice.Domain.Entities;

namespace ArchiveWebAPI.Tests.Controllers
{
    public class ArchiveControllerTests : IClassFixture<ArchiveWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ArchiveControllerTests(ArchiveWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }
        [Theory]
        [InlineData("C4725F3E-E024-46D9-B93F-4F6AD8BEC02A", true)]
        [InlineData("99999999-9999-9999-9999-999999999999", false)]
        public async Task GetUserArchives_ReturnsExpectedResult(Guid userId, bool isSuccessExpected)
        {
            // Arrange
            var token = TokenGenerator.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync($"/archivewebapi/archive/user/{userId}");

            // Assert
            if (isSuccessExpected)
            {
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadFromJsonAsync<IEnumerable<Archive>>();
                Assert.NotNull(responseData);
            }
            else
            {
                Assert.False(response.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }


    }
}
