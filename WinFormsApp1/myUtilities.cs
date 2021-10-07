
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
};
