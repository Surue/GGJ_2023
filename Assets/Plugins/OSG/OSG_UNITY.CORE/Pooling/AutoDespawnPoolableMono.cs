// Old Skull Games
// antoinepastor  
// Friday, 29 September 2017

using System.Collections;
using UnityEngine;

namespace OSG
{
    public class AutoDespawnPoolableMono : PoolableMono
    {
        [SerializeField] public int timeToLive = 1000;
        
        public void PlanDespawn(AutoDespawnPool thePool)
        {
            StartCoroutine(DespawnAfterTTL(thePool));
        }
        
        IEnumerator DespawnAfterTTL(AutoDespawnPool thePool)
        {
            yield return new WaitForSeconds(timeToLive/1000f);
            thePool.Despawn(this);
        }
    }

}