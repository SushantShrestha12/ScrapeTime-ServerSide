using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ScrapeTime.Presentation.Services;

namespace ScrapeTime.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    [EnableCors("AllowAll")] 
    public class YouTubeController : ControllerBase
    {
        private readonly IYoutubeService _youtubeService;
        
        public YouTubeController(IYoutubeService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingYoutubeVideo(string countryCode)
        {
            try
            {
                var topChannels = await _youtubeService.GetTrendingVideoAsync(countryCode);
                return Ok(topChannels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error scraping Instagram: {ex.Message}");

            }
        }
    }
}
