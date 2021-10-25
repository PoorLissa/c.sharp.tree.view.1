using System;
using System.Windows.Forms;

/*
    Custom ComboBox wrapper around standard ComboBox widget
*/

public class myComboBox
{
    ComboBox _cb = null;
    Label    _placeholder = null;

    // --------------------------------------------------------------------------------------------------------

    public myComboBox(ComboBox cb, string placeholder = "...")
    {
        _cb = cb;
        _placeholder = new Label();
        _placeholder.Text = placeholder;

        init();
    }

    // --------------------------------------------------------------------------------------------------------

    private void init()
    {
        if (_cb != null)
        {
            int dpi = _cb.DeviceDpi;

            // Add and subscribe to events
            setUpEvents();

            createDrawingPrimitives(dpi);
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    private void setUpEvents()
    {
        _cb.MouseClick      += new MouseEventHandler    (on_MouseClick);
        _cb.KeyDown         += new KeyEventHandler      (on_KeyDown);
        _cb.TextChanged     += new EventHandler         (on_TextChanged);
        _cb.DropDown        += new EventHandler         (on_DropDownOpened);
        _cb.DropDownClosed  += new EventHandler         (on_DropDownClosed);
//      _cb.HandleDestroyed += new EventHandler         (on_HandleDestroyed);
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_TextChanged(object sender, EventArgs e)
    {
        ComboBox cb = (ComboBox)(sender);
        _placeholder.Visible = cb.Text.Length == 0;
    }

    // --------------------------------------------------------------------------------------------------------

    private void createDrawingPrimitives(int dpi)
    {
        // Text placeholder
        _placeholder.Left = _cb.Left + 2;
        _placeholder.Top = _cb.Top + 4;
        _placeholder.Visible = true;

        _placeholder.Width = _cb.Width - 33;
        _placeholder.Height = _cb.Height - 5;
        _placeholder.BackColor = _cb.BackColor;
        _placeholder.ForeColor = System.Drawing.Color.LightSlateGray;

        _placeholder.MouseClick += new MouseEventHandler(on_Placeholder_MouseClick);

        var font = _cb.Font;
        _placeholder.Font = new System.Drawing.Font(font.Name, font.Size - 1, font.Style, font.Unit, font.GdiCharSet);

        _cb.Parent.Controls.Add(_placeholder);
        _placeholder.BringToFront();
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_Placeholder_MouseClick(object sender, MouseEventArgs e)
    {
        _placeholder.Visible = false;
        _cb.DroppedDown = true;
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_MouseClick(object sender, MouseEventArgs e)
    {
        _cb.DroppedDown = true;
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_KeyDown(object sender, KeyEventArgs e)
    {
        ComboBox cb = (ComboBox)(sender);

        if (e.KeyCode == Keys.Enter)
        {
            cb.Items.Add(cb.Text);
        }
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_HandleDestroyed(object sender, EventArgs e)
    {
        string path = "_cBox.txt";

        if (!System.IO.File.Exists(path))
        {
            System.IO.File.CreateText(path);
        }

        string s = "";

        for (int i = 0; i < _cb.Items.Count; i++)
        {
            s += _cb.Items[i].ToString() + "|";
        }

        using (System.IO.StreamWriter sw = System.IO.File.AppendText(path))
        {
            sw.WriteLine(s);
        }
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_DropDownOpened(object sender, EventArgs e)
    {
        _placeholder.Visible = false;
    }

    // --------------------------------------------------------------------------------------------------------

    private void on_DropDownClosed(object sender, EventArgs e)
    {
        ComboBox cb = (ComboBox)(sender);
        _placeholder.Visible = cb.Text.Length == 0;
    }

    // --------------------------------------------------------------------------------------------------------

};