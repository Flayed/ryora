using Ryora.Client.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Ryora.Client.Services.Implementation
{
    public class BitBlitScreenshotService : IScreenshotService
    {
        #region P/Invoke
        /// <summary>
        /// Receives a handle to the display device context (DC)
        /// </summary>
        /// <param name="hWnd">Handle to the window whose DC is to be retrieved.  If this value is NULL, retrieves the DC for the entire screen.</param>
        /// <param name="hrgnClip">Specifies a clipping region that may be combined with the visible region of the DC.  If the value of the flags is DCX_INTERSECTION or DCX_EXCLUDERGN then the operating system assumes ownership of the region and will automatically delete it when it is no longer needed.  In this case, applications should not use the region - not even delete it - after a successful call.</param>
        /// <param name="flags">Specifies how the DC is created.</param>
        /// <returns>A handle to the display device context (DC)</returns>
        [DllImport("user32.dll")]
        static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, DeviceContextValues flags);

        /// <summary>Deletes the specified device context (DC).</summary>
        /// <param name="hdc">A handle to the device context.</param>
        /// <returns><para>If the function succeeds, the return value is nonzero.</para><para>If the function fails, the return value is zero.</para></returns>
        /// <remarks>An application must not delete a DC whose handle was obtained by calling the <c>GetDC</c> function. Instead, it must call the <c>ReleaseDC</c> function to free the DC.</remarks>
        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern bool DeleteDC([In] IntPtr hdc);

        /// <summary>Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system resources associated with the object. After the object is deleted, the specified handle is no longer valid.</summary>
        /// <param name="hObject">A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
        /// <returns>
        ///   <para>If the function succeeds, the return value is nonzero.</para>
        ///   <para>If the specified handle is not valid or is currently selected into a DC, the return value is zero.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Do not delete a drawing object (pen or brush) while it is still selected into a DC.</para>
        ///   <para>When a pattern brush is deleted, the bitmap associated with the brush is not deleted. The bitmap must be deleted independently.</para>
        /// </remarks>
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        /// <summary>
        ///        Creates a memory device context (DC) compatible with the specified device.
        /// </summary>
        /// <param name="hdc">A handle to an existing DC. If this handle is NULL,
        ///        the function creates a memory DC compatible with the application's current screen.</param>
        /// <returns>
        ///        If the function succeeds, the return value is the handle to a memory DC.
        ///        If the function fails, the return value is <see cref="System.IntPtr.Zero"/>.
        /// </returns>
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
        static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);

        /// <summary>
        ///        Creates a bitmap compatible with the device that is associated with the specified device context.
        /// </summary>
        /// <param name="hdc">A handle to a device context.</param>
        /// <param name="nWidth">The bitmap width, in pixels.</param>
        /// <param name="nHeight">The bitmap height, in pixels.</param>
        /// <returns>If the function succeeds, the return value is a handle to the compatible bitmap (DDB). If the function fails, the return value is <see cref="System.IntPtr.Zero"/>.</returns>
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        static extern IntPtr CreateCompatibleBitmap([In] IntPtr hdc, int nWidth, int nHeight);

        /// <summary>Selects an object into the specified device context (DC). The new object replaces the previous object of the same type.</summary>
        /// <param name="hdc">A handle to the DC.</param>
        /// <param name="hgdiobj">A handle to the object to be selected.</param>
        /// <returns>
        ///   <para>If the selected object is not a region and the function succeeds, the return value is a handle to the object being replaced. If the selected object is a region and the function succeeds, the return value is one of the following values.</para>
        ///   <para>SIMPLEREGION - Region consists of a single rectangle.</para>
        ///   <para>COMPLEXREGION - Region consists of more than one rectangle.</para>
        ///   <para>NULLREGION - Region is empty.</para>
        ///   <para>If an error occurs and the selected object is not a region, the return value is <c>NULL</c>. Otherwise, it is <c>HGDI_ERROR</c>.</para>
        /// </returns>
        /// <remarks>
        ///   <para>This function returns the previously selected object of the specified type. An application should always replace a new object with the original, default object after it has finished drawing with the new object.</para>
        ///   <para>An application cannot select a single bitmap into more than one DC at a time.</para>
        ///   <para>ICM: If the object being selected is a brush or a pen, color management is performed.</para>
        /// </remarks>
        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject([In] IntPtr hdc, [In] IntPtr hgdiobj);

        /// <summary>
        ///    Performs a bit-block transfer of the color data corresponding to a
        ///    rectangle of pixels from the specified source device context into
        ///    a destination device context.
        /// </summary>
        /// <param name="hdc">Handle to the destination device context.</param>
        /// <param name="nXDest">The leftmost x-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nYDest">The topmost y-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nWidth">The width of the source and destination rectangles (in pixels).</param>
        /// <param name="nHeight">The height of the source and the destination rectangles (in pixels).</param>
        /// <param name="hdcSrc">Handle to the source device context.</param>
        /// <param name="nXSrc">The leftmost x-coordinate of the source rectangle (in pixels).</param>
        /// <param name="nYSrc">The topmost y-coordinate of the source rectangle (in pixels).</param>
        /// <param name="dwRop">A raster-operation code.</param>
        /// <returns>
        ///    <c>true</c> if the operation succeedes, <c>false</c> otherwise. To get extended error information, call <see cref="System.Runtime.InteropServices.Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(IntPtr b1, IntPtr b2, long count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static unsafe extern int memcpy(byte* dest, byte* src, long count);

        /// <summary>
        ///     Specifies a raster-operation code. These codes define how the color data for the
        ///     source rectangle is to be combined with the color data for the destination
        ///     rectangle to achieve the final color.
        /// </summary>
        enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062,
            /// <summary>
            /// Capture window as seen on screen.  This includes layered windows 
            /// such as WPF windows with AllowsTransparency="true"
            /// </summary>
            CAPTUREBLT = 0x40000000
        }

        /// <summary>Values to pass to the GetDCEx method.</summary>
        [Flags()]
        private enum DeviceContextValues : uint
        {
            /// <summary>DCX_WINDOW: Returns a DC that corresponds to the window rectangle rather 
            /// than the client rectangle.</summary>
            Window = 0x00000001,

            /// <summary>DCX_CACHE: Returns a DC from the cache, rather than the OWNDC or CLASSDC 
            /// window. Essentially overrides CS_OWNDC and CS_CLASSDC.</summary>
            Cache = 0x00000002,

            /// <summary>DCX_NORESETATTRS: Does not reset the attributes of this DC to the 
            /// default attributes when this DC is released.</summary>
            NoResetAttrs = 0x00000004,

            /// <summary>DCX_CLIPCHILDREN: Excludes the visible regions of all child windows 
            /// below the window identified by hWnd.</summary>
            ClipChildren = 0x00000008,

            /// <summary>DCX_CLIPSIBLINGS: Excludes the visible regions of all sibling windows 
            /// above the window identified by hWnd.</summary>
            ClipSiblings = 0x00000010,

            /// <summary>DCX_PARENTCLIP: Uses the visible region of the parent window. The 
            /// parent's WS_CLIPCHILDREN and CS_PARENTDC style bits are ignored. The origin is 
            /// set to the upper-left corner of the window identified by hWnd.</summary>
            ParentClip = 0x00000020,

            /// <summary>DCX_EXCLUDERGN: The clipping region identified by hrgnClip is excluded 
            /// from the visible region of the returned DC.</summary>
            ExcludeRgn = 0x00000040,

            /// <summary>DCX_INTERSECTRGN: The clipping region identified by hrgnClip is 
            /// intersected with the visible region of the returned DC.</summary>
            IntersectRgn = 0x00000080,

            /// <summary>DCX_EXCLUDEUPDATE: Unknown...Undocumented</summary>
            ExcludeUpdate = 0x00000100,

            /// <summary>DCX_INTERSECTUPDATE: Unknown...Undocumented</summary>
            IntersectUpdate = 0x00000200,

            /// <summary>DCX_LOCKWINDOWUPDATE: Allows drawing even if there is a LockWindowUpdate 
            /// call in effect that would otherwise exclude this window. Used for drawing during 
            /// tracking.</summary>
            LockWindowUpdate = 0x00000400,

            /// <summary>DCX_USESTYLE: Undocumented, something related to WM_NCPAINT message.</summary>
            UseStyle = 0x00010000,

            /// <summary>DCX_VALIDATE When specified with DCX_INTERSECTUPDATE, causes the DC to 
            /// be completely validated. Using this function with both DCX_INTERSECTUPDATE and 
            /// DCX_VALIDATE is identical to using the BeginPaint function.</summary>
            Validate = 0x00200000,
        }
        #endregion  

        private Bitmap PreviousScreen { get; set; }

        public BitBlitScreenshotService()
        {
            PreviousScreen = null;
        }

        public ScreenUpdate GetUpdate()
        {
            var newScreenshot = TakeScreenshot();
            if (PreviousScreen == null)
            {
                PreviousScreen = newScreenshot;
                return new ScreenUpdate(0, 0, 1920, 1080, newScreenshot);
            }

            var difference = GetDifferenceRectangle(PreviousScreen, newScreenshot);
            if (!difference.HasValue) return null;

            PreviousScreen = newScreenshot;
            var differenceBitmap = CropBitmap(newScreenshot, difference.Value);
            return new ScreenUpdate(difference.Value, differenceBitmap);
        }

        public void ForceUpdate(Rectangle updateRectangle)
        {
            //Cache.ForceUpdate(updateRectangle);
        }

        private Bitmap TakeScreenshot(Rectangle bounds)
        {
            return TakeScreenshot(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        private Bitmap TakeScreenshot(int x = 0, int y = 0, int screenWidth = 1920, int screenHeight = 1080)
        {
            IntPtr screenDc = IntPtr.Zero;
            IntPtr memoryDc = IntPtr.Zero;
            try
            {
                screenDc = GetDCEx(IntPtr.Zero, IntPtr.Zero, 0);
                memoryDc = CreateCompatibleDC(screenDc);

                var hBitmap = CreateCompatibleBitmap(screenDc, screenWidth, screenHeight);
                var oldBitmap = SelectObject(memoryDc, hBitmap);

                BitBlt(memoryDc, 0, 0, screenWidth, screenHeight, screenDc, x, y, TernaryRasterOperations.SRCCOPY);

                hBitmap = SelectObject(memoryDc, oldBitmap);

                var bmp = Image.FromHbitmap(hBitmap);

                DeleteObject(hBitmap);
                DeleteObject(oldBitmap);

                return bmp;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (screenDc != IntPtr.Zero)
                    DeleteDC(screenDc);
                if (memoryDc != IntPtr.Zero)
                    DeleteDC(memoryDc);
            }
        }

        private unsafe Rectangle? GetDifferenceRectangle(Bitmap first, Bitmap second)
        {
            var fbmd = first.LockBits(new Rectangle(0, 0, first.Width, first.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var sbmd = second.LockBits(new Rectangle(0, 0, second.Width, second.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var rowLength = fbmd.Width * 4;

            var minX = int.MaxValue;
            var minY = -1;
            var maxX = -1;
            var maxY = -1;

            try
            {
                for (var y = 0; y < first.Height && minY < 0; y++)
                {
                    byte* fp = (byte*)fbmd.Scan0 + (y * rowLength);
                    byte* sp = (byte*)sbmd.Scan0 + (y * rowLength);

                    if (memcmp((IntPtr)fp, (IntPtr)sp, rowLength) != 0)
                    {
                        minY = y;
                    }
                }

                for (var y = first.Height - 1; y >= 0 && maxY < 0; y--)
                {
                    byte* fp = (byte*)fbmd.Scan0 + (y * rowLength);
                    byte* sp = (byte*)sbmd.Scan0 + (y * rowLength);

                    if (memcmp((IntPtr)fp, (IntPtr)sp, rowLength) != 0)
                    {
                        maxY = y;
                    }
                }
                if (minY >= 0)
                {
                    for (var y = minY; y <= maxY; y++)
                    {
                        for (var x = 0; x < rowLength && x < minX; x++)
                        {
                            byte* fp = (byte*)fbmd.Scan0 + (y * rowLength);
                            byte* sp = (byte*)sbmd.Scan0 + (y * rowLength);

                            var d = (*(fp + x) ^ *(sp + x));
                            if (d != 0 && x < minX)
                            {
                                minX = x;
                            }
                        }
                        for (var x = rowLength - 1; x >= 0 && x > maxX; x--)
                        {
                            byte* fp = (byte*)fbmd.Scan0 + (y * rowLength);
                            byte* sp = (byte*)sbmd.Scan0 + (y * rowLength);

                            var d = (*(fp + x) ^ *(sp + x));
                            if (d != 0 && x > maxX)
                            {
                                maxX = x;
                            }
                        }
                        if (minX == 0 && maxX == fbmd.Width)
                            break;
                    }
                }

                minX = minX / 4;
                maxX = maxX / 4;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something bad happened: {ex.Message}");
                return null;
            }
            finally
            {
                first.UnlockBits(fbmd);
                second.UnlockBits(sbmd);
            }

            if (minY == -1) return null;

            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        private Bitmap CropBitmap(Bitmap sourceImage, Rectangle rectangle)
        {
            var croppedImage = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppArgb);

            var sourceBitmapdata = sourceImage.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var croppedBitmapData = croppedImage.LockBits(new Rectangle(0, 0, rectangle.Width, rectangle.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                croppedBitmapData.Stride = sourceBitmapdata.Stride;
                byte* sourceImagePointer = (byte*)sourceBitmapdata.Scan0;
                byte* croppedImagePointer = (byte*)croppedBitmapData.Scan0;
                memcpy(croppedImagePointer, sourceImagePointer,
                       Math.Abs(croppedBitmapData.Stride) * rectangle.Height);
            }

            sourceImage.UnlockBits(sourceBitmapdata);
            croppedImage.UnlockBits(croppedBitmapData);

            return croppedImage;
        }
    }
}
