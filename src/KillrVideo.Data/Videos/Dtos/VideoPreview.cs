﻿using System;

namespace KillrVideo.Data.Videos.Dtos
{
    /// <summary>
    /// DTO representing some basic details of a video necessary for showing a preview.
    /// </summary>
    [Serializable]
    public class VideoPreview
    {
        public Guid VideoId { get; set; }
        public DateTimeOffset AddedDate { get; set; }
        public string Name { get; set; }
        public string PreviewImageLocation { get; set; }
    }
}