using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogFileManager : MonoBehaviour
{
    // Define a log file path
    [SerializeField] private string logFilePath;

    void Start() 
    {
        // Set up the log file path
        // logFilePath = Path.Combine(Application.persistentDataPath, "testLog.txt");
        Debug.Log("File Path: " + logFilePath);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        if (scene.name == "MainMenu_WINTER") 
        {
            CreateLogFile();
        }
    }

    private void CreateLogFile() 
    {
        string logContent = $"Player test session ended at {System.DateTime.Now}\n";

        if (!File.Exists(logFilePath)) 
        {
            File.WriteAllText(logFilePath, logContent);
        }
        else 
        {
            File.AppendAllText(logFilePath, logContent);
        }

        Debug.Log("Log written: " + logContent);
    }
}
