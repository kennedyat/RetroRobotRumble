using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using AK.Wwise;

public class MainMenu : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event clickSFX;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = CastRay();
            clickSFX.Post(gameObject);   // click sound

            if (hit.collider != null)
            {
                if (hit.collider.gameObject.name == "Play") { StartGame(); }
                if (hit.collider.gameObject.name == "Quit") { QuitGame(); }
                if (hit.collider.gameObject.name == "Settings") { OpenSettings(); }
            }
        }
    }

    private RaycastHit CastRay()
    {
        Vector3 screenMousePosFar = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane);

        Vector3 screenMousePosNear = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);

        Vector3 worldMousePosFar = Camera.main.ScreenToWorldPoint(screenMousePosFar);
        Vector3 worldMousePosNear = Camera.main.ScreenToWorldPoint(screenMousePosNear);
        RaycastHit hit;
        Physics.Raycast(worldMousePosNear, worldMousePosFar - worldMousePosNear, out hit);

        return hit;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainInitializer");
    }
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
    }
}
