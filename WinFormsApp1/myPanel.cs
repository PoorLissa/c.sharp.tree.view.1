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
            _wrapHeight = this.DeviceDpi > 96 ? 63 : 43;        // Empirical values

            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        // Collects current settings for the panel's sub-controls and returns them as a string
        // Both 'getSettings' and 'useLatest_onClick' must implement functionality for the same control type
        public string getSettings()
        {
            string res = "";

            for (int i = 0; i < Controls.Count; i++)
            {
                Control ctrl = Controls[i];

                if (ctrl is CheckBox && !ctrl.Name.StartsWith("checkBox_Option_"))
                {
                    res += ctrl.Name;
                    res += ":";
                    res += (ctrl as CheckBox).Checked ? "+" : "-";
                    res += "?";
                    continue;
                }

                if (ctrl is ComboBox)
                {
                    res += ctrl.Name;
                    res += ":";
                    res += (ctrl as ComboBox).Text;
                    res += "?";
                    continue;
                }

                if (ctrl is NumericUpDown)
                {
                    res += ctrl.Name;
                    res += ":";
                    res += (ctrl as NumericUpDown).Value;
                    res += "?";
                    continue;
                }

                if (ctrl is RadioButton && (ctrl as RadioButton).Checked)
                {
                    res += ctrl.Name;
                    res += ":+?";
                    continue;
                }

                if (ctrl is TextBox)
                {
                    res += ctrl.Name;
                    res += ":";
                    res += ctrl.Text;
                    res += "?";
                    continue;
                }
            }

            return res;
        }

        // Set up "Use Latest" button for this panel
        public void UseLatest(ini_file_base ini)
        {
            // todo:
            // fix this:
            /*
panel_content_01 = radioButton21:+?comboBox2:?checkBox3:+?checkBox2:-?;
panel_content_05 = opt_005_predefined_templates:Numeric sequence?comboBox4:###?numericUpDown4:1?;
panel_content_13 = checkBox9:-?textBox11:?numericUpDown9:0?textBox5:?;
panel_content_08 = numericUpDown11:1?numericUpDown8:1?;
panel_content_04 = checkBox7:-?numericUpDown5:1?textBox7:?textBox6:?;
panel_content_02 = numericUpDown2:0?numericUpDown1:1?checkBox4:-?;
panel_content_09 = comboBox5:Start from the Left?textBox8:aaa?radioButton14:+?checkBox8:-?;
panel_content_12 = panel_content_10 = numericUpDown14:0?numericUpDown13:1?numericUpDown12:0?checkBox10:-?numericUpDown10:1?textBox9:?;  <----- ???
panel_content_07 = numericUpDown7:1?numericUpDown6:1?;
panel_content_06 = panel_content_03 = numericUpDown15:1?checkBox11:-?textBox4:?numericUpDown3:1?;   <----- ???
            */

            if (_isSelected)
            {
                string Params = ini[$"myPanelSettings.{Name}"];

                if (Params != null && Params.Length > 0)
                {
                    var b = new Button();
                    b.AccessibleName = Params;      // Just store a string in a parameter we don't otherwise use
                    b.Text = "Use Latest";
                    b.Width  = this.DeviceDpi > 96 ? 80 : 70;
                    b.Height = this.DeviceDpi > 96 ? 60 : 33;
                    b.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                    b.Font = new Font(b.Font.Name, 8.0f, b.Font.Unit);
                    b.Left = this.Width - b.Width - 10;
                    b.Top = 10;
                    b.Click += new EventHandler(useLatest_onClick);
                    Controls.Add(b);
                }

/*
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
*/
            }
            else
            {
                for (int i = 0; i < Controls.Count; i++)
                {
                    if (Controls[i] is Button && Controls[i].Text == "Use Latest")
                    {
                        Controls[i].Click -= new EventHandler(useLatest_onClick);
                        Controls.Remove(Controls[i]);
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

                    // opt_005_predefined_templates:Numeric sequence;comboBox4:;numericUpDown4:1;
                    foreach (var item in Params)
                    {
                        if (item == null || item.Length == 0)
                            continue;

                        int pos = item.IndexOf(':');

                        string name  = item.Substring(0, pos);
                        string value = item.Substring(pos + 1);

                        for (int j = 0; j < Controls.Count; j++)
                        {
                            Control ctrl = Controls[j];

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
