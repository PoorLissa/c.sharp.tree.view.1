using myControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

// todo:
// - add '?' with info panel on mouse hover near the controls/options

public class myRenamerApp_Controls
{
    public List<CheckBox>   optionList       = null;    // List of each option panel's main checkboxes.

    public RichTextBox      richTextBox      = null;

    public CheckBox         option_001_ch_01 = null;
    public CheckBox         option_001_ch_02 = null;
    public CheckBox         option_001_ch_03 = null;
    public myComboBox       option_001_cb_01 = null;
    public RadioButton      option_001_rb_01 = null;
    public RadioButton      option_001_rb_02 = null;

    public CheckBox         option_002_ch_01 = null;
    public CheckBox         option_002_ch_02 = null;
    public NumericUpDown    option_002_num_1 = null;
    public NumericUpDown    option_002_num_2 = null;

    public CheckBox         option_003_ch_01 = null;
    public CheckBox         option_003_ch_02 = null;
    public NumericUpDown    option_003_num_1 = null;
    public NumericUpDown    option_003_num_2 = null;
    public RadioButton      option_003_rb_01 = null;
    public RadioButton      option_003_rb_02 = null;
    public TextBox          option_003_tb_01 = null;

    public CheckBox         option_004_ch_01 = null;
    public CheckBox         option_004_ch_02 = null;
    public TextBox          option_004_tb_01 = null;
    public TextBox          option_004_tb_02 = null;
    public NumericUpDown    option_004_num_1 = null;

    public CheckBox         option_005_ch_01 = null;
    public myComboBox       option_005_cb_01 = null;
    public NumericUpDown    option_005_num_1 = null;
    public ComboBox         option_005_cb_02 = null;
    public Button           option_005_btn_1 = null;

    public CheckBox         option_006_ch_01 = null;
    public CheckBox         option_006_ch_02 = null;
    public CheckBox         option_006_ch_04 = null;
    public myComboBox       option_006_cb_01 = null;
    public RadioButton      option_006_rb_01 = null;
    public RadioButton      option_006_rb_02 = null;
    public RadioButton      option_006_rb_05 = null;
    public CheckBox         option_006_ch_03 = null;
    public RadioButton      option_006_rb_03 = null;
    public RadioButton      option_006_rb_04 = null;

    public CheckBox         option_007_ch_01 = null;
    public NumericUpDown    option_007_num_1 = null;
    public NumericUpDown    option_007_num_2 = null;

    public CheckBox         option_008_ch_01 = null;
    public NumericUpDown    option_008_num_1 = null;
    public NumericUpDown    option_008_num_2 = null;

    public CheckBox         option_009_ch_01 = null;
    public ComboBox         option_009_cb_01 = null;
    public RadioButton      option_009_rb_01 = null;
    public RadioButton      option_009_rb_02 = null;
    public RadioButton      option_009_rb_03 = null;
    public RadioButton      option_009_rb_04 = null;
    public RadioButton      option_009_rb_05 = null;
    public RadioButton      option_009_rb_06 = null;
    public RadioButton      option_009_rb_07 = null;
    public TextBox          option_009_tb_01 = null;
    public CheckBox         option_009_ch_02 = null;

    public CheckBox         option_010_ch_01 = null;
    public TextBox          option_010_tb_01 = null;
    public RadioButton      option_010_rb_01 = null;
    public RadioButton      option_010_rb_02 = null;
    public NumericUpDown    option_010_num_1 = null;
    public NumericUpDown    option_010_num_2 = null;
    public NumericUpDown    option_010_num_3 = null;
    public NumericUpDown    option_010_num_4 = null;
    public CheckBox         option_010_ch_02 = null;

    public CheckBox         option_011_ch_01 = null;
    public RadioButton      option_011_rb_01 = null;
    public RadioButton      option_011_rb_02 = null;
    public RadioButton      option_011_rb_03 = null;
    public RadioButton      option_011_rb_04 = null;
    public TextBox          option_011_tb_01 = null;

    public CheckBox         option_012_ch_01 = null;

