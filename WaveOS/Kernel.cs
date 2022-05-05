using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Sys = Cosmos.System;

using Cosmos.System.FileSystem.VFS;
using Cosmos.System.FileSystem;

namespace WaveOS
{
    public class Kernel : Sys.Kernel
    {
        public static Kernel instance;
        WaveShell shell;

        CosmosVFS fs = new Sys.FileSystem.CosmosVFS();

        protected override void BeforeRun()
        {
            instance = this;
            Console.WriteLine("Starting shell...");

            Sys.FileSystem.VFS.VFSManager.RegisterVFS(fs);

            Console.Clear();
            shell = WaveShell.instance;
            shell.Initialize();
        }

        protected override void Run()
        {
            try
            {
                shell.Run();
            }
            catch(Exception e)
            {
                if (shell.curApp != null && shell.curApp.GetType().IsSubclassOf(typeof(WaveGUIApp)))
                {
                    WaveShell.Canv.VBE.DisableDisplay();
                    Cosmos.System.Graphics.VGAScreen.SetTextMode(VGADriver.TextSize.Size80x25);
                    Cosmos.System.Graphics.VGAScreen.SetFont(WaveShell.FontVga, WaveShell.Font.Height);
                }

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(@"
 $$$$$$$$\                                         
 $$  _____|                                        
 $$ |       $$$$$$\   $$$$$$\   $$$$$$\   $$$$$$\  
 $$$$$\    $$  __$$\ $$  __$$\ $$  __$$\ $$  __$$\ 
 $$  __|   $$ |  \__|$$ |  \__|$$ /  $$ |$$ |  \__|
 $$ |      $$ |      $$ |      $$ |  $$ |$$ |      
 $$$$$$$$\ $$ |      $$ |      \$$$$$$  |$$ |      
 \________|\__|      \__|       \______/ \__|      ");

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\nSomething went wrong!");
                Console.WriteLine(e);
                Console.WriteLine("\nThe OS will try to continue working, however if this issue persists,\ntry restarting the computer.");
                Console.WriteLine("\nPress a key to return to the shell...");
                Console.ReadKey();

                Console.Clear();
                Console.SetCursorPosition(0, 1);

                shell.curApp = null;
            }
        }
    }
}
