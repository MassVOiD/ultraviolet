﻿using System;
using TwistedLogik.Nucleus;
using TwistedLogik.Ultraviolet.Content;
using TwistedLogik.Ultraviolet.Graphics.Graphics2D;
using TwistedLogik.Ultraviolet.Input;
using TwistedLogik.Ultraviolet.Platform;
using TwistedLogik.Ultraviolet.UI.Presentation.Animations;
using TwistedLogik.Ultraviolet.UI.Presentation.Controls;
using TwistedLogik.Ultraviolet.UI.Presentation.Input;
using TwistedLogik.Ultraviolet.UI.Presentation.Styles;

namespace TwistedLogik.Ultraviolet.UI.Presentation
{
    /// <summary>
    /// Represents the top-level container for UI elements.
    /// </summary>
    public sealed class PresentationFoundationView : UIView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PresentationFoundationView"/> class.
        /// </summary>
        /// <param name="uv">The Ultraviolet context.</param>
        /// <param name="viewModelType">The view's associated model type.</param>
        public PresentationFoundationView(UltravioletContext uv, Type viewModelType)
            : base(uv, viewModelType)
        {
            this.namescope = new Namescope();
            this.resources       = new PresentationFoundationViewResources(this);
            this.drawingContext  = new DrawingContext(this);

            this.layoutRoot = new Grid(uv, null);
            this.layoutRoot.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.layoutRoot.VerticalAlignment = VerticalAlignment.Stretch;
            this.layoutRoot.View = this;
            this.layoutRoot.CacheLayoutParameters();
            this.layoutRoot.InvalidateMeasure();

            HookKeyboardEvents();
            HookMouseEvents();
        }

        /// <summary>
        /// Loads an instance of <see cref="PresentationFoundationView"/> from an XML document.
        /// </summary>
        /// <param name="uv">The Ultraviolet context.</param>
        /// <param name="uiPanelDefinition">The <see cref="UIPanelDefinition"/> that defines the view's containing panel.</param>
        /// <returns>The <see cref="PresentationFoundationView"/> that was loaded from the specified XML document.</returns>
        public static PresentationFoundationView Load(UltravioletContext uv, UIPanelDefinition uiPanelDefinition)
        {
            Contract.Require(uv, "uv");
            Contract.Require(uiPanelDefinition, "uiPanelDefinition");

            if (uiPanelDefinition.ViewElement == null)
                return null;

            var view = UvmlLoader.Load(uv, uiPanelDefinition.ViewElement);

            var uvss    = String.Join(Environment.NewLine, uiPanelDefinition.Stylesheets);
            var uvssdoc = UvssDocument.Parse(uvss);

            view.SetStylesheet(uvssdoc);

            return view;
        }

        /// <inheritdoc/>
        public override void Cleanup()
        {
            layoutRoot.Cleanup();
        }

        /// <inheritdoc/>
        public override void Draw(UltravioletTime time, SpriteBatch spriteBatch)
        {
            Contract.Require(time, "time");
            Contract.Require(spriteBatch, "spriteBatch");

            if (Window == null)
                return;

            drawingContext.Reset();
            drawingContext.SpriteBatch = spriteBatch;

            layoutRoot.Draw(time, drawingContext);

            drawingContext.SpriteBatch = null;
        }

        /// <inheritdoc/>
        public override void Update(UltravioletTime time)
        {
            Contract.Require(time, "time");

            if (Window == null)
                return;

            HandleUserInput();

            layoutRoot.Update(time);
        }

        /// <summary>
        /// Invalidates the styling state of the view's layout root.
        /// </summary>
        public void InvalidateStyle()
        {
            layoutRoot.InvalidateStyle();
        }

        /// <summary>
        /// Invalidates the measurement state of the view's layout root.
        /// </summary>
        public void InvalidateMeasure()
        {
            layoutRoot.InvalidateMeasure();
        }

        /// <summary>
        /// Invalidates the arrangement state of the view's layout root.
        /// </summary>
        public void InvalidateArrange()
        {
            layoutRoot.InvalidateArrange();
        }

