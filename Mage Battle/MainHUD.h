// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "MainHUD.generated.h"

/**
 * 
 */
UCLASS()
class TECHDEMO_API UMainHUD : public UUserWidget
{
	GENERATED_BODY()

public:
	UPROPERTY(EditAnywhere, BlueprintReadWrite)
		float P1HPPercent;
	UPROPERTY(EditAnywhere, BlueprintReadWrite)
		float P2HPPercent;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
		float P1MPPercent;
	UPROPERTY(EditAnywhere, BlueprintReadWrite)
		float P2MPPercent;
};
