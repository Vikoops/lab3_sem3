using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text.Json;

namespace ConsoleBattleCity
{
    class GameState
    {
        public int PlayerX { get; set; }
        public int PlayerY { get; set; }
        public List<Enemy> Enemies { get; set; } // Изменено на список врагов
        public List<Obstacle> Obstacles { get; set; } // Изменено на список препятствий
        public int EnemyCount { get; set; }
    }

    class Enemy
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    class Obstacle
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
