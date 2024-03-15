// Copyright Epic Games, Inc. All Rights Reserved.

#include "HKU_Library_TestGameMode.h"
#include "HKU_Library_TestPlayerController.h"
#include "HKU_Library_TestCharacter.h"
#include "UObject/ConstructorHelpers.h"

AHKU_Library_TestGameMode::AHKU_Library_TestGameMode()
{
	// use our custom PlayerController class
	PlayerControllerClass = AHKU_Library_TestPlayerController::StaticClass();

	// set default pawn class to our Blueprinted character
	static ConstructorHelpers::FClassFinder<APawn> PlayerPawnBPClass(TEXT("/Game/TopDown/Blueprints/BP_TopDownCharacter"));
	if (PlayerPawnBPClass.Class != nullptr)
	{
		DefaultPawnClass = PlayerPawnBPClass.Class;
	}

	// set default controller to our Blueprinted controller
	static ConstructorHelpers::FClassFinder<APlayerController> PlayerControllerBPClass(TEXT("/Game/TopDown/Blueprints/BP_TopDownPlayerController"));
	if(PlayerControllerBPClass.Class != NULL)
	{
		PlayerControllerClass = PlayerControllerBPClass.Class;
	}
}