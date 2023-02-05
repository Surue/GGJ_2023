//Created by Antoine Pastor
//Old Skull Games
//11/07/2017

namespace OSG
{
    public class AutoDespawnPool : AbstractPoolableMonoPool<AutoDespawnPoolableMono>
    {
        /// <summary>
        /// Returns the first available object in the stack
        /// </summary>
        /// <returns></returns>
        public override AutoDespawnPoolableMono Spawn()
        {
            AutoDespawnPoolableMono myPoolable = base.Spawn();
#if UNITY_EDITOR
            myPoolable.name += "In Use";
#endif
            myPoolable.OnSpawn();
            myPoolable.PlanDespawn(this);
            return myPoolable;
        }

        /// <summary>
        /// Returns the given object to the pool
        /// </summary>
        /// <param name="poolable"></param>
        public override void Despawn(AutoDespawnPoolableMono myPoolableMono)
        {
#if UNITY_EDITOR
            myPoolableMono.name = myPoolableMono.name.Replace("In Use", "");
#endif
            base.Despawn(myPoolableMono);
            myPoolableMono.OnDespawn();
        }
    }
}