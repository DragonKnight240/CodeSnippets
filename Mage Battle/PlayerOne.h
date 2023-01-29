// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "GameManager.h"
#include "Fireball.h"
#include "RayOfFrost.h"
#include "Kismet/GameplayStatics.h"
#include "HealthPickup.h"
#include "Components/CapsuleComponent.h"
#include "Components/SphereComponent.h"
#include "Components/SceneComponent.h"
#include "Particles/ParticleSystemComponent.h"
#include "Components/ActorComponent.h"
#include "PlayerOne.generated.h"

UCLASS()
class TECHDEMO_API APlayerOne : public ACharacter
{
	GENERATED_BODY()

private:
	UPROPERTY()
		UGameManager* manager;
	UPROPERTY()
		bool healing;
	UPROPERTY()
		int repeatHealing = 0;

	UPROPERTY()
		bool onFire = false;
	UPROPERTY()
		float fireTimer;
	UPROPERTY(EditAnywhere, Category = "Abilities")
		float fireLimit = 3;
	UPROPERTY()
		float fireDamageTimer;
	UPROPERTY(EditAnywhere, Category = "Abilities")
		float fireDamageLimit = 1;
	UPROPERTY(EditAnywhere, Category = "Abilities")
		float burnDamage = 5;
	UPROPERTY()
		bool isFrozen = false;
	UPROPERTY()
		float frozenTimer;
	UPROPERTY(EditAnywhere, Category = "Abilities")
		float frozenLimit = 0;
	UPROPERTY()
		float moveMod = 1;
	UPROPERTY()
		bool isShielded = false;
	UPROPERTY()
		float shieldTimer = 0;
	UPROPERTY(EditAnywhere, Category = "Abilities")
		float shieldLimit = 8;
	UPROPERTY()
		float healingTimer = 0;
	UPROPERTY()
		float healingLimit = 5;

	UPROPERTY()
		float fireCooldown;
	UPROPERTY(EditAnywhere, Category = "Abilities")
		float fireCooldownMax = 1;
	UPROPERTY()
		float frostCooldown;
	UPROPERTY(EditAnywhere, Category = "Abilities")
		float frostCooldownMax = 0;
	UPROPERTY()
		float arcaneCooldown;
	UPROPERTY(EditAnywhere, Category = "Abilities")
		float arcaneCooldownMax = 40;
	UPROPERTY()
		float teleportCooldown;
	UPROPERTY(EditAnywhere, Category = "Abilities")
		float teleportCooldownMax = 4;
	UPROPERTY()
		bool arcaneOnCooldown = false;
	UPROPERTY()
		bool teleportOnCooldown = false;
	UPROPERTY()
		bool fireOnCooldown = false;
	UPROPERTY()
		bool frostAttacking;
	UPROPERTY()
		float timerFrost = 0;
	UPROPERTY()
		float limitFrost = 1;

	UPROPERTY(EditAnywhere, Category = "Spawner Category")
		TSubclassOf<ARayOfFrost> spawnFrost;
	UPROPERTY(EditAnywhere, Category = "Spawner Category")
		TSubclassOf<AFireball> spawnFireball;
	UPROPERTY()
		ARayOfFrost* newFrost;
	UPROPERTY()
		int frozenStack = 0;
	UPROPERTY(EditAnywhere, Category = "Abilities")
		float individualSlow = 0.025;

	UPROPERTY()
		UCapsuleComponent* capComponent;
	UPROPERTY(EditAnywhere)
		USphereComponent* fireZone;
	UPROPERTY(EditAnywhere)
		USphereComponent* teleportLocation;
	UPROPERTY()
		float secondFrozen = 0;
	UPROPERTY(EditAnywhere, Category = "Abilities")
		float limitFrozen = 2;
	UPROPERTY(EditAnywhere, Category = "Abilities")
		float frostDamage = 5;
	UPROPERTY()
		bool isOverlapedFrozen = false;
	UPROPERTY()
		float second = 0;
	UPROPERTY()
		bool canTeleport = true;
	UPROPERTY()
		UParticleSystemComponent* particles;

	UPROPERTY(EditAnywhere, Category = "Animation")
		UAnimationAsset* idle;
	UPROPERTY(EditAnywhere, Category = "Animation")
		UAnimationAsset* walk;
	UPROPERTY(EditAnywhere, Category = "Animation")
		UAnimationAsset* attack1;
	UPROPERTY(EditAnywhere, Category = "Animation")
		UAnimationAsset* attack2;	
	UPROPERTY(EditAnywhere, Category = "Animation")
		UAnimationAsset* shield;
	UPROPERTY(EditAnywhere, Category = "Animation")
		UAnimationAsset* teleport;
	UPROPERTY(EditAnywhere, Category = "Animation")
		UAnimationAsset* death;

	UPROPERTY(EditAnywhere)
		USkeletalMeshComponent* playerMesh;
	UPROPERTY()
		bool walkVerticalFirst = true;
	UPROPERTY()
		bool walkHorizontalFirst = true;
	UPROPERTY()
		bool attacking = false;
	UPROPERTY(EditAnywhere)
		USoundBase* cooldown;
	UPROPERTY()
		bool dead = false;
	UPROPERTY()
		UParticleSystemComponent* shieldPart;
	UPROPERTY(EditAnywhere)
		UParticleSystem* shieldTemp;
	UPROPERTY()
		UParticleSystemComponent* tpPart;
	UPROPERTY()
		APlayerOne* self;

public:
	// Sets default values for this character's properties
	APlayerOne();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	// Called to bind functionality to input
	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;

	UFUNCTION()
		void Horizontal(float amount);
	UFUNCTION()
		void Vertical(float amount);

	UFUNCTION()
		void heal();
	UFUNCTION()
		void Fireball();
	UFUNCTION()
		void Frost();
	UFUNCTION()
		void FrostEnd();
	UFUNCTION()
		void Shield();
	UFUNCTION()
		void Teleport();
	UFUNCTION()
		void OnComponentBeginOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);
	UFUNCTION()
		void OnMyComponentEndOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex);
};