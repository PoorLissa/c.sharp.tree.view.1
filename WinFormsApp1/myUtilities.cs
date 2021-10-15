
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
};
