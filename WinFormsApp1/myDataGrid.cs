using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

/*
    Wrapper class around DataGridView widget.
    Allows customization and provides public methods to work with the widget.
*/

public class myDataGrid
{
    private DataGridView    _dataGrid      = null;
    private DataGridViewRow _myTemplateRow = null;

    private bool _doUseRecursion = false;
    private bool _doShowRecursionMsg = false;

    private Image _imgDir         = null;
    private Image _imgFile        = null;
    private Image _imgDir_Opaque  = null;
    private Image _imgFile_Opaque = null;

    private Brush _gridGradientBrush1 = null;
    private Brush _gridGradientBrush2 = null;
    private Brush _disabledStateBrush = null;

    // Going to hold a reference to the global list of files
    private readonly List<myTreeListDataItem> _globalFileListExtRef = null;

    // Containers to store the state of selected items between grid repopulations
    private HashSet<   int> currentlySelectedIds   = null;
    private HashSet<string> currentlySelectedNames = null;

    // StringFormat for CellId custom text drawing
    private StringFormat strFormat_CellId   = null;
    private StringFormat strFormat_CellName = null;

    private myDataGrid_Cache _cache = null;

    private string _recursionMessage = "";

    // --------------------------------------------------------------------------------------------------------

    public enum Columns
    {
        colId, colChBox, colImage, colName
    };

    public enum PopulateReason
    {
        dirChanged, viewFileChanged, viewDirChanged, filterChanged, recursionChanged_Before, recursionChanged_After
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
            _dataGrid.RowTemplate.MinimumHeight = 2;                                        // Will be used as a flag for on_MouseEnter / on_MouseLeave events

            _dataGrid.SelectionMode   = DataGridViewSelectionMode.FullRowSelect;            // Row select mode
            _dataGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;       // Cell borders
            _dataGrid.EditMode        = DataGridViewEditMode.EditOnF2;                      // How to edit cell (press F2)

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

        _disabledStateBrush = new SolidBrush(Color.FromArgb(150, Color.White));

        strFormat_CellId = new StringFormat(StringFormatFlags.NoClip);
        strFormat_CellId.LineAlignment = StringAlignment.Center;
        strFormat_CellId.Alignment = StringAlignment.Center;

        strFormat_CellName = new StringFormat(StringFormatFlags.NoWrap);
        strFormat_CellName.LineAlignment = StringAlignment.Near;
        strFormat_CellName.Alignment = StringAlignment.Far;

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Add and subscribe to events
    private void setUpEvents()
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.setUpEvents", "");
        #endif

        // Customize whole widget appearance (not used for now)
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
        var cellTemplate = new myDataGridViewCheckBoxCell();
            cellTemplate.CustomDrawing = myDataGridViewCheckBoxCell.DrawMode.Custom3;
            cellTemplate.CustomSize = 26;
            cellTemplate.CustomActiveAreaMargin = 5;
            cellTemplate.CustomImg("icon-tick-box1-checked-64.png",   myDataGridViewCheckBoxCell.cbMode.Checked);
            cellTemplate.CustomImg("icon-tick-box1-unchecked-64.png", myDataGridViewCheckBoxCell.cbMode.Unchecked);
        columnCheckBox.CellTemplate = cellTemplate;
        columnCheckBox.Width = 50;
        columnCheckBox.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        columnCheckBox.Resizable = DataGridViewTriState.False;

        // Icon column Columns.colImage
        var columnImage = new DataGridViewImageColumn();
        columnImage.Width = 30;
        columnImage.Resizable = DataGridViewTriState.False;

        // Text column Columns.colName (auto adjusted to fill all the available width)
        var columnName = new DataGridViewTextBoxColumn();
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

            row.DefaultCellStyle.ForeColor = isDir ? System.Drawing.Color.Black : System.Drawing.Color.Brown;

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

