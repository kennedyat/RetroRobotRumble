using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;


public partial class AssetLoader
{
    class DragAndDropManipulator : PointerManipulator
    {

        Label dropLabel;
        Object droppedModel = null;
        string assetPath = string.Empty;

        public DragAndDropManipulator(VisualElement root)
        {
            target = root.Q<VisualElement>(className: "drop-area");
            dropLabel = root.Q<Label>(className: "drop-area__label");
        }
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<DragEnterEvent>(OnDragEnter);
            target.RegisterCallback<DragPerformEvent>(OnDragPerform);
        }

        protected override void UnregisterCallbacksFromTarget()
        {

        }


        void OnDragEnter(DragEnterEvent _)
        {
            //Change drop label to model name

            var modelName = string.Empty;

            // Make sure its and fbx
            if (DragAndDrop.paths.Length == 1)
            {
                assetPath = DragAndDrop.paths[0];
                var splitPath = assetPath.Split('/');
                modelName = splitPath[splitPath.Length - 1];
            }
            else if (DragAndDrop.objectReferences.Length > 0)
            {
                modelName = DragAndDrop.objectReferences[0].name;
            }
            //Change color

            dropLabel.text = $"Dropping {modelName} ...";
            target.AddToClassList("drop-area--dropping");


        }

        void OnDragLeave(DragLeaveEvent _)
        {

        }

        void OnDragUpdate(DragUpdatedEvent _)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        }

        void OnDragPerform(DragPerformEvent _)
        {
            droppedModel = DragAndDrop.objectReferences[0];
            string draggedName;
             if (assetPath != string.Empty)
            {
                var splitPath = assetPath.Split('/');
                draggedName = splitPath[splitPath.Length - 1];
            }
            else
            {
                draggedName = droppedModel.name;
            }

                // Visually update target to indicate that it now stores an asset.
                dropLabel.text = $"Containing '{draggedName}'...\n\n" +
                    $"(You can also drag from here)";
                target.RemoveFromClassList("drop-area--dropping");
        }
    }
}