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
            var apparelInGame = ThingCategoryDefOf.Apparel.DescendantThingDefs.ToHashSet();
            //var apparelInGame = (from apparelDef in DefDatabase<ThingDef>.AllDefsListForReading
            //                     where apparelDef.IsApparel && apparelDef.thingCategories != null && apparelDef.thingCategories.Count() > 0
            //                     select apparelDef).ToList();

            Log.Message($"Clothing Sorter: Updating {apparelInGame.Count} apparel categories.");

            foreach (var category in ThingCategoryDefOf.Apparel.ThisAndChildCategoryDefs)
            {
                category.childThingDefs.Clear();
                category.childCategories.Clear();
                if (category.parent != ThingCategoryDefOf.Root)
                {
                    category.parent = null;
                }
                category.ClearCachedData();
            }
            foreach( var category in from categories in DefDatabase<ThingCategoryDef>.AllDefsListForReading where categories.defName.StartsWith("CS_") select categories)
            {
                category.childThingDefs.Clear();
                category.childCategories.Clear();
                if (category.parent != ThingCategoryDefOf.Root)
                {
                    category.parent = null;
                }
                category.ClearCachedData();
            }
            // Clean current tags and categories
            foreach (var apparel in apparelInGame)
            {
                apparel.thingCategories.Clear();
            }

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
            //ThingCategoryDefOf.Apparel.ResolveReferences();
            Log.Message($"Clothing Sorter: Update done.");
        }

        private static void SortByLayer(HashSet<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef, bool KeepSorting = false)
        {
            var layerDefs = (from layerDef in DefDatabase<ApparelLayerDef>.AllDefsListForReading orderby layerDef.label select layerDef).ToList();
            layerDefs.Add(new ApparelLayerDef { defName = "Layer_None", label = "NoLayer".Translate() });
            var selectedLayers = new List<ApparelLayerDef>();
            for (var layerInt = 0; layerInt < layerDefs.Count(); layerInt++)
            {
                if (ClothingSorterMod.instance.Settings.CombineLayers && layerDefs.Count() > layerInt + 1 && layerDefs[layerInt].label.ToLower() == layerDefs[layerInt + 1].label.ToLower())
                {
                    selectedLayers.Add(layerDefs[layerInt]);
                    continue;
                }
                var layerDef = layerDefs[layerInt];
                selectedLayers.Add(layerDef);
                var apparelToCheck = (from apparelDef in apparelToSort where apparelDef.apparel?.layers?.Count() > 0 && apparelDef.apparel.layers.SharesElementWith(selectedLayers) select apparelDef).ToHashSet();
                if (layerDef.defName == "Layer_None")
                {
                    apparelToCheck = (from apparelDef in apparelToSort where apparelDef.apparel == null || apparelDef.apparel.layers == null || apparelDef.apparel.layers.Count() == 0 select apparelDef).ToHashSet();
                }
                selectedLayers.Clear();
                var layerDefName = $"{thingCategoryDef.defName}_{layerDef}";
                if (thingCategoryDef == ThingCategoryDefOf.Apparel)
                {
                    layerDefName = $"CS_{layerDef}";
                }
                ThingCategoryDef layerThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(layerDefName);
                if (layerThingCategory == null)
                {
                    layerThingCategory = new ThingCategoryDef { defName = layerDefName, label = layerDef.label };
                    DefGenerator.AddImpliedDef(layerThingCategory);
                }
                if (KeepSorting)
                {
                    SortByTech(apparelToCheck, layerThingCategory);
                    if (layerThingCategory.childCategories.Count > 0)
                    {
                        thingCategoryDef.childCategories.Add(layerThingCategory);
                        layerThingCategory.parent = thingCategoryDef;
                    }
                }
                else
                {
                    AddApparelToCategory(apparelToCheck, layerThingCategory);
                    if (layerThingCategory.childThingDefs.Count > 0 || layerThingCategory.childCategories.Count > 0)
                    {
                        thingCategoryDef.childCategories.Add(layerThingCategory);
                        layerThingCategory.parent = thingCategoryDef;
                    }
                }
                //layerThingCategory.ResolveReferences();
            }
        }

        private static void SortByTech(HashSet<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef, bool KeepSorting = false)
        {
            foreach (TechLevel techLevel in Enum.GetValues(typeof(TechLevel)))
            {
                var apparelToCheck = (from apparelDef in apparelToSort where apparelDef.techLevel == techLevel select apparelDef).ToHashSet();
                var techLevelDefName = $"{thingCategoryDef.defName}_{techLevel}";
                if (thingCategoryDef == ThingCategoryDefOf.Apparel)
                {
                    techLevelDefName = $"CS_{techLevel}";
                }
                ThingCategoryDef techLevelThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(techLevelDefName);
                if (techLevelThingCategory == null)
                {
                    techLevelThingCategory = new ThingCategoryDef { defName = techLevelDefName, label = techLevel.ToStringHuman() };
                    DefGenerator.AddImpliedDef(techLevelThingCategory);
                }
                if (KeepSorting)
                {
                    SortByLayer(apparelToCheck, techLevelThingCategory);
                    if (techLevelThingCategory.childCategories.Count > 0)
                    {
                        thingCategoryDef.childCategories.Add(techLevelThingCategory);
                        techLevelThingCategory.parent = thingCategoryDef;
                    }
                }
                else
                {
                    AddApparelToCategory(apparelToCheck, techLevelThingCategory);
                    if (techLevelThingCategory.childThingDefs.Count > 0 || techLevelThingCategory.childCategories.Count > 0)
                    {
                        thingCategoryDef.childCategories.Add(techLevelThingCategory);
                        techLevelThingCategory.parent = thingCategoryDef;
                    }
                }
                //techLevelThingCategory.ResolveReferences();
            }
        }


        /// <summary>
        /// The sorting of apparel into categories
        /// </summary>
        /// <param name="apparelToSort"></param>
        /// <param name="thingCategoryDef"></param>
        private static void AddApparelToCategory(HashSet<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef)
        {
            var armoredDefName = $"{thingCategoryDef.defName}_Armored";
            ThingCategoryDef armoredThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(armoredDefName);
            if (armoredThingCategory == null)
            {
                armoredThingCategory = new ThingCategoryDef { defName = armoredDefName, label = "CS_Armored".Translate() };
                DefGenerator.AddImpliedDef(armoredThingCategory);
            }
            armoredThingCategory.childCategories.Clear();
            armoredThingCategory.childThingDefs.Clear();
            var psyfocusDefName = $"{thingCategoryDef.defName}_Psyfocus";
            ThingCategoryDef psyfocusThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(psyfocusDefName);
            if (psyfocusThingCategory == null)
            {
                psyfocusThingCategory = new ThingCategoryDef { defName = psyfocusDefName, label = "CS_Psyfocus".Translate() };
                DefGenerator.AddImpliedDef(psyfocusThingCategory);
            }
            psyfocusThingCategory.childCategories.Clear();
            psyfocusThingCategory.childThingDefs.Clear();
            var royaltyDefName = $"{thingCategoryDef.defName}_Royalty";
            ThingCategoryDef royaltyThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(royaltyDefName);
            if (royaltyThingCategory == null)
            {
                royaltyThingCategory = new ThingCategoryDef { defName = royaltyDefName, label = "CS_Royalty".Translate() };
                DefGenerator.AddImpliedDef(royaltyThingCategory);
            }
            royaltyThingCategory.childCategories.Clear();
            royaltyThingCategory.childThingDefs.Clear();
            thingCategoryDef.childCategories.Clear();
            thingCategoryDef.childThingDefs.Clear();
            foreach (var apparel in apparelToSort)
            {
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
                if (ModLister.RoyaltyInstalled && ClothingSorterMod.instance.Settings.PsychicSeparate)
                {
                    if (apparel.apparel.tags != null && apparel.apparel.tags.Contains("Psychic"))
                    {
                        apparel.thingCategories.Add(psyfocusThingCategory);
                        psyfocusThingCategory.childThingDefs.Add(apparel);
                        continue;
                    }
                }
                if (ModLister.RoyaltyInstalled && ClothingSorterMod.instance.Settings.RoyaltySeparate)
                {
                    if (apparel.apparel.tags != null && apparel.apparel.tags.Contains("Royal"))
                    {
                        apparel.thingCategories.Add(royaltyThingCategory);
                        royaltyThingCategory.childThingDefs.Add(apparel);
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
                //armoredThingCategory.ResolveReferences();
            }
            if (ModLister.RoyaltyInstalled && ClothingSorterMod.instance.Settings.PsychicSeparate && psyfocusThingCategory.childThingDefs.Count > 0)
            {
                psyfocusThingCategory.parent = thingCategoryDef;
                thingCategoryDef.childCategories.Add(psyfocusThingCategory);
                //psyfocusThingCategory.ResolveReferences();
            }
            if (ModLister.RoyaltyInstalled && ClothingSorterMod.instance.Settings.RoyaltySeparate && royaltyThingCategory.childThingDefs.Count > 0)
            {
                royaltyThingCategory.parent = thingCategoryDef;
                thingCategoryDef.childCategories.Add(royaltyThingCategory);
                //psyfocusThingCategory.ResolveReferences();
            }
            //thingCategoryDef.ResolveReferences();
        }
    }
}
