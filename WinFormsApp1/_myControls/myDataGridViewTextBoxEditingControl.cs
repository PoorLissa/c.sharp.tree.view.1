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

    Also, some methods of [IDataGridViewEditingControl] are overriden here
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
            }

            return !dataGridViewWantsInputKey;
        }

#if false
        public override void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
/*
            this.Font = dataGridViewCellStyle.Font;
            this.BackColor = System.Drawing.Color.Green;
            this.ForeColor = System.Drawing.Color.Red;
*/
        }
#endif
    };

    // -------------------------------------------------------------------------------------------------------------

    public class myDataGridViewRichTextBoxEditingControl : RichTextBox, IDataGridViewEditingControl
    {
        DataGridView dataGridView;
        bool valueChanged;
        int rowIndex;

        public myDataGridViewRichTextBoxEditingControl() : base()
        {
            ;
        }

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

        public object EditingControlFormattedValue
        {
            get
            {
                return this.Text;
            }

            set
            {
                this.Text = (string)value;
            }
        }

        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {
            return EditingControlFormattedValue;
        }

        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            this.Font = dataGridViewCellStyle.Font;
        }

        public int EditingControlRowIndex
        {
            get { return rowIndex; }
            set { rowIndex = value; }
        }

        // Let the Editing Control handle the keys listed
        // When [true] is returned, the key will be sent to the Editing Control, not DataGrid
        public bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey)
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
            }

            return !dataGridViewWantsInputKey;
        }

        public void PrepareEditingControlForEdit(bool selectAll)
        {
        }

        public bool RepositionEditingControlOnValueChange
        {
            get { return false; }
        }

        public DataGridView EditingControlDataGridView
        {
            get
            {
                return dataGridView;
            }
            set
            {
                dataGridView = value as DataGridView;
            }
        }

        public bool EditingControlValueChanged
        {
            get
            {
                return valueChanged;
            }
            set
            {
                valueChanged = value;
            }
        }

        public Cursor EditingPanelCursor
        {
            get
            {
                return base.Cursor;
            }
        }

        protected override void OnTextChanged(System.EventArgs e)
        {
            valueChanged = true;
            this.EditingControlDataGridView.NotifyCurrentCellDirty(true);
            base.OnTextChanged(e);
        }
    };
};
