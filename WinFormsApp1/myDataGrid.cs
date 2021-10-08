using System.DirectoryServices.ActiveDirectory;
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

    private bool doAlternateColors   = false;
    private bool doUseIcons          = true;
    private bool doFillWithEmptyRows = true;

    private Image  _imgDir  = null;
    private Image  _imgFile = null;
    private Bitmap _rowImg  = null;

    // --------------------------------------------------------------------------------------------------------

    public myDataGrid(DataGridView dgv)
    {
        _dataGrid = dgv;

        init();
    }

    // --------------------------------------------------------------------------------------------------------

    public void init()
    {
        // https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-work-with-image-columns-in-the-windows-forms-datagridview-control?view=netframeworkdesktop-4.8
        if (_dataGrid != null)
        {
            // Set double buffering to reduce flickering:
            // https://stackoverflow.com/questions/41893708/how-to-prevent-datagridview-from-flickering-when-scrolling-horizontally
            PropertyInfo pi = _dataGrid.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(_dataGrid, true, null);

            _imgDir  = Image.FromFile(myUtils.getFilePath("_icons", "icons8-opened-folder-2-16.png"));
            _imgFile = Image.FromFile(myUtils.getFilePath("_icons", "icons8-file-16.png"));

            //_dataGrid.Paint += new PaintEventHandler(OnPaint);                            // Customize whole widget appearance
            //_dataGrid.CellPainting +=
            //  new DataGridViewCellPaintingEventHandler(dataGridView_CellPainting);        // Customize individual cell appearance

            _dataGrid.RowTemplate.Height = 50;                                              // Row height
            _dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;              // Row select mode
            _dataGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;       // Cell borders
            _dataGrid.EditMode = DataGridViewEditMode.EditOnF2;                             // How to edit cell (press F2)
            _dataGrid.AllowUserToAddRows = false;                                           // User can't add new rows
            _dataGrid.AllowUserToResizeRows = false;                                        // User can't resize rows
            _dataGrid.ColumnHeadersVisible = false;                                         // No column headers
            _dataGrid.RowHeadersVisible = false;                                            // No row headers

            // Grid Colors
            _dataGrid.DefaultCellStyle.SelectionBackColor = Color.DarkOrange;               // Row selection color
            _dataGrid.GridColor = Color.LightGray;
            _dataGrid.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Control;
            _dataGrid.BackgroundColor = _dataGrid.DefaultCellStyle.BackColor;               // Instead of OnPaint event

            // --- Add Columns ---

            // Checkbox column
            var checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.Width = 50;
            checkBoxColumn.Resizable = DataGridViewTriState.False;
            _dataGrid.Columns.Add(checkBoxColumn);

            // Icon column
            var imageColumn = new DataGridViewImageColumn();
            imageColumn.Width = 30;
            imageColumn.Resizable = DataGridViewTriState.False;
            _dataGrid.Columns.Add(imageColumn);

            // Text column (auto adjusted to fill all the available width)
            var textColumn = new DataGridViewTextBoxColumn();
            textColumn.Name = "Name";
            textColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dataGrid.Columns.Add(textColumn);
        }
    }

    // --------------------------------------------------------------------------------------------------------

    public void Clear()
    {
        _dataGrid.Rows.Clear();
    }

    // --------------------------------------------------------------------------------------------------------

    // Modify a copy of a template row and give it real values
    public void buildRow(ref DataGridViewRow row, string item, bool isDir)
    {
        int pos = item.LastIndexOf('\\') + 1;

        if (item.Length > 0)
        {
            if (doUseIcons)
            {
                row.Cells[1].Value = isDir ? _imgDir : _imgFile;
            }

            row.Cells[0].Value = false;
            row.Cells[2].Value = item[pos..];

            row.DefaultCellStyle.ForeColor = isDir ? System.Drawing.Color.Black : System.Drawing.Color.Brown;
        }
    }

    // --------------------------------------------------------------------------------------------------------

    // Add a single file/derectory to the DataGridView
    public void addRow(string item, bool isDir)
    {
        int pos = item.LastIndexOf('\\') + 1;
        int i = _dataGrid.Rows.Add();

        // Alternate row background color (Zebra style)
        if (doAlternateColors)
        {
            System.Drawing.Color backColor = (i % 2 == 0) ? System.Drawing.Color.LightGray : System.Drawing.Color.Wheat;
            _dataGrid.Rows[i].DefaultCellStyle.BackColor = backColor;
        }

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

            if (doUseIcons)
            {
                _dataGrid.Rows[i].Cells[1].Value = isDir ? _imgDir : _imgFile;
            }

            _dataGrid.Rows[i].Cells[0].Value = false;
            _dataGrid.Rows[i].Cells[2].Value = item[pos..];
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Add files/derectories to the DataGridView from the List
    public void Populate(System.Collections.Generic.List<string> list, int dirsCount, int filesCount, bool doShowDirs, bool doShowFiles, string searchStr = "")
    {
        _dataGrid.Rows.Clear();

        // The first time we populate our GridView, we create this template row that we'll use for cloning later
        if (_myTemplateRow == null)
        {
            _myTemplateRow = _dataGrid.Rows[_dataGrid.Rows.Add()];
        }

        int Count = 0;
        Count += doShowDirs  ?  dirsCount : 0;
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

                // Skip everything that does not match the search string
                if (searchStr.Length > 0)
                {
                    if (!item.Contains(searchStr))
                        continue;

                    todo:
                    // make 2 different functions: one with search string param, and other without it
                    // the first one is able to reserve known amount of rows
                    // the other one will need to calculate this amount

                }

                // Wasn't able to create new rows any other way
                // So had to stick to the cloning

                // Anyway, create new row and populate it with the data:
                var newRow = (DataGridViewRow)_myTemplateRow.Clone();
                buildRow(ref newRow, item, isDir);
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

    public void getSelectedFiles(System.Collections.Generic.List<string> originalFilesList, System.Collections.Generic.List<string> list, bool doShowDirs, bool doShowFiles)
    {
        list.Clear();

        int i = 0;

        // Get all directories and files
        foreach (var item in originalFilesList)
        {
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

            bool isChecked = (bool)_dataGrid.Rows[i++].Cells[0].Value;

            if (isChecked)
            {
                list.Add(item[2..]);
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
    private void dataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);      // Paint everything
            //e.PaintBackground(e.CellBounds, true);                // Paint only background
            //e.PaintContent(e.CellBounds);                         // Paint only contents

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
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Key Down Event
    public void OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.KeyData)
        {
            // Change checked state of each selected row
            case Keys.Space: {

                    bool isChecked = false;
                    int selectedRowCount = _dataGrid.Rows.GetRowCount(DataGridViewElementStates.Selected);

                    if (selectedRowCount > 0)
                    {
                        for (int i = 0; i < selectedRowCount; i++)
                        {
                            isChecked = (bool)_dataGrid.SelectedRows[i].Cells[0].Value;
                            _dataGrid.SelectedRows[i].Cells[0].Value = (isChecked == true) ? false : true;
                        }
                    }
                }
                break;
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------
};
