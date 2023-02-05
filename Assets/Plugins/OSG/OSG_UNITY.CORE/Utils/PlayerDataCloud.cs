// Old Skull Games
// Bernard Barthelemy
// Friday, June 29, 2018

namespace OSG
{
    /// <summary>
    /// Save and load data from the Cloud.
    /// </summary>
    public class PlayerDataCloud : PlayerData
    {
        protected override void SaveStringInternal(string key, string value)
        {
            throw new System.NotImplementedException();
        }

        protected override void SaveIntInternal(string key, int value)
        {
            throw new System.NotImplementedException();
        }

        protected override void SaveFloatInternal(string key, float value)
        {
            throw new System.NotImplementedException();
        }

        protected override void SaveObjectInternal(string key, object value)
        {
            throw new System.NotImplementedException();
        }


        protected override string LoadStringInternal(string key, string defaultValue)
        {
            throw new System.NotImplementedException();
        }

        protected override int LoadIntInternal(string key, int defaultValue)
        {
            throw new System.NotImplementedException();
        }

        protected override float LoadFloatInternal(string key, float defaultValue)
        {
            throw new System.NotImplementedException();
        }

        protected override object LoadObjectInternal(string key, object defaultValue)
        {
            throw new System.NotImplementedException();
        }

        protected override bool InternalHasKey(string key)
        {
            throw new System.NotImplementedException();
        }

        protected override void InternalDelete(string key)
        {
            throw new System.NotImplementedException();
        }


    }
}