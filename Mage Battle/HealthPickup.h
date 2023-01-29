// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Components/SphereComponent.h"
#include "tpLocation.h"
#include "EngineUtils.h"
#include "Engine/World.h"
#include "HealthPickup.generated.h"

UCLASS()
class TECHDEMO_API AHealthPickup : public AActor
{
	GENERATED_BODY()

private:
	UPROPERTY()
		float timer;
	UPROPERTY(EditAnywhere)
		int limit = 5;
	UPROPERTY()
		bool pickedUp;
	UPROPERTY(EditAnywhere)
		USphereComponent* component;

public:
	UPROPERTY(EditAnywhere, BlueprintReadWrite)
		float sphereRadius = 50.0f;

	UPROPERTY()
		TArray<AActor*> spawnLocations;


public:
	// Sets default values for this actor's properties
	AHealthPickup();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	UFUNCTION()
		void respawn();

	UFUNCTION()
		void OnOverlapBegin(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);
};
