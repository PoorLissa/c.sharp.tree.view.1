using System;
using System.Drawing;
using System.Windows.Forms;

/*
    Custom ComboBox wrapper around standard ComboBox widget
*/

namespace myControls
{
    public class myTextBox
    {
        private TextBox _tb = null;
        private Label _lb = null;

        // --------------------------------------------------------------------------------------------------------

        public myTextBox(TextBox tb, string placeholder = "...")
        {
            _tb = tb;
            _tb.PlaceholderText = placeholder;
            _lb = new Label();

            init();
        }

        // --------------------------------------------------------------------------------------------------------

        private void init()
        {
            if (_tb != null)
            {
                int dpi = _tb.DeviceDpi;

                // Add and subscribe to events
                setUpEvents();

                createDrawingPrimitives(dpi);
            }

            return;
        }

        // --------------------------------------------------------------------------------------------------------

        private void setUpEvents()
        {
            _tb.TextChanged += new EventHandler     (on_TextChanged);
            _tb.MouseHover  += new EventHandler     (on_MouseHover);
            _lb.MouseClick  += new MouseEventHandler(on_lbMouseClick);
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)(sender);
            _lb.Visible = tb.Text.Length != 0;
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_MouseHover(object sender, EventArgs e)
        {
            _lb.BackColor = Color.LightGray;
            _lb.Visible = true;
        }

        // --------------------------------------------------------------------------------------------------------

        private void createDrawingPrimitives(int dpi)
        {
            // 'x' button
            _lb.Visible = false;

            _lb.Width  = _tb.Height - 15;
            _lb.Height = _tb.Height - 15;
            _lb.Left   = _tb.Right - _lb.Width - 2;
            _lb.Top    = _tb.Top + 1;
            _lb.BackColor = _tb.BackColor;
            //_lb.BackColor = Color.Red;
            _lb.ForeColor = System.Drawing.Color.LightSlateGray;
            _lb.Text = " ";

            _lb.TextAlign = ContentAlignment.TopCenter;


            _tb.Parent.Controls.Add(_lb);
            _lb.BringToFront();
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_lbMouseClick(object sender, MouseEventArgs e)
        {
            _tb.Clear();
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_KeyDown(object sender, KeyEventArgs e)
        {
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_HandleDestroyed(object sender, EventArgs e)
        {
        }

        // --------------------------------------------------------------------------------------------------------
    };
};
