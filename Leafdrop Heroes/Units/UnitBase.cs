using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public enum SupportIncrease
{
    HitRate,
    CritRate,
    Damage,
    Dodge,
    DefRes,
}

public class UnitBase : MonoBehaviour
{
    public struct StatusEffect
    {
        public Effects Effects;
        public int Length;
        public int Amount;
    }

    class Node
    {
        public Node PreviousTile;
        public Tile Tile;
        public int FCost;
        public int HCost;
        public int GCost;
    }

    public Slider UIHealth;
    public float HealthLerpSpeed = 10;

    [Header("General")]
    public string UnitName;
    public Sprite UnitImage;
    public int HealthMax = 50;
    public int CurrentHealth;
    internal int[] Position;
    public int Movement = 3;
    public float MoveSpeed = 10;
    internal bool EXPPending = false;
    internal bool Setup = false;

    public List<Tile> MoveableTiles;
    internal Vector3 ToCenter = new Vector3(0, 0, 0);
    public List<Tile> AttackTiles;

    internal bool isAlive = true;
    internal bool Moving = false;
    public List<Tile> Path;

    internal CombatAnimControl AnimControl;

    [Header("Inventory")]
    public Weapon EquipedWeapon;
    public List<Item> Inventory;
    public List<Weapon> WeaponsIninventory;
    internal UnitBase AttackTarget;
    public Weapon BareHands;

    //Turn Checks
    [Header("Turn Checks")]
    internal bool MovedForTurn = false;
    internal bool AttackedForTurn = false;
    internal bool EndTurn = false;

    [Header("Stats")]
    public int Strength = 2;
    public int Dexterity;
    public int Magic;
    public int Defence;
    public int Resistance;
    public int Speed;
    public int Luck;
    public int Level;
    public int EXP;
    public List<int> EXPNeeded;

    public int GrowthRateHP;
    public int GrowthRateStrength;
    public int GrowthRateDexterity;
    public int GrowthRateMagic;
    public int GrowthRateDefence;
    public int GrowthRateResistance;
    public int GrowthRateSpeed;
    public int GrowthRateLuck;

    [Header("Weapon Proficiencies")]
    public float BowProficiency;
    public int BowLevel;

    public float SwordProficiency;
    public int SwordLevel;

    public float MagicProficiency;
    public int MagicLevel;

    public float FistProficiency;
    public int FistLevel;

    public List<int> RankBonus;
    public List<int> RankEXP;

    [Header("Class")]
    public Class Class;

    [Header("Attack")]
    public SpecialAttacks CurrentAttack;
    public List<SpecialAttacks> UnlockedAttacks;
    public List<SpecialAttacks> AvailableAttacks;
    internal List<GameObject> OuterMostMove;
    public GameObject AttackCamera;
    internal bool ToAttack = false;
    bool AttackZoomIn = false;
    bool HitZoomIn = false;
    bool DeathZoomIn = false;
    bool ReturnAttack = false;
    bool NoDamageZoomIn = false;
    public float NoDamageTime = 3.0f;
    float NoDamageTimer = 0.0f;
    public DamageNumbers DamageNumbersController;
    int DamageToTake;
    internal List<UnitBase> SupportedUnits;
    internal bool CanCrit = true;
    internal bool ReturnAttackPossible = true;
    internal bool SpecialZoomIn;
    internal Tile AttackTile;
    internal bool PendingWeaponDistruction;

    public List<UnitBase> InRangeTargets;

    [Header("Supports")]
    public List<UnitSupports> SupportsWith;

    [Header("Status Effects")]
    internal List<StatusEffect> CurrentStatusEffects;

    [Header("UI Stuff")]
    public Image WeaponImage;

    //Sounds
    [Header("Sound Effects")]
    public AudioClip AttackSound;
    public AudioClip HitSound;
    public AudioClip DeathSound;
    public AudioClip WeaponHitSound;

    [Header("VFX")]
    public GameObject HealingParticle;
    public GameObject CritParticle;
    internal GameObject CurrentParticle;
    public GameObject HitParticle;
    public GameObject BossHitParticle;
    public GameObject TauntParticle;
    public GameObject PartSwordLoc;
    public GameObject PartMagicLoc;
    //public GameObject PartBowLoc;
    //public GameObject PartGauntletLocL;
    //public GameObject PartGauntletLocR;

    public Sprite EyesSprite;

    [Header("Drops")]
    public int MoneyDrop;
    public Item ItemDrop;

    // Start is called before the first frame update
    virtual internal void Start()
    {
        MoveableTiles = new List<Tile>();
        AttackTiles = new List<Tile>();
        InRangeTargets = new List<UnitBase>();
        OuterMostMove = new List<GameObject>();
        //WeaponsIninventory = new List<Weapon>();
        Path = new List<Tile>();

        MoveableArea(false);

        WeaponsIninventory.Add(BareHands);

        AnimControl = GetComponent<CombatAnimControl>();
        SupportedUnits = new List<UnitBase>();

    }

