using System;
using System.Collections.Generic;

/*
    Provides the ability to keep the heistory of changes and restore file names to the original values
*/



public class myBackup
{
    private List<myBackupItem> _history = null;

    // --------------------------------------------------------------------------------

    private class myBackupItem
    {
        private List<string> _fileNameHistory = null;

        public string getLast()
        {
            if (_fileNameHistory.Count > 0)
                return _fileNameHistory[_fileNameHistory.Count - 1];

            return null;
        }

        public void Add(string oldName, string newName)
        {
            if (_fileNameHistory == null)
            {
                _fileNameHistory = new List<string>();
                _fileNameHistory.Add(oldName);
            }

            _fileNameHistory.Add(newName);
        }

        public void getHistory(ref string str)
        {
            string tab = "\t";

            for (int i = 0; i < _fileNameHistory.Count; i++)
            {
                str += i > 0 ? tab : "";
                str += _fileNameHistory[i];
                str += '\n';
            }
        }
    };

    // --------------------------------------------------------------------------------

    public myBackup()
    {
        _history = new List<myBackupItem>();
    }

    // --------------------------------------------------------------------------------

    //private Dictionary<string, int> dic = null;

    public void saveState(List<myTreeListDataItem> globalList, List<myTreeListDataItem> updatedList)
    {
        for (int i = 0; i < updatedList.Count; i++)
        {
            int id = updatedList[i].Id;

            // Item has changed:
            if (globalList[id].Name != updatedList[i].Name)
            {
                store(globalList[id].Name, updatedList[i].Name);
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------

    private void store(string oldName, string newName)
    {
        bool found = false;

        // Iterate through history and check if there's an item that contains [oldName] as its last entry
        for (int i = 0; i < _history.Count; i++)
        {
            string last = _history[i].getLast();

            if (last != null && last == oldName)
            {
                found = true;
                _history[i].Add(oldName, newName);
                break;
            }
        }

        if (!found)
        {
            var item = new myBackupItem();
            item.Add(oldName, newName);

            _history.Add(item);
        }

        return;
    }

    // --------------------------------------------------------------------------------

    public string getHistory()
    {
        string res = "";

        for (int i = 0; i < _history.Count; i++)
        {
            _history[i].getHistory(ref res);
            //res += "\n---\n";
        }

        return res;
    }

    // --------------------------------------------------------------------------------
};
