﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Ultraviolet.Core;
using Ultraviolet.Graphics;

namespace Ultraviolet.Shims.Desktop.Graphics
{
    /// <summary>
    /// Represents an implementation of the <see cref="SurfaceSource"/> class for desktop platforms.
    /// </summary>
    public sealed class DesktopSurfaceSource : SurfaceSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopSurfaceSource"/> class.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> that contains the surface data.</param>
        public DesktopSurfaceSource(Stream stream)
        {
            Contract.Require(stream, nameof(stream));

            var data = new Byte[stream.Length];
            stream.Read(data, 0, data.Length);

            using (var mstream = new MemoryStream(data))
            {
                this.bmp = new Bitmap(mstream);
                this.bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopSurfaceSource"/> class.
        /// </summary>
        /// <param name="bmp">The bitmap from which to read surface data.</param>
        public DesktopSurfaceSource(Bitmap bmp)
        {
            Contract.Require(bmp, nameof(bmp));

            this.bmp = bmp;
            this.bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override Color this[int x, int y]
        {
            get
            {
                Contract.EnsureNotDisposed(this, disposed);

                unsafe
                {
                    var pixel = ((byte*)bmpData.Scan0) + (bmpData.Stride * y) + (x * sizeof(UInt32));
                    var b = *pixel++;
                    var g = *pixel++;
                    var r = *pixel++;
                    var a = *pixel++;
                    return new Color(r, g, b, a);
                }
            }
        }

        /// <inheritdoc/>
        public override IntPtr Data => bmpData.Scan0;

        /// <inheritdoc/>
        public override Int32 Stride => bmpData.Stride;

        /// <inheritdoc/>
        public override Int32 Width => bmp.Width;

        /// <inheritdoc/>
        public override Int32 Height => bmp.Height;

        /// <inheritdoc/>
        public override SurfaceSourceDataFormat DataFormat => SurfaceSourceDataFormat.BGRA;

        /// <summary>
        /// Releases resources associated with the object.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if the object is being disposed; <see langword="false"/> if the object is being finalized.</param>
        private void Dispose(Boolean disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                bmp.UnlockBits(bmpData);
                bmp.Dispose();
            }

            disposed = true;
        }

        // State values.
        private readonly Bitmap bmp;
        private readonly BitmapData bmpData;
        private Boolean disposed;
    }
}
