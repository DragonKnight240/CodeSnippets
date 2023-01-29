// Fill out your copyright notice in the Description page of Project Settings.


#include "PlayerOne.h"
#include "Fireball.h"

// Sets default values
APlayerOne::APlayerOne()
{
	// Set this character to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;
	bUseControllerRotationYaw = false;
	capComponent = FindComponentByClass<UCapsuleComponent>();
	capComponent->OnComponentBeginOverlap.AddDynamic(this, &APlayerOne::OnComponentBeginOverlap);
	capComponent->OnComponentEndOverlap.AddDynamic(this, &APlayerOne::OnMyComponentEndOverlap);

	fireZone = CreateDefaultSubobject<USphereComponent>(TEXT("FireLocation"));
	fireZone->SetupAttachment(RootComponent);
	fireZone->SetRelativeLocation(FVector(60, 0, 30));
	fireZone->bHiddenInGame = true;
	fireZone->bCastHiddenShadow = true;

	teleportLocation = CreateDefaultSubobject<USphereComponent>(TEXT("TPLocation"));
	teleportLocation->SetupAttachment(RootComponent);
	teleportLocation->SetRelativeLocation(FVector(300, 0, 0));
	teleportLocation->bHiddenInGame = true;
	teleportLocation->bCastHiddenShadow = true;

	playerMesh = FindComponentByClass<USkeletalMeshComponent>();

	//shieldPart = CreateDefaultSubobject<UParticleSystemComponent>(TEXT("Shield"));
	//shieldPart->SetRelativeScale3D(FVector(0.8, 0.8, 1.25));
	//shieldPart->SetRelativeLocation(FVector(0, 0, -70));
}

// Called when the game starts or when spawned
void APlayerOne::BeginPlay()
{
	Super::BeginPlay();

	manager = Cast<UGameManager>(UGameplayStatics::GetGameInstance(GetWorld()));
	manager->LoadHUD();
}

// Called every frame
void APlayerOne::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	manager->lerpHealthP1(DeltaTime);
	manager->lerpManaP1(DeltaTime);

	if (!dead)
	{
		second += DeltaTime;

		if (fireOnCooldown)
		{
			fireCooldown += DeltaTime;

			if (fireCooldown >= fireCooldownMax)
			{
				fireOnCooldown = false;
			}
		}

		if (frostAttacking)
		{
			timerFrost += DeltaTime;

			if (timerFrost >= limitFrost)
			{
				Frost();
				timerFrost = 0;
			}
		}

		if (arcaneOnCooldown)
		{
			arcaneCooldown += DeltaTime;

			if (arcaneCooldown >= arcaneCooldownMax)
			{
				arcaneOnCooldown = false;
			}
		}

		if (teleportOnCooldown)
		{
			teleportCooldown += DeltaTime;

			if (teleportCooldown >= teleportCooldownMax)
			{
				teleportOnCooldown = false;
			}
		}

		if (((walkHorizontalFirst && walkVerticalFirst && !attacking) || !playerMesh->IsPlaying()) && !dead)
		{
			playerMesh->PlayAnimation(idle, true);
			attacking = false;
		}

		if (healing)
		{
			healingTimer += DeltaTime;

			if (healingTimer >= healingLimit)
			{
				heal();
				repeatHealing += 1;
				healingTimer = 0;

				if (repeatHealing >= 5)
				{
					repeatHealing = 0;
					healing = false;
				}
			}
		}

		if (onFire)
		{
			fireTimer += DeltaTime;
			fireDamageTimer += DeltaTime;

			if (fireDamageTimer >= fireDamageLimit)
			{
				manager->decreaseHealth(true, burnDamage, GetActorLocation(), GetActorRotation());
				fireDamageTimer = 0;
			}

			if (fireTimer >= fireLimit)
			{
				fireTimer = 0;
				fireDamageTimer = 0;
				onFire = false;
			}
		}

		if (isFrozen)
		{
			if (!isOverlapedFrozen)
			{
				frozenTimer += DeltaTime;
				secondFrozen = 0;
			}
			else
			{
				secondFrozen += DeltaTime;
			}

			if (secondFrozen >= limitFrozen)
			{
				secondFrozen = 0;
				frozenTimer = 0;
				manager->decreaseHealth(true, frostDamage, GetActorLocation(), GetActorRotation());
				if (frozenStack != 12)
				{
					frozenStack++;
					moveMod = individualSlow * frozenStack;
				}
			}

			if (frozenTimer >= frozenLimit)
			{
				moveMod = 1;
				isFrozen = false;
				frozenTimer = 0;
				frozenStack = 0;
			}
		}

		if (isShielded)
		{
			shieldTimer += DeltaTime;

			if (shieldTimer >= shieldLimit)
			{
				isShielded = false;
				manager->setDamageMod(1, true);
				shieldTimer = 0;
			}
		}

		if (manager->GetPlayerOneHealth() <= 0 && !dead)
		{
			dead = true;
			playerMesh->PlayAnimation(death, false);
		}

		if (second >= 1)
		{
			manager->increaseMana(true);
			second = 0;
		}
	}
}

// Called to bind functionality to input
void APlayerOne::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);
	PlayerInputComponent->BindAxis("Horizontal", this, &APlayerOne::Horizontal);
	PlayerInputComponent->BindAxis("Vertical", this, &APlayerOne::Vertical);
	PlayerInputComponent->BindAction("Fireball", IE_Released, this, &APlayerOne::Fireball);
	PlayerInputComponent->BindAction("Frost", IE_Released, this, &APlayerOne::FrostEnd);
	PlayerInputComponent->BindAction("Frost", IE_Pressed, this, &APlayerOne::Frost);
	PlayerInputComponent->BindAction("Shield", IE_Released, this, &APlayerOne::Shield);
	PlayerInputComponent->BindAction("Teleport", IE_Released, this, &APlayerOne::Teleport);
}

