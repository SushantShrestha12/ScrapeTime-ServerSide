using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ScrapeTime.Presentation.Contracts;
using SeleniumExtras.WaitHelpers;

namespace ScrapeTime.Presentation.Services;

public interface ITikTokService
{
    Task<List<TikTokCreate>> GetMostLikedVideoAsync(string countryCode);
}

public class TikTokService : ITikTokService
{ 
    private readonly ChromeDriver _driver;

    public TikTokService()
    {
        var options = GetChromeOptions();
        _driver = new ChromeDriver(options);
    }

    private static ChromeOptions GetChromeOptions()
    {
        var options = new ChromeOptions(); 
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        return options;
    }

    private static WebDriverWait GetWebDriverWait(IWebDriver driver)
    {
        return new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    public async Task<List<TikTokCreate>> GetMostLikedVideoAsync(string country)
    {
        await _driver.Navigate().RefreshAsync();
        
        var url = $"https://www.tiktok.com/discover/{country}-trending?lang=en";
       
        await _driver.Navigate().GoToUrlAsync(url);

        var wait = GetWebDriverWait(_driver);

        // Wait for the first video thumbnail to load
        wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@id='app']/div[2]/div[2]/div[2]/div/div[*]/div[1]/div[2]")));

        var videoElements = _driver.FindElements(By.XPath("//*[@id='app']/div[2]/div[2]/div[2]/div/div[*]/div[1]/div[2]")).Take(5);
        var videoData = new List<TikTokCreate>();

        var i = 0;
        foreach (var videoElement in videoElements)
        {
            try
            {
                var viewElement = videoElement.FindElements(By.XPath(
                    "//*[@id='app']/div[2]/div[2]/div[2]/div/div[*]/div[1]/div[1]/div/div/a/div/div[2]/strong"));
                var view = viewElement[i].Text;
                videoElement.Click();
                CloseCaptcha(wait);

                var like = ExtractElementText(wait,
                    By.XPath(
                        "//*[@id='app']/div[2]/div[4]/div/div[2]/div[1]/div/div[1]/div[2]/div/div[1]/div[1]/button[1]/strong"));
                var comment = ExtractElementText(wait,
                    By.XPath(
                        "//*[@id='app']/div[2]/div[4]/div/div[2]/div[1]/div/div[1]/div[2]/div/div[1]/div[1]/button[2]/strong"));
                var videoUrl = ExtractElementText(wait,
                    By.XPath("//*[@id='app']/div[2]/div[4]/div/div[2]/div[1]/div/div[1]/div[2]/div/div[2]/p"));

                var engagementRate = CalculateEngagementRate(like, comment, view);

                videoData.Add(new TikTokCreate
                {
                    Url = videoUrl,
                    Like = like,
                    Comment = comment,
                    Views = view,
                    EngagementRate = engagementRate
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing video: {ex.Message}");
            }
            

            i++;
            var closePopup = _driver.FindElement(By.XPath("//*[@id='app']/div[2]/div[4]/div/div[1]/button[1]"));
            closePopup.Click();
        }
        
        _driver.Quit();
        return videoData;
    }

    private void CloseCaptcha(WebDriverWait wait)
    {
        if (ElementExists(wait, By.XPath("//*[@id='verify-bar-close']")))
        {
            var closeCaptcha = _driver.FindElement(By.XPath("//*[@id='verify-bar-close']"));
            closeCaptcha.Click();
        }
    }
    public static async Task<string> GetTopTrendingTagAsync()
    {
        const string url = "https://ads.tiktok.com/business/creativecenter/inspiration/popular/hashtag/pc/en";
        using var client = new HttpClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var pageContent = await response.Content.ReadAsStringAsync();

        var document = new HtmlDocument();
        document.LoadHtml(pageContent);

        var tagNode = document.DocumentNode.SelectSingleNode("//*[@id='hashtagItemContainer']/div[@class='CardPc_detail__Y92if']/div/span");

        return tagNode?.InnerText ?? throw new Exception("Trending tag not found.");
    }

    private static double ParseLikesOrComments(string count)
    {
        if (count.EndsWith($"K"))
            return double.Parse(count.Replace("K", "")) * 1_000;
        else if (count.EndsWith($"M"))
            return double.Parse(count.Replace("M", "")) * 1_000_000;
        else
            return double.Parse(count); 
    }


    private static string ExtractElementText(WebDriverWait wait, By by)
    {
        var element = wait.Until(drv => drv.FindElement(by));
        return element.Text.Trim();
    }

    private static bool ElementExists(WebDriverWait wait, By by)
    {
        try
        {
            wait.Until(ExpectedConditions.ElementExists(by));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static double CalculateEngagementRate(string like, string comment, string view)
    {
        var likes = ParseLikesOrComments(like);
        var comments = ParseLikesOrComments(comment);
        var views = ParseLikesOrComments(view);
        
        var engagementRate = ((likes + comments) / views) * 100;

        return engagementRate;
    }

}