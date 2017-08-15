﻿using SuperTank.Updatable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperTank.Command
{
    class CommandFire : BaseDirectionCommand
    {
        private IFactoryUnit factoryUnit;
        private IScene scene;
        private int velosityFire;

        public CommandFire(Unit unit, IFactoryUnit factoryUnit, IScene scene, int velosityFire) : base(unit)
        {
            this.scene = scene;
            this.factoryUnit = factoryUnit;
            this.velosityFire = velosityFire;
            PrevShell = null;
        }

        protected virtual Unit PrevShell { get; set; }

        public override void Execute()
        {
            if (PrevShell != null) return;

            Unit shell = null;
            int x = 0, y = 0;
            switch (Direction)
            {
                case Direction.Up:
                    x = Unit.X + ConfigurationGame.WidthTile - ConfigurationGame.WidthShell / 2;
                    y = Unit.Y + ConfigurationGame.HeightShell;
                    break;
                case Direction.Right:
                    x = Unit.X + ConfigurationGame.WidthTank - ConfigurationGame.WidthShell;
                    y = Unit.Y + ConfigurationGame.HeightTile - ConfigurationGame.HeightShell / 2;
                    break;
                case Direction.Down:
                    x = Unit.X + ConfigurationGame.WidthTile - ConfigurationGame.WidthShell / 2;
                    y = Unit.Y + ConfigurationGame.HeigthTank - ConfigurationGame.HeightShell;
                    break;
                case Direction.Left:
                    x = Unit.X + ConfigurationGame.WidthShell;
                    y = Unit.Y + ConfigurationGame.HeightTile - ConfigurationGame.HeightShell / 2;
                    break;
            }
            shell = CreateShell(x, y);
        }

        protected virtual Unit CreateShell(int x, int y)
        {
            Unit shell = factoryUnit.Create(x, y, TypeUnit.Shell);
            PrevShell = shell;

            shell.Properties[PropertiesType.Direction] = Direction;
            shell.Properties[PropertiesType.Velosity] = velosityFire;
            UpdatableShell updatableShell = new UpdatableShell(shell);
            Game.Updatable.Add(updatableShell);
            Action action = new Action(() =>
            {
                PrevShell = null;
                Game.Updatable.Remove(updatableShell);
            });
            shell.Commands.Add(TypeCommand.Move, new CommandMoveSell(shell, scene, action));
            shell.Properties[PropertiesType.Owner] = Unit.Properties[PropertiesType.Owner];
            scene.Add(shell);
            return shell;
        }
    }
}
