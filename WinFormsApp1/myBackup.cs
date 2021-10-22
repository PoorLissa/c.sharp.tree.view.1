using System;
using System.Collections.Generic;

/*
    Provides the ability to keep the heistory of changes and restore file names to the original values
*/



public class myBackup
{
    private List<myBackupItem>      _historyList = null;
    private Dictionary<string, int> _mapIndex    = null;
    // --------------------------------------------------------------------------------

    private class myBackupItem
    {
        // This list will contain a history of names for a single file
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

                // Add the initial name of the file
                _fileNameHistory.Add(oldName);
            }

            // Add the next changed name of this file
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
        _historyList = new List<myBackupItem>();
    }

    // --------------------------------------------------------------------------------

    public void saveState(List<myTreeListDataItem> globalList, List<myTreeListDataItem> updatedList)
    {
        if (_mapIndex == null)
        {
            _mapIndex = new Dictionary<string, int>();
        }

        for (int i = 0; i < updatedList.Count; i++)
        {
            int id = updatedList[i].Id;

            // Item has changed:
            if (globalList[id].Name != updatedList[i].Name)
            {
                //store(globalList[id].Name, updatedList[i].Name);    // 15 sec
                store_map(globalList, id, updatedList, i);
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------

    private void store(string oldName, string newName)
    {
        bool found = false;

        // Iterate through history and check if there's an item that contains [oldName] as its last entry

        for (int i = 0; i < _historyList.Count; i++)
        {
            string last = _historyList[i].getLast();

            if (last != null && last == oldName)
            {
                found = true;
                _historyList[i].Add(oldName, newName);
                break;
            }
        }

        if (!found)
        {
            var item = new myBackupItem();
            item.Add(oldName, newName);

            _historyList.Add(item);
        }

        return;
    }

    // --------------------------------------------------------------------------------

    private void store_map(List<myTreeListDataItem> globalList, int id, List<myTreeListDataItem> updatedList, int i)
    {
        string oldName = globalList[id].Name;
        string newName = updatedList[i].Name;

        int map_id = -1;

        if (_mapIndex.ContainsKey(oldName))
        {
            map_id = _mapIndex[oldName];

            // update the history
            _historyList[map_id].Add(oldName, newName);

            // remove old data from the map
            _mapIndex.Remove(oldName);
        }
        else
        {
            var item = new myBackupItem();
            item.Add(oldName, newName);

            map_id = _historyList.Count;

            // addd new item to the history
            _historyList.Add(item);
        }

        // update the map
        _mapIndex[newName] = map_id;

        return;
    }

    // --------------------------------------------------------------------------------

    public string getHistory()
    {
        string res = "";

        for (int i = 0; i < _historyList.Count; i++)
        {
            _historyList[i].getHistory(ref res);
        }

        return res;
    }

    // --------------------------------------------------------------------------------
};
