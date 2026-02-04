using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PartType), true)]
public class PartTypeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        
        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;
            
            //Lol just wanted to change inspector names 
            string customLabel = prop.displayName;
            if (prop.name == "ultimateAbility") customLabel = "ULTIMATE ABILITY";
            else if (prop.name == "passiveAbility") customLabel = "PASSIVE ABILITY";
            else if (prop.name == "normalAbility") customLabel = "NORMAL ABILITY";
            else if (prop.name == "specialAbility") customLabel = "SPECIAL ABILITY";
            
        
            EditorGUILayout.PropertyField(prop, new GUIContent(customLabel), true);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}