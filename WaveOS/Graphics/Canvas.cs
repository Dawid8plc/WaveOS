﻿using VBEDriver = Cosmos.HAL.Drivers.VBEDriver;
using Mouse = Cosmos.System.MouseManager;
using WaveOS.Formats;
using Cosmos.Core;
using System.Text;
using System.IO;
using System;
using WaveOS.GUI;
using System.Collections.Generic;

namespace WaveOS.Graphics
{
    public unsafe class Canvas
    {
        public Canvas(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
            Buffer = new int*[Width * Height];
            VBE = new((ushort)Width, (ushort)Height, 32);

            Mouse.ScreenWidth = (uint)Width;
            Mouse.ScreenHeight = (uint)Height;
            Mouse.X = (uint)Width / 2;
            Mouse.Y = (uint)Height / 2;
            Update();
        }

        public int Width, Height;
        public float AspectRatio => Height / Width;
        public int*[] Buffer;
        public VBEDriver VBE;
        private DateTime LT;
        private int Frames;
        public int FPS;

        #region Pixel

        public void SetPixel(int X, int Y, Color Color)
        {
            if (X < 0 || Y < 0 || X >= Width || Y >= Height || Color.A == 0)
            {
                return;
            }
            if (Color.A < 255)
            {
                Color = Color.AlphaBlend(Color, GetPixel(X, Y));
            }

            // Draw main pixel
            Buffer[(Width * Y) + X] = (int*)Color.ARGB;
            //MemoryOperations.Copy(Buffer[(Width * Y) + X], (int*)Color.ARGB, 1);
        }
        public Color GetPixel(int X, int Y)
        {
            if (X < 0 || Y < 0 || X >= Width || Y >= Height)
            {
                return Color.Black;
            }

            return new((int)Buffer[(Width * Y) + X]);
        }

        #endregion

        #region Line

        public void DrawLine(int X, int Y, int X2, int Y2, Color Color)
        {
            int dx = Math.Abs(X2 - X), sx = X < X2 ? 1 : -1;
            int dy = Math.Abs(Y2 - Y), sy = Y < Y2 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2;

            while (X != X2 || Y != Y2)
            {
                SetPixel(X, Y, Color);
                int e2 = err;
                if (e2 > -dx) { err -= dy; X += sx; }
                if (e2 < dy) { err += dx; Y += sy; }
            }
        }
        public void DrawAngledLine(int X, int Y, int Angle, int Radius, Color Color)
        {
            DrawLine(X, Y, (int)(X + (Math.Cos(Math.PI * Angle / 180) * Radius)), (int)(X + (Math.Sin(Math.PI * Angle / 180) * Radius)), Color);
        }
        public void DrawBezierLine(int X1, int Y1, int X2, int Y2, int X3, int Y3, Color Color, int N = 6)
        {
            // X2 and Y2 is where the curve should bend to.
            if (N > 0)
            {
                int X12 = (X1 + X2) / 2;
                int Y12 = (Y1 + Y2) / 2;
                int X23 = (X2 + X3) / 2;
                int Y23 = (Y2 + Y3) / 2;
                int X123 = (X12 + X23) / 2;
                int Y123 = (Y12 + Y23) / 2;

                DrawBezierLine(X1, Y1, X12, Y12, X123, Y123, Color, N - 1);
                DrawBezierLine(X123, Y123, X23, Y23, X3, Y3, Color, N - 1);
            }
            else
            {
                DrawLine(X1, Y1, X2, Y2, Color);
                DrawLine(X2, Y2, X3, Y3, Color);
            }
        }

        #endregion

        #region Rectangle

