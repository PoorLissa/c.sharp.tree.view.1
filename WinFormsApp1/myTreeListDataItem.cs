using System;



public class myTreeListDataItem : IComparable<myTreeListDataItem>
{
    // --------------------------------------------------------------------------------

    private int     _id;
    private string  _fileName;
    private bool    _isDir;
    private bool    _isHidden;

    public int Id
    {
        get {  return _id; }
        set { _id = value; }
    }

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
        _id = -1;
        _fileName = file;
    }

    // --------------------------------------------------------------------------------

    public myTreeListDataItem Clone()
    {
        var item = new myTreeListDataItem(_fileName);

        item._id       = _id;
        item.isDir     = _isDir;
        item._isHidden = _isHidden;

        return item;
    }

    // --------------------------------------------------------------------------------

    public static System.Collections.Generic.List<myTreeListDataItem> copyList(System.Collections.Generic.List<myTreeListDataItem> other)
    {
        var copy = new System.Collections.Generic.List<myTreeListDataItem>(other.Count);

        for (int i = 0; i < other.Count; i++)
            copy.Add(other[i].Clone());

        return copy;
    }

    // --------------------------------------------------------------------------------
};
