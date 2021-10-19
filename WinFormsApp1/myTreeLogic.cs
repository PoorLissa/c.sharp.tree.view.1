using System;
using System.Collections.Generic;
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
}
