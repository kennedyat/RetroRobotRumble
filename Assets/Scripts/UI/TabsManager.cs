using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabsManager : MonoBehaviour
{
    public GameObject[] Tabs;
    public Image[] TabButtons;
    public Color InactiveColor, ActiveColor;

    public void SwitchToTab(int TabID)
    {
        foreach (GameObject go in Tabs)
        {
            go.SetActive(false);
        }
        Tabs[TabID].SetActive(true);

        foreach (Image im in TabButtons)
        {
            im.color = InactiveColor;
            // We could also make the active button larger in size as well
        }
        TabButtons[TabID].color = ActiveColor;
    }
}
