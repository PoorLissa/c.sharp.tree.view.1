using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;



public class myDataGrid_ContextMenu
{
    private static DataGridView _dataGrid = null;
    private static System.Collections.Generic.List<myTreeListDataItem> _globalFileListRef = null;

    // --------------------------------------------------------------------------------------------------------

    // Display context menu for the row
    public static void showMenu(object sender, DataGridViewCellMouseEventArgs e, List<myTreeListDataItem> list, bool recursion)
    {
        var dataGrid = (DataGridView)(sender);

        if (_dataGrid == null)
            _dataGrid = dataGrid;

        if (_globalFileListRef == null)
            _globalFileListRef = list;

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

        menu.Items.Add("Copy Name",         null, handler).Name = "1";
        menu.Items.Add("Copy Full Path",    null, handler).Name = "2";
        menu.Items.Add("Select children",   null, handler).Name = "3";
        menu.Items.Add("Deselect children", null, handler).Name = "4";

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
            if (menuItem.Name == "1")
            {
                copyName();
                break;
            }

            // Copy selected items full path to clipboard
            if (menuItem.Name == "2")
            {
                copyFullPath();
                break;
            }

            // Select all the children of the selected item
            if (menuItem.Name == "3")
            {
                selectChildren(true);
                break;
            }

            // Deselect all the children of the selected item
            if (menuItem.Name == "4")
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
                str += row.Cells[(int)myDataGrid.Columns.colName].Value.ToString() + "\n";
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
                int n = (int)(row.Cells[(int)myDataGrid.Columns.colId].Value);
                list.Add(n);
            }
        }

        for (int i = 0; i < list.Count; i++)
        {
            int n = list[i];
            str += _globalFileListRef[n].Name + '\n';
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
        var set = new System.Collections.Generic.HashSet<int>();

        // For every selected row:
        for (int i = 0; i < _dataGrid.SelectedRows.Count; i++)
        {
            var row = _dataGrid.SelectedRows[i];

            int id = (int)(row.Cells[(int)myDataGrid.Columns.colId].Value);
            string path = _globalFileListRef[id].Name[2..];

            set.Add(id);

            // Find indexes of item's children
            for (int j = id + 1; j < _globalFileListRef.Count; j++)
                if (_globalFileListRef[j].Name.Contains(path))
                    set.Add(j);
        }

        foreach (var id in set)
        {
            var cb = _dataGrid.Rows[id].Cells[(int)myDataGrid.Columns.colChBox];
            cb.Value = mode;

            // Also, set selection for the row
            _dataGrid.Rows[id].Selected = mode;
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------
};
