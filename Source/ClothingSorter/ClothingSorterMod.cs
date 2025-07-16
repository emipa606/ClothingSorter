using System;
using System.Collections.Generic;
using Mlie;
using UnityEngine;
using Verse;

namespace ClothingSorter;

[StaticConstructorOnStartup]
internal class ClothingSorterMod : Mod
{
    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static ClothingSorterMod Instance;

    private static string currentVersion;

    /// <summary>
    ///     Cunstructor
    /// </summary>
    /// <param name="content"></param>
    public ClothingSorterMod(ModContentPack content) : base(content)
    {
        Instance = this;
        Settings = GetSettings<ClothingSorterSettings>();
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal ClothingSorterSettings Settings { get; }

    /// <summary>
    ///     The title for the mod-settings
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "Clothing Sorter";
    }

    /// <summary>
    ///     The settings-window
    ///     For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(rect);
        GUI.contentColor = Color.yellow;
        listingStandard.Label("CS_SettingDeselectOptions".Translate());
        GUI.contentColor = Color.white;
        if (!(Settings.SortByLayer || Settings.SortByTech || Settings.SortByMod || Settings.SortByTag))
        {
            Settings.SortByLayer = true;
        }

        var enabledSettings = new List<bool>
            { Settings.SortByLayer, Settings.SortByTech, Settings.SortByMod, Settings.SortByTag };

        if (enabledSettings.Count(b => b.Equals(true)) == 2)
        {
            var categories = new string[2];
            var i = 0;
            for (var j = 0; j < enabledSettings.Count; j++)
            {
                if (!enabledSettings[j])
                {
                    continue;
                }

                switch (j)
                {
                    case 0:
                        listingStandard.CheckboxLabeled("CS_SettingLayerCategories".Translate(),
                            ref Settings.SortByLayer,
                            "CS_SettingLayerCategoriesDescription".Translate());
                        categories[i] = "CS_SettingLayer".Translate();
                        break;
                    case 1:
                        listingStandard.CheckboxLabeled("CS_SettingTechCategories".Translate(),
                            ref Settings.SortByTech,
                            "CS_SettingTechCategoriesDescription".Translate());
                        categories[i] = "CS_SettingTech".Translate();
                        break;
                    case 2:
                        listingStandard.CheckboxLabeled("CS_SettingModCategories".Translate(), ref Settings.SortByMod,
                            "CS_SettingModCategoriesDescription".Translate());
                        categories[i] = "CS_SettingMod".Translate();
                        break;
                    case 3:
                        listingStandard.CheckboxLabeled("CS_SettingTagCategories".Translate(), ref Settings.SortByTag,
                            "CS_SettingTagCategoriesDescription".Translate());
                        categories[i] = "CS_SettingTag".Translate();
                        break;
                }

                i++;
            }

            listingStandard.Gap();
            listingStandard.Label("CS_SettingSortOrder".Translate());
            if (listingStandard.RadioButton($"{categories[0]} / {categories[1]}",
                    Settings.SortSetting == 0))
            {
                Settings.SortSetting = 0;
            }

            if (listingStandard.RadioButton($"{categories[1]} / {categories[0]}",
                    Settings.SortSetting == 1))
            {
                Settings.SortSetting = 1;
            }
        }
        else
        {
            listingStandard.CheckboxLabeled("CS_SettingLayerCategories".Translate(), ref Settings.SortByLayer,
                "CS_SettingLayerCategoriesDescription".Translate());
            listingStandard.CheckboxLabeled("CS_SettingTechCategories".Translate(), ref Settings.SortByTech,
                "CS_SettingTechCategoriesDescription".Translate());
            listingStandard.CheckboxLabeled("CS_SettingModCategories".Translate(), ref Settings.SortByMod,
                "CS_SettingModCategoriesDescription".Translate());
            listingStandard.CheckboxLabeled("CS_SettingTagCategories".Translate(), ref Settings.SortByTag,
                "CS_SettingTagCategoriesDescription".Translate());

            GUI.contentColor = Color.grey;
            listingStandard.Gap();
            listingStandard.Label("CS_SettingSortOrder".Translate());
            listingStandard.Label("/");
            listingStandard.Label("/");
            GUI.contentColor = Color.white;
        }

        listingStandard.GapLine();
        if (Settings.SortByLayer)
        {
            listingStandard.CheckboxLabeled("CS_SettingCombineLayers".Translate(), ref Settings.CombineLayers,
                "CS_SettingCombineLayersDescription".Translate());
            listingStandard.CheckboxLabeled("CS_SettingUniqueLayers".Translate(), ref Settings.UniqueLayers,
                "CS_SettingUniqueLayersDescription".Translate());
        }
        else
        {
            GUI.contentColor = Color.grey;
            listingStandard.Label("CS_SettingCombineLayers".Translate(), -1,
                "CS_SettingCombineLayersDescription".Translate());
            GUI.contentColor = Color.white;
        }

        listingStandard.CheckboxLabeled("CS_SettingArmoredCategories".Translate(), ref Settings.ArmoredSeparate,
            "CS_SettingArmoredCategoriesDescription".Translate());
        if (Settings.ArmoredSeparate)
        {
            Settings.ArmorRating = listingStandard.SliderLabeled(
                "CS_SettingArmoredLowValue".Translate(Math.Round(Settings.ArmorRating * 100)),
                Settings.ArmorRating, 0f, 2f);
            listingStandard.Gap();
            if (ModLister.GetActiveModWithIdentifier("CETeam.CombatExtended", true) != null)
            {
                listingStandard.Label("CS_SettingCEDescription".Translate());
                Settings.CEArmorModifier = listingStandard.SliderLabeled(
                    "CS_SettingCEArmorModifier".Translate(Settings.CEArmorModifier),
                    Settings.CEArmorModifier, 0f, 10f);
                listingStandard.Gap();
            }

            listingStandard.GapLine();
        }

        if (ModLister.RoyaltyInstalled)
        {
            listingStandard.CheckboxLabeled("CS_SettingPsyfocusCategories".Translate(),
                ref Settings.PsychicSeparate, "CS_SettingPsyfocusCategoriesDescription".Translate());
            listingStandard.CheckboxLabeled("CS_SettingRoyaltyCategories".Translate(),
                ref Settings.RoyaltySeparate, "CS_SettingRoyaltyCategoriesDescription".Translate());
        }

        listingStandard.CheckboxLabeled("CS_SettingSpecialCategories".Translate(),
            ref Settings.SpecialSeparate, "CS_SettingSpecialCategoriesDescription".Translate());
        listingStandard.CheckboxLabeled("CS_SettingGenderCategories".Translate(),
            ref Settings.GenderSeparate, "CS_SettingGenderCategoriesDescription".Translate());

        if (ModLister.BiotechInstalled)
        {
            listingStandard.CheckboxLabeled("CS_SettingMechanitorCategories".Translate(),
                ref Settings.MechanitorSeparate, "CS_SettingMechanitorCategoriesDescription".Translate());
        }

        listingStandard.Gap();
        listingStandard.CheckboxLabeled("CS_VerboseLogging".Translate(),
            ref Settings.VerboseLogging, "CS_VerboseLoggingDescription".Translate());

        if (currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("CS_CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        ClothingSorter.SortClothing();
    }
}