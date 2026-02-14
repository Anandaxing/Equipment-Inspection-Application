using System;
using System.Threading;

class MainProgram
{
    public static void WelcomePage()
    {
        string banner = @"
        ╔══════════════════════════════════════════════════════════════════════════════════╗
        ║   _____   ______ _______ _____   ____  _   _           _____                     ║
        ║   |  __ \|  ____|__   __|  __ \ / __ \| \ | |   /\    / ____|     PETRONAS       ║
        ║   | |__) | |__     | |  | |__) | |  | |  \| |  /  \  | (___       Equipment      ║
        ║   |  ___/|  __|    | |  |  _  /| |  | | . ` | / /\ \  \___ \      Maintenance    ║
        ║   | |    | |____   | |  | | \ \| |__| | |\  |/ ____ \ ____) |     & Inspection   ║
        ║   |_|    |______|  |_|  |_|  \_\\____/|_| \_/_/    \_\_____/      v1.0.0         ║
        ╚══════════════════════════════════════════════════════════════════════════════════╝";
        string text = "[System]: Hey there, Welcome to PETRONAS equipment maintenance & inspection application";
        string typing_text = ""; // typing text string container
        Console.ForegroundColor = ConsoleColor.Cyan; // switch to cyan color
        Console.WriteLine(banner); // print banner
        Console.ResetColor(); // switch back to default color (white)
        foreach (var character in text) // Typing test 
        {
            typing_text += character;
            Console.Write($"\r{typing_text}"); // printing typing text
            Thread.Sleep(30); // Delay for 30 milisecond
        }
        Console.WriteLine("");
    }
    public static void ShowMenu()
    {
        string MenuControl = @"
    ╔═══════════════ MENU CONTROL ═══════════════╗
    1. List equipments
    2. List inspection operators
    3. Assign an operator to specific equipment
    4. Quit
    ╚════════════════════════════════════════════╝";
        string input_prompt = "[Your input]: ";
        string typing_text = ""; // typing text string container
        Console.WriteLine(MenuControl);
        foreach (var chars in input_prompt) // typing text
        {
            typing_text += chars;
            Console.Write($"\r{typing_text}"); // printing typing text
            Thread.Sleep(50); // Delay for 50 milisecond
        }
    }
    public static void Main()
    {
        WelcomePage();
        while(true)
        {
            ShowMenu();
            var user_input = Console.ReadLine();
        }
    }
}