using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OnekoSharp
{
    // Code from https://stackoverflow.com/a/27309185/20959896 
    public sealed class KeyboardHook
    {
        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private class Window : NativeWindow
        {
            private static int WM_HOTKEY = 0x0312;

            public Window()
            {
                // create the handle for the window.
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                // check if we got a hot key pressed.
                if (m.Msg == WM_HOTKEY)
                {
                    // get the keys.
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                    // invoke the event to notify the parent.
                    if (KeyPressed != null)
                        KeyPressed(this, new EventArgs());
                }
            }

            public event EventHandler<EventArgs> KeyPressed;
        }

        private Window _window = new Window();
        private int _currentId;

        public KeyboardHook()
        {
            // register the event of the inner native window.
            _window.KeyPressed += delegate (object sender, EventArgs args)
            {
                if (KeyPressed != null)
                    KeyPressed(this, args);
            };
        }

        public void RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            // increment the counter.
            _currentId = _currentId + 1;

            // register the hot key.
            if (!RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key))
                MessageBox.Show("Couldn’t register the hot key.","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }

        public void UnregisterLastHotKey()
        {
            // register the hot key.
            if (UnregisterHotKey(_window.Handle, _currentId))
                _currentId = _currentId - 1;

        }

        public event EventHandler<EventArgs> KeyPressed;
    }

    [Flags]
    public enum ModifierKeys : uint
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
