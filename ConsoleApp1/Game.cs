using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text.Json;

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
                    if (File.Exists("game_state.json"))
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

                // Проверка на возможность движения врага
                if (IsValidMove(newEnemyX, newEnemyY))
                {
                    // Обновляем позицию врага
                    map[enemyY, enemyX] = '.'; // Стираем старую позицию врага
                    enemy.X = newEnemyX;
                    enemy.Y = newEnemyY;
                    map[newEnemyY, newEnemyX] = 'E'; // Рисуем нового врага
                }
            }
        }

        internal bool IsValidMove(int x, int y)
        {
            // Проверка на границы карты и препятствия
            return x > 0 && x < map.GetLength(1) - 1 && y > 0 && y < map.GetLength(0) - 1 &&
                   !obstacles.Any(o => o.X == x && o.Y == y); // Проверка на столкновение с препятствием
        }

        internal bool CheckCollisionWithEnemies()
        {
            return enemies.Any(e => e.X == playerX && e.Y == playerY); // Проверка столкновения с врагами
        }

        internal void SaveGameState()
        {
            GameState gameState = new GameState
            {
                PlayerX = playerX,
                PlayerY = playerY,
                Enemies = enemies.Select(e => new Enemy { X = e.X, Y = e.Y }).ToList(), // Сериализация врагов
                Obstacles = obstacles // Сериализация препятствий
            };

            string json = JsonSerializer.Serialize(gameState);
            File.WriteAllText("game_state.json", json); // Сохранение состояния игры в файл
        }

        internal void LoadGameState()
        {
            try
            {
                if (File.Exists("game_state.json"))
                {
                    string json = File.ReadAllText("game_state.json");
                    GameState gameState = JsonSerializer.Deserialize<GameState>(json);

                    if (gameState != null)
                    {
                        playerX = gameState.PlayerX;
                        playerY = gameState.PlayerY;
                        enemies = gameState.Enemies; // Загрузка врагов
                        obstacles = gameState.Obstacles; // Загрузка препятствий

                        InitializeMap(); // Инициализация карты с новыми позициями
                    }
                }
                else
                {
                    Console.WriteLine("Нет сохраненного состояния игры!");
                    Thread.Sleep(2000);
                    ShowMenu(); // Возвращаемся в меню
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Файл состояния игры не найден.");
                Console.WriteLine(ex.Message);
                Thread.Sleep(2000);
                ShowMenu(); // Возвращаемся в меню
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Ошибка загрузки состояния игры: некорректные данные.");
                Console.WriteLine(ex.Message);
                Thread.Sleep(2000);
                ShowMenu(); // Возвращаемся в меню
            }
            catch (IOException ex)
            {
                Console.WriteLine("Ошибка ввода-вывода при загрузке состояния игры.");
                Console.WriteLine(ex.Message);
                Thread.Sleep(2000);
                ShowMenu(); // Возвращаемся в меню
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла непредвиденная ошибка при загрузке состояния игры.");
                Console.WriteLine(ex.Message);
                Thread.Sleep(2000);
                ShowMenu(); // Возвращаемся в меню
            }
        }
    }
}
