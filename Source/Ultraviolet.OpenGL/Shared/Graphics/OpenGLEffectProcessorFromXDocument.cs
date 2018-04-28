﻿using System;
using System.IO;
using System.Xml.Linq;
using Ultraviolet.Content;
using Ultraviolet.Core;
using Ultraviolet.Graphics;

namespace Ultraviolet.OpenGL.Graphics
{
    /// <summary>
    /// Loads shader effect assets.
    /// </summary>
    [ContentProcessor]
    public sealed class OpenGLEffectProcessorFromXDocument : ContentProcessor<XDocument, Effect>
    {
        /// <inheritdoc/>
        public override void ExportPreprocessed(ContentManager manager, IContentProcessorMetadata metadata, BinaryWriter writer, XDocument input, Boolean delete) =>
            implProcessor.ExportPreprocessed(manager, metadata, writer, input, delete);

        /// <inheritdoc/>
        public override Effect ImportPreprocessed(ContentManager manager, IContentProcessorMetadata metadata, BinaryReader reader) =>
            Effect.Create(implProcessor.ImportPreprocessed(manager, metadata, reader));

        /// <inheritdoc/>
        public override Effect Process(ContentManager manager, IContentProcessorMetadata metadata, XDocument input) =>
            Effect.Create(implProcessor.Process(manager, metadata, input));

        /// <inheritdoc/>
        public override Boolean SupportsPreprocessing => true;

        // State values.
        private readonly OpenGLEffectImplementationProcessorFromXDocument implProcessor =
            new OpenGLEffectImplementationProcessorFromXDocument();
    }
}
