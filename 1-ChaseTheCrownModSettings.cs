using System.Collections.Generic;

// Add settings to your mod by implementing IModSettings.
// IModSettings extends IAutoBoundMod,
// which makes an object of the type available in other scripts via Inject attribute.
// Mod settings are saved to file when the app is closed.
public class ChaseTheCrownModSettings : IModSettings
{
    public int getCrownOnThisManyPerfectNotes = 4;
    public double multiplier = 1.1;

    public double maxMultiplier = 1.5;

    public int minMsNoteLength = 140;


    public List<IModSettingControl> GetModSettingControls()
    {
        return new List<IModSettingControl>()
        {
              new DoubleModSettingControl(() => multiplier, newValue => multiplier = newValue) { Label = "Multiplier" },
              new DoubleModSettingControl(() => maxMultiplier, newValue => maxMultiplier = newValue) { Label = "Max Multiplier" },
              new IntModSettingControl(() => getCrownOnThisManyPerfectNotes, newValue => getCrownOnThisManyPerfectNotes = newValue) { Label = "Perfect notes needed to get crown" },
              new IntModSettingControl(() => minMsNoteLength, newValue => minMsNoteLength = newValue) { Label = "Ignore notes under this length in ms" },
        };
    }
}
