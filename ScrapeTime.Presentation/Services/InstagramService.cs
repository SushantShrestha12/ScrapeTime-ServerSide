using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ScrapeTime.Presentation.Contracts;

namespace ScrapeTime.Presentation.Services
{
    public interface IInstagramService : IDisposable
    {
        Task<List<string>> ScrapeInstagramTopPostsByHashtag(string location);
        Task<List<InstagramCreate>> ScrapeInstagramTopPosts(string hashtag);
    }

    public class InstagramService : IInstagramService
    {
        private readonly ChromeDriver _driver;
        private const string JsonFilePath = "wwwroot/locationInfo.json"; 

        public InstagramService()
        {
            var options = new ChromeOptions();
            
            var proxy = new Proxy
            {
                HttpProxy = "198.49.68.80:80"
            };
            
            options.Proxy = proxy;
            
           //options.AddArgument("headless"); 
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-software-rasterizer");
            options.AddArgument("--disable-web-security"); 
            options.AddArgument("--allow-running-insecure-content");
            options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            options.AddArgument("--user-data-dir=/tmp/chrome_dev"); 
            
            _driver = new ChromeDriver(options);
        }
        
        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
        public Task<List<string>> ScrapeInstagramTopPostsByHashtag(string hashtag)
        {
            var url = $"https://www.instagram.com/explore/tags/{hashtag}/";
            _driver.Navigate().GoToUrl(url);
            Thread.Sleep(3000);

            for (var i = 0; i < 3; i++)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                Thread.Sleep(2000); 
            }

            // Find all post links
            var postElements = _driver.FindElements(By.CssSelector("article div div div div a"));
            var postLinks = postElements.Select(element => element.GetAttribute("href"))
                .Where(postLink => !string.IsNullOrEmpty(postLink))
                .ToList();

            return Task.FromResult(postLinks);
        }

