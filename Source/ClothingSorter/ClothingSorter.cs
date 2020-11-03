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
            var layersInGame = (from layerDef in DefDatabase<ApparelLayerDef>.AllDefsListForReading orderby layerDef.label select layerDef).ToList();
            Log.Message($"Clothing Sorter: Updating {apparelInGame.Count} apparel based on {layersInGame.Count} layers.");

            var customCategories = (from category in DefDatabase<ThingCategoryDef>.AllDefsListForReading
                                    where category.defName.StartsWith("CS_")
                                    select category).ToList();
            var customCategoriesStrings = (from category in DefDatabase<ThingCategoryDef>.AllDefsListForReading
                                           where category.defName.StartsWith("CS_")
                                           select category.defName).ToList();
            foreach (var layer in layersInGame)
            {
                foreach (var prefix in new List<string> { "CS", "CS_Psyfocus", "CS_Armored" })
                {
                    if (customCategoriesStrings.Contains($"{prefix}_{layer.defName}"))
                    {
                        continue;
                    }
                    var categoryLabel = prefix == "CS" ? layer.label : prefix.Replace("CS_", "");
                    var layerCategory = new ThingCategoryDef { defName = $"{prefix}_{layer.defName}", label = categoryLabel };
                    DefGenerator.AddImpliedDef(layerCategory);
                    customCategories.Add(layerCategory);
                }
            }

            // Clean current tags and categories
            foreach (var apparel in apparelInGame)
            {
                apparel.thingCategories.Clear();
            }
            foreach (var category in customCategories)
            {
                category.childThingDefs.Clear();
                category.childCategories.Clear();
            }
            ThingCategoryDefOf.Apparel.childThingDefs.Clear();
            ThingCategoryDefOf.Apparel.childCategories.Clear();

            foreach (var layer in layersInGame)
            {
                var thingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail($"CS_{layer.defName}");
                var armoredThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail($"CS_Armored_{layer.defName}");
                var psyfocusThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail($"CS_Psyfocus_{layer.defName}");

                foreach (var apparel in from apparelDef in apparelInGame where apparelDef.apparel?.layers?.Count() > 0 && apparelDef.apparel.layers.Contains(layer) select apparelDef)
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
                    apparel.thingCategories.Add(thingCategory);
                    thingCategory.childThingDefs.Add(apparel);
                }
                if (ClothingSorterMod.instance.Settings.ArmoredSeparate && armoredThingCategory.childThingDefs.Count > 0)
                {
                    armoredThingCategory.parent = thingCategory;
                    thingCategory.childCategories.Add(armoredThingCategory);
                }
                if (ModLister.RoyaltyInstalled && ClothingSorterMod.instance.Settings.PsychicSeparate && psyfocusThingCategory.childThingDefs.Count > 0)
                {
                    psyfocusThingCategory.parent = thingCategory;
                    thingCategory.childCategories.Add(psyfocusThingCategory);
                }
                if (thingCategory.childThingDefs.Count == 0 && thingCategory.childCategories.Count == 0)
                {
                    continue;
                }
                thingCategory.parent = ThingCategoryDefOf.Apparel;
                ThingCategoryDefOf.Apparel.childCategories.Add(thingCategory);
                //Log.Message($"Clothing Sorter: Generated {thingCategory.defName}, childItems {string.Join(",", thingCategory.childThingDefs)}, child categories {string.Join(",", thingCategory.childCategories)}");
            }
            //Log.Message($"Clothing Sorter: Apparel has childItems {string.Join(",", ThingCategoryDefOf.Apparel.childThingDefs)}, child categories {string.Join(",", ThingCategoryDefOf.Apparel.childCategories)}");
            Log.Message($"Clothing Sorter: Update done.");
        }
    }
}
