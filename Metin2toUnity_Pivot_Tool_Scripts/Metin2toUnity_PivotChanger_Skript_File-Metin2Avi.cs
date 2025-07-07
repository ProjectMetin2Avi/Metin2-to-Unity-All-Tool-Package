using UnityEngine;
using UnityEditor;

public class PivotChanger : EditorWindow
{
    // Renk ayarları
    private Color headerColor = new Color(0.2f, 0.3f, 0.8f, 1f); // Mavi ton
    private Color buttonColor = new Color(0.3f, 0.7f, 0.3f, 1f); // Yeşil ton
    private Color cancelButtonColor = new Color(0.8f, 0.3f, 0.3f, 1f); // Kırmızı ton
    private Color boxColor = new Color(0.9f, 0.9f, 0.9f, 0.1f); // Hafif gri arka plan
    
    // GUI Stilleri
    private GUIStyle headerStyle;
    private GUIStyle boxStyle;
    private GUIStyle buttonStyle;
    private GUIStyle cancelButtonStyle;
    
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
    [Header("Genel Ayarlar")]
    [Tooltip("Otomatik mod kullan")]
    public bool useAutomaticMode = true;

    [Header("Otomatik Mod Ayarları")]
    [Tooltip("Tek prefab için yol")]
    public string prefabPath;

    [Tooltip("Toplu işlem - klasör yolu")]
    public string folderPath;

    [Tooltip("Toplu işlem kullan (klasördeki tüm prefablar)")]
    public bool useBatchProcessing = false;

    [Header("Manuel Ayarlar")]
    [Tooltip("Pivot noktasını değiştirmek istediğiniz obje")]
    public GameObject targetObject;

    [Tooltip("Yeni pivot noktası olarak kullanılacak referans obje")]
    public GameObject referencePoint;

    [Header("Alternatif Yöntem")]
    [Tooltip("Manuel pozisyon girişi (referans obje yerine kullanılabilir)")]
    public Vector3 manualPivotPosition;

    [Tooltip("Manuel pozisyon kullan (otomatik mod kapalıysa etkin)")]
    public bool useManualPosition = false;
    
    [Header("Renk Ayarları (Opsiyonel)")]
    [Tooltip("Renk ayarlarını göster/gizle")]
    public bool showColorSettings = false;

    [MenuItem("Tools/Metin2 Pivot Changer Importer - @Metin2Avi")]
    static void ShowWindow()
    {
        PivotChanger window = GetWindow<PivotChanger>("Metin2 Pivot Changer Importer - @Metin2Avi");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }

