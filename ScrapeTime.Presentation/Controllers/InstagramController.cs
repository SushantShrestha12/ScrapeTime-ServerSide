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
    public class InstagramController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IInstagramService _instagramService;

        public InstagramController(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

        [HttpGet("proxyInstagramTopPostsByLocation")]
        public async Task<IActionResult> ProxyInstagramTopPostsByLocation(string location)
        {
            var externalApiUrl = $"https://scrapetime-881202084187.us-central1.run.app/api/Instagram/scrapeTopPostsByLocation?location={location}";
            try
            {
                var response = await _httpClient.GetAsync(externalApiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { error = $"External API returned an error: {response.StatusCode} - {errorMessage}" });
                }

                if (response.Content == null)
                {
                    return StatusCode(500, new { error = "The external API returned no content." });
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (HttpRequestException httpEx)
            {
                return StatusCode(500, new { error = $"HTTP Request error: {httpEx.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error proxying request: {ex.Message}" });
            }
        }
    }
}


// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Cors;
// using Microsoft.AspNetCore.Mvc;
// using ScrapeTime.Presentation.Services;
//
// namespace ScrapeTime.Presentation.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [AllowAnonymous]
//     [EnableCors("AllowAll")] 
//     public class InstagramController : ControllerBase
//     {
//         private readonly IInstagramService _instagramService;
//
//         public InstagramController(IInstagramService scrapingService)
//         {
//             _instagramService = scrapingService;
//         }
//
//         [HttpGet("scrapeTopPosts")]
//         public async Task<IActionResult> ScrapeTopPosts(string hashtag)
//         {
//             try
//             {
//                 var topPosts = await _instagramService.ScrapeInstagramTopPostsByHashtag(hashtag);
//                 return Ok(topPosts);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, $"Error scraping Instagram: {ex.Message}");
//             }
//         }
//
//         [HttpGet("scrapeTopPostsByLocation")]
//         public async Task<IActionResult> ScrapeTopPostsByLocation(string location)
//         {
//             try
//             {
//                 var topPosts = await _instagramService.ScrapeInstagramTopPosts(location);
//                 return Ok(topPosts);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, $"Error scraping Instagram: {ex.Message}");
//             }
//         }
//     }
// }
