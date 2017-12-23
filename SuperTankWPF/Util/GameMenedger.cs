﻿using GameLibrary.Lib;
using GameLibrary.Service;
using SuperTank;
using SuperTank.Audio;
using SuperTank.FH;
using SuperTank.View;
using SuperTankWPF.Model;
using SuperTankWPF.Service;
using SuperTankWPF.View;
using SuperTankWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ViewLibrary.Audio;
using ViewLibrary.Service;

namespace SuperTankWPF.Util
{
    class GameMenedger : IDisposable
    {
        private ScreenGameViewModel screenGame;
        private LevelInfoViewModel levelInfo;
        private ScreenSceneViewModel screenScene;
        private MainViewModel mainViewModel;
        private ScreenConstructionViewModel construction;
        private ScreenScoreViewModel screenScore;
        private ScreenLockViewModel screenLock;
        private SoundGame sound;
        private GameInfo gameInfo;
        private DialogIPViewModel dialogIPViewModel;
        private Game game;

        public GameMenedger(ScreenGameViewModel screenGame, MainViewModel mainViewModel,
            ScreenSceneViewModel screenScene, LevelInfoViewModel levelInfo,
            ScreenConstructionViewModel construction,
            ScreenScoreViewModel screenScore, ScreenLockViewModel screenLock, SoundGame sound,
            GameInfo iPlayerGameManedger, DialogIPViewModel dialogIPViewModel)
        {
            this.screenGame = screenGame;
            this.mainViewModel = mainViewModel;
            this.screenScene = screenScene;
            this.levelInfo = levelInfo;
            this.construction = construction;
            this.screenScore = screenScore;
            this.screenLock = screenLock;
            this.sound = sound;
            this.gameInfo = iPlayerGameManedger;
            this.dialogIPViewModel = dialogIPViewModel;
        }

        public void IPlayerExecute()
        {
            StartGame(null);
            levelInfo.IsTwoPlayer = false;
            screenScore.IsTwoPlayer = false;
        }

        private async void StartGame(IPAddress iPCurrentComputer)
        {
            await Task.Run(() =>
            {
                gameInfo.OwnerPlayer = Owner.IPlayer;
                gameInfo.EndOfGame += StopoGame;

                game = new Game(); if (construction.HasMap)
                    game.Start(construction.GetAndClerMap(), iPCurrentComputer);
                else
                    game.Start(iPCurrentComputer);
            });

            ThreadPool.QueueUserWorkItem(s =>
            {
                InstanceContext context = new InstanceContext(new GameClient(gameInfo, screenScene, sound));
                DuplexChannelFactory<IGameService> factory =
                    new DuplexChannelFactory<IGameService>(context, new NetNamedPipeBinding(),
                        "net.pipe://localhost/GameService");
                IGameService proxy = factory.CreateChannel();
                proxy.Connect(Owner.IPlayer);

                screenGame.Keyboard = new KeyboardWrapper(proxy, Owner.IPlayer);
            });
        }

        public void IIPlayerExecute()
        {
            FirewallHelper.Test();
            dialogIPViewModel.Init();
            DialogIP dialogIP = new DialogIP
            {
                Owner = Application.Current.MainWindow
            };

            if (dialogIP.ShowDialog() == true)
            {
                if (dialogIPViewModel.NewGame)
                {
                    screenLock.IsCancelVisible = Visibility.Visible;
                    screenLock.Cancel += () =>
                    {
                        StopoGame();
                        mainViewModel.ScreenStartVisibility = Visibility.Visible;
                    };
                    mainViewModel.ScreenLockVisibility = Visibility.Visible;
                    StartGame(dialogIPViewModel.IPCurrentComputer);
                }
                else if (dialogIPViewModel.JoinGame)
                {
                    ThreadPool.QueueUserWorkItem(s =>
                    {
                        mainViewModel.ScreenLockVisibility = Visibility.Visible;

                        InstanceContext context = new InstanceContext(new GameClient(gameInfo, screenScene, sound));
                        DuplexChannelFactory<IGameService> factory =
                            new DuplexChannelFactory<IGameService>(context, new NetTcpBinding(),
                                "net.tcp://" + dialogIPViewModel.IPRemoteComputer + ":9090/GameService");
                        IGameService proxy = factory.CreateChannel();
                        proxy.Connect(Owner.IIPlayer);

                        screenGame.Keyboard = new KeyboardWrapper(proxy, Owner.IIPlayer);
                        
                        gameInfo.OwnerPlayer = Owner.IIPlayer;
                        gameInfo.EndOfGame += StopoGame;
                    });
                }

                levelInfo.IsTwoPlayer = true;
                screenScore.IsTwoPlayer = true;
            }
        }
        public void ConstructionExecute()
        {
            mainViewModel.ScreenConstructionVisibility = Visibility.Visible;
        }

        public void StopoGame()
        {
            gameInfo.EndOfGame -= StopoGame;
            screenGame.Keyboard = null;
            game?.Stop();
            game?.CloseHost();

            dialogIPViewModel.Clerar();
            screenGame.Clear();
            screenScene.Clear();
            levelInfo.Cler();
            screenScore.Clear();
            screenLock.Clear();
        }
        public void Dispose()
        {
            StopoGame();
        }
    }
}
