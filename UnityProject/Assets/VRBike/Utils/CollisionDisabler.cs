using System.Collections;

using UnityEngine;

namespace MedRoad.Utils
{
    /// <summary>
    /// A helper class that waits for two given colliders to become active, and then disables
    /// collisions between them.
    /// </summary>
    public class CollisionDisabler
    {
        private const int TIMEOUT_IN_FRAMES = 900; // ~15 seconds @ 60 FPS

        private int time = 0;
        private Collider obj1;
        private Collider obj2;

        /// <summary>
        /// Create a new instance with the given two colliders.
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        public CollisionDisabler(Collider obj1, Collider obj2)
        {
            if (obj1 == null || obj2 == null)
            {
                Debug.LogWarning("[CollisionDisabler] Given null object, cannot disable collision.");
                return;
            }

            this.obj1 = obj1;
            this.obj2 = obj2;
        }

        /// <summary>
        /// If both colliders given when instantiating were non-null, starts a coroutine that
        /// waits for both to become active and then disabled collisions between them. If both
        /// do not become active before <see cref="TIMEOUT_IN_FRAMES"/>, it will timeout.
        /// </summary>
        public void Start()
        {
            if (obj1 != null && obj2 != null)
                if (ThreadHelper.SingletonCheck("CollisionDisabler"))
                    ThreadHelper.Singleton.StartCoroutine(WaitForActiveColliders());     
        }

        /// <summary>
        /// Waits for both colliders to be active and then disables collision between them. It
        /// may timeout.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForActiveColliders()
        {
            while ((!obj1.enabled || !obj2.enabled) && ++time < TIMEOUT_IN_FRAMES)
                yield return new WaitForEndOfFrame();

            if (obj1.enabled && obj2.enabled)
                Physics.IgnoreCollision(obj1, obj2);
            else
                Debug.LogWarning("[CollisionDisabler] Timed out, colliders never became enabled.");
        }

        /// <summary>
        /// A helper method that gets the TerrainCollider component of the active terrain, or
        /// <c>null</c> otherwise.
        /// </summary>
        /// <returns>The TerrainCollider component of the active terrain, or <c>null</c> if either
        /// their is no active terrain or the active terrain has no TerrainCollider.</returns>
        public static TerrainCollider GetTerrainColliderForActiveTerrain()
        {
            if (Terrain.activeTerrain == null)
            {
                Debug.LogWarning("[GetTerrainColliderForActiveTerrain] There is no active terrain.");
                return null;
            }

            TerrainCollider terrainCollider = Terrain.activeTerrain.GetComponent<TerrainCollider>();
            if (terrainCollider == null)
            {
                Debug.LogWarning("[GetTerrainColliderForActiveTerrain] Active terrain has no terrain collider.");
                return null;
            }

            return terrainCollider;
        }

    }
}
