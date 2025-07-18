﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ClothingSorter;

[StaticConstructorOnStartup]
public class ClothingSorter
{
    private static float CEArmorModifier = 1f;
    private static Dictionary<string, List<ThingDef>> ApparelTagDictionary;

    static ClothingSorter()
    {
        updateTags();
        SortClothing();
    }

    private static void updateTags()
    {
        ApparelTagDictionary = new Dictionary<string, List<ThingDef>>();
        foreach (var apparel in ThingCategoryDefOf.Apparel.DescendantThingDefs)
        {
            if (apparel.apparel?.tags == null || !apparel.apparel.tags.Any())
            {
                continue;
            }

            foreach (var tag in apparel.apparel.tags)
            {
                if (!ApparelTagDictionary.ContainsKey(tag))
                {
                    ApparelTagDictionary[tag] = [apparel];
                    continue;
                }

                ApparelTagDictionary[tag].Add(apparel);
            }
        }
    }

    public static void SortClothing()
    {
        var apparelInGame = ThingCategoryDefOf.Apparel.DescendantThingDefs.ToHashSet();

        logMessage($"Clothing Sorter: Updating {apparelInGame.Count} apparel categories.");

        if (ModLister.GetActiveModWithIdentifier("CETeam.CombatExtended", true) != null)
        {
            CEArmorModifier = ClothingSorterMod.Instance.Settings.CEArmorModifier;
        }

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

        foreach (var category in from categories in DefDatabase<ThingCategoryDef>.AllDefsListForReading
                 where categories.defName.StartsWith("CS_")
                 select categories)
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

        var allSortOptions = new List<bool>
        {
            ClothingSorterMod.Instance.Settings.SortByLayer, ClothingSorterMod.Instance.Settings.SortByTech,
            ClothingSorterMod.Instance.Settings.SortByMod, ClothingSorterMod.Instance.Settings.SortByTag
        };
        if (allSortOptions.Count(b => b.Equals(true)) == 2)
        {
            var firstOption = NextSortOption.None;
            var secondOption = NextSortOption.None;
            for (var j = 0; j < allSortOptions.Count; j++)
            {
                if (!allSortOptions[j])
                {
                    continue;
                }

                firstOption = (NextSortOption)j;
                break;
            }

            for (var j = allSortOptions.Count - 1; j > -1; j--)
            {
                if (!allSortOptions[j])
                {
                    continue;
                }

                secondOption = (NextSortOption)j;
                break;
            }

            if (ClothingSorterMod.Instance.Settings.SortSetting == 1)
            {
                (firstOption, secondOption) = (secondOption, firstOption);
            }

            switch (firstOption)
            {
                case NextSortOption.Layer:
                    sortByLayer(apparelInGame, ThingCategoryDefOf.Apparel, secondOption);
                    break;
                case NextSortOption.Tech:
                    sortByTech(apparelInGame, ThingCategoryDefOf.Apparel, secondOption);
                    break;
                case NextSortOption.Mod:
                    sortByMod(apparelInGame, ThingCategoryDefOf.Apparel, secondOption);
                    break;
                case NextSortOption.Tag:
                    sortByTag(apparelInGame, ThingCategoryDefOf.Apparel, secondOption);
                    break;
            }
        }
        else
        {
            if (ClothingSorterMod.Instance.Settings.SortByLayer)
            {
                sortByLayer(apparelInGame, ThingCategoryDefOf.Apparel);
            }

            if (ClothingSorterMod.Instance.Settings.SortByTech)
            {
                sortByTech(apparelInGame, ThingCategoryDefOf.Apparel);
            }

            if (ClothingSorterMod.Instance.Settings.SortByMod)
            {
                sortByMod(apparelInGame, ThingCategoryDefOf.Apparel);
            }

            if (ClothingSorterMod.Instance.Settings.SortByTag)
            {
                sortByTag(apparelInGame, ThingCategoryDefOf.Apparel);
            }
        }

        ThingCategoryDefOf.Apparel.ResolveReferences();
        logMessage("Clothing Sorter: Update done.");
    }

