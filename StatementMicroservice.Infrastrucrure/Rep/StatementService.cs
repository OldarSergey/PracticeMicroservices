using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewsMicroservice.Protos;
using Shared.Contracts.Kafka;
using Shared.CustomException;
using Shared.DTOs;
using StatementMicroservice.Application.Contracts;
using StatementMicroservice.Application.DTOs;
using StatementMicroservice.Application.DTOs.MessageKafka;
using StatementMicroservice.Domain.Entities;
using StatementMicroservice.Infrastrucrure.Data;
using System.Text.Json;
using UserMicroservice.Protos;

namespace StatementMicroservice.Infrastrucrure.Rep
{
    public class StatementService : IStatementService
    {
        private readonly StatementDbContext _statementDbContext;
        private readonly ILogger<StatementService> _logger;
        private readonly UserService.UserServiceClient _userClient;
        private readonly NewsService.NewsServiceClient _newsClient;
        private readonly IKafkaProducer<string, string> _kafkaProducer;

        public StatementService(
            StatementDbContext statementDbContext,
            ILogger<StatementService> logger,
            UserService.UserServiceClient userClient,
            NewsService.NewsServiceClient newsClient,
            IKafkaProducer<string, string> kafkaProducer)
        {
            _statementDbContext = statementDbContext;
            _logger = logger;
            _userClient = userClient;
            _newsClient = newsClient;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<OperationResponse> CreateStatementAsync(StatementDTO statementDTO)
        {
            try
            {
                var senderResponse = await _userClient.GetUserInfoAsync(new GetUserInfoRequest { UserId = statementDTO.SenderId.ToString() });
                if (senderResponse == null)
                    return new OperationResponse(false, "Sender not found");

                var receiverResponse = await _userClient.GetUserInfoAsync(new GetUserInfoRequest { UserId = statementDTO.ReceiverId.ToString() });
                if (receiverResponse == null)
                    return new OperationResponse(false, "Receiver not found");

                var receiverInfo = new UserDTO
                {
                    Id = Guid.Parse(receiverResponse.Id),
                    Email = receiverResponse.Email,
                };

                var newsResponse = await _newsClient.GetServiceNewsByIdAsync(new GetServiceNewsByIdRequest { NewsId = statementDTO.ServiceNewsId.ToString() });
                if (newsResponse == null)
                    return new OperationResponse(false, "Service news not found");

                var statement = new Statement
                {
                    SenderId = statementDTO.SenderId,
                    ReceiverId = statementDTO.ReceiverId,
                    ServiceNewsId = statementDTO.ServiceNewsId,
                    Status = StatementStatus.Pending,
                    IsSenderAgreed = true,
                    IsReceiverAgreed = false,
                    IsArchived = false
                };

                _statementDbContext.Statements.Add(statement);
                await _statementDbContext.SaveChangesAsync();

                var messageNotificationKafka = new NotificationDTO(receiverInfo.Email, "Notification", "Someone has responded to your news");
                await _kafkaProducer.ProduceAsync("send-notification", receiverInfo.Id.ToString(), JsonSerializer.Serialize(messageNotificationKafka));

                return new OperationResponse(true, "Statement created successfully", statement);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error while creating statement. Sender ID: {SenderId}, Receiver ID: {ReceiverId}", statementDTO.SenderId, statementDTO.ReceiverId);
                return new OperationResponse(false, "Database update error occurred while creating statement");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating statement. Sender ID: {SenderId}, Receiver ID: {ReceiverId}", statementDTO.SenderId, statementDTO.ReceiverId);
                return new OperationResponse(false, "An unexpected error occurred while creating statement");
            }
        }


        public async Task<List<Statement>> GetSentStatementsAsync(Guid userId)
        {
            try
            {
                var sentStatements = await _statementDbContext.Statements
                    .Where(s => s.SenderId == userId && !s.IsArchived && !s.IsDeleted)
                    .ToListAsync();

                return sentStatements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sent statements for user ID: {UserId}", userId);
                throw new DataRetrievalException("An error occurred while retrieving sent statements", ex);
            }
        }

        public async Task<List<Statement>> GetReceivedStatementsAsync(Guid userId)
        {
            try
            {
                var receivedStatements = await _statementDbContext.Statements
                    .Where(s => s.ReceiverId == userId && !s.IsArchived && !s.IsDeleted)
                    .ToListAsync();

                return receivedStatements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving received statements for user ID: {UserId}", userId);
                throw new DataRetrievalException("An error occurred while retrieving received statements", ex);
            }
        }

        public async Task<OperationResponse> ProcessStatementAsync(Guid userId, Guid statementId, bool isReceiverAgreed)
        {
            try
            {
                var statement = await _statementDbContext.Statements
                    .Where(st => !st.IsDeleted)
                    .FirstOrDefaultAsync(st => st.Id == statementId);

                if (statement == null)
                    return new OperationResponse(false, "Statement not found");

                if (userId != statement.ReceiverId)
                    return new OperationResponse(false, "You are not authorized to process this statement");

                statement.IsReceiverAgreed = isReceiverAgreed;

                if (statement.IsSenderAgreed && isReceiverAgreed)
                {
                    statement.Status = StatementStatus.Completed;
                    statement.IsArchived = true;
                    statement.IsDeleted = true;
                    var statementCompletedDTO = new StatementCompletedDTO(statement.Id, statement.ServiceNewsId, statement.SenderId, statement.ReceiverId);
                    await _kafkaProducer.ProduceAsync("statement-completed", statement.Id.ToString(), JsonSerializer.Serialize(statementCompletedDTO));
                }
                else
                {
                    statement.Status = StatementStatus.Rejected;
                    statement.IsArchived = true;
                    statement.IsDeleted = true;
                }

                _statementDbContext.Statements.Update(statement);
                await _statementDbContext.SaveChangesAsync();

                var message = statement.Status == StatementStatus.Completed ? "Statement processed and archived successfully" : "The statement was rejected";
                return new OperationResponse(true, message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error while processing statement ID: {StatementId} for user ID: {UserId}", statementId, userId);
                return new OperationResponse(false, "Database update error occurred while processing statement");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing statement ID: {StatementId} for user ID: {UserId}", statementId, userId);
                return new OperationResponse(false, "An unexpected error occurred while processing statement");
            }
        }



        public async Task<OperationResponse> CopyArchivedStatementAsync(Guid statementId)
        {
            try
            {
                var archivedStatement = await _statementDbContext.Statements
                    .Where(st => st.Id == statementId && st.IsArchived && st.IsDeleted)
                    .FirstOrDefaultAsync();

                if (archivedStatement == null)
                    return new OperationResponse(false, "Archived statement not found or already deleted");

                var newStatement = new Statement
                {
                    SenderId = archivedStatement.SenderId,
                    ReceiverId = archivedStatement.ReceiverId,
                    ServiceNewsId = archivedStatement.ServiceNewsId,
                    Status = StatementStatus.Pending,
                    IsSenderAgreed = false,
                    IsReceiverAgreed = false,
                    IsArchived = false,
                    IsDeleted = false,
                };

                _statementDbContext.Statements.Add(newStatement);
                await _statementDbContext.SaveChangesAsync();

                return new OperationResponse(true, "Statement copied successfully");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error while copying archived statement ID: {StatementId}", statementId);
                return new OperationResponse(false, "Database update error occurred while copying statement");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while copying archived statement ID: {StatementId}", statementId);
                return new OperationResponse(false, "An unexpected error occurred while copying statement");
            }
        }

        public async Task<OperationResponse> DeleteSentStatementAsync(Guid userId, Guid statementId)
        {
            try
            {
                var statement = await _statementDbContext.Statements
                    .Where(s => s.Id == statementId && !s.IsDeleted && s.SenderId == userId)
                    .FirstOrDefaultAsync();

                if (statement == null)
                    return new OperationResponse(false, "Statement not found");

                statement.IsDeleted = true;
                _statementDbContext.Statements.Update(statement);

                await _statementDbContext.SaveChangesAsync();

                return new OperationResponse(true, "Statement deleted successfully");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error while deleting sent statement ID: {StatementId} for user ID: {UserId}", statementId, userId);
                return new OperationResponse(false, "Database update error occurred while deleting statement");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting sent statement ID: {StatementId} for user ID: {UserId}", statementId, userId);
                return new OperationResponse(false, "An unexpected error occurred while deleting statement");
            }
        }
        public async Task DeleteSentStatementByUserIdAsync(Guid userid)
        {
            await _statementDbContext.Statements
                  .Where(st => st.SenderId == userid)
                  .ExecuteUpdateAsync(setters => setters
                      .SetProperty(st => st.IsDeleted, true));
        }
    }
}
