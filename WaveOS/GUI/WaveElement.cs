using Cosmos.System;
using System;
using System.Collections.Generic;
using WaveOS.Apps;
using WaveOS.Graphics;
using WaveOS.Managers;
using Mouse = Cosmos.System.MouseManager;

namespace WaveOS.GUI
{
    [Flags]
    public enum AnchorStyles
    {
        Bottom = 2,
        Left = 4,
        None = 0,
        Right = 8,
        Top = 1
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    public class WaveElement
    {
        public WaveWindow parent;
        public WaveElement parent2;
        public AnchorStyles Anchor = AnchorStyles.Left | AnchorStyles.Top;

        public WaveContextMenu contextMenu;

        public bool ignoreTitleBar = false;

        public int X = 0, Y = 0, Width = 0, Height = 0;
        public object tag;
        public int relativeX { get 
            {
                if (EnumHelper.IsAnchorSet(Anchor, AnchorStyles.Left))
                    if (parent2 == null) //3 is the border size, make border size customizable in the future?
                        return (parent.X + X) + 3;
                    else
                        return parent2.relativeX + X;
                else
                    if(parent2 == null)
                        return (parent.X + parent.Width) - X;
                    else
                        return (parent2.relativeX + parent2.Width) - X;
            }
        }
        public int relativeY { get 
            {
                if (EnumHelper.IsAnchorSet(Anchor, AnchorStyles.Top))
                    if(parent2 == null)
                        return (!ignoreTitleBar) ? (parent.Y + ((!parent.borderless) ? parent.titleBarHeight : 0)) + Y : parent.Y + Y;
                    else
                        return (parent2.relativeY) + Y;
                else
                    if(parent2 == null)
                        return (parent.Y + parent.Height) - Y;
                    else
                        return (parent2.relativeY + parent2.Height) - Y;
            }
        }

        public bool IsFlagSet<T>(T value, T flag) where T : struct
        {
            long lValue = Convert.ToInt64(value);
            long lFlag = Convert.ToInt64(flag);
            return (lValue & lFlag) != 0;
        }

        public bool Clicked, Hovering;
        public bool HitTest = true;
        public Canvas Canv { get { return WaveShell.Canv; } }

        public Action onClick;

        public virtual void Update()
        {
            //foreach (var item in children)
            //{
            //    item.Update();
            //} 
        }
        public virtual void Draw() 
        {
            //foreach (var item in children)
            //{
            //    item.Draw();
            //}
        }
    }

    public class WaveContextMenu : WaveStackPanel
    {
        public WaveContextMenu() { }
        public WaveContextMenu(List<StartMenuItem> items)
        {
            foreach (var item in items)
            {
                item.parent2 = this;
                children.Add(item);
            }

            Height = Height = (children.Count * 20) + 6;
            UpdateView();
        }


    }

    public class WavePanel : WaveElement
    {
        public List<WaveElement> children = new List<WaveElement>();
        public bool DrawBorder = true;

        public WavePanel()
        {
            HitTest = false;
        }
        public override void Update()
        {
            foreach (var item in children)
            {
                UpdateElement(item);
            }
        }
        public override void Draw()
        {
            Canv.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, new Color(191, 191, 191));

            if (DrawBorder)
            {
                //Draw 3D border
                //Top and left white lines
                Canv.DrawFilledRectangle(relativeX, relativeY, Width - 1, 1, 0, Color.White);
                Canv.DrawFilledRectangle(relativeX, relativeY, 1, (Height) - 1, 0, Color.White);

                //Inner shadow Top
                Canv.DrawFilledRectangle(relativeX + 1, relativeY + 1, Width - 1, 1, 0, new Color(223, 223, 223));
                Canv.DrawFilledRectangle(relativeX + 1, relativeY + 1, 1, (Height) - 1, 0, new Color(223, 223, 223));

                //Inner shadow Bottom
                Canv.DrawFilledRectangle(relativeX + 1, (relativeY + Height) - 2, Width - 1, 1, 0, new Color(127, 127, 127));
                Canv.DrawFilledRectangle((relativeX + Width) - 2, relativeY + 1, 1, (Height) - 1, 0, new Color(127, 127, 127));

                //Bottom and right black lines
                Canv.DrawFilledRectangle(relativeX, (relativeY + Height) - 1, Width, 1, 0, Color.Black);
                Canv.DrawFilledRectangle((relativeX + Width) - 1, relativeY, 1, Height, 0, Color.Black);
            }

