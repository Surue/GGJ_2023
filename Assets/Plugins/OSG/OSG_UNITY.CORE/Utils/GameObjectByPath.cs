//Created by Antoine Pastor
//Old Skull Games
//31/08/2017

using System;
using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Lets you save the path of a gameobject in a scene rather than the object itself
    /// Useful for cross scene references or timeline parameters 
    /// </summary>
    [Serializable]
    public class GameObjectByPath  {
        [SerializeField] public string gameobjectName;
        private GameObject _gameObject;

        public GameObject Get()
        {
            if (_gameObject == null)
            {
                if (string.IsNullOrEmpty(gameobjectName))
                    _gameObject = null;
                else 
                    _gameObject = GameObject.Find(gameobjectName);
            }
            return _gameObject;
        }

        public void Set(GameObject go)
        {
            if (go != null)
            {
                _gameObject = go;
                gameobjectName = go.name;
            }
        }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(gameobjectName))
                return true;
            else if (Get() == null)
                return false;
            else return true;
        }
    }
}
