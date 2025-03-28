﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OnekoSharp
{
    internal class Oneko : Form
    {
        private static Random _rand = new Random();
        public int OnekoSpeed { get; set; } = Config.Instance.OnekoSpeed;
        private int _onekoSize = Config.Instance.OnekoSize;
        public int OnekoSize { get => _onekoSize; set
            {
                _onekoSize = value;
                if (!_inabox) Size = new Size(value, value);
            }
        }
        public Point OnekoLocation { get => new Point(Location.X+_onekoOffset.X,Location.Y+_onekoOffset.Y); }
        private Point _onekoOffset;
        private System.Threading.Timer _refreshTimer;
        private OnekoState _state = OnekoState.Idle;
        private int _stateFrame = 1;
        private OnekoSprite _currentSprite = OnekoSprite.Idle;
        private bool _inabox = false;
        private Point _paintLocation = new Point(0,0);
        private NotifyIcon _trayIcon;
        private MenuItem _putInABox;
        private Settings _settings;
        private Image _onekoSprites;
        public Oneko()
        {
            SuspendLayout();
            Text = "Oneko";
            var assembly = Assembly.GetExecutingAssembly();
            Icon = Icon.ExtractAssociatedIcon(assembly.Location);
            _onekoSprites = Image.FromStream(assembly.GetManifestResourceStream("OnekoSharp.oneko.png"));
            MaximizeBox = false;
            MinimumSize = new Size(32, 32);
            TransparencyKey = Color.FromArgb(16711935);
            SetWindowStyle(false);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            _settings = new Settings(this);
            var contextMenu = new ContextMenu();
            _putInABox = new MenuItem("Put Oneko in a box", (_, e) => ToggleBox());
            contextMenu.MenuItems.Add(_putInABox);
            contextMenu.MenuItems.Add(new MenuItem("Settings", (_, e) => _settings.Show()));
            contextMenu.MenuItems.Add(new MenuItem("-"));
            contextMenu.MenuItems.Add(new MenuItem("Kill Oneko", (_, e) => Application.ExitThread()));
            _trayIcon = new NotifyIcon() { Text = "Oneko", Icon = Icon, Visible = true, ContextMenu=contextMenu};
            _refreshTimer = new System.Threading.Timer(OnRefresh, null, 150, 150);
            ResumeLayout(true);
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (_inabox)
            {
                ToggleBox();
                e.Cancel = true;
            }
            base.OnClosing(e);
        }
        private unsafe void OnRefresh(object stateInfo)
        {
            Invoke((MethodInvoker)(() =>
            {
                if (!IsDisposed)
                {
                    OnekoSprite sprite = Animate();
                    if (sprite != _currentSprite)
                    {
                        _currentSprite = sprite;
                        Invalidate();
                    }
                }
            }));
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            if (!_inabox) g.Clear(Color.Fuchsia);
            Utils.DrawOneko(g, _onekoSprites, (int)_currentSprite, new Point(_onekoOffset.X,_onekoOffset.Y), OnekoSize);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_LAYERED = 0x80000;
                const int WS_EX_TRANSPARENT = 0x20;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                const int WS_EX_NOACTIVATE = 0x8000000;
                if (_inabox) return base.CreateParams;
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED;
                cp.ExStyle |= WS_EX_TRANSPARENT;
                cp.ExStyle |= WS_EX_TOOLWINDOW;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                return cp;
            }
        }
        private static int WM_HOTKEY = 0x0312;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY)
            {
                ToggleBox();
            }
        }
        public void RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            Win32.RegisterHotKey(Handle, 0, modifier, (uint)key);
        }

        public void UnregisterLastHotKey()
        {
            Win32.UnregisterHotKey(Handle, 0);
        }
        protected override bool ShowWithoutActivation => true;
        private OnekoSprite Animate()
        {
            Point onekoCenter = OnekoLocation;
            onekoCenter.Offset(OnekoSize / 2, OnekoSize / 2);

            Point target = new Point(MousePosition.X - _paintLocation.X, MousePosition.Y - _paintLocation.Y);

            int distance = Utils.DistanceBetween(onekoCenter,target);

            if (distance > OnekoSpeed * 3)
            {
                switch (_state)
                {
                    case OnekoState.Sleep:
                        _state = OnekoState.WakingUp;
                        return OnekoSprite.Wakeup;
                    case OnekoState.WakingUp:
                        if (_rand.Next(3) < 2)
                        {
                            _stateFrame = (_rand.Next(2)+2) * 6 - 1;
                            _state = OnekoState.Scratch;
                        }
                        else
                        {
                            _stateFrame = 0;
                            _state = OnekoState.Surprised;
                        }
                        return OnekoSprite.Wakeup;
                    case OnekoState.Scratch:
                        return Scratch(OnekoState.Sleep);
                    case OnekoState.Idle:
                        _state = OnekoState.Active;
                        return OnekoSprite.Idle;
                    case OnekoState.Surprised:
                        if (_stateFrame >= 2) _state = OnekoState.Active;
                        _stateFrame++;
                        return OnekoSprite.Surprised;
                    case OnekoState.Active:
                        int deadzone = OnekoSpeed * 2;
                        int xDistance = target.X - onekoCenter.X;
                        int yDistance = target.Y - onekoCenter.Y;

                        AnchorStyles walkDir = AnchorStyles.None;

                        if (xDistance > deadzone) walkDir |= AnchorStyles.Right;
                        else if (xDistance < -deadzone) walkDir |= AnchorStyles.Left;

                        if (yDistance > deadzone) walkDir |= AnchorStyles.Bottom;
                        else if (yDistance < -deadzone) walkDir |= AnchorStyles.Top;

                        return Walk(walkDir);
                    default:
                        _stateFrame = 0;
                        _state = OnekoState.Surprised;
                        return OnekoSprite.Surprised;
                }
            }

            switch (_state)
            {
                case OnekoState.Idle:
                    if (_stateFrame >= 48)
                    {
                        _stateFrame = 0;
                        _state = OnekoState.Yawn;
                    }
                    _stateFrame++;
                    return OnekoSprite.Idle;
                case OnekoState.Yawn:
                    if (_stateFrame >= 5)
                        if (_rand.Next(3) < 2) {
                            _stateFrame = _rand.Next(2)*6-1;
                            _state = OnekoState.Scratch;
                        } else
                        {
                            _stateFrame = _rand.Next(16);
                            _state = OnekoState.Idle;
                        }
                    _stateFrame++;
                    return OnekoSprite.Yawn;
                case OnekoState.Scratch:
                    return Scratch(OnekoState.Sleep);
                case OnekoState.Sleep:
                    _stateFrame++;
                    return OnekoSprite.Sl + ((_stateFrame/6) % 2);
                default:
                    _stateFrame = _rand.Next(16);
                    _state = OnekoState.Idle;
                    return OnekoSprite.Idle;

            }
        }
        private OnekoSprite Scratch(OnekoState continueWith)
        {
            _stateFrame++;
            if (_stateFrame >= 16)
            {
                if (_rand.Next(3) < 2)
                {
                    _stateFrame = 0;
                    _state = continueWith;
                }
                else
                {
                    _stateFrame = _rand.Next(16);
                    _state = OnekoState.Idle;
                }
            }
            return OnekoSprite.S + (_stateFrame / 2 % 2);
        }
        private void MoveOneko(int x=0, int y=0)
        {
            if (_inabox)
            {
                _onekoOffset = new Point(Math.Min(ClientSize.Width-OnekoSize, Math.Max(0, _onekoOffset.X + x)), Math.Min(ClientSize.Height-OnekoSize, Math.Max(0, _onekoOffset.Y + y)));
            }
            else
            {
                Location = new Point(Location.X + x, Location.Y + y);
            }
        }
        private OnekoSprite Walk(AnchorStyles dir)
        {
            Point oldLoc = OnekoLocation;
            if (Utils.HasFlag(dir,AnchorStyles.Top)) MoveOneko(y:-OnekoSpeed);
            else if (Utils.HasFlag(dir, AnchorStyles.Bottom)) MoveOneko(y:OnekoSpeed);
            if (Utils.HasFlag(dir, AnchorStyles.Right)) MoveOneko(OnekoSpeed);
            else if (Utils.HasFlag(dir, AnchorStyles.Left)) MoveOneko(-OnekoSpeed);
            Point newLoc = OnekoLocation;
            _stateFrame++;

            AnchorStyles moveDir = AnchorStyles.None;
            if (oldLoc.X < newLoc.X) moveDir |= AnchorStyles.Right;
            else if (oldLoc.X > newLoc.X) moveDir |= AnchorStyles.Left;
            if (oldLoc.Y < newLoc.Y) moveDir |= AnchorStyles.Bottom;
            else if (oldLoc.Y > newLoc.Y) moveDir |= AnchorStyles.Top;

            switch (moveDir)
            {
                case AnchorStyles.Top:
                    return OnekoSprite.WU+ _stateFrame%2;
                case AnchorStyles.Top|AnchorStyles.Right:
                    return OnekoSprite.WUR+ _stateFrame%2;
                case AnchorStyles.Right:
                    return OnekoSprite.WR+ _stateFrame % 2;
                case AnchorStyles.Bottom|AnchorStyles.Right:
                    return OnekoSprite.WDR+ _stateFrame % 2;
                case AnchorStyles.Bottom:
                    return OnekoSprite.WD+ _stateFrame % 2;
                case AnchorStyles.Bottom | AnchorStyles.Left:
                    return OnekoSprite.WDL+ _stateFrame % 2;
                case AnchorStyles.Left:
                    return OnekoSprite.WL+ _stateFrame % 2;
                case AnchorStyles.Top | AnchorStyles.Left:
                    return OnekoSprite.WUL+ _stateFrame % 2;
                default:
                    if (Utils.HasFlag(dir, AnchorStyles.Right)) return OnekoSprite.SR + (_stateFrame/2)%2;
                    else if (Utils.HasFlag(dir, AnchorStyles.Left)) return OnekoSprite.SL + (_stateFrame / 2)%2;
                    else if (Utils.HasFlag(dir, AnchorStyles.Top)) return OnekoSprite.SU + (_stateFrame / 2) % 2;
                    else if (Utils.HasFlag(dir, AnchorStyles.Bottom)) return OnekoSprite.SD + (_stateFrame / 2) % 2;
                    return OnekoSprite.Idle;
            }
        }
        private void ToggleBox()
        {
            if (_inabox)
            {
                _inabox = false;
                Activate();
                SetWindowStyle(false);
                Location = new Point(Location.X + _onekoOffset.X + _paintLocation.X, Location.Y + _onekoOffset.Y + _paintLocation.Y);
                _onekoOffset = new Point(0, 0);
                _paintLocation = new Point(0, 0);
                _putInABox.Text = "Put Oneko in a box";
            }
            else
            {
                _inabox = true;
                SetWindowStyle(true);
                Point screenRectangle = PointToScreen(ClientRectangle.Location);
                _paintLocation = new Point(screenRectangle.X - Location.X, screenRectangle.Y - Location.Y);
                Location = new Point(Location.X- (int)(OnekoSize * 3.5), Location.Y - (int)(OnekoSize * 2.5));
                _onekoOffset = new Point((int)(OnekoSize * 3.5)-_paintLocation.X, (int)(OnekoSize * 2.5)-_paintLocation.Y);
                _putInABox.Text = "Take Oneko out of the box";
            }
        }
        private void SetWindowStyle(bool box = false)
        {

            FormBorderStyle = box ? FormBorderStyle.Sizable : FormBorderStyle.None;
            ShowInTaskbar = box;
            Size = box ? new Size(OnekoSize * 8, OnekoSize * 6) : new Size(OnekoSize, OnekoSize);
            TopMost = !box;
            RegisterHotKey(Config.Instance.ToggleBoxShortkeyModifier, Config.Instance.ToggleBoxShortkeyKey);
        }
    }
}
