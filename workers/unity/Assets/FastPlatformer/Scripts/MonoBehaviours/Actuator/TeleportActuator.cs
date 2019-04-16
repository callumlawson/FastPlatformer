using KinematicCharacterController;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public class TeleportActuator : MonoBehaviour
    {
        private int ownedPlayerLayer;

        private void Awake()
        {
            ownedPlayerLayer = LayerMask.NameToLayer("OwnedPlayer");
        }

        private void OnTriggerEnter(Collider other)
        {
            var collidingObject = other.gameObject;

            if (collidingObject.layer == ownedPlayerLayer)
            {
                collidingObject.GetComponent<KinematicCharacterMotor>().SetPosition(new Vector3(0, 15, 0));
                collidingObject.GetComponent<KinematicCharacterMotor>().BaseVelocity = Vector3.zero;
            }
        }
    }
}