    private static void sortByLayer(HashSet<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        logMessage($"Sorting {thingCategoryDef} by layer, then by {nextSortOption}");
        var layerDefs = (from layerDef in DefDatabase<ApparelLayerDef>.AllDefsListForReading
            orderby layerDef.label
            select layerDef).ToList();
        layerDefs.Add(new ApparelLayerDef { defName = "Layer_None", label = "CS_NoLayer".Translate() });
        var selectedLayers = new List<ApparelLayerDef>();

        for (var layerInt = 0; layerInt < layerDefs.Count; layerInt++)
        {
            if (ClothingSorterMod.Instance.Settings.CombineLayers && layerDefs.Count > layerInt + 1 &&
                layerDefs[layerInt].label.ToLower() == layerDefs[layerInt + 1].label.ToLower())
            {
                selectedLayers.Add(layerDefs[layerInt]);
                continue;
            }

            var layerDef = layerDefs[layerInt];
            selectedLayers.Add(layerDef);
            var apparelToCheck =
                (from apparelDef in apparelToSort
                    where apparelDef.apparel?.layers?.Count > 0 &&
                          apparelDef.apparel.layers.SharesElementWith(selectedLayers)
                    select apparelDef).ToHashSet();

            var uniqueApparelToCheck =
                (from apparelDef in apparelToCheck
                    where apparelDef.apparel?.layers?.Count == 1
                    select apparelDef).ToHashSet();
            if (layerDef.defName == "Layer_None")
            {
                apparelToCheck =
                    (from apparelDef in apparelToSort
                        where apparelDef.apparel?.layers == null || apparelDef.apparel.layers.Count == 0
                        select apparelDef).ToHashSet();
            }

            selectedLayers.Clear();
            var layerDefName = $"{thingCategoryDef.defName}_{layerDef}";
            if (thingCategoryDef == ThingCategoryDefOf.Apparel)
            {
                layerDefName = $"CS_{layerDef}";
            }

            var layerDefNameUnique = $"{layerDefName}_Single";

            var layerThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(layerDefName);
            if (layerThingCategory == null)
            {
                layerThingCategory = new ThingCategoryDef
                {
                    defName = layerDefName, label = layerDef.label,
                    childSpecialFilters = []
                };
                DefGenerator.AddImpliedDef(layerThingCategory);
            }

            var layerThingCategoryUnique = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(layerDefNameUnique);
            if (layerThingCategoryUnique == null)
            {
                layerThingCategoryUnique = new ThingCategoryDef
                {
                    defName = layerDefNameUnique,
                    label = $"{layerDef.label} ({"CS_UniqueLayerOnly".Translate()})",
                    childSpecialFilters = []
                };
                DefGenerator.AddImpliedDef(layerThingCategoryUnique);
            }

            if (nextSortOption == NextSortOption.None)
            {
                if (ClothingSorterMod.Instance.Settings.UniqueLayers && uniqueApparelToCheck.Any())
                {
                    addApparelToCategory(uniqueApparelToCheck, layerThingCategoryUnique);
                    if (layerThingCategoryUnique.childThingDefs.Count > 0 ||
                        layerThingCategoryUnique.childCategories.Count > 0)
                    {
                        thingCategoryDef.childCategories.Add(layerThingCategoryUnique);
                        layerThingCategoryUnique.parent = thingCategoryDef;
                    }
                }

                addApparelToCategory(apparelToCheck, layerThingCategory);
                if (layerThingCategory.childThingDefs.Count <= 0 && layerThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }
            else
            {
                if (ClothingSorterMod.Instance.Settings.UniqueLayers && uniqueApparelToCheck.Any())
                {
                    switch (nextSortOption)
                    {
                        case NextSortOption.Tech:
                            sortByTech(uniqueApparelToCheck, layerThingCategoryUnique);
                            break;
                        case NextSortOption.Mod:
                            sortByMod(uniqueApparelToCheck, layerThingCategoryUnique);
                            break;
                        case NextSortOption.Tag:
                            sortByTag(uniqueApparelToCheck, layerThingCategoryUnique);
                            break;
                    }

                    if (layerThingCategoryUnique.childCategories.Count > 0)
                    {
                        thingCategoryDef.childCategories.Add(layerThingCategoryUnique);
                        layerThingCategoryUnique.parent = thingCategoryDef;
                    }
                }

                switch (nextSortOption)
                {
                    case NextSortOption.Tech:
                        sortByTech(apparelToCheck, layerThingCategory);
                        break;
                    case NextSortOption.Mod:
                        sortByMod(apparelToCheck, layerThingCategory);
                        break;
                    case NextSortOption.Tag:
                        sortByTag(apparelToCheck, layerThingCategory);
                        break;
                }

                if (layerThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }

            thingCategoryDef.childCategories.Add(layerThingCategory);
            layerThingCategory.parent = thingCategoryDef;

            thingCategoryDef.ResolveReferences();
        }
    }

    private static void sortByTech(HashSet<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        logMessage($"Sorting {thingCategoryDef} by tech, then by {nextSortOption}");
        foreach (TechLevel techLevel in Enum.GetValues(typeof(TechLevel)))
        {
            var apparelToCheck =
                (from apparelDef in apparelToSort where apparelDef.techLevel == techLevel select apparelDef)
                .ToHashSet();
            var techLevelDefName = $"{thingCategoryDef.defName}_{techLevel}";
            if (thingCategoryDef == ThingCategoryDefOf.Apparel)
            {
                techLevelDefName = $"CS_{techLevel}";
            }

            var techLevelThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(techLevelDefName);
            if (techLevelThingCategory == null)
            {
                techLevelThingCategory = new ThingCategoryDef
                {
                    defName = techLevelDefName, label = techLevel.ToStringHuman(),
                    childSpecialFilters = []
                };
                DefGenerator.AddImpliedDef(techLevelThingCategory);
            }

            if (nextSortOption == NextSortOption.None)
            {
                addApparelToCategory(apparelToCheck, techLevelThingCategory);
                if (techLevelThingCategory.childThingDefs.Count <= 0 &&
                    techLevelThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Layer:
                        sortByLayer(apparelToCheck, techLevelThingCategory);
                        break;
                    case NextSortOption.Mod:
                        sortByMod(apparelToCheck, techLevelThingCategory);
                        break;
                    case NextSortOption.Tag:
                        sortByTag(apparelToCheck, techLevelThingCategory);
                        break;
                }

                if (techLevelThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }

            thingCategoryDef.childCategories.Add(techLevelThingCategory);
            techLevelThingCategory.parent = thingCategoryDef;

            thingCategoryDef.ResolveReferences();
        }
    }

    private static void sortByTag(HashSet<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        logMessage($"Sorting by {thingCategoryDef} tag, then by {nextSortOption}");
        foreach (var tag in ApparelTagDictionary.Keys.OrderBy(s => s))
        {
            if (!apparelToSort.SharesElementWith(ApparelTagDictionary[tag]))
            {
                continue;
            }

            var tagCategoryDefName = $"{thingCategoryDef.defName}_tag_{tag}";
            if (thingCategoryDef == ThingCategoryDefOf.Apparel)
            {
                tagCategoryDefName = $"CS_tag_{tag}";
            }

            var tagThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(tagCategoryDefName);
            if (tagThingCategory == null)
            {
                tagThingCategory = new ThingCategoryDef
                    { defName = tagCategoryDefName, label = tag.CapitalizeFirst() };
                DefGenerator.AddImpliedDef(tagThingCategory);
            }

            var apparelToCheck = apparelToSort.Intersect(ApparelTagDictionary[tag]).ToHashSet();

            if (nextSortOption == NextSortOption.None)
            {
                addApparelToCategory(apparelToCheck, tagThingCategory);
                if (tagThingCategory.childThingDefs.Count <= 0 && tagThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Layer:
                        sortByLayer(apparelToCheck, tagThingCategory);
                        break;
                    case NextSortOption.Tech:
                        sortByTech(apparelToCheck, tagThingCategory);
                        break;
                    case NextSortOption.Mod:
                        sortByMod(apparelToCheck, tagThingCategory);
                        break;
                }

                if (tagThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }

            thingCategoryDef.childCategories.Add(tagThingCategory);
            tagThingCategory.parent = thingCategoryDef;

            thingCategoryDef.ResolveReferences();
        }

        var missingApparelToCheck =
            (from apparelDef in apparelToSort
                where apparelDef.apparel?.tags == null || !apparelDef.apparel.tags.Any()
                select apparelDef).ToHashSet();
        if (missingApparelToCheck.Count == 0)
        {
            return;
        }

        var missingTagDefName = $"{thingCategoryDef.defName}_Tag_None";
        if (thingCategoryDef == ThingCategoryDefOf.Apparel)
        {
            missingTagDefName = "CS_Tag_None";
        }

        var missingTagThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(missingTagDefName);
        if (missingTagThingCategory == null)
        {
            missingTagThingCategory = new ThingCategoryDef
            {
                defName = missingTagDefName,
                label = "CS_NoLayer".Translate(),
                childSpecialFilters = []
            };
            DefGenerator.AddImpliedDef(missingTagThingCategory);
        }

        if (nextSortOption == NextSortOption.None)
        {
            addApparelToCategory(missingApparelToCheck, missingTagThingCategory);
            if (missingTagThingCategory.childThingDefs.Count <= 0 &&
                missingTagThingCategory.childCategories.Count <= 0)
            {
                return;
            }
        }
        else
        {
            switch (nextSortOption)
            {
                case NextSortOption.Layer:
                    sortByLayer(missingApparelToCheck, missingTagThingCategory);
                    break;
                case NextSortOption.Tech:
                    sortByTech(missingApparelToCheck, missingTagThingCategory);
                    break;
                case NextSortOption.Mod:
                    sortByMod(missingApparelToCheck, missingTagThingCategory);
                    break;
            }

            if (missingTagThingCategory.childCategories.Count <= 0)
            {
                return;
            }
        }

        thingCategoryDef.childCategories.Add(missingTagThingCategory);
        missingTagThingCategory.parent = thingCategoryDef;

        thingCategoryDef.ResolveReferences();
    }

    private static void sortByMod(HashSet<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        logMessage($"Sorting {thingCategoryDef} by mod, then by {nextSortOption}");
        foreach (var modData in from modData in ModLister.AllInstalledMods where modData.Active select modData)
        {
            var apparelToCheck = (from apparelDef in apparelToSort
                where apparelDef.modContentPack?.PackageId != null &&
                      apparelDef.modContentPack.PackageId == modData.PackageId
                select apparelDef).ToHashSet();
            var modDefName = $"{thingCategoryDef.defName}_{modData.PackageId}";
            if (thingCategoryDef == ThingCategoryDefOf.Apparel)
            {
                modDefName = $"CS_{modData.PackageId}";
            }

            var modThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(modDefName);
            if (modThingCategory == null)
            {
                modThingCategory = new ThingCategoryDef
                {
                    defName = modDefName, label = modData.Name, childSpecialFilters = []
                };
                DefGenerator.AddImpliedDef(modThingCategory);
            }

            if (nextSortOption == NextSortOption.None)
            {
                addApparelToCategory(apparelToCheck, modThingCategory);
                if (modThingCategory.childThingDefs.Count <= 0 && modThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Layer:
                        sortByLayer(apparelToCheck, modThingCategory);
                        break;
                    case NextSortOption.Tech:
                        sortByTech(apparelToCheck, modThingCategory);
                        break;
                    case NextSortOption.Tag:
                        sortByTag(apparelToCheck, modThingCategory);
                        break;
                }

                if (modThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }

            thingCategoryDef.childCategories.Add(modThingCategory);
            modThingCategory.parent = thingCategoryDef;

            thingCategoryDef.ResolveReferences();
        }

        var missingApparelToCheck =
            (from apparelDef in apparelToSort
                where apparelDef.modContentPack?.PackageId == null
                select apparelDef).ToHashSet();
        if (missingApparelToCheck.Count == 0)
        {
            return;
        }

        var missingModDefName = $"{thingCategoryDef.defName}_Mod_None";
        if (thingCategoryDef == ThingCategoryDefOf.Apparel)
        {
            missingModDefName = "CS_Mod_None";
        }

        var missingModThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(missingModDefName);
        if (missingModThingCategory == null)
        {
            missingModThingCategory = new ThingCategoryDef
            {
                defName = missingModDefName, label = "CS_NoLayer".Translate(),
                childSpecialFilters = []
            };
            DefGenerator.AddImpliedDef(missingModThingCategory);
        }

        if (nextSortOption == NextSortOption.None)
        {
            addApparelToCategory(missingApparelToCheck, missingModThingCategory);
            if (missingModThingCategory.childThingDefs.Count <= 0 &&
                missingModThingCategory.childCategories.Count <= 0)
            {
                return;
            }
        }
        else
        {
            switch (nextSortOption)
            {
                case NextSortOption.Layer:
                    sortByLayer(missingApparelToCheck, missingModThingCategory);
                    break;
                case NextSortOption.Tech:
                    sortByTech(missingApparelToCheck, missingModThingCategory);
                    break;
                case NextSortOption.Tag:
                    sortByTag(missingApparelToCheck, missingModThingCategory);
                    break;
            }

            if (missingModThingCategory.childCategories.Count <= 0)
            {
                return;
            }
        }

        thingCategoryDef.childCategories.Add(missingModThingCategory);
        missingModThingCategory.parent = thingCategoryDef;

        thingCategoryDef.ResolveReferences();
    }


    /// <summary>
    ///     The sorting of apparel into categories
    /// </summary>
    /// <param name="apparelToSort"></param>
    /// <param name="thingCategoryDef"></param>
    private static void addApparelToCategory(HashSet<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef)
    {
        var armoredDefName = $"{thingCategoryDef.defName}_Armored";
        var armoredThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(armoredDefName) ??
                                   new ThingCategoryDef
                                   {
                                       defName = armoredDefName, label = "CS_Armored".Translate(),
                                       childSpecialFilters = []
                                   };

        armoredThingCategory.childCategories.Clear();
        armoredThingCategory.childThingDefs.Clear();

        var psyfocusDefName = $"{thingCategoryDef.defName}_Psyfocus";
        var psyfocusThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(psyfocusDefName) ??
                                    new ThingCategoryDef
                                    {
                                        defName = psyfocusDefName, label = "CS_Psyfocus".Translate(),
                                        childSpecialFilters = []
                                    };

        psyfocusThingCategory.childCategories.Clear();
        psyfocusThingCategory.childThingDefs.Clear();

        var royaltyDefName = $"{thingCategoryDef.defName}_Royalty";
        var royaltyThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(royaltyDefName) ??
                                   new ThingCategoryDef
                                   {
                                       defName = royaltyDefName, label = "CS_Royalty".Translate(),
                                       childSpecialFilters = []
                                   };

        royaltyThingCategory.childCategories.Clear();
        royaltyThingCategory.childThingDefs.Clear();

        var specialDefName = $"{thingCategoryDef.defName}_Special";
        var specialThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(specialDefName) ??
                                   new ThingCategoryDef
                                   {
                                       defName = specialDefName, label = "CS_Special".Translate(),
                                       childSpecialFilters = []
                                   };

        specialThingCategory.childCategories.Clear();
        specialThingCategory.childThingDefs.Clear();

        var femaleDefName = $"{thingCategoryDef.defName}_Female";
        var femaleThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(femaleDefName) ??
                                  new ThingCategoryDef
                                  {
                                      defName = femaleDefName, label = "CS_Female".Translate(),
                                      childSpecialFilters = []
                                  };

        femaleThingCategory.childCategories.Clear();
        femaleThingCategory.childThingDefs.Clear();

        var maleDefName = $"{thingCategoryDef.defName}_Male";
        var maleThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(maleDefName) ?? new ThingCategoryDef
        {
            defName = maleDefName, label = "CS_Male".Translate(),
            childSpecialFilters = []
        };

        maleThingCategory.childCategories.Clear();
        maleThingCategory.childThingDefs.Clear();

        var mechanitorDefName = $"{thingCategoryDef.defName}_Mechanitor";
        var mechanitorThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(mechanitorDefName) ??
                                      new ThingCategoryDef
                                      {
                                          defName = mechanitorDefName, label = "CS_Mechanitor".Translate(),
                                          childSpecialFilters = []
                                      };

        mechanitorThingCategory.childCategories.Clear();
        mechanitorThingCategory.childThingDefs.Clear();

        thingCategoryDef.childCategories.Clear();
        thingCategoryDef.childThingDefs.Clear();

        var doPsychicSeparate = ModLister.RoyaltyInstalled && ClothingSorterMod.Instance.Settings.PsychicSeparate;
        var doRoyaltySeparate = ModLister.RoyaltyInstalled && ClothingSorterMod.Instance.Settings.RoyaltySeparate;
        var doSpecialSeparate = ClothingSorterMod.Instance.Settings.SpecialSeparate;
        var doGenderSeparate = ClothingSorterMod.Instance.Settings.GenderSeparate;
        var doMechanitorSeparate = ModLister.BiotechInstalled && ClothingSorterMod.Instance.Settings.MechanitorSeparate;

        foreach (var apparel in apparelToSort)
        {
            var alreadyAdded = false;

            if (ClothingSorterMod.Instance.Settings.ArmoredSeparate)
            {
                if (apparel.StatBaseDefined(StatDefOf.ArmorRating_Blunt) &&
                    apparel.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt) >
                    ClothingSorterMod.Instance.Settings.ArmorRating * CEArmorModifier ||
                    apparel.StatBaseDefined(StatDefOf.ArmorRating_Sharp) &&
                    apparel.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp) >
                    ClothingSorterMod.Instance.Settings.ArmorRating * CEArmorModifier ||
                    apparel.StatBaseDefined(StatDefOf.StuffEffectMultiplierArmor) &&
                    apparel.GetStatValueAbstract(StatDefOf.StuffEffectMultiplierArmor) >
                    ClothingSorterMod.Instance.Settings.ArmorRating * 2 * CEArmorModifier)
                {
                    apparel.thingCategories.Add(armoredThingCategory);
                    armoredThingCategory.childThingDefs.Add(apparel);
                    alreadyAdded = true;
                }
            }

            if (doPsychicSeparate)
            {
                if (apparel.apparel?.tags?.Contains("Psychic") == true)
                {
                    apparel.thingCategories.Add(psyfocusThingCategory);
                    psyfocusThingCategory.childThingDefs.Add(apparel);
                    alreadyAdded = true;
                }
            }

            if (doRoyaltySeparate)
            {
                if (apparel.apparel?.tags?.Contains("Royal") == true)
                {
                    apparel.thingCategories.Add(royaltyThingCategory);
                    royaltyThingCategory.childThingDefs.Add(apparel);
                    alreadyAdded = true;
                }
            }

            if (doGenderSeparate)
            {
                switch (apparel.apparel?.gender)
                {
                    case Gender.Female:
                        apparel.thingCategories.Add(femaleThingCategory);
                        femaleThingCategory.childThingDefs.Add(apparel);
                        alreadyAdded = true;
                        break;
                    case Gender.Male:
                        apparel.thingCategories.Add(maleThingCategory);
                        maleThingCategory.childThingDefs.Add(apparel);
                        alreadyAdded = true;
                        break;
                }
            }

            if (doSpecialSeparate)
            {
                if (
                    apparel.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.PainShockThreshold) != 0
                    || apparel.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.ShootingAccuracyPawn) != 0
                    || apparel.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.RangedCooldownFactor) != 0
                    || apparel.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.PsychicSensitivity) != 0
                    || apparel.defName == "Apparel_TortureCrown"
                    || apparel.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.ToxicEnvironmentResistance) != 0
                    || apparel.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.SuppressionPower) != 0
                    || apparel.equippedStatOffsets.GetStatOffsetFromList(
                        DefDatabase<StatDef>.GetNamedSilentFail("SlaveSuppressionOffset")) > 0

                    // It should work now, at least for defs below
                    //|| apparel.defName == "Apparel_Collar"
                    //|| apparel.defName == "Apparel_BodyStrap"
                )
                {
                    apparel.thingCategories.Add(specialThingCategory);
                    specialThingCategory.childThingDefs.Add(apparel);
                    alreadyAdded = true;
                }
            }

            if (doMechanitorSeparate)
            {
                if (
                    apparel.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.MechBandwidth) != 0
                    || apparel.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.MechControlGroups) != 0
                )
                {
                    apparel.thingCategories.Add(mechanitorThingCategory);
                    mechanitorThingCategory.childThingDefs.Add(apparel);
                    alreadyAdded = true;
                }
            }

            if (alreadyAdded)
            {
                continue;
            }

            apparel.thingCategories.Add(thingCategoryDef);
            thingCategoryDef.childThingDefs.Add(apparel);
        }

        if (ClothingSorterMod.Instance.Settings.ArmoredSeparate && armoredThingCategory.childThingDefs.Count > 0)
        {
            lateAddCategoryToDefDataBase(armoredDefName, armoredThingCategory);
            armoredThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(armoredThingCategory);
            armoredThingCategory.ResolveReferences();
        }

        if (doPsychicSeparate && psyfocusThingCategory.childThingDefs.Count > 0)
        {
            lateAddCategoryToDefDataBase(psyfocusDefName, psyfocusThingCategory);
            psyfocusThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(psyfocusThingCategory);
            psyfocusThingCategory.ResolveReferences();
        }

        if (doSpecialSeparate && specialThingCategory.childThingDefs.Count > 0)
        {
            lateAddCategoryToDefDataBase(specialDefName, specialThingCategory);
            specialThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(specialThingCategory);
            specialThingCategory.ResolveReferences();
        }

        if (doGenderSeparate)
        {
            if (femaleThingCategory.childThingDefs.Count > 0)
            {
                lateAddCategoryToDefDataBase(femaleDefName, femaleThingCategory);
                femaleThingCategory.parent = thingCategoryDef;
                thingCategoryDef.childCategories.Add(femaleThingCategory);
                femaleThingCategory.ResolveReferences();
            }

            if (maleThingCategory.childThingDefs.Count > 0)
            {
                lateAddCategoryToDefDataBase(maleDefName, maleThingCategory);
                maleThingCategory.parent = thingCategoryDef;
                thingCategoryDef.childCategories.Add(maleThingCategory);
                maleThingCategory.ResolveReferences();
            }
        }

        if (doMechanitorSeparate && mechanitorThingCategory.childThingDefs.Count > 0)
        {
            lateAddCategoryToDefDataBase(mechanitorDefName, mechanitorThingCategory);
            mechanitorThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(mechanitorThingCategory);
            mechanitorThingCategory.ResolveReferences();
        }

        if (doRoyaltySeparate && royaltyThingCategory.childThingDefs.Count > 0)
        {
            lateAddCategoryToDefDataBase(royaltyDefName, royaltyThingCategory);
            royaltyThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(royaltyThingCategory);
            royaltyThingCategory.ResolveReferences();
        }

        thingCategoryDef.ResolveReferences();
    }

    /// <summary>
    ///     Blocks empty category so that ThingCategoryDefs won't exceed short hash limit
    /// </summary>
    /// <param name="categoryDefName"></param>
    /// <param name="thingCategoryDef"></param>
    private static void lateAddCategoryToDefDataBase(string categoryDefName, ThingCategoryDef thingCategoryDef)
    {
        if (DefDatabase<ThingCategoryDef>.GetNamedSilentFail(categoryDefName) != null)
        {
            return;
        }

        DefGenerator.AddImpliedDef(thingCategoryDef);
        logMessage($"Added {categoryDefName} to def-database");
    }

    private static void logMessage(string message)
    {
        if (!ClothingSorterMod.Instance.Settings.VerboseLogging)
        {
            return;
        }

        Log.Message($"[ClothingSorter]: {message}");
    }

    private enum NextSortOption
    {
        Layer = 0,
        Tech = 1,
        Mod = 2,
        Tag = 3,
        None = 4
    }
}