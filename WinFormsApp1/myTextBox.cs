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

            _lb.MouseClick  += new MouseEventHandler(on_lbMouseClick);
            _lb.MouseEnter  += new EventHandler     (on_lbMouseEnter);
            _lb.MouseLeave  += new EventHandler     (on_lbMouseLeave);
            _lb.Paint       += new PaintEventHandler(on_lbPaint);
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)(sender);
            _lb.Visible = tb.Text.Length != 0;
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_lbMouseEnter(object sender, EventArgs e)
        {
            _lb.ForeColor = Color.Red;
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_lbMouseLeave(object sender, EventArgs e)
        {
            _lb.ForeColor = System.Drawing.Color.LightSlateGray;
        }

        // --------------------------------------------------------------------------------------------------------

        private void createDrawingPrimitives(int dpi)
        {
            // 'x' button
            _lb.Visible = false;
            _lb.Width  = _tb.Height - 10;
            _lb.Height = _tb.Height - 10;
            _lb.Left   = _tb.Right - _lb.Width - 2;
            _lb.Top    = _tb.Top + 1;
            _lb.BackColor = _tb.BackColor;
            _lb.ForeColor = System.Drawing.Color.LightSlateGray;
            _lb.Text = "x";

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

        private void on_lbPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.White, e.ClipRectangle);
            e.Graphics.DrawRectangle(Pens.DarkRed, e.ClipRectangle.X + 1, e.ClipRectangle.Y + 1, e.ClipRectangle.Width-2, e.ClipRectangle.Height-2);

            StringFormat format = new StringFormat(StringFormatFlags.NoWrap);
                format.LineAlignment = StringAlignment.Center;
                format.Alignment     = StringAlignment.Center;

            e.Graphics.DrawString("x", _lb.Font, Brushes.Red, e.ClipRectangle, format);
        }
    };
};
