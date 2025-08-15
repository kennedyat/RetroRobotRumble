using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class HitboxEditorWindow : EditorWindow
{
    private GameObject selectedHitbox;
    private Vector3 newScale;
    private bool isEditing = false; 

     [MenuItem("Tools/Hitbox Editor")]
    public void DisplayWindow()
    {
        GetWindow<HitboxEditorWindow>("Hitbox Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Hitbox Editor",  EditorStyles.boldLabel );

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

         if(Selection.activeGameObject.TryGetComponent<HitBox>(out HitBox hitbox))
         {
            if(GUILayout.Button("Edit Hitbox"))
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


    }
  
}