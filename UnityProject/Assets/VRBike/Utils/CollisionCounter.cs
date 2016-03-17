using UnityEngine;

namespace VRBike.Utils
{
    public class CollisionCounter : MonoBehaviour
    {
        private Trial _trial;

        private void Awake()
        {
            _trial = GameObject.Find("TrialManager").GetComponent<Trial>();
        }

        //Increases collision counter upon entering a trigger
        private void OnTriggerEnter(Collider other)
        {
            _trial.OnCollision();
        }
    }
}