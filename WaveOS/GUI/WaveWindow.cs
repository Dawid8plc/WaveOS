﻿using Cosmos.System;
using System;
using System.Collections.Generic;
using WaveOS.Apps;
using WaveOS.Graphics;
using WaveOS.Managers;
using static WaveOS.Graphics.Canvas;
using Mouse = Cosmos.System.MouseManager;

namespace WaveOS.GUI
{
    public enum WindowState
    {
        Normal,
        Minimized,
        Maximized
    }

    [Flags]
    public enum WindowResizeState
    {
        None = 0,
        Top = 1,
        Left = 2,
        Right = 4,
        Bottom = 8
    }
    public class WaveWindow
    {
        public int windID;
        public string title;
        public bool Active = false;
        public int X, Y, SavedX, SavedY, IX, IY;
        WindowState _state = WindowState.Normal;
        public WindowState savedState = WindowState.Normal;

        public bool StayOnTop = false;

        public WindowState State { get { return _state; } set { 
                if(value == WindowState.Maximized)
                {
                    if (_state == WindowState.Normal)
                    {
                        SavedX = X; SavedY = Y;

                        X = 0; Y = 0;

                        SavedWidth = Width; SavedHeight = Height;

                        Width = Canv.Width; Height = (Canv.Height - titleBarHeight - Host.restrictedTaskbarSize);

                        CloseButton.Y = 2;
                        MaximizeButton.Y = 2;
                        MinimizeButton.Y = 2;
                    }
                }else if(value == WindowState.Normal)
                {
                    if (_state == WindowState.Maximized)
                    {
                        X = SavedX; Y = SavedY;

                        Width = SavedWidth; Height = SavedHeight;

                        CloseButton.Y = 5;
                        MaximizeButton.Y = 5;
                        MinimizeButton.Y = 5;
                    }
                }else if(value == WindowState.Minimized)
                {
                    savedState = _state;
                }
                _state = value;
            } }

        WindowResizeState ResizeState = WindowResizeState.None;

        public int Width, Height, SavedWidth, SavedHeight;
        public int MinWidth = 100, MinHeight = 100;

        public int titleBarHeight { get { if (!borderless) return 21; else return 0; } }

        public int resizeBorder = 10;
        public int borderSize = 3;

        public bool borderless = false;
        public bool controlbox = true;

        public bool Moving;
        public Canvas Canv { get { return WaveShell.Canv; } }

        public List<WaveElement> children = new List<WaveElement>();

        public WindowManager Host;

        WaveButton CloseButton;
        WaveButton MaximizeButton;
        WaveButton MinimizeButton;
        List<Color> titleBarGradients;
        List<Color> inactiveBarGradients;
        public WaveWindow(string title, int x, int y, int width, int height, WindowManager host)
        {
            this.title = title;
            this.X = x; this.Y = y;
            Width = width; Height = height;

            Host = host;

            CloseButton = new WaveButton() { Text = "X", parent = this, X = 21, Y = 5, Width = 16, Height = 14, ignoreTitleBar = true, Color = Color.Black,
                onClick = () => { Close(); }, 
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };

            MaximizeButton = new WaveButton() { Text = "□", parent = this, X = CloseButton.X + 18, Y = 5, Width = 16, Height = 14, ignoreTitleBar = true, Color = Color.Black,
                onClick = () => { if (State == WindowState.Normal) State = WindowState.Maximized; else State = WindowState.Normal; },
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };

            MinimizeButton = new WaveButton() { Text = "_", parent = this, X = MaximizeButton.X + 18, Y = 5, Width = 16, Height = 14, ignoreTitleBar = true, Color = Color.Black,
                onClick = () => { State = WindowState.Minimized; SetInActive(); },
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };

            titleBarGradients = GetGradients(Color.DeepBlue, Color.Red, 50);
            inactiveBarGradients = GetGradients(Color.LightGray, Color.DeepGray, 50);
        }

        public static List<Color> GetGradients(Color start, Color end, int steps)
        {
            List<Color> list = new List<Color>();

            int stepA = ((end.A - start.A) / (steps - 1));
            int stepR = ((end.R - start.R) / (steps - 1));
            int stepG = ((end.G - start.G) / (steps - 1));
            int stepB = ((end.B - start.B) / (steps - 1));

            for (int i = 0; i < steps; i++)
            {
                list.Add( new Color((byte)(start.A + (stepA * i)),
                                            (byte)(start.R + (stepR * i)),
                                            (byte)(start.G + (stepG * i)),
                                            (byte)(start.B + (stepB * i))) );
            }

            return list;
        }

