// Old Skull Games
// Bernard Barthelemy
// Wednesday, November 27, 2019


using OSG.Core.EventSystem;

namespace OSG
{
    public class PlayerDataEventContainer : CoreEventContainer
    {
        public readonly IntEvent onProfileSelected = new IntEvent();
        public readonly IntEvent onProfileDeleted = new IntEvent();
        public readonly IntEvent onProfileCreated = new IntEvent();
        public readonly IntEvent onProfileRenamed = new IntEvent();
    }
}