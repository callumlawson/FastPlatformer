using System;
using Generated.Improbable;
using Generated.Playground;
using Improbable.Gdk.Core;
using Improbable.Gdk.Core.GameObjectRepresentation;
using Playground.Scripts.UI;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

#region Diagnostic control

#pragma warning disable 649
// ReSharper disable UnassignedReadonlyField
// ReSharper disable UnusedMember.Global

#endregion

namespace Playground
{
    [UpdateInGroup(typeof(SpatialOSUpdateGroup))]
    public class PlayerCommandsSystem : ComponentSystem
    {
        private enum PlayerCommand
        {
            // ReSharper disable once UnusedMember.Local
            None,
            LaunchSmall,
            LaunchLarge
        }

        private const float LargeEnergy = 50.0f;
        private const float SmallEnergy = 10.0f;


        private struct PlayerData
        {
            public readonly int Length;
            [ReadOnly] public ComponentDataArray<SpatialEntityId> SpatialEntity;
            [ReadOnly] public ComponentDataArray<Authoritative<SpatialOSPlayerInput>> PlayerInputAuthority;
            [ReadOnly] public ComponentDataArray<LocalInput> ShootInput;
            [ReadOnly] public ComponentDataArray<Launcher.CommandSenders.LaunchEntity> Sender;
        }

        [Inject] private PlayerData playerData;

        protected override void OnUpdate()
        {
            if (playerData.Length > 1)
            {
                throw new InvalidOperationException($"Expected at most 1 playerData, got: {playerData.Length}");
            }

            PlayerCommand command;
            var input = playerData.ShootInput[0];
            if (input.ShootSmall)
            {
                command = PlayerCommand.LaunchSmall;
            }
            else if (input.ShootLarge)
            {
                command = PlayerCommand.LaunchLarge;
            }
            else
            {
                return;
            }

            var ray = Camera.main.ScreenPointToRay(UIComponent.Main.Reticle.transform.position);
            if (!Physics.Raycast(ray, out var info) || info.rigidbody == null)
            {
                return;
            }

            var rigidBody = info.rigidbody;
            var sender = playerData.Sender[0];
            var playerId = playerData.SpatialEntity[0].EntityId;

            var component = rigidBody.gameObject.GetComponent<SpatialOSComponent>();

            if (component == null || !EntityManager.HasComponent(component.Entity, typeof(Launchable.Component)))
            {
                return;
            }

            var impactPoint = new Vector3f(info.point.x, info.point.y, info.point.z);
            var launchDirection = new Vector3f(ray.direction.x, ray.direction.y, ray.direction.z);

            sender.RequestsToSend.Add(Launcher.LaunchEntity.CreateRequest(playerId,
                new LaunchCommandRequest(component.SpatialEntityId, impactPoint, launchDirection,
                    command == PlayerCommand.LaunchLarge ? LargeEnergy : SmallEnergy,
                    playerId
                )));

            playerData.Sender[0] = sender;
        }
    }
}
