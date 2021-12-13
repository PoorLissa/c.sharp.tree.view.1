using System;
using System.Drawing;
using System.Windows.Forms;

/*
    Custom Panel Class
*/

namespace myControls
{
    public class myPanel : Panel
    {
        public bool UseCustomBorder { get; set; } = false;

        public myPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }

            if (UseCustomBorder)
            {
#if false
                e.Graphics.DrawRectangle(Pens.LightSlateGray, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
#else
                e.Graphics.DrawRectangle(Pens.LightSlateGray, 1, 1, ClientSize.Width - 4, ClientSize.Height - 2);

                e.Graphics.DrawLine(Pens.White, 0, 1, 0, ClientSize.Height);

                e.Graphics.DrawLine(Pens.LightGray, ClientSize.Width - 1, 2, ClientSize.Width - 1, ClientSize.Height - 1);
                e.Graphics.DrawLine(Pens.LightGray, ClientSize.Width - 2, 2, ClientSize.Width - 2, ClientSize.Height - 2);

#endif
            }
        }

    };

};
