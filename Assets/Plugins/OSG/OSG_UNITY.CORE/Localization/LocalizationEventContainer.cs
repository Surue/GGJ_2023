// Old Skull Games
// Bernard Barthelemy
// Wednesday, November 27, 2019

using OSG.Core.EventSystem;
using OSG.EventSystem;

namespace OSG
{
    public class LocalizationEventContainer : CoreEventContainer
    {
        public readonly GameEvent localizationLoaded = new GameEvent();
        public readonly GameEvent localizationChanged = new GameEvent();
    }
}