        /// <summary>
        /// Grants input focus within this view to the specified element.
        /// </summary>
        /// <param name="element">The element to which to grant input focus.</param>
        public void FocusElement(IInputElement element)
        {
            Contract.Require(element, "element");

            if (elementWithFocus == element || !element.Focusable)
                return;

            if (elementWithFocus != null)
            {
                BlurElement(elementWithFocus);
            }

            elementWithFocus = element;
            
            SetIsKeyboardFocusWithin(elementWithFocus, true);

            var dobj = elementWithFocus as DependencyObject;
            if (dobj != null)
            {
                var handledGotFocus = false;
                Keyboard.RaiseGotKeyboardFocus(dobj, ref handledGotFocus);
            }
        }

        /// <summary>
        /// Removes input focus within this view from the specified element.
        /// </summary>
        /// <param name="element">The element from which to remove input focus.</param>
        public void BlurElement(IInputElement element)
        {
            Contract.Require(element, "element");

            if (elementWithFocus != element)
                return;

            var elementWithFocusOld = elementWithFocus;
            elementWithFocus = null;

            SetIsKeyboardFocusWithin(elementWithFocusOld, false);

            var dobj = elementWithFocusOld as DependencyObject;
            if (dobj != null)
            {
                var handledLostFocus = false;
                Keyboard.RaiseLostKeyboardFocus(dobj, ref handledLostFocus);
            }
        }

        /// <summary>
        /// Assigns mouse capture to the specified element.
        /// </summary>
        /// <param name="element">The element to which to assign mouse capture.</param>
        public void CaptureMouse(IInputElement element)
        {
            Contract.Require(element, "element");

            if (elementWithMouseCapture == element)
                return;

            if (elementWithMouseCapture != null)
            {
                ReleaseMouse(elementWithMouseCapture);
            }

            elementWithMouseCapture = element;

            var dobj = elementWithMouseCapture as DependencyObject;
            if (dobj != null)
            {
                var gotMouseCaptureHandled = false;
                Mouse.RaiseGotMouseCapture(dobj, ref gotMouseCaptureHandled);
            }

            UpdateIsMouseOver(elementWithMouseCapture as UIElement);
        }

        /// <summary>
        /// Releases the mouse from the element that is currently capturing it.
        /// </summary>
        /// <param name="element">The element that is attempting to release mouse capture.</param>
        public void ReleaseMouse(IInputElement element)
        {
            Contract.Require(element, "element");

            if (elementWithMouseCapture != element)
                return;

            var dobj = elementWithMouseCapture as DependencyObject;
            if (dobj != null)
            {
                var lostMouseCaptureHandled = false;
                Mouse.RaiseLostMouseCapture(dobj, ref lostMouseCaptureHandled);
            }

            UpdateIsMouseOver(elementWithMouseCapture as UIElement);

            elementWithMouseCapture = null;
        }

        /// <summary>
        /// Gets the element within the view which has the specified identifying name.
        /// </summary>
        /// <param name="name">The identifying name of the element to retrieve.</param>
        /// <returns>The element with the specified identifying name, or <c>null</c> if no such element exists.</returns>
        public UIElement GetElementByName(String name)
        {
            Contract.RequireNotEmpty(name, "id");

            return namescope.GetElementByName(name);
        }

        /// <summary>
        /// Performs a hit test against the view at the specified point in screen space.
        /// </summary>
        /// <param name="x">The x-coordinate in screen space to evaluate.</param>
        /// <param name="y">The y-coordinate in screen space to evaluate.</param>
        /// <returns>The topmost <see cref="Visual"/> in the view which contains the specified point, or <c>null</c>.</returns>
        public Visual HitTestScreenPixel(Int32 x, Int32 y)
        {
            var dipsX = Display.PixelsToDips(x - Area.X);
            var dipsY = Display.PixelsToDips(y - Area.Y);

            return LayoutRoot.HitTest(new Point2D(dipsX, dipsY));
        }

        /// <summary>
        /// Performs a hit test against the view at the specified point in screen space.
        /// </summary>
        /// <param name="point">The point in screen space to evaluate.</param>
        /// <returns>The topmost <see cref="Visual"/> in the view which contains the specified point, or <c>null</c>.</returns>
        public Visual HitTestScreenPixel(Point2 point)
        {
            var dipsPoint = Display.PixelsToDips(point - Area.Location);

            return LayoutRoot.HitTest(dipsPoint);
        }

