using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ExtractTextures : EditorWindow
{
    [MenuItem("Tools/Extract Scene Textures")]
    static void Extract()
    {
        string outputFolder = "Assets/ExtractedTextures";
        if (!AssetDatabase.IsValidFolder(outputFolder))
            AssetDatabase.CreateFolder("Assets", "ExtractedTextures");

        HashSet<Texture> textures = new HashSet<Texture>();

        // Get the current open scene
        var scene = EditorSceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(scene.path))
        {
            Debug.LogError("No scene is open or the scene is not saved.");
            return;
        }

        // Find all GameObjects in the scene
        foreach (var go in scene.GetRootGameObjects())
        {
            // Get renderers and their materials
            foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat != null)
                    {
                        foreach (var prop in mat.GetTexturePropertyNames())
                        {
                            Texture tex = mat.GetTexture(prop);
                            if (tex != null) textures.Add(tex);
                        }
                    }
                }
            }
        }

        // Copy textures to output folder
        foreach (var tex in textures)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            if (!string.IsNullOrEmpty(path))
            {
                string newPath = $"{outputFolder}/{Path.GetFileName(path)}";
                AssetDatabase.CopyAsset(path, newPath);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"Extracted {textures.Count} textures to {outputFolder} for scene: {scene.name}");
    }
}