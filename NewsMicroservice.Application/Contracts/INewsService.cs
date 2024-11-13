using NewsMicroservice.Application.DTOs;
using NewsMicroservice.Domain.Entities;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsMicroservice.Application.Contracts
{
    public interface INewsService
    {
        Task<OperationResponse> CreateServiceNewsAsync(Guid userId, CreateServiceNewsDTO serviceNews);
        Task<List<OutputServiceNewsDTO>> GetUserServiceNewsAsync(Guid userId);
        Task<List<OutputServiceNewsDTO>> GetAllServiceNewsByFilter(string filter, NewsSortOption sortBy = NewsSortOption.Date, bool descending = false);
        Task<OperationResponse> UpdateServiceNewsAsync(Guid userId, Guid newsId, CreateServiceNewsDTO serviceNews);
        Task<OperationResponse> DeleteServiceNewsAsync(Guid userId, Guid newsId);
        Task DeleteServiceNewsByUserIdAsync(Guid userId);
        Task<List<OutputServiceNewsDTO>> GetPendingApprovalNewsAsync();
        Task<OperationResponse> ApprovePendingNewsAsync(Guid newsId, bool isApproved = true);
        Task<OutputServiceNewsDTO> GetServiceNewsByIdAsync(Guid newsId);
    }
}
