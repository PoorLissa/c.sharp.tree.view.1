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

    private static Image _imgChecked = null;
    private static Image _imgUnchecked = null;

    // Delegate to draw the checkbox from outside of the class
    public delegate void myDelegate(Graphics g, int x, int y, int size, bool Checked);
    private static myDelegate _drawFunc = null;

    // --------------------------------------------------------------------------------------------------------

    public enum DrawMode { Default, DefaultCustomSize, Custom1, Custom2, Image };

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

    public void CustomImg_Checked(string file)
    {
        // Allocate resource only if it will be used later
        if (_drawMode == DrawMode.Image)
            _imgChecked = Image.FromFile(myUtils.getFilePath("_icons", file));
    }

    public void CustomImg_Unchecked(string file)
    {
        // Allocate resource only if it will be used later
        if (_drawMode == DrawMode.Image)
            _imgUnchecked = Image.FromFile(myUtils.getFilePath("_icons", file));
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

    protected override void OnMouseMove(DataGridViewCellMouseEventArgs e)
    {
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
        if (e.X >= content.X && e.X <= (content.X + content.Width))
            if (e.Y >= content.Y && e.Y <= (content.Y + content.Height))
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

    // Manually draw custom checkbox
    // This one should look like Windows 10 Flat Checkbox
    private void drawCheckBox_Custom2(Graphics g, int x, int y, bool Checked)
    {
        if (_drawFunc != null)
        {
            _drawFunc(g, x, y, _size, Checked);
        }
        else
        {
            x--;
            y--;

            g.FillRectangle(Brushes.White, x - 10, y - 10, 20, 20);

            if (isHovered)
            {
                g.FillRectangle(Brushes.AliceBlue,      x - 11, y - 11, 22, 22);
                g.FillRectangle(Brushes.White,          x - 10, y - 10, 20, 20);
                g.FillRectangle(Brushes.LightSteelBlue, x -  8, y -  8, 17, 17);
            }

            g.DrawRectangle(Pens.Black, x - 10, y - 10, 20, 20);

            if (Checked)
            {
                Color[] cl = {
                    Color.Gray,
                    Color.Black,
                    Color.DarkGray,
                    Color.WhiteSmoke
                };

                using (Pen p = new Pen(Color.Black, 2))
                {
                    p.Color = cl[0];
                    g.DrawLine(p, x - 6 - 0, y + 0 - 0, x - 2 - 0, y + 4 - 0);
                    g.DrawLine(p, x - 2 + 0, y + 4 - 0, x + 7 + 0, y - 5 - 0);

                    p.Color = cl[1];
                    g.DrawLine(p, x - 6 - 1, y + 0 + 0, x - 2 - 0, y + 4 + 1);
                    g.DrawLine(p, x - 2 + 1, y + 4 - 0, x + 7 + 1, y - 5 - 0);

                    p.Color = cl[2];
                    p.Width = 1;
                    g.DrawLine(p, x - 6 - 1, y + 0 + 1, x - 2 - 1, y + 4 + 1);
                    g.DrawLine(p, x - 2 + 0, y + 4 + 2, x + 7 + 1, y - 5 + 1);

                    p.Color = cl[3];
                    g.DrawLine(p, x - 6 - 1, y + 0 + 2, x - 2 - 1, y + 4 + 2);
                    g.DrawLine(p, x - 2 + 1, y + 4 + 2, x + 7 + 1, y - 5 + 2);
                }
            }

            return;
#if false
            using (Pen p = new Pen(Color.Black, 1))
            {
                Color[] cl = {
                    Color.Gray,
                    Color.Black,
                    Color.DarkGray,
                    Color.LightGray
                };

                p.Color = cl[0];
                g.DrawLine(p, x - 6 - 0, y + 0 + 0, x - 2 - 0, y + 4 + 0);
                g.DrawLine(p, x - 2 + 0, y + 4 - 0, x + 7 + 0, y - 5 - 0);

                p.Color = cl[1];
                //p.Width = 2;
                g.DrawLine(p, x - 6 - 1, y + 0 + 0, x - 2 - 0, y + 4 + 1);
                g.DrawLine(p, x - 2 + 1, y + 4 - 0, x + 7 + 1, y - 5 - 0);

                p.Color = cl[2];
                //p.Width = 1;
                g.DrawLine(p, x - 6 - 1, y + 0 + 1, x - 2 - 1, y + 4 + 1);
                g.DrawLine(p, x - 2 + 1, y + 4 + 1, x + 7 + 1, y - 5 + 1);

                //p.Color = cl[3];
                //g.DrawLine(p, x - 6 - 1, y + 0 + 2, x - 2 - 1, y + 4 + 2);
                //g.DrawLine(p, x - 2 + 1, y + 4 + 1, x + 7 + 1, y - 5 + 1);
            }
#endif
        }
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