void APlayerOne::Horizontal(float amount)
{
	if (!dead && !attacking)
	{
		if (amount != 0)
		{
			FrostEnd();
			if (walkHorizontalFirst && walkVerticalFirst)
			{
				playerMesh->PlayAnimation(walk, true);
			}
			walkHorizontalFirst = false;
		}
		else
		{
			walkHorizontalFirst = true;
		}
		FVector Direction = FRotationMatrix(Controller->GetControlRotation()).GetScaledAxis(EAxis::X);
		AddMovementInput(Direction, -amount * (1 - frozenStack * individualSlow));
	}
}

void APlayerOne::Vertical(float amount)
{
	if (!dead && !attacking)
	{
		if (amount != 0)
		{
			FrostEnd();
			if (walkVerticalFirst && walkHorizontalFirst)
			{
				playerMesh->PlayAnimation(walk, true);
			}
			walkVerticalFirst = false;
		}
		else
		{
			walkVerticalFirst = true;
		}
		FVector Direction = FRotationMatrix(Controller->GetControlRotation()).GetScaledAxis(EAxis::Y);
		AddMovementInput(Direction, amount * (1 - frozenStack * individualSlow));
	}

}

void APlayerOne::heal()
{
	if (!dead)
	{
		manager->addHealth(true, 5);
	}
}

void APlayerOne::Fireball()
{
	if (!dead)
	{
		if (manager->castFireball(true, fireOnCooldown))
		{
			FActorSpawnParameters spawnParams;
			spawnParams.Owner = this;
			spawnParams.Instigator = GetInstigator();

			if (fireZone != nullptr)
			{
				fireOnCooldown = true;
				attacking = true;
				playerMesh->PlayAnimation(attack1, false);
				AFireball* fire = GetWorld()->SpawnActor<AFireball>(spawnFireball, fireZone->GetComponentLocation(), GetActorRotation(), spawnParams);
			}
		}
		else
		{
			UGameplayStatics::PlaySoundAtLocation(this, cooldown, GetActorLocation());
		}
	}
}

void APlayerOne::Frost()
{
	if(!dead)
	{
		if (manager->castFrost(true))
		{
			FActorSpawnParameters spawnParams;
			spawnParams.Owner = this;
			spawnParams.Instigator = GetInstigator();

			if (newFrost == nullptr)
			{
				attacking = true;
				frostAttacking = true;
				playerMesh->PlayAnimation(attack2, false);
				newFrost = GetWorld()->SpawnActor<ARayOfFrost>(spawnFrost, fireZone->GetComponentLocation(), GetActorRotation(), spawnParams);
			}


		}
		else
		{
			UGameplayStatics::PlaySoundAtLocation(this, cooldown, GetActorLocation());
			FrostEnd();
		}
	}
}

void APlayerOne::FrostEnd()
{
	if (newFrost != nullptr)
	{
		newFrost->Destroy();
		newFrost = nullptr;
		frostAttacking = false;

	}
}

void APlayerOne::Shield()
{
	if (!dead)
	{
		if (manager->castShield(true, arcaneOnCooldown))
		{
			arcaneOnCooldown = true;
			attacking = true;
			manager->playSoundshield(GetActorLocation());
			playerMesh->PlayAnimation(shield, false);
			manager->setDamageMod(0.5, true);
			isShielded = true;
		}
		else
		{
			UGameplayStatics::PlaySoundAtLocation(this, cooldown, GetActorLocation());
		}
	}
}

void APlayerOne::Teleport()
{
	if (!dead)
	{
		if (teleportLocation->GetComponentLocation().X > -5740.0 && teleportLocation->GetComponentLocation().X < -3870.0)
		{
			if (teleportLocation->GetComponentLocation().Y < 16449 && teleportLocation->GetComponentLocation().Y > 14962)
			{
				if (manager->castTeleport(true, teleportOnCooldown))
				{
					teleportOnCooldown = true;
					attacking = true;
					playerMesh->PlayAnimation(teleport, false);
					manager->playSoundteleport(GetActorLocation());
					SetActorLocation(teleportLocation->GetComponentLocation());
				}
				else
				{
					UGameplayStatics::PlaySoundAtLocation(this, cooldown, GetActorLocation());
				}
			}
		}
	}
}

void APlayerOne::OnComponentBeginOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult)
{
	if (!dead && OtherActor->GetOwner() != this)
	{
		if (OtherActor->ActorHasTag("RayOfFrost"))
		{
			isFrozen = true;
			isOverlapedFrozen = true;
			manager->decreaseHealth(true, 5, GetActorLocation(), GetActorRotation());
		}
		else if (OtherActor->ActorHasTag("PickUp"))
		{
			healing = true;
			heal();
		}
		else if (OtherActor->ActorHasTag("Fireball"))
		{
			onFire = true;
			manager->decreaseHealth(true, 30, GetActorLocation(), GetActorRotation());

			OtherActor->Destroy();
		}
	}
}

void APlayerOne::OnMyComponentEndOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex)
{
	if(!dead)
	{
		if (OtherActor->ActorHasTag("RayOfFrost"))
		{
			isOverlapedFrozen = false;
		}
	}
}
