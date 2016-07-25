using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Threading.Tasks;
using YoutubeExtractor;
using Google.Apis.Services;
using System.IO;

namespace YoutubeAlarm
{
    public static class YTHelper
    {
        private static string _apiKey = "AIzaSyDbo9Mx7Vap6AgC17D6Jswy7jAtcVwf3jw";
        private static string _appName = "YoutubeAlarmClock";
        private static string _videoUrlTemplate { get { return "youtube.com/watch?v="; } }
        private static YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = _apiKey,
            ApplicationName = _appName
        });

        public static async Task<Dictionary<string, string>> Search(string keyword, int maxResults)
        {
            var request = youtubeService.Search.List("snippet");
            request.Q = keyword;
            request.MaxResults = maxResults;

            SearchListResponse response = await request.ExecuteAsync();

            Dictionary<string, string> videos = new Dictionary<string, string>();

            foreach (var result in response.Items)
            {
                if (result.Id.Kind == "youtube#video")
                    videos.Add(result.Snippet.Title, result.Id.VideoId);
            }

            return videos;
        }


        public static async Task<bool> DownloadAudio(string videoId, string path)
        {
            string videoLink = _videoUrlTemplate + videoId;
            await Task.Run(() =>
            {
                IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(videoLink);
                VideoInfo videoInfo = videoInfos
                .Where(info => info.CanExtractAudio)
                .OrderByDescending(info => info.AudioBitrate)
                .First();

                if (videoInfo.RequiresDecryption)
                {
                    DownloadUrlResolver.DecryptDownloadUrl(videoInfo);
                }
                 var audioDownloader = new AudioDownloader(videoInfo, Path.Combine(path, videoInfo.Title + videoInfo.AudioExtension));
                audioDownloader.Execute();
            });

            return true;
        }


    }
}