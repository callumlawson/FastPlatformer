using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using System;
using FastPlatformer.Scripts.MonoBehaviours;

namespace KinematicCharacterController.Examples
{
    public class PlanetManager : BaseMoverController
    {
        public PhysicsMover PlanetMover;
        public SphereCollider GravityField;
        public float GravityStrength = 10;
        public Vector3 OrbitAxis = Vector3.forward;
        public float OrbitSpeed = 10;
        public Teleporter OnPlaygroundTeleportingZone;
        public Teleporter OnPlanetTeleportingZone;

        private List<PlatformerCharacterController> _characterControllersOnPlanet = new List<PlatformerCharacterController>();
        private Vector3 _savedGravity;
        private Quaternion _lastRotation;

        private void Start()
        {
            OnPlaygroundTeleportingZone.OnCharacterTeleport -= ControlGravity;
            OnPlaygroundTeleportingZone.OnCharacterTeleport += ControlGravity;

            OnPlanetTeleportingZone.OnCharacterTeleport -= UnControlGravity;
            OnPlanetTeleportingZone.OnCharacterTeleport += UnControlGravity;

            _lastRotation = PlanetMover.transform.rotation;
        }

        public override void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            goalPosition = Mover.Rigidbody.position;

            // Rotate
            Quaternion targetRotation = Quaternion.Euler(OrbitAxis * OrbitSpeed * deltaTime) * _lastRotation;
            goalRotation = targetRotation;
            _lastRotation = targetRotation;

            // Apply gravity to characters
            foreach (PlatformerCharacterController cc in _characterControllersOnPlanet)
            {
                cc.BaseGravity = (transform.position - cc.transform.position).normalized * GravityStrength;
            }
        }

        void ControlGravity(PlatformerCharacterController cc)
        {
            _savedGravity = cc.BaseGravity;
            _characterControllersOnPlanet.Add(cc);
        }

        void UnControlGravity(PlatformerCharacterController cc)
        {
            cc.BaseGravity = _savedGravity;
            _characterControllersOnPlanet.Remove(cc);
        }
    }
}