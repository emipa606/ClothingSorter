using System;
using System.Collections.Generic;
using Mlie;
using SettingsHelper;
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
    ///     The private settings
    /// </summary>
    private ClothingSorterSettings settings;

    /// <summary>
    ///     Cunstructor
    /// </summary>
    /// <param name="content"></param>
    public ClothingSorterMod(ModContentPack content) : base(content)
    {
        instance = this;
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(ModLister.GetActiveModWithIdentifier("Mlie.ClothingSorter"));
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal ClothingSorterSettings Settings
    {
        get
        {
            if (settings == null)
            {
                settings = GetSettings<ClothingSorterSettings>();
            }

            return settings;
        }
        set => settings = value;
    }

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
        if (!(Settings.SortByLayer || Settings.SortByTech || Settings.SortByMod))
        {
            Settings.SortByLayer = true;
        }

        if (AtLeastTwo(Settings.SortByLayer, Settings.SortByTech, Settings.SortByMod))
        {
            var categories = new string[2];
            if (Settings.SortByLayer)
            {
                listing_Standard.CheckboxLabeled("CS_SettingLayerCategories".Translate(), ref Settings.SortByLayer,
                    "CS_SettingLayerCategoriesDescription".Translate());
                categories[0] = "CS_SettingLayer".Translate();
            }
            else
            {
                GUI.contentColor = Color.grey;
                listing_Standard.Label("CS_SettingLayerCategories".Translate(), -1,
                    "CS_SettingDeselectOptions".Translate());
                GUI.contentColor = Color.white;
            }

            if (Settings.SortByTech)
            {
                listing_Standard.CheckboxLabeled("CS_SettingTechCategories".Translate(), ref Settings.SortByTech,
                    "CS_SettingTechCategoriesDescription".Translate());
                if (string.IsNullOrEmpty(categories[0]))
                {
                    categories[0] = "CS_SettingTech".Translate();
                }
                else
                {
                    categories[1] = "CS_SettingTech".Translate();
                }
            }
            else
            {
                GUI.contentColor = Color.grey;
                listing_Standard.Label("CS_SettingTechCategories".Translate(), -1,
                    "CS_SettingDeselectOptions".Translate());
                GUI.contentColor = Color.white;
            }

            if (Settings.SortByMod)
            {
                listing_Standard.CheckboxLabeled("CS_SettingModCategories".Translate(), ref Settings.SortByMod,
                    "CS_SettingModCategoriesDescription".Translate());
                categories[1] = "CS_SettingMod".Translate();
            }
            else
            {
                GUI.contentColor = Color.grey;
                listing_Standard.Label("CS_SettingModCategories".Translate(), -1,
                    "CS_SettingDeselectOptions".Translate());
                GUI.contentColor = Color.white;
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
            listing_Standard.AddLabeledSlider(
                "CS_SettingArmoredLowValue".Translate(Math.Round(Settings.ArmorRating * 100)),
                ref Settings.ArmorRating, 0f, 2f, "CS_SettingArmoredMin".Translate(),
                "CS_SettingArmoredMax".Translate(), 0.01f);
            listing_Standard.Gap();
            if (ModLister.GetActiveModWithIdentifier("CETeam.CombatExtended") != null)
            {
                listing_Standard.Label("CS_SettingCEDescription".Translate());
                listing_Standard.AddLabeledSlider("CS_SettingCEArmorModifier".Translate(Settings.CEArmorModifier),
                    ref Settings.CEArmorModifier, 0f, 10f, null, null, 0.1f);
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

        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("CS_CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();

        Settings.Write();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        ClothingSorter.SortClothing();
    }

    public static bool AtLeastTwo(List<bool> listOfBool)
    {
        if (listOfBool.Count != 3)
        {
            return false;
        }

        return AtLeastTwo(listOfBool[0], listOfBool[1], listOfBool[2]);
    }

    private static bool AtLeastTwo(bool a, bool b, bool c)
    {
        return a && (b || c) || b && c;
    }
}