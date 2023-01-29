// Fill out your copyright notice in the Description page of Project Settings.


#include "Fireball.h"

// Sets default values
AFireball::AFireball()
{
	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;
	component = CreateDefaultSubobject<USphereComponent>(TEXT("SphereComponent"));
	component->InitSphereRadius(20);
	component->SetupAttachment(RootComponent);
	movement = FindComponentByClass<UProjectileMovementComponent>();
	RootComponent = component;

	//component->OnComponentHit.AddDynamic(this, &AFireball::OnComponentHit);
	//component->OnComponentBeginOverlap.AddDynamic(this, &AFireball::OnComponentBeginOverlap);
	component->SetCollisionProfileName(TEXT("Target"));
}

// Called when the game starts or when spawned
void AFireball::BeginPlay()
{

	Super::BeginPlay();
	manager = Cast<UGameManager>(UGameplayStatics::GetGameInstance(GetWorld()));
	manager->playSoundfire(GetActorLocation());
	SetLifeSpan(5);
}

// Called every frame
void AFireball::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

