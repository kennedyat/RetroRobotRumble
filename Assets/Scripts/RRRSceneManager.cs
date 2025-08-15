using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// But it is also ok if they don't. There are methods with no params but a longer name.
// A wrapper around SceneManager that makes sense for the game.
//
// Unity's SceneManager is generic, and can't know what's in the scenes.
// We do know what's in our scenes. They are independently runnable, so we don't need funny callback nonsense.
// We won't need LoadAdditive (unless we want to do some funky transitions ig).
// We also know they read from RunData.currentRun.
//
// Anyone can write into RunData.currentRun.
// Ideally, this class should be the only writer.
// Scenes can pass the required info when they change scenes.

public class RRRSceneManager
{
    public static void LoadBuildABot()
    {
        // TODO: Build A Bot isn't independent. We have to initialize it here.
        var thing = SceneManager.LoadSceneAsync("MainBuildABot");

        thing.completed += (AsyncOperation obj) =>
        {
            Scene loadedScene = SceneManager.GetSceneByName("MainBuildABot");
            GameObject gameObject = loadedScene.GetRootGameObjects().First(x => x.name == "BuildABotScreen");
            BuildABotScreen component = gameObject.GetComponent<BuildABotScreen>();

            Debug.Log(RunData.currentRun.availableArms.ToString());
        };
    }

    // If you explicitly want to skip the robot param.
    public static void LoadCombat()
    {
        SceneManager.LoadScene("MainCombat");
    }
}