    public CheckBox         option_013_ch_01 = null;
    public TextBox          option_013_tb_01 = null;
    public TextBox          option_013_tb_02 = null;
    public RadioButton      option_013_rb_01 = null;
    public RadioButton      option_013_rb_02 = null;
    public CheckBox         option_013_ch_02 = null;
    public NumericUpDown    option_013_num_1 = null;
};



public class myRenamerApp
{
    public static int appDpi = -1;

    private myRenamerApp_Controls   _controls     = null;
    private myTree_DataGrid_Manager _myTDGManager = null;
    private ini_file_base           _ini          = null;
    private bool             _useAlternatingColor = true;
    private Form                    _form         = null;

    // --------------------------------------------------------------------------------------------------------

    private class iniItem1
    {
        public int    _data;
        public string _name;
    };

    // --------------------------------------------------------------------------------------------------------

    public myRenamerApp(myRenamerApp_Controls controls, myTree_DataGrid_Manager_Initializer mtdgmi, string path, bool expandEmpty)
    {
        appDpi = mtdgmi.form.DeviceDpi;

        _controls = controls;
        _myTDGManager = new myTree_DataGrid_Manager(ref mtdgmi, path, expandEmpty);
        _form = mtdgmi.form;

        init();

        rePositionPanels();
    }

    // --------------------------------------------------------------------------------------------------------

