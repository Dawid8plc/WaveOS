using Cosmos.HAL;
using Cosmos.System.FileSystem.Listing;
using Figgle;
using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveOS.Apps;
using WaveOS.Graphics;
using WaveOS.Managers;
using Sys = Cosmos.System;

using VFS = Cosmos.System.FileSystem.VFS.VFSManager;

namespace WaveOS
{
    public class WaveShell
    {
        public string curDir = "0:\\";

        public static Canvas Canv;
        public static Random rnd = new Random();
        static WaveShell _instance;
        public static WaveShell instance { get { if (_instance == null) _instance = new WaveShell(); return _instance; } }
        public WaveApp curApp;

        public static Cosmos.System.Graphics.Fonts.PCScreenFont Font = Cosmos.System.Graphics.Fonts.PCScreenFont.Default;
        public static byte[] FontVga;

        static byte hour;
        static byte minute;

        public List<BarItem> barItems = new List<BarItem>();
        public int curBarItem = -1;

        public bool readLining = false;

        void neofetch()
        {
            Console.ForegroundColor = ConsoleColor.White;
            int startedOn = Console.CursorTop + 2;

            string WAVETEXT = @"        $$\      $$\                                
        $$ | $\  $$ |                               
        $$ |$$$\ $$ | $$$$$$\  $$\    $$\  $$$$$$\  
        $$ $$ $$\$$ | \____$$\ \$$\  $$  |$$  __$$\ 
        $$$$  _$$$$ | $$$$$$$ | \$$\$$  / $$$$$$$$ |
        $$$  / \$$$ |$$  __$$ |  \$$$  /  $$   ____|
        $$  /   \$$ |\$$$$$$$ |   \$  /   \$$$$$$$\ 
        \__/     \__| \_______|    \_/     \_______|";

            string OSTEXT = @" $$$$$$\   $$$$$$\  
$$  __$$\ $$  __$$\ 
$$ /  $$ |$$ /  \__|
$$ |  $$ |\$$$$$$\  
$$ |  $$ | \____$$\ 
$$ |  $$ |$$\   $$ |
 $$$$$$  |\$$$$$$  |
 \______/  \______/ ";


            Write("\n\n");
            using (var reader = new StringReader(OSTEXT))
            using (var reader2 = new StringReader(WAVETEXT))
            {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    string waveline = reader2.ReadLine();

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(waveline);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(line);
                }
            }

            Write("\n\n");

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.White;

            WriteLine("OS: WaveOS");
            WriteLine($"Available RAM: {Cosmos.Core.GCImplementation.GetAvailableRAM()} MB");
            WriteLine("Version: 0.0.1\n");
        }

