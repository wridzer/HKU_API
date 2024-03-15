// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;

public class HKU_Library_Test : ModuleRules
{
	public HKU_Library_Test(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

        PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore", "NavigationSystem", "AIModule", "Niagara", "EnhancedInput" });
    }
}
