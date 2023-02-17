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
    public static ClothingSorterMod instance;

    private static string currentVersion;

    /// <summary>
    ///     Cunstructor
    /// </summary>
    /// <param name="content"></param>
    public ClothingSorterMod(ModContentPack content) : base(content)
    {
        instance = this;
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
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect);
        GUI.contentColor = Color.yellow;
        listing_Standard.Label("CS_SettingDeselectOptions".Translate());
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
                        listing_Standard.CheckboxLabeled("CS_SettingLayerCategories".Translate(),
                            ref Settings.SortByLayer,
                            "CS_SettingLayerCategoriesDescription".Translate());
                        categories[i] = "CS_SettingLayer".Translate();
                        break;
                    case 1:
                        listing_Standard.CheckboxLabeled("CS_SettingTechCategories".Translate(),
                            ref Settings.SortByTech,
                            "CS_SettingTechCategoriesDescription".Translate());
                        categories[i] = "CS_SettingTech".Translate();
                        break;
                    case 2:
                        listing_Standard.CheckboxLabeled("CS_SettingModCategories".Translate(), ref Settings.SortByMod,
                            "CS_SettingModCategoriesDescription".Translate());
                        categories[i] = "CS_SettingMod".Translate();
                        break;
                    case 3:
                        listing_Standard.CheckboxLabeled("CS_SettingTagCategories".Translate(), ref Settings.SortByTag,
                            "CS_SettingTagCategoriesDescription".Translate());
                        categories[i] = "CS_SettingTag".Translate();
                        break;
                }

                i++;
            }

            listing_Standard.Gap();
            listing_Standard.Label("CS_SettingSortOrder".Translate());
            if (listing_Standard.RadioButton($"{categories[0]} / {categories[1]}",
                    Settings.SortSetting == 0))
            {
                Settings.SortSetting = 0;
            }

            if (listing_Standard.RadioButton($"{categories[1]} / {categories[0]}",
                    Settings.SortSetting == 1))
            {
                Settings.SortSetting = 1;
            }
        }
        else
        {
            listing_Standard.CheckboxLabeled("CS_SettingLayerCategories".Translate(), ref Settings.SortByLayer,
                "CS_SettingLayerCategoriesDescription".Translate());
            listing_Standard.CheckboxLabeled("CS_SettingTechCategories".Translate(), ref Settings.SortByTech,
                "CS_SettingTechCategoriesDescription".Translate());
            listing_Standard.CheckboxLabeled("CS_SettingModCategories".Translate(), ref Settings.SortByMod,
                "CS_SettingModCategoriesDescription".Translate());
            listing_Standard.CheckboxLabeled("CS_SettingTagCategories".Translate(), ref Settings.SortByTag,
                "CS_SettingTagCategoriesDescription".Translate());

            GUI.contentColor = Color.grey;
            listing_Standard.Gap();
            listing_Standard.Label("CS_SettingSortOrder".Translate());
            listing_Standard.Label("/");
            listing_Standard.Label("/");
            GUI.contentColor = Color.white;
        }

        listing_Standard.GapLine();
        if (Settings.SortByLayer)
        {
            listing_Standard.CheckboxLabeled("CS_SettingCombineLayers".Translate(), ref Settings.CombineLayers,
                "CS_SettingCombineLayersDescription".Translate());
        }
        else
        {
            GUI.contentColor = Color.grey;
            listing_Standard.Label("CS_SettingCombineLayers".Translate(), -1,
                "CS_SettingCombineLayersDescription".Translate());
            GUI.contentColor = Color.white;
        }

        listing_Standard.CheckboxLabeled("CS_SettingArmoredCategories".Translate(), ref Settings.ArmoredSeparate,
            "CS_SettingArmoredCategoriesDescription".Translate());
        if (Settings.ArmoredSeparate)
        {
            Settings.ArmorRating = listing_Standard.SliderLabeled(
                "CS_SettingArmoredLowValue".Translate(Math.Round(Settings.ArmorRating * 100)),
                Settings.ArmorRating, 0f, 2f);
            listing_Standard.Gap();
            if (ModLister.GetActiveModWithIdentifier("CETeam.CombatExtended") != null)
            {
                listing_Standard.Label("CS_SettingCEDescription".Translate());
                Settings.CEArmorModifier = listing_Standard.SliderLabeled(
                    "CS_SettingCEArmorModifier".Translate(Settings.CEArmorModifier),
                    Settings.CEArmorModifier, 0f, 10f);
                listing_Standard.Gap();
            }

            listing_Standard.GapLine();
        }

        if (ModLister.RoyaltyInstalled)
        {
            listing_Standard.CheckboxLabeled("CS_SettingPsyfocusCategories".Translate(),
                ref Settings.PsychicSeparate, "CS_SettingPsyfocusCategoriesDescription".Translate());
            listing_Standard.CheckboxLabeled("CS_SettingRoyaltyCategories".Translate(),
                ref Settings.RoyaltySeparate, "CS_SettingRoyaltyCategoriesDescription".Translate());
        }

        listing_Standard.CheckboxLabeled("CS_SettingSpecialCategories".Translate(),
            ref Settings.SpecialSeparate, "CS_SettingSpecialCategoriesDescription".Translate());

        if (ModLister.BiotechInstalled)
        {
            listing_Standard.CheckboxLabeled("CS_SettingMechanitorCategories".Translate(),
                ref Settings.MechanitorSeparate, "CS_SettingMechanitorCategoriesDescription".Translate());
        }

        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("CS_CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        ClothingSorter.SortClothing();
    }
}