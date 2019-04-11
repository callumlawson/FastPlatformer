using KinematicCharacterController;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public class TeleportActuator : MonoBehaviour
    {
        public Transform TargetMarker;

        private int ownedPlayerLayer;

        private void Awake()
        {
            ownedPlayerLayer = LayerMask.NameToLayer("OwnedPlayer");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TargetMarker == null)
            {
                Debug.LogWarning("TeleportActuator script is missing a target location", this);
            }

            var collidingObject = other.gameObject;

            if (collidingObject.layer == ownedPlayerLayer)
            {
                collidingObject.GetComponent<KinematicCharacterMotor>().SetPosition(TargetMarker.transform.position);
                collidingObject.GetComponent<KinematicCharacterMotor>().BaseVelocity = Vector3.zero;
            }
        }
    }
}
