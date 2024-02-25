using System;
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
        UpdateTags();
        SortClothing();
    }

    public static void UpdateTags()
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
                    ApparelTagDictionary[tag] = new List<ThingDef> { apparel };
                    continue;
                }

                ApparelTagDictionary[tag].Add(apparel);
            }
        }
    }

    public static void SortClothing()
    {
        var apparelInGame = ThingCategoryDefOf.Apparel.DescendantThingDefs.ToHashSet();

        Log.Message($"Clothing Sorter: Updating {apparelInGame.Count} apparel categories.");

        if (ModLister.GetActiveModWithIdentifier("CETeam.CombatExtended") != null)
        {
            CEArmorModifier = ClothingSorterMod.instance.Settings.CEArmorModifier;
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
            ClothingSorterMod.instance.Settings.SortByLayer, ClothingSorterMod.instance.Settings.SortByTech,
            ClothingSorterMod.instance.Settings.SortByMod, ClothingSorterMod.instance.Settings.SortByTag
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

            if (ClothingSorterMod.instance.Settings.SortSetting == 1)
            {
                (firstOption, secondOption) = (secondOption, firstOption);
            }

            switch (firstOption)
            {
                case NextSortOption.Layer:
                    SortByLayer(apparelInGame, ThingCategoryDefOf.Apparel, secondOption);
                    break;
                case NextSortOption.Tech:
                    SortByTech(apparelInGame, ThingCategoryDefOf.Apparel, secondOption);
                    break;
                case NextSortOption.Mod:
                    SortByMod(apparelInGame, ThingCategoryDefOf.Apparel, secondOption);
                    break;
                case NextSortOption.Tag:
                    SortByTag(apparelInGame, ThingCategoryDefOf.Apparel, secondOption);
                    break;
            }
        }
        else
        {
            if (ClothingSorterMod.instance.Settings.SortByLayer)
            {
                SortByLayer(apparelInGame, ThingCategoryDefOf.Apparel);
            }

            if (ClothingSorterMod.instance.Settings.SortByTech)
            {
                SortByTech(apparelInGame, ThingCategoryDefOf.Apparel);
            }

            if (ClothingSorterMod.instance.Settings.SortByMod)
            {
                SortByMod(apparelInGame, ThingCategoryDefOf.Apparel);
            }

            if (ClothingSorterMod.instance.Settings.SortByTag)
            {
                SortByTag(apparelInGame, ThingCategoryDefOf.Apparel);
            }
        }

        ThingCategoryDefOf.Apparel.ResolveReferences();
        Log.Message("Clothing Sorter: Update done.");
    }

    private static void SortByLayer(HashSet<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        Log.Message($"Sorting by layer, then by {nextSortOption}");
        var layerDefs = (from layerDef in DefDatabase<ApparelLayerDef>.AllDefsListForReading
            orderby layerDef.label
            select layerDef).ToList();
        layerDefs.Add(new ApparelLayerDef { defName = "Layer_None", label = "CS_NoLayer".Translate() });
        var selectedLayers = new List<ApparelLayerDef>();
        for (var layerInt = 0; layerInt < layerDefs.Count; layerInt++)
        {
            if (ClothingSorterMod.instance.Settings.CombineLayers && layerDefs.Count > layerInt + 1 &&
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

            var layerThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(layerDefName);
            if (layerThingCategory == null)
            {
                layerThingCategory = new ThingCategoryDef
                {
                    defName = layerDefName, label = layerDef.label,
                    childSpecialFilters = new List<SpecialThingFilterDef>()
                };
                DefGenerator.AddImpliedDef(layerThingCategory);
            }

            if (nextSortOption == NextSortOption.None)
            {
                AddApparelToCategory(apparelToCheck, layerThingCategory);
                if (layerThingCategory.childThingDefs.Count <= 0 && layerThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(layerThingCategory);
                layerThingCategory.parent = thingCategoryDef;
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Tech:
                        SortByTech(apparelToCheck, layerThingCategory);
                        break;
                    case NextSortOption.Mod:
                        SortByMod(apparelToCheck, layerThingCategory);
                        break;
                    case NextSortOption.Tag:
                        SortByTag(apparelToCheck, layerThingCategory);
                        break;
                }

                if (layerThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(layerThingCategory);
                layerThingCategory.parent = thingCategoryDef;
            }

            thingCategoryDef.ResolveReferences();
        }
    }

    private static void SortByTech(HashSet<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        Log.Message($"Sorting by tech, then by {nextSortOption}");
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
                    childSpecialFilters = new List<SpecialThingFilterDef>()
                };
                DefGenerator.AddImpliedDef(techLevelThingCategory);
            }

            if (nextSortOption == NextSortOption.None)
            {
                AddApparelToCategory(apparelToCheck, techLevelThingCategory);
                if (techLevelThingCategory.childThingDefs.Count <= 0 &&
                    techLevelThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(techLevelThingCategory);
                techLevelThingCategory.parent = thingCategoryDef;
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Layer:
                        SortByLayer(apparelToCheck, techLevelThingCategory);
                        break;
                    case NextSortOption.Mod:
                        SortByMod(apparelToCheck, techLevelThingCategory);
                        break;
                    case NextSortOption.Tag:
                        SortByTag(apparelToCheck, techLevelThingCategory);
                        break;
                }

                if (techLevelThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(techLevelThingCategory);
                techLevelThingCategory.parent = thingCategoryDef;
            }

            thingCategoryDef.ResolveReferences();
        }
    }

    private static void SortByTag(HashSet<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        Log.Message($"Sorting by tag, then by {nextSortOption}");
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
                AddApparelToCategory(apparelToCheck, tagThingCategory);
                if (tagThingCategory.childThingDefs.Count <= 0 && tagThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(tagThingCategory);
                tagThingCategory.parent = thingCategoryDef;
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Layer:
                        SortByLayer(apparelToCheck, tagThingCategory);
                        break;
                    case NextSortOption.Tech:
                        SortByTech(apparelToCheck, tagThingCategory);
                        break;
                    case NextSortOption.Mod:
                        SortByMod(apparelToCheck, tagThingCategory);
                        break;
                }

                if (tagThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(tagThingCategory);
                tagThingCategory.parent = thingCategoryDef;
            }

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
                childSpecialFilters = new List<SpecialThingFilterDef>()
            };
            DefGenerator.AddImpliedDef(missingTagThingCategory);
        }

        if (nextSortOption == NextSortOption.None)
        {
            AddApparelToCategory(missingApparelToCheck, missingTagThingCategory);
            if (missingTagThingCategory.childThingDefs.Count <= 0 &&
                missingTagThingCategory.childCategories.Count <= 0)
            {
                return;
            }

            thingCategoryDef.childCategories.Add(missingTagThingCategory);
            missingTagThingCategory.parent = thingCategoryDef;
        }
        else
        {
            switch (nextSortOption)
            {
                case NextSortOption.Layer:
                    SortByLayer(missingApparelToCheck, missingTagThingCategory);
                    break;
                case NextSortOption.Tech:
                    SortByTech(missingApparelToCheck, missingTagThingCategory);
                    break;
                case NextSortOption.Mod:
                    SortByMod(missingApparelToCheck, missingTagThingCategory);
                    break;
            }

            if (missingTagThingCategory.childCategories.Count <= 0)
            {
                return;
            }

            thingCategoryDef.childCategories.Add(missingTagThingCategory);
            missingTagThingCategory.parent = thingCategoryDef;
        }

        thingCategoryDef.ResolveReferences();
    }

    private static void SortByMod(HashSet<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        Log.Message($"Sorting by mod, then by {nextSortOption}");
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
                    defName = modDefName, label = modData.Name, childSpecialFilters = new List<SpecialThingFilterDef>()
                };
                DefGenerator.AddImpliedDef(modThingCategory);
            }

            if (nextSortOption == NextSortOption.None)
            {
                AddApparelToCategory(apparelToCheck, modThingCategory);
                if (modThingCategory.childThingDefs.Count <= 0 && modThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(modThingCategory);
                modThingCategory.parent = thingCategoryDef;
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Layer:
                        SortByLayer(apparelToCheck, modThingCategory);
                        break;
                    case NextSortOption.Tech:
                        SortByTech(apparelToCheck, modThingCategory);
                        break;
                    case NextSortOption.Tag:
                        SortByTag(apparelToCheck, modThingCategory);
                        break;
                }

                if (modThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(modThingCategory);
                modThingCategory.parent = thingCategoryDef;
            }

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
                childSpecialFilters = new List<SpecialThingFilterDef>()
            };
            DefGenerator.AddImpliedDef(missingModThingCategory);
        }

        if (nextSortOption == NextSortOption.None)
        {
            AddApparelToCategory(missingApparelToCheck, missingModThingCategory);
            if (missingModThingCategory.childThingDefs.Count <= 0 &&
                missingModThingCategory.childCategories.Count <= 0)
            {
                return;
            }

            thingCategoryDef.childCategories.Add(missingModThingCategory);
            missingModThingCategory.parent = thingCategoryDef;
        }
        else
        {
            switch (nextSortOption)
            {
                case NextSortOption.Layer:
                    SortByLayer(missingApparelToCheck, missingModThingCategory);
                    break;
                case NextSortOption.Tech:
                    SortByTech(missingApparelToCheck, missingModThingCategory);
                    break;
                case NextSortOption.Tag:
                    SortByTag(missingApparelToCheck, missingModThingCategory);
                    break;
            }

            if (missingModThingCategory.childCategories.Count <= 0)
            {
                return;
            }

            thingCategoryDef.childCategories.Add(missingModThingCategory);
            missingModThingCategory.parent = thingCategoryDef;
        }

        thingCategoryDef.ResolveReferences();
    }


    /// <summary>
    ///     The sorting of apparel into categories
    /// </summary>
    /// <param name="apparelToSort"></param>
    /// <param name="thingCategoryDef"></param>
    private static void AddApparelToCategory(HashSet<ThingDef> apparelToSort, ThingCategoryDef thingCategoryDef)
    {
        var armoredDefName = $"{thingCategoryDef.defName}_Armored";
        var armoredThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(armoredDefName);
        if (armoredThingCategory == null)
        {
            armoredThingCategory = new ThingCategoryDef
            {
                defName = armoredDefName, label = "CS_Armored".Translate(),
                childSpecialFilters = new List<SpecialThingFilterDef>()
            };
        }

        armoredThingCategory.childCategories.Clear();
        armoredThingCategory.childThingDefs.Clear();

        var psyfocusDefName = $"{thingCategoryDef.defName}_Psyfocus";
        var psyfocusThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(psyfocusDefName);
        if (psyfocusThingCategory == null)
        {
            psyfocusThingCategory = new ThingCategoryDef
            {
                defName = psyfocusDefName, label = "CS_Psyfocus".Translate(),
                childSpecialFilters = new List<SpecialThingFilterDef>()
            };
        }

        psyfocusThingCategory.childCategories.Clear();
        psyfocusThingCategory.childThingDefs.Clear();

        var royaltyDefName = $"{thingCategoryDef.defName}_Royalty";
        var royaltyThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(royaltyDefName);
        if (royaltyThingCategory == null)
        {
            royaltyThingCategory = new ThingCategoryDef
            {
                defName = royaltyDefName, label = "CS_Royalty".Translate(),
                childSpecialFilters = new List<SpecialThingFilterDef>()
            };
        }

        royaltyThingCategory.childCategories.Clear();
        royaltyThingCategory.childThingDefs.Clear();

        var specialDefName = $"{thingCategoryDef.defName}_Special";
        var specialThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(specialDefName);
        if (specialThingCategory == null)
        {
            specialThingCategory = new ThingCategoryDef
            {
                defName = specialDefName, label = "CS_Special".Translate(),
                childSpecialFilters = new List<SpecialThingFilterDef>()
            };
        }

        specialThingCategory.childCategories.Clear();
        specialThingCategory.childThingDefs.Clear();

        var femaleDefName = $"{thingCategoryDef.defName}_Female";
        var femaleThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(femaleDefName);
        if (femaleThingCategory == null)
        {
            femaleThingCategory = new ThingCategoryDef
            {
                defName = femaleDefName, label = "CS_Female".Translate(),
                childSpecialFilters = new List<SpecialThingFilterDef>()
            };
        }

        femaleThingCategory.childCategories.Clear();
        femaleThingCategory.childThingDefs.Clear();

        var maleDefName = $"{thingCategoryDef.defName}_Male";
        var maleThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(maleDefName);
        if (maleThingCategory == null)
        {
            maleThingCategory = new ThingCategoryDef
            {
                defName = maleDefName, label = "CS_Male".Translate(),
                childSpecialFilters = new List<SpecialThingFilterDef>()
            };
        }

        maleThingCategory.childCategories.Clear();
        maleThingCategory.childThingDefs.Clear();

        var mechanitorDefName = $"{thingCategoryDef.defName}_Mechanitor";
        var mechanitorThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(mechanitorDefName);
        if (mechanitorThingCategory == null)
        {
            mechanitorThingCategory = new ThingCategoryDef
            {
                defName = mechanitorDefName, label = "CS_Mechanitor".Translate(),
                childSpecialFilters = new List<SpecialThingFilterDef>()
            };
        }

        mechanitorThingCategory.childCategories.Clear();
        mechanitorThingCategory.childThingDefs.Clear();

        thingCategoryDef.childCategories.Clear();
        thingCategoryDef.childThingDefs.Clear();

        var doPsychicSeparate = ModLister.RoyaltyInstalled && ClothingSorterMod.instance.Settings.PsychicSeparate;
        var doRoyaltySeparate = ModLister.RoyaltyInstalled && ClothingSorterMod.instance.Settings.RoyaltySeparate;
        var doSpecialSeparate = ClothingSorterMod.instance.Settings.SpecialSeparate;
        var doGenderSeparate = ClothingSorterMod.instance.Settings.GenderSeparate;
        var doMechanitorSeparate = ModLister.BiotechInstalled && ClothingSorterMod.instance.Settings.MechanitorSeparate;

        foreach (var apparel in apparelToSort)
        {
            var alreadyAdded = false;

            if (ClothingSorterMod.instance.Settings.ArmoredSeparate)
            {
                if (apparel.StatBaseDefined(StatDefOf.ArmorRating_Blunt) &&
                    apparel.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt) >
                    ClothingSorterMod.instance.Settings.ArmorRating * CEArmorModifier ||
                    apparel.StatBaseDefined(StatDefOf.ArmorRating_Sharp) &&
                    apparel.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp) >
                    ClothingSorterMod.instance.Settings.ArmorRating * CEArmorModifier ||
                    apparel.StatBaseDefined(StatDefOf.StuffEffectMultiplierArmor) &&
                    apparel.GetStatValueAbstract(StatDefOf.StuffEffectMultiplierArmor) >
                    ClothingSorterMod.instance.Settings.ArmorRating * 2 * CEArmorModifier)
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
                    || apparel.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.SlaveSuppressionFallRate) != 0
                    // for some reason SlaveSuppressionFallRate doesn't work above
                    // so we list them manually below TODO / HELP WANTED
                    || apparel.defName == "Apparel_Collar"
                    || apparel.defName == "Apparel_BodyStrap"
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

        if (ClothingSorterMod.instance.Settings.ArmoredSeparate && armoredThingCategory.childThingDefs.Count > 0)
        {
            armoredThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(armoredThingCategory);
            armoredThingCategory.ResolveReferences();
        }

        if (doPsychicSeparate && psyfocusThingCategory.childThingDefs.Count > 0)
        {
            psyfocusThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(psyfocusThingCategory);
            psyfocusThingCategory.ResolveReferences();
        }

        if (doSpecialSeparate && specialThingCategory.childThingDefs.Count > 0)
        {
            specialThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(specialThingCategory);
            specialThingCategory.ResolveReferences();
        }

        if (doGenderSeparate)
        {
            if (femaleThingCategory.childThingDefs.Count > 0)
            {
                femaleThingCategory.parent = thingCategoryDef;
                thingCategoryDef.childCategories.Add(femaleThingCategory);
                femaleThingCategory.ResolveReferences();
            }

            if (maleThingCategory.childThingDefs.Count > 0)
            {
                maleThingCategory.parent = thingCategoryDef;
                thingCategoryDef.childCategories.Add(maleThingCategory);
                maleThingCategory.ResolveReferences();
            }
        }

        if (doMechanitorSeparate && mechanitorThingCategory.childThingDefs.Count > 0)
        {
            mechanitorThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(mechanitorThingCategory);
            mechanitorThingCategory.ResolveReferences();
        }

        if (!doRoyaltySeparate || royaltyThingCategory.childThingDefs.Count <= 0)
        {
            thingCategoryDef.ResolveReferences();
            return;
        }

        royaltyThingCategory.parent = thingCategoryDef;
        thingCategoryDef.childCategories.Add(royaltyThingCategory);
        royaltyThingCategory.ResolveReferences();

        LateAddCategoryToDefDataBase(armoredDefName, armoredThingCategory);
        LateAddCategoryToDefDataBase(psyfocusDefName, psyfocusThingCategory);
        LateAddCategoryToDefDataBase(royaltyDefName, royaltyThingCategory);
        LateAddCategoryToDefDataBase(specialDefName, specialThingCategory);
        LateAddCategoryToDefDataBase(femaleDefName, femaleThingCategory);
        LateAddCategoryToDefDataBase(maleDefName, maleThingCategory);
        LateAddCategoryToDefDataBase(mechanitorDefName, mechanitorThingCategory);
        
        thingCategoryDef.ResolveReferences();
    }

    /// <summary>
    ///     Blocks empty category so that ThingCategoryDefs won't exceed short hash limit 
    /// </summary>
    /// <param name="CategoryDefName"></param>
    /// <param name="thingCategoryDef"></param>
    private static void LateAddCategoryToDefDataBase(String CategoryDefName, ThingCategoryDef thingCategoryDef)
    {
        var notEmpty = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(CategoryDefName) == null && thingCategoryDef.childThingDefs.Count > 0;
        if (notEmpty)
        {
            DefGenerator.AddImpliedDef(thingCategoryDef);
            //Log.Message($"added {CategoryDefName} to db ");
        }
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