        public virtual void Draw()
        {
            if (State == WindowState.Minimized) return;

            //Draw window
            if (State == WindowState.Normal)
            {
                if (!borderless)
                {
                    Canv.Draw3DBorder(X, Y, Width, Height + titleBarHeight);

                    ////Draw 3D border
                    ////Top and left white lines
                    //Canv.DrawFilledRectangle(X, Y, Width - 1, 1, 0, Color.White);
                    //Canv.DrawFilledRectangle(X, Y, 1, (Height + titleBarHeight) - 1, 0, Color.White);

                    ////Inner shadow Top
                    //Canv.DrawFilledRectangle(X + 1, Y + 1, Width - 1, 1, 0, new Color(223, 223, 223));
                    //Canv.DrawFilledRectangle(X + 1, Y + 1, 1, (Height + titleBarHeight) - 1, 0, new Color(223, 223, 223));

                    ////Inner shadow Bottom
                    //Canv.DrawFilledRectangle(X + 1, (Y + Height + titleBarHeight) - 2, Width - 1, 1, 0, new Color(127, 127, 127));
                    //Canv.DrawFilledRectangle((X + Width) - 2, Y + 1, 1, (Height + titleBarHeight) - 1, 0, new Color(127, 127, 127));

                    ////Bottom and right black lines
                    //Canv.DrawFilledRectangle(X, (Y + Height + titleBarHeight) - 1, Width, 1, 0, Color.Black);
                    //Canv.DrawFilledRectangle((X + Width) - 1, Y, 1, Height + titleBarHeight, 0, Color.Black);
                }

                //Window bg
                if(!borderless)
                    Canv.DrawFilledRectangle(X + 2, Y + 2, Width - 4, (Height + titleBarHeight) - 4, 0, new Color(191, 191, 191));
                else
                    Canv.DrawFilledRectangle(X, Y, Width, Height, 0, new Color(191, 191, 191));

                if (!borderless)
                {
                    //Titlebar
                    //Gradient
                    for (int i = 0; i < Width - 6; i++)
                    {
                        float e = (float)i / (Width - 6);
                        e = e * titleBarGradients.Count;

                        Canv.DrawLine((X + 3) + i, Y + 3, (X + 3) + i, (Y + 3) + 18, Active ? titleBarGradients[(int)e] : inactiveBarGradients[(int)e]);
                    }
                }
            }
            else if(State == WindowState.Maximized)
            {
                //Window bg
                Canv.DrawFilledRectangle(X, Y + 2, Width, (Height + titleBarHeight) - 2, 0, new Color(191, 191, 191));

                if (!borderless)
                {
                    //Titlebar
                    //Gradient
                    for (int i = 0; i < Width; i++)
                    {
                        float e = (float)i / (Width);
                        e = e * titleBarGradients.Count;

                        Canv.DrawLine(X + i, Y, X + i, Y + 18, Active ? titleBarGradients[(int)e] : inactiveBarGradients[(int)e]);
                    }
                }
            }

            if(State == WindowState.Normal && !borderless)
            {
                Canv.DrawString(X + 5, Y + 4, MinimizeButton.relativeX - (X + 5), Font.Default.Height, title, Color.White);
            }
            else if(State == WindowState.Maximized && !borderless)
            {
                Canv.DrawString(X + 5, Y + 2, title, Color.White);
            }

            if (controlbox)
            {
                CloseButton.Draw();
                MaximizeButton.Draw();
                MinimizeButton.Draw();
            }

            //Draw children
            foreach (var item in children)
            {
                item.Draw();
            }
        }

