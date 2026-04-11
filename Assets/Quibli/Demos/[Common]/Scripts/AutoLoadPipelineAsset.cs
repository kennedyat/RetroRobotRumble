using UnityEngine;
using UnityEngine.Rendering;

namespace Quibli.Demos {
[ExecuteAlways]
public class AutoLoadPipelineAsset : MonoBehaviour {
    [SerializeField]
    private RenderPipelineAsset pipelineAsset;

    private RenderPipelineAsset _previousPipelineAsset;
    private bool _overrodeQualitySettings;

    private void OnEnable() {
        UpdatePipeline();
    }

    private void OnDisable() {
        ResetPipeline();
    }

    private void OnValidate() {
        UpdatePipeline();
    }

    private void UpdatePipeline() {
        if (pipelineAsset) {
            if (QualitySettings.renderPipeline != null && QualitySettings.renderPipeline != pipelineAsset) {
                _previousPipelineAsset = QualitySettings.renderPipeline;
                QualitySettings.renderPipeline = pipelineAsset;
                _overrodeQualitySettings = true;
            } else {
#if UNITY_6000_0_OR_NEWER
                var currentPipeline = GraphicsSettings.defaultRenderPipeline;
#else
                var currentPipeline = GraphicsSettings.renderPipelineAsset;
#endif
                if (currentPipeline != pipelineAsset) {
                    _previousPipelineAsset = currentPipeline;
                    GraphicsSettings.defaultRenderPipeline = pipelineAsset;
                    _overrodeQualitySettings = false;
                }
            }
        }
    }

    private void ResetPipeline() {
        if (_previousPipelineAsset) {
            if (_overrodeQualitySettings) {
                QualitySettings.renderPipeline = _previousPipelineAsset;
            } else {
#if UNITY_6000_0_OR_NEWER
                GraphicsSettings.defaultRenderPipeline = _previousPipelineAsset;
#else
                GraphicsSettings.renderPipelineAsset = _previousPipelineAsset;
#endif
            }
        }
    }
}
}