        public async Task<List<InstagramCreate>> ScrapeInstagramTopPosts(string location)
        {
            var locationId = GetLocationIdFromJson(location) ?? throw new Exception($"Location '{location}' not found in JSON.");
        
            await NavigateToInstagramLocation(locationId, location);
            ScrollPage(3);
        
            var postLinks = _driver.FindElements(By.CssSelector("article div div div div a"))
                                   .Select(element => element.GetAttribute("href"))
                                   .Where(link => !string.IsNullOrEmpty(link))
                                   .Take(5)
                                   .ToList();
        
            return await GetInstagramPosts(postLinks);
        }
        private static string? GetLocationIdFromJson(string locationName)
        {
            var json = File.ReadAllText(JsonFilePath);
            var locations = JsonConvert.DeserializeObject<List<LocationInfo>>(json);

            return locations?.SelectMany(country => country.Cities)
                             .SelectMany(city => city.Locations)
                             .FirstOrDefault(l => l.LocationName.Equals(locationName, StringComparison.OrdinalIgnoreCase))
                             ?.LocationId;
        }
        private async Task NavigateToInstagramLocation(string locationId, string location)
        {
            LoginToInstagram();
            await _driver.Navigate().GoToUrlAsync($"https://www.instagram.com/explore/locations/{locationId}/{location}/");
            await Task.Delay(3000);
        }
        private void ScrollPage(int times)
        {
            for (var i = 0; i < times; i++)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                Thread.Sleep(2000);
            }
        }
        private async Task<List<InstagramCreate>> GetInstagramPosts(IEnumerable<string> postLinks)
         {
             var instagramPosts = new List<InstagramCreate>();
         
             foreach (var postLink in postLinks)
             {
                 await _driver.Navigate().GoToUrlAsync(postLink);
                 await Task.Delay(2000);

                 if (!TryClickLikesButton()) continue;
                 await Task.Delay(1500);
                 var usernameElements = _driver.FindElements(By.XPath("/html/body//div[6]//div[2]//div[2]//span/span")).Take(10);
                     
                 var genderCounts = await CountGendersFromUsernames(usernameElements);
                 var age = await PredictAgeFromAccount();
                 instagramPosts.Add(new InstagramCreate
                 {
                     Url = postLink,
                     MaleCount = genderCounts[0],
                     FemaleCount = genderCounts[1],
                     Age = age
                 });
             }
             return instagramPosts;
        }
        private bool TryClickLikesButton()
        {
            try
            {
                _driver.FindElement(By.CssSelector("section div span a span span")).Click();
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
        private async Task<List<int>> CountGendersFromUsernames(IEnumerable<IWebElement> usernameElements)
        {
            var genderCounts = new List<int> { 0, 0 };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-API-KEY", "7277777ebabc7183f1fa2ad6a73fbfe7");

            foreach (var usernameElement in usernameElements)
            {
                var gender = await GetGenderFromApi(httpClient, usernameElement.Text);
                switch (gender)
                {
                    case "male":
                        genderCounts[0]++;
                        break;
                    case "female":
                        genderCounts[1]++;
                        break;
                }
            }

            return genderCounts;
        }
        private static async Task<string?> GetGenderFromApi(HttpClient httpClient, string username)
        {
            var response = await httpClient.GetAsync($"https://v2.namsor.com/NamSorAPIv2/api2/json/gender/{username}");
            if (!response.IsSuccessStatusCode) return null;

            var genderData = JsonConvert.DeserializeObject<NamSorGenderResponse>(await response.Content.ReadAsStringAsync());
            return genderData?.LikelyGender.ToLower();
        }
        private async Task<string> PredictAgeFromAccount()
        {
            var accountAges = new List<int>();
            var currentYear = DateTime.Now.Year;

            var users = _driver.FindElements(By.CssSelector("div.xf1ldfh a > img")).Take(10).ToList();

            for (var i = 0; i < users.Count; i++)
            {
                try
                {
                    var currentUsers = _driver.FindElements(By.CssSelector("div.xf1ldfh a > img")).ToList();
                    if (i >= currentUsers.Count)
                    {
                        continue;
                    }

                    var user = currentUsers[i];
                    user.Click();
                    await Task.Delay(1000);

                    var optionsButton = _driver.FindElement(By.CssSelector("section.x1xdureb svg > circle:nth-child(2)"));
                    optionsButton.Click();
                    await Task.Delay(1000);

                    var aboutThisAccountButton = _driver.FindElement(By.CssSelector("div.x1lytzrv button:nth-child(5)"));
                    aboutThisAccountButton.Click();
                    await Task.Delay(1000);

                    var joinedDateElement = _driver.FindElement(By.CssSelector("div > div > div > div > div > div:nth-child(2) > div:nth-child(1) > div:nth-child(1) > div > div:nth-child(2) > span:nth-child(2)"));
                    var joinedDateText = joinedDateElement.Text;

                    if (DateTime.TryParse(joinedDateText, out var joinedDate))
                    {
                        var accountAge = currentYear - joinedDate.Year;
                        accountAges.Add(accountAge); 
                    }
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine("Unable to find some elements on the profile.");
                }
                catch (StaleElementReferenceException)
                {
                    Console.WriteLine("Element became stale, skipping this user.");
                    continue;
                }
                finally
                {
                    await _driver.Navigate().BackAsync();
                    await Task.Delay(2000);

                    try
                    {
                        var likesButton = _driver.FindElement(By.CssSelector("section div span a span span"));
                        likesButton.Click();
                        await Task.Delay(2000); 
                    }
                    catch (NoSuchElementException)
                    {
                        Console.WriteLine("Likes button not found after navigating back.");
                    }
                    catch (StaleElementReferenceException)
                    {
                        Console.WriteLine("Likes button became stale after navigating back, attempting to re-locate.");
                        var likesButton = _driver.FindElement(By.CssSelector("section div span a span span"));
                        likesButton.Click();
                        await Task.Delay(2000); 
                    }
                }
            }

            if (accountAges.Count != 0)
            {
                var averageAges = (from t in accountAges 
                    let minAge = t + 10
                    let maxAge = t + 30
                    select (minAge + maxAge) / 2).ToList();

                var overallAverageAge = (int)averageAges.Average();

                return GetAgeCategory(overallAverageAge);
            }
            else
            {
                return "No age data available";
            }
        }
        private static string GetAgeCategory(int averageAge)
        {
            return averageAge switch
            {
                <= 10 => "0-10",
                <= 20 => "10-20",
                <= 30 => "20-30",
                <= 40 => "30-40",
                <= 50 => "40-50",
                <= 60 => "50-60",
                <= 70 => "60-70",
                <= 80 => "70-80",
                <= 90 => "80-90",
                _ => "90-100"
            };
        }
        private void LoginToInstagram()
        {
            _driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
            Thread.Sleep(3000);

            // _driver.FindElement(By.Name("username")).SendKeys("ZapCode10");
            // _driver.FindElement(By.Name("password")).SendKeys("Password110");
            
            _driver.FindElement(By.Name("username")).SendKeys("Scrape620");
            _driver.FindElement(By.Name("password")).SendKeys("$cr@p3/19");
            _driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(5000);
        }
    }
}