        public void DrawRectangle(int X, int Y, int Width, int Height, int Radius, Color Color)
        {
            if (Radius > 0)
            {
                DrawCircle(X, Y, Radius, Color, 180, 270); // Top left
                DrawCircle(X + Width, Y + Height, Radius, Color, 0, 90); // Bottom right
                DrawCircle(X, Y + Height, Radius, Color, 90, 180); // Bottom left
                DrawCircle(X + Width, Y, Radius, Color, 270, 360);
            }
            DrawLine(X + Radius, Y, X + Width - (Radius * 2), Y, Color); // Top Line
            DrawLine(X + Radius, Y + Height, X + Width - (Radius * 2), Height + Y, Color); // Bottom Line
            DrawLine(X, Y + Radius, X, Y + Height - (Radius * 2), Color); // Left Line
            DrawLine(X + Width, Y + Radius, Width + X, Y + Height - (Radius * 2), Color); // Right Line
        }
        public void DrawFilledRectangle(int X, int Y, int Width, int Height, int Radius, Color Color)
        {
            if (Radius == 0)
            {
                for (int IX = X; IX < X + Width; IX++)
                {
                    for (int IY = Y; IY < Y + Height; IY++)
                    {
                        SetPixel(IX, IY, Color);
                    }
                }
            }
            else
            {
                DrawFilledRectangle(X + Radius, Y, Width - Radius * 2, Height, 0, Color);
                DrawFilledRectangle(X, Y + Radius, Width, Height - Radius * 2, 0, Color);

                DrawFilledCircle(X + Radius, Y + Radius, Radius, Color);
                DrawFilledCircle(X + Width - Radius - 1, Y + Radius, Radius, Color);

                DrawFilledCircle(X + Radius, Y + Height - Radius - 1, Radius, Color);
                DrawFilledCircle(X + Width - Radius - 1, Y + Height - Radius - 1, Radius, Color);
            }
        }

        #endregion

        #region Circle

        public void DrawCircle(int X, int Y, int Radius, Color Color, int StartAngle = 0, int EndAngle = 360)
        {
            if (Radius == 0)
            {
                return;
            }

            for (; StartAngle < EndAngle; StartAngle++)
            {
                SetPixel(
                    X: (int)(X + (Radius * Math.Sin(Math.PI * StartAngle / 180))),
                    Y: (int)(Y + (Radius * Math.Cos(Math.PI * StartAngle / 180))),
                    Color: Color);
            }
        }
        public void DrawFilledCircle(int X, int Y, int Radius, Color Color, int StartAngle = 0, int EndAngle = 360)
        {
            if (Radius == 0)
            {
                return;
            }

            for (int I = 0; I < Radius; I++)
            {
                DrawCircle(X, Y, Radius, Color, StartAngle, EndAngle);
            }
        }

        #endregion

        #region Triangle

        public void DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, Color Color)
        {
            DrawLine(X1, Y1, X2, Y2, Color);
            DrawLine(X1, Y1, X3, Y3, Color);
            DrawLine(X2, Y2, X3, Y3, Color);
        }

        #endregion

        #region Image

        public void DrawBitmap(int X, int Y, Bitmap Bitmap)
        {
            for (int IX = 0; IX < Bitmap.Width; IX++)
            {
                for (int IY = 0; IY < Bitmap.Height; IY++)
                    SetPixel(X + IX, Y + IY, new(Bitmap.rawData[(Bitmap.Width * IY) + IX]));
            }
        }
        public void DrawBitmap(int X, int Y, int Width, int Height, Bitmap Bitmap)
        {
            DrawBitmap(X, Y, new((uint)Width, (uint)Height, (byte[])(object)Bitmap.rawData, Bitmap.Depth));
        }
        public void DrawArray(int X, int Y, int Width, int Height, int*[] Buffer)
        {
            if (Buffer.Length != Width * Height)
                return;

            for (int IX = 0; IX < Width; IX++)
            {
                for (int IY = 0; IY < Height; IY++)
                {
                    SetPixel(X + IX, Y + IY, new((int)Buffer[(Width + IY) + IX]));
                }
            }
        }

        #endregion

        #region Text

        public class Font
        {
            public Font(int Width, int Height, string Base64)
            {
                this.Width = Width;
                this.Height = Height;
                MS = new(Convert.FromBase64String(Base64));
            }
            public Font(byte[] Data)
            {
                Width = Data[0];
                Height = Data[1];
                MS = new(Data, 2, Data.Length - 2, true);
            }
            public Font(string FromFile)
            {
                byte[] File = System.IO.File.ReadAllBytes(FromFile);
                Width = File[0];
                Height = File[1];
                MS = new(File, 2, File.Length - 2, true);
            }

