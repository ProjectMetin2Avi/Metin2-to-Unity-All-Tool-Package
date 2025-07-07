using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;

public class Metin2TerrainLayerTransferTool : EditorWindow
{
    private Terrain sourceTerrain;
    [SerializeField] private List<Terrain> targetTerrains = new List<Terrain>();
    private bool autoDetectTerrains = true;
    private bool excludeSourceFromTargets = true;

    private SerializedObject serializedObject;
    private SerializedProperty targetTerrainsProp;
    private ReorderableList targetTerrainsList;
    private Vector2 scrollPosition = Vector2.zero;

    [MenuItem("Tools/Metin2 Terrain Layer Transfer - @Metin2Avi")]
    public static void ShowWindow()
    {
        GetWindow<Metin2TerrainLayerTransferTool>("Metin2 Terrain Layer Transfer - @Metin2Avi");
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        targetTerrainsProp = serializedObject.FindProperty("targetTerrains");

        targetTerrainsList = new ReorderableList(serializedObject, targetTerrainsProp,
            true, true, true, true);

        targetTerrainsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var element = targetTerrainsProp.GetArrayElementAtIndex(index);
            EditorGUI.ObjectField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element,
                GUIContent.none
            );
        };

        targetTerrainsList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Target Terrains");
        };
    }

    [Header("Social Links")]
    [SerializeField] private string iconsFolderPath = "Assets/Tools/Icons";
    [SerializeField] private Texture2D GitHubIcon;
    [SerializeField] private Texture2D InstagramIcon;
    [SerializeField] private Texture2D DiscordIcon;
    [SerializeField] private Texture2D YouTubeIcon;
    [SerializeField] private Texture2D Metin2DownloadsIcon;
    [SerializeField] private Texture2D M2DevIcon;
    [SerializeField] private Texture2D TurkmmoIcon;
    private readonly string GitHubURL = "https://github.com/ProjectMetin2Avi";
    private readonly string instagramURL = "https://www.instagram.com/metin2.avi/";
    private readonly string discordURL = "https://discord.gg/WZMzMgPp38";
    private readonly string youtubeURL = "https://www.youtube.com/@project_avi";
    private readonly string Metin2DownloadsURL = "https://www.metin2downloads.to/cms/user/30621-metin2avi/";
    private readonly string M2DevURL = "https://metin2.dev/profile/53064-metin2avi/";
    private readonly string TurkmmoURL = "https://forum.turkmmo.com/uye/165187-trmove/";

    void OnGUI()
    {
        // Scroll view başlat
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        DrawSocialLinks();
        GUILayout.Label("Metin2 Terrain Layer Transfer - @Metin2Avi", EditorStyles.boldLabel);

        sourceTerrain = (Terrain)EditorGUILayout.ObjectField("Source Terrain", sourceTerrain, typeof(Terrain), true);

        EditorGUILayout.Space(10);
        
        // Otomatik algılama ayarları
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Auto Detection Settings", EditorStyles.boldLabel);
        autoDetectTerrains = EditorGUILayout.Toggle("Auto Detect Terrains", autoDetectTerrains);
        excludeSourceFromTargets = EditorGUILayout.Toggle("Exclude Source from Targets", excludeSourceFromTargets);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto Detect All Terrains", GUILayout.Height(25)))
        {
            AutoDetectTerrains();
        }
        if (GUILayout.Button("Clear Target List", GUILayout.Height(25)))
        {
            ClearTargetTerrains();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Terrain listesi için scroll area
        EditorGUILayout.LabelField($"Target Terrains ({targetTerrains.Count})", EditorStyles.boldLabel);
        
        serializedObject.Update();
        targetTerrainsList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Transfer Layers to All Targets", GUILayout.Height(30)))
        {
            TransferLayers();
        }
        
        // Scroll view bitir
        EditorGUILayout.EndScrollView();
    }

    void TransferLayers()
    {
        if (sourceTerrain == null)
        {
            Debug.LogError("Source terrain must be assigned!");
            return;
        }

        // Otomatik algılama aktifse ve hedef liste boşsa, otomatik algıla
        if (autoDetectTerrains && (targetTerrains == null || targetTerrains.Count == 0))
        {
            AutoDetectTerrains();
        }

        if (targetTerrains == null || targetTerrains.Count == 0)
        {
            Debug.LogError("At least one target terrain must be assigned!");
            return;
        }

        TerrainData sourceData = sourceTerrain.terrainData;
        if (sourceData == null)
        {
            Debug.LogError("Source terrain data not found!");
            return;
        }

        foreach (Terrain target in targetTerrains)
        {
            if (target == null)
            {
                Debug.LogWarning("Skipping null target terrain in the list.");
                continue;
            }

            TerrainData targetData = target.terrainData;
            if (targetData == null)
            {
                Debug.LogError($"Target terrain data not found on {target.name}!");
                continue;
            }

            Undo.RecordObject(targetData, "Terrain Layer Transfer");
            targetData.terrainLayers = sourceData.terrainLayers;
            EditorUtility.SetDirty(targetData);
            Debug.Log($"Successfully transferred layers to {target.name}");
        }

        Debug.Log("Transfer process completed!");
    }

    void AutoDetectTerrains()
    {
        // Sahnedeki tüm terrain'leri bul
        Terrain[] allTerrains = Object.FindObjectsOfType<Terrain>();
        
        if (allTerrains == null || allTerrains.Length == 0)
        {
            Debug.LogWarning("No terrains found in the scene!");
            return;
        }

        targetTerrains.Clear();
        
        foreach (Terrain terrain in allTerrains)
        {
            // Eğer kaynak terrain'i hedeflerden hariç tutmak isteniyorsa
            if (excludeSourceFromTargets && terrain == sourceTerrain)
            {
                continue;
            }
            
            targetTerrains.Add(terrain);
        }
        
        Debug.Log($"Auto-detected {targetTerrains.Count} terrain(s) in the scene.");
        
        // Listeyi sırayla
        targetTerrains.Sort((a, b) => string.Compare(a.name, b.name));
        
        // SerializedObject'i güncelle
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
    }
    
    void ClearTargetTerrains()
    {
        targetTerrains.Clear();
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        Debug.Log("Target terrains list cleared.");
    }

    private void DrawSocialLinks()
    {
        EditorGUILayout.Space(20);
        GUILayout.Label("Follow/Contact", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (DiscordIcon && GUILayout.Button(new GUIContent(GitHubIcon, "GitHub"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(GitHubURL);

        if (DiscordIcon && GUILayout.Button(new GUIContent(InstagramIcon, "Instagram"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(instagramURL);

        if (DiscordIcon && GUILayout.Button(new GUIContent(DiscordIcon, "Discord"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(discordURL);

        if (YouTubeIcon && GUILayout.Button(new GUIContent(YouTubeIcon, "YouTube"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(youtubeURL);

        if (Metin2DownloadsIcon && GUILayout.Button(new GUIContent(Metin2DownloadsIcon, "Metin2Downloads"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(Metin2DownloadsURL);

        if (M2DevIcon && GUILayout.Button(new GUIContent(M2DevIcon, "M2Dev"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(M2DevURL);

        if (TurkmmoIcon && GUILayout.Button(new GUIContent(TurkmmoIcon, "Turkmmo"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(TurkmmoURL);

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}