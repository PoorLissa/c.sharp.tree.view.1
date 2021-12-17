using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

/*
    Custom Panel Class
*/

namespace myControls
{
    public class myPanel : Panel
    {
        private bool _isHovered  = false;           // If the mouse pointer is hovering over the panel
        private bool _isSelected = false;           // If the main checkbox of the panel is checked or unchecked
        private int  _height     = -1;              // Stores original height of the panel to be able to wrap/unwrap it
        private int  _wrapHeight = -1;

        public bool UseCustomBorder { get; set; } = false;

        // Attribute needed, as without it VS added this property to Form1.Designer.cs file.
        // This way, this property was set up during initialization, which resulted in wrong _height value
        [DefaultValue(false)]
        public bool isSelected
        {
            get { return _isSelected; }

            set
            {
                _isSelected = value;

                _height = (_height < 0) ? Height : _height;
                Height = _isSelected ? _height : _wrapHeight;

                for (int i = 0; i < Controls.Count; i++)
                {
                    var subControl = Controls[i];

                    if (subControl.Name.StartsWith("checkBox_Option_"))
                    {
                        subControl.ForeColor = _isSelected ? Color.DarkRed : Color.Black;
                    }
                    else
                    {
                        subControl.Enabled = _isSelected;
                    }
                }

                Invalidate();
            }
        }

        public myPanel() : base()
        {
            _wrapHeight = this.DeviceDpi > 96 ? 63 : 43;

            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (this.GetChildAtPoint(this.PointToClient(MousePosition)) == null)
            {
                _isHovered = false;
                Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            // If the panel is wrapped up, check its main checkbox
            if (Height == _wrapHeight)
            {
                for (int i = 0; i < Controls.Count; i++)
                {
                    var subControl = Controls[i];

                    if (subControl.Name.StartsWith("checkBox_Option_"))
                    {
                        (subControl as CheckBox).Checked = true;
                        break;
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                if (_isHovered)
                {
                    brush.Color = Color.WhiteSmoke;
                    brush.Color = SystemColors.ControlLightLight;
                }

                e.Graphics.FillRectangle(brush, this.ClientRectangle);

                // Draw Shadow
                if (UseCustomBorder)
                {
                    if (_isHovered)
                    {
                        Pen pen2 = Pens.LightGray;
                        //Pen pen2 = Pens.YellowGreen;

                        // Horizontal
                        e.Graphics.DrawLine(pen2, 2, ClientSize.Height - 3, ClientSize.Width-2, ClientSize.Height - 3);
                        e.Graphics.DrawLine(pen2, 3, ClientSize.Height - 2, ClientSize.Width - 2, ClientSize.Height - 2);
                        //e.Graphics.DrawLine(Pens.LightGreen, 3, ClientSize.Height - 2, ClientSize.Width-2, ClientSize.Height - 2);

                        // Vertical
                        //e.Graphics.DrawLine(Pens.LightGreen, ClientSize.Width - 2, 2, ClientSize.Width - 2, ClientSize.Height - 3);
                        //e.Graphics.DrawLine(pen2, ClientSize.Width - 2, 2, ClientSize.Width - 2, ClientSize.Height - 3);
                        //e.Graphics.DrawLine(Pens.LightGray, ClientSize.Width - 1, 3, ClientSize.Width - 1, ClientSize.Height - 2);

                        e.Graphics.DrawLine(pen2, ClientSize.Width - 2, 2, ClientSize.Width - 2, ClientSize.Height - 3);
                        e.Graphics.DrawLine(pen2, ClientSize.Width - 1, 3, ClientSize.Width - 1, ClientSize.Height - 2);
                        //e.Graphics.DrawLine(Pens.LightGreen, ClientSize.Width - 1, 3, ClientSize.Width - 1, ClientSize.Height - 2);

                    }
                    else
                    {
                        e.Graphics.DrawLine(Pens.LightGray, 2, ClientSize.Height - 2, ClientSize.Width, ClientSize.Height - 2);
                    }
                }
            }

            // Draw Border
            if (UseCustomBorder)
            {
                if (_isHovered)
                {
                    e.Graphics.DrawRectangle(Pens.DarkSlateGray, 0, 0, ClientSize.Width - 3, ClientSize.Height - 4);
                    //e.Graphics.DrawRectangle(Pens.DarkGreen, 0, 0, ClientSize.Width - 3, ClientSize.Height - 4);
                    //e.Graphics.DrawRectangle(Pens.LightSlateGray, 0, 0, ClientSize.Width - 3, ClientSize.Height - 4);
                    //e.Graphics.DrawRectangle(Pens.LightSeaGreen, 1, 1, ClientSize.Width - 5, ClientSize.Height - 6);
                }
                else
                {
                    e.Graphics.DrawRectangle(Pens.LightSlateGray, 1, 1, ClientSize.Width - 2, ClientSize.Height - 4);
                }
            }
        }
    };
};
