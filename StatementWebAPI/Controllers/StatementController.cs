using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StatementMicroservice.Application.Contracts;
using StatementMicroservice.Application.DTOs;
using System.Security.Claims;

namespace StatementWebAPI.Controllers
{
    [ApiController]
    [Route("statementwebapi/[controller]")]
    [Authorize]
    public class StatementController : ControllerBase
    {
        private readonly IStatementService _statementService;

        public StatementController(IStatementService statementRepo)
        {
            _statementService = statementRepo;
        }

        /// <summary>
        /// The method of creating the application
        /// </summary>
        /// <param name="statementDTO"></param>
        /// <returns></returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateStatement([FromBody] StatementDTO statementDTO)
        {
            if (statementDTO == null)
                return BadRequest("StatementDTO cannot be null.");

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || Guid.Parse(userIdClaim) != statementDTO.SenderId)
                return Unauthorized("Invalid values");

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (userId == statementDTO.ReceiverId)
                return BadRequest("You cannot create a statement for yourself.");

            var response = await _statementService.CreateStatementAsync(statementDTO);
            var notFoundErrors = new[] { "Sender not found", "Receiver not found", "Service news not found" };
            if (!response.Flag && notFoundErrors.Contains(response.Message))
            {
                return NotFound(response.Message);
            }

            return Ok(response);
        }

        /// <summary>
        /// Display all applications submitted by the user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>

        [HttpGet("sent/{userId}")]
        public async Task<IActionResult> GetSentStatements(Guid userId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || Guid.Parse(userIdClaim) != userId)
                return Unauthorized("You don't have access to other users' requests");

            var sentStatements = await _statementService.GetSentStatementsAsync(userId);
            return Ok(sentStatements);
        }

        /// <summary>
        /// Display all incoming user requests
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("received/{userId}")]
        public async Task<IActionResult> GetReceivedStatements(Guid userId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || Guid.Parse(userIdClaim) != userId)
                return Unauthorized("You don't have access to other users' requests");

            var receivedStatements = await _statementService.GetReceivedStatementsAsync(userId);
            return Ok(receivedStatements);
        }

        /// <summary>
        /// Application processing
        /// </summary>
        /// <param name="statementId"> Application number</param>
        /// <param name="processStatementDTO">I agree/disagree</param>
        /// <returns></returns>
        [HttpPost("process/{statementId}")]
        public async Task<IActionResult> ProcessStatement(Guid statementId, [FromBody] ProcessStatementDTO processStatementDTO)
        {
            if (processStatementDTO == null)
                return BadRequest("ProcessStatementDTO cannot be null.");

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User not authenticated");

            var response = await _statementService.ProcessStatementAsync(Guid.Parse(userIdClaim), statementId, processStatementDTO.IsReceiverAgreed);

            if (!response.Flag)
            {
                if (response.Message == "Statement not found" || response.Message == "You are not authorized to process this statement")
                    return NotFound(response.Message);

                return BadRequest(response.Message);
            }

            return Ok(response);
        }

        /// <summary>
        /// Copy the application from the archive
        /// </summary>
        /// <param name="statementId"></param>
        /// <returns></returns>
        [HttpPost("copy/{statementId}")]
        public async Task<IActionResult> CopyArchivedStatement(Guid statementId)
        {
            var response = await _statementService.CopyArchivedStatementAsync(statementId);
            if (!response.Flag)
                return NotFound(response.Message);

            return Ok(response);
        }

        /// <summary>
        /// Deleting an application
        /// </summary>
        /// <param name="statementId">You can only delete submitted applications</param>
        /// <returns></returns>
        [HttpDelete("delete/{statementId}")]
        public async Task<IActionResult> DeleteStatement(Guid statementId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
                return BadRequest("User not authenticated");

            var userId = Guid.Parse(userIdClaim);

            var response = await _statementService.DeleteSentStatementAsync(userId, statementId);

            if (!response.Flag)
            {
                if (response.Message == "Statement not found")
                    return NotFound(response.Message);

                if (response.Message == "You are not authorized to delete this statement")
                    return Unauthorized(response.Message);

                return BadRequest(response.Message);
            }

            return Ok(response);
        }


    }
}
