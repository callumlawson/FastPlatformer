using System;
using System.Collections.Generic;
using Gameschema.Trusted;
using Gameschema.Untrusted;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.PlayerLifecycle;
using Improbable.Gdk.TransformSynchronization;
using Playground;
using UnityEngine;
using Color = Gameschema.Untrusted.Color;
using Random = UnityEngine.Random;

namespace FastPlatformer.Config.EntityTemplates
{
    public static class PlayerTemplate
    {
        public static EntityTemplate CreatePlayerEntityTemplate(string workerId, byte[] bytes)
        {
            var clientAttribute = $"workerId:{workerId}";

            //Core
            var template = new EntityTemplate();
            template.AddComponent(new Position.Snapshot(), clientAttribute);
            template.AddComponent(new Metadata.Snapshot { EntityType = "PlatformerCharacter" }, WorkerUtils.UnityGameLogic);
            TransformSynchronizationHelper.AddTransformSynchronizationComponents(
                template, clientAttribute, Vector3.one, Quaternion.identity, Vector3.one);
            PlayerLifecycleHelper.AddPlayerLifecycleComponents(template, workerId, clientAttribute, WorkerUtils.UnityGameLogic);
            template.SetReadAccess(WorkerUtils.UnityClient, WorkerUtils.UnityGameLogic, WorkerUtils.AndroidClient, WorkerUtils.iOSClient);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, WorkerUtils.UnityGameLogic);

            //Addons - Server
            template.AddComponent(new FromServerEvents.Snapshot(), WorkerUtils.UnityGameLogic);

            //Addons - Client
            template.AddComponent(new PlayerInput.Snapshot(), clientAttribute);
            template.AddComponent(new PlayerVisualizerEvents.Snapshot(), clientAttribute);
            template.AddComponent(new Color.Snapshot(Random.value, Random.value, Random.value, 1), clientAttribute);
            template.AddComponent(new Name.Snapshot(RandomNameCreator()), clientAttribute);
            template.AddComponent(new GlobalMessage.Snapshot(), clientAttribute);

            numPlayersSpawned++;

            return template;
        }

        private static int numPlayersSpawned;

        private static readonly List<string> Names = new List<string>
        {
            "Goose",
            "Cathy",  
            "Dawn",  
            "Roscoe",  
            "Gussie",  
            "Ramon",  
            "William",  
            "Kristofer",  
            "Tonya",  
            "Joellen",  
            "Riley",  
            "Noma",  
            "Hilda",  
            "Jamie",  
            "Hong",  
            "Manda",  
            "Breann",  
            "Hailey",  
            "Jannette",  
            "Yukiko",  
            "Margo",  
            "Lupita",  
            "Arnoldo",  
            "Aleida",
            "Mitzi",  
            "Kiyoko",  
            "Ardath"
        };

        private static string RandomNameCreator()
        {
            return Names[numPlayersSpawned % Names.Count];
        }
    }
}
