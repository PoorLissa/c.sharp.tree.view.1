using System;
using System.Windows.Forms;

/*
    Custom ComboBox wrapper around standard ComboBox widget
*/

namespace myControls
{
    public enum SortMode
    {
        None,
        Sorted,
        LastOnTop
    };

    public class myComboBox
    {
        ComboBox _cb = null;
        Label _placeholder = null;

        private bool     _isChanged = false;
        private SortMode _sort;

        // --------------------------------------------------------------------------------------------------------

        public myComboBox(ComboBox cb, SortMode sort, string placeholder = "...")
        {
            _cb = cb;
            _placeholder = new Label();
            _placeholder.Text = placeholder;
            _sort = sort;

            init();
        }

        // --------------------------------------------------------------------------------------------------------

        private void init()
        {
            if (_cb != null)
            {
                _cb.AutoCompleteMode   = AutoCompleteMode.Suggest;
                _cb.AutoCompleteSource = AutoCompleteSource.ListItems;

                _cb.DropDownWidth = 300;

                // Add and subscribe to events
                setUpEvents();

                createDrawingPrimitives();
            }

            return;
        }

        // --------------------------------------------------------------------------------------------------------

        private void setUpEvents()
        {
            _cb.MouseClick               += new MouseEventHandler (on_MouseClick);
            _cb.KeyDown                  += new KeyEventHandler   (on_KeyDown);
            _cb.TextChanged              += new EventHandler      (on_TextChanged);
            _cb.DropDown                 += new EventHandler      (on_DropDownOpened);
            _cb.DropDownClosed           += new EventHandler      (on_DropDownClosed);
            _cb.SelectionChangeCommitted += new EventHandler      (on_SelectionChangeCommitted);
        }

        // --------------------------------------------------------------------------------------------------------

        public ComboBox Obj()
        {
            return _cb;
        }

        // --------------------------------------------------------------------------------------------------------

        public bool isChanged()
        {
            return _isChanged;
        }

        // --------------------------------------------------------------------------------------------------------

        // Adds items to combobox from string
        public void setItems(string data, bool doSelectFirstItem = false)
        {
            if (data != null)
            {
                int pos1 = 0;
                int pos2 = 0;

                do
                {
                    pos2 = data.IndexOf('?', pos1);
                    _cb.Items.Add(data.Substring(pos1, pos2 - pos1));
                    pos1 = pos2 + 1;

                } while (pos2 != data.Length - 1);

                if(_sort == SortMode.Sorted)
                    _cb.Sorted = true;

                if (doSelectFirstItem && _cb.Items.Count > 0)
                {
                    _cb.SelectedIndex = 0;
                }
            }

            return;
        }

        // --------------------------------------------------------------------------------------------------------

        // Retrieve all combobox's items as a string
        public string getChanges()
        {
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < _cb.Items.Count; i++)
            {
                sb.Append(_cb.Items[i].ToString());
                sb.Append('?');
            }

            return sb.ToString();
        }

        // --------------------------------------------------------------------------------------------------------

        private void createDrawingPrimitives()
        {
            // Text placeholder
            _placeholder.Left = _cb.Left + 2;
            _placeholder.Top = _cb.Top + 4;

            _placeholder.Width = _cb.Width - 33;
            _placeholder.Height = _cb.Height - 5;
            _placeholder.BackColor = _cb.BackColor;
            _placeholder.ForeColor = System.Drawing.Color.LightSlateGray;

            _placeholder.MouseClick += new MouseEventHandler(on_Placeholder_MouseClick);

            var font = _cb.Font;
            _placeholder.Font = new System.Drawing.Font(font.Name, font.Size - 1, font.Style, font.Unit, font.GdiCharSet);

            _cb.Parent.Controls.Add(_placeholder);
            _placeholder.BringToFront();
            _placeholder.Visible = true;
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_TextChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)(sender);
            _placeholder.Visible = (cb.Text.Length == 0);
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_Placeholder_MouseClick(object sender, MouseEventArgs e)
        {
            _placeholder.Visible = false;
            _cb.Focus();
            _cb.DroppedDown = true;
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_MouseClick(object sender, MouseEventArgs e)
        {
            _cb.DroppedDown = true;
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_KeyDown(object sender, KeyEventArgs e)
        {
            ComboBox cb = (ComboBox)(sender);

            // With AutoCompleteMode, pressing Enter on AutoSuggested item does not result in selecting that item --
            // in case the dropdown also is opened at that time.
            // So we need to close the dropdown before we sellect an item in AutoSuggest.
            // But we don't want to close it until some actual value has been typed in.
            if (e.KeyCode != Keys.ShiftKey)
                _cb.DroppedDown = false;

            if (e.KeyCode == Keys.Enter)
            {
                for(int i = 0; i < cb.Items.Count; i++)
                {
                    var item = cb.Items[i];

                    if (item.ToString() == cb.Text)
                    {
                        if (_sort == SortMode.LastOnTop)
                        {
                            cb.Items.RemoveAt(i);
                            cb.Items.Insert(0, item);
                            cb.SelectedIndex = 0;
                            _isChanged = true;
                        }

                        return;
                    }
                }

                cb.Items.Insert(0, cb.Text);
                _isChanged = true;
            }
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_DropDownOpened(object sender, EventArgs e)
        {
            _placeholder.Visible = false;
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)(sender);
            _placeholder.Visible = (cb.Text.Length == 0);
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)(sender);

            // When selected by mouse, resort the items
            if (cb.DroppedDown && _sort == SortMode.LastOnTop)
            {
                var selectedIndex = cb.SelectedIndex;

                if (selectedIndex > 0)
                {
                    var item = cb.SelectedItem;

                    cb.Items.RemoveAt(selectedIndex);
                    cb.Items.Insert(0, item);
                    cb.SelectedIndex = 0;
                    _isChanged = true;
                }
            }

            return;
        }

        // --------------------------------------------------------------------------------------------------------
    };

};
