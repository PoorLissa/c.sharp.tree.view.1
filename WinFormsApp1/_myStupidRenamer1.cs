

public class myStupidRenamer1
{
    private myTree_DataGrid_Manager _manager = null;

    // -------------------------------------------------------------

    public myStupidRenamer1(myTree_DataGrid_Manager manager)
    {
        _manager = manager;
    }

    // -------------------------------------------------------------

    public void rename(int n = 0)
    {
        var list = _manager.getSelectedFiles();

        // Files are renamed in normal order
        foreach (var item in list)
        {
            if (item.isDir)
                continue;

            switch (n)
            {
                case 1:
                    ren1_func(item);
                    break;

                default:
                    ren0_func(item);
                    break;
            }
        }

        // Folders are renamed in backwards order
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var item = list[i];

            if (!item.isDir)
                continue;

            switch (n)
            {
                case 1:
                    ren1_func(item);
                    break;

                default:
                    ren0_func(item);
                    break;
            }
        }

        _manager.update(list, true);

        return;
    }

    // -------------------------------------------------------------

    private void ren0_func(myTreeListDataItem item)
    {
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

    private void ren1_func(myTreeListDataItem item)
    {
        string oldName = item.Name;

        int pos = oldName.LastIndexOf('\\');
            pos = oldName.IndexOf('_', pos+1);

            item.Name = item.Name.Substring(0, pos);

        if (item.isDir)
        {
//            System.IO.Directory.Move(oldName, item.Name);
        }
        else
        {
//            System.IO.File.Move(oldName, item.Name);
        }
    }

    // -------------------------------------------------------------
};
