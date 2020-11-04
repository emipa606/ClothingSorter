using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ClothingSorter
{
    [StaticConstructorOnStartup]
    public class ClothingSorter
    {
        static ClothingSorter()
        {
            SortClothing();
        }

        public static void SortClothing()
        {
            var apparelInGame = (from apparelDef in DefDatabase<ThingDef>.AllDefsListForReading
                                 where apparelDef.IsApparel && apparelDef.thingCategories != null && apparelDef.thingCategories.Count() > 0
                                 select apparelDef).ToList();

            Log.Message($"Clothing Sorter: Updating {apparelInGame.Count} apparel categories.");

            // Clean current tags and categories
            foreach (var apparel in apparelInGame)
            {
                apparel.thingCategories.Clear();
            }
            ThingCategoryDefOf.Apparel.childThingDefs.Clear();
            ThingCategoryDefOf.Apparel.childCategories.Clear();

            if (ClothingSorterMod.instance.Settings.SortByTech && ClothingSorterMod.instance.Settings.SortByLayer)
            {
                if (ClothingSorterMod.instance.Settings.SortSetting == 0)
                {
                    SortByTech(apparelInGame, ThingCategoryDefOf.Apparel, true);
                }
                else
                {
                    SortByLayer(apparelInGame, ThingCategoryDefOf.Apparel, true);
                }
            }
            else
            {
                if (ClothingSorterMod.instance.Settings.SortByLayer)
                {
                    SortByLayer(apparelInGame, ThingCategoryDefOf.Apparel);
                }
                else
                {
                    SortByTech(apparelInGame, ThingCategoryDefOf.Apparel);
                }
            }
            Log.Message($"Clothing Sorter: Update done.");
        }

        private static void SortByLayer(List<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef, bool KeepSorting = false)
        {
            foreach (var layerDef in from layerDef in DefDatabase<ApparelLayerDef>.AllDefsListForReading orderby layerDef.label select layerDef)
            {
                var apparelToCheck = (from apparelDef in apparelToSort where apparelDef.apparel?.layers?.Count() > 0 && apparelDef.apparel.layers.Contains(layerDef) select apparelDef).ToList();
                var layerDefName = $"{thingCategoryDef.defName}_{layerDef}";
                if (thingCategoryDef == ThingCategoryDefOf.Apparel)
                {
                    layerDefName = $"CS_{layerDef}";
                }
                ThingCategoryDef layerThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(layerDefName);
                if (layerThingCategory != null)
                {
                    layerThingCategory.childThingDefs.Clear();
                    layerThingCategory.childCategories.Clear();
                }
                else
                {
                    layerThingCategory = new ThingCategoryDef { defName = layerDefName, label = layerDef.ToString() };
                    DefGenerator.AddImpliedDef(layerThingCategory);
                }
                if (KeepSorting)
                {
                    SortByTech(apparelToCheck, layerThingCategory);
                    if (layerThingCategory.childCategories.Count > 0)
                    {
                        thingCategoryDef.childCategories.Add(layerThingCategory);
                    }
                }
                else
                {
                    AddApparelToCategory(apparelToCheck, layerThingCategory);
                    if (layerThingCategory.childThingDefs.Count > 0)
                    {
                        thingCategoryDef.childCategories.Add(layerThingCategory);
                    }
                }
            }
        }

        private static void SortByTech(List<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef, bool KeepSorting = false)
        {
            foreach (TechLevel techLevel in Enum.GetValues(typeof(TechLevel)))
            {
                var apparelToCheck = (from apparelDef in apparelToSort where apparelDef.techLevel == techLevel select apparelDef).ToList();
                var techLevelDefName = $"{thingCategoryDef.defName}_{techLevel}";
                if (thingCategoryDef == ThingCategoryDefOf.Apparel)
                {
                    techLevelDefName = $"CS_{techLevel}";
                }
                ThingCategoryDef techLevelThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(techLevelDefName);
                if (techLevelThingCategory != null)
                {
                    techLevelThingCategory.childThingDefs.Clear();
                    techLevelThingCategory.childCategories.Clear();
                }
                else
                {
                    techLevelThingCategory = new ThingCategoryDef { defName = techLevelDefName, label = techLevel.ToString() };
                    DefGenerator.AddImpliedDef(techLevelThingCategory);
                }
                if (KeepSorting)
                {
                    SortByLayer(apparelToCheck, techLevelThingCategory);
                    if (techLevelThingCategory.childCategories.Count > 0)
                    {
                        thingCategoryDef.childCategories.Add(techLevelThingCategory);
                    }
                }
                else
                {
                    AddApparelToCategory(apparelToCheck, techLevelThingCategory);
                    if (techLevelThingCategory.childThingDefs.Count > 0)
                    {
                        thingCategoryDef.childCategories.Add(techLevelThingCategory);
                    }
                }
            }
        }


        /// <summary>
        /// The sorting of apparel into categories
        /// </summary>
        /// <param name="apparelToSort"></param>
        /// <param name="thingCategoryDef"></param>
        private static void AddApparelToCategory(List<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef)
        {
            var psyfocusDefName = $"{thingCategoryDef.defName}_Psyfocus";
            ThingCategoryDef psyfocusThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(psyfocusDefName);
            if (psyfocusThingCategory != null)
            {
                psyfocusThingCategory.childThingDefs.Clear();
            }
            else
            {
                psyfocusThingCategory = new ThingCategoryDef { defName = psyfocusDefName, label = "Psyfocus" };
                DefGenerator.AddImpliedDef(psyfocusThingCategory);
            }
            var armoredDefName = $"{thingCategoryDef.defName}_Armored";
            ThingCategoryDef armoredThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(armoredDefName);
            if (armoredThingCategory != null)
            {
                armoredThingCategory.childThingDefs.Clear();
            }
            else
            {
                armoredThingCategory = new ThingCategoryDef { defName = armoredDefName, label = "Armored" };
                DefGenerator.AddImpliedDef(armoredThingCategory);
            }

            foreach (var apparel in apparelToSort)
            {
                if (ModLister.RoyaltyInstalled && ClothingSorterMod.instance.Settings.PsychicSeparate)
                {
                    if (apparel.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.PsychicEntropyRecoveryRate) > 0)
                    {
                        apparel.thingCategories.Add(psyfocusThingCategory);
                        psyfocusThingCategory.childThingDefs.Add(apparel);
                        continue;
                    }
                }
                if (ClothingSorterMod.instance.Settings.ArmoredSeparate)
                {
                    if ((apparel.StatBaseDefined(StatDefOf.ArmorRating_Blunt) && apparel.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt) > ClothingSorterMod.instance.Settings.ArmorRating) ||
                        (apparel.StatBaseDefined(StatDefOf.ArmorRating_Sharp) && apparel.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp) > ClothingSorterMod.instance.Settings.ArmorRating))
                    {
                        apparel.thingCategories.Add(armoredThingCategory);
                        armoredThingCategory.childThingDefs.Add(apparel);
                        continue;
                    }
                }
                apparel.thingCategories.Add(thingCategoryDef);
                thingCategoryDef.childThingDefs.Add(apparel);
            }
            if (ClothingSorterMod.instance.Settings.ArmoredSeparate && armoredThingCategory.childThingDefs.Count > 0)
            {
                armoredThingCategory.parent = thingCategoryDef;
                thingCategoryDef.childCategories.Add(armoredThingCategory);
            }
            if (ModLister.RoyaltyInstalled && ClothingSorterMod.instance.Settings.PsychicSeparate && psyfocusThingCategory.childThingDefs.Count > 0)
            {
                psyfocusThingCategory.parent = thingCategoryDef;
                thingCategoryDef.childCategories.Add(psyfocusThingCategory);
            }
        }
    }
}
