using System;
using System.Windows.Forms;


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
    // -- Directories precede files
    // -- Case insensitive
    public int CompareTo_old(myTreeListDataItem other)
    {
        if (_isDir != other._isDir)
            return _isDir ? -1 : 1;

        return String.Compare(_fileName, other._fileName, null, System.Globalization.CompareOptions.IgnoreCase);
    }

    // Default comparer:
    // -- Directories precede files
    // -- Non CaseSensitive
    // -- Uses alpha-numeric sort:
    //  [name]
    //  _name
    //  aaa
    //  bbb
    //  file (1)
    //  file (11)
    //  file (111)
    //  ггг
    //  яяя
    public int CompareTo(myTreeListDataItem other)
    {
        if (_isDir != other._isDir)
            return _isDir ? -1 : 1;

        int i = 0;
        int res = _fileName.Length < other._fileName.Length ? -1 : 1;
        int minLen = res < 0 ? _fileName.Length : other._fileName.Length;

        while (i < minLen)
        {
            char ch1 =  this._fileName[i];
            char ch2 = other._fileName[i];

            if (myUtils.charIsDigit(ch1) && myUtils.charIsDigit(ch2))
            {
                int offset = 0;

                int n1 = myUtils.getInt_fromString( this._fileName, i, ref offset);
                int n2 = myUtils.getInt_fromString(other._fileName, i, ref offset);

                if (n1 != n2)
                    return n1 < n2 ? -1 : 1;

                i += offset;
            }
            else
            {
                if (ch1 != ch2)
                {
                    myUtils.charToLowerCase(ref ch1);
                    myUtils.charToLowerCase(ref ch2);

                    // Change sorting order for '_', '[', ']' -- put them before numbers and letters
                    if (ch1 > 90 && ch1 < 96)
                    {
                        if (ch1 == '[') ch1 = (char)(10);
                        if (ch1 == ']') ch1 = (char)(11);
                        if (ch1 == '_') ch1 = (char)(12);
                    }

                    if (ch2 > 90 && ch2 < 96)
                    {
                        if (ch2 == '[') ch2 = (char)(10);
                        if (ch2 == ']') ch2 = (char)(11);
                        if (ch2 == '_') ch2 = (char)(12);
                    }

                    if (ch1 != ch2)
                    {
                        return ch1 < ch2 ? -1 : 1;
                    }
                }
            }

            i++;
        }

        return res;
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
