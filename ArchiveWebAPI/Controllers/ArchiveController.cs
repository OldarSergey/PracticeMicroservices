using ArchiveMicroservice.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArchiveWebAPI.Controllers
{
    [Route("archivewebapi/[controller]")]
    [ApiController]
    [Authorize]
    public class ArchiveController : ControllerBase
    {
        private readonly IArchiveService _archiveService;

        public ArchiveController(IArchiveService archiveService)
        {
            _archiveService = archiveService;
        }


        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserArchives(Guid userId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || Guid.Parse(userIdClaim) != userId)
            {
                return Unauthorized("You are not authorized to access these archives.");
            }

            var archives = await _archiveService.GetUserArchivesAsync(userId);
            return Ok(archives);
        }
        [HttpDelete("delete/{archiveId}")]
        public async Task<IActionResult> DeleteArchive(Guid archiveId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
                return Unauthorized("You are not authorized to access these archives.");

            var response = await _archiveService.DeleteArchiveAsync(archiveId, Guid.Parse(userIdClaim));

            if (response.Flag)
                return Ok(response.Message);

            return NotFound(response.Message);
        }
    }
}