        /// <summary>
        /// Performs a hit test against the view at the specified point in view-relative screen space.
        /// </summary>
        /// <param name="x">The x-coordinate in view-relative screen space to evaluate.</param>
        /// <param name="y">The y-coordinate in view-relative screen space to evaluate.</param>
        /// <returns>The topmost <see cref="Visual"/> in the view which contains the specified point, or <c>null</c>.</returns>
        public Visual HitTestPixel(Int32 x, Int32 y)
        {
            var dipsX = Display.PixelsToDips(x);
            var dipsY = Display.PixelsToDips(y);

            return LayoutRoot.HitTest(new Point2D(dipsX, dipsY));
        }

        /// <summary>
        /// Performs a hit test against the view at the specified point in view-relative screen space.
        /// </summary>
        /// <param name="point">The point in view-relative screen space to evaluate.</param>
        /// <returns>The topmost <see cref="Visual"/> in the view which contains the specified point, or <c>null</c>.</returns>
        public Visual HitTestPixel(Point2 point)
        {
            var dipsPoint = Display.PixelsToDips(point - Area.Location);

            return LayoutRoot.HitTest(dipsPoint);
        }

        /// <summary>
        /// Performs a hit test against the view at the specified point in device-independent view space.
        /// </summary>
        /// <param name="x">The x-coordinate in device-independent view space to evaluate.</param>
        /// <param name="y">The y-coordinate in device-independent view space to evaluate.</param>
        /// <returns>The topmost <see cref="Visual"/> in the view which contains the specified point, or <c>null</c>.</returns>
        public Visual HitTest(Double x, Double y)
        {
            return LayoutRoot.HitTest(new Point2D(x, y));
        }

        /// <summary>
        /// Performs a hit test against the view at the specified point in device-independent view space.
        /// </summary>
        /// <param name="point">The point in device-independent view space to evaluate.</param>
        /// <returns>The topmost <see cref="Visual"/> in the view which contains the specified point, or <c>null</c>.</returns>
        public Visual HitTest(Point2D point)
        {
            return LayoutRoot.HitTest(point);
        }

        /// <summary>
        /// Sets the view's stylesheet.
        /// </summary>
        /// <param name="stylesheet">The view's stylesheet.</param>
        public void SetStylesheet(UvssDocument stylesheet)
        {
            this.stylesheet = stylesheet;

            if (stylesheet != null)
            {
                LoadViewResources(stylesheet);
                layoutRoot.Style(stylesheet);
            }
            else
            {
                LoadViewResources(null);
                layoutRoot.ClearStyledValues(true);
            }
        }

        /// <summary>
        /// Loads the specified resource from the global content manager.
        /// </summary>
        /// <param name="resource">The resource to load.</param>
        /// <param name="asset">The asset identifier that specifies which resource to load.</param>
        public void LoadGlobalResource<T>(FrameworkResource<T> resource, AssetID asset) where T : class
        {
            if (resource == null || GlobalContent == null)
                return;

            resource.Load(GlobalContent, asset);
        }

        /// <summary>
        /// Loads the specified resource from the local content manager.
        /// </summary>
        /// <param name="resource">The resource to load.</param>
        /// <param name="asset">The asset identifier that specifies which resource to load.</param>
        public void LoadLocalResource<T>(FrameworkResource<T> resource, AssetID asset) where T : class
        {
            if (resource == null || LocalContent == null)
                return;

            resource.Load(LocalContent, asset);
        }

