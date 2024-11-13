using ArchiveMicroservice.Application.Contracts;
using ArchiveMicroservice.Application.DTOs.MessageKafka;
using ArchiveMicroservice.Domain.Entities;
using ArchiveMicroservice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.CustomException;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveMicroservice.Infrastructure.Rep
{
    public class ArchiveService : IArchiveService
    {
        private readonly ArchiveDbContext _archiveDbContext;
        private readonly ILogger<ArchiveService> _logger;

        public ArchiveService(ArchiveDbContext appDbContext, ILogger<ArchiveService> logger)
        {
            _archiveDbContext = appDbContext;
            _logger = logger;
        }

        public async Task<List<Archive>> GetUserArchivesAsync(Guid userId)
        {
            try
            {
                var archives = await _archiveDbContext.Archives
                    .Where(a => a.UserId == userId && !a.IsDeleted)
                    .ToListAsync();
                return archives;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error while retrieving archives for user ID: {UserId}", userId);
                throw new DataRetrievalException("Database update error occurred while retrieving archives", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving archives for user ID: {UserId}", userId);
                throw new DataRetrievalException("An unexpected error occurred while retrieving archives", ex);
            }
        }

        public async Task<OperationResponse> DeleteArchiveAsync(Guid archiveId, Guid userId)
        {
            try
            {
                var archive = await _archiveDbContext.Archives
                    .FirstOrDefaultAsync(a => a.Id == archiveId && a.UserId == userId);

                if (archive == null)
                    return new OperationResponse(false, "Archive not found");

                archive.IsDeleted = true;

                _archiveDbContext.Archives.Update(archive);
                await _archiveDbContext.SaveChangesAsync();

                return new OperationResponse(true, "Archive deleted successfully");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error while deleting archive ID: {ArchiveId}", archiveId);
                return new OperationResponse(false, "Database update error occurred while deleting archive");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting archive ID: {ArchiveId}", archiveId);
                return new OperationResponse(false, "An unexpected error occurred while deleting archive");
            }
        }
        public async Task CreateArchivesForStatementAsync(StatementCompletedDTO statementCompletedDTO)
        {
            try
            {
                var senderArchive = new Archive
                {
                    IsDeleted = false,
                    UserId = statementCompletedDTO.SenderId,
                    StatementId = statementCompletedDTO.StatementId
                };
                _archiveDbContext.Archives.Add(senderArchive);

                var receiverArchive = new Archive
                {
                    IsDeleted = false,
                    UserId = statementCompletedDTO.ReceiverId,
                    StatementId = statementCompletedDTO.StatementId
                };
                _archiveDbContext.Archives.Add(receiverArchive);

                await _archiveDbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error while creating archives for statement ID: {StatementId}", statementCompletedDTO.StatementId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating archives for statement ID: {StatementId}", statementCompletedDTO.StatementId);
                throw;
            }
        }

        public async Task DeleteArchiveByUserIdAsync(Guid userId)
        {
            await _archiveDbContext.Archives
                .Where(a => a.UserId == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.IsDeleted, true));
        }

    }
}
