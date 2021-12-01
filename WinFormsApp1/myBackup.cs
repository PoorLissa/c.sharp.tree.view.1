using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

/*
    Provides the ability to keep the history of changes and restore file names to the original values

    _historyList -- contains the list of all the changed names for a single file
    _mapIndex    -- maps the last known file name to its position in [_historyList] for fast retrieval
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

        // Flag indicating whether this item should be saved into the file upon exiting
        private bool _doSave = true;

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

            if (_doSave)
            {
                for (int i = 0; i < _fileNameHistory.Count; i++)
                {
                    str += i > 0 ? tab : "";
                    str += _fileNameHistory[i];
                    str += '\n';
                }

                str += "\n";
            }

            return;
        }

        public ref List<string> getHistory()
        {
            return ref _fileNameHistory;
        }

        public void doSave(bool mode)
        {
            _doSave = mode;
        }
    };

    // --------------------------------------------------------------------------------

    public myBackup()
    {
        _historyList = new List<myBackupItem>();
    }

    // --------------------------------------------------------------------------------

    public void saveHistoryToFile(string file = null)
    {
        string hist = getHistory();

        if (hist.Length > 0)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;

            path += (file != null) ? file : "\\__History.txt";

            if (!System.IO.File.Exists(path))
            {
                System.IO.File.CreateText(path).Dispose();
            }

            using (System.IO.StreamWriter sw = System.IO.File.AppendText(path))
            {
                sw.Write(hist);
                sw.WriteLine("---------------------------------------------------");
            }

            System.IO.File.SetAttributes(path, System.IO.FileAttributes.Hidden);
        }

        return;
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

    // Known issue:
    // Sometimes this will lead to invalid history.
    // Say, we have folders 1, 2, 3. Each of them is renamed:
    // 1 -> 2, 2 -> 3, 3 -> 4
    // In this case, the history will contain only a single record: 1 -> 2 -> 3 -> 4
    // todo: check this and see if it still is an issue.
    private void store_map(List<myTreeListDataItem> globalList, int id, List<myTreeListDataItem> updatedList, int i)
    {
        string oldName = globalList[id].Name;
        string newName = updatedList[i].Name;

        store_map(oldName, newName, isSaveable: true);

        return;
    }

    // --------------------------------------------------------------------------------

    // Known issue:
    // Sometimes this will lead to invalid history.
    // Say, we have folders 1, 2, 3. Each of them is renamed:
    // 1 -> 2, 2 -> 3, 3 -> 4
    // In this case, the history will contain only a single record: 1 -> 2 -> 3 -> 4
    // todo: check this and see if it still is an issue.
    private void store_map(string oldName, string newName, bool isSaveable)
    {
        int map_id = -1;

        if (_mapIndex == null)
        {
            _mapIndex = new Dictionary<string, int>();
        }

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
            item.doSave(isSaveable);
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

    // Given file, returns its history list (if any)
    public List<string> getHistory(string fileName)
    {
        if (_mapIndex.ContainsKey(fileName))
            return _historyList[_mapIndex[fileName]].getHistory();

        return null;
    }

    // --------------------------------------------------------------------------------

    // Given file, returns its history list (if any)
    public void makeSaveable(string fileName)
    {
        if (_mapIndex.ContainsKey(fileName))
            _historyList[_mapIndex[fileName]].doSave(true);
    }

    // --------------------------------------------------------------------------------

    public bool isEmpty()
    {
        return _historyList.Count == 0;
    }

    // --------------------------------------------------------------------------------

    public void loadFromFile()
    {
        void trimSB(ref StringBuilder sb)
        {
            bool doStop = false;

            // Trim the line
            while (sb.Length > 0 && doStop != true)
            {
                int oldLen = sb.Length;

                if (sb[0] == ' ' || sb[0] == '\t' || sb[0] == '\n' || sb[0] == '\r')
                    sb.Remove(0, 1);

                int back = sb.Length - 1;

                if (back >= 0)
                {
                    if (sb[back] == ' ' || sb[back] == '\t' || sb[back] == '\n' || sb[back] == '\r' || sb[back] == ';')
                        sb.Remove(back, 1);
                }

                if (sb.Length == oldLen)
                    doStop = true;
            }

            return;
        }

        // Get line bounded by '\n' symbols
        int getLine(ref string src, ref StringBuilder line, ref int pos)
        {
            line.Clear();

            int i = pos;

            if (pos < src.Length)
            {
                pos = src.IndexOf("\n", pos + 1);

                if (pos < 0)
                    pos = src.Length;

                for (; i < pos; i++)
                {
                    line.Append(src[i]);
                }

                if (pos == src.Length)
                    pos = 0;
            }

            trimSB(ref line);

            return pos;
        };

        // ------------------------------------------------------------

        string path = AppDomain.CurrentDomain.BaseDirectory;
        path += "\\__History.txt";

        if (System.IO.File.Exists(path))
        {
            string rawFile = System.IO.File.ReadAllText(path);

            var lineSB = new StringBuilder();
            int pos = 0;

            string prevLine = null;

            while (getLine(ref rawFile, ref lineSB, ref pos) != 0)
            {
                string line = lineSB.ToString();

                if (line.Length == 0 || line[0] == '-')
                {
                    prevLine = null;
                }
                else
                {
                    if (prevLine == null)
                    {
                        prevLine = line;
                    }
                    else
                    {
                        store_map(prevLine, line, isSaveable: false);

                        prevLine = line;
                    }
                }
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------
};
