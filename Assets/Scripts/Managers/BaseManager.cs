using UnityEngine;

namespace Managers
{
    public abstract class BaseManager<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var objs = FindObjectsByType<T>(FindObjectsSortMode.None);
                    if (objs.Length > 0) _instance = objs[0];
                    if (objs.Length > 1) Debug.LogError("More than one instance of " + typeof(T) + " found");
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.hideFlags = HideFlags.HideAndDontSave;
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }
    }
}