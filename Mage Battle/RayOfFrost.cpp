// Fill out your copyright notice in the Description page of Project Settings.


#include "RayOfFrost.h"

// Sets default values
ARayOfFrost::ARayOfFrost()
{
	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;
	component = CreateDefaultSubobject<USphereComponent>(TEXT("SphereComponent"));
	component->InitSphereRadius(30);
	component->SetupAttachment(RootComponent);
	RootComponent = component;


}

// Called when the game starts or when spawned
void ARayOfFrost::BeginPlay()
{
	Super::BeginPlay();
	manager = Cast<UGameManager>(UGameplayStatics::GetGameInstance(GetWorld()));
	manager->playSoundfrost(GetActorLocation());
}

// Called every frame
void ARayOfFrost::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
	timer += DeltaTime;

	//if (timer >= limit)
	//{
	//	manager->playSoundfrost(GetActorLocation());
	//}
}

