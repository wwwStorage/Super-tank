﻿namespace SuperTank
{
    /// <summary>
    /// The appearance of the tank on stage
    /// </summary>
    public class Star : UpdatableUnit
    {
        private Tank tank;
        private int delay;

        public Star(int id, int x, int y, int width, int height, TypeUnit type, Tank tank) : base(id, x, y, width, height, type)
        {
            this.tank = tank;
            this.Properties[PropertiesType.Owner] = tank.Properties[PropertiesType.Owner];
        }

        public override void Start()
        {
            base.Start();
            AddToScene();
            Scene.Stars.Add(this);
        }
        public override void Update()
        {
            if (delay == ConfigurationGame.TimeAppearanceOfTank)
                Dispose();
            else delay++;
        }
        public override void Dispose()
        {
            Scene.Stars.Remove(this);
            tank.Start();
            tank.IsParking = !tank.IsParking;
            base.Dispose();
        }
    }
}
