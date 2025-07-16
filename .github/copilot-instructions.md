# Copilot Instructions for RimWorld Mod: ClothingSorter

## Mod Overview and Purpose

The ClothingSorter mod is designed to enhance the RimWorld experience by providing a more intuitive and efficient clothing management system. The mod aims to streamline the process of organizing, sorting, and selecting clothing for colonists, making it easier for players to manage their colony's apparel resources.

## Key Features and Systems

- **Automated Clothing Sorting**: Provides automatic sorting algorithms to organize clothing items based on various criteria such as quality, condition, and type.
- **Customizable Sorting Preferences**: Allows players to set their own sorting preferences via an in-game settings menu.
- **Enhanced User Interface**: Improves the user interface for clothing management, providing clear visual indicators and sorting options.
- **Efficient Storage Solutions**: Introduces new storage options that integrate with sorted clothing for improved resource management.

## Coding Patterns and Conventions

- **Namespace Usage**: Ensure all classes are encapsulated within a relevant namespace, for example, `namespace ClothingSorterMod`.
- **Class and Method Naming**: Use CamelCase for class names (e.g., `ClothingSorterMod`) and PascalCase for method names. Method names should be descriptive and start with a verb indicating their function (e.g., `InitializeSettings`).
- **Fields and Properties**: Use underscore-prefixed camelCase for private fields (e.g., `_clothingList`) and PascalCase for public properties.

## XML Integration

- **DefModExtensions**: Leverage RimWorld's XML capabilities to extend existing def properties with additional fields necessary for sorting algorithms.
- **Custom XML Tags**: Utilize custom XML definitions to define new sorting categories or preferences that can be recognized by the mod.
- **Localization Support**: Ensure all user-facing strings are localized using XML to support multiple languages and provide a wider reach.

## Harmony Patching

- **Target Specific Methods**: Use Harmony to patch only those methods within RimWorld's core that directly affect clothing management. This minimizes conflicts with other mods.
- **Prefix and Postfix Patches**: Implement both prefix and postfix patches where necessary to modify the logic of existing methods without completely overriding them.
- **Error Handling**: Incorporate robust error handling around Harmony patches to ensure game stability even if a patch fails.

## Suggestions for Copilot

- **Auto-completions for Sorting Algorithms**: Suggest code snippets for common sorting algorithms (e.g., bubble sort, quick sort) that can be adapted to work with RimWorld's clothing items.
- **UI Enhancements**: Propose UI component designs that align with RimWorld's existing look and feel while providing enhanced functionality.
- **Custom XML Definitions**: Provide templates for defining new XML tags and their integration with C# logic.
- **Harmony Patch Examples**: Offer examples of effective Harmony patches, especially how to correctly identify methods to patch within RimWorld's assemblies.
- **Mod Settings Management**: Assist with the creation of intuitive settings management code to allow players to easily customize their sorting preferences.

By following these instructions, developers can effectively utilize GitHub Copilot to streamline the development of the ClothingSorter mod, enhancing both developer productivity and the end-user experience in RimWorld.
