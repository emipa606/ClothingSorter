using Verse;

namespace ClothingSorter;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class ClothingSorterSettings : ModSettings
{
    public bool ArmoredSeparate = true;
    public float ArmorRating = 0.2f;
    public float CEArmorModifier = 5f;
    public bool CombineLayers = true;
    public bool GenderSeparate;
    public bool MechanitorSeparate;
    public bool PsychicSeparate = true;
    public bool RoyaltySeparate;
    public bool SortByLayer = true;
    public bool SortByMod;
    public bool SortByTag;
    public bool SortByTech;
    public int SortSetting;
    public bool SpecialSeparate;
    public bool VerboseLogging;

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref SortByLayer, "SortByLayer", true);
        Scribe_Values.Look(ref VerboseLogging, "VerboseLogging");
        Scribe_Values.Look(ref SortByTech, "SortByTech");
        Scribe_Values.Look(ref SortByMod, "SortByMod");
        Scribe_Values.Look(ref SortByTag, "SortByTag");
        Scribe_Values.Look(ref SortSetting, "SortSetting");
        Scribe_Values.Look(ref CombineLayers, "CombineLayers", true);
        Scribe_Values.Look(ref ArmoredSeparate, "ArmoredSeparate", true);
        Scribe_Values.Look(ref ArmorRating, "ArmorRating", 0.2f);
        Scribe_Values.Look(ref PsychicSeparate, "PsychicSeparate", true);
        Scribe_Values.Look(ref RoyaltySeparate, "RoyaltySeparate");
        Scribe_Values.Look(ref SpecialSeparate, "SpecialSeparate");
        Scribe_Values.Look(ref GenderSeparate, "GenderSeparate");
        Scribe_Values.Look(ref MechanitorSeparate, "MechanitorSeparate");
        Scribe_Values.Look(ref CEArmorModifier, "CEArmorModifier", 5f);
    }
}