        public void Initialize()
        {
            FontVga = Font.CreateVGAFont();
            Cosmos.System.Graphics.VGAScreen.SetFont(FontVga, Font.Height);

            neofetch();

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            hour = Cosmos.HAL.RTC.Hour;
            minute = Cosmos.HAL.RTC.Minute;

            barItems.Add(new BarButton() { Title = "Echo", Action = () => 
            { 
                if(!string.IsNullOrWhiteSpace(charsString))
                {
                    WriteLine("You wrote: " + charsString);
                }
            } });
            barItems.Add(new BarButton() { Title = "About", Action = () => { 
                Write("Wave", ConsoleColor.White); 
                Write("OS", ConsoleColor.Cyan); 
                WriteLine(" Shell\nVersion 0.0.1", ConsoleColor.White);

                Write("[", ConsoleColor.White); Write(instance.curDir, ConsoleColor.Cyan); Write("]\n", ConsoleColor.White); Write($">");
            } });  ;

            try
            {

                if (VFS.FileExists(curDir + "autoexec.ws"))
                {
                    DirectoryEntry autoexecEntry = VFS.GetFile(curDir + "autoexec.ws");

                    var file_stream = autoexecEntry.GetFileStream();
                    byte[] content = new byte[file_stream.Length];
                    file_stream.Read(content, 0, (int)file_stream.Length);

                    string cont = "";

                    foreach (char ch in content)
                    {
                        cont += ch.ToString();
                    }

                    string[] lines = cont.Split("\n");

                    Write("Executing autoexec\nPress anything to cancel");

                    DateTime LT = DateTime.Now;
                    int lastSecond = -1;

                    while (!((DateTime.Now - LT).TotalSeconds >= 3))
                    {
                        if (lastSecond != (DateTime.Now - LT).TotalSeconds)
                        {
                            lastSecond = (int)(DateTime.Now - LT).TotalSeconds;
                            Write(".");
                        }
                        if (Cosmos.System.KeyboardManager.TryReadKey(out Cosmos.System.KeyEvent k)) { Write("\n"); return; }
                    }

                    Write("\n");

                    foreach (var item in lines)
                    {
                        ProcessCommand(item.Trim());
                    }
                }
            }
            catch { }
        }
        public void Run()
        {
            DrawBar();

            WaveInput.BeforeUpdate();

            if (curApp == null) { Write("[", ConsoleColor.White); Write(curDir, ConsoleColor.Cyan); Write("]\n", ConsoleColor.White); Write($">");
                ProcessCommand(ReadLine()); }
            else if (!curApp.GetType().IsSubclassOf(typeof(WaveGUIApp)))
            {
                int exitCode = curApp.Run();
                if (exitCode != -1)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);

                    if(exitCode != 0)
                        WriteLine(curApp.Name + " exited with exit code " + exitCode);

                    curApp = null;
                }
            }
            else
            {
                int exitCode = curApp.Run();

                if (exitCode != -1)
                {
                    Canv.VBE.DisableDisplay();
                    Cosmos.System.Graphics.VGAScreen.SetTextMode(VGADriver.TextSize.Size80x25);
                    Cosmos.System.Graphics.VGAScreen.SetFont(WaveShell.FontVga, WaveShell.Font.Height);

                    WaveShell.Clear();
                    Console.SetCursorPosition(0, Console.CursorTop);

                    if (exitCode != 0)
                        WriteLine(curApp.Name + " exited with exit code " + exitCode);

                    curApp = null;
                }
            }

            if (hour != Cosmos.HAL.RTC.Hour || minute != Cosmos.HAL.RTC.Minute)
            {
                instance.DrawBar();
                hour = Cosmos.HAL.RTC.Hour;
                minute = Cosmos.HAL.RTC.Minute;
            }

            WaveInput.AfterUpdate();

            if(curApp == null || curApp != null && !curApp.GetType().IsSubclassOf(typeof(WaveGUIApp)))
                Cosmos.Core.Memory.Heap.Collect();
        }

        [ManifestResourceStream(ResourceName = "WaveOS.Assets.Wallpaper.bmp")]
        static byte[] wallpaper;
        public void ProcessCommand(string input)
        {
            WaveApp app = processApp(input);
            if (app != null)
            {
                curApp = app;
                int index = input.IndexOf(' ');
                curApp.args = (index != -1) ? input.Substring(index).TrimStart().Split(" ") : new string[0];

                if (curApp.GetType().IsSubclassOf(typeof(WaveGUIApp))){
                    if(Canv == null) Canv = new(800, 600); else Canv.VBE.VBESet(800, 600, 32, true);
                }

                curApp.Initialize();
            }
            else
            {
                switch(input.ToLower().Split(" ")[0])
                {
                    case "crash":
                        throw new Exception("User initiated crash.");
                        break;

                    case "echo":
                        int index = input.IndexOf(" ");
                        if(index != -1)
                            WriteLine(input.Substring(index).TrimStart());
                        break;

                    case "free":
                        long available_space = VFS.GetAvailableFreeSpace("0:\\");
                        WriteLine("Available Free Space: " + available_space);
                        break;

                    case "dir":
                        var files = VFS.GetDirectoryListing(curDir);

                        WriteLine("Directory listing for " + curDir);
                        WriteLine("File count: " + files.Count);
                        foreach (var file in files)
                        {
                            bool isDirectory = file.mEntryType == Cosmos.System.FileSystem.Listing.DirectoryEntryTypeEnum.Directory;
                            WriteLine((isDirectory ? "[" : "") + file.mName + (isDirectory ? "]" : ""));
                        }
                        break;

                    case "cat":
                        int indexcat = input.IndexOf(" ");
                        if (indexcat != -1)
                        {
                            string catDir = input.Substring(indexcat).TrimStart();

                            if (VFS.FileExists(curDir + catDir))
                            {
                                DirectoryEntry entry = VFS.GetFile(curDir + catDir);

                                var file_stream = entry.GetFileStream();
                                byte[] content = new byte[file_stream.Length];
                                file_stream.Read(content, 0, (int)file_stream.Length);

                                string cont = "";

                                foreach (char ch in content)
                                {
                                    cont += ch.ToString();
                                }

                                WriteLine(cont);
                            }
                            else
                            {
                                WriteLine(catDir + " is not a file or it doesn't exist.");
                            }
                        }
                        break;

                    case "rm":
                        int indexrm = input.IndexOf(" ");
                        if (indexrm != -1)
                        {
                            string rmDir = input.Substring(indexrm).TrimStart();

                            if (VFS.FileExists(curDir + rmDir))
                            {
                                VFS.DeleteFile(curDir + rmDir);
                            }
                            else
                            {
                                WriteLine(rmDir + " is not a file or it doesn't exist.");
                            }
                        }
                        break;

                    case "cd":
                        int indexcd = input.IndexOf(" ");
                        if (indexcd != -1)
                        {
                            string cdDir = input.Substring(indexcd).TrimStart();

                            if (cdDir == "..")
                            {
                                if (curDir != "0:\\")
                                {
                                    int cdInd = curDir.TrimEnd('\\').LastIndexOf("\\");

                                    curDir = curDir.Substring(0, cdInd + 1);
                                }
                            }
                            else
                            {
                                if (VFS.DirectoryExists(curDir + cdDir))
                                {
                                    curDir += cdDir + "\\";
                                }
                                else
                                {
                                    WriteLine(cdDir + " is not a directory or it doesn't exist.");
                                }
                            }
                        }
                        break;

                    case "cauto":
                        var cautoentry = VFS.CreateFile(curDir + "autoexec.ws");
                        var cautostream = cautoentry.GetFileStream();

                        if (cautostream.CanWrite)
                        {
                            byte[] text_to_write = Encoding.ASCII.GetBytes("echo Autoexec from HDD!");
                            cautostream.Write(text_to_write, 0, text_to_write.Length);
                        }
                        break;

                    case "load":
                        Formats.Image wallpaperImg = new Formats.Image(wallpaper);
                        //wallpaperImg.Dispose();
                        break;

                    case "pause":
                        WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;

                    case "clear":
                        Console.Clear();
                        DrawBar();
                        Console.SetCursorPosition(0, 1);
                        break;

                    case "neofetch":
                        neofetch();
                        break;

                    case "reboot":
                        Kernel.instance.Restart();
                        break;

                    case "shutdown":
                        Kernel.instance.Stop();
                        break;

                    case "cleanram":
                        Cosmos.Core.Memory.Heap.Collect();
                        break;

                    default:
                        WriteLine("No command found.");
                        break;
                }
            }
        }

        WaveApp processApp(string input)
        {
            switch (input.ToLower().Split(" ")[0])
            {
                case "helloworld": return new HelloWorld();
                case "help": return new Help();
                case "canvas": return new WindowManager();
                case "ramtest": return new RAMTest();
                default: return null;
            }
        }


        string GenSpaces(string a)
        {
            if (a.Length >= 75) return "";
            else
            {
                string b = "";
                for (int i = a.Length; i < 75; i++)
                {
                    b += " ";
                }

                return b;
            }
        }

        string GetTime()
        {
            // Time
            var hour = Cosmos.HAL.RTC.Hour;
            var minute = Cosmos.HAL.RTC.Minute;
            var strhour = hour.ToString();
            var strmin = minute.ToString();

            var intmin = Convert.ToInt32(strmin);
            var inthour = Convert.ToInt32(strhour);

            bool eh = intmin < 10;
            bool eh2 = inthour < 10;

            return (eh2 ? "0" + strhour : strhour) + ":" + (eh ? "0" + strmin : strmin);
        }

        void DrawBar()
        {
            ConsoleColor ofg = Console.ForegroundColor;
            ConsoleColor obg = Console.BackgroundColor;

            int origX = Console.CursorLeft;
            int origY = Console.CursorTop;

            Console.SetCursorPosition(0, 0);

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Gray;

            Console.Write("                                                                                ");
            Console.SetCursorPosition(0, 0);

            Console.Write((curApp == null ? "WaveOS Shell" : curApp.Name));

            Console.Write(" ");

            if (curApp != null)
            {
                for (int i = 0; i < curApp.barItems.Count; i++)
                {
                    string item = curApp.barItems[i];
                    if (i == curBarItem) Console.BackgroundColor = ConsoleColor.Blue; else Console.BackgroundColor = ConsoleColor.White;
                    Console.Write(item);

                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.Write(" ");
                }
            }
            else
            {
                for (int i = 0; i < barItems.Count; i++)
                {
                    BarItem item = barItems[i];
                    if(i == curBarItem) Console.BackgroundColor = ConsoleColor.Blue; else Console.BackgroundColor = ConsoleColor.White;
                    Console.Write(item.Title);

                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.Write(" ");
                }
            }

            Console.SetCursorPosition(75, 0);
            Console.WriteLine(GetTime());

            Console.BackgroundColor = ConsoleColor.Black;

            Console.SetCursorPosition(origX, origY);
            Console.ForegroundColor = ofg;
            Console.BackgroundColor = obg;
        }

        static List<char> chars;
        static string charsString { get { return new string(chars.ToArray()); } }
        static int currentCount;

        //Taken from Cosmos source code
        public static string ReadLine()
        {
            instance.DrawBar();

            chars = new List<char>(32);
            currentCount = 0;

            while (true)
            {
                instance.readLining = true;

                if (hour != Cosmos.HAL.RTC.Hour || minute != Cosmos.HAL.RTC.Minute)
                {
                    instance.DrawBar();
                    hour = Cosmos.HAL.RTC.Hour;
                    minute = Cosmos.HAL.RTC.Minute;
                }

                if (Sys.KeyboardManager.TryReadKey(out Sys.KeyEvent current))
                {
                    if (current.Key == Sys.ConsoleKeyEx.Tab)
                    {
                        instance.curBarItem = 0;
                        instance.DrawBar();
                        continue;
                    }

                    if(instance.curBarItem != -1)
                    {
                        if(current.Key == Sys.ConsoleKeyEx.LeftArrow)
                        {
                            if (instance.curBarItem > 0)
                                instance.curBarItem--;
                            else
                                instance.curBarItem = instance.barItems.Count - 1;
                            instance.DrawBar();
                        }

                        if (current.Key == Sys.ConsoleKeyEx.RightArrow)
                        {
                            if (instance.curBarItem < instance.barItems.Count - 1)
                                instance.curBarItem++;
                            else
                                instance.curBarItem = 0;
                            instance.DrawBar();
                        }

                        if(current.Key == Sys.ConsoleKeyEx.Enter)
                        {
                            if(instance.barItems[instance.curBarItem].GetType() == typeof(BarButton))
                            {
                                BarButton btn = (BarButton)instance.barItems[instance.curBarItem];
                                if (btn.Action != null)
                                    btn.Action();
                            }

                            instance.curBarItem = -1;
                            instance.DrawBar();
                        }

                        if(current.Key == Sys.ConsoleKeyEx.Escape || current.Key == Sys.ConsoleKeyEx.Tab)
                        {
                            instance.curBarItem = -1;
                            instance.DrawBar();
                            continue;
                        }
                    }
                    else 
                    {

                        if (current.Key == Sys.ConsoleKeyEx.NumEnter || current.Key == Sys.ConsoleKeyEx.Enter)
                        {
                            break;
                        }
                        //Check for "special" keys
                        if (current.Key == Sys.ConsoleKeyEx.Backspace) // Backspace
                        {
                            if (currentCount > 0)
                            {
                                int curCharTemp = Console.CursorLeft;
                                chars.RemoveAt(currentCount - 1);
                                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);

                                //Move characters to the left
                                for (int x = currentCount - 1; x < chars.Count; x++)
                                {
                                    Write(chars[x], null, true);
                                }

                                Write(' ', null, true);

                                Console.SetCursorPosition(curCharTemp - 1, Console.CursorTop);

                                currentCount--;
                            }
                            continue;
                        }
                        else if (current.Key == Sys.ConsoleKeyEx.LeftArrow)
                        {
                            if (currentCount > 0)
                            {
                                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                                currentCount--;
                            }
                            continue;
                        }
                        else if (current.Key == Sys.ConsoleKeyEx.RightArrow)
                        {
                            if (currentCount < chars.Count)
                            {
                                Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                                currentCount++;
                            }
                            continue;
                        }

                        if (current.KeyChar == '\0')
                        {
                            continue;
                        }

                        //Write the character to the screen
                        if (currentCount == chars.Count)
                        {
                            chars.Add(current.KeyChar);
                            Write(current.KeyChar, null, true);
                            currentCount++;
                        }
                        else
                        {
                            //Insert the new character in the correct location
                            //For some reason, List.Insert() doesn't work properly
                            //so the character has to be inserted manually
                            var temp = new List<char>();

                            for (int x = 0; x < chars.Count; x++)
                            {
                                if (x == currentCount)
                                {
                                    temp.Add(current.KeyChar);
                                }

                                temp.Add(chars[x]);
                            }

                            chars = temp;

                            //Shift the characters to the right
                            for (int x = currentCount; x < chars.Count; x++)
                            {
                                Write(chars[x], null, true);
                            }

                            Console.SetCursorPosition(Console.CursorLeft - (chars.Count - currentCount) - 1, Console.CursorTop);
                            currentCount++;
                        }
                    }
                }
            }

            WriteLine();

            char[] final = chars.ToArray();

            instance.readLining = false;
            return new string(final);
        }

        public static void WriteLine()
        {
            instance.DrawBar();
            Console.WriteLine();
        }

        public static void WriteLine(string input, ConsoleColor? color = null)
        {
            //Get rid of whatever user has written, if we trigger WriteLine for example from a BarButton
            if (instance.readLining)
            {
                WriteLine();
                chars.Clear();
                currentCount = 0;
            }

            instance.DrawBar();
            if(color != null)
                Console.ForegroundColor = (ConsoleColor)color;
            Console.WriteLine(input);
        }

        public static void Write(string input, ConsoleColor? color = null, bool userInitiated = false)
        {
            if (!userInitiated && instance.readLining)
            {
                WriteLine();
                chars.Clear();
                currentCount = 0;

                instance.readLining = false;
            }

            instance.DrawBar();
            if (color != null)
                Console.ForegroundColor = (ConsoleColor)color;
            Console.Write(input);
        }

        public static void Write(char input, ConsoleColor? color = null, bool userInitiated = false)
        {
            Write(input.ToString(), color, userInitiated);
        }

        public static void Clear()
        {
            Console.Clear();
            instance.DrawBar();
            Console.SetCursorPosition(0, 1);
        }

    }
}
