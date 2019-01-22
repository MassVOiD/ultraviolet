﻿using System;

namespace Ultraviolet.Graphics
{
    /// <summary>
    /// Represents one render pass in an effect technique.
    /// </summary>
    public abstract class EffectPass : UltravioletResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EffectPass"/> class.
        /// </summary>
        /// <param name="uv">The Ultraviolet context.</param>
        public EffectPass(UltravioletContext uv)
            : base(uv)
        {

        }

        /// <summary>
        /// Applies the effect pass state to the device.
        /// </summary>
        public virtual void Apply()
        {
            Effect?.OnApply();
        }

        /// <summary>
        /// Gets the effect pass's name.
        /// </summary>
        public abstract String Name
        {
            get;
        }

        /// <summary>
        /// Gets or sets the <see cref="Ultraviolet.Graphics.Effect"/> which owns this pass.
        /// </summary>
        internal Effect Effect { get; set; }
    }
}
