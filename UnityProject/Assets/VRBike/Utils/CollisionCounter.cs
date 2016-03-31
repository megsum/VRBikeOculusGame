using UnityEngine;

namespace VRBike.Utils
{
    public class CollisionCounter : MonoBehaviour
    {
        private TrialManager _trialManager;

        private void Awake()
        {
            _trialManager = GameObject.Find("TrialManager").GetComponent<TrialManager>();
        }

        //Increases collision counter upon entering a trigger
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "IgnoreCollision") return;

            _trialManager.OnCollision();
        }
    }
}