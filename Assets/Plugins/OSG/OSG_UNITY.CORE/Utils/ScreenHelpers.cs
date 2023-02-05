using UnityEngine;

namespace OSG
{
    
    public static class  ScreenHelpers  {
        
        public static float GetRealScreenRatio()
        {
            return GetRealScreenSize().x / GetRealScreenSize().y;
        }
        
        public static Vector2 GetRealScreenSize()
        {
#if UNITY_EDITOR
            return new Vector2(GameViewSize.GetMainGameViewSize().x , GameViewSize.GetMainGameViewSize().y);
#else
            return new Vector2((float) Screen.width ,(float) Screen.height);
#endif
        }

    }

}
