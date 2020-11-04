using UnityEngine;
using SettingsHelper;
using Verse;
using System;

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
            if (!Settings.SortByLayer && !Settings.SortByTech)
            {
                Settings.SortByLayer = true;
            }

            listing_Standard.CheckboxLabeled("SettingLayerCategories".Translate(), ref Settings.SortByLayer, "SettingLayerCategoriesDescription".Translate());
            listing_Standard.CheckboxLabeled("SettingTechCategories".Translate(), ref Settings.SortByTech, "SettingTechCategoriesDescription"); 
            if (Settings.SortByTech && Settings.SortByLayer)
            {
                listing_Standard.Gap();
                if(listing_Standard.RadioButton_NewTemp("SettingTechThenLayer".Translate(), Settings.SortSetting == 0))
                {
                    Settings.SortSetting = 0;
                }
                if (listing_Standard.RadioButton_NewTemp("SettingLayerThenTech".Translate(), Settings.SortSetting == 1))
                {
                    Settings.SortSetting = 1;
                }
                //listing_Standard.AddLabeledRadioList(null, TechSortSettingOptions, ref TechSortSettingOptions[Settings.SortSetting], 0);
                listing_Standard.GapLine();
            }
            listing_Standard.Gap();
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
            }
            listing_Standard.End();

            Settings.Write();
        }

        public override void WriteSettings()
        {
            
            base.WriteSettings();
            ClothingSorter.SortClothing();
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
