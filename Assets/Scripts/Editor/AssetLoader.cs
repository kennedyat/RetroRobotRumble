using System.Collections;
using System.Data.Common;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using System.IO;

public partial class AssetLoader : EditorWindow
{
    [SerializeField] VisualTreeAsset uxmlAsset;

    // This manipulator contains all of the event logic for this window.
    DragAndDropManipulator manipulator;

    // This is the minimum size of both windows.
    readonly static Vector2 windowMinSize = new(400, 300);

    // These are the starting positions of the windows.
    readonly static Vector2 windowAPosition = new(50, 50);
    
    // These are the titles of the windows.
    const string windowATitle = "FBX Validator - Drag and Drop";
    
    // This method opens two DragAndDropWindows when a user selects the specified menu item.
    [MenuItem("Tools/FBX Validator")]
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
        if (manipulator != null && manipulator.target != null)
        {
            manipulator.target.RemoveManipulator(manipulator);
        }
    }
}

public partial class AssetLoader
{
    public struct FBXValidationResult
    {
        public string fileName;
        public bool isValidFBX;
        public bool isHumanoid;
        public bool hasAnimator;
        public bool isValid;
        public string errorMessage;
        public string assetPath;
    }

    class DragAndDropManipulator : PointerManipulator
    {
        Label dropLabel;
        Label infoLabel;
        VisualElement infoArea;
        Object droppedModel = null;
        string assetPath = string.Empty;

        public DragAndDropManipulator(VisualElement root)
        {
            target = root.Q<VisualElement>(className: "drop-area");
            dropLabel = root.Q<Label>(className: "drop-area__label");
            infoLabel = root.Q<Label>(className: "info-area__label");
            infoArea = root.Q<VisualElement>(className: "info-area");
        }
        //Registers mouse input
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<DragEnterEvent>(OnDragEnter);
            target.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            target.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.RegisterCallback<DragPerformEvent>(OnDragPerform);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<DragEnterEvent>(OnDragEnter);
            target.UnregisterCallback<DragLeaveEvent>(OnDragLeave);
            target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.UnregisterCallback<DragPerformEvent>(OnDragPerform);
        }

        void OnDragEnter(DragEnterEvent _)
        {
            var modelName = string.Empty;

            //Update label
             if (DragAndDrop.objectReferences.Length > 0)
            {
                modelName = DragAndDrop.objectReferences[0].name;
                assetPath = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0]);
            }

            dropLabel.text = $"Dropping {modelName}...";
            target.AddToClassList("drop-area--dropping");
        }

        void OnDragLeave(DragLeaveEvent _)
        {
            dropLabel.text = "Drag an FBX file here...";
            target.RemoveFromClassList("drop-area--dropping");
        }

        void OnDragUpdate(DragUpdatedEvent _)
        {
            // Check if the dragged item is an fbx
            bool isValidDrag = false;
            string path;
            
            if (DragAndDrop.objectReferences.Length > 0)
            {
                path = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0]);
                isValidDrag = path.ToLower().EndsWith(".fbx");
            }

            DragAndDrop.visualMode = isValidDrag ? DragAndDropVisualMode.Generic : DragAndDropVisualMode.Rejected;
        }

        void OnDragPerform(DragPerformEvent _)
        {
            droppedModel = DragAndDrop.objectReferences[0];
            string draggedPath;
            
            if (assetPath != string.Empty)
            {
                draggedPath = assetPath;
            }
            else
            {
                draggedPath = AssetDatabase.GetAssetPath(droppedModel);
            }

            // Validate the fbx
            var result = ValidateFBX(draggedPath);
            
            // Update UI
            UpdateDropAreaUI(result);
            UpdateInfoAreaUI(result);
            
            target.RemoveFromClassList("drop-area--dropping");
        }

        FBXValidationResult ValidateFBX(string path)
        {
            var result = new FBXValidationResult();
            result.assetPath = path;
            result.fileName = Path.GetFileName(path);
            
            // Check if it's an fbx file (reduntant?)
            if (!path.ToLower().EndsWith(".fbx"))
            {
                result.isValidFBX = false;
                result.errorMessage = "Not an FBX file";
                result.isValid = false;
                return result;
            }
            
            result.isValidFBX = true;
            
           
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                result.errorMessage = "Could not get ModelImporter";
                result.isValid = false;
                return result;
            }
            
            // Check if it's humanoid
            result.isHumanoid = importer.animationType == ModelImporterAnimationType.Human;
            
            // Check for Animator component
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset != null)
            {
               
                result.hasAnimator = asset.GetComponentInChildren<Animator>() != null;
            }
            
            // etermine if it's valid 
            result.isValid = result.isValidFBX && result.isHumanoid && result.hasAnimator;
            
            if (!result.isValid && result.isValidFBX)
            {
                if (!result.isHumanoid && !result.hasAnimator)
                {
                    result.errorMessage = "Not humanoid and no Animator component";
                }
                else if (!result.isHumanoid)
                {
                    result.errorMessage = "Not set as humanoid rig";
                }
                else if (!result.hasAnimator)
                {
                    result.errorMessage = "No Animator component found";
                }
            }
            
            return result;
        }

        void UpdateDropAreaUI(FBXValidationResult result)
        {
            if (result.isValid)
            {
                dropLabel.text = $"✓ {result.fileName}\nValid FBX Asset!";
                target.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f, 1f); // Green
            }
            else
            {
                dropLabel.text = $"✗ {result.fileName}\n{result.errorMessage}";
                target.style.backgroundColor = new Color(0.6f, 0.2f, 0.2f, 1f); // Red
            }
        }

        void UpdateInfoAreaUI(FBXValidationResult result)
        {
            var statusText = "";
            statusText += $"<b>File Name:</b> {result.fileName}\n\n";
            statusText += $"<b>Asset Path:</b> {result.assetPath}\n\n";
            statusText += $"<b>Is FBX:</b> {(result.isValidFBX ? "✓ Yes" : "✗ No")}\n\n";
            statusText += $"<b>Is Humanoid:</b> {(result.isHumanoid ? "✓ Yes" : "✗ No")}\n\n";
            statusText += $"<b>Has Animator:</b> {(result.hasAnimator ? "✓ Yes" : "✗ No")}\n\n";
            statusText += $"<b>Overall Status:</b> ";
            
            if (result.isValid)
            {
                statusText += "<color=#4CAF50><b>✓ VALID</b></color>\n\n";
                statusText += "<color=#4CAF50>This FBX is ready to use!</color>";
                infoArea.style.backgroundColor = new Color(0.2f, 0.4f, 0.2f, 0.8f);
            }
            else
            {
                statusText += "<color=#F44336><b>✗ INVALID</b></color>\n\n";
                statusText += $"<color=#F44336>Issue: {result.errorMessage}</color>";
                infoArea.style.backgroundColor = new Color(0.4f, 0.2f, 0.2f, 0.8f);
            }
            
            infoLabel.text = statusText;
        }
    }
}