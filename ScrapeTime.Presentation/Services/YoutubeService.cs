using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using ScrapeTime.Presentation.Contracts;

namespace ScrapeTime.Presentation.Services
{
    public interface IYoutubeService
    {
        Task<List<YoutubeCreate>> GetTrendingVideoAsync(string countryCode);
    }

    public class YoutubeService : IYoutubeService
    {
        private readonly YouTubeService _youtubeService;

        public YoutubeService()
        {
            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyAMQlI2Be7GqNp1zDvOFEF67QJ9J41L9r8", 
                ApplicationName = GetType().ToString()
            });
        }

        public async Task<List<YoutubeCreate>> GetTrendingVideoAsync(string countryCode)
        {
            var youtubeChannel = new List<YoutubeCreate>();

            try
            {
                var searchListRequest = _youtubeService.Videos.List("snippet,contentDetails,statistics");
                searchListRequest.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
                searchListRequest.RegionCode = countryCode;
                searchListRequest.MaxResults = 10;

                var searchListResponse = await searchListRequest.ExecuteAsync();

                foreach (var video in searchListResponse.Items)
                {
                    var youtubeCreate = new YoutubeCreate
                    {
                        Title = video.Snippet.Title,
                        Channel = video.Snippet.ChannelTitle,
                        PublishedAt = video.Snippet.PublishedAt,
                        Description = video.Snippet.Description,
                        ViewCount = video.Statistics.ViewCount,
                        LikeCount = video.Statistics.LikeCount,
                        CommentCount = video.Statistics.CommentCount,
                        EngagementRate = CalculateEngagementRate(
                            video.Statistics.LikeCount,
                            video.Statistics.CommentCount,
                            video.Statistics.ViewCount),
                        VideoUrl = $"https://www.youtube.com/watch?v={video.Id}",
                        EstimatedAgeRange = ""
                    };

                    // Fetch comments for the video
                    var comments = await GetCommentsAsync(video.Id);

                    // Estimate the most common age range
                    var estimatedAgeRange = EstimateMostCommonAgeRange(comments);
                    youtubeCreate.EstimatedAgeRange = estimatedAgeRange;

                    youtubeChannel.Add(youtubeCreate);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Internal server error: {ex.Message}");
            }

            return youtubeChannel;
        }

        private static double CalculateEngagementRate(ulong? likes, ulong? comments, ulong? views)
        {
            if (views is null or 0)
            {
                return 0;
            }

            var totalLikes = likes ?? 0;
            var totalComments = comments ?? 0;

            var engagementRate = ((double)(totalLikes + totalComments) / (double)views) * 100;
            return Math.Round(engagementRate, 2);
        }

        private async Task<List<CommenterInfo>> GetCommentsAsync(string videoId)
        {
            var commenterInfoList = new List<CommenterInfo>();

            var commentRequest = _youtubeService.CommentThreads.List("snippet");
            commentRequest.VideoId = videoId;
            commentRequest.MaxResults = 20; // Increased to get more data
            commentRequest.TextFormat = CommentThreadsResource.ListRequest.TextFormatEnum.PlainText;

            var commentResponse = await commentRequest.ExecuteAsync();

            foreach (var commentThread in commentResponse.Items)
            {
                var commenterChannelId = commentThread.Snippet.TopLevelComment.Snippet.AuthorChannelId?.Value;

                if (string.IsNullOrEmpty(commenterChannelId)) continue;
                var channelInfo = await GetChannelInfoAsync(commenterChannelId);
                commenterInfoList.Add(channelInfo);
            }

            return commenterInfoList;
        }

        private async Task<CommenterInfo> GetChannelInfoAsync(string channelId)
        {
            var channelRequest = _youtubeService.Channels.List("snippet");
            channelRequest.Id = channelId;

            var channelResponse = await channelRequest.ExecuteAsync();

            var channel = channelResponse.Items.FirstOrDefault();

            if (channel != null)
            {
                return new CommenterInfo
                {
                    ChannelId = channelId,
                    ChannelTitle = channel.Snippet.Title,
                    AccountOpeningDate = channel.Snippet.PublishedAt
                };
            }

            return null;
        }

        private static string EstimateMostCommonAgeRange(List<CommenterInfo> commenters)
        {
            var ageRanges = new Dictionary<string, int>
            {
                { "13-17", 0 },
                { "18-24", 0 },
                { "25-34", 0 },
                { "35-44", 0 },
                { "45-54", 0 },
                { "55-64", 0 },
                { "65+", 0 }
            };

            var currentYear = DateTime.Now.Year;

            foreach (var commenter in commenters)
            {
                if (!commenter.AccountOpeningDate.HasValue) continue;
                var accountCreationYear = commenter.AccountOpeningDate.Value.Year;
                var accountAge = currentYear - accountCreationYear;
                
                var estimatedAge = 16 + accountAge;

                var ageRange = GetAgeRange(estimatedAge);
                if (ageRanges.ContainsKey(ageRange))
                {
                    ageRanges[ageRange]++;
                }
            }

            var mostCommonAgeRange = ageRanges.OrderByDescending(ar => ar.Value).FirstOrDefault().Key;

            return mostCommonAgeRange;
        }

        private static string GetAgeRange(int estimatedAge)
        {
            return estimatedAge switch
            {
                >= 13 and <= 17 => "13-17",
                >= 18 and <= 24 => "18-24",
                >= 25 and <= 34 => "25-34",
                >= 35 and <= 44 => "35-44",
                >= 45 and <= 54 => "45-54",
                >= 55 and <= 64 => "55-64",
                >= 65 => "65+",
                _ => "Unknown"
            };
        }
    }
}


