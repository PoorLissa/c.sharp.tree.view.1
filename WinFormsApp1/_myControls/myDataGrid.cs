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
    }

};
