using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

/*
    Wrapper class around TreeView widget.
    Allows customization and provides public methods to work with the widget.
*/
public class myTree
{
    private TreeView    _tree        = null;
    private myTreeLogic _logic       = null;
    private float       _fontSize    = 0.0f;
    private Color       _foreColor;

    private System.Drawing.Brush _treeBackBrush      = null;
    private System.Drawing.Brush _treeGradientBrush1 = null;
    private System.Drawing.Brush _treeGradientBrush2 = null;
    private System.Drawing.Brush _treeGradientBrush3 = null;

    private Image _imgPlus      = null;
    private Image _imgMinus     = null;
    private Image _imgDir       = null;
    private Image _imgDirOpened = null;
    private Image _imgHDD       = null;

    private bool _doUseDummies   = false;
    private bool _allowRedrawing = false;

    private Font [] _nodeFonts = null;

    private int _winVer = 0;

    // --------------------------------------------------------------------------------------------------------

    public myTree(TreeView tv, string path, bool expandEmpty)
    {
        _tree  = tv;
        _logic = new myTreeLogic();

        _foreColor = tv.ForeColor;
        _fontSize  = tv.Font.Size;

        _doUseDummies   = false;
        _allowRedrawing = true;

        init();
        setPath(path, useDummies: true, expandEmpty: expandEmpty);

        _winVer = Environment.OSVersion.Version.Major * 10 + Environment.OSVersion.Version.Minor;
    }

    // --------------------------------------------------------------------------------------------------------

    private void init()
    {
        if (_tree != null)
        {
            setDoubleBuffering();

            _tree.Indent        = 30;
            _tree.ItemHeight    = 40;
            _tree.HideSelection = false;
            _tree.HotTracking   = true;
            _tree.FullRowSelect = true;
            _tree.ShowLines     = false;        // The ShowLines property must be false for the FullRowSelect property to work

            createDrawingPrimitives();

            // Configure the TreeView control for owner-draw and add a handler for the DrawNode event
            _tree.DrawMode  = TreeViewDrawMode.OwnerDrawAll;
            _tree.DrawNode += new DrawTreeNodeEventHandler(myTree_DrawNode);

            // Populate the first level of the tree
            foreach (var drive in System.Environment.GetLogicalDrives())
            {
                TreeNode newNode = _tree.Nodes.Add(drive, "Drive " + drive.Substring(0, 2));

                newNode.NodeFont = getNodeFont(0);

                // Each yet unopened node will contain this secret node
                newNode.Nodes.Add("[?]", "[?]");
            }
        }
    }

    // --------------------------------------------------------------------------------------------------------

    private void createDrawingPrimitives()
    {
        // Load images
        try
        {
            _imgMinus = Image.FromFile(myUtils.getFilePath("_icons", "icon-tree-node-2-minus-32.png"));
            _imgPlus = Image.FromFile(myUtils.getFilePath("_icons", "icon-tree-node-2-plus-32.png"));
            _imgHDD = Image.FromFile(myUtils.getFilePath("_icons", "icon-hdd-1-48.png"));
            _imgDir = Image.FromFile(myUtils.getFilePath("_icons", "icon-tree-folder-1-closed-30.png"));
            _imgDirOpened = Image.FromFile(myUtils.getFilePath("_icons", "icon-tree-folder-1-opened-30.png"));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "myTree: Failed to load images", MessageBoxButtons.OK);
        }

        // Create brushes, gradients, etc.
        _treeBackBrush = new System.Drawing.SolidBrush(_tree.BackColor);

        var pt1 = new Point(0, 0);
        var pt2 = new Point(0, _tree.ItemHeight);

        _treeGradientBrush1 = new System.Drawing.Drawing2D.LinearGradientBrush(pt1, pt2,
                                    Color.FromArgb(150, 204, 232, 255),
                                    Color.FromArgb(255, 190, 220, 255));

        _treeGradientBrush2 = new System.Drawing.Drawing2D.LinearGradientBrush(pt1, pt2,
                                    Color.FromArgb(33, 204, 232, 255),
                                    Color.FromArgb(100, 204, 232, 255));

