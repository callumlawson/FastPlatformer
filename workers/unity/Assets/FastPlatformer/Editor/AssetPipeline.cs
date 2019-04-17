using FastPlatformer.Scripts.MonoBehaviours.Actuator;
using FastPlatformer.Scripts.MonoBehaviours.Visualizers;
using Improbable.Gdk.TransformSynchronization;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace FastPlatformer.Editor
{
    public static class AssetPipeline
    {
        [MenuItem("Assets/Asset Processing/Create Runtime Prefab")]
        private static void CreateRuntimePrefab()
        {
            GameObject[] selectedAssets = Selection.GetFiltered<GameObject>(SelectionMode.Assets);

            foreach (var gameObject in selectedAssets)
            {
                TryCreatePrefab(gameObject);
            }
        }

        private static void TryCreatePrefab(GameObject gameObject)
        {
            if (!IsValid(gameObject))
            {
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(gameObject);

            //Gamelogic
            var tempModel = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(assetPath));
            tempModel.AddComponent<AuthorityActuator>();
            var transformComponent = tempModel.AddComponent<TransformSynchronization>();
            var transPreset = Preset.GetDefaultForObject(transformComponent);
            transPreset.ApplyTo(transformComponent);
            var basePrefab = PrefabUtility.SaveAsPrefabAsset(tempModel, GetGamelogicPath(gameObject));

            //Client
            var tempModel2 = (GameObject) PrefabUtility.InstantiatePrefab(basePrefab);
            tempModel2.AddComponent<RuntimeEditorVisualzier>();
            Object.DestroyImmediate(tempModel2.GetComponent<AuthorityActuator>());
            PrefabUtility.SaveAsPrefabAsset(tempModel2, GetClientPath(gameObject));

            Debug.Log($"Creating prefab from {gameObject.name}");

            Object.DestroyImmediate(tempModel);
            Object.DestroyImmediate(tempModel2);
        }

        private static bool IsValid(GameObject gameObject)
        {
            if (AssetDatabase.LoadAssetAtPath(GetGamelogicPath(gameObject), typeof(GameObject)))
            {
                if (EditorUtility.DisplayDialog("Are you sure?",
                    $"A Prefab for {gameObject.name} already exists. Do you want to overwrite it?",
                    "Yes - burn it to the ground!",
                    "No - whoops."))
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        private static string GetClientPath(GameObject gameObject)
        {
            return $"Assets/FastPlatformer/Resources/Prefabs/UnityClient/{gameObject.name}.prefab";
        }

        private static string GetGamelogicPath(GameObject gameObject)
        {
            return $"Assets/FastPlatformer/Resources/Prefabs/UnityGameLogic/{gameObject.name}.prefab";
        }

        private static string GetSourcePath(GameObject gameObject)
        {
            return $"Assets/FastPlatformer/Resources/Prefabs/UnityClient/{gameObject.name}.prefab";
        }
    }
}
