using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmSwap : MonoBehaviour
{
    /*V1 Experiment: Test if replacing the original skinnedmeshrenderer with new and reassigning bones from OG to new will allow the new arm to function (this includes adding the SAME bones to children)*/
    [SerializeField] GameObject originalRArmJoint;
    [SerializeField] GameObject originalLArmJoint;
    [SerializeField] GameObject originalLegJoint;
    [SerializeField] GameObject originalChassisJoint;
    [SerializeField] GameObject swapArmJoint;
    [SerializeField] string nameJoint;

    private SkinnedMeshRenderer[] swapSkinMeshRenderers;

    private Dictionary<string, Dictionary<string, Transform>> lookUpPart = new Dictionary<string, Dictionary<string, Transform>>();
    // Start is called before the first frame update
    void Start()
    {
        swapSkinMeshRenderers = swapArmJoint.GetComponentsInChildren<SkinnedMeshRenderer>();
        CreateTable("LeftArm", originalLArmJoint);
        CreateTable("RightArm", originalRArmJoint);
        CreateTable("Chassis", originalChassisJoint);
        CreateTable("Legs", originalLegJoint);

        
        LookUp(nameJoint, swapArmJoint);

        PrintDictionary(lookUpPart);
    }


    //May need this for left right bone swapping
    private string NormalizeString(string name)
    {
        //change to correct bone naming conventions
        if (name.StartsWith("left_"))
            return name.Substring(4);
        if (name.StartsWith("right_"))
            return name.Substring(5);
        else
            Debug.LogWarning("Invalid naming conventions");

        return name;

    }
    //Creates a lookup table made out of the original parts. 
    private void CreateTable(string name, GameObject part)
    {
        if (!lookUpPart.ContainsKey(name))
            lookUpPart[name] = new Dictionary<string, Transform>();
        SkinnedMeshRenderer[] ogSkinMeshRenderers = part.GetComponentsInChildren<SkinnedMeshRenderer>();
        string temp;
        foreach (var smr in ogSkinMeshRenderers)
        {
            if (smr.rootBone == null)
                continue;

            foreach (Transform t in smr.bones)
            {
                temp = NormalizeString(t.name);

                if (!lookUpPart[name].ContainsKey(temp))
                    lookUpPart[name][temp] = t;
            }

            string rootName = NormalizeString(smr.rootBone.name);
            if (!lookUpPart[name].ContainsKey(rootName))
            {
                lookUpPart[name][rootName] = smr.rootBone;
                Debug.Log($"[{name}] Added root bone: {smr.rootBone.name}");
            }
        }

        
    }

    public void LookUp(string partName, GameObject part)
    {
        foreach (SkinnedMeshRenderer newSMR in swapSkinMeshRenderers)
        {
            TransferBones(partName, newSMR);
        }

    }

    private void TransferBones(string partName, SkinnedMeshRenderer newPart)
    {
        Transform[] newBones = new Transform[newPart.bones.Length];


        // Map each bone expected by newPart to the corresponding one in the live skeleton
        for (int i = 0; i < newPart.bones.Length; i++)
        {
            string boneName = NormalizeString(newPart.bones[i].name);
            
            if (lookUpPart[partName].TryGetValue(boneName, out Transform matchingBone))
            {
                newBones[i] = matchingBone;
                Debug.Log($" Match! Name : {newBones[i]} from {partName}");
            }
            else
            {
                Debug.LogWarning($"Bone {boneName} not found in skeleton! Arm may deform incorrectly.");
                newBones[i] = newPart.bones[i]; // fallback to original bone reference
                Debug.Log($" Not matching... Name : {newBones[i]}");
            }
        }

        newPart.bones = newBones;

        // Also remap the root bone
        if (lookUpPart[partName].TryGetValue(NormalizeString(newPart.rootBone.name), out Transform matchingRoot))
            newPart.rootBone = matchingRoot;
    }
    
    private void PrintDictionary(Dictionary<string, Dictionary<string, Transform>> dict)
{
    foreach (var outerPair in dict)
    {
        Debug.Log($"== {outerPair.Key} =="); // e.g. "LeftArm"
        
        foreach (var innerPair in outerPair.Value)
        {
            Debug.Log($"{innerPair.Key} -> {innerPair.Value.name}");
        }
    }
}



}
