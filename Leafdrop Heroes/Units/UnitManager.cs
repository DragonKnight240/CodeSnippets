using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnitManager : MonoBehaviour
{
    public enum UnitType
    {
        Ally,
        Enemy
    }

    [System.Serializable]
    public struct StartingPositions
    {
        public UnitType UnitType;
        public GameObject Unit;
        public int X;
        public int Y;
    }

    public static UnitManager Instance;
    public List<StartingPositions> UnitPositions;
    public List<GameObject> AllyUnits;
    internal List<GameObject> EnemyUnits;
    internal List<UnitBase> DeadEnemyUnits;
    internal List<UnitBase> DeadAllyUnits;
    public string OverWorldScene;
    internal bool SetupFinished = false;
    internal UnityEvent UnitUpdate;
    internal GameObject EnemyMoving;
    internal List<UnitAI> PendingEnemies;
    internal List<UnitBase> PendingDeath;
    internal BossAI Boss;
    internal bool PendingEXP;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        AllyUnits = new List<GameObject>();
        EnemyUnits = new List<GameObject>();
        DeadEnemyUnits = new List<UnitBase>();
        DeadAllyUnits = new List<UnitBase>();
        UnitUpdate = new UnityEvent();
        PendingEnemies = new List<UnitAI>();
        PendingDeath = new List<UnitBase>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void print()
    {
        print("Played");
    }

    private void Update()
    {
        if (!TurnManager.Instance.isPlayerTurn)
        {
            AIUnitMove();
        }

        if (SetupFinished)
        {
            if (!Interact.Instance.CombatMenu.LevelScreen.activeInHierarchy && !Interact.Instance.CombatMenu.AttackScreen.activeInHierarchy
                && !Interact.Instance.CombatMenu.EXPBar.isActiveAndEnabled && !Interact.Instance.CombatMenu.ClassEXPBar.isActiveAndEnabled
                && !PendingEXP)
            {
                if (DeadEnemyUnits.Count == EnemyUnits.Count && Interact.Instance.VirtualCam.activeInHierarchy)
                {
                    //win
                    print("Win");
                    Interact.Instance.CombatMenu.DisplayVictoryScreen();
                }
                else if (DeadAllyUnits.Count == AllyUnits.Count && Interact.Instance.VirtualCam.activeInHierarchy)
                {
                    //lose
                    print("Lose");
                    Interact.Instance.CombatMenu.DisplayDefeatScreen();
                }
            }
        }
    }

    void AIUnitMove()
    {
        PendingEnemies.Clear();
        PendingEnemies = new List<UnitAI>();

        foreach (GameObject Enemy in EnemyUnits)
        {
            if(Enemy.GetComponent<UnitBase>().Moving || Enemy.GetComponent<UnitBase>().ToAttack  
                || Interact.Instance.VirtualCam.transform.position != Interact.Instance.transform.position || CameraMove.Instance.Override
                || PendingDeath.Count > 0)
            {
                break;
            }

            if (!Enemy.GetComponent<UnitBase>().MovedForTurn && Enemy.GetComponent<UnitBase>().isAlive)
            {
                if (Enemy.GetComponent<ScriptedUnit>())
                {
                    Enemy.GetComponent<ScriptedUnit>().FollowScript();
                }
                else if(Enemy.GetComponent<BossAI>())
                {
                    Enemy.GetComponent<BossAI>().AoEPatient();
                }
                else
                {
                    if (Enemy.GetComponent<UnitAI>())
                    {
                        Enemy.GetComponent<UnitAI>().PerformAction();
                        EnemyMoving = Enemy;
                    }
                    else
                    {
                        Enemy.GetComponent<UnitBase>().WaitUnit();
                    }
                    break;
                }
            }
        }

        if(PendingEnemies.Count > 0)
        {
            PendingEnemies[0].NOKAV();

            foreach(UnitAI Unit in PendingEnemies)
            {
                if (Unit.CanAttack())
                {
                    if (Unit.InRangeTargets.Contains(Unit.AttackTarget))
                    {
                        Unit.Attack(Unit.AttackTarget);
                    }
                    else
                    {
                        Unit.MoveAsCloseTo(TileManager.Instance.Grid[Unit.AttackTarget.Position[0], Unit.AttackTarget.Position[1]].GetComponent<Tile>());
                    }
                }
                else
                {
                    Unit.MoveAsCloseTo(TileManager.Instance.Grid[Unit.AttackTarget.Position[0], Unit.AttackTarget.Position[1]].GetComponent<Tile>());
                }

                Unit.WaitUnit();
                EnemyMoving = Unit.gameObject;
            }

            PendingEnemies = new List<UnitAI>();
        }
    }

    public void PlaceUnits()
    {
        int X;
        int Y;

        int Index = 0;
        CharacterData data;

        foreach (StartingPositions Position in UnitPositions)
        {
            X = Position.X;
            Y = Position.Y;

            if (X > TileManager.Instance.Width)
            {
                X = TileManager.Instance.Width;
            }
            else if (X < 0)
            {
                X = 0;
            }

            if (Y > TileManager.Instance.Height)
            {
                Y = TileManager.Instance.Height;
            }
            else if (Y < 0)
            {
                Y = 0;
            }

            GameObject NewUnit;
            UnitBase UnitBase;

            if (Position.UnitType == UnitType.Ally)
            {
                if (Index < GameManager.Instance.AvailableUnits.Count)
                {
                    NewUnit = Instantiate(GameManager.Instance.AvailableUnits[Index], TileManager.Instance.Grid[X, Y].GetComponent<Tile>().CentrePoint.transform.position, Quaternion.identity, transform);
                    UnitBase = NewUnit.GetComponent<UnitBase>();

                    if (GameManager.Instance.UnitData.Count > 0 && Index < GameManager.Instance.UnitData.Count)
                    {
                        data = GameManager.Instance.UnitData[Index];

                        NewUnit.name = data.UnitName;
                        UnitBase.UnitName = data.UnitName;
                        UnitBase.HealthMax = data.HealthMax;
                        UnitBase.CurrentHealth = data.CurrentHealth;
                        UnitBase.Movement = data.Movement;

                        //Inventory
                        UnitBase.Inventory.Clear();
                        UnitBase.Inventory = data.Inventory;

                        //Stats
                        UnitBase.Level = data.Level;
                        UnitBase.EXP = data.EXP;
                        UnitBase.Strength = data.Strength;
                        UnitBase.Dexterity = data.Dexterity;
                        UnitBase.Magic = data.Magic;
                        UnitBase.Defence = data.Defence;
                        UnitBase.Resistance = data.Resistance;
                        UnitBase.Speed = data.Speed;
                        UnitBase.Luck = data.Luck;

                        //Weapon Proficientcy
                        UnitBase.BowProficiency = data.BowProficiency;
                        UnitBase.BowLevel = data.BowLevel;

                        UnitBase.SwordProficiency = data.SwordProficiency;
                        UnitBase.SwordLevel = data.SwordLevel;

                        UnitBase.MagicProficiency = data.MagicProficiency;
                        UnitBase.MagicLevel = data.MagicLevel;

                        UnitBase.FistProficiency = data.FistProficiency;
                        UnitBase.FistLevel = data.FistLevel;

                        //Class
                        UnitBase.Class = data.Class;

                        //Support
                        UnitBase.SupportsWith = data.Supports;

                        //Attack
                        UnitBase.UnlockedAttacks.Clear();
                        UnitBase.UnlockedAttacks = data.UnlockedAttacks;

                        if (UnitBase.CurrentHealth <= 0)
                        {
                            NewUnit.SetActive(false);
                            UnitBase.isAlive = false;
                            DeadAllyUnits.Add(NewUnit.GetComponent<UnitBase>());
                        }

                        UnitBase.Setup = data.Setup;

                        data = new CharacterData();
                    }
                    else
                    {
                        NewUnit.GetComponent<UnitBase>().CurrentHealth = NewUnit.GetComponent<UnitBase>().HealthMax;

                        NewUnit.GetComponent<UnitBase>().Class = Instantiate(NewUnit.GetComponent<UnitBase>().Class);

                        NewUnit.GetComponent<UnitBase>().Class.FindLevel();
                        NewUnit.GetComponent<UnitBase>().Class.AbilityUnlock(NewUnit.GetComponent<UnitBase>());

                        List<UnitSupports> SupportList = new List<UnitSupports>();

                        foreach (UnitSupports Support in NewUnit.GetComponent<UnitBase>().SupportsWith)
                        {
                            SupportList.Add(Instantiate(Support));
                        }

                        NewUnit.GetComponent<UnitBase>().SupportsWith = SupportList;

                    }

                    AllyUnits.Add(NewUnit);
                    TurnManager.Instance.TurnChange.AddListener(UnitBase.TurnChange);

                    Index++;
                }
                else
                {
                    //print("Skip Unit");
                    Index++;
                    continue;
                }
            }
            else
            {
                NewUnit = Instantiate(Position.Unit, TileManager.Instance.Grid[X, Y].GetComponent<Tile>().CentrePoint.transform.position, Quaternion.Euler(0, 180, 0), transform);
                UnitBase = NewUnit.GetComponent<UnitBase>();
                EnemyUnits.Add(NewUnit);
                TurnManager.Instance.TurnChange.AddListener(NewUnit.GetComponent<UnitBase>().TurnChange);

                NewUnit.GetComponent<UnitBase>().CurrentHealth = NewUnit.GetComponent<UnitBase>().HealthMax;

                if (NewUnit.GetComponent<UnitBase>().Class != null)
                {
                    NewUnit.GetComponent<UnitBase>().Class = Instantiate(NewUnit.GetComponent<UnitBase>().Class);

                    NewUnit.GetComponent<UnitBase>().Class.FindLevel();
                    NewUnit.GetComponent<UnitBase>().Class.AbilityUnlock(NewUnit.GetComponent<UnitBase>());
                }
            }

            if(!GameManager.Instance.CombatTutorialComplete)
            {
                UnitBase.CanCrit = false;
            }

            UnitBase.AvailableAttacks = new List<SpecialAttacks>();

            List<Weapon> WeaponInventory = new List<Weapon>();
            List<Item> InventoryInstances = new List<Item>();

            foreach (Item item in UnitBase.Inventory)
            {
                //print(item.Name + " " + UnitBase.gameObject);

                if (item == null)
                {
                    continue;
                }

                //print(item.Type);

                if (item.Type == ItemTypes.Weapon)
                {
                    Weapon weapon = (Weapon)item;

                    if (!UnitBase.Setup)
                    {
                        InventoryInstances.Add(Instantiate(weapon));
                        WeaponInventory.Add((Weapon)InventoryInstances[InventoryInstances.Count - 1]);
                        WeaponInventory[WeaponInventory.Count - 1].CurrentDurablity = WeaponInventory[WeaponInventory.Count - 1].Durablity;
                    }
                    else
                    {
                        WeaponInventory.Add((Weapon)item);
                        //print(weapon.CurrentDurablity);
                    }

                    

                    if (weapon.Special)
                    {
                        if (!UnitBase.UnlockedAttacks.Contains(weapon.Special))
                        {
                            UnitBase.UnlockedAttacks.Add(weapon.Special);

                            if (UnitBase.EquipedWeapon.WeaponType == weapon.Special.WeaponType)
                            {
                                UnitBase.AvailableAttacks.Add(weapon.Special);
                            }
                        }
                    }
                }
                else if (!UnitBase.Setup)
                {
                    InventoryInstances.Add(Instantiate(item));
                }
            }

            UnitBase.WeaponsIninventory = WeaponInventory;

            if (!UnitBase.Setup)
            {
                UnitBase.Inventory = InventoryInstances;

                UnitBase.Setup = true;
            }

            //print(UnitBase.WeaponsIninventory.Count);

            if (UnitBase.WeaponsIninventory.Count > 0)
            {
                UnitBase.EquipedWeapon = UnitBase.WeaponsIninventory[0];
            }

            foreach (SpecialAttacks Attack in UnitBase.UnlockedAttacks)
            {
                if(UnitBase.AvailableAttacks.Contains(Attack))
                {
                    continue;
                }

                if (UnitBase.EquipedWeapon.WeaponType == Attack.WeaponType)
                {
                    UnitBase.AvailableAttacks.Add(Attack);
                }
            }

            UnitBase.CurrentAttack = UnitBase.AvailableAttacks[0];

            if (UnitBase.WeaponsIninventory.Contains(UnitBase.BareHands))
            {
                UnitBase.WeaponsIninventory.Add(Instantiate(UnitBase.BareHands));
                //UnitBase.WeaponsIninventory.Add(UnitBase.BareHands);
            }

            UnitUpdate.AddListener(() => { UnitBase.MoveableArea(false); });

            UnitBase.Position = new int[2];
            UnitBase.Position[0] = X;
            UnitBase.Position[1] = Y;
            
            if (UnitBase.GetComponent<BossAI>())
            {
                UnitBase.GetComponent<BossAI>().MultiPositions = new List<Tile>();
            }
            
            TileManager.Instance.Grid[X, Y].GetComponent<Tile>().ChangeOccupant(UnitBase, UnitBase.GetComponent<BossAI>()? UnitBase.GetComponent<BossAI>().isMultiTile: false);

            UnitBase.UIHealth.maxValue = UnitBase.HealthMax;
            UnitBase.UIHealth.value = UnitBase.CurrentHealth;

            if(UnitBase.GetComponent<BossAI>())
            {
                Boss = UnitBase.GetComponent<BossAI>();
                UnitBase.ReturnAttackPossible = false;
                if(UnitBase.GetComponent<BossAI>().isMultiTile)
                {
                    if(UnitBase.GetComponent<BossAI>().MutiTileAmount%3 != 0)
                    {
                        int BaseOffset = Mathf.FloorToInt(TileManager.Instance.TileSize / 2);

                        UnitBase.GetComponent<BossAI>().ToCenter = new Vector3(BaseOffset, BaseOffset, (UnitBase.GetComponent<CapsuleCollider>().height/2));

                        UnitBase.transform.position = new Vector3(UnitBase.transform.position.x + UnitBase.ToCenter[0], UnitBase.transform.position.y + UnitBase.ToCenter[2], UnitBase.transform.position.z + UnitBase.ToCenter[1]);
                    }
                }
            }
        }

        TurnManager.Instance.TurnChange.AddListener(Interact.Instance.ResetTargets);
        TurnManager.Instance.UnitsToMove = AllyUnits.Count;

        TurnManager.Instance.Orbs = FindObjectsOfType<MagicOrb>();

        SetupFinished = true;

        GameManager.Instance.NextToolTip();
    }

    public void EndingCombat()
    {
        GameManager.Instance.UnitData.Clear();

        //CharacterData data = new CharacterData();
        UnitBase Ally;

        for (int i = 0; i < AllyUnits.Count; i++)
        {
            //data = new CharacterData();
            Ally = AllyUnits[i].GetComponent<UnitBase>();

            GameManager.Instance.AddCharacterData(Ally);
        }

        OnCombatEndDialogue CombatScript = FindObjectOfType<OnCombatEndDialogue>();
        if(CombatScript)
        {
            CombatScript.SetDialogue();
        }

        if(!GameManager.Instance.CombatTutorialComplete)
        {
            GameManager.Instance.RecruitUnit("Magic");
            GameManager.Instance.CombatTutorialComplete = true;
        }

        GameManager.Instance.inCombat = false;

        if (FindObjectOfType<OnCombatEndMainMenu>())
        {
            FindObjectOfType<OnCombatEndMainMenu>().Menu();
        }
        
        SceneLoader.Instance.LoadNewScene(OverWorldScene);
    }
}