        /// <summary>
        /// Loads the specified sourced image.
        /// </summary>
        /// <param name="image">The identifier of the image to load.</param>
        public void LoadImage(SourcedImage image)
        {
            if (image.Resource == null)
                return;

            switch (image.Source)
            {
                case AssetSource.Global:
                    if (GlobalContent != null)
                    {
                        image.Resource.Load(GlobalContent);
                    }
                    break;

                case AssetSource.Local:
                    if (LocalContent != null)
                    {
                        image.Resource.Load(LocalContent);
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Loads the specified sourced resource.
        /// </summary>
        /// <param name="resource">The identifier of the resource to load.</param>
        public void LoadResource<T>(SourcedResource<T> resource) where T : class
        {
            if (resource.Resource == null)
                return;

            switch (resource.Source)
            {
                case AssetSource.Global:
                    if (GlobalContent != null)
                    {
                        resource.Load(GlobalContent);
                    }
                    break;

                case AssetSource.Local:
                    if (LocalContent != null)
                    {
                        resource.Load(LocalContent);
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Searches the view's associated stylesheet for a storyboard with the specified name.
        /// </summary>
        /// <param name="name">The name of the storyboard to retrieve.</param>
        /// <returns>The <see cref="Storyboard"/> with the specified name, or <c>null</c> if the specified storyboard does not exist.</returns>
        public Storyboard FindStoryboard(String name)
        {
            Contract.RequireNotEmpty(name, "name");

            if (Stylesheet != null)
            {
                return Stylesheet.InstantiateStoryboardByName(LayoutRoot.Ultraviolet, name);
            }

            return null;
        }

        /// <summary>
        /// Gets the stylesheet that is currently applied to this view.
        /// </summary>
        public UvssDocument Stylesheet
        {
            get { return stylesheet; }
        }

        /// <summary>
        /// Gets the root element of the view's layout.
        /// </summary>
        public UIElement LayoutRoot
        {
            get { return layoutRoot; }
        }

        /// <summary>
        /// Gets the element that is currently under the mouse cursor.
        /// </summary>
        public IInputElement ElementUnderMouse
        {
            get { return elementUnderMouse; }
        }

        /// <summary>
        /// Gets the element that currently has focus.
        /// </summary>
        public IInputElement ElementWithFocus
        {
            get { return elementWithFocus; }
        }

        /// <summary>
        /// Gets the element that currently has mouse capture.
        /// </summary>
        public IInputElement ElementWithMouseCapture
        {
            get { return elementWithMouseCapture; }
        }

        /// <summary>
        /// Gets the view's global resource collection.
        /// </summary>
        public PresentationFoundationViewResources Resources
        {
            get { return resources; }
        }
        
        /// <summary>
        /// Gets the namescope for the view layout.
        /// </summary>
        internal Namescope Namescope
        {
            get { return namescope; }
        } 

        /// <inheritdoc/>
        protected override void Dispose(Boolean disposing)
        {
            if (disposing && !Ultraviolet.Disposed)
            {
                UnhookKeyboardEvents();
                UnhookMouseEvents();
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override void OnContentManagersChanged()
        {
            resources.Reload();
            layoutRoot.ReloadContent(true);

            base.OnContentManagersChanged();
        }

        /// <inheritdoc/>
        protected override void OnViewModelChanged()
        {            
            layoutRoot.CacheLayoutParameters();

            if (ViewModel != null)
                namescope.PopulateFieldsFromRegisteredElements(ViewModel);

            base.OnViewModelChanged();
        }

        /// <inheritdoc/>
        protected override void OnViewSizeChanged()
        {
            var dipsArea = Display.PixelsToDips(Area);

            layoutRoot.Measure(dipsArea.Size);
            layoutRoot.Arrange(dipsArea);

            base.OnViewSizeChanged();
        }

        /// <summary>
        /// Loads the view's global resources from the specified stylesheet.
        /// </summary>
        /// <param name="stylesheet">The stylesheet from which to load global resources.</param>
        private void LoadViewResources(UvssDocument stylesheet)
        {
            resources.ClearStyledValues();

            if (stylesheet != null)
            {
                resources.ApplyStyles(stylesheet);
            }
        }

        /// <summary>
        /// Hooks into Ultraviolet's keyboard input events.
        /// </summary>
        private void HookKeyboardEvents()
        {
            var input = Ultraviolet.GetInput();
            if (input.IsKeyboardSupported())
            {
                var keyboard          = input.GetKeyboard();
                keyboard.KeyPressed  += keyboard_KeyPressed;
                keyboard.KeyReleased += keyboard_KeyReleased;
                keyboard.TextInput   += keyboard_TextInput;
            }
        }

        /// <summary>
        /// Hooks into Ultraviolet's mouse input events.
        /// </summary>
        private void HookMouseEvents()
        {
            var input = Ultraviolet.GetInput();
            if (input.IsMouseSupported())
            {
                var mouse             = input.GetMouse();
                mouse.Moved          += mouse_Moved;
                mouse.ButtonPressed  += mouse_ButtonPressed;
                mouse.ButtonReleased += mouse_ButtonReleased;
                mouse.Click          += mouse_Click;
                mouse.DoubleClick    += mouse_DoubleClick;
                mouse.WheelScrolled  += mouse_WheelScrolled;
            }
        }

        /// <summary>
        /// Unhooks from Ultraviolet's keyboard input events.
        /// </summary>
        private void UnhookKeyboardEvents()
        {
            var input = Ultraviolet.GetInput();
            if (input.IsKeyboardSupported())
            {
                var keyboard          = input.GetKeyboard();
                keyboard.KeyPressed  -= keyboard_KeyPressed;
                keyboard.KeyReleased -= keyboard_KeyReleased;
                keyboard.TextInput   -= keyboard_TextInput;
            }
        }

        /// <summary>
        /// Unhooks from Ultraviolet's mouse input events.
        /// </summary>
        private void UnhookMouseEvents()
        {
            var input = Ultraviolet.GetInput();
            if (input.IsMouseSupported())
            {
                var mouse             = input.GetMouse();
                mouse.Moved          -= mouse_Moved;
                mouse.ButtonPressed  -= mouse_ButtonPressed;
                mouse.ButtonReleased -= mouse_ButtonReleased;
                mouse.Click          -= mouse_Click;
                mouse.DoubleClick    -= mouse_DoubleClick;
                mouse.WheelScrolled  -= mouse_WheelScrolled;
            }
        }

        /// <summary>
        /// Handles user input by raising input messages on the elements in the view.
        /// </summary>
        private void HandleUserInput()
        {
            if (Ultraviolet.GetInput().IsKeyboardSupported())
            {
                HandleKeyboardInput();
            }
            if (Ultraviolet.GetInput().IsMouseSupported())
            {
                HandleMouseInput();
            }
        }

        /// <summary>
        /// Handles keyboard input.
        /// </summary>
        private void HandleKeyboardInput()
        {
            // TODO
        }

        /// <summary>
        /// Handles mouse input.
        /// </summary>
        private void HandleMouseInput()
        {
            UpdateElementUnderMouse();
        }

        /// <summary>
        /// Updates the value of the <see cref="UIElement.IsMouseOver"/> property for ancestors
        /// of the specified element.
        /// </summary>
        /// <param name="root">The element to update.</param>
        private void UpdateIsMouseOver(UIElement root)
        {
            if (root == null)
                return;

            var mouse    = Ultraviolet.GetInput().GetMouse();
            var mousePos = Display.PixelsToDips(mouse.Position);

            var current = root as DependencyObject;
            while (current != null)
            {
                var uiElement = current as UIElement;
                if (uiElement != null)
                {
                    var bounds = uiElement.AbsoluteBounds;
                    uiElement.IsMouseOver = bounds.Contains(mousePos);
                }
                current = VisualTreeHelper.GetParent(current);
            }
        }

        /// <summary>
        /// Determines which element is currently under the mouse cursor.
        /// </summary>
        private void UpdateElementUnderMouse()
        {
            var mouse = Ultraviolet.GetInput().GetMouse();

            // Determine which element is currently under the mouse cursor.
            if (elementWithMouseCapture == null)
            {
                var mousePos  = mouse.Position;
                var mouseView = mouse.Window == Window ? this : null;

                elementUnderMousePrev = elementUnderMouse;
                elementUnderMouse     = (mouseView == null) ? null : mouseView.HitTestScreenPixel((Point2)mousePos) as UIElement;
            }

            if (elementUnderMouse != null && !IsElementValidForInput(elementUnderMouse))
                elementUnderMouse = null;

            if (elementWithMouseCapture != null && !IsElementValidForInput(elementWithMouseCapture))
                ReleaseMouse(elementWithMouseCapture);

            // Handle mouse motion events
            if (elementUnderMouse != elementUnderMousePrev)
            {
                UpdateIsMouseOver(elementUnderMousePrev as UIElement);

                if (elementUnderMousePrev != null)
                {
                    var uiElement = elementUnderMousePrev as UIElement;
                    if (uiElement != null)
                        uiElement.IsMouseDirectlyOver = false;

                    var dobj = elementUnderMousePrev as DependencyObject;
                    if (dobj != null)
                    {
                        var mouseLeaveHandled = false;
                        Mouse.RaiseMouseLeave(dobj, mouse, ref mouseLeaveHandled);
                    }
                }

                if (elementUnderMouse != null)
                {
                    var uiElement = elementUnderMouse as UIElement;
                    if (uiElement != null)
                        uiElement.IsMouseDirectlyOver = true;

                    var dobj = elementUnderMouse as DependencyObject;
                    if (dobj != null)
                    {
                        var mouseEnterHandled = false;
                        Mouse.RaiseMouseEnter(dobj, mouse, ref mouseEnterHandled);
                    }
                }

                UpdateIsMouseOver(elementUnderMouse as UIElement);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specified element is valid for receiving input.
        /// </summary>
        /// <param name="element">The element to evaluate.</param>
        /// <returns><c>true</c> if the specified element is valid for input; otherwise, <c>false</c>.</returns>
        private Boolean IsElementValidForInput(IInputElement element)
        {
            var uiElement = element as UIElement;
            if (uiElement == null)
                return false;

            return uiElement.IsHitTestVisible && uiElement.IsEnabled;
        }

        /// <summary>
        /// Sets the value of the <see cref="IInputElement.IsKeyboardFocusWithin"/> property on the specified element
        /// and all of its ancestors to the specified value.
        /// </summary>
        /// <param name="element">The element on which to set the property value.</param>
        /// <param name="value">The value to set on the element and its ancestors.</param>
        private void SetIsKeyboardFocusWithin(IInputElement element, Boolean value)
        {
            var visual = element as Visual;

            while (visual != null)
            {
                var uiElement = visual as UIElement;
                if (uiElement != null)
                {
                    uiElement.IsKeyboardFocusWithin = value;
                }

                visual = VisualTreeHelper.GetParent(visual) as Visual;
            }
        }

        /// <summary>
        /// Handles the <see cref="KeyboardDevice.KeyPressed"/> event.
        /// </summary>
        private void keyboard_KeyPressed(IUltravioletWindow window, KeyboardDevice device, Key key, Boolean ctrl, Boolean alt, Boolean shift, Boolean repeat)
        {
            if (!Focused)
                return;

            if (elementWithFocus != null)
            {
                var keyDownHandled = false;

                var dobj = elementWithFocus as DependencyObject;
                if (dobj != null)
                {
                    Keyboard.RaisePreviewKeyDown(dobj, device, key, ctrl, alt, shift, repeat, ref keyDownHandled);
                    Keyboard.RaiseKeyDown(dobj, device, key, ctrl, alt, shift, repeat, ref keyDownHandled);
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="KeyboardDevice.KeyReleased"/> event.
        /// </summary>
        private void keyboard_KeyReleased(IUltravioletWindow window, KeyboardDevice device, Key key)
        {
            if (!Focused)
                return;

            if (elementWithFocus != null)
            {
                var keyUpHandled = false;

                var dobj = elementWithFocus as DependencyObject;
                if (dobj != null)
                {
                    Keyboard.RaisePreviewKeyUp(dobj, device, key, ref keyUpHandled);
                    Keyboard.RaiseKeyUp(dobj, device, key, ref keyUpHandled);
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="KeyboardDevice.TextInput"/> event.
        /// </summary>
        private void keyboard_TextInput(IUltravioletWindow window, KeyboardDevice device)
        {
            if (!Focused)
                return;

            if (elementWithFocus != null)
            {
                var textInputHandled = false;

                var dobj = elementWithFocus as DependencyObject;
                if (dobj != null)
                {
                    Keyboard.RaisePreviewTextInput(dobj, device, ref textInputHandled);
                    Keyboard.RaiseTextInput(dobj, device, ref textInputHandled);
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="MouseDevice.Moved"/> event.
        /// </summary>
        private void mouse_Moved(IUltravioletWindow window, MouseDevice device, Int32 x, Int32 y, Int32 dx, Int32 dy)
        {
            if (window != Window)
                return;

            var recipient = elementWithMouseCapture ?? elementUnderMouse;
            if (recipient != null)
            {
                var dipsX      = Display.PixelsToDips(x);
                var dipsY      = Display.PixelsToDips(y);
                var dipsDeltaX = Display.PixelsToDips(dx);
                var dipsDeltaY = Display.PixelsToDips(dy);

                var mouseMoveHandled = false;

                var dobj = recipient as DependencyObject;
                if (dobj != null)
                {
                    Mouse.RaisePreviewMouseMove(dobj, device, dipsX, dipsY, dipsDeltaX, dipsDeltaY, ref mouseMoveHandled);
                    Mouse.RaiseMouseMove(dobj, device, dipsX, dipsY, dipsDeltaX, dipsDeltaY, ref mouseMoveHandled);
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="MouseDevice.ButtonPressed"/> event.
        /// </summary>
        private void mouse_ButtonPressed(IUltravioletWindow window, MouseDevice device, MouseButton button)
        {
            if (window != Window)
                return;

            var recipient = elementWithMouseCapture;
            if (recipient == null)
            {
                UpdateElementUnderMouse();
                recipient = elementUnderMouse;
            }

            if (recipient != elementWithFocus)
            {
                if (elementWithFocus != null)
                {
                    BlurElement(elementWithFocus);
                }

                if (recipient != null && recipient.Focusable)
                {
                    FocusElement(recipient);
                }
            }

            if (recipient != null)
            {
                var mouseDownHandled = false;

                var dobj = recipient as DependencyObject;
                if (dobj != null)
                {
                    Mouse.RaisePreviewMouseDown(dobj, device, button, ref mouseDownHandled);
                    Mouse.RaiseMouseDown(dobj, device, button, ref mouseDownHandled);
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="MouseDevice.ButtonReleased"/> event.
        /// </summary>
        private void mouse_ButtonReleased(IUltravioletWindow window, MouseDevice device, MouseButton button)
        {
            if (window != Window)
                return;

            var recipient = elementWithMouseCapture ?? elementUnderMouse;
            if (recipient != null)
            {
                var mouseUpHandled = false;

                var dobj = recipient as DependencyObject;
                if (dobj != null)
                {
                    Mouse.RaisePreviewMouseUp(dobj, device, button, ref mouseUpHandled);
                    Mouse.RaiseMouseUp(dobj, device, button, ref mouseUpHandled);
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="MouseDevice.Click"/> event.
        /// </summary>
        private void mouse_Click(IUltravioletWindow window, MouseDevice device, MouseButton button)
        {
            if (window != Window)
                return;

            var recipient = elementWithMouseCapture ?? elementUnderMouse;
            if (recipient != null)
            {
                var mouseClickHandled = false;

                var dobj = recipient as DependencyObject;
                if (dobj != null)
                {
                    Mouse.RaisePreviewMouseClick(dobj, device, button, ref mouseClickHandled);
                    Mouse.RaiseMouseClick(dobj, device, button, ref mouseClickHandled);
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="MouseDevice.DoubleClick"/> event.
        /// </summary>
        private void mouse_DoubleClick(IUltravioletWindow window, MouseDevice device, MouseButton button)
        {
            if (window != Window)
                return;

            var recipient = elementWithMouseCapture ?? elementUnderMouse;
            if (recipient != null)
            {
                var mouseDoubleClickHandled = false;

                var dobj = recipient as DependencyObject;
                if (dobj != null)
                {
                    Mouse.RaisePreviewMouseDoubleClick(dobj, device, button, ref mouseDoubleClickHandled);
                    Mouse.RaiseMouseDoubleClick(dobj, device, button, ref mouseDoubleClickHandled);
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="MouseDevice.WheelScrolled"/> event.
        /// </summary>
        private void mouse_WheelScrolled(IUltravioletWindow window, MouseDevice device, Int32 x, Int32 y)
        {
            if (window != Window)
                return;

            var recipient = elementWithMouseCapture ?? elementUnderMouse;
            if (recipient != null)
            {
                var dipsX = Display.PixelsToDips(x);
                var dipsY = Display.PixelsToDips(y);

                var mouseWheelHandled = false;

                var dobj = recipient as DependencyObject;
                if (dobj != null)
                {
                    Mouse.RaisePreviewMouseWheel(dobj, device, dipsX, dipsY, ref mouseWheelHandled);
                    Mouse.RaiseMouseWheel(dobj, device, dipsX, dipsY, ref mouseWheelHandled);
                }
            }
        }

        // Property values.
        private readonly Namescope namescope;
        private readonly PresentationFoundationViewResources resources;
        private UvssDocument stylesheet;
        private Grid layoutRoot;

        // State values.
        private readonly DrawingContext drawingContext;
        private IInputElement elementUnderMousePrev;
        private IInputElement elementUnderMouse;
        private IInputElement elementWithMouseCapture;
        private IInputElement elementWithFocus;
    }
}
