using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

// https://stackoverflow.com/questions/1597320/split-button-in-net-winforms

namespace myControls
{
    public class mySplitButton
    {
        private static Image _imgArrow = null;

        private struct Action
        {
            public EventHandler func;
            public string caption;
        };

        private Button _button = null;
        private List<Action> clickActions = null;
        private int _dividerPos = 31;

        public mySplitButton(Button b)
        {
            _button = b;

            init();
        }

        // --------------------------------------------------------------------------------------------------------

        private void init()
        {
            if (_button != null)
            {
                setUpEvents();
                createDrawingPrimitives();

                //_button.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
                //_button.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;

                _button.TextAlign = ContentAlignment.MiddleLeft;

                int leftPadding = _button.DeviceDpi > 96 ? 3 : 18;

                _button.Padding = new Padding(
                    _button.Padding.Left + _dividerPos - leftPadding,
                    _button.Padding.Top,
                    _button.Padding.Right,
                    _button.Padding.Bottom);
            }

            return;
        }

        // --------------------------------------------------------------------------------------------------------

        private void setUpEvents()
        {
            _button.KeyPress   += new KeyPressEventHandler  (on_KeyPress);
            _button.MouseClick += new MouseEventHandler     (on_MouseClick);
            _button.Paint      += new PaintEventHandler     (on_Paint);
        }

        // --------------------------------------------------------------------------------------------------------

        private void createDrawingPrimitives()
        {
            _imgArrow = Image.FromFile(myUtils.getFilePath("_icons", "icon-arrow-down-24.png"));
        }

        // --------------------------------------------------------------------------------------------------------

        public Button Obj()
        {
            return _button;
        }

        // --------------------------------------------------------------------------------------------------------

        public void addAction(EventHandler func, string caption)
        {
            if (clickActions == null)
            {
                clickActions = new List<Action>();
            }

            var item = new Action();
                item.func = func;
                item.caption = caption;

            clickActions.Add(item);

            if (clickActions.Count == 1)
            {
                _button.Text = caption;
            }
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Space))
            {
                if (clickActions != null && clickActions.Count > 0)
                {
                    clickActions[0].func(sender, e);
                }
            }

            return;
        }

        // --------------------------------------------------------------------------------------------------------

        private void on_MouseClick(object sender, MouseEventArgs e)
        {
            if (clickActions != null && clickActions.Count > 0)
            {
                if (e.X > _button.Width - _dividerPos)
                {
                    var menu = new ContextMenuStrip();

                    foreach (var item in clickActions)
                    {
                        ToolStripMenuItem i = new ToolStripMenuItem(item.caption, null, item.func);

                        i.Name = item.caption;
/*
                        i.Alignment = ToolStripItemAlignment.Left;
                        i.Padding = new Padding(1, 1, 1, 1);
*/
                        menu.Items.Add(i);
                    }

                    menu.Show(_button, new Point(0, _button.Height), ToolStripDropDownDirection.BelowRight);
                }
                else
                {
                    clickActions[0].func(sender, e);
                }
            }

            return;
        }

        // --------------------------------------------------------------------------------------------------------

        // Draw divider and an arrow
        private void on_Paint(object sender, PaintEventArgs e)
        {
            int x = _button.Width - _dividerPos + 4;
            int y = _button.Height / 2 - 24/2;

            e.Graphics.DrawImage(_imgArrow, x, y, 24, 24);
            e.Graphics.DrawLine(Pens.LightGray, _button.Width - _dividerPos + 0, 4, _button.Width - _dividerPos + 0, _button.Height - 4);
            e.Graphics.DrawLine(Pens.Gray,      _button.Width - _dividerPos + 1, 4, _button.Width - _dividerPos + 1, _button.Height - 4);
            e.Graphics.DrawLine(Pens.LightGray, _button.Width - _dividerPos + 2, 4, _button.Width - _dividerPos + 2, _button.Height - 4);
        }

        // --------------------------------------------------------------------------------------------------------
    };
};
