using System.Windows.Forms;


/*
    myWrappingPanel class.

    Intended to reduce the original control's scroll bar thickness, as perceived by the user.

    Does so by moving the original control to underlying panel and changing the control's width/height in such a way that
    its scroll bars become partially invisible due to exceeding the bounds of the underlying panel.

    To be able to draw custom border, customPanel class is derived from Panel class, and its OnPaint method is overridden.
*/


namespace myControls
{
    public class myWrappingPanel
    {
        private class customPanel : Panel
        {
            public customPanel()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                //e.Graphics.DrawRectangle(System.Drawing.Pens.Red, 0, 0, ClientSize.Width, ClientSize.Height);
            }
        };

        myWrappingPanel(Control ctrl, bool increaseWdth)
        {
            int offset = 20;

            var _panel = new customPanel();
            var parent = ctrl.Parent;

            parent.Controls.Remove(ctrl);
            _panel.Controls.Add(ctrl);

            parent.Controls.Add(_panel);
            _panel.BringToFront();

            _panel.Dock = ctrl.Dock;
            _panel.Width = ctrl.Width;

            ctrl.Dock = DockStyle.None;
            ctrl.Left = 0;
            ctrl.Top = 0;
            ctrl.Width += offset;
            ctrl.Height += offset;
        }

        public static void Wrap(Control control, bool increaseWdth = true)
        {
            new myWrappingPanel(control, increaseWdth);
        }
    }
};