    // Update is called once per frame
    virtual public void Update()
    {
        if (isAlive)
        {
            if (AttackCamera.transform.position == Interact.Instance.transform.position)
            {
                if (HitZoomIn)
                {
                    HideAllChangedTiles();
                    PlayHitAnim();
                }
                else if (DeathZoomIn)
                {
                    HideAllChangedTiles();
                    PlayDeathAnim();
                }
                else if (AttackZoomIn)
                {
                    if (CompareTag("Ally"))
                    {
                        if (EquipedWeapon)
                        {
                            if (!EquipedWeapon.isDestroyed)
                            {
                                EquipedWeapon.CurrentDurablity += CurrentAttack.DurabilityMultiplier;

                                if (EquipedWeapon.CurrentDurablity <= 0)
                                {
                                    PendingWeaponDistruction = true;
                                }
                            }
                        }
                    }

                    HideAllChangedTiles();
                    PlayAttackAnim();
                }
                else if (NoDamageZoomIn)
                {
                    HideAllChangedTiles();
                    NoDamage();
                }
                else if (SpecialZoomIn)
                {
                    HideAllChangedTiles();
                    PlaySpecialAnim();
                }
            }
            else if (Interact.Instance.VirtualCam.activeInHierarchy && PendingWeaponDistruction)
            {
                if (!Interact.Instance.CombatMenu.ShowEXP && !Interact.Instance.CombatMenu.ClassShowEXP
                   && !EXPPending && !Interact.Instance.CombatMenu.LevelScreen.activeInHierarchy
                   && !Interact.Instance.CombatMenu.AttackScreen.activeInHierarchy
                   && EquipedWeapon.DestoryedWeapon != null)
                {
                    Weapon weapon = Instantiate(EquipedWeapon.DestoryedWeapon);

                    WeaponsIninventory.Remove(EquipedWeapon);
                    Inventory.Remove(EquipedWeapon);

                    EquipedWeapon = weapon;

                    Inventory.Add(weapon);
                    WeaponsIninventory.Add(weapon);

                    Interact.Instance.CombatMenu.ItemText.text = EquipedWeapon.Name + " (" + EquipedWeapon.CurrentDurablity + " / " + EquipedWeapon.Durablity + ")";
                    Interact.Instance.CombatMenu.ItemNotification.SetActive(true);

                    PendingWeaponDistruction = false;
                }
            }

            if (CurrentHealth > 0)
            {
                if (Moving && Interact.Instance.VirtualCam.transform.position == Interact.Instance.transform.position)
                {
                    if (Path.Count <= 0)
                    {
                        Moving = false;
                        Interact.Instance.UnitMoving = false;
                        return;
                    }

                    if (new Vector3(Path[0].CentrePoint.transform.position.x + ToCenter[0], transform.position.y, Path[0].CentrePoint.transform.position.z + ToCenter[1]) ==
                        new Vector3(transform.position.x, transform.position.y, transform.position.z))
                    {
                        Path.RemoveAt(0);
                        if (Path.Count <= 0)
                        {
                            Moving = false;
                            Interact.Instance.UnitMoving = false;

                            if (ToAttack)
                            {
                                ToAttack = false;
                                AttackingZoom();
                            }
                            return;
                        }
                    }

                    transform.LookAt(new Vector3(Path[0].CentrePoint.transform.position.x + ToCenter[0], transform.position.y, Path[0].CentrePoint.transform.position.z + ToCenter[1]));
                    transform.position = Vector3.MoveTowards(transform.position, new Vector3(Path[0].transform.position.x + ToCenter[0], transform.position.y, Path[0].transform.position.z + ToCenter[1]), MoveSpeed * Time.deltaTime);
                }
                else if (!Moving)
                {
                    if (ToAttack)
                    {
                        ToAttack = false;
                        AttackingZoom();
                    }
                }
            }
            else
            {
                if (AnimControl.CurrentAnimation != CombatAnimControl.AnimParameters.Death && Interact.Instance.VirtualCam.activeInHierarchy)
                {
                    if (UnitManager.Instance.PendingDeath.Count > 0)
                    {
                        if (UnitManager.Instance.PendingDeath[0] == this)
                        {
                            DeathZoomIn = true;
                            AttackCamera.SetActive(true);
                            Interact.Instance.VirtualCam.SetActive(false);
                            UnitManager.Instance.PendingDeath.RemoveAt(0);
                        }
                    }
                }
            }

            if (EXPPending && Interact.Instance.VirtualCam.activeInHierarchy)
            {
                UnitManager.Instance.PendingEXP = false;
                EXPPending = false;
                Interact.Instance.CombatMenu.EXPSliderShow(this, AttackTarget.DamageToTake);
            }
        }

        if (UIHealth)
        {
            if (UIHealth.value != CurrentHealth)
            {
                UIHealth.value = Mathf.Lerp(UIHealth.value, CurrentHealth, Time.deltaTime * HealthLerpSpeed);
            }
        }
    }

    //Moves the character from the current location to the wanted location
    internal bool Move(Tile NewTile, bool Attacking = false, bool Ignore = false)
    {

        if (MovedForTurn)
        {
            return false;
        }

        if ((MoveableTiles.Contains(NewTile) && NewTile.Special == null && NewTile.Unit == null) || Attacking || Ignore)
        {
            GameManager.Instance.ToolTipCheck(Tutorial.CMove);

            CalculatePath(NewTile);

            MovedForTurn = true;
            Moving = true;
            Interact.Instance.UnitMoving = true;

            if (Path.Count > 0)
            {
                Position[0] = Path[Path.Count - 1].GridPosition[0];
                Position[1] = Path[Path.Count - 1].GridPosition[1];
            }

            if (!Attacking)
            {
                NewTile.ChangeOccupant(this, GetComponent<BossAI>() ? GetComponent<BossAI>().isMultiTile : false);
            }
            else
            {
                if (Path.Count > 0)
                {
                    Path[Path.Count - 1].ChangeOccupant(this, GetComponent<BossAI>() ? GetComponent<BossAI>().isMultiTile : false);
                }
            }

            ResetMoveableTiles();
            UnitManager.Instance.UnitUpdate.Invoke();

            return true;
        }

        return false;
    }

    public void PlayParticleEffect()
    {
        switch (EquipedWeapon.WeaponType)
        {
            case WeaponType.Sword:
                {
                    CurrentParticle = Instantiate(CurrentAttack.Particles, PartSwordLoc.transform);
                    break;
                }
            //case WeaponType.Bow:
            //    {
            //        CurrentParticle = Instantiate(CurrentAttack.Particles, PartBowLoc.transform);
            //        break;
            //    }
            //case WeaponType.Gauntlets:
            //    {
            //        CurrentParticle = Instantiate(CurrentAttack.Particles, PartGauntletLocL.transform);
            //        Instantiate(CurrentAttack.Particles, PartGauntletLocR.transform);
            //        break;
            //    }
            case WeaponType.Staff:
                {
                    CurrentParticle = Instantiate(CurrentAttack.Particles, PartMagicLoc.transform);
                    break;
                }
            default:
                {
                    //CurrentParticle = Instantiate(CurrentAttack.Particles, PartGauntletLocL.transform);
                    //Instantiate(CurrentAttack.Particles, PartGauntletLocR.transform);
                    break;
                }
        }

    }

    public void TurnOffTauntParticle()
    {
        TauntParticle.SetActive(false);
    }

    public void PlayTauntParticle()
    {
        TauntParticle.SetActive(true);
    }

    public void PlayHitParticle()
    {
        HitParticle.SetActive(true);
    }

    public void PlayBossHitParticle()
    {
        BossHitParticle.SetActive(true);
    }

    public void SetParticleSpeed(float Speed)
    {
        if(CurrentParticle.GetComponent<csParticleMove>())
        {
            CurrentParticle.GetComponent<csParticleMove>().speed = Speed;
        }
    }

    //Find Path to the target sqaure
    void CalculatePath(Tile NewTile, bool ChangeOccupant = true)
    {
        if (ChangeOccupant)
        {
            TileManager.Instance.Grid[Position[0], Position[1]].GetComponent<Tile>().ChangeOccupant(null, GetComponent<BossAI>() ? GetComponent<BossAI>().isMultiTile : false);
        }
        Path = new List<Tile>(FindRouteTo(NewTile, false));
    }

    public bool AttackableArea(List<GameObject> CheckingTiles, bool ShowTiles = true)
    {
        CheckingTiles.Add(TileManager.Instance.Grid[Position[0], Position[1]]);

        if (EquipedWeapon)
        {
            for (int i = 0; i < EquipedWeapon.Range; i++)
            {
                CheckingTiles = WeaponRangeAttack(CheckingTiles, ShowTiles);
            }
        }
        else
        {
            CheckingTiles = WeaponRangeAttack(CheckingTiles, ShowTiles);
        }

        //Checks if an enemy unit is in the attack range
        foreach (Tile tile in AttackTiles)
        {
            if (tile.Unit)
            {
                //Makes sure it's not a unit on the same team
                if (!tile.Unit.CompareTag(tag) && tile.Unit != this.gameObject)
                {
                    if (ToolTipManager.Instance && CompareTag("Ally"))
                    {
                        if (ToolTipManager.Instance.CompletedTurn1)
                        {
                            ToolTip Tip = ToolTipManager.Instance.FindToolTip(Tutorial.CAttack);
                            if (!ToolTipManager.Instance.Seen[Tip])
                            {
                                //print("CAttack Tooltip");
                                GameManager.Instance.NextToolTip(Tip);
                            }
                        }
                    }

                    return true;
                }
            }
        }

        return false;

    }

