using System;
using System.Threading;

namespace EasyIotSharp.GateWay.Core.UI
{
    public class ConsoleUI
    {
        private static readonly string Banner = @"
 ███████╗ █████╗ ███████╗██╗   ██╗    ██╗ ██████╗ ████████╗███████╗██╗  ██╗ █████╗ ██████╗ ██████╗                    
 ██╔════╝██╔══██╗██╔════╝╚██╗ ██╔╝    ██║██╔═══██╗╚══██╔══╝██╔════╝██║  ██║██╔══██╗██╔══██╗██╔══██╗                   
 █████╗  ███████║███████╗ ╚████╔╝     ██║██║   ██║   ██║   ███████╗███████║███████║██████╔╝██████╔╝                   
 ██╔══╝  ██╔══██║╚════██║  ╚██╔╝      ██║██║   ██║   ██║   ╚════██║██╔══██║██╔══██║██╔══██╗██╔═══╝                    
 ███████╗██║  ██║███████║   ██║       ██║╚██████╔╝   ██║   ███████║██║  ██║██║  ██║██║  ██║██║                        
 ╚══════╝╚═╝  ╚═╝╚══════╝   ╚═╝       ╚═╝ ╚═════╝    ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝                        
                                                             
  ██████╗  █████╗ ████████╗███████╗██╗    ██╗ █████╗ ██╗   ██╗
 ██╔════╝ ██╔══██╗╚══██╔══╝██╔════╝██║    ██║██╔══██╗╚██╗ ██╔╝
 ██║  ███╗███████║   ██║   █████╗  ██║ █╗ ██║███████║ ╚████╔╝ 
 ██║   ██║██╔══██║   ██║   ██╔══╝  ██║███╗██║██╔══██║  ╚██╔╝  
 ╚██████╔╝██║  ██║   ██║   ███████╗╚███╔███╔╝██║  ██║   ██║   
  ╚═════╝ ╚═╝  ╚═╝   ╚═╝   ╚══════╝ ╚══╝╚══╝ ╚═╝  ╚═╝   ╚═╝   ";

        public static void ShowBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(Banner);
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.Write("║ ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("EasyIotSharp Gateway");
            Console.ResetColor();
            Console.Write(" :: Version 1.0.0 :: Build 2024    ║\n");
            Console.Write("║ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.ResetColor();
            Console.WriteLine(" :: JSuper Team              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        public static void ShowProgress(string message, Action action)
        {
            Console.Write($"[    ] {message}");
            try
            {
                action();
                Console.SetCursorPosition(1, Console.CursorTop);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("DONE");
                Console.ResetColor();
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.SetCursorPosition(1, Console.CursorTop);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("FAIL");
                Console.ResetColor();
                Console.WriteLine($" - {ex.Message}");
                return;
            }
        }

        public static void ShowSeparator()
        {
            Console.WriteLine("══════════════════════════════════════════════════════════");
        }

        public static void ShowSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}