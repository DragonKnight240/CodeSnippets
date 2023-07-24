using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitControlled : UnitBase
{
    UnitBase RecruitTarget;
    public bool Recruited = false;

    [Header("WeaponAssets")]
    public GameObject Sword;
    public GameObject GauntletsL;
    public GameObject GauntletR;
    public GameObject MagicBook;
    public GameObject Bow;

    [Header("Animations Weapon")]
    internal Animator MainAnim;

    public RuntimeAnimatorController SwordAnimControl;
    public RuntimeAnimatorController FistAnimControl;
    public RuntimeAnimatorController MagicAnimControl;
    public RuntimeAnimatorController BowAnimControl;

    override internal void Start()
    {
        base.Start();

        MainAnim = GetComponent<Animator>();
    }

    internal void MoveButton()
    {
        //Interact.Instance.CombatMenu.CombatMenuObject.SetActive(false);
        Interact.Instance.CombatMenu.CombatMenuObject.GetComponent<UIFade>().ToFadeOut();
    }

    internal void AttackButton(UnitBase Unit = null)
    {
        SoundManager.Instance.PlaySFX(Interact.Instance.CombatMenu.ButtonPress);
        FindInRangeTargets(false, false);

        if (InRangeTargets.Count > 0)
        {
            if (Unit)
            {
                if (InRangeTargets.Contains(Unit))
                {
                    CameraMove.Instance.FollowTarget = Unit.transform;
                    AttackTarget = Unit;
                }
                else
                {
                    return;
                }
            }
            else
            {
                CameraMove.Instance.FollowTarget = InRangeTargets[0].transform;
                AttackTarget = InRangeTargets[0];
            }
            //FindWeapons();
            AttackDisplay();
            Interact.Instance.CombatMenu.AttackMenuObject.SetActive(true);
            Interact.Instance.CombatMenu.AttackMenuObject.GetComponent<UIFade>().ToFadeIn();

            //Interact.Instance.CombatMenu.CombatMenuObject.SetActive(false);
            Interact.Instance.CombatMenu.CombatMenuObject.GetComponent<UIFade>().ToFadeOut();
        }
        else
        {
            //Interact.Instance.CombatMenu.CombatMenuObject.SetActive(false);

            Interact.Instance.CombatMenu.CombatMenuObject.GetComponent<UIFade>().ToFadeOut();
        }
    }

    internal void ItemButton()
    {
        SoundManager.Instance.PlaySFX(Interact.Instance.CombatMenu.ButtonPress);
        FindObjectOfType<Inventory>().ButtonText(Inventory);
        Interact.Instance.CombatMenu.InventoryObject.SetActive(true);
        Interact.Instance.CombatMenu.InventoryObject.GetComponent<UIFade>().ToFadeIn();

    }

    internal void SpecialButton()
    {
        SoundManager.Instance.PlaySFX(Interact.Instance.CombatMenu.ButtonPress);
        //Interact.Instance.CombatMenu.CombatMenuObject.SetActive(false);
        Interact.Instance.CombatMenu.CombatMenuObject.GetComponent<UIFade>().ToFadeOut();

        if (RecruitTarget)
        {
            SpecialZoomIn = true;

            AttackCamera.SetActive(true);
            Interact.Instance.VirtualCam.SetActive(false);
        }
        else
        {
            foreach (GameObject tile in TileManager.Instance.Grid[Position[0], Position[1]].GetComponent<Tile>().AdjacentTiles)
            {
                if (tile.GetComponent<Tile>().Special)
                {
                    GameManager.Instance.ToolTipCheck(Tutorial.CWeaponAbility);
                    CameraMove.Instance.FollowTarget = null;
                    tile.GetComponent<Tile>().Special.Special(this);
                    UnitManager.Instance.UnitUpdate.Invoke();
                    Interact.Instance.SelectedUnit = null;
                    Interact.Instance.UISelectedUnit();
                    HideAllChangedTiles();
                    WaitUnit();
                }
            }
        }

        //CameraMove.Instance.FollowTarget = null;
    }

    internal void FindWeapons()
    {
        foreach(Item item in Inventory)
        {
            if(item.Type == ItemTypes.Weapon)
            {
                WeaponsIninventory.Add((Weapon)item);
            }
        }
    }

    public bool CanRecruit()
    {
        if (Class.Name == "Archer")
        {
            if(GameManager.Instance.NumRecruited > GameManager.Instance.MaxRecruitable)
            {
                RecruitTarget = null;
                return false;
            }

            foreach (GameObject TileObj in TileManager.Instance.Grid[Position[0], Position[1]].GetComponent<Tile>().AdjacentTiles)
            {
                if (TileObj.GetComponent<Tile>().Unit)
                {
                    if (TileObj.GetComponent<Tile>().Unit.CompareTag("Enemy"))
                    {
                        if(TileObj.GetComponent<Tile>().Unit.GetComponent<BossAI>())
                        {
                            continue;
                        }

                        if (TileObj.GetComponent<Tile>().Unit.CurrentHealth / TileObj.GetComponent<Tile>().Unit.HealthMax < 0.1f + (RankBonus[Class.Level] / TileObj.GetComponent<Tile>().Unit.HealthMax))
                        {
                            RecruitTarget = TileObj.GetComponent<Tile>().Unit;
                            return true;
                        }
                    }
                }
            }
        }

        RecruitTarget = null;
        return false;
    }

    internal void SpecialRecruit()
    {
        GameManager.Instance.NumRecruited++;
        //print(Mathf.FloorToInt((TotalDexterity() + TotalLuck()) / 2 + RankBonus[BowLevel]));
        RecruitTarget.GetComponent<UnitAI>().AttemptRecruit((TotalDexterity() + TotalLuck()) / 2 + RankBonus[BowLevel]);
    }

    internal void AttackDisplay()
    {
        isSupported(AttackTarget);

        if (Interact.Instance.SelectedUnit.WeaponsIninventory.Count == 0)
        {
            Interact.Instance.CombatMenu.NextWeapon.SetActive(false);
            Interact.Instance.CombatMenu.PreviousWeapon.SetActive(false);
        }
        else
        {
            Interact.Instance.CombatMenu.NextWeapon.SetActive(true);
            Interact.Instance.CombatMenu.PreviousWeapon.SetActive(true);
        }

        if(Interact.Instance.SelectedUnit.AvailableAttacks.Count <= 1 )
        {
            Interact.Instance.CombatMenu.NextAttack.SetActive(false);
            Interact.Instance.CombatMenu.PreviousAttack.SetActive(false);
        }
        else
        {
            Interact.Instance.CombatMenu.NextAttack.SetActive(true);
            Interact.Instance.CombatMenu.PreviousAttack.SetActive(true);
        }

        if(Interact.Instance.SelectedUnit.InRangeTargets.Count <= 1)
        {
            Interact.Instance.CombatMenu.NextTarget.SetActive(false);
            Interact.Instance.CombatMenu.PreviousTarget.SetActive(false);
        }
        else
        {
            Interact.Instance.CombatMenu.NextTarget.SetActive(true);
            Interact.Instance.CombatMenu.PreviousTarget.SetActive(true);
        }

        Interact.Instance.CombatMenu.HealthAlly.value = (float)(CurrentHealth - (AttackTarget.CanReturnAttackIncludeMovement(this)?AttackTarget.CalculateDamage(this): 0) ) / HealthMax;
        Interact.Instance.CombatMenu.Weapon.text = EquipedWeapon.Name + (EquipedWeapon != BareHands || !EquipedWeapon.isDestroyed ? (" (" + EquipedWeapon.CurrentDurablity + " / " + EquipedWeapon.Durablity + ")"): "");
        Interact.Instance.CombatMenu.Attack.text = CurrentAttack.Name + (EquipedWeapon != BareHands || !EquipedWeapon.isDestroyed ? " (" +  CurrentAttack.DurabilityMultiplier + ")": "");
        Interact.Instance.CombatMenu.DamageAlly.text = CalculateDamage().ToString();
        Interact.Instance.CombatMenu.HitAlly.text = (CalcuateHitChance() - AttackTarget.CalculateDodge(this)).ToString();
        Interact.Instance.CombatMenu.CritAlly.text = CalculateCritChance().ToString();

        Interact.Instance.CombatMenu.HealthEnemy.value = (float)(AttackTarget.CurrentHealth - CalculateDamage()) / AttackTarget.HealthMax;
        Interact.Instance.CombatMenu.DamageEnemy.text = (AttackTarget.CanReturnAttackIncludeMovement(this) ? AttackTarget.CalculateDamage(this): 0).ToString();
        Interact.Instance.CombatMenu.HitEnemy.text = (AttackTarget.CanReturnAttackIncludeMovement(this) ? (AttackTarget.CalcuateHitChance() - CalculateDodge(AttackTarget)): 0).ToString();
        Interact.Instance.CombatMenu.CritEnemy.text = (AttackTarget.CanReturnAttackIncludeMovement(this) ? AttackTarget.CalculateCritChance() : 0).ToString();

        Interact.Instance.CombatMenu.DisplaySupport();

        Interact.Instance.SelectedUnit.ShowAllInRangeTiles();
    }
}
