using System.Drawing;
using System.Windows.Forms;


/*
    Custom class for DataGridViewTextBoxCell.
    Used as a template for a DataGridViewTextBoxColumn.
*/

class myDataGridViewTextBoxCell : DataGridViewTextBoxCell
{
    private static int _dpi = -1;

    public myDataGridViewTextBoxCell() : base()
    {
    }

    public myDataGridViewTextBoxCell(int dpi) : base()
    {
        _dpi = _dpi < 0 ? dpi : _dpi;
    }

    // Return the type of the editing control this Cell uses
    public override System.Type EditType
    {
        get
        {
            //return typeof(myControls.myDataGridViewRichTextBoxEditingControl);
            return typeof(myControls.myDataGridViewTextBoxEditingControl);
        }
    }

    // Adjust the editing panel, so that custom painting isn't drawn over when cells go into edit mode
    public override Rectangle PositionEditingPanel(Rectangle cellBounds, Rectangle cellClip, DataGridViewCellStyle cellStyle, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow)
    {
        Rectangle controlBounds = cellBounds;

        int offset = _dpi > 96 ? 11 : 9;

        controlBounds.Y += offset;
        controlBounds.Height -= 2*offset;
        controlBounds.X += offset;
        controlBounds.Width -= 2*offset;

        cellClip.Width = cellBounds.Width;
        cellClip.Y += 10;

        // Let the base class do its adjustments
        return base.PositionEditingPanel(controlBounds, cellClip, cellStyle, singleVerticalBorderAdded, singleHorizontalBorderAdded, isFirstDisplayedColumn, isFirstDisplayedRow);
    }

    public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
    {
        dataGridViewCellStyle.BackColor = Color.White;

        base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

        if (this.EditType == typeof(myControls.myDataGridViewRichTextBoxEditingControl))
        {
            var rtb = DataGridView.EditingControl as RichTextBox;

            if (rtb != null)
            {
                rtb.BorderStyle = BorderStyle.None;
                //rtb.AcceptsReturn = rtb.Multiline = dataGridViewCellStyle.WrapMode == DataGridViewTriState.True;
                rtb.MaxLength = this.MaxInputLength;
                string initialFormattedValueStr = initialFormattedValue as string;

                if (initialFormattedValueStr == null)
                {
                    rtb.Text = string.Empty;
                }
                else
                {
                    rtb.Text = initialFormattedValueStr;
                }
            }
        }
    }

    protected override void Paint(Graphics graphics,
                                    Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
                                    DataGridViewElementStates cellState, object value, object formattedValue,
                                    string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
                                    DataGridViewPaintParts paintParts)
    {
        base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
    }
};
