//Created by Antoine Pastor
//Old Skull Games
//11/07/2017
using System.Collections;
using OSG;

/// <summary>
/// Object with a behaviour intended to be used with the pool system
/// </summary>
public class PoolableMono : OSGMono
{
	
	/// <summary>
	/// Called when the object enters the pool or is reactivated by the pool
	/// </summary>
	public virtual void OnSpawn()
	{
            
	}
        
	/// <summary>
	/// Called when the object of the pool is not needed anymore
	/// </summary>
	public virtual void OnDespawn()
	{
          
	}

	/// <summary>
	/// Called for pooled objects
	/// </summary>
	/// <returns></returns>
	public IEnumerator PoolUpdate()
	{
		yield break;
	}

}
