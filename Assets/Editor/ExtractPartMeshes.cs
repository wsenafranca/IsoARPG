using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class ExtractPartMeshes : EditorWindow
    {
        public GameObject objectSource;
        public Transform rootBone;
        public Material[] materialSource;
        public string filePrefix;

        private SerializedObject _serializedObject;
        
        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
        }

        [MenuItem("Assets/Create/Extract Part Meshes")]
        private static void ShowWindow()
        {
            var window = GetWindow<ExtractPartMeshes>();
            window.titleContent = new GUIContent("Extract Part Meshes");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select a model");
            objectSource = (GameObject)EditorGUILayout.ObjectField(objectSource, typeof(GameObject), true);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select root bone");
            rootBone = (Transform)EditorGUILayout.ObjectField(rootBone, typeof(Transform), true);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select a material to apply in the meshes");
            _serializedObject.Update();
            var materialProperty = _serializedObject.FindProperty("materialSource");
            EditorGUILayout.PropertyField(materialProperty, true);
            _serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Insert a prefix");
            filePrefix = EditorGUILayout.TextField("File prefix", filePrefix);
            
            if (GUILayout.Button("Extract"))
            {
                var path = EditorUtility.OpenFolderPanel("Select a folder", "Prefabs", "");
                Extract(path);
            }
            
            Repaint();
        }

        private void Extract(string path)
        {
            if (rootBone == null || objectSource == null || path.Length == 0) return;

            var bounds = new Bounds();
            
            foreach (var renderer in rootBone.root.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                bounds.Encapsulate(renderer.localBounds);
            }

            var renderers = objectSource.GetComponentsInChildren<SkinnedMeshRenderer>();
            var i = 0;
            foreach (var renderer in renderers)
            {
                var prefabName = filePrefix + "_" + i.ToString().PadLeft(2, '0');
                var prefabObject = new GameObject(prefabName);
                
                var rootObject = Instantiate(rootBone, prefabObject.transform);
                rootObject.name = rootBone.name;
                
                var rendererObject = Instantiate(renderer, prefabObject.transform);
                rendererObject.name = renderer.name;

                var boneMap = new Dictionary<string, Transform>();
                foreach (var bone in rootObject.GetComponentsInChildren<Transform>())
                {
                    boneMap[bone.name] = bone;
                }

                var bones = new Transform[renderer.bones.Length];
                for (var j = 0; j < bones.Length; j++)
                {
                    if (boneMap.ContainsKey(renderer.bones[j].name))
                    {
                        bones[j] = boneMap[renderer.bones[j].name];
                    }
                    else
                    {
                        Debug.Log("Not found " + renderer.bones[j].name);
                    }
                }

                rendererObject.bones = bones;
                rendererObject.rootBone = rootObject;
                rendererObject.sharedMaterials = materialSource;
                rendererObject.localBounds = bounds;
                rendererObject.updateWhenOffscreen = true;
                
                var localPath = FileUtil.GetProjectRelativePath(path) + "/" + prefabName + ".prefab";
                localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
                PrefabUtility.SaveAsPrefabAsset(prefabObject, localPath);
                
                DestroyImmediate(prefabObject);
                
                i++;
            }
        }
    }
}