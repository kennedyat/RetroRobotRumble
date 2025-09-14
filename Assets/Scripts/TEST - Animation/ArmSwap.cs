using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmSwap : MonoBehaviour
{
    /*V1 Experiment: Test if replacing the original skinnedmeshrenderer with new and reassigning bones from OG to new will allow the new arm to function (this includes adding the SAME bones to children)*/
    [SerializeField] GameObject originalArmJoint;
    [SerializeField] GameObject swapArmJoint;


    private SkinnedMeshRenderer[] ogSkinMeshRenderers;
    private SkinnedMeshRenderer[] swapSkinMeshRenderers;

    private Dictionary<string, Transform> lookUpPart = new Dictionary<string, Transform>();
    // Start is called before the first frame update
    void Start()
    {
        ogSkinMeshRenderers = originalArmJoint.GetComponentsInChildren<SkinnedMeshRenderer>();

        swapSkinMeshRenderers = swapArmJoint.GetComponentsInChildren<SkinnedMeshRenderer>();



        CreateTable();
        LookUp();
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    //May need this for left right bone swapping
    /*public string NormalizeString(string name)
    {
        if (name.EndsWith("_R") || name.EndsWith("_L")) //change to correct bone naming conventions
            return name.Substring(0, name.Length - 2);

        return name;

    }*/
    //Creates a lookup table made out of the original parts. 
    public void CreateTable()
    {
        foreach (var smr in ogSkinMeshRenderers)
        {
            if (smr.rootBone == null)
                continue;

            foreach (Transform t in smr.rootBone.GetComponentsInChildren<Transform>())
            {
                if (!lookUpPart.ContainsKey(t.name))
                    lookUpPart[t.name] = t;
            }
        }
    }

    public void LookUp()
    {
        foreach (SkinnedMeshRenderer newSMR in swapSkinMeshRenderers)
        {
           TransferBones(newSMR);
        }

    }

    public void TransferBones(SkinnedMeshRenderer newPart)
    {
        Transform[] newBones = new Transform[newPart.bones.Length];


        // Map each bone expected by newPart to the corresponding one in the live skeleton
        for (int i = 0; i < newPart.bones.Length; i++)
        {
            string boneName = newPart.bones[i].name;
            if (lookUpPart.TryGetValue(boneName, out Transform matchingBone))
            {
                newBones[i] = matchingBone;
            }
            else
            {
                Debug.LogWarning($"Bone {boneName} not found in skeleton! Arm may deform incorrectly.");
                newBones[i] = newPart.bones[i]; // fallback to original bone reference
            }
        }

        newPart.bones = newBones;

        // Also remap the root bone
        if (lookUpPart.TryGetValue(newPart.rootBone.name, out Transform matchingRoot))
            newPart.rootBone = matchingRoot;
    }



}
