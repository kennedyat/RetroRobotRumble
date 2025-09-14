using System.Collections;
using System.Data.Common;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public partial class AssetLoader : EditorWindow
{

   [SerializeField] VisualTreeAsset uxmlAsset;

    // This manipulator contains all of the event logic for this window.
    DragAndDropManipulator manipulator;

    // This is the minimum size of both windows.
    readonly static Vector2 windowMinSize = new(300, 180);

    // These are the starting positions of the windows.
    readonly static Vector2 windowAPosition = new(50, 50);
    

    // These are the titles of the windows.
    const string windowATitle = "Drag and Drop A";
    

    // This method opens two DragAndDropWindows when a user selects the specified menu item.
    [MenuItem("Window/UI Toolkit/Drag And Drop (Editor)")]
    public static void OpenDragAndDropWindows()
    {
        // Create the windows.
        var windowA = CreateInstance<AssetLoader>();

        // Define the attributes of the windows and display them.
        windowA.minSize = windowMinSize;
        windowA.Show();
        windowA.titleContent = new(windowATitle);
        windowA.position = new(windowAPosition, windowMinSize);
    }

    void OnEnable()
    {
        if (uxmlAsset != null)
        {
            uxmlAsset.CloneTree(rootVisualElement);
        }

        // Instantiate manipulator.
        manipulator = new(rootVisualElement);
    }

    void OnDisable()
    {
        // The RemoveManipulator() method calls the Manipulator's UnregisterCallbacksFromTarget() method.
        manipulator.target.RemoveManipulator(manipulator);
    }
    
}
