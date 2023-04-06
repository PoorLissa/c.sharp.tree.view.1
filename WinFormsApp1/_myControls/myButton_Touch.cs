using System.Windows.Forms;

/*
    Button class that is able to react to Press and Release events when using a touchscreen.
    Basic button raises both MouseDown and MouseUp events when the user releases the button on the screen.
    This one is able to react properly
*/

namespace myControls
{
    public class myButton : Button
    {
        public myButton() : base()
        {
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_POINTERDOWN = 0x246;
            const int WM_POINTERUP = 0x247;
            const int WM_MOUSEHOVER = 0x02A1;
            const int WM_LBUTTONDOWN = 0x0201;

            switch (m.Msg)
            {
                case WM_LBUTTONDOWN: {
                    }
                    break;

                case WM_MOUSEHOVER: {
                    }
                    break;

                case WM_POINTERDOWN: {

                        MouseEventArgs args = new MouseEventArgs(MouseButtons.Left, 1, 11, 11, 0);
                        this.OnMouseDown(args);
						return;
                    }

                case WM_POINTERUP: {

                        MouseEventArgs args = new MouseEventArgs(MouseButtons.Left, 1, 11, 11, 0);
                        this.OnMouseUp(args);
						return;
                    }
            }

            base.WndProc(ref m);
        }
    };
};
