using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;


/*
    Wrapper class around DataGridView widget.
    Allows customization and provides public methods to work with the widget.
*/


public class myDataGrid
{
    private DataGridView    _dataGrid      = null;
    private DataGridViewRow _myTemplateRow = null;

    private bool _doUseRecursion     = false;
    private bool _doShowRecursionMsg = false;
    private bool _filterMode         = false;
    private bool _tabFocus           = false;

    private Image _imgDir         = null;
    private Image _imgFile        = null;
    private Image _imgDir_Opaque  = null;
    private Image _imgFile_Opaque = null;

    private Brush _gridGradientBrush1  = null;
    private Brush _gridGradientBrush2  = null;
    private Brush _disabledStateBrush1 = null;
    private Brush _disabledStateBrush2 = null;

    // Going to hold a reference to the global list of files
    private readonly List<myTreeListDataItem> _globalFileListExtRef = null;

    // Containers to store the state of selected items between grid repopulations
    private HashSet<   int> currentlySelectedIds   = null;
    private HashSet<string> currentlySelectedNames = null;

    // StringFormat for CellId custom text drawing
    private StringFormat strFormat_CellId   = null;
    private StringFormat strFormat_CellName = null;
    private StringFormat strFormat_CellNameTooltip = null;

    private myDataGrid_Cache _cache = null;

    private string _recursionMessage = "";
    private string _simulatedFileName = null;

    // --------------------------------------------------------------------------------------------------------

    public enum Columns
    {
        colId, colChBox, colImage, colName
    };

    public enum PopulateReason
    {
        dirChanged, viewFileChanged, viewDirChanged, filterChanged, recursionChanged_Before, recursionChanged_After
    };

    private enum HoverStatus
    {
        DEFAULT = 2, MOUSE_HAS_LEFT, MOUSE_HOVER
    };

    // --------------------------------------------------------------------------------------------------------

    public myDataGrid(DataGridView dgv, List<myTreeListDataItem> listGlobal)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.myDataGrid", "");
        #endif

        _dataGrid = dgv;
        _globalFileListExtRef = listGlobal;

