using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public class RotateActuator : MonoBehaviour
    {
        public Vector3 RotateSpeed;

        private Rigidbody ourRidgidbody;

        private void Start()
        {
            ourRidgidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            ourRidgidbody.angularVelocity = RotateSpeed;
        }
    }
}
