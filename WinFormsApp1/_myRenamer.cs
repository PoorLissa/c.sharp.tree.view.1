using System.Text;
using System.Windows.Forms;


public class myRenamer
{
    private myRenamerApp_Controls   _controls = null;
    private myTree_DataGrid_Manager _manager  = null;

    // --------------------------------------------------------------------------------------------------------

    public myRenamer(myTree_DataGrid_Manager manager, myRenamerApp_Controls controls = null)
    {
        _manager  = manager;
        _controls = controls;
    }

    // --------------------------------------------------------------------------------------------------------

    // Public Rename func
    public void Rename()
    {
        var list = _manager.getSelectedFiles(asCopy: true);
        string err = "";

        // Rename files first, then rename folders
        for (int j = 0; j < 2; j++)
        {
            bool isDir = (j != 0);

            // Rename everything in backwards order
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];

                if (item.isDir == isDir)
                {
                    // Go through every option that the User has selected and apply all the changes to file name
                    applyOptions(item, ref err);
                }
            }
        }

        if (err.Length > 0)
        {
            MessageBox.Show(err, "myRenamer::Rename: Error", MessageBoxButtons.OK);
        }

        _manager.update(list, true);

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Go through every option that the User has selected and apply the changes to file name
    // todo: rewrite it to use string builder (which can be tricky, as SB does not have indexof() and other such methods)
    private void applyOptions(myTreeListDataItem item, ref string err)
    {
        int pos = 0, num = 0;
        int pos_file = item.Name.LastIndexOf('\\') + 1;
        int pos_ext  = item.Name.LastIndexOf('.');

        string newName = null;
        string name = null;

        // Extract name without extension (or just full folder name)
        if (item.isDir)
        {
            name = item.Name.Substring(pos_file);
        }
        else
        {
            name = (pos_ext >= pos_file)
                        ? item.Name.Substring(pos_file, pos_ext - pos_file)
                        : item.Name.Substring(pos_file);
        }

        // --------------------------------------------------------------------------------

        // Option 1-1: Remove symbols before the [delimiter] is found. Start from [beginning]
        if (_controls.option_001_ch_01.Checked)
        {
            bool removeDelim = _controls.option_001_ch_03.Checked;
            string delim     = _controls.option_001_cb_01.Obj().Text;

            if (delim != null && delim.Length > 0)
            {
                pos = name.IndexOf(delim);

                if (pos != -1)
                    name = name.Substring(pos + (removeDelim ? delim.Length : 0));
            }
        }

        // Option 1-2: Remove symbols before the [delimiter] is found. Start from [end]
        if (_controls.option_001_ch_02.Checked)
        {
            bool removeDelim = _controls.option_001_ch_03.Checked;
            string delim     = _controls.option_001_cb_01.Obj().Text;

            if (delim != null && delim.Length > 0)
            {
                pos = name.LastIndexOf(delim);

                if (pos != -1)
                    name = name.Substring(0, pos + (removeDelim ? 0 : delim.Length));
            }
        }

        // --------------------------------------------------------------------------------

        // Option 2: Remove [num] symbols starting at position [pos]
        if (_controls.option_002_ch_01.Checked)
        {
            num = (int)(_controls.option_002_num_1.Value);
            pos = (int)(_controls.option_002_num_2.Value);

            if (pos < name.Length)
            {
                if (pos + num > name.Length)
                    num = name.Length - pos;

                if (_controls.option_002_ch_02.Checked)
                    pos = name.Length - (pos + num);

                name = name.Remove(pos, num);
            }
        }

        // --------------------------------------------------------------------------------

        // --------------------------------------------------------------------------------

        // Option 100: Remove n symbols starting at position of substring [s]

        // --------------------------------------------------------------------------------

        if (item.isDir)
        {
            newName = item.Name.Substring(0, pos_file) + name;
        }
        else
        {
            if (pos_ext >= pos_file)
                newName = item.Name.Substring(0, pos_file) + name + item.Name.Substring(pos_ext);
            else
                newName = item.Name.Substring(0, pos_file) + name;
        }

        RenamePhysical(item, newName, ref err);

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Revert selected files to their original names
    // todo: fix this:
    // - Recursively rename this: \test\bbb_111\001_zzz.txt     -- remove symbols on the right of '_'
    // - Try to restore -- files won't be restored, as the history is not valid (upper level dirs are not updated)
    // todo: when fixed, make it as a nested loop, not 2 loops
    public void undo()
    {
        var list = _manager.getSelectedFiles(asCopy: true);

        string err = "";

        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (!list[i].isDir)
            {
                var historyList = _manager.getBackup().getHistory(list[i].Name);

                if (historyList != null)
                {
                    RenamePhysical(list[i], historyList[0], ref err);
                }
            }
        }

        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i].isDir)
            {
                var historyList = _manager.getBackup().getHistory(list[i].Name);

                if (historyList != null)
                {
                    RenamePhysical(list[i], historyList[0], ref err);
                }
            }
        }

        if (err.Length > 0)
        {
            MessageBox.Show(err, "Error", MessageBoxButtons.OK);
        }

        _manager.update(list, true);

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Actual physical file renaming
    private void RenamePhysical(myTreeListDataItem item, string newName, ref string err)
    {
        if (newName != null && item.Name != newName)
        {
            bool ok = true;

            try
            {
                if (item.isDir)
                    System.IO.Directory.Move(item.Name, newName);

                if (!item.isDir)
                    System.IO.File.Move(item.Name, newName);
            }
            catch (System.Exception ex)
            {
                ok = false;
                err += ex.Message + "\n";
            }

            if (ok)
            {
                item.Name = newName;
                item.isChanged = true;
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

};

