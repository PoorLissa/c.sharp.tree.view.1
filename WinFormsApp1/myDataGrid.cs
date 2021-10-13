using System;
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

    private Image  _imgDir  = null;
    private Image  _imgFile = null;
    private Bitmap _rowImg  = null;

    private enum Columns
    {
        colCheckBox = 0,
        colImage,
        colName,
        colNumber
    };

    // Going to hold a reference to the global list of files
    private readonly System.Collections.Generic.List<string> _globalFileListRef = null;

    // --------------------------------------------------------------------------------------------------------

    public myDataGrid(DataGridView dgv, System.Collections.Generic.List<string> list)
    {
        _dataGrid = dgv;
        _globalFileListRef = list;

        init();
    }

    // --------------------------------------------------------------------------------------------------------

    public void init()
    {
        // https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-work-with-image-columns-in-the-windows-forms-datagridview-control?view=netframeworkdesktop-4.8
        if (_dataGrid != null)
        {
            int dpi = _dataGrid.DeviceDpi;

            setDoubleBuffering();

            _imgDir  = Image.FromFile(myUtils.getFilePath("_icons", "icons8-opened-folder-2-16.png"));
            _imgFile = Image.FromFile(myUtils.getFilePath("_icons", "icons8-file-16.png"));

            // Add and subscribe to events
            setUpEvents();

            _dataGrid.RowTemplate.Height = dpi > 96 ? 50 : 30;                              // Row height

            _dataGrid.SelectionMode   = DataGridViewSelectionMode.FullRowSelect;            // Row select mode
            _dataGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;       // Cell borders
            _dataGrid.EditMode        = DataGridViewEditMode.EditOnF2;                      // How to edit cell (press F2)

            _dataGrid.AllowUserToAddRows    = false;                                        // User can't add new rows
            _dataGrid.AllowUserToResizeRows = false;                                        // User can't resize rows
            _dataGrid.ColumnHeadersVisible  = false;                                        // No column headers
            _dataGrid.RowHeadersVisible     = false;                                        // No row headers

            // Grid Colors
            _dataGrid.DefaultCellStyle.SelectionBackColor = Color.DarkOrange; // Row selection color
            _dataGrid.GridColor = Color.LightGray;
            _dataGrid.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Control;
            _dataGrid.BackgroundColor = _dataGrid.DefaultCellStyle.BackColor; // Instead of OnPaint event

            // Add Columns
            addColumns();

            createDrawingPrimitives(dpi);
        }
    }

    // --------------------------------------------------------------------------------------------------------

    private void createDrawingPrimitives(int dpi)
    {
/*
        var pt1 = new Point(0, 0);
        var pt2 = new Point(0, _dataGrid.RowTemplate.Height);

        private System.Drawing.Brush _gridGradientBrush1 = null;
        _gridGradientBrush1 = new System.Drawing.Drawing2D.LinearGradientBrush(pt1, pt2,
            Color.FromArgb(150, 204, 232, 255),
            Color.FromArgb(255, 190, 220, 255));
*/
    }

    // --------------------------------------------------------------------------------------------------------

    // Add and subscribe to events
    private void setUpEvents()
    {
        // Customize whole widget appearance (not used for now)
        //_dataGrid.Paint += new PaintEventHandler(OnPaint);

        // Customize individual cell appearance
        _dataGrid.CellPainting += new DataGridViewCellPaintingEventHandler(on_CellPainting);

        // Mouse events
        _dataGrid.CellMouseDown += new DataGridViewCellMouseEventHandler(on_CellMouseDown);
        _dataGrid.CellMouseUp   += new DataGridViewCellMouseEventHandler(on_CellMouseUp);

        // Keyboard events
        _dataGrid.KeyDown += new KeyEventHandler(on_KeyDown);
    }

    // --------------------------------------------------------------------------------------------------------

    private void addColumns()
    {
        // Checkbox column Columns.colCheckBox
        var checkBoxColumn = new DataGridViewCheckBoxColumn();
        checkBoxColumn.Width = 50;
        checkBoxColumn.Resizable = DataGridViewTriState.False;
        _dataGrid.Columns.Add(checkBoxColumn);

        // Icon column Columns.colImage
        var imageColumn = new DataGridViewImageColumn();
        imageColumn.Width = 30;
        imageColumn.Resizable = DataGridViewTriState.False;
        _dataGrid.Columns.Add(imageColumn);

        // Text column Columns.colName (auto adjusted to fill all the available width)
        var textColumn = new DataGridViewTextBoxColumn();
        textColumn.Name = "Name";
        textColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _dataGrid.Columns.Add(textColumn);

        // Row number column Columns.colNumber
        var numberColumn = new DataGridViewTextBoxColumn();
        numberColumn.Name = "Num";
        numberColumn.Visible = false;
        _dataGrid.Columns.Add(numberColumn);

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    public void Clear()
    {
        _dataGrid.Rows.Clear();
    }

    // --------------------------------------------------------------------------------------------------------

    // Modify a copy of a template row and give it real values
    public void buildRow(ref DataGridViewRow row, string item, bool isDir, int num)
    {
        if (item.Length > 0)
        {
            int pos = item.LastIndexOf('\\') + 1;

            row.Cells[(int)Columns.colCheckBox].Value = false;
            row.Cells[(int)Columns.colImage   ].Value = isDir ? _imgDir : _imgFile;
            row.Cells[(int)Columns.colName    ].Value = item[pos..];
            row.Cells[(int)Columns.colNumber  ].Value = num;

            row.DefaultCellStyle.ForeColor = isDir ? System.Drawing.Color.Black : System.Drawing.Color.Brown;
        }
    }

    // --------------------------------------------------------------------------------------------------------

    // Add a single item to the DataGridView
    // Not used
    public void addRow(string item, bool isDir)
    {
        int pos = item.LastIndexOf('\\') + 1;
        int i = _dataGrid.Rows.Add();

        if (item.Length > 0)
        {
            if (isDir)
            {
                _dataGrid.Rows[i].DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            }
            else
            {
                _dataGrid.Rows[i].DefaultCellStyle.ForeColor = System.Drawing.Color.Brown;
            }

            _dataGrid.Rows[i].Cells[(int)Columns.colCheckBox].Value = false;
            _dataGrid.Rows[i].Cells[(int)Columns.colImage   ].Value = isDir ? _imgDir : _imgFile;
            _dataGrid.Rows[i].Cells[(int)Columns.colName    ].Value = item[pos..];
            _dataGrid.Rows[i].Cells[(int)Columns.colNumber  ].Value = 999;
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Populate GridView with known amount of rows
    // Single pass
    private void Populate_Fast(System.Collections.Generic.List<string> list, int dirsCount, int filesCount, bool doShowDirs, bool doShowFiles)
    {
/*
        // check the memory impact
        if (true)
        {
            int CNT = 130000, cnt = 0;
            var rows = new DataGridViewRow[CNT];

            for (int i = 0; i < CNT; i++)
            {
                var newRow = (DataGridViewRow)_myTemplateRow.Clone();
                buildRow(ref newRow, item, true);
                rows[cnt++] = newRow;
            }

            _dataGrid.Rows.AddRange(rows);
            _dataGrid.ClearSelection();
            return;
        }
*/
        int Count = 0, i = -1;
        Count += doShowDirs ? dirsCount : 0;
        Count += doShowFiles ? filesCount : 0;

        if (Count > 0)
        {
            // Reserve known amount of rows
            var rows = new DataGridViewRow[Count];

            // Reuse Count
            Count = 0;

            // Display all directories and files
            foreach (var item in list)
            {
                i++;

                bool isDir = (item[0] == '1');

                // Show/skip directories
                if (isDir && !doShowDirs)
                    continue;

                // Show/skip files
                if (!isDir && !doShowFiles)
                    continue;

                // Skip the delimiter
                if (item[0] == '2')
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
    private void Populate_Slow(System.Collections.Generic.List<string> list, int dirsCount, int filesCount, bool doShowDirs, bool doShowFiles, string filterStr)
    {
        int Count = 0;
        Count += doShowDirs  ? dirsCount  : 0;
        Count += doShowFiles ? filesCount : 0;

        if (Count > 0)
        {
            filterStr = filterStr.ToLower();

            // Select only the indexes of the items we need
            var selectedItems = new System.Collections.Generic.List<int>();

            for(int i = 0; i < list.Count; i++)
            {
                bool isDir = (list[i][0] == '1');

                // Show/skip directories
                if (isDir && !doShowDirs)
                    continue;

                // Show/skip files
                if (!isDir && !doShowFiles)
                    continue;

                // Skip the delimiter
                if (list[i][0] == '2')
                    continue;

                string fileName = list[i].Substring(list[i].LastIndexOf("\\") + 1).ToLower();

                // Skip everything that does not match the search string
                if (!fileName.Contains(filterStr))
                    continue;

                selectedItems.Add(i);
            }

            // Reuse Count
            Count = 0;

            // Reserve known amount of rows
            var rows = new DataGridViewRow[selectedItems.Count];

            foreach (var n in selectedItems)
            {
                bool isDir = (list[n][0] == '1');

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

    // Add files/derectories to the DataGridView from the List
    public void Populate(int dirsCount, int filesCount, bool doShowDirs, bool doShowFiles, string filterStr = "")
    {
        _dataGrid.Rows.Clear();

        // The first time we populate our GridView, we create this template row that we'll use for cloning later
        if (_myTemplateRow == null)
        {
            _myTemplateRow = _dataGrid.Rows[_dataGrid.Rows.Add()];
        }

        if (filterStr.Length == 0)
        {
            Populate_Fast(_globalFileListRef, dirsCount, filesCount, doShowDirs, doShowFiles);
        }
        else
        {
            Populate_Slow(_globalFileListRef, dirsCount, filesCount, doShowDirs, doShowFiles, filterStr);
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Get a list of files that are currently checked in the GridView
    public void getSelectedFiles(System.Collections.Generic.List<string> list, bool doShowDirs, bool doShowFiles)
    {
        list.Clear();
/*
        list.Add($"-- Total number of items in List: {originalFilesList.Count}");
        list.Add($"-- Total number of items in Grid: {_dataGrid.Rows.Count}");
*/
        // Get all checked directories and files
        for (int i = 0; i < _dataGrid.Rows.Count; i++)
        {
            DataGridViewRow row = _dataGrid.Rows[i];
            string item = (string)(row.Cells[(int)Columns.colName].Value);
            bool isDir = (item[0] == '1');

            // Show/skip directories
            if (isDir && !doShowDirs)
                continue;

            // Show/skip files
            if (!isDir && !doShowFiles)
                continue;

            // Skip the delimiter
            if (item[0] == '2')
                continue;

            bool isChecked = (bool)(row.Cells[(int)Columns.colCheckBox].Value);

            // If the item is checked, we need to find it in the original list.
            // This is easy and fast, as we know its index in the original list,
            // and accessing item by index in the List is O(1)
            if (isChecked)
            {
                int num = (int)(row.Cells[(int)Columns.colNumber].Value);

                // Add unmodified file name
                // This way, list will contain references to original file names, and not the copies
                list.Add(_globalFileListRef[num]);
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Remove dark gray parts of the GridView in case the rows don't cover all the area of the widget
    // Simplified version, only fills gray parts with background color
    // https://social.msdn.microsoft.com/Forums/windows/en-US/d39e565e-cf33-45b9-993c-99d39813fd15/datagridview-filling-rest-of-rows-with-empty-lines?forum=winforms
    // Not used anymore. Instead, background color of the widget was set.
    public void OnPaint(object sender, System.Windows.Forms.PaintEventArgs e)
    {
        bool doFillWithEmptyRows = false;

        if (doFillWithEmptyRows)
        {
            int rowHeight = _dataGrid.RowTemplate.Height;

            if (_rowImg == null)
            {
                int rowWidth = _dataGrid.Width - 2;

                _rowImg = new Bitmap(rowWidth, rowHeight);
                Graphics g = Graphics.FromImage(_rowImg);
                g.FillRectangle(new SolidBrush(_dataGrid.DefaultCellStyle.BackColor), 0, 0, rowWidth, rowHeight);
                g.Dispose();
            }

            int h = _dataGrid.Rows.Count * rowHeight + 1;

            if (h < _dataGrid.Height)
            {
                int num = (_dataGrid.Height - h) / rowHeight;

                for (int i = 0; i < num; i++)
                    e.Graphics.DrawImage(_rowImg, 1, h + i * rowHeight);

                // Draw once more, at the very bottom
                e.Graphics.DrawImage(_rowImg, 1, _dataGrid.Height - rowHeight - 1); // Bug: Sometimes it covered the lower row
            }
            else
            {
                // Hide dark gray area that appears at the very bottom when scrolling all the way down
                int allowedVerticalScrollOffset = h - _dataGrid.Height;

                if (_dataGrid.VerticalScrollingOffset > allowedVerticalScrollOffset)
                {
                    int diff = _dataGrid.VerticalScrollingOffset - allowedVerticalScrollOffset;

                    e.Graphics.DrawImage(_rowImg, 1, _dataGrid.Height - diff, _dataGrid.Width - 2, diff);
                }
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Customize the look of each cell
    // https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.datagridview.cellpainting?view=netframework-4.8
    private void on_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);      // Paint everything
            //e.PaintBackground(e.CellBounds, true);                // Paint only background
            //e.PaintContent(e.CellBounds);                         // Paint only contents

            // Draw border around selected row
            if ((e.State & DataGridViewElementStates.Selected) != 0)
            {
                int yOffset = e.RowIndex == 0 ? 1 : 0;

                // Draw borders around each cell in a row
                //using (Pen p = new Pen(Color.OrangeRed, 1))
                using (Pen p = new Pen(Color.FloralWhite, 1))
                {
                    Rectangle rect = new Rectangle(e.CellBounds.X, e.CellBounds.Y + yOffset, e.CellBounds.Width - 1, e.CellBounds.Height - 2 - yOffset);
                    e.Graphics.DrawRectangle(p, rect);
                }

                // Remove verical lines, so only the row rectangle remains
                if (e.ColumnIndex > 0)
                {
                    using (Pen p2 = new Pen(e.CellStyle.SelectionBackColor, 2))
                    {
                        e.Graphics.DrawLine(p2, e.CellBounds.X, e.CellBounds.Y + 1 + yOffset, e.CellBounds.X, e.CellBounds.Y + e.CellBounds.Height - 2);
                    }
                }
            }

            e.Handled = true;

/*
            System.Drawing.Color penColor = e.CellStyle.BackColor;

            if ((e.State & DataGridViewElementStates.Selected) != 0)
                penColor = e.CellStyle.SelectionBackColor;

            using (Pen p = new Pen(penColor, 1))
            {
                // Erase cell borders
                Rectangle rect = new Rectangle(e.CellBounds.X - 1, e.CellBounds.Y, e.CellBounds.Width, e.CellBounds.Height - 2);
                e.Graphics.DrawRectangle(p, rect);
            }

            e.Handled = true;
*/
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
    {
        ;
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                myDataGrid_ContextMenu.showMenu(sender, e, _globalFileListRef, _doUseRecursion);
            }
        }
    }

    // --------------------------------------------------------------------------------------------------------

    // Key Down Event
    private void on_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.KeyData)
        {
            case Keys.Space: {

                    // Change checked state of each selected row
                    int selectedRowCount = _dataGrid.Rows.GetRowCount(DataGridViewElementStates.Selected);

                    for (int i = 0; i < selectedRowCount; i++)
                    {
                        var cb = _dataGrid.SelectedRows[i].Cells[(int)Columns.colCheckBox];
                        cb.Value = !(bool)(cb.Value);
                    }
                }
                break;
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Set double buffering to reduce flickering
    private void setDoubleBuffering()
    {
        // https://stackoverflow.com/questions/41893708/how-to-prevent-datagridview-from-flickering-when-scrolling-horizontally
        PropertyInfo pi = _dataGrid.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
        pi.SetValue(_dataGrid, true, null);
    }

    // --------------------------------------------------------------------------------------------------------

    public void setRecursiveMode(bool mode)
    {
        _doUseRecursion = mode;
    }

    // --------------------------------------------------------------------------------------------------------
};
