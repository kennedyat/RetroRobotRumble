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

public partial struct RunData
{
    // This is kind of a weird way to think about it.
    // There is a "current" run at all times.
    // The end of a run immediately starts the next run.
    public static RunData currentRun;

    public Robot robot;

    // The parts you have available, including the ones you have equipped.
    // public List<ChassisType> availableChassis;
    public List<ArmType> availableArms;
    // public List<LegType> availableLegs;


    // stickers
    // stats
}

public partial struct RunData
{
    public static void EndCurrentRun()
    {
        RunData justEnded;
        (justEnded, currentRun) = (currentRun, new RunData());

        // Interpret the currentRun and produce some value.
        // return out;
    }
}