using System;
using System.Windows.Forms;

/*
    Custom DataGridView class
    
    It is available in Toolbox window and can be placed on a Form.
    It can be used as a standard DataGridView widget, but we'll be able to use its internals (protected) as well

*/

namespace myControls
{

    public class myDataGridView : DataGridView
    {
        public myDataGridView() : base()
        {
        }

        // Changes the selection state of the row with the specified index.
        // The selection state changes without regard to the current SelectionMode property value, and without changing the CurrentCell property value.
        public void my_SetSelectedRowCore(int rowIndex, bool selected)
        {
            this.SetSelectedRowCore(rowIndex, selected);
        }

        // Changes the selection state of the cell with the specified row and column indexes.
        // The selection state changes without regard to the current SelectionMode property value, and without changing the CurrentCell property value.
        public void my_SetSelectedCellCore(int columnIndex, int rowIndex, bool selected)
        {
            this.SetSelectedCellCore(columnIndex, rowIndex, selected);
        }

        // Sets the currently active cell.
        // Changes the current cell without changing the selection and optionally without validating the previous cell or changing the selection anchor cell.
        // The anchor cell is the first cell of a block of multiple cells that the user can select by holding down the SHIFT key and clicking the last cell of the block.
        // https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.datagridview.setcurrentcelladdresscore?view=windowsdesktop-6.0
        public bool my_SetCurrentCellAddressCore(int columnIndex, int rowIndex, bool setAnchorCellAddress = false, bool validateCurrentCell = false, bool throughMouseClick = false)
        {
            return this.SetCurrentCellAddressCore(columnIndex, rowIndex, setAnchorCellAddress, validateCurrentCell, throughMouseClick);
        }

        protected override bool ProcessDataGridViewKey(KeyEventArgs e)
        {
            return base.ProcessDataGridViewKey(e);
        }
    };

};
