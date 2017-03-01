﻿using System;
using TwistedLogik.Nucleus.Collections;

namespace TwistedLogik.Ultraviolet.UI.Presentation.Input
{
    /// <summary>
    /// Represents the extended event data for a <see cref="CanExecuteRoutedEventHandler"/> delegate.
    /// </summary>
    public class CanExecuteRoutedEventData : RoutedEventData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CanExecuteRoutedEventData"/> class.
        /// </summary>
        protected internal CanExecuteRoutedEventData() { }

        /// <summary>
        /// Retrieves an instance of the <see cref="RoutedEventData"/> class from the global
        /// pool and initializes it for use with a routed event handler.
        /// </summary>
        /// <param name="source">The object that raised the event.</param>
        /// <param name="canExecute">A value indicating whether the command can be executed with the specified parameter.</param>
        /// <param name="continueRouting">A value indicating whether the input event that caused the command to execute should continue its route.</param>
        /// <param name="handled">A value indicating whether the event has been handled.</param>
        /// <param name="autorelease">A value indicating whether the data is automatically released
        /// back to the global pool after it has been used by an event handler delegate.</param>
        /// <returns>The <see cref="RoutedEventData"/> instance that was retrieved.</returns>
        public static CanExecuteRoutedEventData Retrieve(DependencyObject source, Boolean canExecute = false, Boolean continueRouting = false, Boolean handled = false, Boolean autorelease = true)
        {
            var data = default(CanExecuteRoutedEventData);

            lock (pool)
                data = pool.Retrieve();

            data.OnRetrieved(pool, source, handled, autorelease);
            data.canExecute = canExecute;
            data.continueRouting = continueRouting;
            return data;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command can be executed
        /// with the specified parameter.
        /// </summary>
        public Boolean CanExecute => canExecute;

        /// <summary>
        /// Gets or sets a value indicating whether the input event that
        /// caused the command to execute should continue its route.
        /// </summary>
        public Boolean ContinueRouting => continueRouting;
        
        /// <inheritdoc/>
        protected override void OnRetrieved(IPool origin, DependencyObject source, Boolean handled, Boolean autorelease)
        {
            base.OnRetrieved(origin, source, handled, autorelease);
        }

        /// <summary>
        /// Releases any references held by the object.
        /// </summary>
        protected override void OnReleased()
        {
            this.canExecute = false;
            this.continueRouting = false;
            this.autorelease = true;
            this.origin = null;
        }

        // The global pool of event args objects.
        private static readonly Pool<CanExecuteRoutedEventData> pool =
            new ExpandingPool<CanExecuteRoutedEventData>(1, 8, () => new CanExecuteRoutedEventData());

        // Property values.
        private Boolean canExecute;
        private Boolean continueRouting;
    }
}
