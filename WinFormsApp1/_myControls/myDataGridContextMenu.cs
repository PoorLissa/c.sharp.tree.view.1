﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;



public class myDataGrid_ContextMenu
{
    private static DataGridView _dataGrid = null;
    private static List<myTreeListDataItem> _globalFileListRef = null;

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
        menu.Items.Add("Edit (F2)",         null, handler).Name = "3";
        menu.Items.Add("Select children",   null, handler).Name = "4";
        menu.Items.Add("Deselect children", null, handler).Name = "5";

        pt.X = e.X;
        pt.Y = e.Y + ((e.RowIndex - _dataGrid.FirstDisplayedScrollingRowIndex) * _dataGrid.RowTemplate.Height);

        for (int i = 0; i < e.ColumnIndex; i++)
            pt.X += _dataGrid.Columns[i].Width;

        // Select children is available only in the recursive mode
        menu.Items[3].Enabled = recursion;
        menu.Items[4].Enabled = recursion;

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

            // Edit cell text manually
            if (menuItem.Name == "3")
            {
                edit();
                break;
            }

            // Select all the children of the selected item
            if (menuItem.Name == "4")
            {
                selectChildren(true);
                break;
            }

            // Deselect all the children of the selected item
            if (menuItem.Name == "5")
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
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < _dataGrid.RowCount; i++)
        {
            var row = _dataGrid.Rows[i];

            if (row.Selected)
            {
                sb.Append(row.Cells[(int)myDataGrid_Wrapper.Columns.colName].Value.ToString());
                sb.Append('\n');
            }
        }

        if (sb.Length > 0)
            Clipboard.SetText(sb.ToString());
        else
            Clipboard.Clear();

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    private static void copyFullPath()
    {
        StringBuilder sb = new StringBuilder();

        var list = new List<int>();

        // Find indexes of selected rows
        for (int i = 0; i < _dataGrid.RowCount; i++)
        {
            var row = _dataGrid.Rows[i];

            if (row.Selected)
            {
                int n = (int)(row.Cells[(int)myDataGrid_Wrapper.Columns.colId].Value);
                list.Add(n);
            }
        }

        for (int i = 0; i < list.Count; i++)
        {
            int n = list[i];
            sb.Append(_globalFileListRef[n].Name);
            sb.Append('\n');
        }

        if (sb.Length > 0)
            Clipboard.SetText(sb.ToString());
        else
            Clipboard.Clear();

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Edit cell text manually
    private static void edit()
    {
        var cell = _dataGrid.CurrentCell;

        cell.ReadOnly = false;
        _dataGrid.BeginEdit(false);

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Find any children of selected directory and check/uncheck their checkboxes
    private static void selectChildren(bool mode)
    {
        var set = new HashSet<int>();

        // For every row:
        for (int i = 0; i < _dataGrid.Rows.Count; i++)
        {
            var row = _dataGrid.Rows[i];

            if (row.Selected)
            {
                int id = (int)(row.Cells[(int)myDataGrid_Wrapper.Columns.colId].Value);
                string path = _globalFileListRef[id].Name;

                set.Add(id);

                // Find indexes of item's children
                for (int j = id + 1; j < _globalFileListRef.Count; j++)
                    if (_globalFileListRef[j].Name.Contains(path))
                        set.Add(j);
            }
        }

        // For every row:
        for (int i = 0; i < _dataGrid.Rows.Count; i++)
        {
            var row = _dataGrid.Rows[i];

            int id = (int)(row.Cells[(int)myDataGrid_Wrapper.Columns.colId].Value);

            if (set.Contains(id))
            {
                var cb = row.Cells[(int)myDataGrid_Wrapper.Columns.colChBox];
                cb.Value = mode;

                // Also, set selection for the row
                row.Selected = mode;
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------
};
