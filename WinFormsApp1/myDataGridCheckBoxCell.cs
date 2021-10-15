using System.Drawing;
using System.Windows.Forms;


class myDataGridViewCheckBoxCell : DataGridViewCheckBoxCell
{
    public myDataGridViewCheckBoxCell()
    {
    }

    protected override void OnMouseDown(System.Windows.Forms.DataGridViewCellMouseEventArgs e)
    {
//        base.OnMouseDown(e);
//        return;

        DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)this.DataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];

        
        if (cell.Value == cell.FalseValue || cell.Value == null|| (bool)cell.Value == false)
        {
            cell.Value = cell.TrueValue;
        }
        else
        {
            cell.Value = cell.FalseValue;
        }

        //cell.Value = !(bool)(cell.Value);

        //this.DataGridView.InvalidateCell(cell);

        //this.DataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);

        if (this.DataGridView.IsCurrentCellDirty)
        {
            this.DataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }


        return;


        DataGridView.InvalidateCell(cell);
    }
    /*
        protected override void OnClick(System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            System.Windows.Forms.DataGridViewCheckBoxCell cell = (System.Windows.Forms.DataGridViewCheckBoxCell)this.DataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];

            var cb = cell;
            cb.Value = !(bool)(cb.Value);

            if (this.DataGridView.IsCurrentCellDirty) 
            {
                this.DataGridView.CommitEdit(System.Windows.Forms.DataGridViewDataErrorContexts.Commit);
            }

            DataGridView.InvalidateCell(cell);

            base.OnClick(e);
        }
    */

    /*
        protected override void OnContentClick(System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            //base.OnContentClick(e);
        }
    */

    protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState,
        object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
        DataGridViewPaintParts paintParts)
    {
        base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

        int size = 11;

        int x = cellBounds.X + cellBounds.Width / 2;
        int y = cellBounds.Y + cellBounds.Height / 2;

//        graphics.FillRectangle(System.Drawing.Brushes.Red, x - size, y - size, 2*size, 2*size);

        ControlPaint.DrawCheckBox(graphics, x - size, y - size, 2 * size, 2 * size,
                (bool)formattedValue
                        ? ButtonState.Checked | ButtonState.Flat
                        : ButtonState.Normal  | ButtonState.Flat);
    }
}
