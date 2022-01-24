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
        // Return [true] if the character was processed by the control; otherwise, [false]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((int)keyData == (int)Keys.Control + (int)Keys.Back)
            {
                return false;
            }

            if ((int)keyData == (int)Keys.Tab)
            {
                return true;
            }

            bool res = base.ProcessCmdKey(ref msg, keyData);

            return res;
        }

        // Let the Editing Control handle the keys listed
        // When [true] is returned, the key will be sent to the Editing Control, not DataGrid
        public override bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey)
        {
            switch (key & Keys.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                case Keys.PageDown:
                case Keys.PageUp:
                    return true;

                default:
                    return !dataGridViewWantsInputKey;
            }
        }
    };
};
