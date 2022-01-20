using System.Drawing;
using System.Windows.Forms;


/*
    Custom class for DataGridViewTextBoxCell.
    Used as a template for a DataGridViewTextBoxColumn.
*/

class myDataGridViewTextBoxCell : DataGridViewTextBoxCell
{
#if false

    public override void PositionEditingControl(bool setLocation, bool setSize, Rectangle cellBounds, Rectangle cellClip, DataGridViewCellStyle cellStyle, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow)
    {
        Rectangle bounds = cellBounds;

        bounds.Y += 11;
        bounds.Height -= 22;
        bounds.X += 11;
        bounds.Width -= 22;

        Rectangle clip = cellClip;

        clip.Y += 11;
        clip.Height -= 22;
        clip.X += 11;
        clip.Width -= 22;

        //base.PositionEditingControl(setLocation, setSize, cellBounds, cellClip, cellStyle, singleVerticalBorderAdded, singleHorizontalBorderAdded, isFirstDisplayedColumn, isFirstDisplayedRow);

        //base.PositionEditingControl(true, false, cellBounds, cellClip, cellStyle, singleVerticalBorderAdded, singleHorizontalBorderAdded, isFirstDisplayedColumn, isFirstDisplayedRow);
    }
#endif

#if false
    public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
    {
        var style = new DataGridViewCellStyle();

        style.SelectionBackColor = Color.Green;

        //dataGridViewCellStyle.BackColor = Color.Yellow;
        //dataGridViewCellStyle.Padding = new Padding(0, 20, 0, 0);

        //base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

        base.InitializeEditingControl(rowIndex, initialFormattedValue, style);

        //dataGridViewCellStyle.Padding = new Padding(111, 222, 333, 444);

        dataGridViewCellStyle.BackColor = Color.Yellow;
        dataGridViewCellStyle.SelectionBackColor = Color.Red;
    }
#endif

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

        // First, let base class do its adjustments
        return base.PositionEditingPanel(controlBounds, cellClip, cellStyle, singleVerticalBorderAdded, singleHorizontalBorderAdded, isFirstDisplayedColumn, isFirstDisplayedRow);
    }
};
