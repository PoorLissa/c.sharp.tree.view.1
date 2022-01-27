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
            // todo:
            // can we use this with shift+up/down to change our selection without delelecting and restoring the selection?
            // protected bool ProcessUpKey(Keys keyData) in DataGridViewMethods.cs
            this.SetSelectedRowCore(rowIndex, selected);
        }

        protected override bool ProcessDataGridViewKey(KeyEventArgs e)
        {
            return base.ProcessDataGridViewKey(e);
        }
    };

};