// using Google.Apis.Auth.OAuth2;
// using Google.Apis.Services;
// using Google.Apis.YouTube.v3;
// using ScrapeTime.Presentation.Contracts;
//
// namespace ScrapeTime.Presentation.Services
// {
//     public interface IYoutubeService
//     {
//         Task<List<YoutubeCreate>> GetTrendingVideoAsync(string countryCode);
//     }
//
//     public class YoutubeService : IYoutubeService
//     {
//         private readonly YouTubeService _youtubeService;
//
//         public YoutubeService()
//         {
//             var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
//                 new ClientSecrets
//                 {
//                     ClientId = "42240397428-7o9rleloe2vs3a5b8d4m76cbdn5g83h2.apps.googleusercontent.com", 
//                     ClientSecret = "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" 
//                 },
//                 new[] { YouTubeService.Scope.YoutubeForceSsl },
//                 "sthasushant1@gmail.com",
//                 CancellationToken.None
//             ).Result;
//
//             _youtubeService = new YouTubeService(new BaseClientService.Initializer()
//             {
//                 HttpClientInitializer = credential,
//                 ApplicationName = GetType().ToString()
//             });
//         }
//
//         public async Task<List<YoutubeCreate>> GetTrendingVideoAsync(string countryCode)
//         {
//             var youtubeChannel = new List<YoutubeCreate>();
//
//             try
//             {
//                 var searchListRequest = _youtubeService.Videos.List("snippet,contentDetails,statistics");
//                 searchListRequest.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
//                 searchListRequest.RegionCode = countryCode;
//                 searchListRequest.MaxResults = 10;
//
//                 var searchListResponse = await searchListRequest.ExecuteAsync();
//
//                 foreach (var video in searchListResponse.Items)
//                 {
//                     var youtubeCreate = new YoutubeCreate
//                     {
//                         Title = video.Snippet.Title,
//                         Channel = video.Snippet.ChannelTitle,
//                         PublishedAt = video.Snippet.PublishedAt,
//                         Description = video.Snippet.Description,
//                         ViewCount = video.Statistics.ViewCount,
//                         LikeCount = video.Statistics.LikeCount,
//                         CommentCount = video.Statistics.CommentCount,
//                         EngagementRate = CalculateEngagementRate(
//                             video.Statistics.LikeCount,
//                             video.Statistics.CommentCount,
//                             video.Statistics.ViewCount),
//                         VideoUrl = $"https://www.youtube.com/watch?v={video.Id}",
//                         CommenterAccountOpeningDates = new List<CommenterInfo>()
//                     };
//
//                     // Fetch comments for the video
//                     var comments = await GetCommentsAsync(video.Id);
//                     youtubeCreate.CommenterAccountOpeningDates.AddRange(comments);
//
//                     youtubeChannel.Add(youtubeCreate);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 throw new Exception($"Internal server error: {ex.Message}");
//             }
//
//             return youtubeChannel;
//         }
//
//         private static double CalculateEngagementRate(ulong? likes, ulong? comments, ulong? views)
//         {
//             if (views is null or 0)
//             {
//                 return 0;
//             }
//
//             var totalLikes = likes ?? 0;
//             var totalComments = comments ?? 0;
//
//             var engagementRate = ((double)(totalLikes + totalComments) / (double)views) * 100;
//             return Math.Round(engagementRate, 2);
//         }
//
//         private async Task<List<CommenterInfo>> GetCommentsAsync(string videoId)
//         {
//             var commenterInfoList = new List<CommenterInfo>();
//
//             var commentRequest = _youtubeService.CommentThreads.List("snippet");
//             commentRequest.VideoId = videoId;
//             commentRequest.MaxResults = 20; // Adjust as needed
//             commentRequest.TextFormat = CommentThreadsResource.ListRequest.TextFormatEnum.PlainText;
//
//             var commentResponse = await commentRequest.ExecuteAsync();
//
//             foreach (var commentThread in commentResponse.Items)
//             {
//                 var commenterChannelId = commentThread.Snippet.TopLevelComment.Snippet.AuthorChannelId?.Value;
//
//                 if (string.IsNullOrEmpty(commenterChannelId)) continue;
//                 var channelInfo = await GetChannelInfoAsync(commenterChannelId);
//                 commenterInfoList.Add(channelInfo);
//             }
//
//             return commenterInfoList;
//         }
//
//         private async Task<CommenterInfo> GetChannelInfoAsync(string channelId)
//         {
//             var channelRequest = _youtubeService.Channels.List("snippet");
//             channelRequest.Id = channelId;
//
//             var channelResponse = await channelRequest.ExecuteAsync();
//
//             var channel = channelResponse.Items.FirstOrDefault();
//
//             if (channel != null)
//             {
//                 return new CommenterInfo
//                 {
//                     ChannelId = channelId,
//                     ChannelTitle = channel.Snippet.Title,
//                     AccountOpeningDate = channel.Snippet.PublishedAt
//                 };
//             }
//
//             return null;
//         }
//     }
// }


