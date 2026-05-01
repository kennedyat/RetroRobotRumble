// Data that lives as long as the "run", longer than the individual Unity scenes that make the run.
//
// If you are editing this:
// Remember there are *many* users. Most will ignore most of the fields. 
// Also, all values should have a reasonable default.
// That means nulls are expected, and zeros are meaningful.
//
// If you are reading/writing to this:
// Your scene should work with for both a default currentRun and one that has been written to.
// One idea is to treat nulls as "do nothing," and treat values as overrides.
// Example, you may load a robot matching the override, or use the robot you already have set up in a (test) scene.
// Another idea is to use ?? on nulls with some default.
// Example, you may want to replace empty string (default, which is like null) with an explicit dummy string.
//
// I chose to use a static instead of passing something between scenes.
// The static, being a struct, is never null, and can be reset to default whenever.
// There should ideally only be one reader/writer at a time, the currently active scene.
// But there are no protections for that.
using System.Collections.Generic;

// A singleton for 
//
// This partial block contains fields and methods for interacting with the fields.
public partial struct RunData
{
    public Robot Robot { get => GetRobot(); }

    // The parts you have available, including the ones you have equipped.
    // You are allowed to append to these lists, not remove.
    // You cannot assume these lists are not empty or null.

    public static List<ChassisType> availableChassis;
    public static List<ArmType> availableArms;
    public static List<LegType> availableLegs;
    public static List<PartType> lockedParts;
    public static List<Sticker> availableStickers;
    public static List<Sticker> commonStickers;
    public static List<Sticker> rareStickers;
    public static List<Sticker> legendaryStickers;
    

    // You are allowed to read and write to this freely. Defaults are 0 of course.
    // Avoid making left and right arm equal, but nothing is stopping you from doing so.
    // Nothing also stops you from going OOB.

    public int equippedChassis;
    public int? equippedLeftArm;
    public int? equippedRightArm;
    public int equippedLegs;

    // stickers
    public List<Sticker> equippedStickers;
    // stats

    public Robot GetRobot()
    {
        return new Robot()
        {
            // TODO: This is silly.
            leftArm = equippedLeftArm is int yay ? availableArms[yay] : null,
            rightArm = equippedRightArm is int yay2 ? availableArms[yay2] : null,
            chassis = equippedChassis is int lol ? availableChassis[lol] : null,
            legs =  equippedLegs is int lol2 ? availableLegs[lol2] : null,
            stickers = equippedStickers,
        };
    }
}

// The static currentRun and methods for interacting with the static.
public partial struct RunData
{
    // This is kind of a weird way to think about it.
    // There is a "current" run at all times.
    // The end of a run immediately starts the next run.
    public static RunData currentRun;
    public static int currentRound = 0;
    public static bool test = false;

    public static void EndCurrentRound()
    {
        RunData justEnded;
        (justEnded, currentRun) = (currentRun, new RunData());

        currentRound++;
        RRRSceneManager.LoadBuildABot();
        // Interpret the currentRun and produce some value.
        // return out;
    }
}