            foreach (var item in children)
            {
                item.Draw();
            }
        }

        public void UpdateElement(WaveElement item)
        {
            item.Hovering = IsMouseWithin(item.relativeX, item.relativeY, item.Width, item.Height);
            //item.Clicked = item.Hovering && Mouse.MouseState == MouseState.Left;

            if (item.Clicked)
            {
                if (WaveInput.mState != MouseState.Left)
                {
                    //Trigger mouse click on element
                    if (item.Hovering)
                    {
                        item.onClick?.Invoke();
                    }

                    item.Clicked = false;
                }
                //item.clicked = Mouse.MouseState == MouseState.Left
            }
            else
            {
                if (!WaveInput.MouseHit)
                    item.Clicked = item.Hovering && WaveInput.WasLMBPressed();

                if (item.Clicked && item.HitTest)
                    WaveInput.MouseHit = true;
            }

            item.Update();
        }

        public static bool IsMouseWithin(int X, int Y, int Width, int Height)
        {
            return Mouse.X >= X && Mouse.Y >= Y && Mouse.X <= X + Width && Mouse.Y <= Y + Height;
        }
    }

    public class WaveStackPanel : WavePanel
    {
        public void UpdateView()
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (i > 0)
                    children[i].Y = children[i - 1].Y + children[i - 1].Height;
                else
                    children[i].Y = 0;
            }
        }
    }


    public class WaveLabel : WaveElement
    {
        public string Text = "";
        public Color Color = Color.White;

        public override void Draw()
        {
            if(parent2 == null)
                Canv.DrawString(relativeX, relativeY, parent, Text, Color);
            //else
                //Canv.DrawString(relativeX, relativeY, parent2, Text, Color);
        }
    }

    public class WaveButton : WaveElement
    {
        public string Text = "";
        public Color Color = Color.White;
        public TextAlignment TextAlignment = TextAlignment.Center;
        public bool forcePressed = false;

        public override void Draw()
        {
            if (forcePressed)
            {
                //Top and left black lines
                Canv.DrawFilledRectangle(relativeX, relativeY, Width - 1, 1, 0, Color.Black);
                Canv.DrawFilledRectangle(relativeX, relativeY, 1, Height - 1, 0, Color.Black);

                //Inner shadow Top
                Canv.DrawFilledRectangle(relativeX + 1, relativeY + 1, Width - 1, 1, 0, new Color(127, 127, 127));
                Canv.DrawFilledRectangle(relativeX + 1, relativeY + 1, 1, Height - 1, 0, new Color(127, 127, 127));

                //Inner shadow Bottom
                Canv.DrawFilledRectangle(relativeX + 1, (relativeY + Height) - 2, Width - 1, 1, 0, new Color(223, 223, 223));
                Canv.DrawFilledRectangle((relativeX + Width) - 2, relativeY + 1, 1, Height - 1, 0, new Color(223, 223, 223));

                //Bottom and right white lines
                Canv.DrawFilledRectangle(relativeX, (relativeY + Height) - 1, Width, 1, 0, Color.White);
                Canv.DrawFilledRectangle((relativeX + Width) - 1, relativeY, 1, Height, 0, Color.White);

                Canv.DrawFilledRectangle(relativeX + 2, relativeY + 2, Width - 4, Height - 4, 0, new Color(191, 191, 191));


                //Canv.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, new Color(20, 20, 20));
            }
            else if (Hovering && !Clicked) //Hover
            {
                //Top and left white lines
                Canv.DrawFilledRectangle(relativeX, relativeY, Width - 1, 1, 0, Color.White);
                Canv.DrawFilledRectangle(relativeX, relativeY, 1, Height - 1, 0, Color.White);

                //Inner shadow Top
                Canv.DrawFilledRectangle(relativeX + 1, relativeY + 1, Width - 1, 1, 0, new Color(223, 223, 223));
                Canv.DrawFilledRectangle(relativeX + 1, relativeY + 1, 1, Height - 1, 0, new Color(223, 223, 223));

                //Inner shadow Bottom
                Canv.DrawFilledRectangle(relativeX + 1, (relativeY + Height) - 2, Width - 1, 1, 0, new Color(127, 127, 127));
                Canv.DrawFilledRectangle((relativeX + Width) - 2, relativeY + 1, 1, Height - 1, 0, new Color(127, 127, 127));

                //Bottom and right black lines
                Canv.DrawFilledRectangle(relativeX, (relativeY + Height) - 1, Width, 1, 0, Color.Black);
                Canv.DrawFilledRectangle((relativeX + Width) - 1, relativeY, 1, Height, 0, Color.Black);

                Canv.DrawFilledRectangle(relativeX + 2, relativeY + 2, Width - 4, Height - 4, 0, new Color(191, 191, 191));

                //Canv.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, new Color(54, 54, 54));
            }
            else if (!Clicked) //Normal
            {
                //Top and left white lines
                Canv.DrawFilledRectangle(relativeX, relativeY, Width - 1, 1, 0, Color.White);
                Canv.DrawFilledRectangle(relativeX, relativeY, 1, Height - 1, 0, Color.White);

                //Inner shadow Top
                Canv.DrawFilledRectangle(relativeX + 1, relativeY + 1, Width - 1, 1, 0, new Color(223, 223, 223));
                Canv.DrawFilledRectangle(relativeX + 1, relativeY + 1, 1, Height - 1, 0, new Color(223, 223, 223));

                //Inner shadow Bottom
                Canv.DrawFilledRectangle(relativeX + 1, (relativeY + Height) - 2, Width - 1, 1, 0, new Color(127, 127, 127));
                Canv.DrawFilledRectangle((relativeX + Width) - 2, relativeY + 1, 1, Height - 1, 0, new Color(127, 127, 127));

                //Bottom and right black lines
                Canv.DrawFilledRectangle(relativeX, (relativeY + Height) - 1, Width, 1, 0, Color.Black);
                Canv.DrawFilledRectangle((relativeX + Width) - 1, relativeY, 1, Height, 0, Color.Black);

                Canv.DrawFilledRectangle(relativeX + 2, relativeY + 2, Width - 4, Height - 4, 0, new Color(191, 191, 191));

                //Canv.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, new Color(36, 36, 36));
            }
            else if (Clicked && Hovering) //Clicked and Hovering
            {
                //Top and left black lines
                Canv.DrawFilledRectangle(relativeX, relativeY, Width - 1, 1, 0, Color.Black);
                Canv.DrawFilledRectangle(relativeX, relativeY, 1, Height - 1, 0, Color.Black);

                //Inner shadow Top
                Canv.DrawFilledRectangle(relativeX + 1, relativeY + 1, Width - 1, 1, 0, new Color(127, 127, 127));
                Canv.DrawFilledRectangle(relativeX + 1, relativeY + 1, 1, Height - 1, 0, new Color(127, 127, 127));

                //Inner shadow Bottom
                Canv.DrawFilledRectangle(relativeX + 1, (relativeY + Height) - 2, Width - 1, 1, 0, new Color(223, 223, 223));
                Canv.DrawFilledRectangle((relativeX + Width) - 2, relativeY + 1, 1, Height - 1, 0, new Color(223, 223, 223));

                //Bottom and right white lines
                Canv.DrawFilledRectangle(relativeX, (relativeY + Height) - 1, Width, 1, 0, Color.White);
                Canv.DrawFilledRectangle((relativeX + Width) - 1, relativeY, 1, Height, 0, Color.White);

                Canv.DrawFilledRectangle(relativeX + 2, relativeY + 2, Width - 4, Height - 4, 0, new Color(191, 191, 191));


                //Canv.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, new Color(20, 20, 20));
            }
            else if (Clicked && !Hovering)
            {
                //Top and left white lines
                Canv.DrawFilledRectangle(relativeX, relativeY, Width - 1, 1, 0, Color.White);
                Canv.DrawFilledRectangle(relativeX, relativeY, 1, Height - 1, 0, Color.White);

                //Inner shadow Top
                Canv.DrawFilledRectangle(relativeX + 1, relativeY + 1, Width - 1, 1, 0, new Color(223, 223, 223));
                Canv.DrawFilledRectangle(relativeX + 1, relativeY + 1, 1, Height - 1, 0, new Color(223, 223, 223));

                //Inner shadow Bottom
                Canv.DrawFilledRectangle(relativeX + 1, (relativeY + Height) - 2, Width - 1, 1, 0, new Color(127, 127, 127));
                Canv.DrawFilledRectangle((relativeX + Width) - 2, relativeY + 1, 1, Height - 1, 0, new Color(127, 127, 127));

                //Bottom and right black lines
                Canv.DrawFilledRectangle(relativeX, (relativeY + Height) - 1, Width, 1, 0, Color.Black);
                Canv.DrawFilledRectangle((relativeX + Width) - 1, relativeY, 1, Height, 0, Color.Black);

                Canv.DrawFilledRectangle(relativeX + 2, relativeY + 2, Width - 4, Height - 4, 0, new Color(191, 191, 191));
            }
            if(TextAlignment == TextAlignment.Left)
                Canv.DrawString(relativeX + 3, relativeY + (Height / 2) - 7, this, Text, Color);
            else if (TextAlignment == TextAlignment.Center)
                Canv.DrawString((relativeX + (Width / 2)) - ((Text.Length * 8) / 2), relativeY + (Height / 2) - 7, this, Text, Color);
            else if(TextAlignment == TextAlignment.Right) //Not implemented, uses Center setting
                Canv.DrawString((relativeX + (Width / 2)) - ((Text.Length * 8) / 2), relativeY + (Height / 2) - 7, this, Text, Color);
        }
    }

    public class StartMenuItem : WaveElement
    {
        public string Text = "";
        //public Color Color = Color.White;
        public TextAlignment TextAlignment = TextAlignment.Center;
        public bool forcePressed = false;

        WaveContextMenu itemList;

        public StartMenuItem(int Width, List<StartMenuItem> items = null)
        {
            this.Width = Width;

            if (items != null)
            {
                itemList = new WaveContextMenu(items)
                {
                    X = Width,
                    Y = 0,
                    parent = parent,
                    parent2 = this,
                    Width = 166
                };
            }

            if (items != null)
            {
                //itemList.UpdateView();
                //itemList.Height = Height = (itemList.children.Count * 20) + 6;
            }
        }

        //public void UpdateView()
        //{
        //    itemList.Height = (Items.Count * 20) + 6;

        //    if(Items.Count > 0)
        //    {
        //        itemList.Update();
        //    }
        //}

        public override void Draw()
        {
            Color Color = Color.Black;

            if (Hovering && !Clicked) //Hover
            {
                Canv.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, new Color(0, 0, 128));
                Color = Color.White;
            }
            else if (!Clicked) //Normal
            {   
                Canv.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, new Color(191, 191, 191));
            }
            else if (Clicked && Hovering) //Clicked and Hovering
            {
                Canv.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, new Color(0, 0, 128));
            }
            else if (Clicked && !Hovering)
            {
                Canv.DrawFilledRectangle(relativeX, relativeY, Width, Height, 0, new Color(0, 0, 128));
            }
            if (TextAlignment == TextAlignment.Left)
                Canv.DrawString(relativeX + 3, relativeY + (Height / 2) - 7, this, Text, Color);
            else if (TextAlignment == TextAlignment.Center)
                Canv.DrawString((relativeX + (Width / 2)) - ((Text.Length * 8) / 2), relativeY + (Height / 2) - 7, this, Text, Color);
            else if (TextAlignment == TextAlignment.Right) //Not implemented, uses Center setting
                Canv.DrawString((relativeX + (Width / 2)) - ((Text.Length * 8) / 2), relativeY + (Height / 2) - 7, this, Text, Color);

            if(itemList.children.Count > 0)
            {
                itemList.Draw();
            }
        }

        public override void Update()
        {
            if(itemList.children.Count > 0)
            {
                itemList.Update();
            }
        }
    }
}
