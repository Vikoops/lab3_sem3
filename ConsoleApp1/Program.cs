using System;
using System.IO;
using System.Threading;

namespace ConsoleBattleCity
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.ShowMenu(); // Отображение меню при запуске
        }
    }
}
