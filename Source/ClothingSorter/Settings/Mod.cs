using UnityEngine;
using Verse;

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
            set
            {
                settings = value;
            }
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
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect);
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled("Armored sub-categories", ref Settings.ArmoredSeparate, "Should apparel with armor-values be sorted in a sub-category?");
            if(ModLister.RoyaltyInstalled)
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

        /// <summary>
        /// The private settings
        /// </summary>
        private ClothingSorterSettings settings;

    }
}