            public int Width;
            public int Height;
            public MemoryStream MS;
            public readonly static Font Default = new(8, 16, "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AAB4zMzM2MzGxsbMAAAAAAAA/sbGwMDAwMDAwAAAAAAAAAAA/mxsbGxsbGwAAAAAAAAA/sZgMBgwYMb+AAAAAAAAAAAAftjY2NjYcAAAAAAAAAAAZmZmZmZ8YGDAAAAAAAAAAHbcGBgYGBgYAAAAAAAAAH4YPGZmZjwYfgAAAAAAAAA4bMbG/sbGbDgAAAAAAAA4bMbGxmxsbGzuAAAAAAAAHjAYDD5mZmZmPAAAAAAAAAAAAH7b29t+AAAAAAAAAAAAAwZ+29vzfmDAAAAAAAAAHDBgYHxgYGAwHAAAAAAAAAB8xsbGxsbGxsYAAAAAAAAAAP4AAP4AAP4AAAAAAAAAAAAYGH4YGAAA/wAAAAAAAAAwGAwGDBgwAH4AAAAAAAAADBgwYDAYDAB+AAAAAAAADhsbGBgYGBgYGBgYGBgYGBgYGBgYGNjY2HAAAAAAAAAAABgYAH4AGBgAAAAAAAAAAAAAdtwAdtwAAAAAAAAAOGxsOAAAAAAAAAAAAAAAAAAAAAAAABgYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAADwwMDAwM7GxsPBwAAAAAANhsbGxsbAAAAAAAAAAAAABw2DBgyPgAAAAAAAAAAAAAAAAAfHx8fHx8fAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==");
        }
        public void DrawString(int X, int Y, string Text, Color Color)
        {
            string[] Lines = Text.Split('\n');
            for (int Line = 0; Line < Lines.Length; Line++)
            {
                for (int Char = 0; Char < Lines[Line].Length; Char++)
                {
                    Font.Default.MS.Seek((Encoding.ASCII.GetBytes(Lines[Line][Char].ToString())[0] & 0xFF) * Font.Default.Height, SeekOrigin.Begin);
                    byte[] fontbuf = new byte[Font.Default.Height];
                    Font.Default.MS.Read(fontbuf, 0, fontbuf.Length);

                    for (int IY = 0; IY < Font.Default.Height; IY++)
                    {
                        for (int IX = 0; IX < Font.Default.Width; IX++)
                        {
                            if ((fontbuf[IY] & (0x80 >> IX)) != 0)
                                SetPixel(X + IX + (Char * Font.Default.Width), Y + IY + (Line * Font.Default.Height), Color);
                        }
                    }
                }
            }
        }

        public void DrawString(int X, int Y, WaveWindow window, string Text, Color Color)
        {
            string[] Lines = Text.Split('\n');
            for (int Line = 0; Line < Lines.Length; Line++)
            {
                for (int Char = 0; Char < Lines[Line].Length; Char++)
                {
                    Font.Default.MS.Seek((Encoding.ASCII.GetBytes(Lines[Line][Char].ToString())[0] & 0xFF) * Font.Default.Height, SeekOrigin.Begin);
                    byte[] fontbuf = new byte[Font.Default.Height];
                    Font.Default.MS.Read(fontbuf, 0, fontbuf.Length);

                    for (int IY = 0; IY < Font.Default.Height; IY++)
                    {
                        for (int IX = 0; IX < Font.Default.Width; IX++)
                        {
                            if ((fontbuf[IY] & (0x80 >> IX)) != 0 
                                && !(X + IX + (Char * Font.Default.Width) > window.X + window.Width) && !(Y + IY + (Line * Font.Default.Height) > window.Y + window.Height + window.titleBarHeight)
                                && X + IX + (Char * Font.Default.Width) > window.X && Y + IY + (Line * Font.Default.Height) > window.Y + window.titleBarHeight
                                )
                                SetPixel(X + IX + (Char * Font.Default.Width), Y + IY + (Line * Font.Default.Height), Color);
                        }
                    }
                }
            }
        }

        public void DrawString(int X, int Y, WaveElement element, string Text, Color Color)
        {
            string[] Lines = Text.Split('\n');
            for (int Line = 0; Line < Lines.Length; Line++)
            {
                for (int Char = 0; Char < Lines[Line].Length; Char++)
                {
                    Font.Default.MS.Seek((Encoding.ASCII.GetBytes(Lines[Line][Char].ToString())[0] & 0xFF) * Font.Default.Height, SeekOrigin.Begin);
                    byte[] fontbuf = new byte[Font.Default.Height];
                    Font.Default.MS.Read(fontbuf, 0, fontbuf.Length);

                    for (int IY = 0; IY < Font.Default.Height; IY++)
                    {
                        for (int IX = 0; IX < Font.Default.Width; IX++)
                        {
                            if ((fontbuf[IY] & (0x80 >> IX)) != 0
                                && !(X + IX + (Char * Font.Default.Width) > element.relativeX + element.Width) && !(Y + IY + (Line * Font.Default.Height) > element.relativeY + element.Height)
                                && X + IX + (Char * Font.Default.Width) > element.relativeX && Y + IY + (Line * Font.Default.Height) > element.relativeY
                                )
                                SetPixel(X + IX + (Char * Font.Default.Width), Y + IY + (Line * Font.Default.Height), Color);
                        }
                    }
                }
            }
        }

