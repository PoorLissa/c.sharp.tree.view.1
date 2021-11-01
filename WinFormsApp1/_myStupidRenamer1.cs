

public class myStupidRenamer1
{
    private myTree_DataGrid_Manager _manager = null;

    // -------------------------------------------------------------

    public myStupidRenamer1(myTree_DataGrid_Manager manager)
    {
        _manager = manager;
    }

    // -------------------------------------------------------------

    // Actual file renaming
    private void Rename(myTreeListDataItem item, string newName)
    {
        if (newName != null && item.Name != newName)
        {
            bool ok = true;

            try
            {
                if (item.isDir)
                    System.IO.Directory.Move(item.Name, newName);
                else
                    System.IO.File.Move(item.Name, newName);
            }
            catch(System.Exception)
            {
                ok = false;
            }

            if (ok)
            {
                item.Name = newName;
            }
        }
    }

    // -------------------------------------------------------------

    // Public Rename func
    public void rename(int param = 0)
    {
        var list_original = _manager.getSelectedFiles();

        // Make a copy, as otherwise we're going to change the global list from [myTree_DataGrid_Manager]
        var list = myTreeListDataItem.copyList(list_original);

        // Rename files first, then rename folders
        for (int j = 0; j < 2; j++)
        {
            bool isDir = (j != 0);

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];

                if (item.isDir == isDir)
                {
                    myUtils.logMsg(item.Name, "");

                    switch (param)
                    {
                        case 0:
                            ren0_func(item);
                            break;

                        case 1:
                            ren1_func(item);
                            break;

                        case 2:
                            ren2_func(item);
                            break;
                    }
                }
            }
        }

        _manager.update(list, true);

        return;
    }

    // -------------------------------------------------------------

    private void ren0_func(myTreeListDataItem item)
    {
        return;

        string oldName = item.Name;

        if (item.isDir)
        {
            item.Name = oldName + "_d";
            System.IO.Directory.Move(oldName, item.Name);
        }
        else
        {
            int pos = oldName.LastIndexOf('.');
            item.Name = oldName.Substring(0, pos) + "_f" + oldName.Substring(pos);
            System.IO.File.Move(oldName, item.Name);
        }
    }

    // -------------------------------------------------------------

    // Remove symbols on the right of '_'
    private void ren1_func(myTreeListDataItem item)
    {
        string oldName = item.Name;
        string newName = null;

        int pos_file = oldName.LastIndexOf('\\');
        int pos      = oldName.LastIndexOf('_');

        if (pos > pos_file)
        {
            if (item.isDir)
            {
                newName = oldName.Substring(0, pos);
            }
            else
            {
                int pos_ext = oldName.LastIndexOf('.');
                newName = oldName.Substring(0, pos) + oldName.Substring(pos_ext);
            }

            Rename(item, newName);
        }

        return;
    }

    // -------------------------------------------------------------

    // Remove symbols on the left of '_'
    private void ren2_func(myTreeListDataItem item)
    {
        string oldName = item.Name;
        string newName = null;

        int pos_file = oldName.LastIndexOf('\\');
        int pos = oldName.IndexOf('_', pos_file);

        if (pos > pos_file)
        {
            newName = oldName.Substring(0, pos_file + 1) + oldName.Substring(pos + 1);

            Rename(item, newName);
        }

        return;
    }

    // -------------------------------------------------------------

    // Revert selected files to their original names
    // todo: fix this:
    // - Recursively rename this: \test\bbb_111\001_zzz.txt     -- remove symbold on the right of '_'
    // - Try to restore -- files won't be restored, as the history is not right
    public void undo()
    {
        var list_original = _manager.getSelectedFiles();

        var list_copy = myTreeListDataItem.copyList(list_original);

        for (int i = list_copy.Count - 1; i >= 0; i--)
        {
            if (!list_copy[i].isDir)
            {
                var historyList = _manager.getBackup().getHistory(list_copy[i].Name);

                if (historyList != null)
                {
                    Rename(list_copy[i], historyList[0]);
                }
            }
        }

        for (int i = list_copy.Count - 1; i >= 0; i--)
        {
            if (list_copy[i].isDir)
            {
                var historyList = _manager.getBackup().getHistory(list_copy[i].Name);

                if (historyList != null)
                {
                    Rename(list_copy[i], historyList[0]);
                }
            }
        }

        _manager.update(list_copy, true);

        return;
    }

    // -------------------------------------------------------------
};