// using Google.Apis.Services;
// using Google.Apis.YouTube.v3;
// using ScrapeTime.Presentation.Contracts;
//
// namespace ScrapeTime.Presentation.Services;
//
// public interface IYoutubeService
// {
//     Task<List<YoutubeCreate>> GetTrendingVideo(string countryCode);
// }
//
// public class YoutubeService : IYoutubeService
// {
//
//     private readonly YouTubeService _youtubeService;
//     
//     public YoutubeService()
//     {
//         _youtubeService = new YouTubeService(new BaseClientService.Initializer()
//         {
//             ApiKey = "AIzaSyAMQlI2Be7GqNp1zDvOFEF67QJ9J41L9r8",
//             ApplicationName = this.GetType().ToString()
//         });
//     }
//
//     public async Task<List<YoutubeCreate>> GetTrendingVideo(string countryCode)
//     {
//         var youtubeChannel = new List<YoutubeCreate>();
//         try
//         {
//             var searchListRequest = _youtubeService.Videos.List("snippet,contentDetails,statistics");
//             searchListRequest.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
//             searchListRequest.RegionCode = countryCode;
//             searchListRequest.MaxResults = 10;
//
//             var searchListResponse = await searchListRequest.ExecuteAsync();
//             
//             if (searchListResponse.Items.Count > 0)
//             {
//                 youtubeChannel.AddRange(searchListResponse.Items.Select(video => new YoutubeCreate
//                 {
//                     Title = video.Snippet.Title,
//                     Channel = video.Snippet.ChannelTitle,
//                     PublishedAt = video.Snippet.PublishedAt,
//                     Description = video.Snippet.Description,
//                     ViewCount = video.Statistics.ViewCount,
//                     LikeCount = video.Statistics.LikeCount,
//                     CommentCount = video.Statistics.CommentCount,
//                     EngagementRate = CalculateEngagementRate(video.Statistics.LikeCount, video.Statistics.CommentCount,
//                         video.Statistics.ViewCount),
//                     VideoUrl = $"https://www.youtube.com/watch?v={video.Id}"
//                 }));
//             }
//         }
//         catch (Exception ex)
//         {
//             throw new Exception($"Internal server error: {ex.Message}");
//         }
//
//         return youtubeChannel;
//     }
//
//     private static double CalculateEngagementRate(ulong? likes, ulong? comments, ulong? views)
//     {
//         if (views is null or 0)
//         {
//             return 0;
//         }
//
//         var totalLikes = likes ?? 0;
//         var totalComments = comments ?? 0;
//
//         var engagementRate = ((double)(totalLikes + totalComments) / (double)views) * 100;
//         return Math.Round(engagementRate, 2);
//     }
// }