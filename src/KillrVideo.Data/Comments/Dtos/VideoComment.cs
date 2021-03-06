﻿using System;

namespace KillrVideo.Data.Comments.Dtos
{
    /// <summary>
    /// A comment on a video.
    /// </summary>
    [Serializable]
    public class VideoComment
    {
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset CommentTimestamp { get; set; }
    }
}