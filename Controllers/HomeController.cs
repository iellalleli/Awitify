using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using KaraokeApp.Models;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace KaraokeApp.Controllers
{
    public class QueueItem
    {
        public string? VideoId { get; set; }
        public string? Title { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private const string QUEUE_KEY = "VideoQueue";
        private const string API_KEY = "AIzaSyBatiTvFUGqf-uTj5gp2DnfJtEeNCXBDeQ";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Search(string query)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = API_KEY
            });

            string modifiedQuery = $"{query} karaoke";
            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = modifiedQuery;
            searchRequest.MaxResults = 10;
            searchRequest.Type = "video";

            var searchResponse = await searchRequest.ExecuteAsync();

            var videos = searchResponse.Items
                .Where(item => item.Id.Kind == "youtube#video")
                .Select(item => new
                {
                    Title = item.Snippet.Title,
                    VideoId = item.Id.VideoId
                })
                .ToList();

            ViewBag.Videos = videos;

            return View("HomePage");
        }

        public async Task<IActionResult> HomePage(string query)
        {
            ViewBag.RecommendedVideos = await GetRecommendedVideos();

            if (!string.IsNullOrEmpty(query))
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = API_KEY
                });

                var searchRequest = youtubeService.Search.List("snippet");
                searchRequest.Q = query + " karaoke";
                searchRequest.MaxResults = 10;
                searchRequest.Type = "video";

                var searchResponse = await searchRequest.ExecuteAsync();
                ViewBag.Videos = searchResponse.Items
                    .Where(item => item.Id.Kind == "youtube#video")
                    .Select(item => new
                    {
                        Title = item.Snippet.Title,
                        VideoId = item.Id.VideoId
                    })
                    .ToList();
            }

            return View();
        }

        private async Task<List<VideoItem>> GetRecommendedVideos()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = API_KEY
            });

            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = "top karaoke songs";
            searchRequest.MaxResults = 6;
            searchRequest.Type = "video";

            var searchResponse = await searchRequest.ExecuteAsync();
            return searchResponse.Items
                .Where(item => item.Id.Kind == "youtube#video")
                .Select(item => new VideoItem
                {
                    Title = item.Snippet.Title,
                    VideoId = item.Id.VideoId
                })
                .ToList();
        }

        public IActionResult Play(string videoId)
        {
            // TempData["ReturnUrl"] = Request.Headers["Referer"].ToString();

            if (string.IsNullOrEmpty(videoId))
            {
                return RedirectToAction("HomePage");
            }

            var queue = HttpContext.Session.Get<List<QueueItem>>(QUEUE_KEY) ?? new List<QueueItem>();
            _logger.LogInformation($"Current queue count in Play action: {queue.Count}");

            ViewBag.VideoId = videoId;
            ViewBag.Queue = queue;

            return View();
        }

        [HttpPost]
        public IActionResult AddToQueue(string videoId, string title)
        {
            try
            {
                if (string.IsNullOrEmpty(videoId) || string.IsNullOrEmpty(title))
                {
                    _logger.LogWarning("AddToQueue called with invalid parameters. VideoId: {VideoId}, Title: {Title}", videoId, title);
                    return Json(new { success = false, message = "Invalid video information" });
                }

                var queue = HttpContext.Session.Get<List<QueueItem>>(QUEUE_KEY) ?? new List<QueueItem>();

                if (queue.Count >= 10)
                {
                    return Json(new { success = false, message = "Queue is full (maximum 10 items)" });
                }

                if (queue.Any(q => q.VideoId == videoId))
                {
                    return Json(new { success = false, message = "This video is already in the queue" });
                }

                var newItem = new QueueItem
                {
                    VideoId = videoId,
                    Title = title
                };

                queue.Add(newItem);
                HttpContext.Session.Set(QUEUE_KEY, queue);

                _logger.LogInformation("Added video to queue. VideoId: {VideoId}, Title: {Title}, Queue Count: {Count}",
                    videoId, title, queue.Count);

                return Json(new { success = true, queue = queue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to queue. VideoId: {VideoId}, Title: {Title}", videoId, title);
                return Json(new { success = false, message = "An error occurred while adding to queue" });
            }
        }

        [HttpPost]
        public IActionResult GetQueue()
        {
            try
            {
                var queue = HttpContext.Session.Get<List<QueueItem>>(QUEUE_KEY) ?? new List<QueueItem>();
                _logger.LogInformation("Retrieved queue with {Count} items", queue.Count);
                return Json(queue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving queue");
                return Json(new List<QueueItem>());
            }
        }

        [HttpPost]
        public IActionResult PlayNow(string videoId)
        {
            try
            {
                var queue = HttpContext.Session.Get<List<QueueItem>>(QUEUE_KEY) ?? new List<QueueItem>();
                queue.RemoveAll(q => q.VideoId == videoId);
                HttpContext.Session.Set(QUEUE_KEY, queue);

                return Json(new { success = true, videoId = videoId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PlayNow");
                return Json(new { success = false, message = "Failed to play video" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SearchOnPlay(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return BadRequest("Search query is required.");

                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = API_KEY
                });

                var searchRequest = youtubeService.Search.List("snippet");
                searchRequest.Q = $"{query} karaoke";
                searchRequest.MaxResults = 10;

                var searchResponse = await searchRequest.ExecuteAsync();

                var videos = searchResponse.Items
                    .Where(item => item.Id.Kind == "youtube#video")
                    .Select(item => new
                    {
                        Title = item.Snippet.Title,
                        VideoId = item.Id.VideoId
                    })
                    .ToList();

                return PartialView("SearchResults", videos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in SearchOnPlay");
                return StatusCode(500, "An internal error occurred. Please try again later.");
            }
        }

        [HttpPost]
        public IActionResult RemoveFromQueue(string videoId)
        {
            var queue = HttpContext.Session.Get<List<QueueItem>>(QUEUE_KEY) ?? new List<QueueItem>();

            // Remove the video from the queue
            queue.RemoveAll(q => q.VideoId == videoId);

            // Update the session
            HttpContext.Session.Set(QUEUE_KEY, queue);

            return Json(new { success = true, queue = queue });
        }

        private async Task<dynamic> GetVideoDetails(string videoId)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = API_KEY
            });

            var videoRequest = youtubeService.Videos.List("snippet");
            videoRequest.Id = videoId;

            var videoResponse = await videoRequest.ExecuteAsync();
            var video = videoResponse.Items.FirstOrDefault();

            return new
            {
                Title = video?.Snippet.Title,
                VideoId = videoId
            };
        }
    }
}