        public virtual void Update()
        {
            bool activeHit = false;

            if (!WaveInput.MouseHit && WaveInput.WasLMBPressed() && WaveInput.IsMouseWithin(X, Y, Width, Height + titleBarHeight))
            {
                activeHit = true;
                if (!Active)
                    SetActive();
            }

            if (controlbox)
            {
                UpdateElement(CloseButton);
                UpdateElement(MaximizeButton);
                UpdateElement(MinimizeButton);
            }

            //Resizing window
            if (!borderless && !WaveInput.MouseHit && State != WindowState.Maximized && WaveInput.WasLMBPressed() && !Moving && ResizeState == WindowResizeState.None)
            {
                if(WaveInput.IsMouseWithin(X, Y, Width, resizeBorder))
                {
                    WaveInput.MouseHit = true;
                    ResizeState |= WindowResizeState.Top;
                    IY = (int)Mouse.Y;
                }

                if(WaveInput.IsMouseWithin(X, (Y + Height + titleBarHeight) - resizeBorder, Width, resizeBorder))
                {
                    WaveInput.MouseHit = true;
                    ResizeState |= WindowResizeState.Bottom;
                    IY = (int)Mouse.Y;
                }

                if(WaveInput.IsMouseWithin(X, Y, resizeBorder, Height + titleBarHeight))
                {
                    WaveInput.MouseHit = true;
                    ResizeState |= WindowResizeState.Left;
                    IX = (int)Mouse.X;
                }

                if(WaveInput.IsMouseWithin((X + Width) - resizeBorder, Y, resizeBorder, Height + titleBarHeight))
                {
                    WaveInput.MouseHit = true;
                    ResizeState |= WindowResizeState.Right;
                    IX = (int)Mouse.X;
                }
            }

            if (WaveInput.mState == MouseState.Left && ResizeState != WindowResizeState.None)
            {

                if (WaveInput.mState == MouseState.Left && EnumHelper.IsResizeSet(ResizeState, WindowResizeState.Top))
                {
                    if (Height - ((int)Mouse.Y - IY) > MinHeight)
                    {
                        Height -= (int)Mouse.Y - IY;
                        IY = (int)Mouse.Y;
                        Y = (int)Mouse.Y;
                    }
                }
                else if (EnumHelper.IsResizeSet(ResizeState, WindowResizeState.Bottom))
                {
                    if (Height + ((int)Mouse.Y - IY) > MinHeight)
                    {
                        Height += (int)Mouse.Y - IY;
                        IY = (int)Mouse.Y;
                    }
                }

                if (EnumHelper.IsResizeSet(ResizeState, WindowResizeState.Left))
                {
                    if (Width - ((int)Mouse.X - IX) > MinWidth)
                    {
                        Width -= (int)Mouse.X - IX;
                        IX = (int)Mouse.X;
                        X = (int)Mouse.X;
                    }
                }
                else if (EnumHelper.IsResizeSet(ResizeState, WindowResizeState.Right))
                {
                    if (Width + ((int)Mouse.X - IX) > MinWidth)
                    {
                        Width += (int)Mouse.X - IX;
                        IX = (int)Mouse.X;
                    }
                }
            }
            else
            {
                ResizeState = WindowResizeState.None;
            }

            //Moving window
            if (!borderless && State != WindowState.Maximized && !WaveInput.MouseHit && WaveInput.WasLMBPressed()
                && !Moving && ResizeState == WindowResizeState.None
                && WaveInput.IsMouseWithin(X, Y, Width, titleBarHeight) 
                && !WaveInput.IsMouseWithin(CloseButton.relativeX, CloseButton.relativeY, CloseButton.Width, CloseButton.Height)
                && !WaveInput.IsMouseWithin(MaximizeButton.relativeX, MaximizeButton.relativeY, MaximizeButton.Width, MaximizeButton.Height)
                && !WaveInput.IsMouseWithin(MinimizeButton.relativeX, MinimizeButton.relativeY, MinimizeButton.Width, MinimizeButton.Height))
            {
                Moving = true;
                IX = (int)Mouse.X - X;
                IY = (int)Mouse.Y - Y;
                WaveInput.MouseHit = true;
            }

            if(Mouse.MouseState == Cosmos.System.MouseState.Left && Moving)
            {
                X = (int)Mouse.X - IX;
                Y = (int)Mouse.Y - IY;
            }
            else
            {
                Moving = false;
            }

            foreach (var item in children)
            {
                UpdateElement(item);
            }

            if (activeHit)
                WaveInput.MouseHit = true;
        }

        public void UpdateElement(WaveElement item)
        {
            item.Hovering = WaveInput.IsMouseWithin(item.relativeX, item.relativeY, item.Width, item.Height);

            if (item.Clicked)
            {
                if (WaveInput.mState != MouseState.Left)
                {
                    if (item.Hovering)
                    {
                        item.onClick?.Invoke();
                    }

                    item.Clicked = false;
                }
            }
            else
            {
                if(!WaveInput.MouseHit)
                item.Clicked = item.Hovering && WaveInput.WasLMBPressed();

                if (item.Clicked && item.HitTest)
                    WaveInput.MouseHit = true;
            }

            item.Update();
        }

        public void Close()
        {
            Host.CloseWindow(this);
        }

        public void SetActive()
        {
            Host.SetActiveWindow(this);
        }

        public void SetInActive()
        {
            Host.SetInActiveWindow(this);
        }
    }
}