    // Populate GridView with unknown amount of rows
    // Multiple pass
    private void Populate_Slow(List<myTreeListDataItem> list, int dirsCount, int filesCount, bool doShowDirs, bool doShowFiles, string filterStr, string filterOutStr)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.Populate_Slow", "");
        #endif

        int Count = 0;
        Count += doShowDirs  ? dirsCount  : 0;
        Count += doShowFiles ? filesCount : 0;

        if (Count > 0)
        {
            // Select only the indexes of the items we need
            var selectedItems = new List<int>();

            for(int i = 0; i < list.Count; i++)
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

                int filePos = list[i].Name.LastIndexOf('\\') + 1;

                // Skip everything that does not match the search string [filterStr]
                if (filterStr.Length > 0 && !myUtils.fastStrContains(filterStr, list[i].Name, filePos, -1, false))
                    continue;

                // Skip everything that DOES match the search string [filterOutStr]
                if (filterOutStr.Length > 0 && myUtils.fastStrContains(filterOutStr, list[i].Name, filePos, -1, false))
                    continue;

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
    public void Populate(int dirsCount, int filesCount, bool doShowDirs, bool doShowFiles, PopulateReason reason, string filterStr = "", string filterOutStr = "")
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
            Populate_Slow(_globalFileListExtRef, dirsCount, filesCount, doShowDirs, doShowFiles, filterStr, filterOutStr);
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

    // Get a list of files that are currently checked in the GridView
    public void getSelectedFiles(System.Collections.Generic.List<myTreeListDataItem> list)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.getSelectedFiles", "");
        #endif

        list.Clear();
/*
        list.Add($"-- Total number of items in List: {originalFilesList.Count}");
        list.Add($"-- Total number of items in Grid: {_dataGrid.Rows.Count}");
*/
        // Get all checked directories and files
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

                // Add unmodified file name
                // This way, list will contain references to original file names, and not the copies
                list.Add(_globalFileListExtRef[id]);
            }
        }

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
            e.Graphics.FillRectangle(_disabledStateBrush, e.ClipRectangle);

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
            char hoverStatus   = '0';
            var row            = _dataGrid.Rows[e.RowIndex];
            bool isRowSelected = (e.State & DataGridViewElementStates.Selected) != 0;

