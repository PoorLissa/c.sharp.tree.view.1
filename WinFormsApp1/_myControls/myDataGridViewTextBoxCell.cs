using System.Drawing;
using System.Windows.Forms;


/*
    Custom class for DataGridViewTextBoxCell.
    Used as a template for a DataGridViewTextBoxColumn.
*/

class myDataGridViewTextBoxCell : DataGridViewTextBoxCell
{
    // Return the type of the editing control this Cell uses
    public override System.Type EditType
    {
        get
        {
            return typeof(myControls.myDataGridViewTextBoxEditingControl);
        }
    }

    // Adjust the editing panel, so that custom painting isn't drawn over when cells go into edit mode
    public override Rectangle PositionEditingPanel(Rectangle cellBounds, Rectangle cellClip, DataGridViewCellStyle cellStyle, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow)
    {
        Rectangle controlBounds = cellBounds;

        controlBounds.Y += 11;
        controlBounds.Height -= 22;
        controlBounds.X += 11;
        controlBounds.Width -= 22;

        cellClip.Width = cellBounds.Width;
        cellClip.Y += 10;

        // Let the base class do its adjustments
        return base.PositionEditingPanel(controlBounds, cellClip, cellStyle, singleVerticalBorderAdded, singleHorizontalBorderAdded, isFirstDisplayedColumn, isFirstDisplayedRow);
    }

    public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
    {
        dataGridViewCellStyle.BackColor = Color.White;

        base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

//      var tb = DataGridView.EditingControl as TextBox;
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
