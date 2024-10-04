using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ScrapeTime.Presentation.Services;

namespace ScrapeTime.Presentation.Controllers
{
    [Route("[controller]")]
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

        [HttpGet("trending")]
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
        
        [HttpGet("proxyMostLikedVideo")]
        public async Task<IActionResult> ProxyGetMostLikedVideo(string countryCode)
        {
            try
            {
                var backendUrl = $"https://scrapetime-881202084187.us-central1.run.app/tiktok/trending?countryCode={countryCode}";

                using var client = new HttpClient();
                var response = await client.GetAsync(backendUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // Optionally, you can deserialize the content if needed (e.g., if it returns JSON)
                    // var videoData = JsonConvert.DeserializeObject<YourExpectedModel>(content);

                    return Ok(content);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, $"Error fetching most liked video: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching most liked video: {ex.Message}");
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
