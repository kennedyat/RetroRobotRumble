#if UNITY_EDITOR
using UnityEngine;

namespace Dustyroom {
[UnityEditor.CustomEditor(typeof(GrassMeshGenerator))]
public class GrassMeshGeneratorEditor : ExternalPropertyAttributes.Editor.ExternalCustomInspector {
    private GrassMeshGenerator _generator;

    protected override void OnEnable() {
        _generator = (GrassMeshGenerator)target;
        base.OnEnable();
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Add to scene")) {
            _generator.AddToScene();
        }

        if (GUILayout.Button("Save prefab")) {
            _generator.Save();
        }

        if (GUILayout.Button("Save prefab as...")) {
            _generator.SaveAs();
        }
    }
}
}
#endif