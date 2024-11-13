using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsMicroservice.Application.Contracts;
using NewsMicroservice.Application.DTOs;
using NewsMicroservice.Domain.Entities;
using Shared.CustomException;
using System.Security.Claims;

namespace NewsWebAPI.Controllers
{
    [ApiController]
    [Route("newswebapi/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;

        public NewsController(INewsService newsService)
        {
            _newsService = newsService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateServiceNews(Guid userId, [FromBody] CreateServiceNewsDTO serviceNewsDto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || Guid.Parse(userIdClaim) != userId)
                return Unauthorized("You are not authorized to create a news item from another user");

            var result = await _newsService.CreateServiceNewsAsync(userId, serviceNewsDto);
            return Ok(result);
        }


        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserServiceNews(Guid userId)
        {
            try
            {
                var newsList = await _newsService.GetUserServiceNewsAsync(userId);
                return Ok(newsList);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllServiceNewsByFilter([FromQuery] string filter = "", [FromQuery] NewsSortOption sortBy = NewsSortOption.Date, [FromQuery] bool descending = false)
        {
            try
            {
                var newsList = await _newsService.GetAllServiceNewsByFilter(filter, sortBy, descending);
                return Ok(newsList);
            }
            catch (DataRetrievalException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }


        [HttpGet("get/{newsId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServiceNewsById(Guid newsId)
        {
            var news = await _newsService.GetServiceNewsByIdAsync(newsId);

            if (news == null)
                return NotFound("Service news not found.");

            return Ok(news);
        }

        [HttpPut("update/{newsId}")]
        public async Task<IActionResult> UpdateServiceNews(Guid newsId, [FromBody] CreateServiceNewsDTO serviceNews)
        {
            if (serviceNews == null)
                return BadRequest("Invalid news data.");

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return BadRequest();

            var response = await _newsService.UpdateServiceNewsAsync(Guid.Parse(userId), newsId, serviceNews);

            if (response.Flag)
                return Ok(response);

            if (response.Message == "Service news not found")
                return NotFound(response.Message);

            else return Unauthorized(response.Message);
        }

        [HttpDelete("delete/{newsId}")]
        public async Task<IActionResult> DeleteServiceNews(Guid newsId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return BadRequest();

            var response = await _newsService.DeleteServiceNewsAsync(Guid.Parse(userId), newsId);
            if (response.Flag)
                return Ok(response);

            if (response.Message == "Service news not found")
                return NotFound(response.Message);

            else return Unauthorized(response.Message);
        }
        [HttpGet("pending-approval")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingApprovalNews()
        {
            var news = await _newsService.GetPendingApprovalNewsAsync();
            return Ok(news);
        }

        [HttpPost("approve/{newsId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApprovePendingNews(Guid newsId, bool isApproved = true)
        {
            var response = await _newsService.ApprovePendingNewsAsync(newsId, isApproved);
            if (!response.Flag && response.Message == "News not found")
                return NotFound(response.Message);

            if (!response.Flag)
                return BadRequest(response.Message);

            return Ok(response.Message);
        }
    }
}
