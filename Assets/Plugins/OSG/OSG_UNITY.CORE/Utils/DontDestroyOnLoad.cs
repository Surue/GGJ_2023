
using System.Collections;
using OSG;

public class DontDestroyOnLoad : OSGMono {
	
	public override IEnumerator Init()
	{
		DontDestroyOnLoad(gameObject);
		return base.Init();
	}
}