    //Finds what tiles can be moved to from the current place
    public void MoveableArea(bool ShowTiles = true)
    {
        HideAllChangedTiles();
        MoveableTiles.Clear();
        AttackTiles.Clear();
        OuterMostMove = new List<GameObject>();
        OuterMostMove.Clear();

        List<GameObject> CheckingTiles = new List<GameObject>();
        CheckingTiles.Add(TileManager.Instance.Grid[Position[0], Position[1]]);

        for (int i = 0; i < Movement; i++)
        {
            CheckingTiles = CheckTiles(CheckingTiles, false, ShowTiles);
        }

        foreach (GameObject tile in CheckingTiles)
        {
            if (!OuterMostMove.Contains(tile))
            {
                OuterMostMove.Add(tile);
            }
        }

        if (OuterMostMove.Count > 0)
        {
            AttackableArea(OuterMostMove, ShowTiles);
        }
        else
        {
            AttackableArea(CheckingTiles, ShowTiles);
        }

        if (!ShowTiles)
        {
            HideAllChangedTiles();
        }
        else
        {
            ShowAllInRangeTiles();
        }
    }

    //Adds the adjacent tiles to moveable/attack tiles list and shows the tiles in the correct colour
    internal List<GameObject> CheckTiles(List<GameObject> tiles, bool WeaponRange = false, bool ShowTile = true)
    {
        List<GameObject> NextLayer = new List<GameObject>();
        bool MoveEnd = false;

        foreach (GameObject tile in tiles)
        {
            MoveEnd = false;
            foreach (GameObject AdjacentTile in tile.GetComponent<Tile>().AdjacentTiles)
            {
                if (!MoveableTiles.Contains(AdjacentTile.GetComponent<Tile>()))
                {

                    if (AdjacentTile.GetComponent<Tile>().CanMoveOn || AdjacentTile.GetComponent<Tile>().Unit || AdjacentTile.GetComponent<Tile>().Special)
                    {

                        if (GetComponent<BossAI>())
                        {
                            if (GetComponent<BossAI>().isMultiTile)
                            {
                                int XGrid = AdjacentTile.GetComponent<Tile>().GridPosition[0];
                                int YGrid = AdjacentTile.GetComponent<Tile>().GridPosition[1];

                                //Right
                                if (XGrid + 1 < TileManager.Instance.Width)
                                {
                                    if (TileManager.Instance.Grid[XGrid + 1, YGrid].GetComponent<Tile>().Unit)
                                    {
                                        if (TileManager.Instance.Grid[XGrid + 1, YGrid].GetComponent<Tile>().Unit != this)
                                        {
                                            continue;
                                        }
                                    }

                                    if (TileManager.Instance.Grid[XGrid + 1, YGrid].GetComponent<Tile>().Special)
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }

                                //UP
                                if (YGrid + 1 < TileManager.Instance.Height)
                                {
                                    if (TileManager.Instance.Grid[XGrid, YGrid + 1].GetComponent<Tile>().Unit)
                                    {
                                        if (TileManager.Instance.Grid[XGrid, YGrid + 1].GetComponent<Tile>().Unit != this)
                                        {
                                            continue;
                                        }
                                    }

                                    if (TileManager.Instance.Grid[XGrid, YGrid + 1].GetComponent<Tile>().Special)
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }

                                //UPRight
                                if (XGrid + 1 < TileManager.Instance.Width && YGrid + 1 < TileManager.Instance.Height)
                                {
                                    if (TileManager.Instance.Grid[XGrid + 1, YGrid + 1].GetComponent<Tile>().Unit)
                                    {
                                        if (TileManager.Instance.Grid[XGrid + 1, YGrid + 1].GetComponent<Tile>().Unit != this)
                                        {
                                            continue;
                                        }
                                    }

                                    if (TileManager.Instance.Grid[XGrid + 1, YGrid + 1].GetComponent<Tile>().Special)
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }

                            }
                        }

                        if (!WeaponRange)
                        {
                            MoveableTiles.Add(AdjacentTile.GetComponent<Tile>());
                        }

                        if (AdjacentTile.GetComponent<Tile>().Unit)
                        {
                            if (AdjacentTile.GetComponent<Tile>().Unit != this)
                            {
                                MoveEnd = true;
                            }
                        }
                        else if (AdjacentTile.GetComponent<Tile>().Special)
                        {
                            MoveEnd = true;
                        }
                        else
                        {
                            NextLayer.Add(AdjacentTile);
                        }


                    }

                    if (!AttackTiles.Contains(AdjacentTile.GetComponent<Tile>()))
                    {
                        AttackTiles.Add(AdjacentTile.GetComponent<Tile>());
                    }

                    if (ShowTile)
                    {
                        AdjacentTile.GetComponent<Tile>().Show(WeaponRange);
                    }
                }
            }

            if (MoveEnd)
            {
                OuterMostMove.Add(tile);
            }
        }

        return NextLayer;
    }

    internal List<GameObject> WeaponRangeAttack(List<GameObject> tiles, bool ShowTiles)
    {
        List<GameObject> NextLayer = new List<GameObject>();

        foreach (GameObject tile in tiles)
        {
            foreach (GameObject AdjacentTile in tile.GetComponent<Tile>().AdjacentTiles)
            {
                if (AdjacentTile.GetComponent<Tile>().CanMoveOn || AdjacentTile.GetComponent<Tile>().Unit || AdjacentTile.GetComponent<Tile>().Special)
                {
                    if (!NextLayer.Contains(AdjacentTile))
                    {
                        NextLayer.Add(AdjacentTile);
                    }

                    if (!AttackTiles.Contains(AdjacentTile.GetComponent<Tile>()))
                    {
                        AttackTiles.Add(AdjacentTile.GetComponent<Tile>());
                    }

                    if (ShowTiles)
                    {
                        AdjacentTile.GetComponent<Tile>().Show(true);
                    }
                }
            }
        }

        return NextLayer;
    }

    //Hides all changed tiles
    internal void HideAllChangedTiles(bool Reset = true)
    {
        foreach (Tile tile in AttackTiles)
        {
            if (Reset)
            {
                tile.Hide();
            }
            else
            {
                tile.WhichColour();
            }
        }

    }

    internal void ShowAllInRangeTiles()
    {
        foreach (Tile tile in AttackTiles)
        {
            if (Interact.Instance.CurrentTile == tile)
            {
                tile.Show(false, true);
                continue;
            }
            tile.WhichColour(this, true);
        }
    }

