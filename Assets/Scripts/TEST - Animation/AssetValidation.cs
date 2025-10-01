using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class AssetValidation
{
    public struct ValidationResult
    {
        public bool allBonesFound;
        public List<string> missingBones;
        public List<string> foundBones;
        public int totalRequiredBones;
        public int foundBonesCount;
        public string partName; // e.g., "LeftArm", "RightArm", etc.
    }

    public struct FullValidationResult
    {
        public bool isValid;
        public Dictionary<string, ValidationResult> partResults;
        public string summaryMessage;
    }

    private Dictionary<string, HashSet<string>> requiredBonesLookup = new Dictionary<string, HashSet<string>>();

    // Initialize with your original skeleton parts
    public void Initialize(GameObject originalRArmJoint, GameObject originalLArmJoint, 
                          GameObject originalLegJoint, GameObject originalChassisJoint)
    {
        requiredBonesLookup.Clear();
        
        BuildBoneList("RightArm", originalRArmJoint);
        BuildBoneList("LeftArm", originalLArmJoint);
        BuildBoneList("Legs", originalLegJoint);
        BuildBoneList("Chassis", originalChassisJoint);
    }

    // Alternative: Initialize from an FBX path (for editor use)
    public void InitializeFromFBX(string originalFBXPath)
    {
        requiredBonesLookup.Clear();
        
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(originalFBXPath);
        if (asset == null)
        {
            Debug.LogError($"Could not load original FBX at path: {originalFBXPath}");
            return;
        }

        // Get all SkinnedMeshRenderers to extract bone hierarchy
        var smrs = asset.GetComponentsInChildren<SkinnedMeshRenderer>();
        
        if (smrs.Length == 0)
        {
            Debug.LogWarning("No SkinnedMeshRenderers found in original FBX");
            return;
        }

        // Build a master bone list from all renderers
        BuildBoneListFromSMRs("Master", smrs);
    }

    private void BuildBoneList(string partName, GameObject part)
    {
        if (!requiredBonesLookup.ContainsKey(partName))
            requiredBonesLookup[partName] = new HashSet<string>();

        var smrs = part.GetComponentsInChildren<SkinnedMeshRenderer>();
        
        foreach (var smr in smrs)
        {
            if (smr.rootBone != null)
            {
                string rootName = NormalizeString(smr.rootBone.name);
                requiredBonesLookup[partName].Add(rootName);
            }

            foreach (Transform bone in smr.bones)
            {
                if (bone != null)
                {
                    string boneName = NormalizeString(bone.name);
                    requiredBonesLookup[partName].Add(boneName);
                }
            }
        }
    }

    private void BuildBoneListFromSMRs(string partName, SkinnedMeshRenderer[] smrs)
    {
        if (!requiredBonesLookup.ContainsKey(partName))
            requiredBonesLookup[partName] = new HashSet<string>();

        foreach (var smr in smrs)
        {
            if (smr.rootBone != null)
            {
                string rootName = NormalizeString(smr.rootBone.name);
                requiredBonesLookup[partName].Add(rootName);
            }

            foreach (Transform bone in smr.bones)
            {
                if (bone != null)
                {
                    string boneName = NormalizeString(bone.name);
                    requiredBonesLookup[partName].Add(boneName);
                }
            }
        }
    }

    // Validate an FBX file path
    public FullValidationResult ValidateFBX(string fbxPath)
    {
        var fullResult = new FullValidationResult();
        fullResult.partResults = new Dictionary<string, ValidationResult>();

        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (asset == null)
        {
            fullResult.isValid = false;
            fullResult.summaryMessage = "Could not load FBX asset";
            return fullResult;
        }

        return ValidateGameObject(asset);
    }

    // Validate a GameObject (works for both runtime and editor)
    public FullValidationResult ValidateGameObject(GameObject targetObject)
    {
        var fullResult = new FullValidationResult();
        fullResult.partResults = new Dictionary<string, ValidationResult>();

        if (requiredBonesLookup.Count == 0)
        {
            fullResult.isValid = false;
            fullResult.summaryMessage = "Validator not initialized. Call Initialize() first.";
            return fullResult;
        }

        // Extract all bones from the target FBX
        var targetBones = new HashSet<string>();
        var smrs = targetObject.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var smr in smrs)
        {
            if (smr.rootBone != null)
            {
                targetBones.Add(NormalizeString(smr.rootBone.name));
            }

            foreach (Transform bone in smr.bones)
            {
                if (bone != null)
                {
                    targetBones.Add(NormalizeString(bone.name));
                }
            }
        }

        // Also check the entire transform hierarchy (in case bones aren't in SMR)
        var allTransforms = targetObject.GetComponentsInChildren<Transform>();
        foreach (var t in allTransforms)
        {
            targetBones.Add(NormalizeString(t.name));
        }

        // Validate each part
        bool allPartsValid = true;
        foreach (var kvp in requiredBonesLookup)
        {
            var partResult = ValidatePart(kvp.Key, kvp.Value, targetBones);
            fullResult.partResults[kvp.Key] = partResult;
            
            if (!partResult.allBonesFound)
                allPartsValid = false;
        }

        fullResult.isValid = allPartsValid;
        fullResult.summaryMessage = GenerateSummary(fullResult);

        return fullResult;
    }

    private ValidationResult ValidatePart(string partName, HashSet<string> requiredBones, HashSet<string> targetBones)
    {
        var result = new ValidationResult();
        result.partName = partName;
        result.missingBones = new List<string>();
        result.foundBones = new List<string>();
        result.totalRequiredBones = requiredBones.Count;

        foreach (var requiredBone in requiredBones)
        {
            if (targetBones.Contains(requiredBone))
            {
                result.foundBones.Add(requiredBone);
            }
            else
            {
                result.missingBones.Add(requiredBone);
            }
        }

        result.foundBonesCount = result.foundBones.Count;
        result.allBonesFound = result.missingBones.Count == 0;

        return result;
    }

    private string NormalizeString(string name)
    {
        // Remove left_/right_ prefixes for comparison
        if (name.StartsWith("left_"))
            return name.Substring(5);
        if (name.StartsWith("right_"))
            return name.Substring(6);
        
        return name;
    }

    private string GenerateSummary(FullValidationResult result)
    {
        if (result.isValid)
        {
            return "All required bones found in FBX!";
        }

        var summary = "Bone validation failed:\n";
        foreach (var kvp in result.partResults)
        {
            if (!kvp.Value.allBonesFound)
            {
                summary += $"\n{kvp.Key}: {kvp.Value.foundBonesCount}/{kvp.Value.totalRequiredBones} bones found";
                summary += $"\nMissing: {string.Join(", ", kvp.Value.missingBones)}";
            }
        }

        return summary;
    }

    // Helper method to get detailed report as formatted string
    public string GetDetailedReport(FullValidationResult result)
    {
        var report = "";
        report += $"<b>Overall Status:</b> {(result.isValid ? "✓ VALID" : "✗ INVALID")}\n\n";

        foreach (var kvp in result.partResults)
        {
            var partResult = kvp.Value;
            report += $"<b>{kvp.Key}:</b>\n";
            report += $"  Found: {partResult.foundBonesCount}/{partResult.totalRequiredBones} bones\n";
            
            if (partResult.allBonesFound)
            {
                report += $"  <color=#4CAF50>✓ All bones present</color>\n\n";
            }
            else
            {
                report += $"  <color=#F44336>✗ Missing {partResult.missingBones.Count} bones:</color>\n";
                foreach (var missing in partResult.missingBones)
                {
                    report += $"    - {missing}\n";
                }
                report += "\n";
            }
        }

        return report;
    }
}