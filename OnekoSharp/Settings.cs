using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace OnekoSharp
{
    internal class Settings : Form
    {
        readonly private static List<int> _speedSettings = new List<int>() { 4, 8, 12, 16, 24, 32, 48, 64, 72, 96, 128, 192 };
        private bool _settingsUpdated = false;
        public Settings(Oneko oneko) {
            Text = "Oneko Settings";
            Icon = Properties.Resources.onekoIcon;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Size = new Size(256, 256);
            GroupBox onekoGroup = new GroupBox()
            {
                Text = "Oneko",
                Size = new Size(256 - 16 - 8, 256 - 31 - 16 - 4 - 24),
                Location = new Point(4, 4)
            };

            TrackBar onekoSize = new TrackBar() { Width = 128, LargeChange = 1, Minimum = 1, Maximum = 8 };
            TrackBar onekoSpeed = new TrackBar() { Width = 128, LargeChange = 1, Maximum = 11 };
            CheckBox skAlt = new CheckBox() { Text = "Alt", Tag = OnekoSharp.ModifierKeys.Alt, AutoSize = true, };
            CheckBox skCtrl = new CheckBox() { Text = "Ctrl", Tag = OnekoSharp.ModifierKeys.Control, AutoSize = true, };
            CheckBox skShift = new CheckBox() { Text = "Shift", Tag = OnekoSharp.ModifierKeys.Shift, AutoSize = true, };
            CheckBox skWin = new CheckBox() { Text = "Win", Tag = OnekoSharp.ModifierKeys.Win, AutoSize = true, };
            ComboBox skKey = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList};
            foreach (var key in Enum.GetValues(typeof(Keys)))
            {
                skKey.Items.Add(key);
            }
            onekoGroup.Controls.AddRange(new Control[] { 
                new Label() { Text = "Oneko Size" },
                onekoSize,
                new Label() { Text = "Oneko Speed" },
                onekoSpeed,
                new Label() { Text = "Keyboard shortcut to put Oneko in a box", AutoSize=true },
                skAlt,
                skCtrl,
            });
            StackLayout(onekoGroup.Controls, new Point(8,16));
            skShift.Location = new Point(skAlt.Right + 2, skAlt.Top);
            skWin.Location = new Point(skCtrl.Right + 2, skCtrl.Top);
            Label sizeIndicator = new Label() { Location = new Point(onekoSize.Right + 2,onekoSize.Top + 4), Text = "32px" };
            Label speedIndicator = new Label() { Location = new Point(onekoSpeed.Right + 2, onekoSpeed.Top + 4), Text = "80px/s" };
            onekoGroup.Controls.AddRange(new Control[] { sizeIndicator, speedIndicator, skShift, skWin, skKey });
            skKey.Location = new Point(skShift.Right + 2, skAlt.Top);
            onekoSize.ValueChanged += (s, e) =>
            {
                _settingsUpdated = true;
                Config.Instance.OnekoSize = onekoSize.Value * 32;
                sizeIndicator.Text = onekoSize.Value * 32 + "px";
            };
            onekoSpeed.ValueChanged += (s, e) =>
            {
                _settingsUpdated = true;
                Config.Instance.OnekoSpeed = _speedSettings[onekoSpeed.Value];
                speedIndicator.Text = Math.Round(Config.Instance.OnekoSpeed*(1000/150f)) + "px/s";
            };
            skAlt.CheckedChanged += ToggleModifierKey;
            skCtrl.CheckedChanged += ToggleModifierKey;
            skShift.CheckedChanged += ToggleModifierKey;
            skWin.CheckedChanged += ToggleModifierKey;
            skKey.SelectedValueChanged += (s, e) =>
            {
                _settingsUpdated = true;
                Config.Instance.ToggleBoxShortkeyKey = (Keys)skKey.SelectedItem;
            };
            onekoSize.Value = Config.Instance.OnekoSize / 32;
            onekoSpeed.Value = _speedSettings.FindLastIndex(x=>x<=Config.Instance.OnekoSpeed);
            skAlt.Checked = Config.Instance.ToggleBoxShortkeyModifier.HasFlag(OnekoSharp.ModifierKeys.Alt);
            skCtrl.Checked = Config.Instance.ToggleBoxShortkeyModifier.HasFlag(OnekoSharp.ModifierKeys.Control);
            skShift.Checked = Config.Instance.ToggleBoxShortkeyModifier.HasFlag(OnekoSharp.ModifierKeys.Shift);
            skWin.Checked = Config.Instance.ToggleBoxShortkeyModifier.HasFlag(OnekoSharp.ModifierKeys.Win);
            skKey.SelectedItem = Config.Instance.ToggleBoxShortkeyKey;
            Controls.Add(onekoGroup);
            Button apply = new Button() { Text = "Apply", Size = new Size(64,24), Location = new Point(256-16 - 4-64, 256 - 31 - 12 - 24) };
            apply.Click += (s, e) => {
                oneko.OnekoSize = Config.Instance.OnekoSize;
                oneko.OnekoSpeed = Config.Instance.OnekoSpeed;
                oneko.hook.UnregisterLastHotKey();
                oneko.hook.RegisterHotKey(Config.Instance.ToggleBoxShortkeyModifier, Config.Instance.ToggleBoxShortkeyKey);
            };
            Controls.Add(apply);
            FormClosing += (s, e) => {
                if (_settingsUpdated)
                {
                    _settingsUpdated = false;
                    apply.PerformClick();
                    Config.Instance.SaveConfig();
                }
                if (Visible)
                {
                    e.Cancel = true;
                    Hide();
                }
            };
            _settingsUpdated = false;
        }
        void ToggleModifierKey(object s, EventArgs e)
        {
            Debug.WriteLine(s.GetType());
            _settingsUpdated = true;
            CheckBox box = s as CheckBox;
            int key = (int)(ModifierKeys)box.Tag;
            Config.Instance.ToggleBoxShortkeyModifier = (ModifierKeys)(((int)Config.Instance.ToggleBoxShortkeyModifier | key) - key * (box.Checked ? 0 : 1));
        }
        void StackLayout(Control.ControlCollection controls, Point location)
        {
            int yOffset = location.Y;
            foreach (Control control in controls)
            {
                bool isLabel = control is Label;
                if (control is TrackBar) yOffset -= 8;
                if (control is TextBox) yOffset += 2;
                control.Location = new Point(location.X + (isLabel?0:2), yOffset);
                yOffset += control.Height;
                if (control is TrackBar) yOffset -= 8;
                control.BringToFront();
            }
        }
    }
}
