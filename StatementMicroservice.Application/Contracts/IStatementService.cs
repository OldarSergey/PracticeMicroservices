using Shared.DTOs;
using StatementMicroservice.Application.DTOs;
using StatementMicroservice.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatementMicroservice.Application.Contracts
{
    public interface IStatementService
    {
        Task<OperationResponse> CreateStatementAsync(StatementDTO statementDTO);

        Task<List<Statement>> GetSentStatementsAsync(Guid userId);

        Task<List<Statement>> GetReceivedStatementsAsync(Guid userId);

        Task<OperationResponse> ProcessStatementAsync(Guid userId, Guid statementId, bool isReceiverAgreed);
        Task<OperationResponse> CopyArchivedStatementAsync(Guid statementId);
        Task<OperationResponse> DeleteSentStatementAsync(Guid userid, Guid statementId);
        Task DeleteSentStatementByUserIdAsync(Guid userid);
    }
}
