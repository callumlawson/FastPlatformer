﻿using System.Collections.Generic;
using Generated.Improbable;
using Generated.Playground;
using Improbable.Gdk.Core;
using Improbable.Gdk.Core.Commands;
using Improbable.Gdk.Core.GameObjectRepresentation;
using Improbable.Worker;
using Improbable.Worker.Core;
using UnityEngine;
using Transform = Generated.Improbable.Transform.Transform;

#region Diagnostic control

// Disable the "variable is never assigned" for injected fields.
#pragma warning disable 649

// ReSharper disable PossibleInvalidOperationException

#endregion

namespace Playground.MonoBehaviours
{
    public class SpawnCubeCommandReceiver : MonoBehaviour
    {
        [Require] private Transform.Requirables.Reader transformReader;
        [Require] private CubeSpawner.Requirables.CommandRequestHandler cubeSpawnerCommandRequestHandler;
        [Require] private CubeSpawner.Requirables.Writer cubeSpawnerWriter;
        [Require] private WorldCommands.Requirables.WorldCommandRequestSender worldCommandRequestSender;
        [Require] private WorldCommands.Requirables.WorldCommandResponseHandler worldCommandResponseHandler;

        private ILogDispatcher logDispatcher;

        public void OnEnable()
        {
            logDispatcher = GetComponent<SpatialOSComponent>().LogDispatcher;
            cubeSpawnerCommandRequestHandler.OnSpawnCubeRequest += OnSpawnCubeRequest;
            worldCommandResponseHandler.OnReserveEntityIdsResponse += OnEntityIdsReserved;
            worldCommandResponseHandler.OnCreateEntityResponse += OnEntityCreated;
        }

        private void OnSpawnCubeRequest(CubeSpawner.SpawnCube.RequestResponder requestResponder)
        {
            requestResponder.SendResponse(new Empty());

            worldCommandRequestSender.ReserveEntityIds(1, context: this);
        }

        private void OnEntityIdsReserved(WorldCommands.ReserveEntityIds.ReceivedResponse response)
        {
            if (!ReferenceEquals(this, response.Context))
            {
                // This response was not for a command from this behaviour.
                return;
            }

            var responseOp = response.Op;

            if (responseOp.StatusCode != StatusCode.Success)
            {
                logDispatcher.HandleLog(LogType.Error,
                    new LogEvent(string.Format("Failed to reserve entity id: {0}", responseOp.Message)));

                return;
            }

            var location = transformReader.Data.Location;
            var cubeEntityTemplate =
                CubeTemplate.CreateCubeEntityTemplate(new Coordinates(location.X, location.Y + 2, location.Z));
            var expectedEntityId = responseOp.FirstEntityId.Value;

            worldCommandRequestSender.CreateEntity(cubeEntityTemplate, expectedEntityId, context: this);
        }

        private void OnEntityCreated(WorldCommands.CreateEntity.ReceivedResponse response)
        {
            if (!ReferenceEquals(this, response.Context))
            {
                // This response was not for a command from this behaviour.
                return;
            }

            var createEntityResponseOp = response.Op;

            if (createEntityResponseOp.StatusCode != StatusCode.Success)
            {
                logDispatcher.HandleLog(LogType.Error,
                    new LogEvent(string.Format("Create entity (for id {0}) failed with message: \"{1}\"",
                        response.RequestPayload.EntityId,
                        createEntityResponseOp.Message)));

                return;
            }

            var spawnedCubesCopy =
                new List<EntityId>(CubeSpawnerInputBehaviour.GetSpawnedCubes(cubeSpawnerWriter.Data));
            var newEntityId = createEntityResponseOp.EntityId.Value;

            spawnedCubesCopy.Add(newEntityId);

            cubeSpawnerWriter.Send(new CubeSpawner.Update
            {
                SpawnedCubes = spawnedCubesCopy,
                NumSpawnedCubes = (uint) spawnedCubesCopy.Count
            });
        }
    }
}
