using System;
using System.Windows.Forms;



public class myRenamerApp_Controls
{
    public CheckBox                 option_001_ch_01 = null;
    public CheckBox                 option_001_ch_02 = null;
    public CheckBox                 option_001_ch_03 = null;
    public myControls.myComboBox    option_001_cb_01 = null;
};



public class myRenamerApp
{
    private myRenamerApp_Controls   _controls     = null;
    private myTree_DataGrid_Manager _myTDGManager = null;

    // --------------------------------------------------------------------------------------------------------

    public myRenamerApp(myRenamerApp_Controls controls, myTree_DataGrid_Manager_Initializer mtdgmi, string path, bool expandEmpty)
    {
        _controls = controls;
        _myTDGManager = new myTree_DataGrid_Manager(ref mtdgmi, path, expandEmpty);

        init();
    }

    // --------------------------------------------------------------------------------------------------------

    private void init()
    {
        var checkboxChanged_CommonEvent = new EventHandler(checkboxChanged_Common);

        _controls.option_001_ch_01.CheckedChanged += checkboxChanged_CommonEvent;
        _controls.option_001_ch_02.CheckedChanged += checkboxChanged_CommonEvent;

        _controls.option_001_cb_01.Obj().Items.Add("_");
        _controls.option_001_cb_01.Obj().Items.Add("_aaa_");
        _controls.option_001_cb_01.Obj().Items.Add("img");
    }

    // --------------------------------------------------------------------------------------------------------

    public void Rename()
    {
        new myRenamer(_myTDGManager, _controls).Rename();
    }

    // --------------------------------------------------------------------------------------------------------

    public void Undo_Rename()
    {
        new myRenamer(_myTDGManager).undo();
    }

    // --------------------------------------------------------------------------------------------------------

    // Every checkbox that needs additional processing on its Clicked Event should subscribe to this method
    private void checkboxChanged_Common(object sender, EventArgs e)
    {
        CheckBox cb = (CheckBox)(sender);

        do
        {
            if (cb.Name == _controls.option_001_ch_01.Name)
            {
                if (cb.Checked)
                    _controls.option_001_ch_02.Checked = false;
                break;
            }

            if (cb.Name == _controls.option_001_ch_02.Name)
            {
                if (cb.Checked)
                    _controls.option_001_ch_01.Checked = false;
                break;
            }

        } while (false);

        return;
    }

    // --------------------------------------------------------------------------------------------------------
};
