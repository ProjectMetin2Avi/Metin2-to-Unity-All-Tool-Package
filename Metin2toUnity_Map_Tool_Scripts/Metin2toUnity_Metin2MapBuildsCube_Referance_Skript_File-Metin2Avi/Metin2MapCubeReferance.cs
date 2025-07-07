using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

public class Metin2MapCubeReferance : EditorWindow
{
    private Terrain targetTerrain;
    private string mapFolderPath = "";
    private Vector3 scaleFactor = Vector3.one;
    private Vector2 scrollPosition;
    private const float COORDINATE_SCALE = 100f;
    private bool flipX = false;
    private bool flipZ = false;

    [MenuItem("Tools/Metin2 Map Cube Referance - @Metin2Avi")]
    public static void ShowWindow()
    {
        GetWindow<Metin2MapCubeReferance>("Metin2 Map Cube Referance - @Metin2Avi");
    }

    private void OnGUI()
    {
        GUILayout.Label("Metin2 Map Cube Referance - @Metin2Avi", EditorStyles.boldLabel);

        targetTerrain = EditorGUILayout.ObjectField("Target Terrain", targetTerrain, typeof(Terrain), true) as Terrain;

        EditorGUILayout.BeginHorizontal();
        mapFolderPath = EditorGUILayout.TextField("Map Folder Path", mapFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Map Folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                mapFolderPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        flipX = EditorGUILayout.Toggle("Flip X Coordinates", flipX);
        flipZ = EditorGUILayout.Toggle("Flip Z Coordinates", flipZ);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Import All Objects"))
        {
            if (targetTerrain == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a target terrain!", "OK");
                return;
            }

            if (string.IsNullOrEmpty(mapFolderPath) || !Directory.Exists(mapFolderPath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid map folder!", "OK");
                return;
            }

            ImportAllObjects();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Instructions:\n" +
            "1. Assign your terrain\n" +
            "2. Select your map folder containing areadata files\n" +
            "3. Adjust flip settings if needed\n" +
            "4. Click 'Import All Objects'\n" +
            "Note: Objects will be scaled and positioned relative to terrain size",
            MessageType.Info
        );
    }

    private void ImportAllObjects()
    {
        GameObject parentObject = new GameObject("Metin2_All_Referances_Cube_Objects");
        int totalObjectsImported = 0;

        // Terrain boyutlarý
        Vector3 terrainPos = targetTerrain.transform.position;
        Vector3 terrainSize = targetTerrain.terrainData.size;


        float scaleFactor = 131f / 256f; // 131 / 256 = ~0.511 size factor

        string[] subDirectories = Directory.GetDirectories(mapFolderPath, "*", SearchOption.AllDirectories);

        foreach (string dir in subDirectories)
        {
            string areadataPath = Path.Combine(dir, "areadata.txt");
            if (File.Exists(areadataPath))
            {
                string folderName = new DirectoryInfo(dir).Name;
                GameObject sectorContainer = new GameObject($"Sector_{folderName}");
                sectorContainer.transform.parent = parentObject.transform;

                string[] lines = File.ReadAllLines(areadataPath);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (line.StartsWith("Start Object"))
                    {
                        string objectName = line.Replace("Start Object", "").Trim();

                        string[] positionData = lines[i + 1].Trim().Split(' ');
                        if (positionData.Length >= 3)
                        {
                            // Koordinatlarý Metin2 formatýndan Unity formatýna çevirelim
                            float originalX = float.Parse(positionData[0], CultureInfo.InvariantCulture);
                            float originalY = float.Parse(positionData[1], CultureInfo.InvariantCulture);
                            float originalZ = float.Parse(positionData[2], CultureInfo.InvariantCulture);

                            // Koordinatlarý ölçeklendirelim
                            float scaledX = (originalX / COORDINATE_SCALE) * scaleFactor;
                            float scaledZ = (-originalY / COORDINATE_SCALE) * scaleFactor; // Y'yi Z'ye çeviriyoruz

                            if (flipX) scaledX = -scaledX;
                            if (flipZ) scaledZ = -scaledZ;

                            GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            newObject.name = $"Object{objectName}_X{originalX:F2}_Z{originalY:F2}";
                            newObject.transform.parent = sectorContainer.transform;

                            // Terrain merkezine göre pozisyonlama
                            Vector3 relativePosition = new Vector3(
                                terrainPos.x + (scaledX + terrainSize.x / 2),
                                terrainPos.y + originalZ / COORDINATE_SCALE, // Yükseklik
                                terrainPos.z + (scaledZ + terrainSize.z / 2)
                            );

                            newObject.transform.position = relativePosition;

                            // Rotasyonu ayarla
                            Vector3 rotation = Vector3.zero;
                            string[] rotationData = lines[i + 3].Trim().Split('#');
                            if (rotationData.Length >= 3)
                            {
                                rotation = new Vector3(
                                    float.Parse(rotationData[0], CultureInfo.InvariantCulture),
                                    float.Parse(rotationData[2], CultureInfo.InvariantCulture),
                                    float.Parse(rotationData[1], CultureInfo.InvariantCulture)
                                );
                            }

                            if (flipZ)
                            {
                                rotation.y = 180f - rotation.y;
                            }

                            newObject.transform.eulerAngles = rotation;
                            totalObjectsImported++;
                        }
                    }
                }
            }
        }

        EditorUtility.DisplayDialog("Success", $"All objects imported successfully!\nTotal objects imported: {totalObjectsImported}", "OK");
    }
}