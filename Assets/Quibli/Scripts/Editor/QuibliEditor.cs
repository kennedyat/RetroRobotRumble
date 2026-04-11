using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Quibli {
public class QuibliEditor : BaseShaderGUI {
    private Material _target;
    private MaterialProperty[] _properties;

    private static readonly Dictionary<string, bool> FoldoutStates =
        new Dictionary<string, bool> { { "Rendering options", false } };

    private const string UnityVersion = "JLE8GP";

    void DrawStandard(MaterialEditor editor, MaterialProperty property) {
        string displayName = property.displayName;

        // Remove everything in brackets.
        displayName = Regex.Replace(displayName, @" ?\[.*?\]", string.Empty);
        displayName = Regex.Replace(displayName, @" ?\{.*?\}", string.Empty);

        var tooltip = Tooltips.Get(editor, displayName);
        var guiContent = new GUIContent(displayName, tooltip);

        bool isTexture;
#if UNITY_6000_1_OR_NEWER
        isTexture = property.propertyType == ShaderPropertyType.Texture;
#else
        isTexture = property.type == MaterialProperty.PropType.Texture;
#endif
        if (isTexture && !property.displayName.Contains("Gradient") && !property.name.Contains("Ramp")) {
            if (!property.name.Contains("_BaseMap") && !property.name.Contains("_EmissionMap")) {
                EditorGUILayout.Space(15);
            }

            materialEditor.TexturePropertySingleLine(guiContent, property);
        } else {
            materialEditor.ShaderProperty(property, guiContent);
        }
    }

    MaterialProperty FindProperty(string name) {
        return FindProperty(name, _properties);
    }

    bool HasProperty(string name) {
        return _target != null && _target.HasProperty(name);
    }

    bool IsTextureImpactLocked() {
        return HasProperty("_TextureImpact") && HasProperty("_BaseMap") && _target.GetTexture("_BaseMap") == null;
    }

    bool HasTextureAssigned(string name) {
        return HasProperty(name) && _target.GetTexture(name) != null;
    }


#if UNITY_2021_2_OR_NEWER
    public override void ValidateMaterial(Material material) {
#else
    public override void MaterialChanged(Material material) {
#endif
        if (material == null) throw new ArgumentNullException(nameof(material));
        SetMaterialKeywords(material);
    }

    public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties) {
        materialEditor = editor;
        _properties = properties;
        _target = editor.target as Material;
        Debug.Assert(_target != null);

        FindProperties(properties);

        if (!Application.unityVersion.Contains(Rev(UnityVersion)) && !Application.unityVersion.Contains('b') &&
            !Application.unityVersion.Contains('a') && !Application.unityVersion.Replace('.', '^').Contains("23^2")) {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            // Icon.
            {
                EditorGUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                var icon = EditorGUIUtility.IconContent("console.erroricon@2x").image;
                var iconSize = Mathf.Min(Mathf.Min(60, icon.width), EditorGUIUtility.currentViewWidth - 100);
                GUILayout.Label(icon,
                                new GUIStyle {
                                    alignment = TextAnchor.MiddleCenter,
                                    imagePosition = ImagePosition.ImageLeft,
                                    fixedWidth = iconSize,
                                    fixedHeight = iconSize,
                                    padding = new RectOffset(0, 0, 5, 5),
                                    margin = new RectOffset(0, 0, 0, 0),
                                });
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }

            var unityMajorVersion = Application.unityVersion.Substring(0, Application.unityVersion.LastIndexOf('.'));
            var m = $"This version of <b>Quibli</b> is designed for <b>Unity {Rev(UnityVersion)}</b>. " +
                    $"You are currently using <b>Unity {unityMajorVersion}</b>.\n" +
                    "<i>The shader and the UI below may not work correctly.</i>\n" +
                    "Please <b>re-download Quibli</b> to get the compatible version.";
            var style = new GUIStyle(EditorStyles.wordWrappedLabel) {
                alignment = TextAnchor.MiddleLeft,
                richText = true,
                fontSize = 12,
                padding = new RectOffset(0, 5, 5, 5),
                margin = new RectOffset(0, 0, 0, 0),
            };
            EditorGUILayout.LabelField(m, style);
            EditorGUILayout.EndHorizontal();

            // Unity version help buttons.
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                const float buttonWidth = 120;

                if (GUILayout.Button("Package Manager", EditorStyles.miniButton, GUILayout.Width(buttonWidth))) {
                    const string packageName = "Quibli: Anime Shaders and Tools";

                    var type = typeof(UnityEditor.PackageManager.UI.Window);
                    var method = type.GetMethod("OpenFilter",
                                                System.Reflection.BindingFlags.NonPublic |
                                                System.Reflection.BindingFlags.Static);
                    if (method != null) {
                        method.Invoke(null, new object[] { $"AssetStore/{packageName}" });
                    } else {
                        UnityEditor.PackageManager.UI.Window.Open(packageName);
                    }
                }

                if (GUILayout.Button("Asset Store", EditorStyles.miniButton, GUILayout.Width(buttonWidth))) {
                    const string assetStoreUrl =
                        "https://assetstore.unity.com/packages/vfx/quibli-anime-shaders-and-tools-203178";
                    Application.OpenURL(assetStoreUrl);
                }

                if (GUILayout.Button("Support", EditorStyles.miniButton, GUILayout.Width(buttonWidth))) {
                    const string contactUrl = "https://quibli.dustyroom.com/contact-details/";
                    Application.OpenURL(contactUrl);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            foreach (var property in properties) {
                DrawStandard(editor, property);
            }

            return;
        }

        if (_target.IsKeywordEnabled("DR_OUTLINE_ON") && _target.IsKeywordEnabled("_ALPHATEST_ON")) {
            const string m = "The 'Outline' and 'Alpha Clip' features are usually incompatible. The outline shader " +
                             "pass will not be using alpha clipping.";
            EditorGUILayout.HelpBox(m, MessageType.Warning);
        }

        bool textureImpactLocked = IsTextureImpactLocked();
        bool baseMapLocked = textureImpactLocked;
        bool detailMapLocked = HasProperty("_DetailMap") && !HasTextureAssigned("_DetailMap");
        if (textureImpactLocked) {
            _target.SetFloat("_TextureImpact", 0f);
        }

        int originalIntentLevel = EditorGUI.indentLevel;
        int foldoutRemainingItems = 0;
        bool latestFoldoutState = false;

        foreach (MaterialProperty property in properties) {
            string displayName = property.displayName;

            if (displayName.Contains("[") && !displayName.Contains("FOLDOUT")) {
                EditorGUI.indentLevel += 1;
            }

            var skipProperty = false;
            foreach (Match match in Regex.Matches(displayName, @" ?\[DR_.*?\]")) {
                var keyword = match.Value.Replace("[", string.Empty).Replace("]", string.Empty);
                skipProperty |= !_target.IsKeywordEnabled(keyword);
            }

            if (_target.IsKeywordEnabled("DR_ENABLE_LIGHTMAP_DIR") &&
                displayName.ToLower().Contains("override light direction")) {
                var dirPitch = _target.GetFloat("_LightmapDirectionPitch");
                var dirYaw = _target.GetFloat("_LightmapDirectionYaw");

                var dirPitchRad = dirPitch * Mathf.Deg2Rad;
                var dirYawRad = dirYaw * Mathf.Deg2Rad;

                var direction = new Vector4(Mathf.Sin(dirPitchRad) * Mathf.Sin(dirYawRad), Mathf.Cos(dirPitchRad),
                                            Mathf.Sin(dirPitchRad) * Mathf.Cos(dirYawRad), 0.0f);
                _target.SetVector("_LightmapDirection", direction);
            }

            if (displayName.Contains("FOLDOUT")) {
                string foldoutName = displayName.Split('(', ')')[1];
                string foldoutItemCount = displayName.Split('{', '}')[1];
                foldoutRemainingItems = Convert.ToInt32(foldoutItemCount);
                if (!FoldoutStates.ContainsKey(property.name)) {
                    FoldoutStates.Add(property.name, false);
                }

                EditorGUILayout.Space();
                FoldoutStates[property.name] = EditorGUILayout.Foldout(FoldoutStates[property.name], foldoutName);
                latestFoldoutState = FoldoutStates[property.name];
            }

            if (foldoutRemainingItems > 0) {
                skipProperty = skipProperty || !latestFoldoutState;
                EditorGUI.indentLevel += 1;
                --foldoutRemainingItems;
            }

            bool hideInInspector;
#if UNITY_6000_1_OR_NEWER
            hideInInspector = (property.propertyFlags & ShaderPropertyFlags.HideInInspector) != 0;
#else
            hideInInspector = (property.flags & MaterialProperty.PropFlags.HideInInspector) != 0;
#endif
            if (!hideInInspector && !skipProperty) {
                bool disableGroup = false;

                if (baseMapLocked && (property.name == "_TextureImpact" ||
                                      property.name == "_TextureBlendingMode" ||
                                      property.name == "_BaseMapPremultiply")) {
                    disableGroup = true;
                }

                if (detailMapLocked && (property.name == "_DetailMapColor" ||
                                        property.name == "_DetailMapBlendingMode" ||
                                        property.name == "_DetailMapImpact")) {
                    disableGroup = true;
                }

                EditorGUI.BeginChangeCheck();
                if (disableGroup) {
                    EditorGUI.BeginDisabledGroup(true);
                }
                DrawStandard(editor, property);
                if (disableGroup) {
                    EditorGUI.EndDisabledGroup();
                }
                if (EditorGUI.EndChangeCheck()) {
#if UNITY_2021_2_OR_NEWER
                    ValidateMaterial(_target);
#else
                    MaterialChanged(_target);
#endif
                }

                string helpBoxAfterProperty = null;
                if (baseMapLocked && property.name == "_BaseMap") {
                    helpBoxAfterProperty = "Assign an Albedo texture to enable related shading controls.";
                }

                if (detailMapLocked && property.name == "_DetailMap") {
                    helpBoxAfterProperty = "Assign a Detail Map texture to enable its blending controls.";
                }

                if (!string.IsNullOrEmpty(helpBoxAfterProperty)) {
                    EditorGUILayout.HelpBox(helpBoxAfterProperty, MessageType.Info);
                }
            }

            if (!skipProperty && property.name.Contains("_EmissionMap")) {
                EditorGUILayout.Space(15);
                DrawEmissionProperties(_target, true);

                EditorGUILayout.Space(15);
                DrawTileOffset(editor, FindProperty("_BaseMap"));
            }

            EditorGUI.indentLevel = originalIntentLevel;
        }

        EditorGUILayout.Space();
        FoldoutStates["Rendering options"] =
            EditorGUILayout.Foldout(FoldoutStates["Rendering options"], "Rendering options");

        if (FoldoutStates["Rendering options"]) {
            EditorGUI.indentLevel += 1;

            HandleUrpSettings(_target, materialEditor);

            EditorGUILayout.Space();
            materialEditor.EnableInstancingField();
        }

        // Toggle the outline pass.
        _target.SetShaderPassEnabled("SRPDefaultUnlit", _target.IsKeywordEnabled("DR_OUTLINE_ON"));
    }

    // Adapted from BaseShaderGUI.cs.
    private void HandleUrpSettings(Material material, MaterialEditor materialEditor) {
        bool alphaClip = false;
        if (material.HasProperty("_AlphaClip")) {
            alphaClip = material.GetFloat("_AlphaClip") >= 0.5;
        }

        if (alphaClip) {
            material.EnableKeyword("_ALPHATEST_ON");
        } else {
            material.DisableKeyword("_ALPHATEST_ON");
        }

        if (HasProperty("_Surface")) {
            EditorGUI.BeginChangeCheck();
            var surfaceProp = FindProperty("_Surface");
            EditorGUI.showMixedValue = surfaceProp.hasMixedValue;
            var surfaceType = (SurfaceType)surfaceProp.floatValue;
            EditorGUILayout.Separator();
            surfaceType = (SurfaceType)EditorGUILayout.EnumPopup("Surface Type", surfaceType);
            if (EditorGUI.EndChangeCheck()) {
                materialEditor.RegisterPropertyChangeUndo("Surface Type");
                surfaceProp.floatValue = (float)surfaceType;
            }

            if (surfaceType == SurfaceType.Opaque) {
                if (alphaClip) {
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                } else {
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                    material.SetOverrideTag("RenderType", "Opaque");
                }

                material.renderQueue +=
                    material.HasProperty("_QueueOffset") ? (int)material.GetFloat("_QueueOffset") : 0;
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.SetShaderPassEnabled("ShadowCaster", true);
            } else // Transparent
            {
                BlendMode blendMode = (BlendMode)material.GetFloat("_Blend");

                // Specific Transparent Mode Settings
                switch (blendMode) {
                    case BlendMode.Alpha:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;
                    case BlendMode.Premultiply:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;
                    case BlendMode.Additive:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;
                    case BlendMode.Multiply:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.EnableKeyword("_ALPHAMODULATE_ON");
                        break;
                }

                // General Transparent Material Settings
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_ZWrite", 0);
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                material.renderQueue +=
                    material.HasProperty("_QueueOffset") ? (int)material.GetFloat("_QueueOffset") : 0;
                material.SetShaderPassEnabled("ShadowCaster", false);
            }

            // DR: draw popup.
            if (surfaceType == SurfaceType.Transparent && HasProperty("_Blend")) {
                EditorGUI.BeginChangeCheck();
                var blendModeProp = FindProperty("_Blend");
                EditorGUI.showMixedValue = blendModeProp.hasMixedValue;
                var blendMode = (BlendMode)blendModeProp.floatValue;
                blendMode = (BlendMode)EditorGUILayout.EnumPopup("Blend Mode", blendMode);
                if (EditorGUI.EndChangeCheck()) {
                    materialEditor.RegisterPropertyChangeUndo("Blend Mode");
                    blendModeProp.floatValue = (float)blendMode;
                }
            }
        }

        // DR: draw popup.
        if (HasProperty("_Cull")) {
            EditorGUILayout.Separator();
            EditorGUI.BeginChangeCheck();
            var cullingProp = FindProperty("_Cull");
            EditorGUI.showMixedValue = cullingProp.hasMixedValue;
            var culling = (RenderFace)cullingProp.floatValue;
            culling = (RenderFace)EditorGUILayout.EnumPopup("Render Faces", culling);
            if (EditorGUI.EndChangeCheck()) {
                materialEditor.RegisterPropertyChangeUndo("Render Faces");
                cullingProp.floatValue = (float)culling;
                material.doubleSidedGI = (RenderFace)cullingProp.floatValue != RenderFace.Front;
            }
        }

        if (HasProperty("_AlphaClip")) {
            EditorGUILayout.Separator();
            EditorGUI.BeginChangeCheck();
            var alphaClipProp = FindProperty("_AlphaClip");
            EditorGUI.showMixedValue = alphaClipProp.hasMixedValue;
            var alphaClipEnabled = EditorGUILayout.Toggle("Alpha Clipping", alphaClipProp.floatValue == 1);
            if (EditorGUI.EndChangeCheck()) alphaClipProp.floatValue = alphaClipEnabled ? 1 : 0;
            EditorGUI.showMixedValue = false;

            if (alphaClipProp.floatValue == 1 && HasProperty("_Cutoff")) {
                var alphaCutoffProp = FindProperty("_Cutoff");
                materialEditor.ShaderProperty(alphaCutoffProp, "Threshold", 1);
            }
        }
    }

    private static string Rev(string a) {
        StringBuilder b = new StringBuilder(a.Length);
        int i = 0;

        foreach (char c in a) {
            b.Append((char)(c - "8<3F9="[i] % 32));
            i = (i + 1) % 6;
        }

        return b.ToString();
    }

    // Adapted from BaseShaderGUI.cs.
    private new static void SetMaterialKeywords(Material material, Action<Material> shadingModelFunc = null,
                                                Action<Material> shaderFunc = null) {
        // Setup blending - consistent across all Universal RP shaders
        SetupMaterialBlendMode(material);

        // Receive Shadows
        if (material.HasProperty("_ReceiveShadows"))
            CoreUtils.SetKeyword(material, "_RECEIVE_SHADOWS_OFF", material.GetFloat("_ReceiveShadows") == 0.0f);

        // Emission
        if (material.HasProperty("_EmissionColor")) MaterialEditor.FixupEmissiveFlag(material);
        bool shouldEmissionBeEnabled =
            (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
        if (material.HasProperty("_EmissionEnabled") && !shouldEmissionBeEnabled)
            shouldEmissionBeEnabled = material.GetFloat("_EmissionEnabled") >= 0.5f;
        CoreUtils.SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);

        // Normal Map
        if (material.HasProperty("_BumpMap"))
            CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));

        // Shader specific keyword functions
        shadingModelFunc?.Invoke(material);
        shaderFunc?.Invoke(material);
    }
}
}
