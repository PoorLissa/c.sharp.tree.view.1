using System.Drawing;
using System.Windows.Forms;

/*
    Custom DataGridViewCheckBoxCell class.
    Used as a template for DataGridViewCheckBoxColumn:

        var checkBoxColumn = new DataGridViewCheckBoxColumn();

        var cellTemplate = new myDataGridViewCheckBoxCell();
            cellTemplate.Size = 13;
            cellTemplate.UseCustomDrawing = true;

        checkBoxColumn.CellTemplate = cellTemplate;

    Allows the user to create custom checkbox column in DataGridView
*/

class myDataGridViewCheckBoxCell : DataGridViewCheckBoxCell
{
    // Size of the custom checkbox
    private static int _size = 10;

    // If [true], will draw custom checkbox
    private static bool _useCustomDrawing = false;

    // How the checkbox responds to mouse click:
    //  - if [ -1  ], only original checkbox area is active
    //  - if [  0  ], the whole cell containing the checkbox is active
    //  - if [ > 0 ], the active area is a [cell minus margin]
    private static int _cellMargin = -1;

    private static Image _imgChecked = null;
    private static Image _imgUnchecked = null;

    // --------------------------------------------------------------------------------------------------------

    public int CustomSize
    {
        get {  return _size; }
        set { _size = value; }
    }

    public bool CustomDrawing
    {
        get {  return _useCustomDrawing; }
        set { _useCustomDrawing = value; }
    }

    public int CustomActiveAreaMargin
    {
        get {  return _cellMargin; }
        set { _cellMargin = value; }
    }

    public void CustomImg_Checked(string file)
    {
        _imgChecked = Image.FromFile(myUtils.getFilePath("_icons", file));
    }

    public void CustomImg_Unchecked(string file)
    {
        _imgUnchecked = Image.FromFile(myUtils.getFilePath("_icons", file));
    }

    // --------------------------------------------------------------------------------------------------------

    public myDataGridViewCheckBoxCell() : base()
    {
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
        // Draw the original checkbox using system style
        base.Paint(graph, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

        // Now draw the custom checkbox over it
        if (_useCustomDrawing)
        {
            int cellCenterX = cellBounds.X + cellBounds.Width  / 2;
            int cellCenterY = cellBounds.Y + cellBounds.Height / 2;

            if (_imgUnchecked != null && _imgChecked != null)
            {
                Image img = (bool)formattedValue ? _imgChecked : _imgUnchecked;

                int imgWidth  = img.Width;
                int imgHeight = img.Height;

                if (_size != 0)
                {
                    imgWidth  = _size;
                    imgHeight = _size;
                }

                graph.DrawImage(img, cellCenterX - imgWidth/2, cellCenterY - imgHeight/2, imgWidth, imgHeight);
            }
            else
            {
//              graphics.FillRectangle(System.Drawing.Brushes.Red, x - size, y - size, 2*size, 2*size);

                ControlPaint.DrawCheckBox(graph, cellCenterX - _size, cellCenterY - _size, 2 * _size, 2 * _size,
                                (bool)formattedValue
                                        ? ButtonState.Checked | ButtonState.Flat
                                        : ButtonState.Normal  | ButtonState.Flat);
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------
}
