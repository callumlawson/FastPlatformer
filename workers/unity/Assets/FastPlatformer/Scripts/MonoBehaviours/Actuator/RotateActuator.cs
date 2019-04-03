using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public class RotateActuator : MonoBehaviour
    {
        public Vector3 RotateSpeed;

        private Rigidbody ourRigidbody;

        private void Start()
        {
            ourRigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (!ourRigidbody)
            {
                transform.Rotate(RotateSpeed * Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (ourRigidbody)
            {
                ourRigidbody.angularVelocity = RotateSpeed;
            }
        }
    }
}