    //Hides all changed tiles and Clears both lists
    internal void ResetMoveableTiles()
    {
        HideAllChangedTiles();

        MoveableTiles.Clear();
        AttackTiles.Clear();
    }

    internal void FindInRangeTargets(bool IgnoreMovement = false, bool ShowTiles = true, GameObject TileObj = null)
    {
        InRangeTargets.Clear();

        if (MovedForTurn || IgnoreMovement)
        {
            List<GameObject> Tiles = new List<GameObject>();
            Tiles.Add(TileObj ? TileObj : TileManager.Instance.Grid[Position[0], Position[1]]);
            AttackTiles.Clear();
            AttackTiles = new List<Tile>();

            //print(Tiles[0]);

            for (int i = 0; i < EquipedWeapon.Range; i++)
            {
                Tiles = WeaponRangeAttack(Tiles, ShowTiles);
            }
        }

        foreach (Tile tile in AttackTiles)
        {
            if (tile.Unit)
            {
                if (!InRangeTargets.Contains(tile.Unit))
                {
                    if (!tile.Unit.CompareTag(tag))
                    {
                        if (tile.Unit.isAlive)
                        {
                            InRangeTargets.Add(tile.Unit);
                        }
                    }
                }
            }
        }
    }

    //Main Attack Function
    internal void Attack(UnitBase Enemy)
    {
        if (CompareTag("Ally"))
        {
            isSupported(Enemy);
        }
        List<GameObject> tiles = new List<GameObject>();
        tiles.Add(TileManager.Instance.Grid[Position[0], Position[1]]);

        ToAttack = true;
        AttackTarget = Enemy;
        AttackTarget.AttackTarget = this;
        if (CompareTag("Ally"))
        {
            GameManager.Instance.ToolTipCheck(Tutorial.CAttack);
        }

        AttackTiles.Clear();
        AttackTiles = new List<Tile>();
        FindInRangeTargets(true, false);

        //Checks from current Position
        if (InRangeTargets.Count != 0)
        {
            if (InRangeTargets.Contains(Enemy))
            {
                print("Attack from Current Position");
                MoveableArea(false);
                HideAllChangedTiles();
                CalculateReturnAttack();
                return;
            }
        }

        //print("Move");
        MoveableArea(false);
        Move(TileManager.Instance.Grid[Enemy.Position[0], Enemy.Position[1]].GetComponent<Tile>(), true);

        CalculateReturnAttack();
    }

    public void DisplayAttack()
    {
        Interact.Instance.CombatMenu.AttackText.text = CurrentAttack.Name;

        Interact.Instance.CombatMenu.AttackText.gameObject.SetActive(true);
        Interact.Instance.CombatMenu.AttackText.gameObject.GetComponent<UIFade>().ToFadeIn();
        Interact.Instance.CombatMenu.AttackText.gameObject.GetComponent<UIFade>().Both = true;
    }

    void CalculateReturnAttack()
    {
        if (!ReturnAttack)
        {
            //print("Return Attack false");
            AttackTarget.ReturnAttack = AttackTarget.CanReturnAttack(this);
        }
        else
        {
            //print("Return Attack true");
            ReturnAttack = false;
        }
    }

    public void PlayWeaponHitSound()
    {
        SoundManager.Instance.PlaySFX(AttackTarget.WeaponHitSound);
    }

    public void AttackingZoom()
    {
        if (!CurrentAttack.isAOE)
        {
            transform.LookAt(AttackTarget.transform);
            AttackTarget.transform.LookAt(transform);
        }

        AttackCamera.SetActive(true);
        Interact.Instance.VirtualCam.SetActive(false);

        AttackZoomIn = true;

    }

    public void NoDamage()
    {
        if (NoDamageTimer == 0)
        {
            ShowDamageNumbers();
        }

        NoDamageTimer += Time.deltaTime;

        if (NoDamageTimer >= NoDamageTime)
        {
            NoDamageTimer = 0.0f;
            NoDamageZoomIn = false;
            ReturnTo();
        }
    }

    public void HitZoom()
    {
        AttackTarget.AttackCamera.SetActive(true);
        AttackCamera.SetActive(false);

        if (CalcuateHitChance() - AttackTarget.CalculateDodge(this) >= Random.Range(0, 101))
        {
            AttackTarget.DamageToTake = CalculateDamage();

            if (CalculateCritChance() >= Random.Range(0, 101) && CanCrit)
            {
                AttackTarget.DamageToTake *= 3;
                Interact.Instance.CombatMenu.CritEyesImage.sprite = EyesSprite;
                Interact.Instance.CombatMenu.ShowCritMessage();
            }

            if (AttackTarget.CurrentHealth - AttackTarget.DamageToTake > 0)
            {
                AttackTarget.HitZoomIn = true;

            }
            else
            {
                AttackTarget.DeathZoomIn = true;
            }
        }
        else
        {
            AttackTarget.DamageToTake = 0;
            AttackTarget.NoDamageZoomIn = true;
        }


        if (CompareTag("Ally") && !ReturnAttack)
        {
            EXPPending = true;
            UnitManager.Instance.PendingEXP = true;
        }
    }

    public void ShowDamageNumbers()
    {
        DamageNumbersController.ResetDamageNumber();
        DecreaseHealth(DamageToTake);
        DamageNumbersController.PlayDamage(0, DamageToTake);
    }

    internal void ShowLongDistanceDamageNumbers(int Damage)
    {
        if (Damage < 1)
        {
            Damage = 1;
        }

        BossHitParticle.SetActive(true);

        DamageNumbersController.ResetFarDamageNumber();
        DecreaseHealth(Damage);
        DamageNumbersController.PlayFarDamage(Damage);
    }

    public void ReturnTo()
    {
        if (ReturnAttack && ReturnAttackPossible)
        {
            AttackingZoom();
        }
        else
        {
            ReturnMainCamera();
        }
    }

    internal void PlaySpecialAnim()
    {
        AnimControl.ChangeAnim("Special", CombatAnimControl.AnimParameters.Special);

        WaitUnit();
        SpecialZoomIn = false;
    }

    internal void PlayAttackAnim()
    {
        AnimControl.ChangeAnim("Attack", CombatAnimControl.AnimParameters.Attack);
        //print("Attack " + gameObject);
        SoundManager.Instance.PlaySFX(AttackSound);

        ResetMoveableTiles();

        WaitUnit();

        AttackZoomIn = false;
    }

    void PlayHitAnim()
    {
        AnimControl.ChangeAnim("Hit", CombatAnimControl.AnimParameters.Hit);
        SoundManager.Instance.PlaySFX(HitSound);
        //print("Hit sound " + gameObject);

        HitZoomIn = false;
    }

    void PlayDeathAnim()
    {
        AnimControl.ChangeAnim("Death", CombatAnimControl.AnimParameters.Death);
        SoundManager.Instance.PlaySFX(DeathSound);
        //print("Death sound " + gameObject);
        isAlive = false;

        DeathZoomIn = false;
    }

