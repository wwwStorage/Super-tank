﻿using SuperTank.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceModel;
using SuperTank.Audio;
using System.Threading;
using System.IO;
using System.Net;
using SuperTank.Comunication;

namespace SuperTank.WindowsForms
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public partial class GameForm : Form, ITwoComputer
    {
        private readonly int portChannelKeyboard = 9091;
        private readonly int portTwoComputer = 9092;
        private readonly int portHostRender = 9090;
        private readonly int portHostSound = 9090;
        private readonly int portHostInfo = 9090;

        private IPAddress ipIIPlayer;
        private Game game;
        
        private ServiceHost hostTwoComputer;
        private GameOver gameOver = new GameOver();
        private StartScren startScren;
        private ScrenRecord screnRecord = null;
        private ScrenConstructor screnConstructor;

        private SoundGame soundGame;
        private SceneScene sceneView;
        private ScrenGame screnGame;
        private static IViewSound viewSound;

        private bool wcfClose;
        private bool isGameStop = true;
        private bool isStartScren = true;
        private bool waitForConectToGame = false;
        private bool constructorHasMap;
        private Owner ownerPlayer;

        private Action closeHost = () => { };
        private Action closeChannel = () => { };

        public GameForm()
        {
            InitializeComponent();

            soundGame = new SoundGame();

            GameForm.viewSound = soundGame;
            this.ClientSize = new Size(ConfigurationView.WindowClientWidth, ConfigurationView.WindowClientHeight);
            this.FormClosing += GameForm_FormClosing;

            gameOver.Size = this.ClientSize;
            startScren = new StartScren(this.ClientSize);

            screnConstructor = new ScrenConstructor();
            screnConstructor.Size = this.ClientSize;

            startScren.Start();
            Controls.Add(startScren);
        }

        public static IViewSound Sound { get { return viewSound; } }
        public IKeyboard Keyboard { get; set; }

        public void StartedTwoComp()
        {
            hostTwoComputer.BeginClose(null, null);
            StartNewGame(ipIIPlayer);
            waitForConectToGame = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!isGameStop)
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Up:
                    case Keys.Down:
                    case Keys.Space:
                    case Keys.Enter:
                    case Keys.Escape:
                        Keyboard.KeyDown(e.KeyCode);
                        break;
                }
            }
            else if (screnConstructor.IsActiv)
            {
                screnConstructor.KeyDown(e.KeyCode);
            }
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (waitForConectToGame) return;

            if (!isGameStop)
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Up:
                    case Keys.Down:
                    case Keys.Space:
                    case Keys.Enter:
                    case Keys.Escape:
                        Keyboard.KeyUp(e.KeyCode);
                        break;
                }
            }
            else if (isStartScren)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (startScren.IndexMenu == 0)
                    {
                        StartNewGame(null);
                        ownerPlayer = SuperTank.Owner.IPlayer;
                    }
                    else if (startScren.IndexMenu == 1)
                    {
                        ConfigureGameTwoPlayer();
                    }
                    else if (startScren.IndexMenu == 2)
                    {
                        StartConstructor();
                    }
                }
                else if (e.KeyCode == Keys.Up)
                {
                    startScren.MenuCursorUp();
                }
                else if (e.KeyCode == Keys.Down)
                {
                    startScren.MenuCursorDown();
                }
            }
            else if (screnConstructor.IsActiv)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    StopConstructor();
                }
                screnConstructor.KeyUp(e.KeyCode);
            }
        }

        private void StartNewGame(IPAddress ipIIPlayer)
        {
            isStartScren = false;
            sceneView = new SceneScene();
            LevelInfo levelInfo = null;
            ScrenScore screnScore = null;
            if (ipIIPlayer != null)
            {
                levelInfo = new LevelInfoTwoPlayer();
                screnScore = new ScrenScoreTwoPlayers();
            }
            else
            {
                levelInfo = new LevelInfo();
                screnScore = new ScrenScore();
            }
            screnGame = new ScrenGame(sceneView, levelInfo, screnScore, this.GameOver);

            this.OpenHost();
            Controls.Remove(startScren);
            Controls.Add(screnGame);

            ThreadPool.QueueUserWorkItem((o) =>
            {
                ThreadPool.QueueUserWorkItem((s) =>
                {
                    game = new Game();
                    if (constructorHasMap) game.Start(screnConstructor.GetMap(), ipIIPlayer);
                    else game.Start(ipIIPlayer);
                    isGameStop = false;
                });

                this.OpenChannel();
            });
        }
        private void GameOver()
        {
            ThreadPool.QueueUserWorkItem((s) =>
            {
                constructorHasMap = false;
                int maxPointsInFile = 0;
                int maxPointsInGame = 0;
                Thread.Sleep(ConfigurationView.DelayScrenPoints);
                Invoke(new Action(() =>
                {
                    Controls.Remove(screnGame);
                    Controls.Add(gameOver);
                    gameOver.Invalidate();
                }));
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    maxPointsInFile = int.Parse(File.ReadAllText(ConfigurationView.MaxPointsPath));
                    if (ownerPlayer == SuperTank.Owner.IIPlayer) maxPointsInGame = screnGame.CountPointsIIPlayer;
                    else maxPointsInGame = screnGame.CountPointsIPlayer;
                    if (maxPointsInGame > maxPointsInFile)
                    {
                        File.WriteAllText(ConfigurationView.MaxPointsPath, maxPointsInGame.ToString());
                    }
                });

                Thread.Sleep(ConfigurationView.TimeScrenGameOver);

                if (maxPointsInGame > maxPointsInFile)
                {
                    Invoke(new Action(() =>
                    {
                        Controls.Remove(gameOver);
                        screnRecord = new ScrenRecord(maxPointsInGame);
                        screnRecord.Size = ClientSize;
                        viewSound.HighScore();
                        Controls.Add(screnRecord);
                        screnRecord.Start();
                        screnRecord.Invalidate();
                    }));
                    Thread.Sleep(ConfigurationView.DelayScrenRecord);
                    Invoke(new Action(() =>
                    {
                        Controls.Remove(screnRecord);
                    }));
                }
                StopGame();
                Invoke(new Action(() =>
                {
                    Controls.Remove(gameOver);
                    startScren.Start();
                    Controls.Add(startScren);
                    isStartScren = true;
                }));
            });
        }
        private void StopGame()
        {
            if (isGameStop) return;

            isGameStop = true;

            if (game != null)
            {
                game.Stop();
                Thread.Sleep(300);
                game.CloseChannelFactory(); 
                this.CloseChannelFactory();
                game.CloseHost();
                game = null;
            }
            this.CloseHost();
        }
        private void StartConstructor()
        {
            isStartScren = false;
            screnConstructor.Start();
            Controls.Remove(startScren);
            Controls.Add(screnConstructor);
        }
        private void StopConstructor()
        {
            isStartScren = true;
            screnConstructor.Stop();
            Controls.Remove(screnConstructor);
            Controls.Add(startScren);
            constructorHasMap = true;
        }
        private void ConfigureGameTwoPlayer()
        {
            DialogIP dialogIP = new DialogIP();
            if (DialogResult.OK == dialogIP.ShowDialog())
            {
                if (dialogIP.NewGame)
                {
                    ipIIPlayer = dialogIP.GameIP;
                    hostTwoComputer = new ServiceHost(this);
                    hostTwoComputer.CloseTimeout = TimeSpan.FromMilliseconds(0);
                    hostTwoComputer.AddServiceEndpoint(typeof(ITwoComputer), new NetTcpBinding(), "net.tcp://" + dialogIP.GameIP + ":" + portTwoComputer + "/ITwoComputer");
                    hostTwoComputer.Open();
                    waitForConectToGame = true;
                }
                else if (dialogIP.JoinGame)
                {
                    JoinToGame(dialogIP);
                }
            }
        }
        private void JoinToGame(DialogIP dialogIP)
        {
            try
            {
                ChannelFactory<ITwoComputer> factoryTwoComputer = new ChannelFactory<ITwoComputer>(new NetTcpBinding(), "net.tcp://" + dialogIP.GameIP + ":" + portTwoComputer + "/ITwoComputer");

                sceneView = new SceneScene();
                screnGame = new ScrenGame(sceneView, new LevelInfoTwoPlayer(), new ScrenScoreTwoPlayers(), this.GameOver);
                this.OpenHost(dialogIP.GameIP);

                factoryTwoComputer.CreateChannel().StartedTwoComp();
                factoryTwoComputer.BeginClose(null, null);
                ownerPlayer = SuperTank.Owner.IIPlayer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                CloseHost();
                return;
            }

            OpenTCPChannel(dialogIP.GameIP);

            Controls.Remove(startScren);
            Controls.Add(screnGame);

            isStartScren = false;
            isGameStop = false;
        }

        public void CloseChannelFactory()
        {
            try
            {
                closeChannel.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                closeChannel = () => { };
            }
        }
        public void OpenChannel()
        {
            ChannelFactory<IKeyboard>  factoryKeyboard = new ChannelFactory<IKeyboard>(new NetNamedPipeBinding(), "net.pipe://localhost/IKeyboard");
            Keyboard = factoryKeyboard.CreateChannel();
            closeChannel += () =>
            {
                factoryKeyboard.Close();
            };
            ownerPlayer = SuperTank.Owner.IPlayer;
        }
        public void OpenTCPChannel(IPAddress ipIIPlayer)
        {
            string ipAddress = ipIIPlayer.ToString();
            ChannelFactory<IKeyboard>  factoryKeyboard = new ChannelFactory<IKeyboard>(new NetTcpBinding(), "net.tcp://" + ipAddress + ":" + portChannelKeyboard + "/IKeyboard");
            factoryKeyboard.Open(TimeSpan.FromMilliseconds(500));
            Keyboard = factoryKeyboard.CreateChannel();
            closeChannel += () =>
            {
                factoryKeyboard.Close();
            };
        }
        public void OpenHost()
        {
            ServiceHost hostSound = new ServiceHost(soundGame);
            hostSound.CloseTimeout = TimeSpan.FromMilliseconds(0);
            hostSound.AddServiceEndpoint(typeof(ISoundGame), new NetNamedPipeBinding(), "net.pipe://localhost/ISoundGame");
            hostSound.Open();

            ServiceHost hostSceneView = new ServiceHost(sceneView);
            hostSceneView.CloseTimeout = TimeSpan.FromMilliseconds(0);
            hostSceneView.AddServiceEndpoint(typeof(IRender), new NetNamedPipeBinding(), "net.pipe://localhost/IRender");
            hostSceneView.Open();

            ServiceHost hostGameInfo = new ServiceHost(screnGame);
            hostGameInfo.CloseTimeout = TimeSpan.FromMilliseconds(0);
            hostGameInfo.AddServiceEndpoint(typeof(IGameInfo), new NetNamedPipeBinding(), "net.pipe://localhost/IGameInfo");
            hostGameInfo.Open();

            closeHost += () =>
            {
                hostSound.Close();
                hostSceneView.Close();
                hostGameInfo.Close();
            };
        }
        private void OpenHost(IPAddress ipIIPlayer)
        {
            string ipAddress = ipIIPlayer.ToString();
            ServiceHost hostSound = new ServiceHost(soundGame);
            hostSound.CloseTimeout = TimeSpan.FromMilliseconds(0);
            hostSound.AddServiceEndpoint(typeof(ISoundGame), new NetTcpBinding(), "net.tcp://" + ipAddress + ":" + portHostSound + "/ISoundGame");
            hostSound.Open();

            ServiceHost hostSceneView = new ServiceHost(sceneView);
            hostSceneView.CloseTimeout = TimeSpan.FromMilliseconds(0);
            hostSceneView.AddServiceEndpoint(typeof(IRender), new NetTcpBinding(), "net.tcp://" + ipAddress + ":" + portHostRender + "/IRender");
            hostSceneView.Open();

            ServiceHost hostGameInfo = new ServiceHost(screnGame);
            hostGameInfo.CloseTimeout = TimeSpan.FromMilliseconds(0);
            hostGameInfo.AddServiceEndpoint(typeof(IGameInfo), new NetTcpBinding(), "net.tcp://" + ipAddress + ":" + portHostInfo + "/IGameInfo");
            hostGameInfo.Open();

            closeHost += () =>
            {
                hostSound.Close();
                hostSceneView.Close();
                hostGameInfo.Close();
            };
        }
        public void CloseHost()
        {
            closeHost.Invoke();
            closeHost = () => { };
        }
        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!wcfClose)
            {
                e.Cancel = true;
                soundGame.Dispose();

                ThreadPool.QueueUserWorkItem(s =>
                {
                    StopGame();
                    wcfClose = true;

                    Invoke((MethodInvoker)delegate ()
                    {
                        ((Form)sender).Close();
                    });
                });
            }
        }
    }
}