    public void Rename()
    {
        try
        {
            myRenamer.getInstance().Rename();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "myRenamerApp: Failed to rename files", MessageBoxButtons.OK);
        }
    }

    // --------------------------------------------------------------------------------------------------------

    public void Undo(bool useHistoryFile = false)
    {
        try
        {
            myRenamer.getInstance().undo(useHistoryFile);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "myRenamerApp: Failed to undo", MessageBoxButtons.OK);
        }
    }

    // --------------------------------------------------------------------------------------------------------

    private void _form_onFormClosing(object sender, FormClosingEventArgs e)
    {
        // Save panel settings
        foreach (var item in _controls.optionList)
        {
            myPanel p = item.Parent as myPanel;

            if (p != null && p.isSelected)
            {
                string param = $"myPanelSettings.{p.Name}";

                _ini[param] = p.getSettings();
            }
        }
    }

    // --------------------------------------------------------------------------------------------------------

    private void Application_onExit(object sender, EventArgs e)
    {
        // --- Save state of the controls that can be modified by the User ---
        if (_controls.option_001_cb_01.isChanged())
        {
            _ini[$"myControls.{_controls.option_001_cb_01.Obj().Name}"] = _controls.option_001_cb_01.getChanges();
        }

        if (_controls.option_005_cb_01.isChanged())
        {
            _ini[$"myControls.{_controls.option_005_cb_01.Obj().Name}"] = _controls.option_005_cb_01.getChanges();
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
                int nValOld = (sValOld != null) ? int.Parse(sValOld) : 0;

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
        CheckBox cb = sender as CheckBox;

        if (cb != null)
        {
            do
            {

                if (cb.Name.StartsWith("checkBox_Option_"))
                {
                    var parent = cb.Parent as myPanel;
                    parent.setUpLatest(_ini, cb.Checked);
                    parent.isSelected = cb.Checked;
                    break;
                }

            } while (false);

        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Every radio button that needs additional processing of its Checked Event should subscribe to this method
    private void radioButtonChecked_Common(object sender, EventArgs e)
    {
        RadioButton rb = sender as RadioButton;

        if (rb != null)
        {
            do
            {

                // While we have only 2 buttons, no need to check the second one
                if (rb == _controls.option_013_rb_01)
                {
                    _controls.option_013_ch_02.Text = _controls.option_013_rb_01.Checked
                                                            ? "Count Position from the End"
                                                            : "Insert at the Back of the Substring";
                }

            } while (false);

        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Every button that needs additional processing of its Clicked Event should subscribe to this method
    private void buttonClicked_Common(object sender, EventArgs e)
    {
        Button btn = sender as Button;

        if (btn != null)
        {
            do
            {

                if (btn == _controls.option_005_btn_1)
                {
                    switch (_controls.option_005_cb_02.SelectedIndex)
                    {
                        case 0:
                            _controls.option_005_cb_01.Obj().Text += "###";
                            break;

                        case 1:
                            _controls.option_005_cb_01.Obj().Text += "*";
                            break;

                        case 2:
                            _controls.option_005_cb_01.Obj().Text += "%parent%";
                            break;
                    }
                }

            } while (false);

        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    private void init()
    {
        // Initialize the instance, so it becomes available later on
        new myRenamer(_myTDGManager, _controls);

        _ini = new ini_file_base();
        _ini.read();

        // The form will receive key events before the event is passed to the control that has focus
        _form.KeyPreview = true;

        // To be able to react to global key commands
        _form.KeyDown += new KeyEventHandler(on_KeyDown);
        _form.FormClosing += new FormClosingEventHandler(_form_onFormClosing);

        // Subscribe to ApplicationExit event to finalize stuff
        Application.ApplicationExit += new EventHandler(Application_onExit);

        // Common checkbox changed event
        // var checkboxChanged_CommonEvent = new EventHandler(checkboxChanged_Common);

        // Common radio button checked event
        var radioButtonChecked_CommonEvent = new EventHandler(radioButtonChecked_Common);

        // Common button clicked event
        var buttonClicked_CommonEvent = new EventHandler(buttonClicked_Common);

        // Option 1
        _controls.option_001_ch_03.Checked = true;
        _controls.option_001_cb_01.setItems(_ini[$"myControls.{_controls.option_001_cb_01.Obj().Name}"]);
        _controls.option_001_rb_01.Checked = true;

        // Option 2
        _controls.option_002_num_1.Value = 1;
        _controls.option_002_num_1.TextAlign = HorizontalAlignment.Center;
        _controls.option_002_num_2.TextAlign = HorizontalAlignment.Center;

        // Option 3
        _controls.option_003_num_1.Value = 1;
        _controls.option_003_num_2.Minimum = 1;
        _controls.option_003_num_1.TextAlign = HorizontalAlignment.Center;
        _controls.option_003_tb_01.PlaceholderText = "Substring";

        // Option 4
        _controls.option_004_tb_01.PlaceholderText = "src";
        _controls.option_004_tb_02.PlaceholderText = "dest";

        // Option 5
        _controls.option_005_cb_01.setItems(_ini[$"myControls.{_controls.option_005_cb_01.Obj().Name}"], doSelectFirstItem: false);
        _controls.option_005_cb_02.Items.Add("Numeric sequence");
        _controls.option_005_cb_02.Items.Add("Original file name");
        _controls.option_005_cb_02.Items.Add("Parent folder name");
        _controls.option_005_cb_02.SelectedIndex = 0;
        _controls.option_005_cb_02.DropDownStyle = ComboBoxStyle.DropDownList;
        _controls.option_005_btn_1.Click += buttonClicked_CommonEvent;

        // Option 6
        _controls.option_006_cb_01.setItems(_ini[$"myControls.{_controls.option_006_cb_01.Obj().Name}"]);

        // Option 8
        _controls.option_008_num_1.Minimum = -1000;
        _controls.option_008_num_1.Maximum = +1000;

        _controls.option_008_num_1.Value = 1;
        _controls.option_008_num_2.Value = 1;

        // Option 9
        _controls.option_009_cb_01.SelectedIndex = 0;

        // Option 10
        _controls.option_010_tb_01.PlaceholderText = "Delimiter";

        // Option 11:
        _controls.option_011_tb_01.Text = "yyyy.MM.dd - *";

        // Option 13:
        _controls.option_013_num_1.Value = 0;
        _controls.option_013_tb_01.PlaceholderText = "String";
        _controls.option_013_tb_02.PlaceholderText = "Substring";
        _controls.option_013_ch_02.Text = "Count Position from the End";
        _controls.option_013_rb_01.CheckedChanged += radioButtonChecked_CommonEvent;
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
            // Common checkbox changed event
            var checkboxChanged_CommonEvent = new EventHandler(checkboxChanged_Common);

            _controls.optionList = new List<CheckBox>();

            for (int i = 0; i < panel_base.Controls.Count; i++)
            {
                var control = panel_base.Controls[i];

                if (control is myPanel)
                {
                    (control as myPanel).isSelected = false;

                    for (int j = 0; j < control.Controls.Count; j++)
                    {
                        var subControl = control.Controls[j];

                        if (subControl.Name.StartsWith("checkBox_Option_"))
                        {
                            var checkbox = subControl as CheckBox;

                            if (checkbox != null)
                            {
                                _controls.optionList.Add(checkbox);
                                checkbox.CheckedChanged += checkboxChanged_CommonEvent;
                            }
                            break;
                        }
                    }
                }
            }
        }

        // -------------------------------------------------------------

        var dic = _ini.getDic();
        int cnt = 0;

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

                // Find option panels and sort them within [panel_base]
                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = 0; j < _controls.optionList.Count; j++)
                    {
                        if (list[i]._name == _controls.optionList[j].Name)
                        {
                            var panel = _controls.optionList[j].Parent as myPanel;

                            panel.UseCustomBorder = true;

                            if (_useAlternatingColor)
                            {
                                panel.BackColor = (cnt++ % 2 == 0)
                                    ? Color.FromArgb( 1, Color.LightGray)
                                    : Color.FromArgb(25, Color.Azure);
                            }

                            // Make the panel last from the top
                            panel.BringToFront();

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
        else
        {
            // Only for the first time run, when the ini file does not exist yet
            for (int j = 0; j < _controls.optionList.Count; j++)
            {
                var panel = _controls.optionList[j].Parent as myPanel;

                panel.UseCustomBorder = true;

                if (_useAlternatingColor)
                {
                    panel.BackColor = (cnt++ % 2 == 0)
                            ? Color.FromArgb( 1, Color.LightGray)
                            : Color.FromArgb(25, Color.Azure);
                }

                // Make the panel last from the top
                panel.BringToFront();
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    // Global key press event
    private void on_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            // Numeric keys
            case Keys.D1:
            case Keys.D2:
            case Keys.D3:
            case Keys.D4:
            case Keys.D5:
            case Keys.D6:
            case Keys.D7:
            case Keys.D8:
            case Keys.D9:
            case Keys.D0: {

                    if (e.Modifiers == Keys.Alt)
                    {
                        activatePanel(e.KeyValue - 49, useLatest: true);
                        e.SuppressKeyPress = true;      // No system beep/sound on pressing Alt key
                        e.Handled = true;
                    }
                }
                break;

            case Keys.F2: {

                    // In case DataGrid has lost its focus, we still want it to be able to enter edit mode
                    if (!_myTDGManager.getGrid().Obj().Focused)
                    {
                        e.Handled = _myTDGManager.processExternalKeyDown(e.KeyCode);
                    }
                }
                break;

            case Keys.F5: {

                    _myTDGManager.refresh();
                    e.Handled = true;
                }
                break;

            case Keys.Tab: {

                    // No system beep/sound on pressing Tab key in TreeView (which must move focus to the Grid silently)
                    // We're suposed to reach this point after calling to 'tree_grid_onPreviewKeyDown(...)'
                    e.SuppressKeyPress = true;
                }
                break;
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------

    private void activatePanel(int panel, bool useLatest)
    {
        CheckBox getCheckBox(int n)
        {
            CheckBox cb = null;
            SortedDictionary<int, CheckBox> dic = new SortedDictionary<int, CheckBox>();

            foreach (var item in _controls.optionList)
            {
                myPanel p = item.Parent as myPanel;

                if (p != null)
                {
                    dic.Add(p.Top, item);
                }
            }

            // Adjust for D0 key, which is less than D1
            n += n < 0 ? 10 : 0;
            int i = 0;

            foreach (var item in dic)
            {
                if (i++ == n)
                {
                    cb = item.Value;
                    break;
                }
            }

            return cb;
        }

        // -----------------------------------------------

        CheckBox cb = getCheckBox(panel);

        if (cb != null)
        {
            cb.Checked = !cb.Checked;

            if (useLatest && cb.Checked)
            {
                myPanel p = cb.Parent as myPanel;

                if (p != null)
                {
                    for (int i = 0; i < p.Controls.Count; i++)
                    {
                        if (p.Controls[i] is Button && p.Controls[i].Text == "Use Latest")
                        {
                            var btn = p.Controls[i] as Button;
                            btn.PerformClick();
                            break;
                        }
                    }
                }
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------------------------------
};
