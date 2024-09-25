using Microsoft.AspNetCore.Mvc;
using ScrapeTime.Presentation.Services;

namespace ScrapeTime.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstagramController : ControllerBase
    {
        private readonly IInstagramService _instagramService;

        public InstagramController(IInstagramService scrapingService)
        {
            _instagramService = scrapingService;
        }

        [HttpGet("scrapeTopPosts")]
        public async Task<IActionResult> ScrapeTopPosts(string hashtag)
        {
            try
            {
                var topPosts = await _instagramService.ScrapeInstagramTopPostsByHashtag(hashtag);
                return Ok(topPosts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error scraping Instagram: {ex.Message}");
            }
        }

        [HttpGet("scrapeTopPostsByLocation")]
        public async Task<IActionResult> ScrapeTopPostsByLocation(string location)
        {
            try
            {
                var topPosts = await _instagramService.ScrapeInstagramTopPosts(location);
                return Ok(topPosts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error scraping Instagram: {ex.Message}");
            }
        }
    }
}
