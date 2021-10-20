/*
  ===============================================================================
    Custom DataGridViewCheckBoxCell class.
    Allows the user to create custom checkbox column in DataGridView widget.
    Used as a template for DataGridViewCheckBoxColumn:

        var checkBoxColumn = new DataGridViewCheckBoxColumn();

        // Custom cell template
        var cellTemplate = new myDataGridViewCheckBoxCell(); <--- THIS CLASS
            cellTemplate.CustomSize = 20;
            cellTemplate.CustomDrawing = true;
            cellTemplate.CustomActiveAreaMargin = 5;
            cellTemplate.CustomImg_Checked  ("icon-checked.png");
            cellTemplate.CustomImg_Unchecked("icon-unchecked.png");
        checkBoxColumn.CellTemplate = cellTemplate;

        dataGrid.Columns.Add(checkBoxColumn);
  ===============================================================================
*/


using System.Drawing;
using System.Windows.Forms;


class myDataGridViewCheckBoxCell : DataGridViewCheckBoxCell
{

    // Size of the custom checkbox
    // Also sets the size of the image. If zero, the image will be drawn in its original size
    private static int _size = 10;

    // If [true], will draw custom checkbox
    private static DrawMode _drawMode = 0;

    // How the checkbox responds to mouse click:
    //  - if [ -1  ], only original checkbox area is active
    //  - if [  0  ], the whole cell containing the checkbox is active
    //  - if [ > 0 ], the active area is a [cell minus margin]
    private static int _cellMargin = -1;

    // Mouse is hovering over the checkbox
    private bool isHovered = false;

    private static Image _imgChecked   = null;
    private static Image _imgUnchecked = null;
    private static Image _imgTick      = null;

    private static Brush _hoveredCheckboxBrush = null;

    // Delegate to draw the checkbox from outside of the class
    public delegate void myDelegate(Graphics g, int x, int y, int size, bool Checked);
    private static myDelegate _drawFunc = null;

    // --------------------------------------------------------------------------------------------------------

    public enum DrawMode { Default, DefaultCustomSize, Custom1, Custom2, Custom3, Image };
    public enum cbMode   { Checked, Unchecked };

    // --------------------------------------------------------------------------------------------------------

    public int CustomSize
    {
        get {  return _size; }
        set { _size = value; }
    }

    public DrawMode CustomDrawing
    {
        get {  return _drawMode; }
        set { _drawMode = value; }
    }

    public int CustomActiveAreaMargin
    {
        get {  return _cellMargin; }
        set { _cellMargin = value; }
    }

    public void CustomImg(string file, cbMode mode)
    {
        // Allocate resource only if it will be actually used later
        if (_drawMode == DrawMode.Image)
        {
            switch (mode)
            {
                case cbMode.Checked:
                    _imgChecked = Image.FromFile(myUtils.getFilePath("_icons", file));
                    break;

                case cbMode.Unchecked:
                    _imgUnchecked = Image.FromFile(myUtils.getFilePath("_icons", file));
                    break;
            }
        }
    }

    public void CustomDrawFunc(myDelegate f)
    {
        _drawFunc = f;
    }

    // --------------------------------------------------------------------------------------------------------

    public myDataGridViewCheckBoxCell() : base()
    {
    }

    // --------------------------------------------------------------------------------------------------------

    // Check if the mouse cursor is hovering over the active area and make the cell visually respond to this
    protected override void OnMouseMove(DataGridViewCellMouseEventArgs e)
    {
        base.OnMouseMove(e);

        bool withinBounds = false;
        var cell = (DataGridViewCheckBoxCell)DataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];

        if (e.X >= _cellMargin && e.X <= cell.Size.Width - _cellMargin)
            if (e.Y >= _cellMargin && e.Y <= cell.Size.Height - _cellMargin)
                withinBounds = true;

