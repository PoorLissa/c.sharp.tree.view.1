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

        public string PlaceholderText
        {
            get { return _tb.PlaceholderText;  }
            set { _tb.PlaceholderText = value; } 
        }

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

        public ref TextBox Obj()
        {
            return ref _tb;
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
            _lb.ImageIndex = 0;
            _lb.ForeColor = Color.DarkRed;
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_lbMouseLeave(object sender, EventArgs e)
        {
            _lb.ImageIndex = 3;
            _lb.ForeColor = Color.LightSlateGray;

            // Gradually reduce color
            new System.Threading.Tasks.Task(() =>
            {
                while(_lb.ImageIndex > 0)
                {
                    System.Threading.Tasks.Task.Delay(50).Wait();
                    _lb.Invoke(new MethodInvoker(delegate { _lb.ImageIndex--; }));
                }

            }).Start();
        }

        // --------------------------------------------------------------------------------------------------------

        private void createDrawingPrimitives(int dpi)
        {
            // 'x' button
            _lb.Visible = false;
            _lb.Width  = _tb.Height - 6;
            _lb.Height = _tb.Height - 2;
            _lb.Left   = _tb.Right - _lb.Width - 1;
            _lb.Top    = _tb.Top + 1;
            _lb.BackColor = _tb.BackColor;
            _lb.ForeColor = Color.LightSlateGray;
            _lb.Text = "";

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

        // Draw 'x' button with some effects
        private void on_lbPaint(object sender, PaintEventArgs e)
        {
            var bgrBrush = Brushes.White;
            bool isHovered = _lb.ForeColor == Color.DarkRed;

            int h = _lb.DeviceDpi > 96 ? _tb.Height / 2 : _tb.Height / 2 - 1;
            int w = _lb.DeviceDpi > 96 ? 5 : 4;
            int a = _lb.DeviceDpi > 96 ? 7 : 4;

            if (isHovered || _lb.ImageIndex != 0)
            {
                bgrBrush = Brushes.LightSteelBlue;

                e.Graphics.FillRectangle(bgrBrush, 0, 0, 99, 99);
                e.Graphics.DrawLine(Pens.Gray, 0, 0, 0, h+h);

                if (_lb.ImageIndex > 0)
                {
                    using (Brush b = new SolidBrush(Color.FromArgb(255 - 66 * _lb.ImageIndex, _tb.BackColor)))
                    {
                        e.Graphics.FillRectangle(b, 0, 0, 99, 99);
                    }
                }
            }

            using (Pen p = new Pen(_lb.ForeColor, 2))
            {
                e.Graphics.DrawLine(p, a, h-w, a+w+w, h+w);
                e.Graphics.DrawLine(p, a, h+w, a+w+w, h-w);
                e.Graphics.FillRectangle(bgrBrush, a, h+w, 1, 1);
                e.Graphics.FillRectangle(bgrBrush, a+w+w, h + w, 1, 1);
            }
        }
    };

};
