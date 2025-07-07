using UnityEditor;
using UnityEngine;

public class AlignMenu
{
    [MenuItem("Align/Align Position %&p")]
    private static void AlignPosition()
    {
        if (!IsValidSelection()) return;

        GameObject source = Selection.activeGameObject;
        GameObject target = GetFirstSelectedExceptActive();

        if (source == null || target == null) return;

        Undo.RecordObject(source.transform, "Align Position");
        source.transform.position = target.transform.position;
    }

    [MenuItem("Align/Align Rotation %&r")]
    private static void AlignRotation()
    {
        if (!IsValidSelection()) return;

        GameObject source = Selection.activeGameObject;
        GameObject target = GetFirstSelectedExceptActive();

        if (source == null || target == null) return;

        Undo.RecordObject(source.transform, "Align Rotation");
        source.transform.rotation = target.transform.rotation;
    }

    [MenuItem("Align/Align Position & Rotation %&a")]
    private static void AlignPositionAndRotation()
    {
        if (!IsValidSelection()) return;

        GameObject source = Selection.activeGameObject;
        GameObject target = GetFirstSelectedExceptActive();

        if (source == null || target == null) return;

        Undo.RecordObject(source.transform, "Align Position & Rotation");
        source.transform.position = target.transform.position;
        source.transform.rotation = target.transform.rotation;
    }

    [MenuItem("Align/Copy Full Transform %&f")]
    private static void CopyFullTransform()
    {
        if (!IsValidSelection()) return;

        GameObject source = Selection.activeGameObject;
        GameObject target = GetFirstSelectedExceptActive();

        if (source == null || target == null) return;

        Undo.RecordObject(source.transform, "Copy Full Transform");
        source.transform.position = target.transform.position;
        source.transform.rotation = target.transform.rotation;
        source.transform.localScale = target.transform.localScale;
    }

    private static bool IsValidSelection()
    {
        if (Selection.gameObjects.Length < 2)
        {
            EditorUtility.DisplayDialog("Uyarý", "Lütfen CTRL ile en az 2 obje seç.\nÝlk seçtiðin hedef, son týkladýðýn iþlem yapýlacak objedir.", "Tamam");
            return false;
        }
        return true;
    }

    private static GameObject GetFirstSelectedExceptActive()
    {
        foreach (var obj in Selection.gameObjects)
        {
            if (obj != Selection.activeGameObject)
                return obj;
        }
        return null;
    }
}
