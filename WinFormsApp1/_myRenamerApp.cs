using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;



public class myRenamerApp_Controls
{
    public List<CheckBox>           optionList       = null;            // List of each option panel's main checkboxes

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
    public CheckBox                 option_004_ch_02 = null;
    public TextBox                  option_004_tb_01 = null;
    public TextBox                  option_004_tb_02 = null;
    public NumericUpDown            option_004_num_1 = null;

    public CheckBox                 option_005_ch_01 = null;
    public TextBox                  option_005_tb_01 = null;
    public NumericUpDown            option_005_num_1 = null;

    public CheckBox                 option_006_ch_01 = null;
    public CheckBox                 option_006_ch_02 = null;
    public CheckBox                 option_006_ch_04 = null;
    public myControls.myComboBox    option_006_cb_01 = null;
    public RadioButton              option_006_rb_01 = null;
    public RadioButton              option_006_rb_02 = null;
    public RadioButton              option_006_rb_05 = null;
    public CheckBox                 option_006_ch_03 = null;
    public RadioButton              option_006_rb_03 = null;
    public RadioButton              option_006_rb_04 = null;

    public CheckBox                 option_007_ch_01 = null;
    public NumericUpDown            option_007_num_1 = null;
    public NumericUpDown            option_007_num_2 = null;

    public CheckBox                 option_008_ch_01 = null;
    public NumericUpDown            option_008_num_1 = null;

    public CheckBox                 option_009_ch_01 = null;
    public RadioButton              option_009_rb_01 = null;
    public RadioButton              option_009_rb_02 = null;
    public RadioButton              option_009_rb_03 = null;
    public RadioButton              option_009_rb_04 = null;
    public RadioButton              option_009_rb_05 = null;
    public RadioButton              option_009_rb_06 = null;
    public RadioButton              option_009_rb_07 = null;
    public TextBox                  option_009_tb_01 = null;
    public CheckBox                 option_009_ch_02 = null;
};



public class myRenamerApp
{
    private myRenamerApp_Controls   _controls     = null;
    private myTree_DataGrid_Manager _myTDGManager = null;
    private ini_file_base           _ini          = null;

    // --------------------------------------------------------------------------------------------------------

    private class iniItem1
    {
        public int    _data;
        public string _name;
    };

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

    private void Application_onExit(object sender, EventArgs e)
    {
        // --- Save state of the controls that can be modified by the User ---
        if (_controls.option_001_cb_01.isChanged())
        {
            _ini[$"myControls.{_controls.option_001_cb_01.Obj().Name}"] = _controls.option_001_cb_01.getChanges();
        }

        if (_controls.option_006_cb_01.isChanged())
        {
            _ini[$"myControls.{_controls.option_006_cb_01.Obj().Name}"] = _controls.option_006_cb_01.getChanges();
        }

        // --- Save usage statistics for each selectable option ---
        foreach (var item in _controls.optionList)
        {
            string param = $"myCheckBoxCounters.{item.Name}";

            string sValOld = _ini[param];
            int    nValNew = (item.Tag != null) ? (int)(item.Tag) : 0;

            // Update ini-file:
            // -- if usage statistics has changed
            // -- if the option item has not beed added to ini-file yet
            if (nValNew > 0 || sValOld == null)
            {
                int nValOld = (sValOld != null) ? Int32.Parse(sValOld) : 0;

                nValNew += nValOld;
                _ini[param] = nValNew.ToString();
            }
        }

        _ini.save();
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
        _ini = new ini_file_base();
        _ini.read();

        // Subscribe to ApplicationExit event to finalize stuff
        Application.ApplicationExit += new EventHandler(Application_onExit);

        // Common checkbox changed event
        //      var checkboxChanged_CommonEvent = new EventHandler(checkboxChanged_Common);

        // Option 1
        _controls.option_001_ch_03.Checked = true;
        _controls.option_001_cb_01.setItems(_ini[$"myControls.{_controls.option_001_cb_01.Obj().Name}"]);

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

        // Option 6
        _controls.option_006_cb_01.setItems(_ini[$"myControls.{_controls.option_006_cb_01.Obj().Name}"]);

        // Option 8
        _controls.option_008_num_1.Minimum = -1000;
        _controls.option_008_num_1.Maximum = +1000;
    }

    // --------------------------------------------------------------------------------------------------------

    // Reposition option panels in most used order
    private void rePositionPanels()
    {
        var tBefore = DateTime.Now.Ticks;

        Control panel_base = _controls.option_001_ch_01.Parent.Parent as Control;

        // Collect each option's main checkboxes into a list
        if (_controls.optionList == null)
        {
            _controls.optionList = new List<CheckBox>();

            for (int i = 0; i < panel_base.Controls.Count; i++)
            {
                var control = panel_base.Controls[i];

                if (control is Panel)
                {
                    for (int j = 0; j < control.Controls.Count; j++)
                    {
                        var subControl = control.Controls[j];

                        if (subControl.Name.StartsWith("checkBox_Option_"))
                        {
                            _controls.optionList.Add(subControl as CheckBox);
                            break;
                        }
                    }
                }
            }
        }

        // -------------------------------------------------------------

        var dic = _ini.getDic();

        if (dic != null)
        {
            var list = new List<iniItem1>();

            // Get options from [myCheckBoxCounters] section in ini-file and put them into a list
            foreach (var item in dic)
            {
                if (item.Key.Contains("myCheckBoxCounters"))
                {
                    var listItem = new iniItem1();

                    listItem._data = Int32.Parse(item.Value.paramData);
                    listItem._name = item.Key.Substring(item.Key.IndexOf('.') + 1);

                    list.Add(listItem);
                }
            }

            if (list.Count > 0)
            {
                // Sort list using custom sorter method with the respect to int value
                list.Sort((iniItem1 a, iniItem1 b) =>
                {
                    if (a._data != b._data)
                        return (a._data > b._data) ? -1 : 1;
                    return 0;
                });

                void addDelimiterPanel()
                {
                    var delim = new Panel();
                    delim.Height = 2;
                    delim.Dock = DockStyle.Top;
                    delim.BackColor = Color.LightGray;
                    panel_base.Controls.Add(delim);
                    delim.BringToFront();
                }

                addDelimiterPanel();

                // Find option panels and sort them within [panel_base]
                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = 0; j < _controls.optionList.Count; j++)
                    {
                        if (list[i]._name == _controls.optionList[j].Name)
                        {
                            var panel = _controls.optionList[j].Parent as Panel;
                            panel.BringToFront();

                            // Also add delimiter panel
                            addDelimiterPanel();

                            break;
                        }
                    }
                }
            }

            list.Clear();

            var tDiff = (DateTime.Now.Ticks - tBefore);
            TimeSpan elapsedSpan = new TimeSpan(tDiff);
            _controls.richTextBox.AppendText($"call to _manager.rePositionPanels() took {elapsedSpan.TotalMilliseconds} ms\n");
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------
};
