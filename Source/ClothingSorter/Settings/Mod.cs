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
            if (!Settings.SortByLayer && !Settings.SortByTech) Settings.SortByLayer = true;
            listing_Standard.CheckboxLabeled("Layer categories", ref Settings.SortByLayer, "Should apparel be sorted on layers?");
            listing_Standard.CheckboxLabeled("Tech-level categories", ref Settings.SortByTech, "Should apparel be sorted on tech-level?"); 
            if (Settings.SortByTech && Settings.SortByLayer)
            {
                listing_Standard.AddLabeledRadioList(null, TechSortSettingOptions, ref Settings.TechSortSetting, 0);
                listing_Standard.Gap();
                listing_Standard.GapLine();
            }
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled("Armored sub-categories", ref Settings.ArmoredSeparate, "Should apparel with armor-values be sorted in a sub-category?");
            if (Settings.ArmoredSeparate)
            {
                listing_Standard.AddLabeledSlider($"Low value to count as armored: {Math.Round(Settings.ArmorRating * 100)}%", ref Settings.ArmorRating, 0f, 2f, "0% armor", "200% armor", 0.01f, false);
                listing_Standard.Gap();
                listing_Standard.GapLine();
            }
            if (ModLister.RoyaltyInstalled)
            {
                listing_Standard.CheckboxLabeled("Psyfocus sub-categories", ref Settings.PsychicSeparate, "Should apparel with psyfocus-values be sorted in a sub-category?");
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

        public static string[] TechSortSettingOptions = new string[] { "Tech-level first, then layer", "Layer first, then tech-level" };

        /// <summary>
        /// The private settings
        /// </summary>
        private ClothingSorterSettings settings;

    }
}
