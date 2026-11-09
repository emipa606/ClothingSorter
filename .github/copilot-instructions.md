# GitHub Copilot Instructions for Clothing Sorter

## Mod Overview and Purpose

Clothing Sorter is a RimWorld mod that sorts apparel based on various properties. It allows users to sort apparel by layer, tech-level, mod-name, and apparel tag. Additionally, it provides options to combine layer-categories with the same name, generate layer-categories for apparel that only occupy one layer, and generate subcategories based on armor-value, Psycic/Royalty-tagged apparel, Machanitors and other gear with special effects, gender, and CE-armor calculation. This mod works with all apparel, modded and vanilla, and is safe to add/remove from saves.

## Key Features and Systems

### Apparel Sorting
Clothing Sorter allows users to sort apparel by various properties, including:

- Layer
- Tech-level
- Mod-name
- Apparel tag

### Layer and Subcategory Management
Clothing Sorter provides options to:

- Combine layer-categories with the same name
- Generate layer-categories for apparel that only occupy one layer
- Generate subcategories based on armor-value, Psycic/Royalty-tagged apparel, Machanitors and other gear with special effects, gender, and CE-armor calculation

### Safe to Add/Remove from Saves
Clothing Sorter is safe to add/remove from saves, making it easy to manage.

## Coding Patterns and Conventions

Clothing Sorter follows standard C# and XML coding patterns and conventions. The main classes and methods are defined in the following C# files:

- `ClothingSorterMod.cs`
- `ClothingSorterSettings.cs`
- `ClothingSorter.cs`

The main XML file is:

- `Test-file.xml`

## XML Integration

Clothing Sorter uses XML to define the apparel layers and subcategories. The XML file contains the following definitions:

- `ApparelLayerDef`
- `ThingDef`

## Harmony Patching

Clothing Sorter uses Harmony to patch the game's code and add its functionality. Harmony is a popular modding framework for RimWorld.

## Suggestions for Copilot

### C# Code
When using Copilot to generate C# code for Clothing Sorter, keep the following in mind:

- The main classes and methods are defined in the `ClothingSorterMod.cs`, `ClothingSorterSettings.cs`, and `ClothingSorter.cs` files.
- The main XML file is `Test-file.xml`.
- Follow standard C# and XML coding patterns and conventions.

### XML Integration
When using Copilot to generate XML code for Clothing Sorter, keep the following in mind:

- The XML file contains `ApparelLayerDef` and `ThingDef` definitions.
- Use standard XML syntax and conventions.

### Harmony Patching
When using Copilot to generate Harmony patches for Clothing Sorter, keep the following in mind:

- Harmony is a popular modding framework for RimWorld.
- Use standard Harmony syntax and conventions.

## Conclusion

Clothing Sorter is a powerful mod that sorts apparel based on various properties. It is safe to add/remove from saves and provides options to manage layer and subcategory categories. When using Copilot to generate code for Clothing Sorter, keep the coding patterns and conventions, XML integration, and Harmony patching in mind.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

