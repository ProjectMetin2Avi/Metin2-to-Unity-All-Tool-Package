using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class TextureToTerrainLayerTool : EditorWindow
{
    private string sourceTextureFolder = "";
    private string targetLayerFolder = "";
    private Vector2 scrollPosition;
    private List<string> textureFiles = new List<string>();
    private bool showTextureList = false;

    [MenuItem("Tools/Texture to Terrain Layer Tool")]
    public static void ShowWindow()
    {
        GetWindow<TextureToTerrainLayerTool>("Texture to Terrain Layer Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Texture to Terrain Layer Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Source folder selection
        GUILayout.Label("Texture Dosyalarının Bulunduğu Klasör:", EditorStyles.label);
        GUILayout.BeginHorizontal();
        sourceTextureFolder = EditorGUILayout.TextField(sourceTextureFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Texture Klasörü Seç", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                sourceTextureFolder = GetRelativeAssetPath(selectedPath);
                RefreshTextureList();
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Target folder selection
        GUILayout.Label("Terrain Layer'ların Oluşturulacağı Klasör:", EditorStyles.label);
        GUILayout.BeginHorizontal();
        targetLayerFolder = EditorGUILayout.TextField(targetLayerFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Terrain Layer Klasörü Seç", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                targetLayerFolder = GetRelativeAssetPath(selectedPath);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Texture list display
        if (!string.IsNullOrEmpty(sourceTextureFolder))
        {
            if (GUILayout.Button("Texture Listesini Yenile"))
            {
                RefreshTextureList();
            }

            if (textureFiles.Count > 0)
            {
                showTextureList = EditorGUILayout.Foldout(showTextureList, $"Bulunan Texture Dosyaları ({textureFiles.Count})");

                if (showTextureList)
                {
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                    foreach (string texturePath in textureFiles)
                    {
                        string layerName = GenerateLayerName(texturePath);
                        EditorGUILayout.LabelField($"Texture: {Path.GetFileName(texturePath)} → Layer: {layerName}");
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Seçilen klasörde texture dosyası bulunamadı.", MessageType.Warning);
            }
        }

        GUILayout.Space(20);

        // Create button
        GUI.enabled = !string.IsNullOrEmpty(sourceTextureFolder) && !string.IsNullOrEmpty(targetLayerFolder) && textureFiles.Count > 0;
        if (GUILayout.Button("Terrain Layer'ları Oluştur", GUILayout.Height(40)))
        {
            CreateTerrainLayers();
        }
        GUI.enabled = true;

        GUILayout.Space(10);

        // Help text
        EditorGUILayout.HelpBox(
            "Bu tool, seçilen klasördeki texture dosyalarını Terrain Layer'a dönüştürür.\n" +
            "Layer isimleri 'klasör_adı_texture_ismi' formatında oluşturulur.\n" +
            "Örnek: 'b_grass 01', 'b_grass 02' vb.",
            MessageType.Info
        );
    }

    private string GetRelativeAssetPath(string absolutePath)
    {
        if (absolutePath.StartsWith(Application.dataPath))
        {
            return "Assets" + absolutePath.Substring(Application.dataPath.Length);
        }
        return absolutePath;
    }

    private void RefreshTextureList()
    {
        textureFiles.Clear();

        if (string.IsNullOrEmpty(sourceTextureFolder) || !Directory.Exists(sourceTextureFolder))
            return;

        string[] supportedExtensions = { ".png", ".jpg", ".jpeg", ".tga", ".bmp", ".tiff", ".psd" };
        string[] allFiles = Directory.GetFiles(sourceTextureFolder, "*.*", SearchOption.AllDirectories);

        foreach (string file in allFiles)
        {
            string extension = Path.GetExtension(file).ToLower();
            if (System.Array.Exists(supportedExtensions, ext => ext == extension))
            {
                textureFiles.Add(file);
            }
        }
    }

    private string GenerateLayerName(string texturePath)
    {
        string textureName = Path.GetFileNameWithoutExtension(texturePath);

        // Find the reference folder after "Metin2_to_Unity_Terrainmaps"
        string normalizedPath = texturePath.Replace('\\', '/');
        string referenceFolderName = "";

        // Look for "Metin2_to_Unity_Terrainmaps" in the path
        int terrainmapsIndex = normalizedPath.IndexOf("Metin2_to_Unity_Terrainmaps");
        if (terrainmapsIndex >= 0)
        {
            // Get the part after "Metin2_to_Unity_Terrainmaps"
            string afterTerrainmaps = normalizedPath.Substring(terrainmapsIndex + "Metin2_to_Unity_Terrainmaps".Length);
            afterTerrainmaps = afterTerrainmaps.TrimStart('/');

            // Get the first folder after Metin2_to_Unity_Terrainmaps
            string[] pathParts = afterTerrainmaps.Split('/');
            if (pathParts.Length > 0 && !string.IsNullOrEmpty(pathParts[0]))
            {
                referenceFolderName = pathParts[0];
            }
        }

        // If we couldn't find the reference folder, fall back to the immediate parent folder
        if (string.IsNullOrEmpty(referenceFolderName))
        {
            string relativePath = texturePath.Replace(sourceTextureFolder, "").TrimStart('\\', '/');
            string folderName = Path.GetDirectoryName(relativePath);

            if (!string.IsNullOrEmpty(folderName))
            {
                string[] folderParts = folderName.Split('\\', '/');
                referenceFolderName = folderParts[folderParts.Length - 1];
            }
            else
            {
                referenceFolderName = Path.GetFileName(sourceTextureFolder);
            }
        }

        return $"{referenceFolderName}_{textureName}";
    }

    private void CreateTerrainLayers()
    {
        if (!Directory.Exists(targetLayerFolder))
        {
            Directory.CreateDirectory(targetLayerFolder);
        }

        int successCount = 0;
        int failCount = 0;

        foreach (string texturePath in textureFiles)
        {
            try
            {
                // Load the texture
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null)
                {
                    Debug.LogError($"Texture yüklenemedi: {texturePath}");
                    failCount++;
                    continue;
                }

                // Generate layer name
                string layerName = GenerateLayerName(texturePath);

                // Create terrain layer
                TerrainLayer terrainLayer = new TerrainLayer();
                terrainLayer.diffuseTexture = texture;
                terrainLayer.tileSize = new Vector2(15, 15); // Default tile size
                terrainLayer.tileOffset = Vector2.zero;

                // Save the terrain layer
                string layerPath = Path.Combine(targetLayerFolder, layerName + ".terrainlayer");
                layerPath = layerPath.Replace('\\', '/');

                AssetDatabase.CreateAsset(terrainLayer, layerPath);
                successCount++;

                Debug.Log($"Terrain Layer oluşturuldu: {layerName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Hata oluştu - {texturePath}: {e.Message}");
                failCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "İşlem Tamamlandı",
            $"Terrain Layer oluşturma işlemi tamamlandı!\n\nBaşarılı: {successCount}\nHatalı: {failCount}",
            "Tamam"
        );
    }
}