        init();
    }

    // --------------------------------------------------------------------------------------------------------

    public ref DataGridView Obj()
    {
        return ref _dataGrid;
    }

    // --------------------------------------------------------------------------------------------------------

    public void init()
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.init", "");
        #endif

        // https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-work-with-image-columns-in-the-windows-forms-datagridview-control?view=netframeworkdesktop-4.8
        if (_dataGrid != null)
        {
            int dpi = _dataGrid.DeviceDpi;

            setDoubleBuffering();

            // Add and subscribe to events
            setUpEvents();

            _dataGrid.RowTemplate.Height = dpi > 96 ? 60 : 40;                              // Row height
            _dataGrid.RowTemplate.MinimumHeight = (int)HoverStatus.DEFAULT;                 // Will be used as a flag for on_MouseEnter / on_MouseLeave events

            _dataGrid.SelectionMode     = DataGridViewSelectionMode.FullRowSelect;          // Row select mode
            _dataGrid.CellBorderStyle   = DataGridViewCellBorderStyle.SingleHorizontal;     // Cell borders
            _dataGrid.EditMode          = DataGridViewEditMode.EditOnF2;                    // How to edit cell (press F2)
            _dataGrid.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;            // To be able to manually copy data from edited cell

            _dataGrid.AllowUserToAddRows    = false;                                        // User can't add new rows
            _dataGrid.AllowUserToResizeRows = false;                                        // User can't resize rows
            _dataGrid.ColumnHeadersVisible  = false;                                        // No column headers
            _dataGrid.RowHeadersVisible     = false;                                        // No row headers
            _dataGrid.ShowCellToolTips      = false;                                        // No cell tooltips
            _dataGrid.MultiSelect           = true;                                         // User is allowed to select several rows

            // Grid Colors
            _dataGrid.DefaultCellStyle.SelectionForeColor = Color.Black;                    // Selected row's font color
            _dataGrid.DefaultCellStyle.SelectionBackColor = Color.DarkOrange;               // Row selection color
            _dataGrid.GridColor = Color.LightGray;
            _dataGrid.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Control;
            _dataGrid.BackgroundColor = _dataGrid.DefaultCellStyle.BackColor;               // Instead of OnPaint event

            // Add Columns
            addColumns();

            createDrawingPrimitives(dpi);
        }
    }

    // --------------------------------------------------------------------------------------------------------

    private void createDrawingPrimitives(int dpi)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.createDrawingPrimitives", "");
        #endif

        _cache = new myDataGrid_Cache(dpi);

        // Load images
        _imgDir  = Image.FromFile(myUtils.getFilePath("_icons", "icon-folder-1-30.png"));
        _imgFile = Image.FromFile(myUtils.getFilePath("_icons", "icon-file-1-30.png"));

        _imgDir_Opaque  = myUtils.ChangeImageOpacity(_imgDir,  0.35);
        _imgFile_Opaque = myUtils.ChangeImageOpacity(_imgFile, 0.35);

        // Create brushes, gradients, etc.
        var pt1 = new Point(0, 0);
        var pt2 = new Point(0, _dataGrid.RowTemplate.Height);

        // Selected, but not hovered upon
        _gridGradientBrush1 = new System.Drawing.Drawing2D.LinearGradientBrush(pt1, pt2,
                                    Color.FromArgb( 50, 255, 200, 133),
                                    Color.FromArgb(175, 255, 128,   0));

        // Selected and hovered upon
        _gridGradientBrush2 = new System.Drawing.Drawing2D.LinearGradientBrush(pt1, pt2,
                                    Color.FromArgb(150, 255, 200, 133),
                                    Color.FromArgb(233, 255, 128,   0));

        _disabledStateBrush1 = new SolidBrush(Color.FromArgb(150, Color.White));
        _disabledStateBrush2 = new SolidBrush(Color.FromArgb(200, Color.White));

        strFormat_CellId = new StringFormat(StringFormatFlags.NoClip);
        strFormat_CellId.LineAlignment = StringAlignment.Center;
        strFormat_CellId.Alignment = StringAlignment.Center;

        strFormat_CellName = new StringFormat(StringFormatFlags.NoWrap);
        strFormat_CellName.LineAlignment = StringAlignment.Near;
        strFormat_CellName.Alignment = StringAlignment.Far;

        strFormat_CellNameTooltip = new StringFormat(StringFormatFlags.NoWrap);
        strFormat_CellNameTooltip.LineAlignment = StringAlignment.Center;
        strFormat_CellNameTooltip.Alignment = StringAlignment.Near;

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Add and subscribe to events
    private void setUpEvents()
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.setUpEvents", "");
        #endif

        // Customize whole widget appearance
        _dataGrid.Paint += new PaintEventHandler(OnPaint);

        // Customize individual cell appearance
        _dataGrid.CellPainting += new DataGridViewCellPaintingEventHandler(on_CellPainting);

        // Mouse events
        _dataGrid.CellMouseDown  += new DataGridViewCellMouseEventHandler(on_CellMouseDown);
        _dataGrid.CellMouseUp    += new DataGridViewCellMouseEventHandler(on_CellMouseUp);
        _dataGrid.CellMouseEnter += new DataGridViewCellEventHandler(on_CellMouseEnter);
        _dataGrid.CellMouseLeave += new DataGridViewCellEventHandler(on_CellMouseLeave);

        // Keyboard events
        _dataGrid.KeyDown += new KeyEventHandler(on_KeyDown);

        // Manual edit events
        _dataGrid.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(on_CellControlShowing);
        _dataGrid.CellBeginEdit         += new DataGridViewCellCancelEventHandler(on_CellBeginEdit);
        _dataGrid.CellEndEdit           += new DataGridViewCellEventHandler(on_CellEndEdit);
    }

    // --------------------------------------------------------------------------------------------------------

    private void addColumns()
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.addColumns", "");
        #endif

        // Row id column Columns.colId
        var columnId = new DataGridViewTextBoxColumn();
            columnId.Name = "id";
            columnId.Visible = true;
            columnId.DividerWidth = 1;
            columnId.ReadOnly = true;
            columnId.MinimumWidth = _dataGrid.RowTemplate.Height;
            columnId.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            columnId.DefaultCellStyle.Font = new Font("Segoe UI", 8, FontStyle.Regular, GraphicsUnit.Point);

        // Checkbox column Columns.colCheckBox
        var columnCheckBox = new DataGridViewCheckBoxColumn();

            // Custom cell template
            var cellCBTemplate = new myDataGridViewCheckBoxCell();
                cellCBTemplate.CustomDrawing = myDataGridViewCheckBoxCell.DrawMode.Custom3;
                cellCBTemplate.CustomSize = 26;
                cellCBTemplate.CustomActiveAreaMargin = 5;
                cellCBTemplate.CustomImg("icon-tick-box1-checked-64.png",   myDataGridViewCheckBoxCell.cbMode.Checked);
                cellCBTemplate.CustomImg("icon-tick-box1-unchecked-64.png", myDataGridViewCheckBoxCell.cbMode.Unchecked);

            columnCheckBox.CellTemplate = cellCBTemplate;
            columnCheckBox.Width = 50;
            columnCheckBox.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            columnCheckBox.Resizable = DataGridViewTriState.False;

        // Icon column Columns.colImage
        var columnImage = new DataGridViewImageColumn();
            columnImage.Width = 30;
            columnImage.Resizable = DataGridViewTriState.False;

        // Text column Columns.colName (auto adjusted to fill all the available width)
        var columnName = new DataGridViewTextBoxColumn();
            columnName.CellTemplate = new myDataGridViewTextBoxCell();
            columnName.Name = "Name";
            columnName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            columnName.ReadOnly = true;
            columnName.DefaultCellStyle.Padding = new Padding(20, 0, 0, 0);

        // Index of columns must match Columns enum
        _dataGrid.Columns.Add(columnId);
        _dataGrid.Columns.Add(columnCheckBox);
        _dataGrid.Columns.Add(columnImage);
        _dataGrid.Columns.Add(columnName);

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    public void Clear()
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.Clear", "");
        #endif

        _dataGrid.Rows.Clear();
    }

    // --------------------------------------------------------------------------------------------------------

    // Modify a copy of a template row and give it real values
    private void buildRow(ref DataGridViewRow row, myTreeListDataItem item, bool isDir, int id)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.buildRow", "");
        #endif

        if (item.Name.Length > 0)
        {
            Image img = isDir
                            ? item.isHidden ? _imgDir_Opaque  : _imgDir
                            : item.isHidden ? _imgFile_Opaque : _imgFile;

            int pos = item.Name.LastIndexOf('\\') + 1;

            row.Cells[(int)Columns.colId   ].Value = id;
            row.Cells[(int)Columns.colChBox].Value = false;
            row.Cells[(int)Columns.colImage].Value = img;
            row.Cells[(int)Columns.colName ].Value = item.Name[pos..];

            row.DefaultCellStyle.ForeColor = isDir
                                                ? Color.Black
                                                : Color.Brown;
            if (item.isHidden)
            {
                row.Cells[(int)Columns.colName].Style.ForeColor = Color.Gray;
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Add a single item to the DataGridView
    // Not used
    public void addRow(string item, bool isDir)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.addRow", "");
        #endif

        int pos = item.LastIndexOf('\\') + 1;
        int i = _dataGrid.Rows.Add();

        if (item.Length > 0)
        {
            if (isDir)
            {
                _dataGrid.Rows[i].DefaultCellStyle.ForeColor = Color.Black;
            }
            else
            {
                _dataGrid.Rows[i].DefaultCellStyle.ForeColor = Color.Brown;
            }

            _dataGrid.Rows[i].Cells[(int)Columns.colChBox].Value = false;
            _dataGrid.Rows[i].Cells[(int)Columns.colImage].Value = isDir ? _imgDir : _imgFile;
            _dataGrid.Rows[i].Cells[(int)Columns.colName ].Value = item[pos..];
            _dataGrid.Rows[i].Cells[(int)Columns.colId   ].Value = -1;
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Populate GridView with known amount of rows
    // Single pass
    private void Populate_Fast(List<myTreeListDataItem> list, int dirsCount, int filesCount, bool doShowDirs, bool doShowFiles)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.Populate_Fast", "");
        #endif

        int Count = 0;
        Count += doShowDirs  ? dirsCount  : 0;
        Count += doShowFiles ? filesCount : 0;

        if (Count > 0)
        {
            // Reserve known amount of rows
            var rows = new DataGridViewRow[Count];

            // Reuse Count
            Count = 0;

            // Display all directories and files
            for(int i = 0; i < list.Count; i++)
            {
                var item = list[i];

                if (item.Id < 0)
                    item.Id = i;

                bool isDir = item.isDir;

                // Show/skip directories
                if (isDir && !doShowDirs)
                    continue;

                // Show/skip files
                if (!isDir && !doShowFiles)
                    continue;

                // Wasn't able to create new rows any other way
                // So had to stick to the cloning

                // Anyway, create new row and populate it with the data:
                var newRow = (DataGridViewRow)_myTemplateRow.Clone();
                buildRow(ref newRow, item, isDir, i);
                rows[Count++] = newRow;
            }

            // Add all the rows in a single motion
            // Less redrawing, less flickering
            _dataGrid.Rows.AddRange(rows);

            _dataGrid.ClearSelection();
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Populate GridView with unknown amount of rows (depending on filters value)
    // Multiple pass
    private void Populate_Slow(List<myTreeListDataItem> list, TreeNode selectedNode, int dirsCount, int filesCount,
                                        bool doShowDirs, bool doShowFiles, string filterStr, string filterOutStr)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.Populate_Slow", "");
        #endif

        int Count = 0;
        Count += doShowDirs  ? dirsCount  : 0;
        Count += doShowFiles ? filesCount : 0;

        if (Count > 0)
        {
            // Select only indexes of the items we need
            var selectedItems = new List<int>(Count);

            bool doSkip       = true;
            bool useFilter    =    filterStr.Length > 0;
            bool useFilterOut = filterOutStr.Length > 0;

            var filters    =    filterStr.Split(':', StringSplitOptions.RemoveEmptyEntries);
            var filtersOut = filterOutStr.Split(':', StringSplitOptions.RemoveEmptyEntries);

            int filePos = 0;

            if (_filterMode == true)
            {
                // Filter through the path starting with currently selected directory
                filePos = selectedNode.FullPath.Length - selectedNode.FullPath.IndexOf('\\') + 3;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id < 0)
                    list[i].Id = i;

                bool isDir = (list[i].isDir);

                // Show/skip directories
                if (isDir && !doShowDirs)
                    continue;

                // Show/skip files
                if (!isDir && !doShowFiles)
                    continue;

                if (_filterMode == false)
                {
                    // Filter through only file name (without any path)
                    filePos = list[i].Name.LastIndexOf('\\') + 1;
                }

                // Skip everything that does not match the search string [filterStr]
                if (useFilter)
                {
                    doSkip = true;

                    foreach (var filter in filters)
                    {
                        if (myUtils.fastStrContains(filter, list[i].Name, filePos, -1, caseSensitive: false))
                        {
                            doSkip = false;
                            break;
                        }
                    }

                    if (doSkip)
                        continue;
                }

                // Skip everything that DOES match the search string [filterOutStr]
                if (useFilterOut)
                {
                    doSkip = false;

                    foreach (var filterOut in filtersOut)
                    {
                        if (myUtils.fastStrContains(filterOut, list[i].Name, filePos, -1, caseSensitive: false))
                        {
                            doSkip = true;
                            break;
                        }
                    }

                    if (doSkip)
                        continue;
                }

                selectedItems.Add(i);
            }

            // Reuse Count
            Count = 0;

            // Reserve known amount of rows
            var rows = new DataGridViewRow[selectedItems.Count];

            for (int i = 0; i < selectedItems.Count; i++)
            {
                int n = selectedItems[i];

                bool isDir = (list[n].isDir);

                // Wasn't able to create new rows any other way
                // So had to stick to the cloning

                // Anyway, create new row and populate it with the data:
                var newRow = (DataGridViewRow)_myTemplateRow.Clone();
                buildRow(ref newRow, list[n], isDir, n);
                rows[Count++] = newRow;
            }

            // Add all the rows in a single motion
            // Less redrawing, less flickering
            _dataGrid.Rows.AddRange(rows);

            _dataGrid.ClearSelection();
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Collect all the selected entries
    // - or -
    // Restore the selection
    private void Collect_Or_Restore(PopulateReason reason, bool action)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.Collect_Or_Restore", "");
        #endif

        const bool doStoreState   = true;
        const bool doRestoreState = false;

        if (action == doStoreState)
        {
            switch (reason)
            {
                case PopulateReason.dirChanged:

                    if (currentlySelectedNames != null)
                        currentlySelectedNames.Clear();

                    if (currentlySelectedIds != null)
                        currentlySelectedIds.Clear();

                    break;

                // In case we're on the same directory as before, collect all the selected entries to be able to restore the selection later on
                case PopulateReason.filterChanged:
                case PopulateReason.viewDirChanged:
                case PopulateReason.viewFileChanged:

                    getSelectedIds(ref currentlySelectedIds);
                    break;

                case PopulateReason.recursionChanged_Before:

                    getSelectedNames(ref currentlySelectedNames);
                    break;

                case PopulateReason.recursionChanged_After:
                    break;
            }
        }

        if (action == doRestoreState)
        {
            switch (reason)
            {
                case PopulateReason.dirChanged:
                    break;

                // In case we're on the same directory as before, restore the selection
                case PopulateReason.filterChanged:
                case PopulateReason.viewDirChanged:
                case PopulateReason.viewFileChanged:

                    restoreSelectedIds(currentlySelectedIds);
                    break;

                case PopulateReason.recursionChanged_Before:
                    break;

                case PopulateReason.recursionChanged_After:

                    restoreSelectedNames(currentlySelectedNames);
                    break;
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Add files/derectories to the DataGridView from the List
    public void Populate(int dirsCount, int filesCount, bool doShowDirs, bool doShowFiles, PopulateReason reason, TreeNode selectedNode, string filterStr = "", string filterOutStr = "")
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.Populate", "");
        #endif

        // In case we're on the same directory as before, collect all the selected entries to be able to restore the selection later on
        Collect_Or_Restore(reason, true);

        _dataGrid.Rows.Clear();

        // The first time we populate our GridView, we create this template row that we'll use for cloning later
        if (_myTemplateRow == null)
        {
            _myTemplateRow = _dataGrid.Rows[_dataGrid.Rows.Add()];
            _dataGrid.Rows.Clear();
        }

        if (filterStr.Length == 0 && filterOutStr.Length == 0)
        {
            // Populate GridView with known amount of rows -- Single pass
            Populate_Fast(_globalFileListExtRef, dirsCount, filesCount, doShowDirs, doShowFiles);
        }
        else
        {
            // Populate GridView with unknown amount of rows -- Multiple pass
            Populate_Slow(_globalFileListExtRef, selectedNode, dirsCount, filesCount, doShowDirs, doShowFiles, filterStr, filterOutStr);
        }

        // Try to restore the selection from before:
        Collect_Or_Restore(reason, false);

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Store a list of files that are currently checked in the GridView
    private void getSelectedIds(ref System.Collections.Generic.HashSet<int> set)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.getSelectedIds", "");
        #endif

        if (set == null)
            set = new System.Collections.Generic.HashSet<int>();

        for (int i = 0; i < _dataGrid.Rows.Count; i++)
        {
            DataGridViewRow row = _dataGrid.Rows[i];

            bool isChecked = (bool)(row.Cells[(int)Columns.colChBox].Value);

            int id = (int)(row.Cells[(int)Columns.colId].Value);

            // If the item is checked, we store its id:
            if (isChecked)
            {
                set.Add(id);
            }
            else
            {
                set.Remove(id);
            }
        }
    }

    // --------------------------------------------------------------------------------------------------------

    // Restore checked state for the files stored in the set
    private void restoreSelectedIds(System.Collections.Generic.HashSet<int> set)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.restoreSelectedIds", "");
        #endif

        if (set.Count > 0)
        {
            for (int i = 0; i < _dataGrid.Rows.Count; i++)
            {
                DataGridViewRow row = _dataGrid.Rows[i];

                int id = (int)(row.Cells[(int)Columns.colId].Value);

                if (set.Contains(id))
                {
                    row.Cells[(int)Columns.colChBox].Value = true;
                }
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Get a list of files that are currently checked in the GridView
    private void getSelectedNames(ref System.Collections.Generic.HashSet<string> set)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.getSelectedNames", "");
        #endif

        if (set == null)
            set = new System.Collections.Generic.HashSet<string>();

        for (int i = 0; i < _dataGrid.Rows.Count; i++)
        {
            DataGridViewRow row = _dataGrid.Rows[i];

            bool isChecked = (bool)(row.Cells[(int)Columns.colChBox].Value);

            int id = (int)(row.Cells[(int)Columns.colId].Value);

            // If the item is checked, we store its name:
            if (isChecked)
            {
                set.Add(_globalFileListExtRef[id].Name);
            }
            else
            {
                set.Remove(_globalFileListExtRef[id].Name);
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Set checked state for the files from the list
    private void restoreSelectedNames(System.Collections.Generic.HashSet<string> set)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.restoreSelectedNames", "");
        #endif

        if (set.Count > 0)
        {
            for (int i = 0; i < _dataGrid.Rows.Count; i++)
            {
                DataGridViewRow row = _dataGrid.Rows[i];

                int id = (int)(row.Cells[(int)Columns.colId].Value);

                if (set.Contains(_globalFileListExtRef[id].Name))
                {
                    row.Cells[(int)Columns.colChBox].Value = true;
                }
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    public void getVisibleFiles(List<myTreeListDataItem> list)
    {
        list.Clear();

        var visibleRowsCount = _dataGrid.DisplayedRowCount(true);
        var firstVisibleRowIndex = _dataGrid.FirstDisplayedCell.RowIndex;
        var lastvisibleRowIndex  = (firstVisibleRowIndex + visibleRowsCount) - 1;

        StringBuilder sb = new StringBuilder(260);
        Dictionary<string, int> dic = new Dictionary<string, int>();

        for (int i = 0; i <= lastvisibleRowIndex; i++)
        {
            DataGridViewRow row = _dataGrid.Rows[i];

            int  id        =  (int)(row.Cells[(int)Columns.colId   ].Value);
            bool isChecked = (bool)(row.Cells[(int)Columns.colChBox].Value);

            if (isChecked)
            {
                enumerateFiles(id, ref dic, ref sb);
            }

            if (i >= firstVisibleRowIndex)
            {
                list.Add(_globalFileListExtRef[id]);
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Get a list of files that are currently checked in the GridView
    public void getSelectedFiles(List<myTreeListDataItem> list)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.getSelectedFiles", "");
        #endif

        list.Clear();
/*
        list.Add($"-- Total number of items in List: {originalFilesList.Count}");
        list.Add($"-- Total number of items in Grid: {_dataGrid.Rows.Count}");
*/
        StringBuilder sb = new StringBuilder(260);
        Dictionary<string, int> dic = new Dictionary<string, int>();

        // Get all checked directories and files from the grid
        for (int i = 0; i < _dataGrid.Rows.Count; i++)
        {
            DataGridViewRow row = _dataGrid.Rows[i];

            bool isChecked = (bool)(row.Cells[(int)Columns.colChBox].Value);

            // If the item is checked, we need to find it in the original list.
            // This is easy and fast, as we know its index in the original list,
            // and accessing item by index in the List is O(1)
            if (isChecked)
            {
                int id = (int)(row.Cells[(int)Columns.colId].Value);

                enumerateFiles(id, ref dic, ref sb);

                // Add unmodified file name
                // This way, list will contain references to original file names, and not the copies
                list.Add(_globalFileListExtRef[id]);
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Assign every dir/file a unique index number 1-2-3-... (will be used in renaming using template)
    // Indexes are unique within every directory
    private void enumerateFiles(int id, ref Dictionary<string, int> dic, ref StringBuilder sb)
    {
        var item = _globalFileListExtRef[id];

        sb.Clear();
        sb.Append(item.Name, 0, item.Name.LastIndexOf('\\'));
        sb.Append(item.isDir ? "?d" : "?f");

        string path = sb.ToString();

        item.Num = dic.ContainsKey(path) ? dic[path] + 1 : 1;
        dic[path] = item.Num;

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Customize the look of the whole widget
    public void OnPaint(object sender, PaintEventArgs e)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.OnPaint", "");
        #endif

        // Paint transparent box -- Simulate disabled state
        if (_dataGrid.Enabled == false)
        {
            e.Graphics.FillRectangle(_disabledStateBrush1, e.ClipRectangle);

            if (_doUseRecursion && _doShowRecursionMsg)
            {
                var font = _dataGrid.Font;
                //using (Font f = new Font(font.Name, 20, font.Style, font.Unit, font.GdiCharSet))
                using (Font f = new Font("Arial", 20, font.Style, font.Unit, font.GdiCharSet))
                {
                    e.Graphics.DrawString(_recursionMessage, f, Brushes.Gray, e.ClipRectangle, strFormat_CellId);
                }

                _doShowRecursionMsg = false;
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Customize the look of each cell
    // To be more precise, this customizes the appearance of each row as a whole
    // https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.datagridview.cellpainting?view=netframework-4.8
    private void on_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.on_CellPainting", "");
        #endif

        bool isCellIdVisible = _dataGrid.Columns[(int)Columns.colId].Visible;

        if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            HoverStatus hoverStatus = HoverStatus.DEFAULT;
            var row                 = _dataGrid.Rows[e.RowIndex];
            bool isRowSelected      = (e.State & DataGridViewElementStates.Selected) != 0;

            // Check hover status of the row (row.MinimumHeight is used as a flag here)
            if (row.MinimumHeight > (int)HoverStatus.DEFAULT)
            {
                hoverStatus = (HoverStatus)(row.MinimumHeight);

                // Reset row's hover status to default (but only if the mouse has left the row)
                if (hoverStatus == HoverStatus.MOUSE_HAS_LEFT && e.ColumnIndex == (int)Columns.colName)
                    row.MinimumHeight = (int)HoverStatus.DEFAULT;
            }

            // ---------------------------------------

            int x = e.CellBounds.X + 1;
            int y = e.CellBounds.Y + 1;
            int w = e.CellBounds.Width;
            int h = e.CellBounds.Height - 3;

            if (e.ColumnIndex > (int)Columns.colChBox)
            {
                x -= 1;
                w += 2;
            }

            // Paint background of a cell (white or gray)
            e.PaintBackground(e.CellBounds, false);

            // Paint gradient background
            paintGradientBgr(e, hoverStatus, isRowSelected, x, y, w, h);

            // Paint the contents of each cell
            paintCustomContent(e, hoverStatus, isRowSelected);

            // Paint custom border around each row
            paintCustomBorder(e, hoverStatus, isRowSelected, isCellIdVisible, x, y, w, h);

            // Paint custom editor in case the cell is being edited
            paintCustomEditControl(e, isRowSelected);

            e.Handled = true;
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    private void paintCustomContent(DataGridViewCellPaintingEventArgs e, HoverStatus hoverStatus, bool isRowSelected)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.paintCustomContent", "");
        #endif

        bool done = false;
        bool doPaintTooltip = (hoverStatus == HoverStatus.MOUSE_HOVER && e.ColumnIndex == (int)Columns.colName);

        // Paint Id column
        if (e.ColumnIndex == (int)Columns.colId)
        {
            var font = isRowSelected ? _cache.getCustomContentFont(e.CellStyle.Font) : e.CellStyle.Font;

            e.Graphics.DrawString(e.Value.ToString(), font, Brushes.Black, e.CellBounds, strFormat_CellId);
            done = true;
        }

        // Paint original content in original style
        if (!done && !doPaintTooltip)
        {
            e.PaintContent(e.CellBounds);
        }

        // Paint path in the right bottom corner on mouse hover
        if (doPaintTooltip)
        {
            paintCustomRowTooltip(e);
            paintSimulatedNameTooltip(e);
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Paint tooltip: path to current object in the right bottom corner on mouse hover
    private void paintCustomRowTooltip(DataGridViewCellPaintingEventArgs e)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.paintCustomRowTooltip", "");
        #endif

        Rectangle r = _cache.getRect(e.CellBounds.X, e.CellBounds.Y + 2, e.CellBounds.Width - 2, e.CellBounds.Height);
        Font f = _cache.getCellTooltipFont(e.CellStyle.Font);

        int id = (int)_dataGrid.Rows[e.RowIndex].Cells[(int)Columns.colId].Value;
        string tooltip = myUtils.condensePath(e, _globalFileListExtRef[id].Name, r, f, strFormat_CellName);

        e.Graphics.DrawString(tooltip, f, Brushes.Gray, r, strFormat_CellName);

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Paint tooltip: file name with every selected option applied
    private void paintSimulatedNameTooltip(DataGridViewCellPaintingEventArgs e)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.paintSimulatedNameTooltip", "");
        #endif

        if (_simulatedFileName != null)
        {
            Rectangle r = _cache.getRect(e.CellBounds.X + e.CellStyle.Padding.Left, e.CellBounds.Y, e.CellBounds.Width, e.CellBounds.Height);
            Font f = _cache.getCustomContentFont(e.CellStyle.Font);

            e.Graphics.DrawString(_simulatedFileName, f, Brushes.DarkRed, r, strFormat_CellNameTooltip);
        }
        else
        {
            e.PaintContent(e.CellBounds);
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Paint gradient background of a cell
    private void paintGradientBgr(DataGridViewCellPaintingEventArgs e, HoverStatus hoverStatus, bool isRowSelected, int x, int y, int w, int h)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.paintGradientBgr", "");
        #endif

        if (isRowSelected)
        {
            // Don't paint columnId cell
            if (e.ColumnIndex != (int)Columns.colId)
            {
                e.Graphics.FillRectangle(hoverStatus == HoverStatus.MOUSE_HOVER ? _gridGradientBrush2 : _gridGradientBrush1, x, y, w, h);

                if (_tabFocus == false)
                {
                    e.Graphics.FillRectangle(_disabledStateBrush2, x, y, w, h);
                }
            }
        }
    }

    // --------------------------------------------------------------------------------------------------------

    // Paint custom border around each row
    // I'm painting it at the time the last cell in a row is painted,
    // But it also needs to be partly restored in the first 2 cells when they're being selected
    private void paintCustomBorder(DataGridViewCellPaintingEventArgs e, HoverStatus hoverStatus, bool isRowSelected, bool isCellIdVisible, int x, int y, int w, int h)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.paintCustomBorder", "");
        #endif

        if (isCellIdVisible)
        {
            paintCustomBorder2(e, hoverStatus, isRowSelected, x, y, w, h);
        }
        else
        {
            paintCustomBorder1(e, hoverStatus, isRowSelected, x, y, w, h);
        }
    }

    // --------------------------------------------------------------------------------------------------------

    // Paint custom border around each row
    // I'm painting it at the time the last cell in a row is painted,
    // But it also needs to be partly restored in the first 2 cells when they're being selected
    // This version targets the case when columnId is invisible
    private void paintCustomBorder1(DataGridViewCellPaintingEventArgs e, HoverStatus hoverStatus, bool isSelected, int x, int y, int w, int h)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.paintCustomBorder1", "");
        #endif

        if (isSelected)
        {
            if (e.ColumnIndex <= (int)Columns.colName)
            {
                var customBorderPen = (hoverStatus == HoverStatus.MOUSE_HOVER) ? Pens.DarkMagenta : Pens.DarkOrange;

                if (e.ColumnIndex < (int)Columns.colName)
                {
                    h--;

                    e.Graphics.DrawLine(customBorderPen, x, y, x + w, y);
                    e.Graphics.DrawLine(customBorderPen, x, y + h, x + w, y + h);

                    if (e.ColumnIndex == (int)Columns.colChBox)
                        e.Graphics.DrawLine(customBorderPen, x, y, x, y + h);
                }
                else
                {
                    Rectangle rect = _cache.getRect(2, e.CellBounds.Y + 1, e.CellBounds.Width - 4 + e.CellBounds.X, e.CellBounds.Height - 4);
                    e.Graphics.DrawRectangle(customBorderPen, rect);
                }
            }
        }
        else
        {
            if (hoverStatus == HoverStatus.MOUSE_HOVER && (e.ColumnIndex <= (int)Columns.colName))
            {
                if (e.ColumnIndex < (int)Columns.colName)
                {
                    h--;

                    e.Graphics.DrawLine(Pens.DarkOrange, x, y, x + w, y);
                    e.Graphics.DrawLine(Pens.DarkOrange, x, y + h, x + w, y + h);

                    if (e.ColumnIndex == (int)Columns.colChBox)
                        e.Graphics.DrawLine(Pens.DarkOrange, x, y, x, y + h);
                }
                else
                {
                    Rectangle rect = _cache.getRect(2, e.CellBounds.Y + 1, e.CellBounds.Width - 4 + e.CellBounds.X, e.CellBounds.Height - 4);
                    e.Graphics.DrawRectangle(Pens.DarkOrange, rect);
                }
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Paint custom border around each row
    // I'm painting it at the time the last cell in a row is painted,
    // But it also needs to be partly restored in the first 2 cells when they're being selected
    // This version targets the case when columnId is visible
    private void paintCustomBorder2(DataGridViewCellPaintingEventArgs e, HoverStatus hoverStatus, bool isSelected, int x, int y, int w, int h)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.paintCustomBorder2", "");
        #endif

        bool doPaint = false;
        Pen customBorderPen = null;

        if (isSelected)
        {
            if (e.ColumnIndex <= (int)Columns.colName)
            {
                doPaint = true;
                customBorderPen = (hoverStatus == HoverStatus.MOUSE_HOVER) ? Pens.DarkMagenta : Pens.DarkOrange;
            }
        }
        else
        {
            if (hoverStatus == HoverStatus.MOUSE_HOVER && (e.ColumnIndex <= (int)Columns.colName))
            {
                doPaint = true;
                customBorderPen = Pens.DarkOrange;
            }
        }

        if (doPaint)
        {
            if (e.ColumnIndex == (int)Columns.colId)
            {
#if false
                int divider = _dataGrid.Columns[(int)Columns.colId].DividerWidth;

                Rectangle rect = _cache.getRect(2, e.CellBounds.Y + 1, e.CellBounds.Width - 4 + e.CellBounds.X - divider, e.CellBounds.Height - 4);
                e.Graphics.DrawRectangle(customBorderPen, rect);
#endif
            }
            else
            {
                if (e.ColumnIndex < (int)Columns.colName)
                {
                    h--;

                    e.Graphics.DrawLine(customBorderPen, x, y, x + w, y);
                    e.Graphics.DrawLine(customBorderPen, x, y + h, x + w, y + h);

                    if (e.ColumnIndex == (int)Columns.colChBox)
                        e.Graphics.DrawLine(customBorderPen, x, y, x, y + h);
                }
                else
                {
                    int colIdWidth = _dataGrid.Columns[(int)Columns.colId].Width;

                    Rectangle rect = _cache.getRect(colIdWidth + 2, e.CellBounds.Y + 1, e.CellBounds.Width - 4 + e.CellBounds.X - colIdWidth, e.CellBounds.Height - 4);
                    e.Graphics.DrawRectangle(customBorderPen, rect);
                }
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Custom painting of an editing control -- for a cell that is being edited
    private void paintCustomEditControl(DataGridViewCellPaintingEventArgs e, bool isRowSelected)
    {
        if (isRowSelected == true && e.ColumnIndex == (int)Columns.colName)
        {
            var cell = _dataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];

            if (cell.IsInEditMode)
            {
                Rectangle rect = _cache.getRect(e.CellBounds.X + 29, e.CellBounds.Y + 9, e.CellBounds.Width - 39, e.CellBounds.Height - 20);
                e.Graphics.DrawRectangle(Pens.White, rect);

                rect = _cache.getRect(e.CellBounds.X + 27, e.CellBounds.Y + 7, e.CellBounds.Width - 35, e.CellBounds.Height - 16);
                e.Graphics.DrawRectangle(Pens.Brown, rect);
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.on_CellMouseDown", "");
        #endif

        if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
        {
            _dataGrid.Rows[e.RowIndex].MinimumHeight = (int)(HoverStatus.MOUSE_HOVER);
        }
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.on_CellMouseUp", "");
        #endif

        if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // This call will also cause CellMouseLeave event
                myDataGrid_ContextMenu.showMenu(sender, e, _globalFileListExtRef, _doUseRecursion);
            }
        }
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.on_CellMouseEnter", "");
        #endif

        // Change row's appearance while mouse is hovering upon it
        _dataGrid.Rows[e.RowIndex].MinimumHeight = (int)(HoverStatus.MOUSE_HOVER);
        _dataGrid.InvalidateRow(e.RowIndex);
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.on_CellMouseLeave", "");
        #endif

        // Change row's appearance when mouse is leaving it
        if (_dataGrid.Rows[e.RowIndex].MinimumHeight == (int)(HoverStatus.MOUSE_HOVER))
            _dataGrid.Rows[e.RowIndex].MinimumHeight = (int)(HoverStatus.MOUSE_HAS_LEFT);

        _dataGrid.InvalidateRow(e.RowIndex);
    }

    // --------------------------------------------------------------------------------------------------------

    // Manual cell edit -- Control Showing
    private void on_CellControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
    {
        var tb = e.Control as TextBox;

        if (tb != null)
        {
            if (tb.Tag == null)
            {
                // Needs to be done only once, as the same control is actually reused in each editing session
                tb.KeyDown += new KeyEventHandler(keyDownHandler);
                tb.Tag = 1;
            }
        }

        // ----------------------------------------------------------

        // KeyDown Handler for an Editing Control of the DataGrid.
        // In order to trap some keys, [myDataGridViewTextBoxEditingControl] class has been introduced
        void keyDownHandler(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;

            switch (e.KeyCode)
            {
                // Ctrl+Backspace
                case Keys.Back: {

                        if (e.Modifiers == Keys.Control)
                        {
                            int start = tb.SelectionStart;

                            if (start <= tb.TextLength)
                            {
                                var sb = new StringBuilder();

                                // Find the first available stop position
                                int pos = myUtils.findStopPosition(tb.Text, start, moveRight: false);

                                // Decide what to do with the stop position (if found):
                                if (pos < start && pos != -1)
                                {
                                    sb.Append(tb.Text, 0, pos);
                                }

                                sb.Append(tb.Text, start, tb.TextLength - start);

                                tb.Text = sb.ToString();
                                tb.SelectionStart = pos >= 0 ? pos : 0;     // Put caret where it belongs
                                e.Handled = true;
                            }

                            e.SuppressKeyPress = true;
                        }
                    }
                    break;

                // Ctrl+Delete
                case Keys.Delete: {

                        if (e.Modifiers == Keys.Control && tb.TextLength > 0)
                        {
                            int start = tb.SelectionStart;

                            if (start < tb.TextLength)
                            {
                                var sb = new StringBuilder();
                                sb.Append(tb.Text, 0, start);

                                // Find the first available stop position
                                int pos = myUtils.findStopPosition(tb.Text, start, moveRight: true);

                                // Decide what to do with the stop position (if found):
                                if (pos == start)
                                {
                                    sb.Append(tb.Text, start + 1, tb.TextLength - start - 1);
                                }

                                if (pos > start)
                                {
                                    sb.Append(tb.Text, pos, tb.TextLength - pos);
                                }

                                tb.Text = sb.ToString();
                                tb.SelectionStart = start;      // Put caret where it belongs
                                e.Handled = true;
                            }
                        }
                    }
                    break;

                // Ctrl+Right
                case Keys.Right: {

                        if (e.Modifiers == Keys.Control && tb.TextLength > 0)
                        {
                            int start = tb.SelectionStart;

                            if (start < tb.TextLength)
                            {
                                // Find the first available stop position
                                int pos = myUtils.findStopPosition(tb.Text, start, moveRight: true);

                                tb.SelectionStart = pos >= 0 ? pos : tb.TextLength;
                                e.Handled = true;
                            }
                        }
                    }
                    break;

                // Left, Ctrl+Left
                case Keys.Left: {

                        int start = tb.SelectionStart;

                        if (start == 0)
                        {
                            // As a default behaviour, GridView will stop editing and attempt to select the cell on the left of this cell
                            // To prevent this:
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                        }
                        else
                        {
                            if (e.Modifiers == Keys.Control && tb.TextLength > 0)
                            {
                                if (start <= tb.TextLength)
                                {
                                    // Find the first available stop position
                                    int pos = myUtils.findStopPosition(tb.Text, start, moveRight: false);

                                    tb.SelectionStart = pos >= 0 ? pos : 0;
                                    e.Handled = true;
                                }
                            }
                        }
                    }
                    break;
            }

            return;
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Manual cell edit -- Begin
    private void on_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
    {
        if (e.ColumnIndex == (int)Columns.colName)
        {
            var cell = _dataGrid.CurrentCell;
        }
    }

    // --------------------------------------------------------------------------------------------------------

    // Manual cell edit -- End
    private void on_CellEndEdit(object sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == (int)Columns.colName)
        {
            int id = (int)_dataGrid.Rows[e.RowIndex].Cells[(int)Columns.colId].Value;
            var cell = _dataGrid.CurrentCell;

            string old = _globalFileListExtRef[id].Name;
            old = old.Substring((old.LastIndexOf('\\') + 1));

            if (cell.Value != null)
            {
                // If Ecs key is hit, the name in the cell will be restored to the original value, but we still will reach this point
                if (cell.Value.ToString() != old)
                {
                    try
                    {
                        var list = new List<myTreeListDataItem>();
                        list.Add(_globalFileListExtRef[id].Clone());

                        if (!myRenamer.getInstance().RenameManual(list, cell.Value.ToString()))
                        {
                            cell.Value = old;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "myRenamerApp: Failed to rename a file", MessageBoxButtons.OK);
                    }
                }
            }
            else
            {
                cell.Value = old;
            }

            cell.ReadOnly = true;
        }
    }

    // --------------------------------------------------------------------------------------------------------

    // Key Down Event
    private void on_KeyDown(object sender, KeyEventArgs e)
    {
#if DEBUG_TRACE
        myUtils.logMsg("myDataGrid.on_KeyDown", "");
#endif

        int currRow = _dataGrid.RowCount > 0 ? _dataGrid.CurrentRow.Index : -1;

        switch (e.KeyCode)
        {
            // Default widget's reaction
            case Keys.PageUp:
            case Keys.PageDown:
            case Keys.F4:
            case Keys.F5:
                return;

            // Ctrl+A: on the first click, uses default widget's reaction
            // If then pressed repeatedly, will toggle the state of every row in the grid
            // todo: do we need to toggle or just to check everything?
            case Keys.A: {

                    if (e.Modifiers == Keys.Control && _dataGrid.SelectedRows.Count == _dataGrid.Rows.Count)
                    {
                        // Change checked state of each selected row
                        for (int i = 0; i < _dataGrid.RowCount; i++)
                        {
                            if (_dataGrid.Rows[i].Selected)
                            {
                                var cb = _dataGrid.Rows[i].Cells[(int)Columns.colChBox];
                                cb.Value = !(bool)(cb.Value);
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                break;

            // Ctrl+C / Ctrl+Ins: copy file name from 'Name' cell
            case Keys.Insert:
            case Keys.C: {

                    if (e.Modifiers == Keys.Control)
                    {
                        var cell = _dataGrid[(int)Columns.colName, currRow];

                        if (cell.Value != null)
                        {
                            string text = cell.Value.ToString();

                            if (text.Length > 0)
                                Clipboard.SetText(text);
                            else
                                Clipboard.Clear();
                        }
                    }
                }
                break;

            // Up and Down arrow keys work mostly as expected
            // The only problem arises in the following case: Shift + End --> (not releasing Shift) --> Up Arrow
            // The selection is lost, instead of subtracting items from the selection
            // In order to address that, added 2 additional handlers for Up and Down keys:
            case Keys.Up: {

                    if (e.Modifiers == Keys.Shift && _dataGrid.SelectedRows.Count > 1)
                    {
                        int cnt = 0, i = currRow;

                        // Make sure the selection is contiguous
                        for (; i >= 0; i--)
                        {
                            if (!_dataGrid.Rows[i].Selected)
                            {
                                i++;
                                break;
                            }

                            cnt++;
                        }

                        if (cnt == _dataGrid.SelectedRows.Count)
                        {
                            i = (i < 0) ? 0 : i;
                            currRow--;
                            _dataGrid.CurrentCell = _dataGrid[0, currRow];

                            for (; i != currRow; i++)
                                _dataGrid.Rows[i].Selected = true;

                            e.Handled = true;
                        }
                    }
                }
                return;

            // todo: fix this:
            // shitf+up --> select some files. then shift+down; then shift+up again --> selection breaks
            case Keys.Down: {

                    if (e.Modifiers == Keys.Shift && _dataGrid.SelectedRows.Count > 1)
                    {
                        int cnt = 0, i = currRow;

                        // Make sure the selection is contiguous
                        for (; i < _dataGrid.RowCount; i++)
                        {
                            if (!_dataGrid.Rows[i].Selected)
                                break;

                            cnt++;
                        }

                        if (cnt == _dataGrid.SelectedRows.Count)
                        {
                            currRow++;
                            _dataGrid.CurrentCell = _dataGrid[0, currRow];

                            for (cnt = currRow; cnt < i; cnt++)
                                _dataGrid.Rows[cnt].Selected = true;

                            e.Handled = true;
                        }
                    }
                }
                return;

            case Keys.Space: {

                    // Change checked state of each selected row
                    toggleSelectedCheckboxes();
                }
                break;

            case Keys.Home: {

                    if (_dataGrid.Rows.Count > 0)
                    {
                        List<int> list = null;

                        // Store current selection
                        if (_dataGrid.SelectedRows.Count > 0)
                        {
                            list = new List<int>();
                            for (int i = 0; i < _dataGrid.SelectedRows.Count; i++)
                                list.Add(_dataGrid.SelectedRows[i].Index);
                        }

                        _dataGrid.CurrentCell = _dataGrid[0, 0];                            // Jump home

                        if ((e.Modifiers & Keys.Shift) == Keys.Shift)
                        {
                            for (int i = 0; i <= currRow; i++)
                                _dataGrid.Rows[i].Selected = true;                          // Select everything from current up to home

                            if (list != null)
                                for (int i = 0; i < list.Count; i++)
                                    _dataGrid.Rows[list[i]].Selected = true;                // Restore old selection
                        }
                    }
                }
                break;

            case Keys.End: {

                    if (_dataGrid.Rows.Count > 0)
                    {
                        List<int> list = null;
                        int curr = _dataGrid.CurrentRow.Index;

                        // Store current selection
                        if (_dataGrid.SelectedRows.Count > 0)
                        {
                            list = new List<int>();
                            for (int i = 0; i < _dataGrid.SelectedRows.Count; i++)
                                list.Add(_dataGrid.SelectedRows[i].Index);
                        }

                        _dataGrid.CurrentCell = _dataGrid[0, _dataGrid.Rows.Count-1];       // Jump to the bottom

                        if ((e.Modifiers & Keys.Shift) == Keys.Shift)
                        {
                            for (int i = currRow; i < _dataGrid.Rows.Count; i++)
                                _dataGrid.Rows[i].Selected = true;                          // Select everything from current down to the bottom

                            if (list != null)
                                for (int i = 0; i < list.Count; i++)
                                    _dataGrid.Rows[list[i]].Selected = true;                // Restore old selection
                        }
                    }
                }
                break;

            // Rename a file in the grid manually
            case Keys.F2: {

                    var cell = _dataGrid[(int)Columns.colName, currRow];
                    _dataGrid.CurrentCell = cell;
                    cell.ReadOnly = false;
                    _dataGrid.BeginEdit(false);
                }
                break;
        }

        e.Handled = true;

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Set double buffering to reduce flickering
    private void setDoubleBuffering()
    {
#if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.setDoubleBuffering", "");
#endif

        // https://stackoverflow.com/questions/41893708/how-to-prevent-datagridview-from-flickering-when-scrolling-horizontally
        PropertyInfo pi = _dataGrid.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
        pi.SetValue(_dataGrid, true, null);
    }

    // --------------------------------------------------------------------------------------------------------

    public void setRecursiveMode(bool mode)
    {
#if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.setRecursiveMode", "");
#endif

        _doUseRecursion = mode;

        // Also, store the list of files selected by user
        // This needs to be done BEFORE [myTree_DataGrid_Manager.tree_onAfterSelect] is called,
        // as it will change the global file list contents
        Collect_Or_Restore(PopulateReason.recursionChanged_Before, true);

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    public void displayRecursionMsg(string msg)
    {
#if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.displayRecursionMsg", "");
#endif

        _recursionMessage = msg;
        _doShowRecursionMsg = true;
        _dataGrid.Invalidate();
    }

    // --------------------------------------------------------------------------------------------------------

    // Update DataGrid's state
    public void update(System.Collections.Generic.List<myTreeListDataItem> updatedList = null)
    {
#if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.update", "");
#endif

        if (updatedList != null)
        {
            // Update from the supplied [updateList]
            for (int i = 0, j = 0; i < _dataGrid.Rows.Count && j != updatedList.Count; i++)
            {
                DataGridViewRow row = _dataGrid.Rows[i];

                int id = (int)row.Cells[(int)Columns.colId].Value;

                if (id == updatedList[j].Id)
                {
                    int pos = updatedList[j].Name.LastIndexOf('\\') + 1;
                    row.Cells[(int)Columns.colName].Value = updatedList[j].Name.Substring(pos);

                    _globalFileListExtRef[id].Name = updatedList[j++].Name;
                }
            }
        }
        else
        {
            // Update from global list (default option)
            for (int i = 0; i < _dataGrid.Rows.Count; i++)
            {
                DataGridViewRow row = _dataGrid.Rows[i];

                int id = (int)row.Cells[(int)Columns.colId].Value;

                string rowName = (string)row.Cells[(int)Columns.colName].Value;

                if (rowName != _globalFileListExtRef[id].Name)
                {
                    int pos = _globalFileListExtRef[id].Name.LastIndexOf('\\') + 1;
                    row.Cells[(int)Columns.colName].Value = _globalFileListExtRef[id].Name.Substring(pos);

                    _dataGrid.InvalidateRow(i);
                }
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Set Enabled or Disabled look of the widget
    // Visual style applies via OnPaint() event
    public void Enable(bool mode)
    {
#if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.Enable", "");
#endif

        _doShowRecursionMsg = false;
        _dataGrid.Enabled = mode;
    }

    // --------------------------------------------------------------------------------------------------------

    // Set simulated name -- to use it as a hover tooltip
    public void setSimulatedName(string name)
    {
#if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.setSimulatedName", "");
#endif

        _simulatedFileName = name;
    }

    // --------------------------------------------------------------------------------------------------------

    public void setFilterMode(bool mode)
    {
        _filterMode = mode;
    }

    // --------------------------------------------------------------------------------------------------------

    public bool getTabFocus()
    {
        return _tabFocus;
    }

    // --------------------------------------------------------------------------------------------------------

    public void setTabFocus(bool mode)
    {
        if (_tabFocus != mode)
        {
            _tabFocus = mode;

            // To redraw selected rows when _dataGrid obtains/loses focus
            _dataGrid.Invalidate();
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Change checked state of each selected row
    public void toggleSelectedCheckboxes()
    {
        for (int i = 0; i < _dataGrid.RowCount; i++)
        {
            if (_dataGrid.Rows[i].Selected)
            {
                var cb = _dataGrid.Rows[i].Cells[(int)Columns.colChBox];
                cb.Value = !(bool)(cb.Value);
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------
};
