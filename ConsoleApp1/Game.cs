using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ConsoleBattleCity
{
    class Game
    {
        internal char[,] map = new char[10, 20];
        internal int playerX = 1, playerY = 1;
        internal List<Obstacle> obstacles = new List<Obstacle>
        {
            new Obstacle { X = 3, Y = 3 }, new Obstacle { X = 4, Y = 3 }, new Obstacle { X = 5, Y = 3 },
            new Obstacle { X = 3, Y = 5 }, new Obstacle { X = 6, Y = 6 }, new Obstacle { X = 7, Y = 6 }
        };
        internal List<Enemy> enemies = new List<Enemy>();
        internal Random random = new Random();

        public void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("Управление игрой:\nW - вверх\nA - влево\nS - вниз\nD - вправо\nпробел - стрельба (вверх)\nEsc - выйти с сохранением игры");
            Console.WriteLine("====================");
            Console.WriteLine("=== Главное меню ===");
            Console.WriteLine("1. Новая игра");
            Console.WriteLine("2. Загрузить игру");
            Console.WriteLine("3. Выход");
            Console.WriteLine("Выберите опцию: ");

            var choice = Console.ReadKey(true).Key;

            switch (choice)
            {
                case ConsoleKey.D1: // Новая игра
                    InitializeNewGame();
                    break;
                case ConsoleKey.D2: // Загрузить игру
                    if (File.Exists("game_state.txt"))
                    {
                        LoadGameState(); // Загрузка состояния игры из файла
                        StartGame();
                    }
                    else
                    {
                        Console.WriteLine("Нет сохраненного состояния игры!");
                        Thread.Sleep(2000);
                        ShowMenu(); // Возвращаемся в меню
                    }
                    break;
                case ConsoleKey.D3: // Выход
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Неверный выбор. Пожалуйста, попробуйте еще раз.");
                    Thread.Sleep(2000);
                    ShowMenu(); // Возвращаемся в меню
                    break;
            }
        }

        internal void InitializeNewGame()
        {
            InitializeMap();
            SpawnEnemies(4); // Установлено 4 врага
            StartGame();
        }

        internal void StartGame()
        {
            while (true)
            {
                DrawMap();
                HandleInput();
                MoveEnemies();

                // Проверка столкновения с врагами
                if (CheckCollisionWithEnemies())
                {
                    Console.Clear();
                    Console.WriteLine("Вы столкнулись с врагом! Игра начнется заново...");
                    Thread.Sleep(2000); // Задержка перед перезапуском
                    ShowMenu(); // Перезапуск игры с меню
                }

                // Проверка, остались ли враги
                if (enemies.Count == 0)
                {
                    Console.Clear();
                    Console.WriteLine("Вы убили всех врагов! Хотите перезапустить игру? (Y/N)");
                    var key = Console.ReadKey(true).Key;

                    if (key == ConsoleKey.Y)
                    {
                        SaveGameState(); // Сохранение состояния игры
                        StartGame(); // Перезапуск игры
                    }
                    else
                    {
                        SaveGameState(); // Сохранение состояния игры перед выходом
                        Console.WriteLine("\nСпасибо за игру! Нажмите любую клавишу для выхода...");
                        Console.ReadKey(); // Ждём нажатия клавиши перед выходом
                        break; // Выход из игры
                    }
                }

                Thread.Sleep(100); // Задержка для управления скоростью игры
            }
        }

        internal void InitializeMap()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    map[i, j] = '.'; // Пустое пространство
                }
            }

            // Стены
            for (int i = 0; i < map.GetLength(0); i++)
            {
                map[i, 0] = '|'; // Левый край
                map[i, map.GetLength(1) - 1] = '|'; // Правый край
            }
            for (int j = 0; j < map.GetLength(1); j++)
            {
                map[0, j] = '-'; // Верхний край
                map[map.GetLength(0) - 1, j] = '-'; // Нижний край
            }

            // Препятствия
            foreach (var obstacle in obstacles)
            {
                map[obstacle.Y, obstacle.X] = '|'; // Препятствия
            }
            map[playerY, playerX] = 'P'; // Игрок
        }

        internal void DrawMap()
        {
            Console.Clear();

            // Отображаем карту с врагами
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    // Отображение врагов на карте
                    if (enemies.Any(e => e.Y == i && e.X == j))
                    {
                        Console.Write('E'); // E - враг
                    }
                    else
                    {
                        Console.Write(map[i, j]);
                    }
                }
                Console.WriteLine();
            }
        }

        internal void HandleInput()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;

                // Сохраняем текущее положение игрока
                int newPlayerX = playerX;
                int newPlayerY = playerY;

                switch (key)
                {
                    case ConsoleKey.W: // Вверх
                        newPlayerY--;
                        break;
                    case ConsoleKey.S: // Вниз
                        newPlayerY++;
                        break;
                    case ConsoleKey.A: // Влево
                        newPlayerX--;
                        break;
                    case ConsoleKey.D: // Вправо
                        newPlayerX++;
                        break;
                    case ConsoleKey.Spacebar: // Выстрел
                        Shoot();
                        break;
                    case ConsoleKey.Escape: // Сохранить и выйти
                        SaveGameState();
                        Environment.Exit(0);
                        break;
                }

                // Проверка на столкновение с препятствием
                if (IsValidMove(newPlayerX, newPlayerY))
                {
                    // Обновляем позицию игрока
                    map[playerY, playerX] = '.';
                    playerX = newPlayerX;
                    playerY = newPlayerY;
                    map[playerY, playerX] = 'P';
                }
            }
        }

        internal void Shoot()
        {
            int bulletX = playerX;
            int bulletY = playerY;

            // Стрельба вверх
            while (bulletY > 0)
            {
                bulletY--;
                DrawMap();

                // Проверка на столкновение с врагами
                for (int i = enemies.Count - 1; i >= 0; i--) // Итерируемся в обратном порядке
                {
                    if (enemies[i].Y == bulletY && enemies[i].X == bulletX)
                    {
                        // Уничтожаем врага
                        enemies.RemoveAt(i);
                        return; // Враг уничтожен, выходим из метода
                    }
                }

                if (map[bulletY, bulletX] == '#' || map[bulletY, bulletX] == '|' || map[bulletY, bulletX] == '-')
                {
                    break;
                }

                // Отрисовка пули
                map[bulletY, bulletX] = '*';
                Thread.Sleep(100);
                DrawMap();
                map[bulletY, bulletX] = '.';
            }
        }

        internal void SpawnEnemies(int count)
        {
            enemies.Clear(); // Очищаем старых врагов

            for (int i = 0; i < count; i++)
            {
                int enemyX, enemyY;
                do
                {
                    enemyX = random.Next(1, map.GetLength(1) - 1);
                    enemyY = random.Next(1, map.GetLength(0) - 1);
                } while (map[enemyY, enemyX] != '.');

                enemies.Add(new Enemy { X = enemyX, Y = enemyY }); // Добавление врага в виде объекта
            }
        }

        internal void MoveEnemies()
        {
            List<Enemy> currentEnemies = enemies.ToList(); // Копируем список врагов для изменения
            foreach (var enemy in currentEnemies)
            {
                // Определяем случайное направление движения
                int moveDirection = random.Next(0, 4);
                int enemyX = enemy.X;
                int enemyY = enemy.Y;

                // Изменяем позицию врага в зависимости от направления
                int newEnemyX = enemyX;
                int newEnemyY = enemyY;

                switch (moveDirection)
                {
                    case 0: // Вверх
                        newEnemyY--;
                        break;
                    case 1: // Вниз
                        newEnemyY++;
                        break;
                    case 2: // Влево
                        newEnemyX--;
                        break;
                    case 3: // Вправо
                        newEnemyX++;
                        break;
                }

                // Проверка на допустимость движения
                if (IsValidMove(newEnemyX, newEnemyY))
                {
                    enemy.X = newEnemyX;
                    enemy.Y = newEnemyY;
                }
            }
        }

        internal bool IsValidMove(int x, int y)
        {
            if (x < 1 || x >= map.GetLength(1) - 1 || y < 1 || y >= map.GetLength(0) - 1) // За пределами карты
                return false;

            if (map[y, x] == '|' || map[y, x] == '-' || map[y, x] == 'E') // Столкновение с препятствиями или врагами
                return false;

            return true;
        }

        internal bool CheckCollisionWithEnemies()
        {
            return enemies.Any(e => e.X == playerX && e.Y == playerY); // Если игрок на позиции врага
        }

        internal void SaveGameState()
        {
            try
            {
                List<string> gameStateLines = new List<string>
                {
                    playerX.ToString(),
                    playerY.ToString(),
                    enemies.Count.ToString()
                };

                foreach (var enemy in enemies)
                {
                    gameStateLines.Add($"{enemy.X},{enemy.Y}");
                }

                gameStateLines.Add(obstacles.Count.ToString());
                foreach (var obstacle in obstacles)
                {
                    gameStateLines.Add($"{obstacle.X},{obstacle.Y}");
                }

                File.WriteAllLines("game_state.txt", gameStateLines);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Ошибка при сохранении состояния игры: " + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Нет доступа к файлу для сохранения: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла непредвиденная ошибка при сохранении: " + ex.Message);
            }
        }

        internal void LoadGameState()
        {
            try
            {
                if (File.Exists("game_state.txt"))
                {
                    string[] gameStateLines = File.ReadAllLines("game_state.txt");

                    playerX = int.Parse(gameStateLines[0]);
                    playerY = int.Parse(gameStateLines[1]);

                    int enemyCount = int.Parse(gameStateLines[2]);
                    enemies.Clear();
                    for (int i = 0; i < enemyCount; i++)
                    {
                        string[] enemyCoords = gameStateLines[3 + i].Split(',');
                        int enemyX = int.Parse(enemyCoords[0]);
                        int enemyY = int.Parse(enemyCoords[1]);
                        enemies.Add(new Enemy { X = enemyX, Y = enemyY });
                    }

                    int obstacleCount = int.Parse(gameStateLines[3 + enemyCount]);
                    obstacles.Clear();
                    for (int i = 0; i < obstacleCount; i++)
                    {
                        string[] obstacleCoords = gameStateLines[4 + enemyCount + i].Split(',');
                        int obstacleX = int.Parse(obstacleCoords[0]);
                        int obstacleY = int.Parse(obstacleCoords[1]);
                        obstacles.Add(new Obstacle { X = obstacleX, Y = obstacleY });
                    }

                    InitializeMap();
                }
                else
                {
                    Console.WriteLine("Нет сохраненного состояния игры!");
                    Thread.Sleep(2000);
                    ShowMenu();
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Файл состояния игры не найден.");
                Console.WriteLine(ex.Message);
                Thread.Sleep(2000);
                ShowMenu();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Ошибка ввода-вывода при загрузке состояния игры.");
                Console.WriteLine(ex.Message);
                Thread.Sleep(2000);
                ShowMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла непредвиденная ошибка при загрузке состояния игры.");
                Console.WriteLine(ex.Message);
                Thread.Sleep(2000);
                ShowMenu();
            }
        }
    }

}
