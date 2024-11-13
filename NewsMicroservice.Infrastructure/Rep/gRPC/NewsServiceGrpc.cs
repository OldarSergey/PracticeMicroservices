using Grpc.Core;
using NewsMicroservice.Application.Contracts;
using NewsMicroservice.Protos;

namespace NewsMicroservice.Infrastructure.Rep.gRPC
{
    public class NewsServiceGrpc : NewsMicroservice.Protos.NewsService.NewsServiceBase
    {
        private readonly INewsService _newsService;

        public NewsServiceGrpc(INewsService newsService)
        {
            _newsService = newsService;
        }

        public override async Task<GetServiceNewsByIdResponse> GetServiceNewsById(GetServiceNewsByIdRequest request, ServerCallContext context)
        {
            var news = await _newsService.GetServiceNewsByIdAsync(Guid.Parse(request.NewsId));

            if (news == null)
            {
                return new GetServiceNewsByIdResponse
                {
                    Success = false,
                    Message = "Service news not found"
                };
            }

            return new GetServiceNewsByIdResponse
            {
                Id = news.Id.ToString(),
                Title = news.Title,
                Description = news.Description,
                ShortDescription = news.ShortDescription,
                AuthorId = news.AuthorId.ToString(),
                Skills = news.Skills,
                Date = news.Date.ToString("o"),
                Success = true
            };
        }
    }
}