        _treeGradientBrush3 = new System.Drawing.Drawing2D.LinearGradientBrush(pt1, pt2,
                                    Color.FromArgb(100, 204, 232, 255),
                                    Color.FromArgb(250, 204, 232, 255));
        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Custom drawing function for a node
    private void myTree_DrawNode(object sender, DrawTreeNodeEventArgs e)
    {
        if (_allowRedrawing)
        {
            // Retrieve the node font. If it has not been set, use the TreeView font instead
            Font nodeFont = (e.Node.NodeFont == null) ? ((TreeView)sender).Font : e.Node.NodeFont;

            bool isNodeExpanded = e.Node.IsExpanded;
            bool isNodeEmpty    = e.Node.Nodes.Count == 0;
            bool isNodeHovered  = (e.State & TreeNodeStates.Hot) != 0;
            bool isNodeClicked  = (e.State & TreeNodeStates.Focused) != 0;
            bool doErase        = false;
            bool doDrawIcon     = true;
            Image expandImg     = null;

            System.Drawing.Brush backBrush = _treeBackBrush;
            System.Drawing.Brush fontBrush = Brushes.Black;

            int x = 0, y = 0, gradientBrush = 0;
            int yAdjustment     = 0;
            int xAdjustmentText = 0;
            int xAdjustmentBgr  = 0;
            int yAdjustmentBgr  = 0;
            int xLeftMargin     = 40;
            int drawWidth       = _tree.ClientRectangle.Right - e.Node.Bounds.X;

            // Don't let the selection rectangle be shorter than the node's text (Doesn't work in Win7)
            {
                if (_winVer > 90)
                {
                    if (e.Node.Bounds.Right >= _tree.ClientRectangle.Right)
                        drawWidth += 10;
                }
                else
                {
                    if (_tree.ClientRectangle.Right - e.Node.Bounds.Right < 37)
                        drawWidth += 10;
                }
            }

            // The node that was selected before the user selected another one
            if ((e.State & TreeNodeStates.Selected) != 0)
            {
                xAdjustmentText += xAdjustmentText == 0 ? 0 : 1;
                backBrush = Brushes.AliceBlue;
            }

            if (isNodeHovered || isNodeClicked)
            {
                doErase = true;

                // The node the mouse is hovering upon
                if (isNodeHovered)
                {
                    xAdjustmentText = 2;
                    gradientBrush = 2;
                }

                // Clicked node
                if (isNodeClicked)
                    gradientBrush = 3;

                if (isNodeHovered && isNodeClicked)
                    gradientBrush = 1;
            }

            // Draw the background of the selected node
            {
                // Erase the text, as we're going to shift it a bit, and it will left some trace otherwise
                if (doErase)
                {
                    x = e.Node.Bounds.X;
                    y = e.Node.Bounds.Y;
                    e.Graphics.FillRectangle(_treeBackBrush, x, y, drawWidth, e.Node.Bounds.Height);
                }

                // Erase the canvas under plus-minus icon, as semitransparent areas of the image tend to 'sum up' and become darker each time it is drawn
                x = e.Node.Bounds.Location.X - 26;
                y = e.Node.Bounds.Location.Y + 11;

                e.Graphics.FillRectangle(_treeBackBrush, x, y, 21, 21);

                // Draw background or erase the old node
                {
                    x = e.Node.Bounds.X - 2;
                    y = e.Node.Bounds.Y;

                    // Set up gradient brush
                    if (gradientBrush > 0)
                    {
                        x += 1;
                        y += 2;

                        xAdjustmentBgr = 3;
                        yAdjustmentBgr = 4;

                        if (gradientBrush == 1)
                            backBrush = _treeGradientBrush1;

                        if (gradientBrush == 2)
                            backBrush = _treeGradientBrush2;

                        if (gradientBrush == 3)
                            backBrush = _treeGradientBrush3;
                    }

                    e.Graphics.FillRectangle(backBrush, x, y, drawWidth - xAdjustmentBgr, e.Node.Bounds.Height - yAdjustmentBgr);
                }
            }

            // Draw plus/minus icon
            {
                if (_doUseDummies || e.Node.Level == 0)
                {
                    expandImg = (isNodeExpanded || isNodeEmpty) ? _imgMinus : _imgPlus;
                }
                else
                {
                    expandImg = (isNodeExpanded) ? _imgMinus : _imgPlus;

                    if (isNodeEmpty)
                        doDrawIcon = false;
                }

                if (doDrawIcon)
                {
                    x = e.Node.Bounds.Location.X - 35;
                    y = e.Node.Bounds.Location.Y - 0;

                    e.Graphics.DrawImage(expandImg, x, y, 40, 40);
                }
            }

            // Draw node image
            {
                Image dirImg = isNodeExpanded ? _imgDirOpened : _imgDir;
                x = e.Node.Bounds.Location.X + 3;
                y = e.Node.Bounds.Location.Y + 5;

                //e.Graphics.DrawImage(e.Node.Level == 0 ? _imgHDD : dirImg, x, y, 30, 30);

                if (e.Node.Level == 0)
                {
                    //e.Graphics.DrawImage(_imgHDD, x, y, 32, 32);
                    //e.Graphics.DrawImage(_imgHDD, x, y, 64, 64);
                    e.Graphics.DrawImage(_imgHDD, x, y, 32, 32);
                }
                else
                {
                    //e.Graphics.DrawImage(dirImg, x, y, 30, 30);
                    e.Graphics.DrawImage(dirImg, x-1, y-1, 32, 32);
                }


            }

            // Draw node text
            {
                if (nodeFont.Height > 17)
                    yAdjustment = -1;

                if (nodeFont.Height < 7)
                    yAdjustment = 1;

                x = e.Node.Bounds.X + xAdjustmentText + xLeftMargin;
                y = e.Node.Bounds.Y + (e.Node.TreeView.ItemHeight - nodeFont.Height) / 2 + yAdjustment;

                e.Graphics.DrawString(e.Node.Text, nodeFont, fontBrush, x, y);
            }

            // If the node is clicked or hovered upon, draw a focus rectangle
            if (isNodeHovered || isNodeClicked)
            {
                x = e.Node.Bounds.X - 2;
                y = e.Node.Bounds.Y + 1;

                var rect = new Rectangle(x, y, drawWidth - 2, e.Node.Bounds.Height - 3);
                Color focusPenColor = Color.LightBlue;

                if (isNodeClicked)
                    focusPenColor = Color.CornflowerBlue;

                using (Pen focusPen = new Pen(focusPenColor))
                {
                    myUtils.DrawRoundedRectangle(e.Graphics, focusPen, rect, cornerRadius: 3);
                }
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Populates the list with directory and file names
    // Returns the number of errors
    // ref int itemsFound parameter receives the number of items found (depending on the search boolean parameters)
    public int nodeSelected(TreeNode n, System.Collections.Generic.List<string> files, ref int dirsFound, ref int filesFound, bool useRecursion = false)
    {
        int res = 0;

        if (useRecursion)
        {
            dirsFound = 0;
            filesFound = 0;

            files.Clear();
            var listTmpDirs  = new System.Collections.Generic.List<string>();
            var listTmpFiles = new System.Collections.Generic.List<string>();
            var stack = new System.Collections.Generic.Stack<string>(20);

            // Get all subfolders in current folder
            _logic.getDirectories(n.Name, listTmpDirs, doClear: true);
            listTmpDirs.Sort();

            for (int i = listTmpDirs.Count-1; i >= 0; i--)
                stack.Push(listTmpDirs[i][2..]);

            // Get all files in current folder
            _logic.getFiles(n.Name, listTmpFiles, doClear: true);
            listTmpFiles.Sort();
            filesFound += listTmpFiles.Count;
            foreach (var file in listTmpFiles)
                files.Add(file);

            while (stack.Count > 0)
            {
                string currentDir = stack.Pop();

                files.Add("1?" + currentDir);
                dirsFound++;

                _logic.getFiles(currentDir, listTmpFiles, doClear: true);
                listTmpFiles.Sort();
                filesFound += listTmpFiles.Count;
                foreach (var file in listTmpFiles)
                    files.Add(file);

                _logic.getDirectories(currentDir, listTmpDirs, doClear: true);
                listTmpDirs.Sort();
                for (int i = listTmpDirs.Count - 1; i >= 0; i--)
                    stack.Push(listTmpDirs[i][2..]);
            }
        }
        else
        {
            // Get directories first
            res += _logic.getDirectories(n.Name, files, doClear: true);

            dirsFound = files.Count;

            files.Add("2?");

            // Get files next
            res += _logic.getFiles(n.Name, files, doClear: false);

            filesFound = files.Count - dirsFound - 1;

            // Sort the results
            files.Sort();
        }

        return res;
    }

    // --------------------------------------------------------------------------------------------------------

    // Disallow redrawing of nodes
    public void AllowRedrawing(bool val)
    {
        _allowRedrawing = val;
    }

    // --------------------------------------------------------------------------------------------------------

    // Cache node fonts and return them as needed
    private ref Font getNodeFont(int level)
    {
        int max = 25;
        int min = 9;

        if (_nodeFonts == null)
        {
            int size = max - min + 1;
            _nodeFonts = new Font[size];

            for (int i = 0; i < size; i++)
            {
                _nodeFonts[i] = new Font("Segoe UI", max - i, FontStyle.Regular, GraphicsUnit.Pixel);
            }
        }

        // Reducing font at every second level
        level /= 2;
        level = (level > (max - min)) ? max - min : level;

        return ref _nodeFonts[level];
    }

    // --------------------------------------------------------------------------------------------------------

    // Tree Node Expand Event
    // Should be called from TreeView::BeforeExpand
    // Should be preceeded by AllowRedrawing(false), and should be followed by tree.AllowRedrawing(true);
    // The latter should be called from TreeView::AfterExpand
    public void nodeExpanded_Before(TreeNode n, System.Collections.Generic.List<string> dirs)
    {
        if (n.Nodes.Count == 1 && n.Nodes[0].Text == "[?]")
        {
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            n.Nodes.Clear();
            _logic.getDirectories(n.Name, dirs);

            if (dirs.Count > 0)
            {
                dirs.Sort();

                TreeNode[] childNodes = new TreeNode[dirs.Count];
                int i = 0;

                foreach (var dir in dirs)
                {
                    string key  = dir[2..];                                 // Full path
                    string text = dir[(dir.LastIndexOf('\\') + 1)..];       // Only name

                    var newNode = new TreeNode(key);
                    newNode.Name = key;
                    newNode.Text = text;
                    newNode.NodeFont = getNodeFont(n.Level);
                    addDummySubNode(ref newNode);

                    childNodes[i++] = newNode;
                }

                n.Nodes.AddRange(childNodes);
            }
            else
            {
                // AfterExpand event won't be called, as the node is actually empty
                // So we're allowing the redrawing manually from here
                AllowRedrawing(true);
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Arrow;
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Adds dummy subnode to a node (thus, it will have a plus icon and can be expanded later)
    private void addDummySubNode(ref TreeNode node)
    {
        // If [_doUseDummies] parameter is set to true, it does not check if the folder actually has any subfoldes
        // Otherwise, it checks for it and only adds the dummy if the folder does have at least one subfolder
        if (_doUseDummies == true || _logic.folderHasSubfolders(node.Name))
        {
            node.Nodes.Add("[?]", "[?]");
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Sets current path in the tree and expands all the directories along this path.
    // If [useDummy] parameter is [true], the function will traverse the tree is a fast fashion,
    // adding dummies to each found subfolder. After that, it will create an async task and traverse the tree once more,
    // expanding empty directories to remove the plus icon from them.
    public void setPath(string path, bool useDummies, bool expandEmpty)
    {
        if (path.Length > 0)
        {
            AllowRedrawing(false);

            bool _doUseDummies_old = _doUseDummies;
            _doUseDummies = useDummies ? true : _doUseDummies;

            _logic.getLastValidPath(ref path);

            TreeNode last = null;

            if (path.Length > 0)
            {
                TreeNodeCollection thisLevelNodes = _tree.Nodes;
                var dirs = new System.Collections.Generic.List<string>();

                while (path.Length > 0)
                {
                    string nodeName = _logic.getLeftmostPartFromPath(ref path).ToLower();

                    foreach (TreeNode node in thisLevelNodes)
                    {
                        string text = node.Text.ToLower();

                        if (node.Level == 0) // "Drive C:" --> "C:"
                            text = text.Substring(6);

                        if (text == nodeName)
                        {
                            // Fill the node with actual data
                            nodeExpanded_Before(node, dirs);

                            if (path.Length == 0)
                            {
                                last = node;
                                _tree.SelectedNode = node;
                                node.Expand();
                            }

                            thisLevelNodes = node.Nodes;
                            break;
                        }
                    }
                }
            }

            AllowRedrawing(true);
            _doUseDummies = _doUseDummies_old;

            // Move up the directory tree and expand all the directories that are empty
            if (expandEmpty)
                expandEmptyFolders(last, useDummies);
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Move up the directory tree and expand all the directories that are empty
    // This is done to remove 'plus' icons from the nodes that are actually empty
    // This might take a while, so it should be executed in async manner, so it would not block the application
    // Note: this is the first atempt to make it work, and it turns out, we can't update controls
    // from a thread that has not created the control in the first place.
    // So this method should not be used. Use expandEmptyFolders(TreeNode node, bool useDummies) instead.
    private void expandEmptyFolders(TreeNode node)
    {
        if (node != null && node.Level != 0)
        {
            node = node.Parent;

            do
            {
                foreach (TreeNode n in node.Nodes)
                {
                    if (!n.IsExpanded)
                    {
                        if (!_logic.folderHasSubfolders(n.Name))
                        {
                            n.Expand();
                        }
                    }
                }

                node = node.Level > 0 ? node.Parent : null;

            } while (node != null);
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Move up the directory tree and expand all the directories that are empty.
    // This is done to remove 'plus' icons from the nodes that are actually empty.
    // This might take a while, so it is executed in async manner, so it should not block the application.
    private void expandEmptyFolders(TreeNode node, bool useDummies)
    {
        var t = new System.Threading.Tasks.Task(() =>
        {
            try
            {
                // Wait until the Form has been created and the tree Handle was obtained
                while (_tree.InvokeRequired == false)
                {
                    System.Threading.Tasks.Task.Delay(10).Wait();
                }

                _tree.Invoke(new MethodInvoker(delegate{ _tree.Enabled = false; }));

                do
                {
                    foreach (TreeNode n in node.Nodes)
                    {
                        bool isNodeExpanded = false;

                        if (_tree.InvokeRequired)
                        {
                            _tree.Invoke(new MethodInvoker(delegate { isNodeExpanded = n.IsExpanded; }));
                        }

                        if (!isNodeExpanded)
                        {
                            if (!_logic.folderHasSubfolders(n.Name))
                            {
                                if (_tree.InvokeRequired)
                                {
                                    // Don't really need to expand the node. Clearing it is enough
                                    _tree.Invoke(new MethodInvoker(delegate { n.Nodes.Clear(); }));
                                }
                            }
                        }
                    }

                    node = node.Level > 0 ? node.Parent : null;

                } while (node != null);

                _tree.Invoke(new MethodInvoker(delegate { _tree.Enabled = true; }));

            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        });

        var t2 = new System.Threading.Tasks.Task(() =>
        {
            try
            {
                // Wait until the Form has been created and the tree Handle was obtained
                while (_tree.InvokeRequired == false)
                {
                    System.Threading.Tasks.Task.Delay(10).Wait();
                }

                var list1 = new System.Collections.Generic.List<TreeNode>();
                var list2 = new System.Collections.Generic.List<TreeNode>();

                // Collect all the nodes that are not expanded
                _tree.Invoke(new MethodInvoker(delegate
                {
                    _tree.Cursor = System.Windows.Forms.Cursors.WaitCursor;

                    do
                    {
                        foreach (TreeNode n in node.Nodes)
                            if (!n.IsExpanded)
                                list1.Add(n);

                        node = node.Level > 0 ? node.Parent : null;
                    }
                    while (node != null);

                }));

                // Select all the nodes that don't have any subfolders in them
                foreach (var node in list1)
                    if (!_logic.folderHasSubfolders(node.Name))
                        list2.Add(node);

                // Clear subnodes of all selected nodes
                // Don't really need to expand them. Clearing is enough
                _tree.Invoke(new MethodInvoker(delegate
                {
                    _tree.BeginUpdate();

                    foreach (TreeNode n in list2)
                        n.Nodes.Clear();

                    _tree.EndUpdate();
                    _tree.Cursor = System.Windows.Forms.Cursors.Arrow;
                }));

            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        });

        if (useDummies && node != null && node.Level != 0)
        {
            t2.Start();
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Unfinished unused
    // Trying to make it jump to the end of current nodes list
    // called from private void treeView1_KeyDown(object sender, KeyEventArgs e)
    public void aaa(object sender, KeyEventArgs e)
    {
        TreeNode nextNode = null, prevNode = null, startNode = _tree.SelectedNode;

        if (_tree.SelectedNode.Nodes.Count == 0)
        {
            prevNode = _tree.SelectedNode;
            nextNode = _tree.SelectedNode.NextVisibleNode;

            while (nextNode.Nodes.Count == 0)
            {
                prevNode = nextNode;
                nextNode = nextNode.NextVisibleNode;
            }

            if (nextNode.Parent == startNode.Parent)
            {
                _tree.SelectedNode = nextNode;
            }
            else
            {
                _tree.SelectedNode = prevNode;
            }
        }

        //e.Handled = true;
        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Set double buffering to reduce flickering
    private void setDoubleBuffering()
    {
        // Set double buffering to reduce flickering:
        // https://stackoverflow.com/questions/41893708/how-to-prevent-datagridview-from-flickering-when-scrolling-horizontally
        PropertyInfo pi = _tree.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
        pi.SetValue(_tree, true, null);
    }

    // --------------------------------------------------------------------------------------------------------
};
