﻿using System;
using System.Collections.Generic;
using KillrVideo.Data;

namespace KillrVideo.Models.Videos
{
    /// <summary>
    /// View model for viewing a video.
    /// </summary>
    public class ViewVideoViewModel
    {
        public Guid VideoId { get; set; }
        public DateTimeOffset UploadDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public VideoLocationType LocationType { get; set; }
        public string Location { get; set; }

        public bool InProgress { get; set; }
        public string InProgressJobId { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }
}