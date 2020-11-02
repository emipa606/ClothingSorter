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

            Log.Message($"Clothing Sorter: Updating {apparelInGame.Count} items.");
            var layersInGame = new List<ApparelLayerDef> {
                ApparelLayerDefOf.Overhead,
                ApparelLayerDefOf.OnSkin,
                ApparelLayerDefOf.Middle,
                ApparelLayerDefOf.Shell,
                ApparelLayerDefOf.Belt
            };

            // Clean current tags
            var existingCategories = new HashSet<ThingCategoryDef>();
            foreach (var apparel in apparelInGame)
            {
                existingCategories.AddRange(apparel.thingCategories);
                apparel.thingCategories.Clear();
            }

            var categoriesToClear = (from category in DefDatabase<ThingCategoryDef>.AllDefsListForReading
                                     where category.defName.StartsWith("CS_")
                                     select category).ToList();
            foreach (var category in categoriesToClear)
            {
                category.childThingDefs.Clear();
            }
            ThingCategoryDefOf.Apparel.childThingDefs.Clear();
            ThingCategoryDefOf.Apparel.childCategories.Clear();

            foreach (var layer in layersInGame)
            {
                var thingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail($"CS_{layer.defName}");
                if (thingCategory == null)
                {
                    Log.Warning($"Clothing Sorter: ThingCategoryDef named CS_{layer.defName} could not be found.");
                    continue;
                }
                ThingCategoryDefOf.Apparel.childCategories.Add(thingCategory);
                var armoredThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail($"CS_Armored_{layer.defName}");
                if (ClothingSorterMod.instance.Settings.ArmoredSeparate && armoredThingCategory == null)
                {
                    Log.Warning($"Clothing Sorter: ThingCategoryDef named CS_Armored_{layer.defName} could not be found.");
                    continue;
                }
                var psyfocusThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail($"CS_Psyfocus_{layer.defName}");
                if (ModLister.RoyaltyInstalled && ClothingSorterMod.instance.Settings.PsychicSeparate && psyfocusThingCategory == null)
                {
                    Log.Warning($"Clothing Sorter: ThingCategoryDef named CS_Psyfocus_{layer.defName} could not be found.");
                    continue;
                }
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
                        if (apparel.StatBaseDefined(StatDefOf.ArmorRating_Blunt) || apparel.StatBaseDefined(StatDefOf.ArmorRating_Sharp))
                        {
                            apparel.thingCategories.Add(armoredThingCategory);
                            armoredThingCategory.childThingDefs.Add(apparel);
                            continue;
                        }
                    }
                    apparel.thingCategories.Add(thingCategory);
                    thingCategory.childThingDefs.Add(apparel);
                }
                //armoredThingCategory.ResolveReferences();
                //thingCategory.ResolveReferences();
            }
            //ThingCategoryDefOf.Apparel.ResolveReferences();
            Log.Message($"Clothing Sorter: Update done.");
        }
    }
}
