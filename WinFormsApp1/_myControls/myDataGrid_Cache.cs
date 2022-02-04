using System.Drawing;
using System.Windows.Forms;

/*
    Contains cached persistent primitives used in myDataGrid class
*/

class myDataGrid_Cache
{
    public myDataGrid_Cache()
    {
        _rect_01 = new Rectangle(0, 0, 0, 0);
    }

    // -----------------------------------------------------------------------

    private Rectangle _rect_01;
    public ref Rectangle getRect(int x = 0, int y = 0, int w = 0, int h = 0)
    {
        _rect_01.X      = x;
        _rect_01.Y      = y;
        _rect_01.Width  = w;
        _rect_01.Height = h;

        return ref _rect_01;
    }

    // -----------------------------------------------------------------------

    private Font _cellTooltipFont = null;
    public ref Font getCellTooltipFont(Font refFont)
    {
        if (_cellTooltipFont == null)
        {
            if (myRenamerApp.appDpi > 96)
            {
                _cellTooltipFont = new Font("Helvetica Condensed", refFont.Size - 3, FontStyle.Regular, refFont.Unit, refFont.GdiCharSet);
            }
            else
            {
                _cellTooltipFont = new Font("Calibri", refFont.Size - 1, FontStyle.Regular, refFont.Unit, refFont.GdiCharSet);
            }
        }

        return ref _cellTooltipFont;
    }

    // -----------------------------------------------------------------------

    private Font _customContentFont = null;
    public ref Font getCustomContentFont(Font refFont)
    {
        if (_customContentFont == null)
        {
            _customContentFont = new Font(refFont.Name, refFont.Size + 1, FontStyle.Bold, refFont.Unit, refFont.GdiCharSet);
        }

        return ref _customContentFont;
    }

    // -----------------------------------------------------------------------
};
