using UnityEngine;

namespace ITXCM
{
    /// <summary>
    /// Mono单例
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public bool global = true;
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null) instance = (T)FindObjectOfType<T>();
                return instance;
            }
        }

        private void Awake()
        {
            if (global)
            {
                if (instance != null && instance != gameObject.GetComponent<T>())
                {
                    Destroy(gameObject);
                    return;
                }
                DontDestroyOnLoad(gameObject);
                instance = gameObject.GetComponent<T>();
            }
            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }
    }
}