using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ScrapeTime.Presentation.Services;

namespace ScrapeTime.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    [EnableCors("AllowAll")] 
    public class TikTokController : ControllerBase
    {
        private readonly ITikTokService _tikTokService;

        public TikTokController(ITikTokService tikTokService)
        {
            _tikTokService = tikTokService;
        }

        [HttpGet("trending/{countryCode}")]
        public async Task<IActionResult> GetMostLikedVideo(string countryCode)
        {
            try
            {
                var videoData = await _tikTokService.GetMostLikedVideoAsync(countryCode);

                return Ok(videoData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("tiktok-tag")]
        public async Task<IActionResult> GetTopTrendingTag()
        {
            try
            {
                var trendingTag = await TikTokService.GetTopTrendingTagAsync();
                return Ok(trendingTag);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
