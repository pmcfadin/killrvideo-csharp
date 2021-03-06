﻿using System;

namespace KillrVideo.Models.Upload
{
    /// <summary>
    /// Return model for a successfully created asset.
    /// </summary>
    [Serializable]
    public class AssetCreatedViewModel
    {
        /// <summary>
        /// The unique identifier of the asset.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The name of the file being uploaded.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The storage URL in Azure Media Services where the video can be uploaded.
        /// </summary>
        public string UploadUrl { get; set; }

        /// <summary>
        /// The Id for the upload locator.
        /// </summary>
        public string UploadLocatorId { get; set; }
    }
}