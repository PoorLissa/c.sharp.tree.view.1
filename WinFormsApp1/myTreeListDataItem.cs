using System;



public class myTreeListDataItem : IComparable<myTreeListDataItem>
{
    // --------------------------------------------------------------------------------

    private int     _id;
    private int     _num;
    private string  _fileName;
    private bool    _isDir;
    private bool    _isHidden;
    private bool    _isChanged;

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

    public bool isChanged
    {
        get {  return _isChanged; }
        set { _isChanged = value; }
    }

    public int num
    {
        get {  return _num; }
        set { _num = value; }
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

        item._id        = _id;
        item.isDir      = _isDir;
        item._isHidden  = _isHidden;
        item._isChanged = _isChanged;
        item._num       = _num;

        return item;
    }

    // --------------------------------------------------------------------------------
};
