using UnityEngine;
using SettingsHelper;
using Verse;
using System;
using System.Collections.Generic;

namespace ClothingSorter
{
    [StaticConstructorOnStartup]
    internal class ClothingSorterMod : Mod
    {
        /// <summary>
        /// Cunstructor
        /// </summary>
        /// <param name="content"></param>
        public ClothingSorterMod(ModContentPack content) : base(content)
        {
            instance = this;
        }

        /// <summary>
        /// The instance-settings for the mod
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
        /// The title for the mod-settings
        /// </summary>
        /// <returns></returns>
        public override string SettingsCategory()
        {
            return "Clothing Sorter";
        }

        /// <summary>
        /// The settings-window
        /// For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
        /// </summary>
        /// <param name="rect"></param>
        public override void DoSettingsWindowContents(Rect rect)
        {
            var listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect);
            listing_Standard.Label("SettingDeselectOptions".Translate());
            if (!(Settings.SortByLayer || Settings.SortByTech || Settings.SortByMod))
            {
                Settings.SortByLayer = true;
            }
            if (AtLeastTwo(Settings.SortByLayer, Settings.SortByTech, Settings.SortByMod))
            {
                var categories = new string[2];
                if (Settings.SortByLayer)
                {
                    listing_Standard.CheckboxLabeled("SettingLayerCategories".Translate(), ref Settings.SortByLayer, "SettingLayerCategoriesDescription".Translate());
                    categories[0] = "SettingLayer".Translate();
                } else
                {
                    listing_Standard.Label("SettingLayerCategories".Translate());
                }
                if (Settings.SortByTech)
                {
                    listing_Standard.CheckboxLabeled("SettingTechCategories".Translate(), ref Settings.SortByTech, "SettingTechCategoriesDescription".Translate());
                    if(string.IsNullOrEmpty(categories[0]))
                    {
                        categories[0] = "SettingTech".Translate();
                    } else
                    {
                        categories[1] = "SettingTech".Translate();
                    }
                } else
                {
                    listing_Standard.Label("SettingTechCategories".Translate());
                }
                if (Settings.SortByMod)
                {
                    listing_Standard.CheckboxLabeled("SettingModCategories".Translate(), ref Settings.SortByMod, "SettingModCategoriesDescription".Translate());
                    categories[1] = "SettingMod".Translate();
                } else
                {
                    listing_Standard.Label("SettingModCategories".Translate());
                }               
                listing_Standard.Gap();
                listing_Standard.Label("SettingSortOrder".Translate());
                if (listing_Standard.RadioButton_NewTemp($"{categories[0]} / {categories[1]}", Settings.SortSetting == 0))
                {
                    Settings.SortSetting = 0;
                }
                if (listing_Standard.RadioButton_NewTemp($"{categories[1]} / {categories[0]}", Settings.SortSetting == 1))
                {
                    Settings.SortSetting = 1;
                }
            } else
            {
                    listing_Standard.CheckboxLabeled("SettingLayerCategories".Translate(), ref Settings.SortByLayer, "SettingLayerCategoriesDescription".Translate());
                    listing_Standard.CheckboxLabeled("SettingTechCategories".Translate(), ref Settings.SortByTech, "SettingTechCategoriesDescription".Translate());
                    listing_Standard.CheckboxLabeled("SettingModCategories".Translate(), ref Settings.SortByMod, "SettingModCategoriesDescription".Translate());
            }
            listing_Standard.GapLine();
            if (Settings.SortByLayer)
            {
                listing_Standard.CheckboxLabeled("SettingCombineLayers".Translate(), ref Settings.CombineLayers, "SettingCombineLayersDescription".Translate());
            }
            listing_Standard.CheckboxLabeled("SettingArmoredCategories".Translate(), ref Settings.ArmoredSeparate, "SettingArmoredCategoriesDescription".Translate());
            if (Settings.ArmoredSeparate)
            {
                listing_Standard.AddLabeledSlider(TranslatorFormattedStringExtensions.Translate("SettingArmoredLowValue", Math.Round(Settings.ArmorRating * 100)), ref Settings.ArmorRating, 0f, 2f, "SettingArmoredMin".Translate(), "SettingArmoredMax".Translate(), 0.01f, false);
                listing_Standard.Gap();
                listing_Standard.GapLine();
            }
            if (ModLister.RoyaltyInstalled)
            {
                listing_Standard.CheckboxLabeled("SettingPsyfocusCategories".Translate(), ref Settings.PsychicSeparate, "SettingPsyfocusCategoriesDescription".Translate());
                listing_Standard.CheckboxLabeled("SettingRoyaltyCategories".Translate(), ref Settings.RoyaltySeparate, "SettingRoyaltyCategoriesDescription".Translate());
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
            if(listOfBool.Count != 3)
            {
                return false;
            }
            return AtLeastTwo(listOfBool[0], listOfBool[1], listOfBool[2]);
        }

        private static bool AtLeastTwo(bool a, bool b, bool c)
        {
            return (a && (b || c)) || (b && c);
        }

        /// <summary>
        /// The instance of the settings to be read by the mod
        /// </summary>
        public static ClothingSorterMod instance;

        /// <summary>
        /// The private settings
        /// </summary>
        private ClothingSorterSettings settings;

    }
}
