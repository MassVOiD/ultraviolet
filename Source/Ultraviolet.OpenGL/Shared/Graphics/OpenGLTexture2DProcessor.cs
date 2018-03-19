﻿using System;
using System.IO;
using Ultraviolet.Content;
using Ultraviolet.Core;
using Ultraviolet.Graphics;
using Ultraviolet.OpenGL.Bindings;

namespace Ultraviolet.OpenGL.Graphics
{
    /// <summary>
    /// Loads 2D texture assets.
    /// </summary>
    [Preserve(AllMembers = true)]
    [ContentProcessor]
    public sealed class OpenGLTexture2DProcessor : ContentProcessor<PlatformNativeSurface, Texture2D>
    {
        /// <inheritdoc/>
        public override void ExportPreprocessed(ContentManager manager, IContentProcessorMetadata metadata, BinaryWriter writer, PlatformNativeSurface input, Boolean delete)
        {
            var mdat = metadata.As<OpenGLTexture2DProcessorMetadata>();

            using (var surface = Surface2D.Create(input))
            {
                var flipdir = manager.Ultraviolet.GetGraphics().Capabilities.FlippedTextures ? SurfaceFlipDirection.Vertical : SurfaceFlipDirection.None;
                surface.FlipAndProcessAlpha(flipdir, mdat.PremultiplyAlpha, mdat.Opaque ? null : (Color?)Color.Magenta);

                using (var memstream = new MemoryStream())
                {
                    surface.SaveAsPng(memstream);
                    writer.Write((int)memstream.Length);
                    writer.Write(memstream.ToArray());
                }
            }
        }

        /// <inheritdoc/>
        public override Texture2D ImportPreprocessed(ContentManager manager, IContentProcessorMetadata metadata, BinaryReader reader)
        {
            var length = reader.ReadInt32();
            var bytes = reader.ReadBytes(length);

            using (var stream = new MemoryStream(bytes))
            {
                using (var source = SurfaceSource.Create(stream))
                {
                    var imgInternalFormat = gl.IsGLES2 ? gl.GL_RGBA : gl.GL_RGBA8;
                    var imgFormat = (source.DataFormat == SurfaceSourceDataFormat.RGBA) ? gl.GL_RGBA : gl.GL_BGRA;

                    var imgTexture = new OpenGLTexture2D(manager.Ultraviolet, imgInternalFormat,
                        source.Width, source.Height, imgFormat, gl.GL_UNSIGNED_BYTE, source.Data, true);

                    return imgTexture;
                }
            }
        }

        /// <inheritdoc/>
        public override Texture2D Process(ContentManager manager, IContentProcessorMetadata metadata, PlatformNativeSurface input)
        {
            var mdat = metadata.As<OpenGLTexture2DProcessorMetadata>();

            using (var surface = Surface2D.Create(input))
            {
                var flipdir = manager.Ultraviolet.GetGraphics().Capabilities.FlippedTextures ? SurfaceFlipDirection.Vertical : SurfaceFlipDirection.None;
                surface.FlipAndProcessAlpha(flipdir, mdat.PremultiplyAlpha, mdat.Opaque ? null : (Color?)Color.Magenta);

                return surface.CreateTexture(unprocessed: true);
            }
        }

        /// <inheritdoc/>
        public override Boolean SupportsPreprocessing
        {
            get { return true; }
        }
    }
}