    void OnGUI()
    {
        // Stilleri başlat
        InitializeStyles();
        
        // Ana arka plan rengi
        Color originalColor = GUI.color;
        GUI.color = boxColor;
        EditorGUILayout.BeginVertical(boxStyle);
        GUI.color = originalColor;
        
        // Sosyal medya bağlantılarını göster
        DrawSocialLinks();
        EditorGUILayout.Space();

        // Otomatik mod seçimi
        GUI.color = headerColor;
        EditorGUILayout.LabelField("Genel Ayarlar", headerStyle);
        GUI.color = originalColor;
        useAutomaticMode = EditorGUILayout.Toggle("Otomatik Mod Kullan", useAutomaticMode);

        EditorGUILayout.Space();

        if (useAutomaticMode)
        {
            // Otomatik mod ayarları
            GUI.color = headerColor;
            EditorGUILayout.LabelField("Otomatik Mod Ayarları", headerStyle);
            GUI.color = originalColor;

            // Toplu işlem seçeneği
            useBatchProcessing = EditorGUILayout.Toggle("Toplu İşlem (Klasördeki Tüm Prefablar)", useBatchProcessing);

            EditorGUILayout.Space();

            if (useBatchProcessing)
            {
                // Klasör seçimi
                EditorGUILayout.BeginHorizontal();

                folderPath = EditorGUILayout.TextField("Klasör Yolu", folderPath);

                if (GUILayout.Button("Klasör Seç", GUILayout.Width(80)))
                {
                    string selectedPath = EditorUtility.OpenFolderPanel("Prefab Klasörü Seç", "Assets", "");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        // Mutlak yolu göreceli yola çevir
                        if (selectedPath.StartsWith(Application.dataPath))
                        {
                            folderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                        }
                        else
                        {
                            folderPath = selectedPath;
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();

                // Klasör içeriği önizlemesi
                if (!string.IsNullOrEmpty(folderPath))
                {
                    string[] prefabGuids = AssetDatabase.FindAssets("t:GameObject", new string[] { folderPath });
                    int prefabCount = 0;

                    foreach (string guid in prefabGuids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (path.EndsWith(".prefab"))
                            prefabCount++;
                    }

                    if (prefabCount > 0)
                    {
                        EditorGUILayout.HelpBox($"Klasörde {prefabCount} adet prefab bulundu.", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Klasörde prefab bulunamadı!", MessageType.Warning);
                    }
                }
            }
            else
            {
                // Tek prefab seçimi
                EditorGUILayout.BeginHorizontal();

                prefabPath = EditorGUILayout.TextField("Prefab Yolu", prefabPath);

                if (GUILayout.Button("Gözat", GUILayout.Width(60)))
                {
                    string selectedPath = EditorUtility.OpenFilePanel("Prefab Seç", "Assets", "prefab");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        // Mutlak yolu göreceli yola çevir
                        if (selectedPath.StartsWith(Application.dataPath))
                        {
                            prefabPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                        }
                        else
                        {
                            prefabPath = selectedPath;
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();

                // Önizleme bilgisi
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab != null)
                    {
                        EditorGUILayout.HelpBox($"Prefab: {prefab.name}\nİlk çocuk: {(prefab.transform.childCount > 0 ? prefab.transform.GetChild(0).name : "Yok")}", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Geçersiz prefab yolu!", MessageType.Warning);
                    }
                }
            }
        }
        else
        {
            // Manuel mod ayarları
            GUI.color = headerColor;
            EditorGUILayout.LabelField("Manuel Ayarlar", headerStyle);
            GUI.color = originalColor;

            targetObject = (GameObject)EditorGUILayout.ObjectField("Hedef Obje", targetObject, typeof(GameObject), true);
            referencePoint = (GameObject)EditorGUILayout.ObjectField("Referans Noktası", referencePoint, typeof(GameObject), true);

            EditorGUILayout.Space();
            GUI.color = headerColor;
            EditorGUILayout.LabelField("Alternatif Yöntem", headerStyle);
            GUI.color = originalColor;

            useManualPosition = EditorGUILayout.Toggle("Manuel Pozisyon Kullan", useManualPosition);

            if (useManualPosition)
            {
                manualPivotPosition = EditorGUILayout.Vector3Field("Manuel Pivot Pozisyonu", manualPivotPosition);
            }
        }

        EditorGUILayout.Space();
        
        // Renk ayarları bölümü
        DrawColorSettings();
        
        EditorGUILayout.Space();
        
        // Butonlar
        DrawButtons();
        
        EditorGUILayout.EndVertical();
    }
    
    void InitializeStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }
        
        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(5, 5, 5, 5)
            };
        }
        
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }
        