        public void DrawString(int X, int Y, int XBounds, int YBounds, string Text, Color Color)
        {
            string[] Lines = Text.Split('\n');
            for (int Line = 0; Line < Lines.Length; Line++)
            {
                for (int Char = 0; Char < Lines[Line].Length; Char++)
                {
                    Font.Default.MS.Seek((Encoding.ASCII.GetBytes(Lines[Line][Char].ToString())[0] & 0xFF) * Font.Default.Height, SeekOrigin.Begin);
                    byte[] fontbuf = new byte[Font.Default.Height];
                    Font.Default.MS.Read(fontbuf, 0, fontbuf.Length);

                    for (int IY = 0; IY < Font.Default.Height; IY++)
                    {
                        for (int IX = 0; IX < Font.Default.Width; IX++)
                        {
                            if ((fontbuf[IY] & (0x80 >> IX)) != 0
                                && !(X + IX + (Char * Font.Default.Width) > X + XBounds) && !(Y + IY + (Line * Font.Default.Height) > Y + YBounds)
                                )
                                SetPixel(X + IX + (Char * Font.Default.Width), Y + IY + (Line * Font.Default.Height), Color);
                        }
                    }
                }
            }
        }

        #endregion

        #region WaveOS Specific
        public void Draw3DBorder(int X, int Y, int Width, int Height)
        {
            //Top and left white lines
            DrawFilledRectangle(X, Y, Width - 1, 1, 0, Color.White);
            DrawFilledRectangle(X, Y, 1, Height - 1, 0, Color.White);

            //Inner shadow Top
            DrawFilledRectangle(X + 1, Y + 1, Width - 1, 1, 0, new Color(223, 223, 223));
            DrawFilledRectangle(X + 1, Y + 1, 1, Height - 1, 0, new Color(223, 223, 223));

            //Inner shadow Bottom
            DrawFilledRectangle(X + 1, (Y + Height) - 2, Width - 1, 1, 0, new Color(127, 127, 127));
            DrawFilledRectangle((X + Width) - 2, Y + 1, 1, Height - 1, 0, new Color(127, 127, 127));

            //Bottom and right black lines
            DrawFilledRectangle(X, (Y + Height) - 1, Width, 1, 0, Color.Black);
            DrawFilledRectangle((X + Width) - 1, Y, 1, Height, 0, Color.Black);
        }

        public void DrawHorizontalGradient(int X, int Y, int Width, int Height, List<Color> gradient)
        {
            for (int i = 0; i < Width; i++)
            {
                float e = (float)i / (Width);
                e = e * gradient.Count;

                DrawLine(X + i, Y, X + i, Y + Height, gradient[(int)e]);
            }
        }

        public void DrawVerticalGradient(int X, int Y, int Width, int Height, List<Color> gradient)
        {
            for (int i = 0; i < Height; i++)
            {
                float e = (float)i / (Height);
                e = e * gradient.Count;

                //DrawLine(X + i, Y, X + i, Y + Height, gradient[(int)e]);
                DrawLine(X, Y + i, X + Width, Y + i, gradient[(int)e]);
            }
        }

        #endregion

        #region Misc

        public void Clear(Color? Color = null)
        {
            if (Color == null)
                Color = Graphics.Color.Black;

            MemoryOperations.Fill((int[])(object)Buffer, Color.Value.ARGB);
        }

        public void Update()
        {
            Frames++;
            if ((DateTime.Now - LT).TotalSeconds >= 1)
            {
                Cosmos.Core.Memory.Heap.Collect();
                FPS = Frames;
                Frames = 0;
                LT = DateTime.Now;
            }

            Global.BaseIOGroups.VBE.LinearFrameBuffer.Copy((int[])(object)Buffer);
        }

        #endregion
    }
}