using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(PartComponent), true)]
public class PartComponentDrawer : PropertyDrawer
{
    //Needed something to remember TT
    private static Dictionary<string, bool> foldouts = new Dictionary<string, bool>();
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        PartComponent component = property.objectReferenceValue as PartComponent;
        
    
        Rect objectFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(objectFieldRect, property, label, false);
        
        if (component != null)
        {
            string key = property.propertyPath;
            if (!foldouts.ContainsKey(key))
                foldouts[key] = false;
            
            // Custom foldout
            Rect foldoutRect = new Rect(position.x, objectFieldRect.yMax + 2, position.width, EditorGUIUtility.singleLineHeight);
            foldouts[key] = EditorGUI.Foldout(foldoutRect, foldouts[key], $"└─ {component.GetType().Name} Details", true);
            
            if (foldouts[key])
            {
                float yPos = foldoutRect.yMax + 2;
                
                SerializedObject componentSO = new SerializedObject(component);
                componentSO.Update();
                
                SerializedProperty componentProp = componentSO.GetIterator();
                if (componentProp.NextVisible(true))
                {
                    EditorGUI.indentLevel++;
                    
                    do
                    {
                        // Skip script reff
                        if (componentProp.name == "m_Script")
                            continue;
                        
                        float propHeight = EditorGUI.GetPropertyHeight(componentProp, true);
                        Rect propRect = new Rect(position.x, yPos, position.width, propHeight);
                        EditorGUI.PropertyField(propRect, componentProp, true);
                        yPos += propHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                    while (componentProp.NextVisible(false));
                    
                    EditorGUI.indentLevel--;
                }
                
                componentSO.ApplyModifiedProperties();
            }
        }
        
        EditorGUI.EndProperty();
    }
    
    //Needed to make size dynamic
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight; 
        
        PartComponent component = property.objectReferenceValue as PartComponent;
        if (component != null)
        {
            height += EditorGUIUtility.singleLineHeight + 2; 
            
            string key = property.propertyPath;
            if (foldouts.ContainsKey(key) && foldouts[key])
            {
              
                SerializedObject componentSO = new SerializedObject(component);
                SerializedProperty componentProp = componentSO.GetIterator();
                
                if (componentProp.NextVisible(true))
                {
                    do
                    {
                        if (componentProp.name == "m_Script")
                            continue;
                        
                        height += EditorGUI.GetPropertyHeight(componentProp, true) + EditorGUIUtility.standardVerticalSpacing;
                    }
                    while (componentProp.NextVisible(false));
                }
            }
        }
        
        return height + 4; // Extra padding
    }
}