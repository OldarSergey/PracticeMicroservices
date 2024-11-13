using ArchiveMicroservice.Application.DTOs.MessageKafka;
using ArchiveMicroservice.Domain.Entities;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveMicroservice.Application.Contracts
{
    public interface IArchiveService
    {
        Task<List<Archive>> GetUserArchivesAsync(Guid userId);
        Task<OperationResponse> DeleteArchiveAsync(Guid archiveId, Guid userId);
        Task CreateArchivesForStatementAsync(StatementCompletedDTO statementCompletedDTO);
        Task DeleteArchiveByUserIdAsync(Guid userId);
    }
}
