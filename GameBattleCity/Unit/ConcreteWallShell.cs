﻿using SuperTank.Audio;

namespace SuperTank
{
    /// <summary>
    /// A shell that penetrates armored walls
    /// </summary>
    class ConcreteWallShell : PlayerShell
    {
        public ConcreteWallShell(int id, int x, int y, int width, int height, TypeUnit type, int velosity, Direction direction, TankPlayer ownerTank, ISoundGame soundGame) : base(id, x, y, width, height, type, velosity, direction, ownerTank, soundGame)
        {
        }

        protected override void Detonation(Unit item, bool removeItem)
        {
            if (item != null && item.Type == TypeUnit.ConcreteWall)
                base.Detonation(item, true);
            else base.Detonation(item, removeItem);
        }
    }
}
