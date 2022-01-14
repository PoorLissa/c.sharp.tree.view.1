using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
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

        // Attribute needed, as without it VS adds this property to Form1.Designer.cs file.
        // This way, this property is set up during initialization, which results in wrong _height value
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
            _wrapHeight = this.DeviceDpi > 96 ? 63 : 43;        // Empirical values

            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        // Collects current settings for the panel's sub-controls and returns them as a string
        // Both 'getSettings' and 'useLatest_onClick' must implement functionality for the same control types
        public string getSettings()
        {
            List<Control> list = new List<Control>();
            getSubControls(this, ref list);

            var sb = new StringBuilder();

            for (int i = 0; i < list.Count; i++)
            {
                Control ctrl = list[i];

                if (ctrl is CheckBox && !ctrl.Name.StartsWith("checkBox_Option_"))
                {
                    sb.Append(ctrl.Name);
                    sb.Append(":");
                    sb.Append((ctrl as CheckBox).Checked ? "+" : "-");
                    sb.Append("?");
                    continue;
                }

                if (ctrl is ComboBox)
                {
                    sb.Append(ctrl.Name);
                    sb.Append(":");
                    sb.Append((ctrl as ComboBox).Text);
                    sb.Append("?");
                    continue;
                }

                if (ctrl is NumericUpDown)
                {
                    sb.Append(ctrl.Name);
                    sb.Append(":");
                    sb.Append((ctrl as NumericUpDown).Value);
                    sb.Append("?");
                    continue;
                }

                if (ctrl is RadioButton && (ctrl as RadioButton).Checked)
                {
                    sb.Append(ctrl.Name);
                    sb.Append(":+?");
                    continue;
                }

                if (ctrl is TextBox)
                {
                    sb.Append(ctrl.Name);
                    sb.Append(":");
                    sb.Append(ctrl.Text);
                    sb.Append("?");
                    continue;
                }
            }

            return sb.ToString();
        }

        // Set up "Use Latest" button for this panel
        public void setUpLatest(ini_file_base ini, bool isSelected)
        {
            if (isSelected)
            {
                int offsetY = DeviceDpi > 96 ? 35 : 25;

                // Check if the button has been added already:
                bool btnExists = false;

                for (int i = 0; i < Controls.Count; i++)
                {
                    if (Controls[i] is Button && Controls[i].Text == "Use Latest")
                    {
                        Controls[i].Visible = true;
                        btnExists = true;
                        break;
                    }
                }

                if (!btnExists)
                {
                    string Params = ini[$"myPanelSettings.{Name}"];

                    if (Params != null && Params.Length > 0)
                    {
                        _height += offsetY;

                        for (int i = 0; i < Controls.Count; i++)
                            if (!Controls[i].Name.StartsWith("checkBox_Option_"))
                                Controls[i].Top += offsetY;

                        var b = new Button();
                        b.AccessibleName = Params;      // Just store a string in a parameter we don't otherwise use
                        b.Text = "Use Latest";
                        b.Height = this.DeviceDpi > 96 ? 50 : 33;
                        b.Font = new Font(b.Font.Name, 8.0f, b.Font.Unit);

                        b.Top = DeviceDpi > 96 ? 58 : 35;
                        b.Left = 100;
                        b.Width = this.Width - b.Left - 100;

                        b.Click += new EventHandler(useLatest_onClick);
                        Controls.Add(b);
                    }
                }

#if false
                if (false)
                {
                    var p = new Panel();
                    p.Width  = 33;
                    p.Height = 33;
                    p.Left   = 10;
                    p.Top    = 50;
                    p.BorderStyle = BorderStyle.FixedSingle;
                    p.BringToFront();
                    Controls.Add(p);
                }
#endif
            }
            else
            {
                for (int i = 0; i < Controls.Count; i++)
                {
                    if (Controls[i] is Button && Controls[i].Text == "Use Latest")
                    {
                        Controls[i].Visible = false;
                        break;
                    }
                }
            }

            return;
        }

        // OnClick event for the "Use Latest" button
        // Both 'getSettings' and 'useLatest_onClick' must implement functionality for the same control type
        private void useLatest_onClick(object sender, EventArgs e)
        {
            string param = (sender as Button).AccessibleName;

            if (param != null && param.Length > 0)
            {
                try
                {
                    var Params = param.Split('?');

                    List<Control> list = new List<Control>();
                    getSubControls(this, ref list);

                    // opt_005_predefined_templates:Numeric sequence;comboBox4:;numericUpDown4:1;
                    foreach (var item in Params)
                    {
                        if (item == null || item.Length == 0)
                            continue;

                        int pos = item.IndexOf(':');

                        string name  = item.Substring(0, pos);
                        string value = item.Substring(pos + 1);

                        for (int j = 0; j < list.Count; j++)
                        {
                            Control ctrl = list[j];

                            if (ctrl.Name == name)
                            {
                                if (ctrl is NumericUpDown)
                                {
                                    (ctrl as NumericUpDown).Value = int.Parse(value);
                                    break;
                                }

                                if (ctrl is ComboBox)
                                {
                                    (ctrl as ComboBox).Text = value;
                                    break;
                                }

                                if (ctrl is CheckBox)
                                {
                                    (ctrl as CheckBox).Checked = (value == "+");
                                    break;
                                }

                                if (ctrl is RadioButton && value == "+")
                                {
                                    (ctrl as RadioButton).Checked = true;
                                    break;
                                }

                                if (ctrl is TextBox)
                                {
                                    (ctrl as TextBox).Text = value;
                                    break;
                                }

                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + ",\n Where Param = " + param, "Parameter restore error", MessageBoxButtons.OK);
                }
            }

            return;
        }

        // Recursive: Collect all subcontrols of a Control
        private void getSubControls(Control Ctrl, ref List<Control> list)
        {
            for (int i = 0; i < Ctrl.Controls.Count; i++)
            {
                Control ctrl = Ctrl.Controls[i];

                if (ctrl is GroupBox || ctrl is Panel)
                {
                    getSubControls(ctrl, ref list);
                }
                else
                {
                    list.Add(ctrl);
                }
            }

            return;
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
                    if (Controls[i].Name.StartsWith("checkBox_Option_"))
                    {
                        (Controls[i] as CheckBox).Checked = true;
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
