﻿////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2012-2017 Flax Engine. All rights reserved.
////////////////////////////////////////////////////////////////////////////////////

namespace FlaxEditor.Content.Import
{
    /// <summary>
    /// Texture asset import entry.
    /// </summary>
    /// <seealso cref="FlaxEditor.Content.Import.AssetFileEntry" />
    public class TextureFileEntry : AssetFileEntry
    {
        // TODO: add import texture settings

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureFileEntry"/> class.
        /// </summary>
        /// <param name="url">The source file url.</param>
        /// <param name="resultUrl">The result file url.</param>
        public TextureFileEntry(string url, string resultUrl)
            : base(url, resultUrl)
        {
            // TODO: prepare import options based on file name
        }

        /// <inheritdoc />
        public override bool HasSettings => true;
    }
}