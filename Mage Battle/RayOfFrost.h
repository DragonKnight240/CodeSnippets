// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "GameManager.h"
#include "Components/SphereComponent.h"
#include "RayOfFrost.generated.h"

UCLASS()
class TECHDEMO_API ARayOfFrost : public AActor
{
	GENERATED_BODY()

		UPROPERTY()
		UGameManager* manager;
	UPROPERTY(EditAnywhere, Category = "Stats")
		int power = 30;
	UPROPERTY(EditAnywhere, Category = "Stats")
		int manaValue = 15;
	UPROPERTY(EditAnywhere, Category = "Stats")
		float speedIncrease = 10;
	UPROPERTY(EditAnywhere)
		USphereComponent* component;
	UPROPERTY()
		float limit = 5;
	UPROPERTY()
		float timer = 0;

public:
	// Sets default values for this actor's properties
	ARayOfFrost();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;

};
