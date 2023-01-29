// Fill out your copyright notice in the Description page of Project Settings.


#include "HealthPickup.h"

// Sets default values
AHealthPickup::AHealthPickup()
{
	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

	component = CreateDefaultSubobject<USphereComponent>(TEXT("SphereComponent"));
	component->InitSphereRadius(sphereRadius);
	component->SetupAttachment(RootComponent);

	component->SetCollisionProfileName(TEXT("Target"));
	component->OnComponentBeginOverlap.AddDynamic(this, &AHealthPickup::OnOverlapBegin);
}

// Called when the game starts or when spawned
void AHealthPickup::BeginPlay()
{
	Super::BeginPlay();

	for (TActorIterator<AtpLocation> ActorItr(GetWorld()); ActorItr; ++ActorItr)
	{
		spawnLocations.Add(*ActorItr);
	}
}

// Called every frame
void AHealthPickup::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	if (pickedUp)
	{
		timer += DeltaTime;

		if (timer >= limit)
		{
			respawn();
			timer = 0;
			pickedUp = false;
		}
	}
}

void AHealthPickup::respawn()
{
	SetActorLocation(spawnLocations[FMath::RandRange(1,spawnLocations.Num()) - 1]->GetActorLocation());
}

void AHealthPickup::OnOverlapBegin(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult)
{
	if (OtherActor->ActorHasTag("PlayerOne"))
	{
		SetActorLocation(FVector(0, 0, 0));
		pickedUp = true;
	}
	else if (OtherActor->ActorHasTag("PlayerTwo"))
	{
		SetActorLocation(FVector(0, 0, 0));
		pickedUp = true;
	}
}

