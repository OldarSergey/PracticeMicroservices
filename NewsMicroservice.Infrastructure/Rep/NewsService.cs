using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewsMicroservice.Application.Contracts;
using NewsMicroservice.Application.DTOs;
using NewsMicroservice.Application.DTOs.MessageKafka;
using NewsMicroservice.Domain.Entities;
using NewsMicroservice.Infrastructure.Data;
using Shared.Contracts.Kafka;
using Shared.CustomException;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NewsMicroservice.Infrastructure.Rep
{
    public class NewsService : INewsService
    {
        private readonly NewsDbContext _newsDbContext;
        private readonly ILogger<NewsService> _logger;
        private readonly IKafkaProducer<string, string> _kafkaProducer;

        public NewsService(NewsDbContext appDbContext, ILogger<NewsService> logger, IKafkaProducer<string, string> kafkaProducer)
        {
            _newsDbContext = appDbContext;
            _logger = logger;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<OperationResponse> CreateServiceNewsAsync(Guid userId, CreateServiceNewsDTO serviceNews)
        {
            try
            {
                var newServiceNews = new ServiceNews
                {
                    Title = serviceNews.Title,
                    Description = serviceNews.Description,
                    ShortDescription = serviceNews.ShortDescription,
                    Skills = serviceNews.Skills,
                    UserId = userId,
                    Date = DateTime.UtcNow
                };

                _newsDbContext.ServiceNews.Add(newServiceNews);
                await _newsDbContext.SaveChangesAsync();

                return new OperationResponse(true, "Service news created successfully", newServiceNews);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error while creating service news. User ID: {UserId}", userId);
                return new OperationResponse(false, "Database update error occurred while creating service news");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating service news. User ID: {UserId}", userId);
                return new OperationResponse(false, "An unexpected error occurred while creating service news");
            }
        }

        public async Task<List<OutputServiceNewsDTO>> GetAllServiceNewsByFilter(string filter, NewsSortOption sortBy = NewsSortOption.Date, bool descending = false)
        {
            try
            {
                var query = _newsDbContext.ServiceNews.AsQueryable();

                if (!string.IsNullOrEmpty(filter))
                {
                    query = query.Where(sn => sn.Title.Contains(filter) || sn.Skills.Contains(filter));
                }

                switch (sortBy)
                {
                    case NewsSortOption.Title:
                        query = descending ? query.OrderByDescending(sn => sn.Title) : query.OrderBy(sn => sn.Title);
                        break;
                    case NewsSortOption.Date:
                    default:
                        query = descending ? query.OrderByDescending(sn => sn.Date) : query.OrderBy(sn => sn.Date);
                        break;
                }

                var filteredNews = await query
                    .Where(sn => sn.IsApproved && !sn.IsDeleted)
                    .Select(sn => new OutputServiceNewsDTO
                    {
                        Id = sn.Id,
                        Title = sn.Title,
                        Description = sn.Description,
                        ShortDescription = sn.ShortDescription,
                        Skills = sn.Skills,
                        AuthorId = sn.UserId,
                        Date = sn.Date
                    })
                    .ToListAsync();

                return filteredNews;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service news by filter. Filter: {Filter}", filter);
                throw new DataRetrievalException("An error occurred while retrieving service news", ex);
            }
        }



        public async Task<List<OutputServiceNewsDTO>> GetUserServiceNewsAsync(Guid userId)
        {
            try
            {
                var serviceNewsList = await _newsDbContext.ServiceNews
                    .Where(sn => sn.UserId == userId && !sn.IsDeleted && sn.IsApproved)
                    .Select(sn => new OutputServiceNewsDTO
                    {
                        Id = sn.Id,
                        Title = sn.Title,
                        Description = sn.Description,
                        ShortDescription = sn.ShortDescription,
                        Skills = sn.Skills,
                        AuthorId = sn.UserId,
                        Date = sn.Date
                    })
                    .ToListAsync();

                return serviceNewsList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user service news. User ID: {UserId}", userId);
                throw new DataRetrievalException("An error occurred while retrieving user service news", ex);
            }
        }

        public async Task<OperationResponse> UpdateServiceNewsAsync(Guid userId, Guid newsId, CreateServiceNewsDTO serviceNews)
        {
            try
            {
                var news = await _newsDbContext.ServiceNews
                    .FirstOrDefaultAsync(sn => sn.Id == newsId && !sn.IsDeleted && sn.UserId == userId && sn.IsApproved);

                if (news == null)
                {
                    _logger.LogWarning("Service news not found for update. User ID: {UserId}, News ID: {NewsId}", userId, newsId);
                    return new OperationResponse(false, "Service news not found");
                }

                news.Title = serviceNews.Title;
                news.Description = serviceNews.Description;
                news.ShortDescription = serviceNews.ShortDescription;
                news.Skills = serviceNews.Skills;
                news.Date = DateTime.UtcNow;
                _newsDbContext.ServiceNews.Update(news);
                await _newsDbContext.SaveChangesAsync();

                return new OperationResponse(true, "Service news updated successfully", news);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating service news. User ID: {UserId}, News ID: {NewsId}", userId, newsId);
                return new OperationResponse(false, "An unexpected error occurred while updating service news");
            }
        }

        public async Task<List<OutputServiceNewsDTO>> GetPendingApprovalNewsAsync()
        {
            try
            {
                return await _newsDbContext.ServiceNews
                    .Where(sn => !sn.IsApproved && !sn.IsDeleted)
                    .Select(sn => new OutputServiceNewsDTO
                    {
                        Id = sn.Id,
                        Title = sn.Title,
                        Description = sn.Description,
                        ShortDescription = sn.ShortDescription,
                        AuthorId = sn.UserId,
                        Skills = sn.Skills,
                        Date = sn.Date
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending approval news.");
                throw new DataRetrievalException("An error occurred while retrieving pending approval news", ex);
            }
        }

        public async Task<OutputServiceNewsDTO> GetServiceNewsByIdAsync(Guid newsId)
        {
            try
            {
                var news = await _newsDbContext.ServiceNews
                .Where(n => n.Id == newsId && !n.IsDeleted)
                .Select(n => new OutputServiceNewsDTO
                {
                    Id = n.Id,
                    Title = n.Title,
                    Description = n.Description,
                    ShortDescription = n.ShortDescription,
                    AuthorId = n.UserId,
                    Skills = n.Skills,
                    Date = n.Date
                })
                .FirstOrDefaultAsync();
                return news;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending approval news.");
                throw new DataRetrievalException("An error occurred while retrieving pending approval news", ex);
            }

        }

        public async Task<OperationResponse> ApprovePendingNewsAsync(Guid newsId, bool isApproved = true)
        {
            try
            {
                var news = await _newsDbContext.ServiceNews
                    .FirstOrDefaultAsync(sn => sn.Id == newsId && !sn.IsDeleted && !sn.IsApproved);
                if (news == null)
                    return new OperationResponse(false, "News not found");

                news.IsApproved = isApproved;
                news.IsDeleted = !isApproved;

                _newsDbContext.ServiceNews.Update(news);
                await _newsDbContext.SaveChangesAsync();
                var message = isApproved ? "News approved successfully" : "News deleted successfully";
                var newsApprovalStatusDto = new NewsApprovalStatusDTO(news.UserId, news.Id, message);
                await _kafkaProducer.ProduceAsync("status-approve-news", newsId.ToString(), JsonSerializer.Serialize(newsApprovalStatusDto));
                return new OperationResponse(true, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving or deleting news with ID: {NewsId}", newsId);
                return new OperationResponse(false, "An error occurred while updating the news status");
            }
        }


        public async Task<OperationResponse> DeleteServiceNewsAsync(Guid userId, Guid newsId)
        {
            try
            {
                var news = await _newsDbContext.ServiceNews
                    .FirstOrDefaultAsync(sn => sn.Id == newsId && !sn.IsDeleted && sn.UserId == userId && sn.IsApproved);

                if (news == null)
                {
                    _logger.LogWarning("Service news not found for deletion. User ID: {UserId}, News ID: {NewsId}", userId, newsId);
                    return new OperationResponse(false, "Service news not found");
                }

                news.IsDeleted = true;

                _newsDbContext.ServiceNews.Update(news);
                await _newsDbContext.SaveChangesAsync();

                return new OperationResponse(true, "Service news deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting service news. User ID: {UserId}, News ID: {NewsId}", userId, newsId);
                return new OperationResponse(false, "An unexpected error occurred while deleting service news");
            }
        }
        public async Task DeleteServiceNewsByUserIdAsync(Guid userId)
        {
            await _newsDbContext.ServiceNews
                .Where(sn => sn.UserId == userId)
                .ExecuteUpdateAsync(setters => setters
                   .SetProperty(a => a.IsDeleted, true));
        }
    }
}
