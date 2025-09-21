using UnityEditor;
using UnityEngine;

public class HitboxEditorWindow : EditorWindow
{
    private GameObject selectedHitbox;
    private Vector3 newScale;
    private bool isEditing = false;

    private string[] meshOptions = { "Cube", "Sphere", "Capsule" };
    private int selectedMeshIndex = 0;

#if UNITY_EDITOR
    [MenuItem("Tools/Hitbox")]
    public static void DisplayWindow()
    {
        Debug.Log("Window item");
        GetWindow<HitboxEditorWindow>("Hitbox Editor");
    }
#endif
    protected void OnGUI()
    {
        GUILayout.Label("Hitbox Editor", EditorStyles.boldLabel);

        DisplayCurrentSelection();

        DisplayEditor();

    }

    private void DisplayCurrentSelection()
    {
        GUILayout.Label("Current Selection:", EditorStyles.miniBoldLabel);

        if (Selection.activeGameObject == null)
        {
            GUILayout.Label("Nothing selected. Select a GameObject in the hierarchy.");
            return;
        }

        GUILayout.Label($"Selected: {Selection.activeGameObject.name}");

        if (Selection.activeGameObject.TryGetComponent<HitBox>(out HitBox hitbox))
        {
            if (GUILayout.Button("Edit Hitbox"))
            {
                selectedHitbox = Selection.activeGameObject;
                newScale = selectedHitbox.transform.localScale;
            }
        }
    }

    private void DisplayEditor()
    {
        if (selectedHitbox == null)
        {
            GUILayout.Label("No hitbox selected for editing.");
            return;
        }

        GUILayout.Label($"Editing: {selectedHitbox.name}");

        EditorGUI.BeginChangeCheck();
        newScale = EditorGUILayout.Vector3Field("Scale", newScale);

        if (EditorGUI.EndChangeCheck())
        {
            if (!isEditing)
            {
                Undo.RecordObject(selectedHitbox.transform, "Change Hitbox Scale");
                isEditing = true;
            }

            selectedHitbox.transform.localScale = newScale;
        }

        if (isEditing && !EditorGUIUtility.editingTextField)
        {
            isEditing = false;
        }

        selectedMeshIndex = EditorGUILayout.Popup("Mesh Type", selectedMeshIndex, meshOptions);
        if (GUILayout.Button("Apply Selected Mesh"))
        {
            ApplyMesh(meshOptions[selectedMeshIndex]);
        }
    }

    private void ApplyMesh(string selectedMesh)
    {
        if (selectedHitbox == null)
            return;

        GameObject tempShape;
        switch (selectedMesh)
        {
            case "Cube":
                tempShape = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case "Sphere":
                tempShape = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case "Capsule":
                tempShape = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                break;
            default:
                return;
        }

        selectedHitbox.GetComponent<MeshFilter>().sharedMesh = tempShape.GetComponent<MeshFilter>().sharedMesh;
        DestroyImmediate(tempShape);

    }
}
