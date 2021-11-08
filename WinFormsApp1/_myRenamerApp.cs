using System;
using System.Windows.Forms;



public class myRenamerApp_Controls
{
    public RichTextBox              richTextBox      = null;

    public CheckBox                 option_001_ch_01 = null;
    public CheckBox                 option_001_ch_02 = null;
    public CheckBox                 option_001_ch_03 = null;
    public myControls.myComboBox    option_001_cb_01 = null;

    public CheckBox                 option_002_ch_01 = null;
    public CheckBox                 option_002_ch_02 = null;
    public NumericUpDown            option_002_num_1 = null;
    public NumericUpDown            option_002_num_2 = null;

    public CheckBox                 option_003_ch_01 = null;
    public NumericUpDown            option_003_num_1 = null;
    public RadioButton              option_003_rb_01 = null;
    public RadioButton              option_003_rb_02 = null;
    public TextBox                  option_003_tb_01 = null;

    public CheckBox                 option_004_ch_01 = null;
    public TextBox                  option_004_tb_01 = null;
    public TextBox                  option_004_tb_02 = null;
    public NumericUpDown            option_004_num_1 = null;

    public CheckBox                 option_005_ch_01 = null;
    public TextBox                  option_005_tb_01 = null;
    public NumericUpDown            option_005_num_1 = null;
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

        rePositionPanels();
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

    // Every checkbox that needs additional processing of its Clicked Event should subscribe to this method
    private void checkboxChanged_Common(object sender, EventArgs e)
    {
        CheckBox cb = (CheckBox)(sender);

        do
        {

        } while (false);

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    private void init()
    {
        // Common checkbox changed event
//      var checkboxChanged_CommonEvent = new EventHandler(checkboxChanged_Common);

        // Option 1
        _controls.option_001_ch_03.Checked = true;

        _controls.option_001_cb_01.Obj().Items.Add("_");
        _controls.option_001_cb_01.Obj().Items.Add("_aaa_");
        _controls.option_001_cb_01.Obj().Items.Add("img");

        // Option 2
        _controls.option_002_num_1.Value = 1;
        _controls.option_002_num_1.TextAlign = HorizontalAlignment.Center;
        _controls.option_002_num_2.TextAlign = HorizontalAlignment.Center;

        // Option 3
        _controls.option_003_num_1.Value = 1;
        _controls.option_003_num_1.TextAlign = HorizontalAlignment.Center;
        _controls.option_003_tb_01.PlaceholderText = "Substring";

        // Option 4
        _controls.option_004_tb_01.PlaceholderText = "src";
        _controls.option_004_tb_02.PlaceholderText = "dest";

        // Option 5
        _controls.option_005_tb_01.PlaceholderText = "[### - *]";
    }

    // --------------------------------------------------------------------------------------------------------

    private void rePositionPanels()
    {
        // todo: implement repositioning in most used order

/*
        this.panel_content_05.BringToFront();
        this.panel_content_04.BringToFront();
        this.panel_content_03.BringToFront();
        this.panel_content_02.BringToFront();
        this.panel_content_01.BringToFront();
*/
    }

    // --------------------------------------------------------------------------------------------------------
};
