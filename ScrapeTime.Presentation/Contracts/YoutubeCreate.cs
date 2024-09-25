using System.Threading.Channels;
using ScrapeTime.Presentation.Services;

namespace ScrapeTime.Presentation.Contracts;

public class YoutubeCreate
{
    public string? Title { get; set; }
    public string? Channel { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Description { get; set; }
    public ulong? ViewCount { get; set; }
    public ulong? LikeCount { get; set; }
    public ulong? CommentCount { get; set; }
    public string? VideoUrl { get; set; }
    public double EngagementRate { get; set; }
    public string EstimatedAgeRange { get; set; }
}

public class CommenterInfo
{
    public string? ChannelId { get; set; }
    public string? ChannelTitle { get; set; }
    public DateTime? AccountOpeningDate { get; set; }
}