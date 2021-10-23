﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

/*
    Misc utilities
*/

public class myUtils
{
    // --------------------------------------------------------------------------------------------------------

    // Gets file name and (absolute or relative) path to a folder.
    // Then tries to resolve the path and return full path to a file
    public static string getFilePath(string dir, string file)
    {
        char slash = '\\';

        if (!dir.EndsWith(slash))
            dir += slash;

        // Try to find the directory
        if (!System.IO.Directory.Exists(dir))
        {
            // Check if the directory is relative or absolute
            if (dir[1] == ':')
            {
                // Absolute
                throw(new System.Exception("Not Implemented"));
            }
            else
            {
                // Relative
                string currentDir = System.AppDomain.CurrentDomain.BaseDirectory;

                if (dir[0] != slash)
                    dir = slash + dir;

                do
                {

                    int index = currentDir.LastIndexOf(slash);
                    currentDir = index > 0 ? currentDir.Substring(0, index) : "";

                }
                while (currentDir.Length > 0 && !System.IO.Directory.Exists(currentDir + dir));

                dir = currentDir + dir;
            }
        }

        return dir + file;
    }

    // --------------------------------------------------------------------------------------------------------

    public static System.Drawing.Drawing2D.GraphicsPath RoundedRect(System.Drawing.Rectangle bounds, int radius)
    {
        int diameter = radius * 2;
        var size = new System.Drawing.Size(diameter, diameter);
        var arc = new System.Drawing.Rectangle(bounds.Location, size);
        System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

        if (radius == 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        // top left arc  
        path.AddArc(arc, 180, 90);

        // top right arc  
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);

        // bottom right arc  
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);

        // bottom left arc 
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();
        return path;
    }

    // --------------------------------------------------------------------------------------------------------

    public static void DrawRoundedRectangle(System.Drawing.Graphics graphics, System.Drawing.Pen pen, System.Drawing.Rectangle bounds, int cornerRadius)
    {
        using (System.Drawing.Drawing2D.GraphicsPath path = RoundedRect(bounds, cornerRadius))
        {
            graphics.DrawPath(pen, path);
        }
    }

    // --------------------------------------------------------------------------------------------------------

    // Change the opacity of an image
    // https://stackoverflow.com/questions/4779027/changing-the-opacity-of-a-bitmap-image
    public static Image ChangeImageOpacity(Image originalImage, double opacity)
    {
        const int bytesPerPixel = 4;

        // Cannot modify an image with indexed colors
        if ((originalImage.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
        {
            return originalImage;
        }

        Bitmap bmp = (Bitmap)originalImage.Clone();

        // Specify a pixel format
        PixelFormat pxf = PixelFormat.Format32bppArgb;

        // Lock the bitmap's bits
        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);

        // Get the address of the first line
        IntPtr ptr = bmpData.Scan0;

        // Declare an array to hold the bytes of the bitmap.
        // This code is specific to a bitmap with 32 bits per pixels (32 bits = 4 bytes, 3 for RGB and 1 byte for alpha)
        int numBytes = bmp.Width * bmp.Height * bytesPerPixel;
        byte[] argbValues = new byte[numBytes];

        // Copy the ARGB values into the array
        System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, numBytes);

        // Manipulate the bitmap, such as changing the RGB values for all pixels in the the bitmap
        // (argbValues is in format BGRA (Blue, Green, Red, Alpha))
        for (int counter = 0; counter < argbValues.Length; counter += bytesPerPixel)
        {
            // If 100% transparent, skip pixel
            if (argbValues[counter + bytesPerPixel - 1] == 0)
                continue;

            int pos = 0;
            pos++; // B value
            pos++; // G value
            pos++; // R value

            argbValues[counter + pos] = (byte)(argbValues[counter + pos] * opacity);
        }

        // Copy the ARGB values back to the bitmap
        System.Runtime.InteropServices.Marshal.Copy(argbValues, 0, ptr, numBytes);

        // Unlock the bits
        bmp.UnlockBits(bmpData);

        return bmp;
    }

    // --------------------------------------------------------------------------------------------------------

    // Shortens the path in a way it fits the rectangle
    public static string condensePath(DataGridViewCellPaintingEventArgs e, string path, Rectangle r, Font f, StringFormat format)
    {
        int pos = path.LastIndexOf('\\') + 1;

        StringBuilder sb = new StringBuilder(path, 0, pos, 0);

        SizeF size = e.Graphics.MeasureString(sb.ToString(), f, r.Width * 2, format);

        bool ok = size.Width <= r.Width;

        // Repeat while string does not fit the rectangle
        for (int maxlen = 10; !ok && maxlen != 7; maxlen--)
        {
            while (!ok && shortenPathNodes(sb, maxlen))
            {
                size = e.Graphics.MeasureString(sb.ToString(), f, r.Width * 2, format);
                ok = (size.Width <= r.Width);
            }
        }

        if (!ok)
        {
            sb.Insert(0, "...");

            // Skip "c:\\"
            for (pos = 0; pos < sb.Length; pos++)
                if (sb[pos + 3] == '\\')
                    break;
            sb.Remove(3, pos);

            do
            {
                for (pos = 1; pos < sb.Length; pos++)
                    if (sb[pos + 3] == '\\')
                        break;

                sb.Remove(3, pos);

                size = e.Graphics.MeasureString(sb.ToString(), f, r.Width * 2, format);

            } while (size.Width > r.Width);
        }

        return sb.ToString();
    }

    // --------------------------------------------------------------------------------------------------------

    // Finds the longest part in the path enclosed between back slashes.
    // Reduces its length: "c:\aaa\bbb\1234567890_abc\ccc\" => "c:\aaa\bbb\1234567...\ccc\"
    // Returns true if it was able to reduce the length of the path
    private static bool shortenPathNodes(StringBuilder sb, int maxLen = 10)
    {
        int beg = -1, end = -1, pos = 0, len = 0;

        for (int i = 0; i < sb.Length; i++)
        {
            if (sb[i] == '\\')
            {
                if (end == -1)
                {
                    beg = i;
                    end = i;
                }
                else
                {
                    end = i;

                    int Len = i - beg;

                    if (Len > len)
                    {
                        len = Len;
                        pos = beg;
                    }

                    beg = i;
                }
            }
        }

        if (len > maxLen + 1)
        {
            sb.Remove(pos + maxLen - 2, len - maxLen + 2);
            sb.Insert(pos + maxLen - 2, "...");
            return true;
        }

        return false;
    }

    // --------------------------------------------------------------------------------------------------------

    public static void logMsg(string func, string msg)
    {
        string path = "WriteText.txt";
        
        if (!System.IO.File.Exists(path))
        {
            System.IO.File.CreateText(path);
        }

        using (System.IO.StreamWriter sw = System.IO.File.AppendText(path))
        {
            sw.WriteLine(func + " : " + msg + "\n");
        }
    }

    // --------------------------------------------------------------------------------------------------------
};
