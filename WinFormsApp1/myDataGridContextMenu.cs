using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


public class myDataGrid_ContextMenu
{
    private static DataGridView _dataGrid = null;

    // Going to hold a reference to the global list of files
    private static System.Collections.Generic.List<string> _globalFileListRef = null;


    // Must be the same as the enum in myDataGrid class
    private enum Columns
    {
        colCheckBox = 0,
        colImage,
        colName,
        colNumber
    };

    // --------------------------------------------------------------------------------------------------------

    // Display context menu of the row
    public static void showMenu(object sender, DataGridViewCellMouseEventArgs e, List<string> list, bool recursion)
    {
        var dataGrid = (DataGridView)(sender);

        if (_dataGrid == null)
        {
            _dataGrid = dataGrid;
            _globalFileListRef = list;
        }

        var cell = _dataGrid[e.ColumnIndex, e.RowIndex];

        if (!cell.Selected)
        {
            cell.DataGridView.ClearSelection();
            cell.DataGridView.CurrentCell = cell;
            cell.Selected = true;
        }

        var menu    = new System.Windows.Forms.ContextMenuStrip();
        var pt      = new Point(0, 0);
        var handler = new EventHandler(contextMenuHandler);

        menu.Items.Add("Copy Name",         null, handler);
        menu.Items.Add("Copy Full Path",    null, handler);
        menu.Items.Add("Select children",   null, handler);
        menu.Items.Add("Deselect children", null, handler);

        pt.X = e.X;
        pt.Y = e.Y + ((e.RowIndex - _dataGrid.FirstDisplayedScrollingRowIndex) * _dataGrid.RowTemplate.Height);

        for (int i = 0; i < e.ColumnIndex; i++)
            pt.X += _dataGrid.Columns[i].Width;

        // Select children is available only in the recursive mode
        menu.Items[2].Enabled = recursion;
        menu.Items[3].Enabled = recursion;

        menu.Show(_dataGrid, pt);

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    private static void contextMenuHandler(object sender, EventArgs e)
    {
        ToolStripItem menuItem = (ToolStripItem)sender;

        do
        {
            // Copy selected items names to clipboard
            if (menuItem.Text == "Copy Name")
            {
                copyName();
                break;
            }

            // Copy selected items full path to clipboard
            if (menuItem.Text == "Copy Full Path")
            {
                copyFullPath();
                break;
            }

            // Select all the children of the selected item
            if (menuItem.Text == "Select children")
            {
                selectChildren(true);
                break;
            }

            // Deselect all the children of the selected item
            if (menuItem.Text == "Deselect children")
            {
                selectChildren(false);
                break;
            }

        } while (false);

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    private static void copyName()
    {
        string str = "";

        for (int i = 0; i < _dataGrid.RowCount; i++)
        {
            var row = _dataGrid.Rows[i];

            if (row.Selected)
                str += row.Cells[(int)Columns.colName].Value.ToString() + "\n";
        }

        if (str.Length > 0)
            Clipboard.SetText(str);
        else
            Clipboard.Clear();

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    private static void copyFullPath()
    {
        string str = "";

        var list = new List<int>();

        // Find indexes of selected rows
        for (int i = 0; i < _dataGrid.RowCount; i++)
        {
            var row = _dataGrid.Rows[i];

            if (row.Selected)
            {
                int n = (int)(row.Cells[(int)Columns.colNumber].Value);
                list.Add(n);
            }
        }

        for (int i = 0; i < list.Count; i++)
        {
            int n = list[i];
            str += _globalFileListRef[n][2..] + '\n';
        }

        if (str.Length > 0)
            Clipboard.SetText(str);
        else
            Clipboard.Clear();

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Find any children of selected directory and check/uncheck their checkboxes
    private static void selectChildren(bool mode)
    {
        var list = new System.Collections.Generic.List<int>();
        var row = _dataGrid.SelectedRows[0];
        int num = (int)(row.Cells[(int)Columns.colNumber].Value);

        string path = _globalFileListRef[num][2..];

        // Find indexes of item's children
        for (int i = 0; i < _globalFileListRef.Count; i++)
        {
            if (_globalFileListRef[i].Contains(path))
                list.Add(i);
        }

        for (int i = 0; i < list.Count; i++)
        {
            int n = list[i];
            var cb = _dataGrid.Rows[n].Cells[(int)Columns.colCheckBox];
            cb.Value = mode;
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------
};
