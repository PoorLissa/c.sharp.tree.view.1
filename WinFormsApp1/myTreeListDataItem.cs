using System;
using System.Collections.Generic;



public class myTreeListDataItem : IComparable<myTreeListDataItem>
{
    // --------------------------------------------------------------------------------

    private string  _fileName;
    private bool    _isDir;
    private bool    _isHidden;

    public string Name
    {
        get {  return _fileName; }
        set { _fileName = value; }
    }

    public bool isDir
    {
        get {  return _isDir; }
        set { _isDir = value; }
    }

    public bool isHidden
    {
        get {  return _isHidden; }
        set { _isHidden = value; }
    }

    // --------------------------------------------------------------------------------

    // Default comparer:
    // - Directories precede files
    // - Case insensitive
    public int CompareTo(myTreeListDataItem other)
    {
        if (_isDir != other._isDir)
            return _isDir ? -1 : 1;

        int length = _fileName.Length > other._fileName.Length ? _fileName.Length : other._fileName.Length;

        return String.Compare(_fileName, 0, other._fileName, 0, length, null, System.Globalization.CompareOptions.IgnoreCase);
    }

    // --------------------------------------------------------------------------------

    public myTreeListDataItem(string file)
    {
        _fileName = file;
    }

    // --------------------------------------------------------------------------------
};