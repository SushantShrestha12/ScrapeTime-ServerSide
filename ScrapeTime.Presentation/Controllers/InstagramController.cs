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

        public InstagramController(HttpClient httpClient, IInstagramService instagramService)
        {
            _httpClient = httpClient;
            _instagramService = instagramService;
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
            try
            {
                var url = $"https://scrapetime-881202084187.us-central1.run.app/api/Instagram/scrapeTopPostsByLocation?location={Uri.EscapeDataString(location)}";

                using var client = new HttpClient();
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // Optionally, you can deserialize the content if needed (e.g., if it returns JSON)
                    // var result = JsonConvert.DeserializeObject<YourExpectedModel>(content);
                    return Ok(content);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, $"Error fetching top posts: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching top posts: {ex.Message}");
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
