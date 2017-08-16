﻿using SuperTank.Audio;
using SuperTank.Command;
using SuperTank.Updatable;
using SuperTank.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperTank
{
    public class FactoryUnit : IFactoryUnit
    {
        private IScene scene;

        public FactoryUnit(IScene scene)
        {
            this.scene = scene;
        }

        public Unit Create(int x, int y, TypeUnit type)
        {
            switch (type)
            {
                case TypeUnit.Star: 
                    return new Unit(Unit.NextID, x, y, ConfigurationGame.WidthTile * 2, ConfigurationGame.HeightTile * 2, TypeUnit.Star);
                case TypeUnit.PainTank:
                    return CreatePlainTank(x, y);
                case TypeUnit.Shell:
                    return CreateShell(x, y);
                case TypeUnit.BrickWall:
                    return CreateBrickWall(x, y);
                case TypeUnit.ConcreteWall:
                    return CreateConcreteWall(x, y);
                case TypeUnit.Water:
                    return CreateWater(x, y);
                case TypeUnit.Forest:
                    return CreateForest(x, y);
                case TypeUnit.Ice:
                    return CreateIce(x, y);
                case TypeUnit.Eagle:
                    return CreateEagle(x, y);
            }
            return null;
        }

        private Unit CreateEagle(int x, int y)
        {
            return new Unit(Unit.NextID, x, y, ConfigurationGame.WidthTile, ConfigurationGame.HeightTile, TypeUnit.Eagle);
        }

        private Unit CreateIce(int x, int y)
        {
            return new Unit(Unit.NextID, x, y, ConfigurationGame.WidthTile, ConfigurationGame.HeightTile, TypeUnit.Ice);
        }

        private Unit CreateForest(int x, int y)
        {
            return new Unit(Unit.NextID, x, y, ConfigurationGame.WidthTile, ConfigurationGame.HeightTile, TypeUnit.Forest);
        }

        private Unit CreateWater(int x, int y)
        {
            return new Unit(Unit.NextID, x, y, ConfigurationGame.WidthTile, ConfigurationGame.HeightTile, TypeUnit.Water);
        }

        private Unit CreateConcreteWall(int x, int y)
        {
            return new Unit(Unit.NextID, x, y, ConfigurationGame.WidthTile, ConfigurationGame.HeightTile, TypeUnit.ConcreteWall);
        }

        private Unit CreateBrickWall(int x, int y)
        {
            Unit res = new Unit(Unit.NextID, x, y, ConfigurationGame.WidthTile, ConfigurationGame.HeightTile, TypeUnit.BrickWall);
            return res;
        }

        private Unit CreateShell(int x, int y)
        {
            return new Unit(Unit.NextID, x, y, ConfigurationGame.WidthShell, ConfigurationGame.HeightShell, TypeUnit.Shell);
        }

        private Unit CreatePlainTank(int x, int y)
        {
            Unit tank = new Unit(Unit.NextID, x, y, ConfigurationGame.WidthTank, ConfigurationGame.HeigthTank, TypeUnit.PainTank);
            tank.Properties[PropertiesType.Velosity] = ConfigurationGame.VelostyPlainTank;
            tank.Properties[PropertiesType.Direction] = Direction.Up;
            tank.Properties[PropertiesType.IsParking] = true;

            tank.Commands.Add(TypeCommand.TurnDown, new CommandTurn(tank, Direction.Down));
            tank.Commands.Add(TypeCommand.TurnUp, new CommandTurn(tank, Direction.Up));
            tank.Commands.Add(TypeCommand.TurnLeft, new CommandTurn(tank, Direction.Left));
            tank.Commands.Add(TypeCommand.TurnRight, new CommandTurn(tank, Direction.Right));
            tank.Commands.Add(TypeCommand.Stop, new CommandStop(tank));
            tank.Commands.Add(TypeCommand.Move, new CommandMoveTank(tank, scene));
            tank.Commands.Add(TypeCommand.Fire, new CommandFire(tank, this, scene, ConfigurationGame.VelostyShellPlainTank));

            return tank;
        }

        public Unit CreatePlaeyrTank(int x, int y, TypeUnit type, ISoundGame soundGame)
        {
            Unit tank = null;
            switch (type)
            {
                case TypeUnit.PainTank:
                    tank = CreatePlainTank(x, y);
                    tank.Commands.Replace(TypeCommand.Move, new CommandMoveTankWithSound(tank, scene, soundGame));
                    tank.Commands.Replace(TypeCommand.Stop, new CommandStopWithSound(tank, soundGame));
                    tank.Commands.Replace(TypeCommand.Fire, new CommandFireWithSound(tank, this, scene, ConfigurationGame.VelostyShellPlainTank, soundGame));
                    break;
            }
            return tank;
        }
    }
}
