using UnityEngine.SceneManagement;

// But it is also ok if they don't. There are methods with no params but a longer name.
// A wrapper around SceneManager that makes sense for the game.
//
// Unity's SceneManager is generic, and can't know what's in the scenes.
// We do know what's in our scenes. They are independently runnable, so we don't need funny callback nonsense.
// We also don't need LoadAdditive.
// We also know they read from RunData.currentRun.
//
// Anyone can write into RunData.currentRun.
// Ideally, this class should be the only writer.
// Scenes can pass the required info when they change scenes.

public class RRRSceneManager
{
    public static void LoadBuildABot(bool allowChassis)
    {
        // TODO: The Equip Screen isn't independent. We have to initialize it here.
        SceneManager.LoadScene("EquipScreenHarness");
    }

    public static void LoadCombat(Robot robot)
    {
        RunData.currentRun.robot = robot;
        LoadCombatAlreadyInit();
    }

    // If you explicitly want to skip the robot param.
    public static void LoadCombatAlreadyInit()
    {
        // SceneManager.LoadScene("EquipScreenHarness");
    }
}