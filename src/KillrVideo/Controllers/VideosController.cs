﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using KillrVideo.ActionFilters;
using KillrVideo.ActionResults;
using KillrVideo.Authentication;
using KillrVideo.Data;
using KillrVideo.Data.Upload;
using KillrVideo.Data.Upload.Dtos;
using KillrVideo.Data.Videos;
using KillrVideo.Data.Videos.Dtos;
using KillrVideo.Models.Videos;

namespace KillrVideo.Controllers
{
    /// <summary>
    /// Controller for viewing videos.
    /// </summary>
    public class VideosController : ConventionControllerBase
    {
        private readonly IVideoReadModel _videoReadModel;
        private readonly IVideoWriteModel _videoWriteModel;
        private readonly IUploadedVideosReadModel _uploadReadModel;

        public VideosController(IVideoReadModel videoReadModel, IVideoWriteModel videoWriteModel, IUploadedVideosReadModel uploadReadModel)
        {
            if (videoReadModel == null) throw new ArgumentNullException("videoReadModel");
            if (videoWriteModel == null) throw new ArgumentNullException("videoWriteModel");
            if (uploadReadModel == null) throw new ArgumentNullException("uploadReadModel");

            _videoReadModel = videoReadModel;
            _videoWriteModel = videoWriteModel;
            _uploadReadModel = uploadReadModel;
        }

        /// <summary>
        /// Shows the View for viewing a specific video.
        /// </summary>
        [HttpGet]
        public async Task<ViewResult> ViewVideo(Guid videoId)
        {
            // Try to find the video by id
            VideoDetails videoDetails = await _videoReadModel.GetVideo(videoId);
            if (videoDetails != null)
            {
                // Found the video, display it
                return View(new ViewVideoViewModel
                {
                    VideoId = videoId,
                    Title = videoDetails.Name,
                    Description = videoDetails.Description,
                    LocationType = videoDetails.LocationType,
                    Location = videoDetails.Location,
                    Tags = videoDetails.Tags,
                    UploadDate = videoDetails.AddedDate,
                    InProgress = false
                });
            }

            // The video might currently be processing (i.e. if it was just uploaded), so try and find it
            UploadedVideo uploadDetails = await _uploadReadModel.GetByVideoId(videoId);
            if (uploadDetails == null)
                throw new ArgumentException(string.Format("Could not find a video with id {0}", videoId));

            return View(new ViewVideoViewModel
            {
                VideoId = videoId,
                Title = uploadDetails.Name,
                Description = uploadDetails.Description,
                UploadDate = uploadDetails.AddedDate,
                LocationType = VideoLocationType.Upload,
                Tags = uploadDetails.Tags,
                InProgress = true,
                InProgressJobId = uploadDetails.JobId
            });
        }

        /// <summary>
        /// Shows the View for adding a new video.
        /// </summary>
        [HttpGet, Authorize]
        public ActionResult Add()
        {
            return View();
        }
        
        /// <summary>
        /// Gets related videos for the video specified in the model.
        /// </summary>
        [HttpPost]
        public async Task<JsonNetResult> Related(GetRelatedVideosViewModel model)
        {
            RelatedVideos relatedVideos = await _videoReadModel.GetRelatedVideos(model.VideoId);
            return JsonSuccess(new RelatedVideosViewModel
            {
                Videos = relatedVideos.Videos.Select(VideoPreviewViewModel.FromDataModel).ToList()
            });
        }

        /// <summary>
        /// Gets videos for the specified user or the currently logged in user if one is not specified.
        /// </summary>
        [HttpPost]
        public async Task<JsonNetResult> ByUser(GetUserVideosViewModel model)
        {
            // If no user was specified, default to the current logged in user
            Guid? userId = model.UserId ?? User.GetCurrentUserId();
            if (userId == null)
            {
                ModelState.AddModelError(string.Empty, "No user specified and no user currently logged in.");
                return JsonFailure();
            }

            UserVideos userVideos = await _videoReadModel.GetUserVideos(new GetUserVideos
            {
                UserId = userId.Value,
                PageSize = model.PageSize,
                FirstVideoOnPageAddedDate = model.FirstVideoOnPage == null ? (DateTimeOffset?) null : model.FirstVideoOnPage.AddedDate,
                FirstVideoOnPageVideoId = model.FirstVideoOnPage == null ? (Guid?) null : model.FirstVideoOnPage.VideoId
            });

            return JsonSuccess(new UserVideosViewModel
            {
                UserId = userVideos.UserId,
                Videos = userVideos.Videos.Select(VideoPreviewViewModel.FromDataModel).ToList()
            });
        }

        /// <summary>
        /// Gets the most recent videos.
        /// </summary>
        [HttpPost]
        public async Task<JsonNetResult> Recent(GetRecentVideosViewModel model)
        {
            LatestVideos recentVideos = await _videoReadModel.GetLastestVideos(model.PageSize);
            return JsonSuccess(new RecentVideosViewModel
            {
                Videos = recentVideos.Videos.Select(VideoPreviewViewModel.FromDataModel).ToList()
            });
        }

        /// <summary>
        /// Get the ratings data for a video.
        /// </summary>
        [HttpGet, NoCache]
        public async Task<JsonNetResult> GetRatings(GetRatingsViewModel model)
        {
            // We definitely want the overall rating info, so start there
            Task<VideoRating> ratingTask = _videoReadModel.GetRating(model.VideoId);

            // If a user is logged in, we also want their rating
            Guid? userId = User.GetCurrentUserId();
            Task<UserVideoRating> userRatingTask = null;
            if (userId.HasValue)
                userRatingTask = _videoReadModel.GetRatingFromUser(model.VideoId, userId.Value);

            // Await data appropriately
            VideoRating ratingData = await ratingTask;
            UserVideoRating userRating = null;
            if (userRatingTask != null)
                userRating = await userRatingTask;

            return JsonSuccess(new RatingsViewModel
            {
                VideoId = ratingData.VideoId,
                CurrentUserLoggedIn = userId.HasValue,
                CurrentUserRating = userRating == null ? 0 : userRating.Rating,
                RatingsCount = ratingData.RatingsCount,
                RatingsSum = ratingData.RatingsTotal
            });
        }

        /// <summary>
        /// Rates a video.
        /// </summary>
        [HttpPost, Authorize]
        public async Task<JsonNetResult> Rate(RateVideoViewModel model)
        {
            await _videoWriteModel.RateVideo(new RateVideo
            {
                VideoId = model.VideoId, 
                UserId = User.GetCurrentUserId().Value,
                Rating = model.Rating
            });
            return JsonSuccess();
        }
	}
}