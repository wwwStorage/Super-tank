﻿using SuperTank.Audio;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Timers;
using System.Linq;
using System.Threading.Tasks;

namespace SuperTank
{
    /// <summary>
    /// Controls the logic of the game end load level
    /// </summary>
    public class LevelManager
    {
        private bool removeAllTankEnemy;
        private DateTime startGameOver;
        private static bool abortUpdate;
        private static Random random = new Random();
        private static readonly Timer timer = new Timer();
        private static readonly List<IUpdatable> updatable = new List<IUpdatable>();

        private IGameInfo gameInfo;
        private ISoundGame soundGame;
        private int curentLevel;
        private Player IPlayer;
        private Player IIPlayer;
        private Enemy enemy;
        private bool isGameOver;
        private bool isLevelStart;
        private int countPlayers;
        private TimeSpan DelayScrenPoints;

        static LevelManager()
        {
            timer.Interval = ConfigurationGame.TimerInterval;
            timer.Elapsed += Timer_Elapsed;
        }
        public LevelManager(ISoundGame soundGame, IGameInfo gameInfo, Player IPlayer, Player IIPlayer, Enemy enemy)
        {
            countPlayers = 1;
            abortUpdate = false;
            curentLevel = 0;
            this.soundGame = soundGame;
            this.gameInfo = gameInfo;

            IPlayer.PlayerGameOver += GameOverPlayer;
            this.IPlayer = IPlayer;
            IPlayer.CountTank = 2;
            IPlayer.StartPosition = ConfigurationGame.StartPositionTankIPlaeyr;

            this.IIPlayer = IIPlayer;
            if (IIPlayer != null)
            {
                IIPlayer.CountTank = 2;
                IIPlayer.PlayerGameOver += GameOverPlayer;
                IIPlayer.StartPosition = ConfigurationGame.StartPositionTankIIPlaeyr;
                countPlayers++;
            }
            this.enemy = enemy;
            this.enemy.RemoveAllTank += Enemy_RemoveAllTank;
            CountLevel = ConfigurationGame.CountLevel;
        }

        public static List<IUpdatable> Updatable { get { return updatable; } }
        public static Random Random { get { return random; } }
        public static int CountLevel { get; set; }

        public void EagleDelited()
        {
            IPlayer.EagleDestoed();
            if (IIPlayer != null) IIPlayer.EagleDestoed();
            GameOver();
        }
        public void GameOverPlayer(Owner player)
        {
            countPlayers--;
            if(countPlayers == 0) GameOver();
        }
        public void GameOver()
        {
            if (isGameOver) return;

            isGameOver = true;
            startGameOver = DateTime.Now;
            Timer t = new Timer(ConfigurationBase.TimeGameOver);
            t.Elapsed += (s, a) =>
            {
                SetPointsToView();
                abortUpdate = true;
                Stop();
                Updatable.Clear();
                System.Threading.Thread.Sleep(100);
                enemy.Clear();
                IPlayer.Clear();
                IPlayer.CountTank = 0;
                if (IIPlayer != null)
                {
                    IIPlayer.Clear();
                    IIPlayer.CountTank = 0;
                }
                gameInfo.SetCountTankEnemy(0);
                Scene.Clear();
                curentLevel = 0;
                t.Stop();
                CountLevel = ConfigurationGame.CountLevel;
            };
            t.Start();
            gameInfo.GameOver();
        }

        private void SetPointsToView()
        {
            isLevelStart = false;
            int countTank;
            if (IIPlayer == null)
            {
                gameInfo.EndLevel(curentLevel, IPlayer.Points, IPlayer.DestroyedTanks, 0, null);
                countTank = IPlayer.DestroyedTanks.Values.Sum();
            }
            else
            {
                gameInfo.EndLevel(curentLevel, IPlayer.Points, IPlayer.DestroyedTanks, IIPlayer.Points, IIPlayer.DestroyedTanks);

                countTank = IPlayer.DestroyedTanks.Values.Sum() + IIPlayer.DestroyedTanks.Values.Sum();
            }
            DelayScrenPoints = ConfigurationGame.GetDelayScrenPoints(countTank);
        }

        public async void StartLevel()
        {
            DateTime start = DateTime.Now;
            if (curentLevel == ConfigurationGame.CountLevel) curentLevel = 0;
            gameInfo.StartLevel(curentLevel + 1);
            await Task.Delay(1200);
            IPlayer.Keyboard.Clear();
            IIPlayer?.Keyboard.Clear();
            Scene.Clear();
            CreateLevel(curentLevel + 1);
            TimeSpan timeSlip = ConfigurationGame.DelayScrenLoadLevel - (DateTime.Now - start);
            System.Threading.Thread.Sleep(timeSlip > TimeSpan.Zero ? timeSlip : TimeSpan.Zero);
            IPlayer.Start();
            if (IIPlayer != null) IIPlayer.Start();
            enemy.Start();
            abortUpdate = false;
            timer.Start();
            isLevelStart = true;
        }
        public void StartLevel(char[,] map)
        {
            ClerPos(map, ConfigurationGame.PositionEagle);
            ClerPos(map, ConfigurationGame.StartPositionTankIPlaeyr);
            ClerPos(map, ConfigurationGame.StartPositionTankIIPlaeyr);
            ClerPos(map, ConfigurationGame.StartPositionTankEnemy1);
            ClerPos(map, ConfigurationGame.StartPositionTankEnemy2);
            ClerPos(map, ConfigurationGame.StartPositionTankEnemy3);

            StringBuilder res = new StringBuilder();
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    res.Append(map[i, j].ToString());
                }
                res.Append('\n');
            }

