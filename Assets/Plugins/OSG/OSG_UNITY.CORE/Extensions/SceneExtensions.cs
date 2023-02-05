using UnityEngine;
using UnityEngine.SceneManagement;


namespace OSG
{
    public static class SceneExtensions {
		public static void MoveRootObjectsTo(this Scene scene, Vector3 wantedPosition)
		{
			if (!scene.isLoaded)
				UnityEngine.Debug.LogWarning("You are trying to move RootObjects of a scene that is not loaded ("+scene.name+")");
			foreach (var rootGameObject in scene.GetRootGameObjects())
			{
				rootGameObject.transform.position = wantedPosition;	
			}
		}
		
		public static void SetRootObjectsActivation(this Scene scene, bool p_active)
		{
			if (!scene.isLoaded)
				UnityEngine.Debug.LogWarning("You are trying to "+(p_active?"activate":"deactivate")+" RootObjects of a scene that is not loaded ("+scene.name+")");
			foreach (var rootGameObject in scene.GetRootGameObjects())
			{
				rootGameObject.SetActive(p_active);	
			}
		}

        public static bool ScenesAreAllLoaded()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (!SceneManager.GetSceneAt(i).isLoaded)
                    return false;
            }

            return true;
        }
    }
}