        if (isHovered)
        {
            if (!withinBounds)
            {
                isHovered = false;
                DataGridView.InvalidateCell(cell);
            }
        }
        else
        {
            if (withinBounds)
            {
                isHovered = true;
                DataGridView.InvalidateCell(cell);
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    protected override void OnMouseLeave(int rowIndex)
    {
        if (isHovered)
        {
            isHovered = false;
            DataGridView.InvalidateRow(rowIndex);
        }
    }

    // --------------------------------------------------------------------------------------------------------

    protected override void OnMouseDown(System.Windows.Forms.DataGridViewCellMouseEventArgs e)
    {
        var cell = (DataGridViewCheckBoxCell)DataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
        Rectangle content = cell.ContentBounds;

        // Totally skip, if the mouse pointer is within the original checkbox limits.
        // OnContentClick event will take care of this situation
        if (e.X > content.X && e.X < (content.X + content.Width))
            if (e.Y > content.Y && e.Y < (content.Y + content.Height))
                return;

        if (_cellMargin == -1)
            return;

        // Proceed and make outside area of the checkbox active as well:
        base.OnMouseDown(e);

        if (e.X >= _cellMargin && e.X <= cell.Size.Width - _cellMargin)
        {
            if (e.Y >= _cellMargin && e.Y <= cell.Size.Height - _cellMargin)
            {
                this.DataGridView.EndEdit();
                cell.Value = !(bool)(cell.Value);
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    protected override void OnContentClick(System.Windows.Forms.DataGridViewCellEventArgs e)
    {
        base.OnContentClick(e);
    }

    // --------------------------------------------------------------------------------------------------------

    protected override void Paint(
        Graphics graph,
            Rectangle clipBounds,
                Rectangle cellBounds,
                    int rowIndex,
                        DataGridViewElementStates elementState,
                            object value,
                                object formattedValue,
                                    string errorText,
                                        DataGridViewCellStyle cellStyle,
                                            DataGridViewAdvancedBorderStyle advancedBorderStyle,
                                                DataGridViewPaintParts paintParts)
    {
        // Let the system draw original checkbox
        base.Paint(graph, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);



        // Now draw our custom checkbox over it
        int cellCenterX = cellBounds.X + cellBounds.Width  / 2;
        int cellCenterY = cellBounds.Y + cellBounds.Height / 2;

        switch (_drawMode)
        {
            case DrawMode.Default:
                // base.Paint(...) did that already
                break;

            case DrawMode.DefaultCustomSize:
                drawCheckBox_DefaultCustomSize(graph, cellCenterX, cellCenterY, (bool)formattedValue);
                break;

            case DrawMode.Custom1:
                drawCheckBox_Custom1(graph, cellCenterX, cellCenterY, (bool)formattedValue);
                break;

            case DrawMode.Custom2:
                drawCheckBox_Custom2(graph, cellCenterX, cellCenterY, (bool)formattedValue);
                break;

            case DrawMode.Custom3:
                drawCheckBox_Custom3(graph, cellCenterX, cellCenterY, (bool)formattedValue);
                break;

            case DrawMode.Image:
                drawCheckBox_Image(graph, cellCenterX, cellCenterY, (bool)formattedValue);
                break;
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Draw custom checkbox using standard Windows API
    private void drawCheckBox_DefaultCustomSize(Graphics g, int x, int y, bool Checked)
    {
        ControlPaint.DrawCheckBox(g, x - _size, y - _size, 2 * _size, 2 * _size,
            Checked
                ? ButtonState.Checked | ButtonState.Flat
                : ButtonState.Normal  | ButtonState.Flat);
        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Manually draw custom checkbox
    private void drawCheckBox_Custom1(Graphics g, int x, int y, bool Checked)
    {
        if (_drawFunc != null)
        {
            _drawFunc(g, x, y, _size, Checked);
        }
        else
        {
            // Make the size uneven
            if (_size % 2 == 0)
                _size++;

            g.FillRectangle(Brushes.White, x - _size, y - _size, 2 * _size, 2 * _size);

            Brush b = Checked ? Brushes.Green : Brushes.DarkRed;
            g.FillRectangle(b, x - _size + 3, y - _size + 3, 2 * _size - 5, 2 * _size - 5);
            g.DrawRectangle(Pens.DarkGray, x - _size, y - _size, 2 * _size, 2 * _size);
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    Bitmap b = null;

    // Manually draw custom checkbox
    // This one should look like Windows 10 Flat Checkbox
    // The size of this checkbox is fixed, but we'll adjust it for different dpi
    private void drawCheckBox_Custom2(Graphics g, int x, int y, bool Checked)
    {
        if (_drawFunc != null)
        {
            _drawFunc(g, x, y, _size, Checked);
        }
        else
        {
            if (b == null)
            {
                b = new Bitmap(18, 18);
                Graphics gr = Graphics.FromImage(b);

                int X = 10;
                int Y = 10;

                using (Pen p = new Pen(Color.Black, 2))
                {
                    p.Color = Color.Gray;
                    gr.DrawLine(p, X - 6 - 0, Y + 0 - 0, X - 2 - 0, Y + 4 - 0);
                    gr.DrawLine(p, X - 2 + 0, Y + 4 - 0, X + 7 + 0, Y - 5 - 0);

                    p.Color = Color.Black;
                    gr.DrawLine(p, X - 6 - 1, Y + 0 + 0, X - 2 - 0, Y + 4 + 1);
                    gr.DrawLine(p, X - 2 + 1, Y + 4 - 0, X + 7 + 1, Y - 5 - 0);

                    p.Color = Color.DarkGray;
                    p.Width = 1;
                    gr.DrawLine(p, X - 6 - 1, Y + 0 + 1, X - 2 - 1, Y + 4 + 1);
                    gr.DrawLine(p, X - 2 + 0, Y + 4 + 2, X + 7 + 1, Y - 5 + 1);

                    p.Color = Color.WhiteSmoke;
                    gr.DrawLine(p, X - 6 - 1, Y + 0 + 2, X - 2 - 1, Y + 4 + 2);
                    gr.DrawLine(p, X - 2 + 1, Y + 4 + 2, X + 7 + 1, Y - 5 + 2);

                    // Erase part of the bitmap, making this part transparent
                    gr.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    using (var br = new SolidBrush(Color.FromArgb(0, 0, 0, 0)))
                    {
                        gr.FillRectangle(br, X - 8, Y - 8, 11, 11);
                    }
                }

                gr.Dispose();
            }

            int size = g.DpiX > 96 ? 20 : 16;

            x--;
            y--;

            g.FillRectangle(Brushes.White, x - size/2, y - size/2, size, size);

            if (isHovered)
            {
                if (size == 20)
                {
                    //g.FillRectangle(Brushes.AliceBlue,      x - 11, y - 11, 22, 22);  // work
                    g.FillRectangle(Brushes.AliceBlue,      x - 11, y - 11, 23, 23);    // home
                    g.FillRectangle(Brushes.White,          x - 10, y - 10, 20, 20);
                    g.FillRectangle(Brushes.LightSteelBlue, x -  8, y -  8, 17, 17);
                }
                else
                {
                    g.FillRectangle(Brushes.AliceBlue,      x - 9, y - 9, 19, 19);
                    g.FillRectangle(Brushes.White,          x - 8, y - 8, 16, 16);
                    g.FillRectangle(Brushes.LightSteelBlue, x - 6, y - 6, 13, 13);
                }
            }

            g.DrawRectangle(Pens.Black, x - size/2, y - size/2, size, size);


            if (Checked)
            {
                g.DrawImage(b, x - size / 2, y - size / 2);
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Manually draw custom checkbox frame
    // Use the tick from image file
    // The size of this checkbox is fixed, but we'll adjust it for different dpi
    private void drawCheckBox_Custom3(Graphics g, int x, int y, bool Checked)
    {
        int size = g.DpiX > 96 ? 20 : 16;

        if (_imgTick == null)
        {
            if (size == 20)
                _imgTick = Image.FromFile(myUtils.getFilePath("_icons", "check-box-1-tick-24-large.png"));

            if (size == 16)
                _imgTick = Image.FromFile(myUtils.getFilePath("_icons", "check-box-1-tick-24-small.png"));

            _hoveredCheckboxBrush = new SolidBrush(Color.FromArgb(50, 50, 175, 255));
        }

        x--;
        y--;

        g.FillRectangle(Brushes.White, x - size/2, y - size/2, size, size);

        if (isHovered)
        {
            if (size == 16)
            {
                g.FillRectangle(Brushes.AliceBlue,      x - 9, y - 9, 19, 19);
                g.FillRectangle(Brushes.White,          x - 8, y - 8, 16, 16);
                g.FillRectangle(_hoveredCheckboxBrush,  x - 6, y - 6, 13, 13);
            }

            if (size == 20)
            {
                g.FillRectangle(Brushes.AliceBlue,      x - 11, y - 11, 23, 23);
                g.FillRectangle(Brushes.White,          x - 10, y - 10, 20, 20);
                g.FillRectangle(_hoveredCheckboxBrush,  x -  8, y -  8, 17, 17);
            }

            g.DrawRectangle(Pens.Black, x - size/2 - 1, y - size/2 - 1, size + 2, size + 2);
        }
        else
        {
            g.DrawRectangle(Pens.Black, x - size/2, y - size/2, size, size);
        }

        if (Checked)
        {
            if (size == 16)
                g.DrawImage(_imgTick, x - size/2 - 3, y - size/2 - 4);

            if (size == 20)
                g.DrawImage(_imgTick, x - size/2 - 1, y - size/2 - 1);
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Draw custom checkbox using loaded image for that
    private void drawCheckBox_Image(Graphics g, int x, int y, bool Checked)
    {
        if (_imgUnchecked != null && _imgChecked != null)
        {
            Image img = Checked ? _imgChecked : _imgUnchecked;

            int imgWidth  = (_size == 0) ? img.Width  : _size;
            int imgHeight = (_size == 0) ? img.Height : _size;

            g.DrawImage(img, x - imgWidth / 2, y - imgHeight / 2, imgWidth, imgHeight);
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------
}