        if (cancelButtonStyle == null)
        {
            cancelButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }
    }
    
    void DrawColorSettings()
    {
        Color originalColor = GUI.color;
        
        // Katlanabilir renk ayarları bölümü
        showColorSettings = EditorGUILayout.Foldout(showColorSettings, "🎨 Renk Ayarları", true);
        
        if (showColorSettings)
        {
            EditorGUILayout.BeginVertical("box");
            
            GUI.color = headerColor;
            EditorGUILayout.LabelField("Tema Renkleri", headerStyle);
            GUI.color = originalColor;
            
            EditorGUILayout.Space(5);
            
            // Renk seçiciler
            headerColor = EditorGUILayout.ColorField("Başlık Rengi", headerColor);
            buttonColor = EditorGUILayout.ColorField("Ana Buton Rengi", buttonColor);
            cancelButtonColor = EditorGUILayout.ColorField("Kapat Buton Rengi", cancelButtonColor);
            boxColor = EditorGUILayout.ColorField("Arka Plan Rengi", boxColor);
            
            EditorGUILayout.Space(5);
            
            // Ön tanımlı temalar
            EditorGUILayout.LabelField("Hızlı Temalar:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Mavi Tema", GUILayout.Height(25)))
            {
                SetBlueTheme();
            }
            
            if (GUILayout.Button("Yeşil Tema", GUILayout.Height(25)))
            {
                SetGreenTheme();
            }
            
            if (GUILayout.Button("Kırmızı Tema", GUILayout.Height(25)))
            {
                SetRedTheme();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Mor Tema", GUILayout.Height(25)))
            {
                SetPurpleTheme();
            }
            
            if (GUILayout.Button("Turuncu Tema", GUILayout.Height(25)))
            {
                SetOrangeTheme();
            }
            
            if (GUILayout.Button("Varsayılan", GUILayout.Height(25)))
            {
                SetDefaultTheme();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
    }
    
    // Tema metodları
    void SetBlueTheme()
    {
        headerColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        buttonColor = new Color(0.3f, 0.5f, 0.9f, 1f);
        cancelButtonColor = new Color(0.7f, 0.3f, 0.3f, 1f);
        boxColor = new Color(0.9f, 0.95f, 1f, 0.1f);
    }
    
    void SetGreenTheme()
    {
        headerColor = new Color(0.2f, 0.7f, 0.3f, 1f);
        buttonColor = new Color(0.3f, 0.8f, 0.4f, 1f);
        cancelButtonColor = new Color(0.8f, 0.4f, 0.2f, 1f);
        boxColor = new Color(0.9f, 1f, 0.9f, 0.1f);
    }
    
    void SetRedTheme()
    {
        headerColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        buttonColor = new Color(0.9f, 0.3f, 0.3f, 1f);
        cancelButtonColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        boxColor = new Color(1f, 0.9f, 0.9f, 0.1f);
    }
    
    void SetPurpleTheme()
    {
        headerColor = new Color(0.6f, 0.2f, 0.8f, 1f);
        buttonColor = new Color(0.7f, 0.3f, 0.9f, 1f);
        cancelButtonColor = new Color(0.8f, 0.3f, 0.5f, 1f);
        boxColor = new Color(0.98f, 0.9f, 1f, 0.1f);
    }
    
    void SetOrangeTheme()
    {
        headerColor = new Color(0.9f, 0.5f, 0.1f, 1f);
        buttonColor = new Color(1f, 0.6f, 0.2f, 1f);
        cancelButtonColor = new Color(0.7f, 0.3f, 0.3f, 1f);
        boxColor = new Color(1f, 0.97f, 0.9f, 0.1f);
    }
    
    void SetDefaultTheme()
    {
        headerColor = new Color(0.2f, 0.3f, 0.8f, 1f);
        buttonColor = new Color(0.3f, 0.7f, 0.3f, 1f);
        cancelButtonColor = new Color(0.8f, 0.3f, 0.3f, 1f);
        boxColor = new Color(0.9f, 0.9f, 0.9f, 0.1f);
    }

    void DrawButtons()
    {
        EditorGUILayout.BeginHorizontal();
        
        // Validation kontrolü
        bool isValid = ValidateInputs();
        
        // Ana buton (Pivot'u Değiştir)
        Color originalBGColor = GUI.backgroundColor;
        GUI.enabled = isValid;
        GUI.backgroundColor = isValid ? buttonColor : Color.gray;
        if (GUILayout.Button("Pivot'u Değiştir", buttonStyle, GUILayout.Height(35)))
        {
            ChangePivot();
        }
        GUI.enabled = true;
        
        // Kapat butonu
        GUI.backgroundColor = cancelButtonColor;
        if (GUILayout.Button("Kapat", cancelButtonStyle, GUILayout.Height(35), GUILayout.Width(80)))
        {
            Close();
        }
        GUI.backgroundColor = originalBGColor;
        
        EditorGUILayout.EndHorizontal();
        
        // Validation mesajları
        if (!isValid)
        {
            string errorMessage = GetValidationMessage();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }
        }
    }
    
    bool ValidateInputs()
    {
        if (useAutomaticMode)
        {
            if (useBatchProcessing)
            {
                return !string.IsNullOrEmpty(folderPath);
            }
            else
            {
                return !string.IsNullOrEmpty(prefabPath);
            }
        }
        else
        {
            return targetObject != null && (useManualPosition || referencePoint != null);
        }
    }
    
    string GetValidationMessage()
    {
        if (useAutomaticMode)
        {
            if (useBatchProcessing)
            {
                if (string.IsNullOrEmpty(folderPath))
                    return "Lütfen klasör yolunu girin.";
            }
            else
            {
                if (string.IsNullOrEmpty(prefabPath))
                    return "Lütfen prefab yolunu girin.";
            }
        }
        else
        {
            if (targetObject == null)
                return "Lütfen pivot noktasını değiştirmek istediğiniz objeyi seçin.";
            
            if (!useManualPosition && referencePoint == null)
                return "Lütfen referans noktası olarak kullanılacak objeyi seçin veya manuel pozisyon kullanın.";
        }
        
        return "";
    }

    void ChangePivot()
    {
        if (useAutomaticMode)
        {
            if (useBatchProcessing)
            {
                ProcessAllPrefabsInFolder();
                return;
            }
            else
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    EditorUtility.DisplayDialog("Hata", "Prefab yüklenemedi! Geçerli bir yol sağladığınızdan emin olun.\n\nYol: " + prefabPath, "Tamam");
                    return;
                }

                ProcessSinglePrefab(prefab);
                return;
            }
        }
        else
        {
            if (targetObject == null)
            {
                EditorUtility.DisplayDialog("Hata", "Hedef obje seçilmemiş!", "Tamam");
                return;
            }

            if (!useManualPosition && referencePoint == null)
            {
                EditorUtility.DisplayDialog("Hata", "Referans noktası belirtilmemiş!", "Tamam");
                return;
            }

            // Manuel modda tek obje işle
            ApplyPivotChange(true);
        }
    }

    void ProcessAllPrefabsInFolder()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:GameObject", new string[] { folderPath });
        var prefabPaths = new System.Collections.Generic.List<string>();

        // Sadece .prefab dosyalarını filtrele
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".prefab"))
                prefabPaths.Add(path);
        }

        if (prefabPaths.Count == 0)
        {
            EditorUtility.DisplayDialog("Hata", "Klasörde prefab bulunamadı!", "Tamam");
            return;
        }

        int successCount = 0;
        int failureCount = 0;

        EditorUtility.DisplayProgressBar("Pivot Değiştiriliyor", "Başlatılıyor...", 0f);

        for (int i = 0; i < prefabPaths.Count; i++)
        {
            string path = prefabPaths[i];
            float progress = (float)i / prefabPaths.Count;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                EditorUtility.DisplayProgressBar("Pivot Değiştiriliyor", $"İşleniyor: {prefab.name} ({i + 1}/{prefabPaths.Count})", progress);

                if (ProcessSinglePrefab(prefab, false)) // false = dialog gösterme
                {
                    successCount++;
                }
                else
                {
                    failureCount++;
                }
            }
            else
            {
                failureCount++;
            }
        }

        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("Toplu İşlem Tamamlandı",
            $"Toplam: {prefabPaths.Count} prefab\n" +
            $"Başarılı: {successCount}\n" +
            $"Başarısız: {failureCount}\n\n" +
            $"Klasör: {folderPath}", "Tamam");
    }

    bool ProcessSinglePrefab(GameObject prefab, bool showDialog = true)
    {
        if (prefab == null)
            return false;

        if (prefab.transform.childCount == 0)
        {
            if (showDialog)
                EditorUtility.DisplayDialog("Hata", $"Prefab '{prefab.name}' objesinin çocuk objesi bulunamadı!\n\nPrefab'ın en az bir çocuk objesine sahip olması gerekir.", "Tamam");
            return false;
        }

        targetObject = prefab;
        referencePoint = prefab.transform.GetChild(0).gameObject;

        if (showDialog)
            Debug.Log($"İşleniyor: Target Object = {targetObject.name}, Reference Point = {referencePoint.name}");

        return ApplyPivotChange(showDialog);
    }

    bool ApplyPivotChange(bool showDialog = true)
    {
        try
        {
            // Yeni pivot pozisyonunu belirle
            Vector3 newPivotPosition;
            if (useManualPosition)
            {
                newPivotPosition = manualPivotPosition;
            }
            else if (referencePoint != null)
            {
                newPivotPosition = referencePoint.transform.position;
            }
            else
            {
                if (showDialog)
                    EditorUtility.DisplayDialog("Hata", "Pivot pozisyonu belirlenemedi!", "Tamam");
                return false;
            }

            // Undo kaydı oluştur
            Undo.RecordObject(targetObject.transform, "Pivot Değiştir");

            // Mevcut pivot ile yeni pivot arasındaki farkı hesapla
            Vector3 pivotOffset = targetObject.transform.position - newPivotPosition;

            // Eğer objenin mesh'i varsa, mesh'i offset kadar kaydır
            MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                // Mesh'in bir kopyasını oluştur
                Mesh originalMesh = meshFilter.sharedMesh;
                Mesh newMesh = Instantiate(originalMesh);

                // Vertex'leri offset kadar kaydır
                Vector3[] vertices = newMesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] += targetObject.transform.InverseTransformVector(pivotOffset);
                }
                newMesh.vertices = vertices;
                newMesh.RecalculateBounds();

                // Mesh'i güncelle
                meshFilter.mesh = newMesh;

                // Asset olarak kaydet
                string assetPath = AssetDatabase.GetAssetPath(originalMesh);
                if (string.IsNullOrEmpty(assetPath))
                {
                    assetPath = "Assets/ModifiedMeshes/";
                    if (!AssetDatabase.IsValidFolder(assetPath))
                    {
                        AssetDatabase.CreateFolder("Assets", "ModifiedMeshes");
                    }
                    assetPath += targetObject.name + "_ModifiedMesh.asset";
                    AssetDatabase.CreateAsset(newMesh, assetPath);
                }
            }

            // Collider'ları güncelle
            Collider[] colliders = targetObject.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                if (col is BoxCollider boxCol)
                {
                    Undo.RecordObject(boxCol, "Pivot Değiştir - Box Collider");
                    boxCol.center += targetObject.transform.InverseTransformVector(pivotOffset);
                }
                else if (col is SphereCollider sphereCol)
                {
                    Undo.RecordObject(sphereCol, "Pivot Değiştir - Sphere Collider");
                    sphereCol.center += targetObject.transform.InverseTransformVector(pivotOffset);
                }
                else if (col is CapsuleCollider capsuleCol)
                {
                    Undo.RecordObject(capsuleCol, "Pivot Değiştir - Capsule Collider");
                    capsuleCol.center += targetObject.transform.InverseTransformVector(pivotOffset);
                }
            }

            // Child objeleri de güncelle
            foreach (Transform child in targetObject.transform)
            {
                Undo.RecordObject(child, "Pivot Değiştir - Child");
                child.position += pivotOffset;
            }

            // Transform pozisyonunu yeni pivot noktasına taşı
            targetObject.transform.position = newPivotPosition;

            // Değişiklikleri kaydet
            EditorUtility.SetDirty(targetObject);
            AssetDatabase.SaveAssets();

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Başarılı",
                    $"'{targetObject.name}' objesinin pivot noktası başarıyla değiştirildi!\n" +
                    $"Yeni pivot pozisyonu: {newPivotPosition}", "Tamam");
            }

            return true;
        }
        catch (System.Exception ex)
        {
            if (showDialog)
                EditorUtility.DisplayDialog("Hata", $"Pivot değiştirilirken hata oluştu: {ex.Message}", "Tamam");
            return false;
        }
    }


    private void DrawSocialLinks()
    {
        EditorGUILayout.Space(20);
        GUILayout.Label("Follow/Contact", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GitHubIcon && GUILayout.Button(new GUIContent(GitHubIcon, "GitHub"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(GitHubURL);

        if (InstagramIcon && GUILayout.Button(new GUIContent(InstagramIcon, "Instagram"), GUILayout.Width(40), GUILayout.Height(40)))
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