    public void ResetAnimation()
    {
        AnimControl.ChangeAnim("Idle", CombatAnimControl.AnimParameters.Idle);
    }

    public void ReturnMainCamera()
    {
        ResetAnimation();
        Interact.Instance.VirtualCam.SetActive(true);
        AttackCamera.SetActive(false);
    }

    public void OnDeath()
    {
        TileManager.Instance.Grid[Position[0], Position[1]].GetComponent<Tile>().ChangeOccupant(null, GetComponent<BossAI>() ? GetComponent<BossAI>().isMultiTile : false);

        if (CompareTag("Enemy"))
        {
            if (!UnitManager.Instance.DeadEnemyUnits.Contains(this))
            {
                UnitManager.Instance.DeadEnemyUnits.Add(this);
            }

            if (ItemDrop)
            {
                if (AttackTarget.Inventory.Count == 6)
                {
                    AttackTarget.Inventory.Add(ItemDrop);
                }
                else
                {
                    GameManager.Instance.Convoy.Add(ItemDrop);
                }
            }

            if (MoneyDrop != 0)
            {
                GameManager.Instance.Money += MoneyDrop;
            }

        }
        else
        {
            if(EXPPending)
            {
                UnitManager.Instance.PendingEXP = false;
            }

            if (!UnitManager.Instance.DeadAllyUnits.Contains(this))
            {
                UnitManager.Instance.DeadAllyUnits.Add(this);
            }
        }

        AttackCamera.SetActive(false);
        Interact.Instance.VirtualCam.SetActive(true);
        gameObject.SetActive(false);
    }

    internal void GainSupportEXP(int Damage)
    {
        foreach (UnitBase Unit in SupportedUnits)
        {
            for (int i = 0; i < SupportsWith.Count; i++)
            {
                if (Unit.UnitName == SupportsWith[i].UnitObj.GetComponent<UnitBase>().UnitName)
                {
                    SupportsWith[i].EXP += Mathf.RoundToInt(Damage * Random.Range(0.15f, 0.25f));
                }
            }
        }
    }

    internal void GainWeaponEXP()
    {
        switch (EquipedWeapon.WeaponType)
        {
            case WeaponType.Bow:
                {
                    BowProficiency += Mathf.RoundToInt((EquipedWeapon.ProficiencyIncrease + CurrentAttack.ProficiencyIncreaseMultiplier) * Random.Range(0.25f, 0.35f));

                    if (BowProficiency >= RankEXP[BowLevel - 1])
                    {
                        BowProficiency++;
                    }

                    break;
                }
            case WeaponType.Sword:
                {
                    SwordProficiency += Mathf.RoundToInt((EquipedWeapon.ProficiencyIncrease + CurrentAttack.ProficiencyIncreaseMultiplier) * Random.Range(0.25f, 0.35f));

                    if (SwordProficiency >= RankEXP[SwordLevel - 1])
                    {
                        SwordProficiency++;
                    }

                    break;
                }
            case WeaponType.Staff:
                {
                    MagicProficiency += Mathf.RoundToInt((EquipedWeapon.ProficiencyIncrease + CurrentAttack.ProficiencyIncreaseMultiplier) * Random.Range(0.25f, 0.35f));

                    if (MagicProficiency >= RankEXP[MagicLevel - 1])
                    {
                        MagicProficiency++;
                    }

                    break;
                }
            case WeaponType.Gauntlets:
                {
                    FistProficiency += Mathf.RoundToInt((EquipedWeapon.ProficiencyIncrease + CurrentAttack.ProficiencyIncreaseMultiplier) * Random.Range(0.25f, 0.35f));

                    if (FistProficiency >= RankEXP[FistLevel - 1])
                    {
                        FistProficiency++;
                    }

                    break;
                }
        }
    }

    internal void GainClassEXP(int Damage)
    {
        if (Class.Level > Class.TotalEXPNeeded.Count)
        {
            return;
        }

        Class.EXP += Mathf.RoundToInt((Damage + EquipedWeapon.ProficiencyIncrease + CurrentAttack.ProficiencyIncreaseMultiplier) * Random.Range(0.15f, 0.25f));

        if (Class.EXP >= Class.TotalEXPNeeded[Class.Level - 1])
        {
            Interact.Instance.CombatMenu.ClassToLevel = this;
            Class.Level++;

            Interact.Instance.CombatMenu.NewAttack = Class.NewAbility(this);
        }
    }

    internal void GainCharacterEXP(int Damage)
    {
        EXP += Mathf.RoundToInt(Damage * Random.Range(0.25f, 0.35f));

        LevelUp();
    }

    //Main LevelUP
    void LevelUp()
    {
        if (Level > EXPNeeded.Count)
        {
            return;
        }

        if (EXP >= EXPNeeded[Level - 1])
        {
            int NewAmount = Mathf.FloorToInt(GrowthTotal(Class.StrengthGrowthRate, GrowthRateStrength));
            Strength += NewAmount;
            Interact.Instance.CombatMenu.StatIncrease(Stats.Str, NewAmount);

            NewAmount = Mathf.FloorToInt(GrowthTotal(Class.DexterityGrowthRate, GrowthRateDexterity));
            Dexterity += NewAmount;
            Interact.Instance.CombatMenu.StatIncrease(Stats.Dex, NewAmount);

            NewAmount = Mathf.FloorToInt(GrowthTotal(Class.MagicGrowthRate, GrowthRateMagic));
            Magic += NewAmount;
            Interact.Instance.CombatMenu.StatIncrease(Stats.Mag, NewAmount);

            NewAmount = Mathf.FloorToInt(GrowthTotal(Class.DefenceGrowthRate, GrowthRateDefence));
            Defence += NewAmount;
            Interact.Instance.CombatMenu.StatIncrease(Stats.Def, NewAmount);

            NewAmount = Mathf.FloorToInt(GrowthTotal(Class.ResistanceGrowthRate, GrowthRateResistance));
            Resistance += NewAmount;
            Interact.Instance.CombatMenu.StatIncrease(Stats.Res, NewAmount);

            NewAmount = Mathf.FloorToInt(GrowthTotal(Class.SpeedGrowthRate, GrowthRateSpeed));
            Speed += NewAmount;
            Interact.Instance.CombatMenu.StatIncrease(Stats.Speed, NewAmount);

            NewAmount = Mathf.FloorToInt(GrowthTotal(Class.LuckGrowthRate, GrowthRateLuck));
            Luck += NewAmount;
            Interact.Instance.CombatMenu.StatIncrease(Stats.Luck, NewAmount);

            NewAmount = Mathf.FloorToInt(GrowthTotal(Class.HPGrowthRate, GrowthRateHP));
            HealthMax += NewAmount;
            Interact.Instance.CombatMenu.StatIncrease(Stats.HP, NewAmount);

            Level++;
            Interact.Instance.CombatMenu.ToLevel = this;
        }
    }

    float GrowthTotal(int ClassRate, int CharacterRate)
    {
        float Increase = ClassRate + CharacterRate;

        Increase /= 100;

        float Rate = Increase % 1;

        if (Random.Range(0, 101) <= Rate)
        {
            Increase++;
        }

        return Increase;
    }

