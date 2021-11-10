using System;
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

    public static bool charIsLetter(char ch)
    {
        if ((ch >= 65 && ch <= 90) || (ch >= 1040 && ch <= 1071))
            return true;

        if ((ch >= 97 && ch <= 122) || (ch >= 1072 && ch <= 1103))
            return true;

        if (ch == 1025 || ch == 1105)
            return true;

        return false;
    }

    // --------------------------------------------------------------------------------------------------------

    // Extracts numeric characters starting from position [pos] and returns them as an integer
    public static int getInt_fromString(string s, int pos, ref int offset)
    {
        int res = 0;
        offset = 0;
        char ch;

        do
        {
            ch = s[pos++];

            if (!charIsDigit(ch))
                break;

            res *= 10;
            res += (int)(ch - 48);
            offset++;

        } while (pos < s.Length);

        return res;
    }

    // --------------------------------------------------------------------------------------------------------

    public static bool charIsDigit(char ch)
    {
        return (ch > 47 && ch < 58);
    }

    // --------------------------------------------------------------------------------------------------------

    // Converts character to lower case
    // Eng: A =   65, Z =   90, a =   97, z =  122
    // Rus: А = 1040, Я = 1071, а = 1072, я = 1103 + ё = 1105 + Ё = 1025
    public static void charToLowerCase(ref char ch)
    {
        // Eng + Rus
        if ((ch >= 65 && ch <= 90) || (ch >= 1040 && ch <= 1071))
           ch += (char)(32);

        // Rus 'Ё'
        if (ch == 1025)
            ch = (char)(1105);

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Converts character to upper case
    // Eng: A =   65, Z =   90, a =   97, z =  122
    // Rus: А = 1040, Я = 1071, а = 1072, я = 1103 + ё = 1105 + Ё = 1025
    public static void charToUpperCase(ref char ch)
    {
        // Eng + Rus
        if ((ch >= 97 && ch <= 122) || (ch >= 1072 && ch <= 1103))
            ch -= (char)(32);

        // Rus 'Ё'
        if (ch == 1105)
            ch = (char)(1025);

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Checks if string [str] equals to s.Substr(pos, len) -- without extracting the substring
    public static bool fastStrCompare(string str, string s, int pos, int len, bool caseSensitive)
    {
        len = (len < 0) ? s.Length - pos : len;

        if (len != str.Length)
            return false;

        if (caseSensitive)
        {
            for (int i = 0; i < len; i++)
                if (str[i] != s[pos + i])
                    return false;
        }
        else
        {
            for (int i = 0; i < len; i++)
            {
                char ch1 = str[i];
                char ch2 = s[pos+i];

                charToLowerCase(ref ch1);
                charToLowerCase(ref ch2);

                if (ch1 != ch2)
                    return false;
            }
        }

        return true;
    }

    // --------------------------------------------------------------------------------------------------------

    // Checks if s.Substr(pos, len) contains string [str] -- without extracting the substring
    // Eng: A =   65, z =  122
    // Rus: А = 1040, я = 1103
    public static bool fastStrContains(string str, string s, int pos, int len, bool caseSensitive)
    {
        len = (len < 0) ? s.Length - pos : len;

        if (caseSensitive)
        {
            for (int i = 0; i <= len - str.Length; i++)
                if (str[0] == s[pos + i])
                    if (myUtils.fastStrCompare(str, s, pos + i, str.Length, caseSensitive))
                        return true;
        }
        else
        {
            char ch1 = str[0];
            charToLowerCase(ref ch1);

            for (int i = 0; i <= len - str.Length; i++)
            {
                char ch2 = s[pos+i];
                charToLowerCase(ref ch2);

                if (ch1 == ch2)
                    if (myUtils.fastStrCompare(str, s, pos + i, str.Length, caseSensitive))
                        return true;
            }
        }

        return false;
    }

    // --------------------------------------------------------------------------------------------------------

    // Log message
    // Only for simple debugging purposes
    public static void logMsg(string func, string msg)
    {
        string path = "WriteText.txt";
        
        if (!System.IO.File.Exists(path))
        {
            System.IO.File.CreateText(path).Dispose();
        }

        using (System.IO.StreamWriter sw = System.IO.File.AppendText(path))
        {
            sw.WriteLine(func + " : " + msg + "\n");
        }
    }

    // --------------------------------------------------------------------------------------------------------

    public static void strBuilderAppend(StringBuilder sb, string str, int pos, int len = -1)
    {
        len = (len < 0) ? str.Length - pos : len;

        int oldLen = sb.Length;
        int newLen = sb.Length + len;

        if (newLen > sb.Capacity)
            sb.Capacity = newLen * 2;

        sb.Length = newLen;

        for (int i = 0; i < len; i++)
        {
            sb[oldLen + i] = str[pos + i];
        }
    }

    // --------------------------------------------------------------------------------------------------------
};
