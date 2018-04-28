﻿using System;
using Newtonsoft.Json;
using Ultraviolet.Core;

namespace Ultraviolet.Graphics.Graphics2D.Text
{
    /// <summary>
    /// An intermediate representation of a cell in a <see cref="TextTableLayout"/> used during content loading.
    /// </summary>
    internal class TextTableLayoutCellDescription
    {
        /// <summary>
        /// Gets or sets the set of <see cref="TextFlags"/> values used to draw the cell's text.
        /// </summary>
        [JsonProperty(Required = Required.DisallowNull)]
        [JsonConverter(typeof(CoreEnumJsonConverter))]
        public TextFlags TextFlags { get; set; }

        /// <summary>
        /// Gets or sets the cell's text.
        /// </summary>
        public String Text { get; set; }

        /// <summary>
        /// Gets or sets the cell's format string.
        /// </summary>
        public String Format { get; set; }

        /// <summary>
        /// Gets or sets the cell's view model binding string.
        /// </summary>
        public String Binding { get; set; }

        /// <summary>
        /// Gets or sets the cell's width in pixels.
        /// </summary>
        public Int32? Width { get; set; }

        /// <summary>
        /// Gets or sets the cell's height in pixels.
        /// </summary>
        public Int32? Height { get; set; }
    }
}
