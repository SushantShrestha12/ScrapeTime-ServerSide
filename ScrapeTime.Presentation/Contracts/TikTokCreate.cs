using System.Collections.ObjectModel;
using OpenQA.Selenium;

namespace ScrapeTime.Presentation.Contracts;

public class TikTokCreate
{
    public string? Url { get; set; }
    public string? Like { get; set; }
    public string? Comment { get; set; }
    public string? Views { get; set; }
    public double? EngagementRate { get; set; }
}