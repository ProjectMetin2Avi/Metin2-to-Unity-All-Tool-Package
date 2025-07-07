using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor tool for creating neighboring planes around an existing plane.
/// Similar to Unity's "Create Neighbor Terrains" functionality for terrains.
/// </summary>
[InitializeOnLoad]
public class PlaneNeighborCreator : Editor
{
    // Static constructor to subscribe to events when editor starts
    static PlaneNeighborCreator()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    // Cleanup when editor is closed
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
    static void OnDestroy()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    // Settings
    private static float spacing = 0f; // Gap between planes
    private static bool matchSize = true;
    private static bool matchMaterial = true;
    private static string neighborNamePrefix = "Plane_";
    private static bool showHandles = false;
    private static GameObject selectedPlane;

    // Visual settings
    private static readonly Color handleColor = new Color(0.3f, 0.7f, 1.0f, 0.8f);
    private static readonly Color handleHoverColor = new Color(1f, 0.9f, 0.1f, 1f);
    private static readonly Color handleSelectedColor = new Color(0f, 1f, 0f, 1f);
    private static readonly float handleSize = 1.5f;

    // Add context menu item to GameObject right-click menu
    [MenuItem("GameObject/Create Neighbor Planes", false, 10)]
    static void CreateNeighborPlanesMenu(MenuCommand menuCommand)
    {
        // Get the selected GameObject
        GameObject selectedObject = menuCommand.context as GameObject;
        if (selectedObject == null || !IsPlane(selectedObject))
        {
            EditorUtility.DisplayDialog("Invalid Selection", "Please select a valid plane object.", "OK");
            return;
        }

        // Store the selected plane and enable handles
        selectedPlane = selectedObject;
        showHandles = true;

        // Focus on the selected plane
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
            SceneView.lastActiveSceneView.Repaint();
        }
    }

    // Draw scene GUI elements
    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!showHandles || selectedPlane == null || !IsPlane(selectedPlane))
            return;

        // Get plane information
        Vector3 planeSize = CalculatePlaneSize(selectedPlane);
        Vector3 position = selectedPlane.transform.position;
        Quaternion rotation = selectedPlane.transform.rotation;

        // Draw handles for each direction
        DrawDirectionalHandles(position, rotation, planeSize);
    }

    // Draw the directional handles around the plane
    private static void DrawDirectionalHandles(Vector3 position, Quaternion rotation, Vector3 planeSize)
    {
        // Calculate positions for neighbor planes
        Vector3 northPos = position + rotation * new Vector3(0, 0, planeSize.z + spacing);
        Vector3 southPos = position + rotation * new Vector3(0, 0, -(planeSize.z + spacing));
        Vector3 eastPos = position + rotation * new Vector3(planeSize.x + spacing, 0, 0);
        Vector3 westPos = position + rotation * new Vector3(-(planeSize.x + spacing), 0, 0);
        Vector3 northEastPos = position + rotation * new Vector3(planeSize.x + spacing, 0, planeSize.z + spacing);
        Vector3 northWestPos = position + rotation * new Vector3(-(planeSize.x + spacing), 0, planeSize.z + spacing);
        Vector3 southEastPos = position + rotation * new Vector3(planeSize.x + spacing, 0, -(planeSize.z + spacing));
        Vector3 southWestPos = position + rotation * new Vector3(-(planeSize.x + spacing), 0, -(planeSize.z + spacing));

        // Draw button handles for each position
        Handles.BeginGUI();

        // Main directions
        if (DrawPlaneButton(northPos, "North", rotation))
            CreateSingleNeighbor("North", northPos, rotation);

        if (DrawPlaneButton(southPos, "South", rotation))
            CreateSingleNeighbor("South", southPos, rotation);

        if (DrawPlaneButton(eastPos, "East", rotation))
            CreateSingleNeighbor("East", eastPos, rotation);

        if (DrawPlaneButton(westPos, "West", rotation))
            CreateSingleNeighbor("West", westPos, rotation);

        // Corner positions
        if (DrawPlaneButton(northEastPos, "NE", rotation))
            CreateSingleNeighbor("NorthEast", northEastPos, rotation);

        if (DrawPlaneButton(northWestPos, "NW", rotation))
            CreateSingleNeighbor("NorthWest", northWestPos, rotation);

        if (DrawPlaneButton(southEastPos, "SE", rotation))
            CreateSingleNeighbor("SouthEast", southEastPos, rotation);

        if (DrawPlaneButton(southWestPos, "SW", rotation))
            CreateSingleNeighbor("SouthWest", southWestPos, rotation);

        // Draw settings UI
        DrawSettingsGUI();

        Handles.EndGUI();
    }

    // Draw a button for a plane position
    private static bool DrawPlaneButton(Vector3 worldPos, string label, Quaternion rotation)
    {
        // Convert world position to screen position
        Vector3 screenPos = HandleUtility.WorldToGUIPoint(worldPos);

        // Define button rectangle (centered on the screen position)
        float size = 60;
        Rect buttonRect = new Rect(screenPos.x - size / 2, screenPos.y - size / 2, size, size);

        // Draw an arrow in the direction
        Vector3 center = buttonRect.center;

        // Draw button with label
        bool clicked = GUI.Button(buttonRect, label);

        // Draw a visual line from the plane to the button position
        Handles.color = handleColor;
        Handles.DrawLine(selectedPlane.transform.position, worldPos);

        // Draw a plane representation at the position
        Handles.color = clicked ? handleSelectedColor : (buttonRect.Contains(Event.current.mousePosition) ? handleHoverColor : handleColor);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(worldPos, rotation, Vector3.one);
        Vector3 planeSize = CalculatePlaneSize(selectedPlane);

        // Use a simpler approach to draw a plane shape
        using (new Handles.DrawingScope(rotationMatrix))
        {
            Vector3 planeHalfSize = planeSize * 0.5f;
            Vector3[] corners = new Vector3[]
            {
                new Vector3(-planeHalfSize.x, 0, -planeHalfSize.z),
                new Vector3(planeHalfSize.x, 0, -planeHalfSize.z),
                new Vector3(planeHalfSize.x, 0, planeHalfSize.z),
                new Vector3(-planeHalfSize.x, 0, planeHalfSize.z)
            };

            Handles.DrawSolidRectangleWithOutline(corners, new Color(handleColor.r, handleColor.g, handleColor.b, 0.2f), handleColor);
        }

        return clicked;
    }

    // Draw the settings GUI
    private static void DrawSettingsGUI()
    {
        // Calculate position for settings panel - right side of the screen
        Rect settingsRect = new Rect(
            Screen.width - 250,
            50,
            240,
            140);

        // Draw background panel
        GUI.Box(settingsRect, "Neighbor Plane Settings");

        // Draw settings controls
        Rect controlRect = new Rect(settingsRect.x + 10, settingsRect.y + 25, settingsRect.width - 20, 20);

        GUI.Label(controlRect, "Spacing");
        controlRect.y += 20;
        spacing = EditorGUI.FloatField(controlRect, spacing);

        controlRect.y += 25;
        matchSize = EditorGUI.Toggle(controlRect, "Match Size", matchSize);

        controlRect.y += 20;
        matchMaterial = EditorGUI.Toggle(controlRect, "Match Material", matchMaterial);

        controlRect.y += 25;
        if (GUI.Button(controlRect, "Close Tool"))
        {
            showHandles = false;
            selectedPlane = null;
        }
    }

    // Create a single neighbor plane
    private static void CreateSingleNeighbor(string direction, Vector3 position, Quaternion rotation)
    {
        if (selectedPlane == null)
            return;

        MeshRenderer meshRenderer = selectedPlane.GetComponent<MeshRenderer>();
        Vector3 scale = selectedPlane.transform.localScale;

        // Create the plane
        GameObject newPlane = CreateNeighborPlane(direction, position, rotation, scale, meshRenderer);

        // Select the new plane
        Selection.activeGameObject = newPlane;

        // Update the selected plane to the new one so handles will be shown for it
        selectedPlane = newPlane;
        showHandles = true; // Ensure handles remain visible

        // Focus on the new plane
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
            SceneView.lastActiveSceneView.Repaint();
        }
    }

    // Check if an object is a plane
    private static bool IsPlane(GameObject obj)
    {
        // Check if the object has a MeshFilter with a mesh
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return false;

        // Additional checks could be added here if needed
        // For example, checking if the mesh has a specific vertex count typical for planes

        return true;
    }

    // Create a neighbor plane with the given parameters
    private static GameObject CreateNeighborPlane(string direction, Vector3 position, Quaternion rotation, Vector3 scale, MeshRenderer sourceMeshRenderer)
    {
        string planeName = neighborNamePrefix + direction;

        // Create a new plane GameObject
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = planeName;
        plane.transform.position = position;
        plane.transform.rotation = rotation;

        if (matchSize)
        {
            plane.transform.localScale = scale;
        }

        // Match material if needed
        if (matchMaterial && sourceMeshRenderer != null && sourceMeshRenderer.sharedMaterial != null)
        {
            MeshRenderer planeRenderer = plane.GetComponent<MeshRenderer>();
            if (planeRenderer != null)
            {
                planeRenderer.sharedMaterial = sourceMeshRenderer.sharedMaterial;
            }
        }

        // Register undo
        Undo.RegisterCreatedObjectUndo(plane, "Create Neighbor Plane");

        return plane;
    }

    // Calculate the size of a plane
    private static Vector3 CalculatePlaneSize(GameObject plane)
    {
        MeshFilter meshFilter = plane.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return Vector3.one * 10f; // Default size

        // Get the mesh bounds
        Bounds bounds = meshFilter.sharedMesh.bounds;

        // Apply the local scale to get the actual size in world space
        Vector3 size = Vector3.Scale(bounds.size, plane.transform.localScale);

        return size;
    }
}