            // Check hover status of the row
            if (row.MinimumHeight > 2)
            {
                hoverStatus = row.MinimumHeight == 4 ? '2' : '1';

                // Reset row's hover status to default (but only if the mouse has left the row)
                if (hoverStatus == '3' && e.ColumnIndex == (int)Columns.colName)
                    row.MinimumHeight = 2;
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

            e.Handled = true;
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    private void paintCustomContent(DataGridViewCellPaintingEventArgs e, char hoverStatus, bool isRowSelected)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.paintCustomContent", "");
        #endif

        bool done = false;

        if (e.ColumnIndex == (int)Columns.colId)
        {
            var font = isRowSelected ? _cache.getCustomContentFont(e.CellStyle.Font) : e.CellStyle.Font;

            e.Graphics.DrawString(e.Value.ToString(), font, Brushes.Black, e.CellBounds, strFormat_CellId);
            done = true;
        }

        if (!done)
        {
            // Paint original content in original style
            e.PaintContent(e.CellBounds);
        }

        // Paint path in the right bottom corner on mouse hover
        if (hoverStatus == '2' && e.ColumnIndex == (int)Columns.colName)
        {
            paintCustomRowTooltip(e);
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

    // Paint gradient background of a cell
    private void paintGradientBgr(DataGridViewCellPaintingEventArgs e, char hoverStatus, bool isRowSelected, int x, int y, int w, int h)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.paintGradientBgr", "");
        #endif

        if (isRowSelected)
        {
            // Skip columnId
            if (e.ColumnIndex != (int)Columns.colId)
            {
                e.Graphics.FillRectangle(hoverStatus == '2' ? _gridGradientBrush2 : _gridGradientBrush1, x, y, w, h);
            }
        }
    }

    // --------------------------------------------------------------------------------------------------------

    // Paint custom border around each row
    // I'm painting it at the time the last cell in a row is painted,
    // But it also needs to be partly restored in the first 2 cells when they're being selected
    private void paintCustomBorder(DataGridViewCellPaintingEventArgs e, char hoverStatus, bool isRowSelected, bool isCellIdVisible, int x, int y, int w, int h)
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
    private void paintCustomBorder1(DataGridViewCellPaintingEventArgs e, char hoverStatus, bool isSelected, int x, int y, int w, int h)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.paintCustomBorder1", "");
        #endif

        if (isSelected)
        {
            if (e.ColumnIndex <= (int)Columns.colName)
            {
                var customBorderPen = (hoverStatus == '2') ? Pens.DarkMagenta : Pens.DarkOrange;

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
            if (hoverStatus == '2' && (e.ColumnIndex <= (int)Columns.colName))
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
    private void paintCustomBorder2(DataGridViewCellPaintingEventArgs e, char hoverStatus, bool isSelected, int x, int y, int w, int h)
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
                customBorderPen = (hoverStatus == '2') ? Pens.DarkMagenta : Pens.DarkOrange;
            }
        }
        else
        {
            if (hoverStatus == '2' && (e.ColumnIndex <= (int)Columns.colName))
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

    private void on_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.on_CellMouseDown", "");
        #endif

        if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
        {
            _dataGrid.Rows[e.RowIndex].MinimumHeight = 4;
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
        _dataGrid.Rows[e.RowIndex].MinimumHeight = 4;
        _dataGrid.InvalidateRow(e.RowIndex);
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
    {
        #if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.on_CellMouseLeave", "");
        #endif

        // Change row's appearance when mouse is leaving it
        if (_dataGrid.Rows[e.RowIndex].MinimumHeight == 4)
            _dataGrid.Rows[e.RowIndex].MinimumHeight = 3;

        _dataGrid.InvalidateRow(e.RowIndex);
    }

    // --------------------------------------------------------------------------------------------------------

    // Key Down Event
    private void on_KeyDown(object sender, KeyEventArgs e)
    {
#if DEBUG_TRACE
            myUtils.logMsg("myDataGrid.on_KeyDown", "");
#endif

        int currRow = _dataGrid.CurrentRow.Index;

        switch (e.KeyCode)
        {
            // Default widget's reaction
            case Keys.Up:
            case Keys.PageUp:
            case Keys.Down:
            case Keys.PageDown:
            case Keys.F4:
                return;

            case Keys.Space: {

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
                break;

            case Keys.Home: {

                    if (_dataGrid.Rows.Count > 0)
                    {
                        _dataGrid.ClearSelection();
                        _dataGrid.CurrentCell = _dataGrid[0, 0];                            // Jump home

                        if (e.Modifiers == Keys.Shift)                                      // Select everything from current up to home
                            for (int i = currRow; i >= 0; i--)
                                _dataGrid.Rows[i].Selected = true;

                        _dataGrid.CurrentRow.Selected = true;
                    }
                }
                break;

            case Keys.End: {

                    if (_dataGrid.Rows.Count > 0)
                    {
                        _dataGrid.ClearSelection();
                        _dataGrid.CurrentCell = _dataGrid[0, _dataGrid.Rows.Count - 1];     // Jump to the botton

                        if (e.Modifiers == Keys.Shift)                                      // Select everything from current down to the bottom
                            for (int i = currRow; i < _dataGrid.Rows.Count; i++)
                                _dataGrid.Rows[i].Selected = true;

                        _dataGrid.CurrentRow.Selected = true;
                    }
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
        _doShowRecursionMsg = false;
        _dataGrid.Enabled = mode;
    }

    // --------------------------------------------------------------------------------------------------------

};