    internal int CalculateDodge(UnitBase OtherUnit)
    {
        int Dodge = (TotalSpeed() * 2 - OtherUnit.TotalSpeed()) + TotalLuck() + +AddSupport(SupportIncrease.Dodge);
        //print("Dodge " + Dodge);

        return Dodge;
    }

    internal int CalculateDamage(UnitBase Unit = null)
    {
        int Damage;
        if (EquipedWeapon)
        {
            switch(EquipedWeapon.WeaponType)
            {
                case WeaponType.Sword:
                    {
                        Damage = Mathf.RoundToInt(TotalStrength() + EquipedWeapon.Damage + (CurrentAttack.DamageMultiplier / (6 - SwordLevel))) - (Unit?Unit.CalculatePhysicalDefence(EquipedWeapon.WeaponType): AttackTarget.CalculatePhysicalDefence(EquipedWeapon.WeaponType));
                        break;
                    }
                case WeaponType.Bow:
                    {
                        Damage = Mathf.RoundToInt(TotalDexterity() + EquipedWeapon.Damage + (CurrentAttack.DamageMultiplier / (6 - BowLevel))) - (Unit ? Unit.CalculatePhysicalDefence(EquipedWeapon.WeaponType) : AttackTarget.CalculatePhysicalDefence(EquipedWeapon.WeaponType));
                        break;
                    }
                case WeaponType.Gauntlets:
                    {
                        Damage = Mathf.RoundToInt(TotalStrength() + EquipedWeapon.Damage + (CurrentAttack.DamageMultiplier / (6 - FistLevel))) - (Unit ? Unit.CalculatePhysicalDefence(EquipedWeapon.WeaponType) : AttackTarget.CalculatePhysicalDefence(EquipedWeapon.WeaponType));
                        break;
                    }
                case WeaponType.Staff:
                    {
                        Damage = Mathf.RoundToInt(TotalMagic() + EquipedWeapon.Damage + (CurrentAttack.DamageMultiplier / (6 - MagicLevel))) - (Unit ? Unit.CalculateMagicDefence(EquipedWeapon.WeaponType) : AttackTarget.CalculateMagicDefence(EquipedWeapon.WeaponType));
                        break;
                    }
                default:
                    {
                        Damage = Mathf.RoundToInt(TotalStrength() + EquipedWeapon.Damage + (CurrentAttack.DamageMultiplier / (6))) - (Unit ? Unit.CalculatePhysicalDefence(EquipedWeapon.WeaponType) : AttackTarget.CalculatePhysicalDefence(EquipedWeapon.WeaponType));
                        break;
                    }
            }
        }
        else
        {
            Damage = Mathf.RoundToInt(TotalStrength() + EquipedWeapon.Damage + (CurrentAttack.DamageMultiplier / (5))) - (Unit ? Unit.CalculatePhysicalDefence(EquipedWeapon.WeaponType) : AttackTarget.CalculatePhysicalDefence(EquipedWeapon.WeaponType));
        }
        
        return Damage + AddSupport(SupportIncrease.Damage);
    }

    internal int MultiAttack(UnitBase OtherUnit)
    {
        int MultiAttack = Mathf.RoundToInt((TotalSpeed() + TotalLuck() / (OtherUnit.TotalSpeed() * 3)));

        if(MultiAttack < 1)
        {
            MultiAttack = 1;
        }

        return MultiAttack;
    }

    internal int CalculateMagicDefence(WeaponType Weapon)
    {
        int MDefence = TotalDefence() + (TotalResistance() / 3) + AddSupport(SupportIncrease.DefRes);

        switch (Weapon)
        {
            case WeaponType.Bow:
                {
                    MDefence += RankBonus[BowLevel];
                    break;
                }
            case WeaponType.Gauntlets:
                {
                    MDefence += RankBonus[FistLevel];
                    break;
                }
            case WeaponType.Staff:
                {
                    MDefence += RankBonus[MagicLevel];
                    break;
                }
            case WeaponType.Sword:
                {
                    MDefence += RankBonus[SwordLevel];
                    break;
                }
        }

        return MDefence;
    }

    internal int CalculatePhysicalDefence(WeaponType Weapon)
    {
        int PDefence = TotalDefence() + (TotalResistance()/3) + AddSupport(SupportIncrease.DefRes);

        switch(Weapon)
        {
            case WeaponType.Bow:
                {
                    PDefence += RankBonus[BowLevel];
                    break;
                }
            case WeaponType.Gauntlets:
                {
                    PDefence += RankBonus[FistLevel];
                    break;
                }
            case WeaponType.Staff:
                {
                    PDefence += RankBonus[MagicLevel];
                    break;
                }
            case WeaponType.Sword:
                {
                    PDefence += RankBonus[SwordLevel];
                    break;
                }
            default:
                {
                    //print("No Weapon");
                    break;
                }
        }

        return PDefence;
    }

    internal bool CanReturnAttackIncludeMovement(UnitBase Unit)
    {
        AttackTiles.Clear();
        AttackTiles = new List<Tile>();

        List<GameObject> Tiles = new List<GameObject>();

        Tiles.Add(TileManager.Instance.Grid[Position[0], Position[1]]);

        AttackableArea(Tiles, false);

        if (AttackTiles.Contains(Unit.AttackTile))
        {
            MoveableArea(false);
            //HideAllChangedTiles();
            return true;
        }
        MoveableArea(false);
        //HideAllChangedTiles();
        return false;
    }

    internal bool CanReturnAttack(UnitBase OtherUnit)
    {
        List<GameObject> Tile = new List<GameObject>();
        Tile.Add(TileManager.Instance.Grid[Position[0], Position[1]]);

        FindInRangeTargets(true, false);


        if (InRangeTargets.Contains(OtherUnit))
        {
            //print("Return Attack" + this.gameObject.name);
            return true;
        }
        //print("Too Far " + this.gameObject.name);
        return false;
    }

    int AddSupport(SupportIncrease Stat)
    {
        int TotalAdded = 0;

        foreach (UnitBase Unit in SupportedUnits)
        {
            foreach (UnitSupports Supportable in SupportsWith)
            {
                if (Unit == Supportable.Unit)
                {
                    for (int i = 0; i < Supportable.Level; i++)
                    {
                        if (Stat == Supportable.SupportStats[i].Stat)
                        {
                            TotalAdded += Supportable.SupportStats[i].Increase;
                        }
                    }

                    break;
                }
            }
        }

        return TotalAdded;
    }

