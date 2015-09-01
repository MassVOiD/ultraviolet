﻿using System;
using TwistedLogik.Ultraviolet.Graphics;
using TwistedLogik.Ultraviolet.Graphics.Graphics2D;

namespace TwistedLogik.Ultraviolet.UI.Presentation.Media.Effects
{
    /// <summary>
    /// Represents an effect that blurs target element.
    /// </summary>
    [UvmlKnownType]
    public sealed class BlurEffect : Effect
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlurEffect"/> class.
        /// </summary>
        public BlurEffect()
        {
            effect.Value.Mix = 0f;
        }

        /// <inheritdoc/>
        public override Int32 AdditionalRenderTargetsRequired
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets or sets the radius of the blur which is applied to the element.
        /// </summary>
        public Double Radius
        {
            get { return GetValue<Double>(BlurRadiusProperty); }
            set { SetValue(BlurRadiusProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="BlurRadius"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BlurRadiusProperty = DependencyProperty.Register("Radius", typeof(Double), typeof(BlurEffect),
            new PropertyMetadata<Double>(5.0, PropertyMetadataOptions.None));        

        /// <inheritdoc/>
        protected internal override void DrawRenderTargets(DrawingContext dc, UIElement element, OutOfBandRenderTarget target)
        {
            var shadowTarget = target.Next.RenderTarget;

            var gfx = dc.SpriteBatch.Ultraviolet.GetGraphics();
            gfx.SetRenderTarget(shadowTarget);
            gfx.Clear(Color.Transparent);

            effect.Value.Radius = GetRadiusInPixels(element);
            effect.Value.Direction = BlurDirection.Horizontal;

            dc.Begin(SpriteSortMode.Immediate, effect, Matrix.Identity);
            dc.SpriteBatch.Draw(target.ColorBuffer, Vector2.Zero, Color.White);
            dc.End();
        }

        /// <inheritdoc/>
        protected internal override void Draw(DrawingContext dc, UIElement element, OutOfBandRenderTarget target)
        {
            var state = dc.SpriteBatch.GetCurrentState();
            
            var position = (Vector2)element.View.Display.DipsToPixels(target.RelativeVisualBounds.Location);
            var positionRounded = new Vector2((Int32)position.X, (Int32)position.Y);

            dc.End();

            effect.Value.Radius = GetRadiusInPixels(element);
            effect.Value.Direction = BlurDirection.Vertical;

            dc.Begin(SpriteSortMode.Immediate, effect, Matrix.Identity);

            var shadowTexture = target.Next.ColorBuffer;
            dc.SpriteBatch.Draw(shadowTexture, positionRounded, null, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
            
            dc.End();
            dc.Begin(state.SortMode, state.Effect, state.TransformMatrix);
        }

        /// <inheritdoc/>
        protected internal override void ModifyVisualBounds(ref RectangleD bounds)
        {
            var radius = Radius;
            RectangleD.Inflate(ref bounds, radius, radius, out bounds);
        }

        /// <summary>
        /// Gets the blur radius for the specified element, in pixels.
        /// </summary>
        private Single GetRadiusInPixels(UIElement element)
        {
            var display = element.View.Display;
            return (Single)display.DipsToPixels(Radius);
        }

        // The singleton instance of effect used to render the shadow.
        private static readonly UltravioletSingleton<Graphics.BlurEffect> effect = new UltravioletSingleton<Graphics.BlurEffect>((uv) =>
        {
            return Graphics.BlurEffect.Create();
        });
    }
}
