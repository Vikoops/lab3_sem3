using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ConsoleBattleCity;
using System;
using System.IO;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class GameTests
    {
        private Game game;

        [TestInitialize]
        public void SetUp()
        {
            game = new Game();
        }

        [TestMethod]
        public void TestInitializeMap()
        {
            game.InitializeMap();

            // Проверяем, что карта инициализировалась и игрок находится в правильной позиции
            Assert.AreEqual('P', game.map[game.playerY, game.playerX], "Игрок не находится в правильной позиции.");
        }

        [TestMethod]
        public void TestPlayerMovement_InvalidMove()
        {
            game.InitializeMap();

            // Сдвигаем игрока в край карты, чтобы проверить блокировку движения
            game.playerX = 0;
            game.playerY = 0;

            // Моделируем нажатие клавиши "W" для движения вверх
            Console.SetIn(new StringReader("W"));
            game.HandleInput();

            // Проверяем, что игрок не вышел за границы
            Assert.AreEqual(0, game.playerX);
            Assert.AreEqual(0, game.playerY);
        }

        [TestMethod]
        public void TestSpawnEnemies()
        {
            game.InitializeMap();
            game.SpawnEnemies(4);

            // Проверяем, что было создано 4 врага
            Assert.AreEqual(4, game.enemies.Count);
        }

        [TestMethod]
        public void TestMoveEnemies()
        {
            game.InitializeMap();
            game.SpawnEnemies(1);
            var initialEnemyPosition = (game.enemies[0].X, game.enemies[0].Y);

            game.MoveEnemies();

            // Проверяем, что враг переместился с исходной позиции
            Assert.AreNotEqual(initialEnemyPosition, (game.enemies[0].X, game.enemies[0].Y));
        }

        [TestMethod]
        public void TestCheckCollisionWithEnemies_NoCollision()
        {
            game.InitializeMap();
            game.SpawnEnemies(1);

            // Игрок и враги не сталкиваются вначале
            bool collision = game.CheckCollisionWithEnemies();
            Assert.IsFalse(collision);
        }

        [TestMethod]
        public void TestCheckCollisionWithEnemies_Collision()
        {
            game.InitializeMap();
            game.enemies.Add(new Enemy { X = game.playerX, Y = game.playerY }); // Добавляем врага в позицию игрока

            // Проверяем столкновение
            bool collision = game.CheckCollisionWithEnemies();
            Assert.IsTrue(collision);
        }

        [TestMethod]
        public void TestSaveAndLoadGameState()
        {
            game.InitializeMap();
            game.SpawnEnemies(3);
            game.SaveGameState();

            var newGame = new Game();
            newGame.LoadGameState();

            // Проверяем, что состояние игры было загружено корректно
            Assert.AreEqual(game.playerX, newGame.playerX);
            Assert.AreEqual(game.playerY, newGame.playerY);
            Assert.AreEqual(game.enemies.Count, newGame.enemies.Count);
        }    
    }
}