            for (int i = 0; i < ConfigurationGame.CountTankEnemy; i++)
            {
                res.Append(ConfigurationGame.CharPlainTank);
            }
            File.WriteAllText(ConfigurationGame.Maps + "0", res.ToString());
            curentLevel = -1;
            StartLevel();

            CountLevel++;
        }
        public void EndLevel()
        {
            SetPointsToView();
            timer.Stop();
            abortUpdate = true;
            updatable.Clear();
            enemy.Clear();
            IPlayer.Clear();
            if (IIPlayer != null) IIPlayer.Clear();
            System.Threading.Thread.Sleep(200);
            for (int i = Scene.Units.Count - 1; i >= 0; i--)
            {
                switch (Scene.Units[i].Type)
                {
                    case TypeUnit.BrickWall:
                    case TypeUnit.ConcreteWall:
                    case TypeUnit.Water:
                    case TypeUnit.Forest:
                    case TypeUnit.Ice:
                    case TypeUnit.Eagle:
                        break;
                    default:
                        Scene.Remove(Scene.Units[i]);
                        break;
                }
            }
            System.Threading.Thread.Sleep(DelayScrenPoints);
        }
        public void CreateLevel(int level)
        {
            curentLevel = level;
            string[] linesTileMap = File.ReadAllLines(ConfigurationGame.Maps + level);

            List<Unit> objGame = GetStaticObjGame(linesTileMap);
            Unit eagle = FactoryUnit.CreateEagle(ConfigurationGame.PositionEagle.X,
                ConfigurationGame.PositionEagle.Y, soundGame);
            eagle.UnitDisposable += u => { EagleDelited(); };
            objGame.Add(eagle);
            Scene.AddRange(objGame);

            LoadEnemyTank(linesTileMap[26], enemy);
        }
        public void Stop()
        {
            timer.Stop();
            IPlayer.SoundGame.StopAll();
            IIPlayer?.SoundGame.StopAll();
        }
        public bool Pause(bool isPause)
        {
            if (removeAllTankEnemy || isGameOver || !isLevelStart) return false;

            if (isPause) Stop();
            else
            {
                timer.Start();
                IPlayer.SoundGame.StopSondTank();
                IIPlayer?.SoundGame.StopSondTank();
            }

            return true;
        }

        private void ClerPos(char[,] map, Point point)
        {
            int x = point.X / ConfigurationGame.WidthTile;
            int y = point.Y / ConfigurationGame.HeightTile;
            map[y, x] = map[y, x + 1] = map[y + 1, x] = map[y + 1, x + 1] = ' ';
        }

        private void LoadEnemyTank(string data, Enemy enemy)
        {
            foreach (char c in data)
            {

                if(c == ConfigurationGame.CharPlainTank)
                {
                    enemy.AddTypeTank(TypeUnit.PlainTank);
                }
                else if (c == ConfigurationGame.CharArmoredPersonnelCarrierTank)
                {
                    enemy.AddTypeTank(TypeUnit.ArmoredPersonnelCarrierTank);
                }
                else if ( c == ConfigurationGame.CharQuickFireTank)
                {
                    enemy.AddTypeTank(TypeUnit.QuickFireTank);
                }
                else if (c == ConfigurationGame.CharArmoredTank)
                {
                    enemy.AddTypeTank(TypeUnit.ArmoredTank);
                }
            }
        }
        private List<Unit> GetStaticObjGame(string[] linesTileMap)
        {
            List<Unit> objGame = new List<Unit>();
            int x = 0, y = 0;
            foreach (string line in linesTileMap)
            {
                foreach (char c in line)
                {
                    if (c == ConfigurationGame.CharBrickWall)
                    {
                        objGame.Add(FactoryUnit.CreateUnit(x, y, TypeUnit.BrickWall));
                    }
                    else if (c == ConfigurationGame.CharConcreteWall)
                    {
                        objGame.Add(FactoryUnit.CreateUnit(x, y, TypeUnit.ConcreteWall));
                    }
                    else if (c == ConfigurationGame.CharWater)
                    {
                        objGame.Add(FactoryUnit.CreateUnit(x, y, TypeUnit.Water));
                    }
                    else if (c == ConfigurationGame.CharForest)
                    {
                        objGame.Add(FactoryUnit.CreateUnit(x, y, TypeUnit.Forest));
                    }
                    else if (c == ConfigurationGame.CharIce)
                    {
                        objGame.Add(FactoryUnit.CreateUnit(x, y, TypeUnit.Ice));
                    }
                    x += ConfigurationGame.WidthTile;
                }
                x = 0;
                y += ConfigurationGame.HeightTile;
            }

            return objGame;
        }
        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (timer)
            {
                for (int i = 0; i < Updatable.Count; i++)
                {
                    if (abortUpdate) return;
                    Updatable[i].Update();
                }
            }
        }
        private void Enemy_RemoveAllTank()
        {
            removeAllTankEnemy = true;
            if (curentLevel == ConfigurationBase.CountLevel)
                curentLevel = 0;

            System.Threading.ThreadPool.QueueUserWorkItem(s =>
            {
                System.Threading.Thread.Sleep(3000);
                if (isGameOver) return;

                EndLevel();

                StartLevel();
                removeAllTankEnemy = false;
            });
        }
    }
}
