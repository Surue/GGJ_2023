// Old Skull Games
// Bernard Barthelemy
// Wednesday, November 27, 2019


using System;
using OSG.Core.EventSystem;
using OSG.EventSystem;

namespace OSG
{
    [Serializable]
    public class ShakeEvent : GameEvent<ShakeParameters>{}   
    public class CameraShakeEventContainer : CoreEventContainer
    {
        public readonly ShakeEvent cameraShake = new ShakeEvent();
        public readonly GameEvent cameraStopShake = new GameEvent();
    }
}