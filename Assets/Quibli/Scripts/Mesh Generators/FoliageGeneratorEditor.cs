#if UNITY_EDITOR
using UnityEngine;

namespace Dustyroom {
[UnityEditor.CustomEditor(typeof(FoliageGenerator))]
public class FoliageGeneratorEditor : ExternalPropertyAttributes.Editor.ExternalCustomInspector {
    private FoliageGenerator _generator;

    protected override void OnEnable() {
        _generator = (FoliageGenerator)target;
        base.OnEnable();
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Export Mesh")) {
            _generator.ExportMesh();
        }
    }
}
}
#endif