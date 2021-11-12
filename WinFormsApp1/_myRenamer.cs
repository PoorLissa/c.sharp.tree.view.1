using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

/*
    Adding new subpanels to Tab Control:
	    - in Properties window, find and select from the dropdown list first divider panel (panel_div_0)
	    - with pressed Ctrl, click any existing content panel that is located lower than the selected divider panel
	    - copy the 2 panels from the UI window: Ctrl+C
	    - select panel 'panel_base' in Properties window from the dropdown list of widgets
	    - click anywhere on the black background around the UI window
	    - press Ctrl+V
	    - the copied panels are inserted at the bottom into 'panel_base'

    todo
        - Finish Simulate
        - Add preview function
*/


public class myRenamer
{
    private myRenamerApp_Controls   _controls   = null;
    private myTree_DataGrid_Manager _manager    = null;

    // --------------------------------------------------------------------------------------------------------

    public myRenamer(myTree_DataGrid_Manager manager, myRenamerApp_Controls controls = null)
    {
        _manager  = manager;
        _controls = controls;
    }

    // --------------------------------------------------------------------------------------------------------

    private bool Simulate()
    {
        bool res = true;

        var list = _manager.getSelectedFiles(asCopy: true);
        string err = "";

        // Files first, then folders
        for (int j = 0; j < 2; j++)
        {
            bool isDir = (j != 0);

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];

                if (item.isDir == isDir)
                {
                    applyOptions(item, ref err);
                }
            }
        }

        return res;
    }

    // --------------------------------------------------------------------------------------------------------

    private void renTo_TmpName(myTreeListDataItem item, ref string err)
    {
        int num = 0;
        var sb = new StringBuilder(item.Name);
        sb.Append("#");

        try
        {
            while (true)
            {
                if (item.isDir)
                {
                    if (!System.IO.Directory.Exists(sb.ToString()))
                    {
                        System.IO.Directory.Move(item.Name, sb.ToString());
                        break;
                    }
                }
                else
                {
                    if (!System.IO.File.Exists(sb.ToString()))
                    {
                        System.IO.File.Move(item.Name, sb.ToString());
                        break;
                    }
                }

                sb.Clear();
                sb.Append(item.Name);
                sb.Append("#");
                sb.Append(num++);
            }
        }
        catch (System.Exception ex)
        {
            err += ex.Message + "\n";
        }

        item.Name = sb.ToString();

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Public Rename func
    public void Rename()
    {
        var tBefore = System.DateTime.Now.Ticks;

        var list = _manager.getSelectedFiles(asCopy: true);
        string err = "";

        var tDiff = (System.DateTime.Now.Ticks - tBefore);
        System.TimeSpan elapsedSpan = new System.TimeSpan(tDiff);
        _controls.richTextBox.AppendText($"call to _manager.getSelectedFiles(asCopy: true) took {elapsedSpan.TotalMilliseconds} ms\n");


        // For every checked option, update its usage counter
        foreach (var item in _controls.optionList)
            updateCnt(item);


        // Step 1: rename to tmp names
        {
            // Rename files first, then rename folders
            for (int j = 0; j < 2; j++)
            {
                bool isDir = (j != 0);

                // Rename everything to tmp in backwards order
                for (int i = list.Count - 1; i >= 0; i--)
                    if (list[i].isDir == isDir)
                        renTo_TmpName(list[i], ref err);
            }

            if (err.Length > 0)
            {
                MessageBox.Show(err, "myRenamer.Rename: Error 1", MessageBoxButtons.OK);
            }

            _manager.update(list, true, false);
        }


        // Step 2: apply options and make a final rename
        {
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
                MessageBox.Show(err, "myRenamer.Rename: Error 2", MessageBoxButtons.OK);
            }

            _manager.update(list, true, true);

        }

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
        int pos_tmp  = item.Name.LastIndexOf('#');

        string newName = null;
        string name    = null;
        string ext     = null;

        // Extract name and extension:
        if (item.isDir)
        {
            // In case of folder, just its full name
            name = item.Name.Substring(pos_file, pos_tmp - pos_file);
        }
        else
        {
            name = (pos_ext >= pos_file)
                        ? item.Name.Substring(pos_file, pos_ext - pos_file)
                        : item.Name.Substring(pos_file, pos_tmp - pos_file);

            ext = (pos_ext >= pos_file)
                ? item.Name.Substring(pos_ext, pos_tmp - pos_ext)
                : "";
        }

        // --------------------------------------------------------------------------------

        // Option 1: Remove symbols before the [delimiter] is found
        if (_controls.option_001_ch_01.Checked)
        {
            bool removeDelim = _controls.option_001_ch_03.Checked;
            string delim     = _controls.option_001_cb_01.Obj().Text;

            if (delim != null && delim.Length > 0)
            {
                if (_controls.option_001_ch_02.Checked)
                {
                    // Start from [end]
                    pos = name.LastIndexOf(delim);

                    if (pos != -1)
                        name = name.Substring(0, pos + (removeDelim ? 0 : delim.Length));
                }
                else
                {
                    // Start from [beginning]
                    pos = name.IndexOf(delim);

                    if (pos != -1)
                        name = name.Substring(pos + (removeDelim ? delim.Length : 0));
                }
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

        // Option 3: Remove n symbols before or after the position of substring [str]
        if (_controls.option_003_ch_01.Checked)
        {
            if (_controls.option_003_rb_01.Checked || _controls.option_003_rb_02.Checked)
            {
                string delim = _controls.option_003_tb_01.Text;

                if (delim != null && delim.Length > 0)
                {
                    num = (int)(_controls.option_003_num_1.Value);
                    pos = name.IndexOf(delim);

                    if (pos >= 0)
                    {
                        if (_controls.option_003_rb_02.Checked)
                        {
                            pos += delim.Length;

                            if (pos + num > name.Length)
                                num = name.Length - pos;
                        }
                        else
                        {
                            if (pos < num)
                                num = pos;

                            pos -= num;
                        }

                        name = name.Remove(pos, num);
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------

        // Option 4: Replace occurence of substring [src] with other string, [dst] -- cnt times
        if (_controls.option_004_ch_01.Checked)
        {
            string src = _controls.option_004_tb_01.Text;
            string dst = _controls.option_004_tb_02.Text;

            ref string strRef = ref (_controls.option_004_ch_02.Checked ? ref ext : ref name);

            if (src != null && src.Length > 0)
            {
                pos = strRef.IndexOf(src);

                if (pos >= 0)
                {
                    int cnt = (int)(_controls.option_004_num_1.Value);
                        cnt = cnt > 0 ? cnt : 1000;                         // In case cnt == 0, replace all occurences
                    int offset = 0;

                    StringBuilder sb = new StringBuilder(strRef);

                    do
                    {
                        sb.Remove(pos + offset, src.Length);
                        sb.Insert(pos + offset, dst);

                        pos = strRef.IndexOf(src, pos + 1);

                        offset += (dst.Length - src.Length);
                    }
                    while (pos >= 0 && --cnt != 0);

                    strRef = sb.ToString();
                }
            }
        }

        // --------------------------------------------------------------------------------

        // Option 5: Rename using template
        if (_controls.option_005_ch_01.Checked)
        {
            string strTemplate = _controls.option_005_tb_01.Text;

            StringBuilder res = new StringBuilder(33);

            if (strTemplate != null && strTemplate.Length > 0)
            {
                for (int i = 0; i < strTemplate.Length; i++)
                {
                    bool doAppendChar = true;

                    switch (strTemplate[i])
                    {
                        // Insert original name
                        case '*':
                            doAppendChar = false;
                            res.Append(name);
                            break;

                        // Insert number
                        case '#':
                            doAppendChar = false;

                            // Get number of '#' symbols
                            int cnt = 1;
                            while (i + cnt < strTemplate.Length && strTemplate[i + cnt] == '#')
                                cnt++;
                            i += cnt - 1;

                            int n = (int)(_controls.option_005_num_1.Value) + item.num - 1;

                            string strNUM = n.ToString();

                            for(int j = strNUM.Length; j < cnt; j++)
                                res.Append('0');
                            res.Append(strNUM);
                            break;
                    }

                    if (doAppendChar)
                        res.Append(strTemplate[i]);
                }

                name = res.ToString();
            }
        }

        // --------------------------------------------------------------------------------

        // Option 6: File name to UPPER/lower case
        if (_controls.option_006_ch_01.Checked)
        {
            if (_controls.option_006_ch_02.Checked)
            {
                if (_controls.option_006_rb_01.Checked)
                {
                    name = name.ToUpper();
                }

                if (_controls.option_006_rb_02.Checked)
                {
                    name = name.ToLower();
                }

                if (_controls.option_006_rb_05.Checked)
                {
                    renToCamelCase(ref name, skipShortWords: _controls.option_006_ch_04.Checked);
                }
            }

            if (_controls.option_006_ch_03.Checked)
            {
                if (_controls.option_006_rb_03.Checked)
                {
                    ext = ext.ToUpper();
                }

                if (_controls.option_006_rb_04.Checked)
                {
                    ext = ext.ToLower();
                }
            }
        }

        // --------------------------------------------------------------------------------

        // Option 7: Find numeric sequence and prepend it with zeroes
        if (_controls.option_007_ch_01.Checked)
        {
            num = (int)(_controls.option_007_num_1.Value);  // Total final length of numeric sequence
            pos = (int)(_controls.option_007_num_2.Value);  // Number of numeric sequence in the file name

            for (int i = 0; i < name.Length; i++)
            {
                if (myUtils.charIsDigit(name[i]))
                {
                    int offset = 0;
                    myUtils.getInt_fromString(name, i, ref offset);

                    if (pos-- == 1)
                    {
                        // Now offset shows us the length of this numeric sequence
                        var sb = new StringBuilder(name);

                        for (int j = 0; j < (num - offset); j++)
                            sb.Insert(i, '0');

                        name = sb.ToString();
                        break;
                    }

                    i += offset;
                }
            }
        }

        // --------------------------------------------------------------------------------

        // Option 8: Find numeric sequence and increase/decrease it it by number n
        if (_controls.option_008_ch_01.Checked)
        {
            num = (int)(_controls.option_008_num_1.Value);  // Number to add to the sequence
//            pos = (int)(_controls.option_007_num_2.Value);  // Number of numeric sequence in the file name

            pos = 1;

            for (int i = 0; i < name.Length; i++)
            {
                if (myUtils.charIsDigit(name[i]))
                {
                    int offset = 0;
                    int currentNum = myUtils.getInt_fromString(name, i, ref offset);

                    if (pos-- == 1)
                    {
                        // Now offset shows us the length of this numeric sequence
                        var sb = new StringBuilder(name);

                        currentNum += num;

                        sb.Remove(i, offset);
                        sb.Insert(i, currentNum.ToString());

                        name = sb.ToString();
                        break;
                    }

                    i += offset;
                }
            }
        }

        // --------------------------------------------------------------------------------


        if (item.isDir)
        {
            newName = item.Name.Substring(0, pos_file) + name;
        }
        else
        {
            if (pos_ext >= pos_file)
                newName = item.Name.Substring(0, pos_file) + name + ext;
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

        for (int step = 0; step < 2; step++)
        {
            // Files
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (!list[i].isDir)
                {
                    var historyList = _manager.getBackup().getHistory(list[i].Name);

                    if (historyList != null)
                    {
                        if (step == 0)
                        {
                            RenamePhysical(list[i], historyList[1], ref err);
                        }

                        if (step == 1)
                        {
                            RenamePhysical(list[i], historyList[0], ref err);
                        }
                    }
                }
            }

            // Directories
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].isDir)
                {
                    var historyList = _manager.getBackup().getHistory(list[i].Name);

                    if (historyList != null)
                    {
                        if (step == 0)
                        {
                            RenamePhysical(list[i], historyList[1], ref err);
                        }

                        if (step == 1)
                        {
                            RenamePhysical(list[i], historyList[0], ref err);
                        }
                    }
                }
            }

            if (err.Length > 0)
            {
                MessageBox.Show(err, "Error", MessageBoxButtons.OK);
            }

            bool updWidgets = (step > 0);
            _manager.update(list, true, updWidgets);
        }

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
                {
                    System.IO.Directory.Move(item.Name, newName);
                }
                else
                {
                    System.IO.File.Move(item.Name, newName);
                }
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

    // Count the number of uses of the selected option
    // Store it in the Tag property
    private void updateCnt(object obj)
    {
        var cb = obj as CheckBox;

        if (cb.Checked)
        {
            if (cb.Tag == null)
            {
                cb.Tag = new int();
                cb.Tag = 0;
            }

            cb.Tag = (int)(cb.Tag) + 1;
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    private void renToCamelCase(ref string name, bool skipShortWords)
    {
        // Every word found directly on the right of these substrings must start with capital letter
        string[] arr = { " - ", "; " };

        StringBuilder sb = new StringBuilder(name.ToLower());

        bool firstLetterFound = false;

        for (int i = 0; i < sb.Length; i++)
        {
            // New word is starting from here
            if (i == 0 || sb[i-1] == ' ')
            {
                // Extract current word and check if it belongs to the [_shortWords] list
                if (skipShortWords)
                {
                    // But first, check if we still should start this word with Capital letter:
                    bool mustBeCapital = (i == 0) || !firstLetterFound;

                    // Still check if must be capital letter...
                    if (!mustBeCapital)
                    {
                        for (int j = 0; j < arr.Length; j++)
                        {
                            if (i >= arr[j].Length)
                            {
                                bool found_InArr = myUtils.fastStrCompare(arr[j], name, i - arr[j].Length, arr[j].Length, caseSensitive: false);

                                if (found_InArr)
                                {
                                    mustBeCapital = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!mustBeCapital)
                    {
                        bool found_InList = false;
                        int pos = name.IndexOf(' ', i);

                        var _shortWords = _controls.option_006_cb_01.Obj().Items;

                        for (int j = 0; j < _shortWords.Count; j++)
                        {
                            found_InList = myUtils.fastStrCompare(_shortWords[j].ToString(), name, i, pos - i, caseSensitive: false);

                            if (found_InList)
                                break;
                        }

                        if (found_InList)
                            continue;
                    }
                }

                char ch = sb[i];
                myUtils.charToUpperCase(ref ch);
                sb[i] = ch;
            }

            if (!firstLetterFound)
                if (myUtils.charIsLetter(sb[i]))
                    firstLetterFound = true;
        }

        name = sb.ToString();

        return;
    }

    // --------------------------------------------------------------------------------------------------------
};

