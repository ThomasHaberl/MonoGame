// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if WINDOWS && DIRECTX
using SharpDX.Multimedia;
using SharpDX.RawInput;
#endif

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Allows reading position and button click information from mouse.
    /// </summary>
    public static partial class Mouse
    {
        internal static GameWindow PrimaryWindow;

        private static readonly MouseState _defaultState = new MouseState();

        // Fields for relative mouse movement info:
        // For a first person shooter, the mouse position info needs to be relative and not be 
        // limited by the screen border. This can be achieved by calling SetPosition() each frame
        // and measuring the position change between the current pos and the pos specified in 
        // SetPosition(). However, this is not accurate when the frame rate is high (>>60 Hz).
        // (Mouse seems to be slow.) In WINDOWS && DIRECTX we can use the raw mouse device for
        // accurate mouse movement. (See also SetPosition() and MouseState.DeltaX/DeltaY.)
        private static int _defaultPositionX;
        private static int _defaultPositionY;
        private static bool _isRelative;
#if (WINDOWS && DIRECTX) || (WINDOWS_STOREAPP && !WINDOWS_PHONE81) || WINDOWS_UAP
        private static int _deltaX;
        private static int _deltaY;
#endif
        /// <summary>
        /// Gets or sets a value indicating whether the game requires relative mouse movement data
        /// instead of absolute mouse movement data.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the game requires relative mouse movement data; otherwise, 
        /// <see langword="false" />.
        /// </value>
        /// <remarks>
        /// Absolute mouse movement is used in desktop applications. Relative mouse movement is 
        /// required, for example, for first person shooter controls. When <see cref="IsRelative"/> 
        /// is <see langword="true"/>, the mouse cursor is not limited by the screen boundaries,
        /// and <see cref="MouseState.DeltaX"/> and <see cref="MouseState.DeltaY"/> can be used
        /// to get the relative mouse movement since the last <see cref="SetPosition"/> call.
        /// </remarks>
        public static bool IsRelative
        {
            get { return _isRelative; }
            set
            {
                _isRelative = value;
#if (WINDOWS && DIRECTX) || (WINDOWS_STOREAPP && !WINDOWS_PHONE81) || WINDOWS_UAP
                if (_isRelative)
                {
#if WINDOWS && DIRECTX
                    // Register the raw mouse device once, and never unregister the device or event.
                    Device.RegisterDevice(UsagePage.Generic, UsageId.GenericMouse, DeviceFlags.InputSink, WindowHandle);
                    Device.MouseInput += OnRawMouseInput;
#elif WINDOWS_STOREAPP || WINDOWS_UAP
                    MouseDevice.GetForCurrentView().MouseMoved += OnWinStoreMouseMoved;
#endif
                }
                else
                {
#if WINDOWS && DIRECTX
                    Device.MouseInput -= OnRawMouseInput;
#elif WINDOWS_STOREAPP || WINDOWS_UAP
                    // We have to unregister the event handler because while the event is handled 
                    // the mouse behaves differently than the default absolute mouse.
                    MouseDevice.GetForCurrentView().MouseMoved -= OnWinStoreMouseMoved;
#endif
                }
#endif
            }
        }

        /// <summary>
        /// Gets or sets the window handle for current mouse processing.
        /// </summary> 
        public static IntPtr WindowHandle
        {
            get { return PlatformGetWindowHandle(); }
            set { PlatformSetWindowHandle(value); }
        }

        /// <summary>
        /// This API is an extension to XNA.
        /// Gets mouse state information that includes position and button
        /// presses for the provided window
        /// </summary>
        /// <returns>Current state of the mouse.</returns>
        public static MouseState GetState(GameWindow window)
        {
#if (WINDOWS && DIRECTX) || (WINDOWS_STOREAPP && !WINDOWS_PHONE81) || WINDOWS_UAP
            window.MouseState.DeltaX = _deltaX;
            window.MouseState.DeltaY = _deltaY;
#else
            window.MouseState.DeltaX = window.MouseState.X - _defaultPositionX;
            window.MouseState.DeltaY = window.MouseState.Y - _defaultPositionY;
#endif

            return PlatformGetState(window);
        }

        /// <summary>
        /// Gets mouse state information that includes position and button presses
        /// for the primary window
        /// </summary>
        /// <returns>Current state of the mouse.</returns>
        public static MouseState GetState()
        {
            if (PrimaryWindow != null)
                return GetState(PrimaryWindow);

            return _defaultState;
        }

        /// <summary>
        /// Sets mouse cursor's relative position to game-window.
        /// </summary>
        /// <param name="x">Relative horizontal position of the cursor.</param>
        /// <param name="y">Relative vertical position of the cursor.</param>
        public static void SetPosition(int x, int y)
        {
            _defaultPositionX = x;
            _defaultPositionY = y;
#if (WINDOWS && DIRECTX) || (WINDOWS_STOREAPP && !WINDOWS_PHONE81) || WINDOWS_UAP
            _deltaX = 0;
            _deltaY = 0;
#endif

            PlatformSetPosition(x, y);
        }

        /// <summary>
        /// Sets the cursor image to the specified MouseCursor.
        /// </summary>
        /// <param name="cursor">Mouse cursor to use for the cursor image.</param>
        public static void SetCursor(MouseCursor cursor)
        {
            PlatformSetCursor(cursor);
        }


#if WINDOWS && DIRECTX
        private static void OnRawMouseInput(object sender, MouseInputEventArgs mouseEventArgs)
        {
            _deltaX += mouseEventArgs.X;
            _deltaY += mouseEventArgs.Y;
        }
#endif
    }
}
