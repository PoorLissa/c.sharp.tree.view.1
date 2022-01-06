using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;
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

    private Brush _treeBackBrush      = null;
    private Brush _treeGradientBrush1 = null;
    private Brush _treeGradientBrush2 = null;
    private Brush _treeGradientBrush3 = null;

    private Image _imgPlus        = null;
    private Image _imgMinus       = null;
    private Image _imgDir1        = null;
    private Image _imgDir2        = null;
    private Image _imgDir1_Opaque = null;
    private Image _imgDir2_Opaque = null;
    private Image _imgHDD         = null;

    private bool _doUseDummies    = false;
    private bool _allowRedrawing  = false;
    private bool _allowWaitCursor = false;

    private Font [] _nodeFonts = null;

    private StringBuilder pathStrBuilder = null;

    private int _winVer  = 0;
    private int _treeDpi = 0;

    // Going to hold a reference to the global list of files
    private readonly List<myTreeListDataItem> _globalFileListExtRef = null;
    private          List<myTreeListDataItem> _dirsListTmpExt       = null;

    private RichTextBox _richTextBox = null;

    // --------------------------------------------------------------------------------------------------------

    public myTree(TreeView tv, List<myTreeListDataItem> listGlobal, string path, bool expandEmpty, RichTextBox rb = null)
    {
        _tree  = tv;
        _logic = new myTreeLogic();
        _richTextBox = rb;

        _foreColor = tv.ForeColor;
        _fontSize  = tv.Font.Size;

        _doUseDummies   = false;
        _allowRedrawing = true;

        _dirsListTmpExt = new List<myTreeListDataItem>();
        _globalFileListExtRef = listGlobal;

        init();
        setPath(path, useDummies: true, expandEmpty: expandEmpty);

        _winVer = Environment.OSVersion.Version.Major * 10 + Environment.OSVersion.Version.Minor;

        _allowWaitCursor = false;
    }

    // --------------------------------------------------------------------------------------------------------

    public ref TreeView Obj()
    {
        return ref _tree;
    }

    // --------------------------------------------------------------------------------------------------------

    private void init()
    {
        if (_tree != null)
        {
            setDoubleBuffering();

            _treeDpi            = _tree.DeviceDpi;
            _tree.Indent        = 30;
            _tree.ItemHeight    = _treeDpi > 96 ? 40 : 32;
            _tree.HideSelection = false;
            _tree.HotTracking   = true;
            _tree.FullRowSelect = true;
            _tree.ShowLines     = false;        // The ShowLines property must be false for the FullRowSelect property to work

            createDrawingPrimitives();

            // Set up events
            _tree.MouseEnter += new EventHandler(on_MouseEnter);

            // Configure the TreeView control for owner-draw and add a handler for the DrawNode event
            _tree.DrawMode  = TreeViewDrawMode.OwnerDrawAll;
            _tree.DrawNode += new DrawTreeNodeEventHandler(myTree_DrawNode);

            pathStrBuilder = new StringBuilder();

            // Populate the first level of the tree
            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                pathStrBuilder.Clear();

                pathStrBuilder.Append(drive.IsReady ? drive.VolumeLabel : null);
                pathStrBuilder.Append(pathStrBuilder.Length > 0 ? " (" : "(");
                pathStrBuilder.Append(drive.Name[0]);
                pathStrBuilder.Append(drive.Name[1]);
                pathStrBuilder.Append(")");

                // .Add(driveNode.Name, driveNode.Text)
                TreeNode driveNode = _tree.Nodes.Add(drive.Name, pathStrBuilder.ToString());
                driveNode.NodeFont = getNodeFont(0);

                // Each yet unopened node will contain this secret node
                driveNode.Nodes.Add("[?]", "[?]");
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
            _imgPlus  = Image.FromFile(myUtils.getFilePath("_icons", "icon-tree-node-2-plus-32.png"));
            _imgHDD   = Image.FromFile(myUtils.getFilePath("_icons", "icon-hdd-1-48-wide.png"));

            if (_tree.ItemHeight <= 35)
            {
                _imgDir1 = Image.FromFile(myUtils.getFilePath("_icons", "icon-tree-folder-1-closed-30-wide.png"));
                _imgDir2 = Image.FromFile(myUtils.getFilePath("_icons", "icon-tree-folder-1-opened-30-wide.png"));
            }
            else
            {
                _imgDir1 = Image.FromFile(myUtils.getFilePath("_icons", "icon-tree-folder-1-closed-30.png"));
                _imgDir2 = Image.FromFile(myUtils.getFilePath("_icons", "icon-tree-folder-1-opened-30.png"));
            }

            _imgDir1_Opaque = myUtils.ChangeImageOpacity(_imgDir1, 0.35);
            _imgDir2_Opaque = myUtils.ChangeImageOpacity(_imgDir2, 0.35);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "myTree: Failed to load images", MessageBoxButtons.OK);
        }

        // Create brushes, gradients, etc.
        _treeBackBrush = new SolidBrush(_tree.BackColor);

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
            bool isNodeSelected = (e.State & TreeNodeStates.Selected) != 0;
            bool isHidden       = e.Node.ForeColor == Color.Gray;
            bool doErase        = false;
            bool doDrawIcon     = true;
            Image expandImg     = null;

            Brush backBrush = _treeBackBrush;
            Brush fontBrush = Brushes.Black;

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
            if (isNodeSelected)
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
                    xAdjustmentText = 1;
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
                int iconHeight = _treeDpi > 96 ?  40 : 32;

                x = e.Node.Bounds.Location.X - iconHeight;
                y = e.Node.Bounds.Location.Y + (_tree.ItemHeight - iconHeight) / 2;

                // Erase the canvas under the icon, as semitransparent areas of the image tend to 'sum up' and become darker each time it is drawn
                e.Graphics.FillRectangle(_treeBackBrush, x + 5, y + 5, iconHeight - 10, iconHeight - 10);

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
                    e.Graphics.DrawImage(expandImg, x, y, iconHeight, iconHeight);
                }
            }

            // Draw node image
            {
                x = e.Node.Bounds.Location.X + 3 + xAdjustmentText;

                if (e.Node.Level == 0)
                {
                    int hddH = _imgHDD.Height < _tree.ItemHeight ? _imgHDD.Height : _tree.ItemHeight;

                    y = e.Node.Bounds.Location.Y + (_tree.ItemHeight - hddH) / 2;
                    e.Graphics.DrawImage(_imgHDD, x, y, hddH, hddH);
                }
                else
                {
                    Image dirImg = isNodeExpanded
                                        ? isHidden ? _imgDir2_Opaque : _imgDir2
                                        : isHidden ? _imgDir1_Opaque : _imgDir1;

                    y = e.Node.Bounds.Location.Y + (_tree.ItemHeight - dirImg.Height) / 2;
                    e.Graphics.DrawImage(dirImg, x - 1, y, 32, 32);
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

                e.Graphics.DrawString(e.Node.Text, nodeFont, isHidden ? Brushes.LightSlateGray : fontBrush, x, y);
            }

            // If the node is clicked or hovered upon, draw a focus rectangle
            if (isNodeHovered || isNodeClicked || (isNodeSelected && !_tree.Focused))
            {
                x = e.Node.Bounds.X - 2;
                y = e.Node.Bounds.Y + 1;

                var rect = new Rectangle(x, y, drawWidth - 2, e.Node.Bounds.Height - 3);
                Color focusPenColor = Color.LightBlue;

                if (isNodeSelected)
                    focusPenColor = Color.LightSlateGray;

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

    // Populates the supplied list with directory and file names
    // Returns the number of errors
    // ref int itemsFound parameters receive the number of items found
    // Cancelable version: to be used with Threading.Task
    public int nodeSelected_Cancellable(TreeNode n, ref int dirsFound, ref int filesFound, System.Threading.CancellationToken token, bool useRecursion = false)
    {
        token.ThrowIfCancellationRequested();

        int res = 0;
        _globalFileListExtRef.Clear();

        string path = getFullPath(n);

        if (useRecursion)
        {
            dirsFound  = 0;
            filesFound = 0;

            var listTmpDirs  = new List<myTreeListDataItem>();
            var listTmpFiles = new List<myTreeListDataItem>();
            var stack        = new Stack<myTreeListDataItem>(100);

            // Get all subfolders in current folder
            _logic.getDirectories(path, listTmpDirs, doClear: true, doSort: true);
            
            for (int i = listTmpDirs.Count - 1; i >= 0; i--)
                stack.Push(listTmpDirs[i]);

            token.ThrowIfCancellationRequested();

            // Get all files in current folder
            _logic.getFiles(path, listTmpFiles, doClear: true, doSort: true);
            filesFound += listTmpFiles.Count;

            for (int i = 0; i < listTmpFiles.Count; i++)
                _globalFileListExtRef.Add(listTmpFiles[i]);

            while (stack.Count > 0)
            {
                // Break the loop in case the cancellation was requested
                token.ThrowIfCancellationRequested();

                myTreeListDataItem currentDir = stack.Pop();

                _globalFileListExtRef.Add(currentDir);
                dirsFound++;

                _logic.getFiles(currentDir.Name, listTmpFiles, doClear: true, doSort: true);
                filesFound += listTmpFiles.Count;

                for (int i = 0; i < listTmpFiles.Count; i++)
                    _globalFileListExtRef.Add(listTmpFiles[i]);

                token.ThrowIfCancellationRequested();

                _logic.getDirectories(currentDir.Name, listTmpDirs, doClear: true, doSort: true);

                for (int i = listTmpDirs.Count - 1; i >= 0; i--)
                    stack.Push(listTmpDirs[i]);
            }
        }
        else
        {
            // Get directories first
            res += _logic.getDirectories(path, _globalFileListExtRef, doClear: false);

            dirsFound = _globalFileListExtRef.Count;

            // Get files next
            res += _logic.getFiles(path, _globalFileListExtRef, doClear: false);

            filesFound = _globalFileListExtRef.Count - dirsFound;

            // Sort the results
            _globalFileListExtRef.Sort();
        }

        return res;
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_MouseEnter(object sender, EventArgs e)
    {
        _tree.Focus();
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
        int max = _treeDpi > 96 ? 25 : 18;
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

    // Builds full path from the root to the node
    private string getFullPath(TreeNode n)
    {
        pathStrBuilder.Clear();

        while (n.Level != 0)
        {
            pathStrBuilder.Insert(0, n.Name);
            pathStrBuilder.Insert(0, '\\');
            n = n.Parent;
        }

        // Add drive name
        if (pathStrBuilder.Length == 0)
        {
            pathStrBuilder.Append(n.Name);              // sb = 'c:\\'
        }
        else
        {
            pathStrBuilder.Insert(0, n.Name[1]);        // sb.insert('c:')
            pathStrBuilder.Insert(0, n.Name[0]);
        }

        return pathStrBuilder.ToString();
    }

    // --------------------------------------------------------------------------------------------------------

    // Tree Node Expand Event
    // Should be called from TreeView::BeforeExpand
    // Should be preceeded by AllowRedrawing(false), and should be followed by tree.AllowRedrawing(true);
    // The latter should be called from TreeView::AfterExpand
    public void nodeExpanded_Before(TreeNode n)
    {
        if (n.Nodes.Count == 1 && n.Nodes[0].Text == "[?]")
        {
            if (_allowWaitCursor)
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            }

            n.Nodes.Clear();

            _logic.getDirectories(getFullPath(n), _dirsListTmpExt, doClear: true);

            if (_dirsListTmpExt.Count > 0)
            {
                _dirsListTmpExt.Sort();

                TreeNode[] childNodes = new TreeNode[_dirsListTmpExt.Count];

                for (int i = 0; i < _dirsListTmpExt.Count; i++)
                {
                    var dir = _dirsListTmpExt[i];

                    string key = dir.Name[(dir.Name.LastIndexOf('\\') + 1)..];     // Get only name from full path

                    var newNode = new TreeNode(key);
                    newNode.Name = key;
                    newNode.Text = key;
                    newNode.NodeFont = getNodeFont(n.Level);
                    newNode.ForeColor = dir.isHidden ? Color.Gray : Color.Black;
                    addDummySubNode(ref newNode, dir.Name);

                    childNodes[i] = newNode;
                }

                n.Nodes.AddRange(childNodes);
            }
            else
            {
                // AfterExpand event won't be called, as the node is actually empty
                // So we're allowing the redrawing manually from here on
                _tree.EndUpdate();
                AllowRedrawing(true);
            }

            if (_allowWaitCursor)
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Arrow;
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Adds dummy subnode to a node (thus, it will have a plus icon and can be expanded later)
    private void addDummySubNode(ref TreeNode node, string fullPath)
    {
        // If [_doUseDummies] parameter is set to true, it does not check if the folder actually has any subfoldes
        // Otherwise, it checks for it and only adds the dummy if the folder does have at least one subfolder
        if (_doUseDummies == true || _logic.folderHasSubfolders(fullPath))
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
        _logic.getLastValidPath(ref path);

        if (path.Length > 0)
        {
            AllowRedrawing(false);

            bool _doUseDummies_old = _doUseDummies;
            _doUseDummies = useDummies ? true : _doUseDummies;

            TreeNode last = null;

            if (path.Length > 0)
            {
                TreeNodeCollection thisLevelNodes = _tree.Nodes;

                while (path.Length > 0)
                {
                    string nodeName = _logic.getLeftmostPartFromPath(ref path).ToLower();

                    // todo: get rid of tolower and use fast compare instead
                    foreach (TreeNode node in thisLevelNodes)
                    {
                        string text = node.Text.ToLower();

                        if (node.Level == 0)
                        {
                            int index = text.IndexOf(':') - 1;
                            text = text.Substring(index, 2);        // "Win 7 SSD (C:)" ==> "C:"
                        }

                        if (text == nodeName)
                        {
                            // Fill the node with actual data
                            nodeExpanded_Before(node);

                            if (path.Length == 0)
                            {
                                last = node;
                                _tree.SelectedNode = node;

                                // Mark the node:
                                // myTree_DataGrid_Manager.tree_onAfterSelect() method will know it needs to make dataGrid's first line selected
                                node.ImageIndex = -2;
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
            {
                expandEmptyFolders(last, useDummies);
            }
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
                        if (!_logic.folderHasSubfolders(getFullPath(n)))
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
                            if (!_logic.folderHasSubfolders(getFullPath(n)))
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
                    if (!_logic.folderHasSubfolders(getFullPath(node)))
                        list2.Add(node);

                // Clear subnodes of all selected nodes
                // Don't really need to expand them. Clearing is enough
                _tree.Invoke(new MethodInvoker(delegate
                {
                    _tree.BeginUpdate();

                    foreach (TreeNode n in list2)
                        n.Nodes.Clear();

                    _tree.EndUpdate();
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

    // todo:
    // Unfinished unused
    // Trying to make it jump to the end of current nodes list
    // called from private void treeView1_KeyDown(object sender, KeyEventArgs e)
    // the idea: on the right arrow jump to the first of opened node's children. next right arrow makes it jump to the last of its children
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
        #if DEBUG_TRACE
            myUtils.logMsg("myTree.setDoubleBuffering", "");
        #endif

        // Set double buffering to reduce flickering:
        // https://stackoverflow.com/questions/41893708/how-to-prevent-datagridview-from-flickering-when-scrolling-horizontally
        PropertyInfo pi = _tree.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
        pi.SetValue(_tree, true, null);
    }

    // --------------------------------------------------------------------------------------------------------

    // Update Tree's state
    public void update(myBackup backUp, List<myTreeListDataItem> updatedList)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myTree.update", "");
        #endif

        TreeNode n = _tree.SelectedNode;

        if (n.Nodes.Count == 1 && n.Nodes[0].Name == "[?]")
        {
            // node has not been opened yet -- no need to update it
        }
        else
        {
            AllowRedrawing(false);

            // For each updated item: get its history info from the backUp
            // In order to properly address recursive mode, iterate backwards
            for (int i = updatedList.Count - 1; i >= 0; i--)
            {
                // Skip files, as the tree contains only directories
                if (updatedList[i].isDir)
                {
                    if (updatedList[i].isChanged)
                    {
                        var fileNameHistory = backUp.getHistory(updatedList[i].Name);

                        if (fileNameHistory != null)
                        {
                            // History list shall contain at least 2 items (if not null)
                            // upd.:
                            // With the latest changes (2-step renaming with tmp iteration in the middle),
                            // the history shall contain at least 3 items at this point: name_orig -> name_tmp -> name_final
                            // We want to compare original name with final name here:
                            var curr = fileNameHistory[fileNameHistory.Count - 1];
                            var prev = fileNameHistory[fileNameHistory.Count - 3];

                            // Using old and new paths, update corresponding subnode
                            if (!_logic.updateNode(n, prev, curr))
                            {
                                MessageBox.Show($"Could not update subnode:\n{n.Name}\n{prev}\n{curr}", "Tree Error",
                                    MessageBoxButtons.OK);
                            }
                        }
                    }
                }
            }

            AllowRedrawing(true);
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------
};
