using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;   // For dll import

/*
    Helper class for myTree class.
    Provides file/directory operations logic, tree traversal logic, etc.
*/
public class myTreeLogic
{
    // Checks if the directory contains at least one subdirectory
    //  Prerequisites:
    //      - Path parameter must be ending with "*.*"
    [DllImport("cpp.helper.dll", CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool dirHasSubdirs(string dir);

    // --------------------------------------------------------------------------------------------------------

    // Wrapper for c++ function dirHasSubdirs, imported from cpp.helper.dll
    // Checks if the directory contains at least one subdirectory
    public bool folderHasSubfolders(string dir)
    {
        return dirHasSubdirs(dir + "\\*.*");
    }

    // --------------------------------------------------------------------------------------------------------

    public int getFiles(string dir, List<myTreeListDataItem> listExt, bool doClear = true)
    {
        int res = 0;

        if (doClear)
            listExt.Clear();

        if (System.IO.Directory.Exists(dir))
        {
            string[] files = null;

            try
            {
                files = System.IO.Directory.GetFiles(dir);
            }
            catch (Exception e)
            {
                string s = e.Message;
                res++;
            }

            if (files != null)
            {
                foreach (var file in files)
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);

                    bool isHidden = (fi.Attributes & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden;
                    bool isSystem = (fi.Attributes & System.IO.FileAttributes.System) == System.IO.FileAttributes.System;

                    var File = new myTreeListDataItem(file);
                    File.isDir = false;
                    File.isHidden = isHidden || isSystem;
                    listExt.Add(File);
                }

            }
        }

        return res;
    }

    // --------------------------------------------------------------------------------------------------------

    public int getDirectories(string dir, List<myTreeListDataItem> listExt, bool doClear = true)
    {
        int res = 0;

        if (doClear)
            listExt.Clear();

        if (System.IO.Directory.Exists(dir))
        {
            string[] dirs = null;

            try
            {
                dirs = System.IO.Directory.GetDirectories(dir);
            }
            catch (Exception e)
            {
                string s = e.Message;
                res++;
            }

            if (dirs != null)
            {
                foreach (var d in dirs)
                {
                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(d);

                    bool isHidden = (di.Attributes & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden;
                    bool isSystem = (di.Attributes & System.IO.FileAttributes.System) == System.IO.FileAttributes.System;

                    var Dir = new myTreeListDataItem(d);
                    Dir.isDir = true;
                    Dir.isHidden = isHidden || isSystem;
                    listExt.Add(Dir);
                }
            }
        }

        return res;
    }

    // --------------------------------------------------------------------------------------------------------

    public static void TraverseTree(string root, RichTextBox tb, TreeView tv)
    {
        // Data structure to hold names of subfolders to be examined for files
        var dirs = new Stack<string>(20);

        if (!System.IO.Directory.Exists(root))
        {
            throw new ArgumentException();
        }

        dirs.Push(root);

        while (dirs.Count > 0)
        {
            string currentDir = dirs.Pop();
            string[] subDirs;

            try
            {
                subDirs = System.IO.Directory.GetDirectories(currentDir);
            }
            // An UnauthorizedAccessException exception will be thrown if we do not have
            // discovery permission on a folder or file. It may or may not be acceptable
            // to ignore the exception and continue enumerating the remaining files and
            // folders. It is also possible (but unlikely) that a DirectoryNotFound exception
            // will be raised. This will happen if currentDir has been deleted by
            // another application or thread after our call to Directory.Exists. The
            // choice of which exceptions to catch depends entirely on the specific task
            // you are intending to perform and also on how much you know with certainty
            // about the systems on which this code will run.
            catch (UnauthorizedAccessException e)
            {
                tb.Text += e.Message + "\n";
                continue;
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                tb.Text += e.Message + "\n";
                continue;
            }

            string[] files = null;
            try
            {
                files = System.IO.Directory.GetFiles(currentDir);
            }
            catch (UnauthorizedAccessException e)
            {
                tb.Text += e.Message + "\n";
                continue;
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                tb.Text += e.Message + "\n";
                continue;
            }

            // Push the subdirectories onto the stack for traversal.
            // This could also be done before handing the files.
            foreach (string dir in subDirs)
            {
                tv.Nodes.Add(dir, dir.Substring(dir.LastIndexOf('\\') + 1));
                //dirs.Push(str);
            }

            // Perform the required action on each file here.
            // Modify this block to perform your required task.
            foreach (string file in files)
            {
                try
                {
                    // Perform whatever action is required in your scenario.
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);
                    tb.Text += $"{fi.Name}: {fi.Length}, {fi.CreationTime}\n";
                }
                catch (System.IO.FileNotFoundException e)
                {
                    // If file was deleted by a separate application
                    //  or thread since the call to TraverseTree()
                    // then just continue.
                    tb.Text += e.Message + "\n";
                    continue;
                }
            }
        }
    }

    // --------------------------------------------------------------------------------------------------------

    // Returns the longest subpath that is a valid existing path
    public void getLastValidPath(ref string path)
    {
        while (path.Length > 0 && !System.IO.Directory.Exists(path))
        {
            int index = path.LastIndexOf("\\");
            path = index > 0 ? path.Substring(0, index) : "";
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Cut the leftmost part of the path and return it
    public string getLeftmostPartFromPath(ref string path)
    {
        string res = "";

        int index = path.IndexOf("\\");

        if (index > 0)
        {
            res = path.Substring(0, index);
            path = path.Substring(index + 1);
        }
        else
        {
            res = path;
            path = "";
        }

        return res;
    }

    // --------------------------------------------------------------------------------------------------------

    // Change the opacity of an image
    // https://stackoverflow.com/questions/4779027/changing-the-opacity-of-a-bitmap-image
    public Image ChangeImageOpacity(Image originalImage, double opacity)
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
}
