// Old Skull Games
// Pierre Planeau
// Monday, December 09, 2019

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OSG.Services
{
    [InitializeOnLoadAttribute]
    public static class EditorDynamicServicesStarter
	{
		static EditorDynamicServicesStarter()
        {
            //EditorApplication.playModeStateChanged += OnPlayModeChanged;

            DynamicServiceManager.AddEditorDynamicServices();
        }

        
    }
}
