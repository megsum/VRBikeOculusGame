using UnityEngine;

namespace MedRoad.Utils
{
    /// <summary>
    /// An extension of a MonoBehaviour that enforces a singleton pattern. A static reference is
    /// maintained to the instance of the class, and new instances either update the reference or
    /// are automatically destroyed.
    /// 
    /// The type parameter should be the name of the class that extends this class. For instance,
    /// instead of <c>public class MyClass : MonoBehaviour</c> to make a singleton MonoBehaviour
    /// use <c>public class MyClass : MonoBehaviourSingleton&lt;MyClass&gt;</c>.
    /// </summary>
    /// <typeparam name="T">The name of the class that extends this class.</typeparam>
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
    {
        /// <summary>
        /// Gets the singleton for this instance.
        /// </summary>
        public static T Singleton { get; private set; }

        /// <summary>
        /// If <c>true</c>, always keep the singleton instance and destroy any newly created
        /// instances. Otherwise, set the singleton reference to the newest instance.
        /// </summary>
        [Tooltip("Prevent this instance from being destroyed during scene changes and " +
            "automatically destroy any newly created instances.")]
        public bool destroyNewInstancesOnLoad = true;

        /// <summary>
        /// Set the singleton instance. If the singleton instance already exists and
        /// <see cref="destroyNewInstancesOnLoad"/> is set to <c>true</c>, destroy this instance.
        /// </summary>
        /// <returns><c>true</c> if this instance was destroyed, <c>false</c> otherwise.</returns>
        private bool PerformDestroyOnLoad()
        {
            if (this.destroyNewInstancesOnLoad)
            {
                if (Singleton != null)
                {
                    UnityEngine.Object.Destroy(base.gameObject);
                    return true;
                }

                Singleton = (T) this;
                UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
            }
            else
            {
                Singleton = (T)this;
            }

            return false;
        }

        /// <summary>
        /// On awake we need to set the singleton and check if this instance needs to be destroyed.
        /// When inheriting SingletonMonoBehaviour<T> DO NOT HIDE THE AWAKE METHOD. Instead,
        /// override the Awake2, which will be called if the instance isn't to be destroyed.
        /// </summary>
        protected void Awake()
        {
            if (!this.PerformDestroyOnLoad())
                Awake2();
        }

        /// <summary>
        /// Override this method to perform any required Awake actions in the subclass. When
        /// inheriting SingletonMonoBehaviour<T> DO NOT HIDE THE AWAKE METHOD. Instead, override
        /// this method.
        /// </summary>
        protected virtual void Awake2() { }

    }
}
