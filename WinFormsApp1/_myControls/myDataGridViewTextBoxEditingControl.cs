using System.Windows.Forms;


/*
    Replacement class for a DataGridViewTextBoxEditingControl.
    Overrides default ProcessCmdKey method in order to enable 'Ctrl+Backspace' trapping in DataGrid's EditingControl.
    
    This class is accessed/used in
        a) myDataGridViewTextBoxCell : DataGridViewTextBoxCell
        b) myDataGrid.on_CellControlShowing():

            if (e.KeyCode == Keys.Back && e.Modifiers == Keys.Control)
            {
                ...
            }

    Original DataGridViewTextBoxEditingControl is based on a TextBox class, which in its ProcessCmdKey method
    disables some shortcuts, including [(int)Keys.Control + (int)Keys.Back]
*/


namespace myControls
{
    class myDataGridViewTextBoxEditingControl : DataGridViewTextBoxEditingControl
    {
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((int)keyData == (int)Keys.Control + (int)Keys.Back)
            {
                return false;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    };
};