    internal bool isSupported(UnitBase ToAttackUnit)
    {
        Tile tile;

        SupportedUnits.Clear();
        SupportedUnits = new List<UnitBase>();

        FindInRangeTargets(true, false);

        if (InRangeTargets.Contains(ToAttackUnit))
        {
            foreach (GameObject Adjacent in TileManager.Instance.Grid[Position[0], Position[1]].GetComponent<Tile>().AdjacentTiles)
            {
                tile = Adjacent.GetComponent<Tile>();

                if (tile.Unit)
                {
                    if (tile.Unit.CompareTag(tag) && tile.Unit != this && !tile.Unit.GetComponent<UnitControlled>().Recruited)
                    {
                        SupportedUnits.Add(tile.Unit);
                    }
                }
            }

            AttackTile = TileManager.Instance.Grid[Position[0], Position[1]].GetComponent<Tile>();
        }
        else
        {
            List<Tile> PathTo = new List<Tile>();
            PathTo = FindRouteTo(TileManager.Instance.Grid[ToAttackUnit.Position[0], ToAttackUnit.Position[1]].GetComponent<Tile>());

            foreach (GameObject Adjacent in PathTo[PathTo.Count - 1].AdjacentTiles)
            {
                tile = Adjacent.GetComponent<Tile>();

                if (tile.Unit)
                {
                    if (tile.Unit.CompareTag(tag) && tile.Unit != this && !tile.Unit.GetComponent<UnitControlled>().Recruited)
                    {
                        SupportedUnits.Add(tile.Unit);
                    }
                }
            }

            AttackTile = PathTo[PathTo.Count - 1];
        }

        MoveableArea(false);

        if (SupportedUnits.Count > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    internal int CalcuateHitChance()
    {
        int HitChance = Mathf.RoundToInt((float)CurrentAttack.HitRateMultiplier + (float)(EquipedWeapon.HitRate * 0.2)) + AddSupport(SupportIncrease.HitRate);

        if(HitChance > 100)
        {
            HitChance = 100;
        }
        else if(HitChance < 1)
        {
            HitChance = 1;
        }
        return HitChance;
    }

    internal int CalculateCritChance()
    {
        int CritChance = Mathf.RoundToInt((float)CurrentAttack.CritRateMultiplier + (float)(EquipedWeapon.CritRate * 0.2)) + AddSupport(SupportIncrease.CritRate);
        if (CritChance > 100)
        {
            CritChance = 100;
        }
        return CritChance;
    }

    internal int CalculateReturnDamage()
    {
        int ReturnDamage = 0;
        return ReturnDamage;
    }

    internal int CalculateReturnHitChance()
    {
        int ReturnHitChance = 0;
        return ReturnHitChance;
    }

    internal int CalculateReturnCritChance()
    {
        int ReturnCritChance = 0;
        return ReturnCritChance;
    }

    internal int TotalStrength()
    {
        if (Class)
        {
            return Strength + Class.Strength;
        }
        else
        {
            return Strength;
        }
    }

    internal int TotalDexterity()
    {
        if (Class)
        {
            return Dexterity + Class.Dexterity;
        }
        else
        {
            return Dexterity;
        }
    }

    internal int TotalMagic()
    {
        if (Class)
        {
            return Magic + Class.Magic;
        }
        else
        {
            return Magic;
        }
    }

    internal int TotalDefence()
    {
        if (Class)
        {
            return Defence + Class.Defence;
        }
        else
        {
            return Defence;
        }
    }

    internal int TotalResistance()
    {
        if (Class)
        {
            return Resistance + Class.Resistance;
        }
        else
        {
            return Resistance;
        }
    }

    internal int TotalSpeed()
    {
        if (Class)
        {
            return Speed + Class.Speed;
        }
        else
        {
            return Speed;
        }
    }

    internal int TotalLuck()
    {
        if (Class)
        {
            return Luck + Class.Luck;
        }
        else
        {
            return Luck;
        }
    }

    internal void DecreaseHealth(int Attack)
    {
        CurrentHealth -= Attack;
    }

    internal void IncreaseHealth(int Health)
    {
        HealingParticle.SetActive(true);

        if (Health + CurrentHealth < HealthMax)
        {
            CurrentHealth += Health;
        }
        else
        {
            CurrentHealth = HealthMax;
        }
    }

    private void OnMouseEnter()
    {
        if (!UnitManager.Instance.SetupFinished || !Interact.Instance.VirtualCam.activeInHierarchy || Options.Instance.OptionsMenuUI.activeInHierarchy
            || Interact.Instance.CombatMenu.VictoryScreen.activeInHierarchy || Interact.Instance.CombatMenu.DefeatScreen.activeInHierarchy)
        {
            return;
        }

        if (EndTurn)
        {
            return;
        }

        if (!Interact.Instance.CombatMenu.transform.GetChild(0).gameObject.activeInHierarchy 
            && !Interact.Instance.CombatMenu.AttackMenuObject.gameObject.activeInHierarchy
            && Interact.Instance.SelectedUnit == null)
        {
            if (!CompareTag("Enemy"))
            {
                if (!MovedForTurn)
                {
                    ShowAllInRangeTiles();
                }
                else if (!AttackedForTurn)
                {
                    List<GameObject> Tiles = new List<GameObject>();
                    Tiles.Add(TileManager.Instance.Grid[Position[0], Position[1]]);
                    AttackableArea(Tiles);
                }
            }
            else
            {
                if (GetComponent<BossAI>())
                {
                    if (GetComponent<BossAI>().PendingAttack)
                    {
                        //GetComponent<BossAI>().ShowDamageRange();
                    }
                    else
                    {
                        ShowAllInRangeTiles();
                    }
                }
                else
                {
                    ShowAllInRangeTiles();
                }
            }
        }

        if (!Interact.Instance.CombatMenu.AttackMenuObject.gameObject.activeInHierarchy
             && !Interact.Instance.CombatMenu.CombatMenuObject.gameObject.activeInHierarchy)
        {
            TileManager.Instance.Grid[Position[0], Position[1]].GetComponent<Tile>().Show(false, true);
        }
    }

    private void OnMouseExit()
    {
        if (!UnitManager.Instance.SetupFinished)
        {
            return;
        }

        if (!Interact.Instance.CombatMenu.transform.GetChild(0).gameObject.activeInHierarchy
            && !Interact.Instance.CombatMenu.AttackMenuObject.gameObject.activeInHierarchy
            && Interact.Instance.SelectedUnit == null)
        {
            HideAllChangedTiles();
        }
    }

    internal void ChangeWeaponImage()
    {
         WeaponImage.sprite = EquipedWeapon.WeaponImage;
    }

    public void TurnChange()
    {
        HideAllChangedTiles();

        MovedForTurn = false;
        AttackedForTurn = false;
        EndTurn = false;

        MoveableArea(false);
    }

    internal void WaitUnit(bool Playsound = true)
    {
        if (Playsound)
        {
            SoundManager.Instance.PlaySFX(Interact.Instance.CombatMenu.ButtonPress);
        }
        MovedForTurn = true;
        AttackedForTurn = true;
        EndTurn = true;

        Interact.Instance.SelectedUnit = null;
        Interact.Instance.UISelectedUnit();

        Interact.Instance.CombatMenu.CombatMenuObject.GetComponent<UIFade>().ToFadeOut();
        Interact.Instance.CombatMenu.AttackMenuObject.GetComponent<UIFade>().ToFadeOut();
        Interact.Instance.CombatMenu.InventoryObject.GetComponent<UIFade>().ToFadeOut();

        if (CameraMove.Instance.FollowTarget)
        {
            if (!CameraMove.Instance.Override)
            {
                CameraMove.Instance.FollowTarget = null;
            }
        }

        if (gameObject.CompareTag("Ally"))
        {
            GameManager.Instance.ToolTipCheck(Tutorial.CWait);
            TurnManager.Instance.UnitsToMove -= 1;
        }

        UnitManager.Instance.UnitUpdate.Invoke();

        HideAllChangedTiles(false);
    }

    //A* Pathfinding
    internal List<Tile> FindRouteTo(Tile TargetTile, bool Ignore = false)
    {
        List<Node> ToCheckNodes = new List<Node>();
        Dictionary<Tile, Node> CheckedNodes = new Dictionary<Tile, Node>();

        Node Start = new Node { Tile = TileManager.Instance.Grid[Position[0], Position[1]].GetComponent<Tile>(), FCost = 0, GCost = 0, HCost = 0 };
        Node End = new Node { Tile = TargetTile, FCost = 0, GCost = 0, HCost = 0 };

        ToCheckNodes.Add(Start);
        Node CurrentNode;

        int G;
        List<Tile> PathTo = new List<Tile>();

        while (ToCheckNodes.Count > 0)
        {
            CurrentNode = ToCheckNodes[0];

            foreach(Node Node in ToCheckNodes)
            {
                if(Node.FCost < CurrentNode.FCost)
                {
                    CurrentNode = Node;
                }
            }

            CheckedNodes.Add(CurrentNode.Tile, CurrentNode);
            ToCheckNodes.Remove(CurrentNode);

            if(CurrentNode.Tile == End.Tile)
            {
                PathTo = FindPath(CurrentNode, Ignore);
                //print("Success");
                return PathTo;
            }

            foreach(GameObject AdjacentTile in CurrentNode.Tile.AdjacentTiles)
            {

                if (CheckedNodes.ContainsKey(AdjacentTile.GetComponent<Tile>()) || 
                    (AdjacentTile.GetComponent<Tile>().Unit && AdjacentTile.GetComponent<Tile>() != End.Tile) || AdjacentTile.GetComponent<Tile>().Special)
                {
                    continue;
                }

                if (GetComponent<BossAI>())
                {
                    if (GetComponent<BossAI>().isMultiTile)
                    {
                        int XGrid = AdjacentTile.GetComponent<Tile>().GridPosition[0];
                        int YGrid = AdjacentTile.GetComponent<Tile>().GridPosition[1];

                        //Right
                        if (XGrid + 1 < TileManager.Instance.Width)
                        {
                            if (TileManager.Instance.Grid[XGrid + 1, YGrid].GetComponent<Tile>().Unit && TileManager.Instance.Grid[XGrid + 1, YGrid].GetComponent<Tile>() != End.Tile)
                            {
                                if (TileManager.Instance.Grid[XGrid + 1, YGrid].GetComponent<Tile>().Unit != this)
                                {
                                    continue;
                                }
                            }

                            if (TileManager.Instance.Grid[XGrid + 1, YGrid].GetComponent<Tile>().Special)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        //UP
                        if (YGrid + 1 < TileManager.Instance.Height)
                        {
                            if (TileManager.Instance.Grid[XGrid, YGrid + 1].GetComponent<Tile>().Unit && TileManager.Instance.Grid[XGrid, YGrid + 1].GetComponent<Tile>() != End.Tile)
                            {
                                if (TileManager.Instance.Grid[XGrid, YGrid + 1].GetComponent<Tile>().Unit != this)
                                {
                                    continue;
                                }
                            }

                            if (TileManager.Instance.Grid[XGrid, YGrid + 1].GetComponent<Tile>().Special)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        //UPRight
                        if (XGrid + 1 < TileManager.Instance.Width && YGrid + 1 < TileManager.Instance.Height)
                        {
                            if (TileManager.Instance.Grid[XGrid + 1, YGrid + 1].GetComponent<Tile>().Unit && TileManager.Instance.Grid[XGrid + 1, YGrid + 1].GetComponent<Tile>() != End.Tile)
                            {
                                if (TileManager.Instance.Grid[XGrid + 1, YGrid + 1].GetComponent<Tile>().Unit != this)
                                {
                                    continue;
                                }
                            }

                            if (TileManager.Instance.Grid[XGrid + 1, YGrid + 1].GetComponent<Tile>().Special)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                    }
                }

                G = CurrentNode.GCost + 1;

                if(G<CurrentNode.GCost || !HasTile(ToCheckNodes, AdjacentTile.GetComponent<Tile>()))
                {
                    Node AdjacentNode = new Node { PreviousTile = CurrentNode, Tile = AdjacentTile.GetComponent<Tile>(), GCost = G, HCost = DistanceToTile(AdjacentTile.GetComponent<Tile>(), End.Tile) };
                    AdjacentNode.FCost = AdjacentNode.GCost + AdjacentNode.HCost;

                    if(!ToCheckNodes.Contains(AdjacentNode))
                    {
                        ToCheckNodes.Add(AdjacentNode);
                    }
                }
            }
        }

        //print("Failed " + gameObject);
        return PathTo;
    }

    List<Tile> FindPath(Node EndNode, bool Ignore)
    {
        List<Tile> PathTo = new List<Tile>();
        Node Node = EndNode;

        if ((Node.Tile.Unit == null && (MoveableTiles.Contains(Node.Tile)) || Ignore))
        {
            PathTo.Add(EndNode.Tile);
        }

        while (Node.Tile != TileManager.Instance.Grid[Position[0], Position[1]].GetComponent<Tile>())
        {
            if ((Node.PreviousTile.Tile.Unit == null && (MoveableTiles.Contains(Node.PreviousTile.Tile)) || Ignore))
            {
                PathTo.Add(Node.PreviousTile.Tile);
            }
            
            Node = Node.PreviousTile;
        }

        if(GetComponent<BossAI>())
        {
            if(GetComponent<BossAI>().isMultiTile)
            {
                if (PathTo.Count > 1)
                {
                    //Path.RemoveRange(0, Mathf.FloorToInt(GetComponent<BossAI>().MutiTileAmount/2));
                }
                else
                {
                    Moving = false;
                    Interact.Instance.UnitMoving = false;
                }
            }
        }

        if (EquipedWeapon.Range < Path.Count && ToAttack)
        {
            print("Remove");
            Path.RemoveRange(0, EquipedWeapon.Range - 1);
        }

        PathTo.Reverse();

        return PathTo;
    }

    int DistanceToTile(Tile StartTile, Tile EndTile)
    {
        return Mathf.Abs(StartTile.GridPosition[0] - EndTile.GridPosition[0]) + Mathf.Abs(StartTile.GridPosition[1] - EndTile.GridPosition[1]);
    }

    bool HasTile(List<Node> Nodes, Tile EndTile)
    {
        foreach(Node Node in Nodes)
        {
            if(EndTile == Node.Tile)
            {
                return true;
            }
        }

        return false;
    }

}
