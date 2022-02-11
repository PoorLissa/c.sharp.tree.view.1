﻿using System.Windows.Forms;


/*
    myWrappingPanel class.

    Intended to reduce the original control's scroll bar thickness, as perceived by the user.

    Does so by moving the original control to underlying panel and changing the control's width/height in such a way that
    its scroll bars become partially invisible due to exceeding the bounds of the underlying panel.

    To be able to draw custom border, customPanel class is derived from Panel class, and its OnPaint method is overridden.

    https://www.codeproject.com/Articles/14801/How-to-skin-scrollbars-for-Panels-in-C
*/


namespace myControls
{
    public class myWrappingPanel
    {
        public class customPanel : Panel
        {
            public customPanel()
            {
                //SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                //e.Graphics.DrawRectangle(System.Drawing.Pens.Red, 0, 0, ClientSize.Width, ClientSize.Height);
            }
        };

        myWrappingPanel(Control ctrl, bool addBorder, bool adjustHorizontal)
        {
            int offset = 16;

            var panel = new customPanel();
            var parent = ctrl.Parent;

            panel.Left = ctrl.Left;
            panel.BringToFront();

            parent.Controls.Remove(ctrl);
            panel.Controls.Add(ctrl);
            parent.Controls.Add(panel);

            panel.Dock = ctrl.Dock;
            panel.Width = ctrl.Width;
            panel.BorderStyle = addBorder ? BorderStyle.FixedSingle : BorderStyle.None;

            ctrl.Dock = DockStyle.None;
            ctrl.Left = 0;
            ctrl.Top = 0;
            ctrl.Width += offset;
            ctrl.Height += adjustHorizontal ? offset + 3 : 0;
        }

        public static void Wrap(Control control, bool addBorder, bool adjustHorizontal)
        {
            new myWrappingPanel(control, addBorder, adjustHorizontal);
        }
    }
};