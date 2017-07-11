﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTankCore
{
    class Tank : Unit, ITank
    {
        private bool isParking;
        // Cтарое направление движения
        private Direction prevDirection;
        private Direction newDirection;

        private IDriwer driver;
        private IShooter shooter;
        private ICannon cannon;
        private IColision colision;

        public Tank(int x, int y, int width, int height, Direction direction, TypeObjGame type, int velosity)
            : base(x, y, width, height, direction, type, velosity)
        {
            isParking = true;
            newDirection = prevDirection = Direction;
            Level.UpdateObjects.Add(this);
        }


        public IDriwer Driver { set { driver = value; } }
        public ICannon Cannon
        {
            set
            {
                cannon = value;
                if(shooter != null)
                    shooter.Cannon = cannon;
            }
        }
        public IShooter Shooter
        {
            set
            {
                shooter = value;
                if (shooter != null)
                    shooter.Cannon = cannon;
            }
        }
        public IColision Colision { set { colision = value; } }

        public void Update()
        {
            if (shooter != null)
                shooter.Update();

            prevDirection = Direction;
            isParking = true;
            if (driver != null)
                driver.Update();
            newDirection = Direction;
            Direction = prevDirection;

            // если изменили направление
            if (prevDirection != newDirection)
            {
                // если розвеонудлся на 180 градусов
                int tmp = (int)(prevDirection - newDirection);
                if (tmp == 2 || tmp == -2)
                    Direction = newDirection;

                else if (offsetToBorderTile())
                    Direction = newDirection;
            }

            if (isParking)
                offsetToBorderTile();
        }

        public override void Move()
        {
            base.Move();
            isParking = false;
            if (colision != null)
                colision.Update();
        }
        public override void Dispose()
        {
            base.Dispose();
            Level.UpdateObjects.Remove(this);
        }

        /// <summary>
        /// Смищение к границам тайла
        /// </summary>
        private bool offsetToBorderTile()
        {
            switch (Direction)
            {
                case Direction.Left:
                    return OffsetToLeft();
                case Direction.Up:
                    return OffsetToUp();
                case Direction.Right:
                    return OffsetToRight();
                case Direction.Down:
                    return OffsetToDown();
            }
            return false;
        }
        private bool OffsetToDown()
        {
            int offset = Configuration.HeightTile - Y % Configuration.HeightTile;
            if (offset == Configuration.HeightTile)
                return true;

            if (offset >= Velosity) Y += Velosity;
            else Y += offset;
            return false;
        }
        private bool OffsetToRight()
        {
            int offset = Configuration.WidthTile - X % Configuration.WidthTile;
            if (offset == Configuration.WidthTile)
                return true;

            if (offset >= Velosity) X += Velosity;
            else X += offset;
            return false;
        }
        private bool OffsetToUp()
        {
            int offset = Y % Configuration.HeightTile;
            if (offset == 0)
                return true;

            if (offset >= Velosity) Y -= Velosity;
            else Y -= offset;
            return false;
        }
        private bool OffsetToLeft()
        {
            int offset = X % Configuration.WidthTile;
            if (offset == 0)
                return true;

            if (offset >= Velosity) X -= Velosity;
            else X -= offset;
            return false;
        }
    }
}
