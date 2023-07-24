using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AttackStats
{
    public UnitBase AttackingUnit;
    public Dictionary<UnitBase, int> TargetUnit;
    public float AttackValue;
}


public class UnitAI : UnitBase
{
    public enum BehaviourTypes
    {
        NOKAV,
        AttackValue,
        Patient,
        Inpatient
    }

    UnitBase InRangeTarget;
    public BehaviourTypes Behaviour;
    internal List<AttackStats> AttackProfiles;
    internal AttackStats CurrentAttackStats;

    public GameObject AllyVariant;

    override internal void Start()
    {
        base.Start();
        AttackProfiles = new List<AttackStats>();
    }


    override public void Update()
    {
        base.Update();
        
        if(!Moving)
        {
            if(UnitManager.Instance.EnemyMoving == this)
            {
                UnitManager.Instance.EnemyMoving = null;
            }
        }
        else
        {
            CameraMove.Instance.FollowTarget = transform;

            if(Path.Count == 0)
            {
                Moving = false;
                Interact.Instance.UnitMoving = false;
            }
        }
    }

    internal void AttemptRecruit(float Chance)
    {
        if(Chance >= Random.Range(0, 101))
        {
            GameObject AllyVersion = Instantiate(AllyVariant, UnitManager.Instance.gameObject.transform, true);

            UnitBase UnitBase = AllyVersion.GetComponent<UnitBase>();
            UnitManager.Instance.AllyUnits.Add(AllyVersion);
            TurnManager.Instance.TurnChange.AddListener(UnitBase.TurnChange);

            UnitManager.Instance.UnitUpdate.AddListener(() => { UnitBase.MoveableArea(false); });

            UnitBase.Position = new int[2];
            UnitBase.Position[0] = Position[0];
            UnitBase.Position[1] = Position[1];
            TileManager.Instance.Grid[Position[0], Position[1]].GetComponent<Tile>().ChangeOccupant(UnitBase);

            UnitBase.UIHealth.maxValue = UnitBase.HealthMax;
            UnitBase.UIHealth.value = Mathf.RoundToInt(UnitBase.HealthMax/2);
            UnitBase.CurrentHealth = Mathf.RoundToInt(UnitBase.HealthMax / 2);

            UnitBase.AvailableAttacks = new List<SpecialAttacks>();
            UnitBase.AvailableAttacks = AvailableAttacks;

            UnitBase.WeaponsIninventory = WeaponsIninventory;

            UnitBase.CurrentAttack = UnitBase.AvailableAttacks[0];

            UnitBase.Inventory = Inventory;

            UnitBase.transform.position = transform.position;


            //Add to dead eNEMIES OGUnit

            UnitManager.Instance.DeadEnemyUnits.Add(this);

            CurrentHealth = 0;
            isAlive = false;

            

            //Set inActive OGUnit
            gameObject.SetActive(false);
        }
    }

    internal void MoveAsCloseTo(Tile TargetTile)
    {
        MovedForTurn = true;

        if (Path.Count <= 0)
        {
            Path = new List<Tile>(FindRouteTo(TargetTile));
        }
        else
        {
            //print(TargetTile.name);
        }

        int Index = 0;

        foreach (Tile tile in Path)
        {
            if (MoveableTiles.Contains(tile))
            {
                Index++;
                continue;
            }

            break;
        }

        Path.RemoveRange(Index, Path.Count - Index);

        Moving = true;
        Interact.Instance.UnitMoving = true;

        if (Path.Count > 0)
        {
            TileManager.Instance.Grid[Position[0], Position[1]].GetComponent<Tile>().ChangeOccupant(null, GetComponent<BossAI>() ? GetComponent<BossAI>().isMultiTile : false);
            Position[0] = Path[Path.Count - 1].GridPosition[0];
            Position[1] = Path[Path.Count - 1].GridPosition[1];
        }

        if (Path.Count > 0)
        {
            Path[Path.Count - 1].ChangeOccupant(this, GetComponent<BossAI>() ? GetComponent<BossAI>().isMultiTile : false);
        }

        ResetMoveableTiles();
        UnitManager.Instance.UnitUpdate.Invoke();

    }

    public void PerformAction()
    {
        switch(Behaviour)
        {
            case BehaviourTypes.NOKAV:
                {
                    AttackValue();
                    UnitManager.Instance.PendingEnemies.Add(this);
                    break;
                }
            case BehaviourTypes.AttackValue:
                {
                    AttackValue();
                    UnitManager.Instance.PendingEnemies.Add(this);
                    break;
                }
            case BehaviourTypes.Patient:
                {
                    Patient();
                    WaitUnit();
                    break;
                }
            case BehaviourTypes.Inpatient:
                {
                    Inpatient();
                    WaitUnit();
                    break;
                }

        }
    }

    //Ignores possible move locations
    internal void MoveAnywhere(Tile Target)
    {
        Move(Target, false, true);
        UnitManager.Instance.EnemyMoving = gameObject;
    }

    //Finds a random location to move to
    internal void RandomMovement()
    {
        int RandLocation;
        do
        {
            RandLocation = Random.Range(0, MoveableTiles.Count - 1);
        } while (Move(MoveableTiles[RandLocation]));
    }

    internal bool CanAttack()
    {
        InRangeTargets.Clear();
        InRangeTargets = new List<UnitBase>();

        foreach(Tile tile in AttackTiles)
        {
            if(tile.Unit)
            {
                if(tile.Unit.CompareTag("Ally"))
                {
                    if(FindRouteTo(TileManager.Instance.Grid[tile.GridPosition[0], tile.GridPosition[1]].GetComponent<Tile>()).Count > 0)
                    {
                        //print("InRange Unit - " + tile.Unit);
                        InRangeTargets.Add(tile.Unit);
                    }
                }
            }
        }

        if(InRangeTargets.Count > 0)
        {
            return true;
        }

        return false;
    }

    //Compares current attack target to next unit
    public void FindLowestDefence(UnitBase Unit)
    {
        if (Unit.isAlive)
        {
            if (AttackTarget == null)
            {
                AttackTarget = Unit;
            }

            if (EquipedWeapon.WeaponType == WeaponType.Staff)
            {
                if (AttackTarget.CalculateMagicDefence(WeaponType.Staff) > Unit.CalculateMagicDefence(WeaponType.Staff))
                {
                    AttackTarget = Unit;
                }
            }
            else
            {
                if (AttackTarget.CalculatePhysicalDefence(EquipedWeapon.WeaponType) > Unit.CalculatePhysicalDefence(EquipedWeapon.WeaponType))
                {
                    AttackTarget = Unit;
                }
            }
        }
    }

    internal void CheckCurrentRange()
    {
        if (InRangeTargets.Count > 0)
        {
            AttackTarget = InRangeTargets[0];

            foreach (UnitBase Target in InRangeTargets)
            {
                FindLowestDefence(Target);
            }
        }
    }

    //Attacks lowest defence target if in range otherwise does nothing
    internal void Patient()
    {
        AttackTarget = null;

        if(CanAttack())
        {
            //print("Can Attack");

            CheckCurrentRange();
        }

        if(AttackTarget != null)
        {
            Attack(AttackTarget);
        }
    }

    //Attacks lowest defence (to equiped weapon)  and sets that as the target
    internal void Inpatient()
    {
        AttackTarget = null;

        if (CanAttack())
        {
            CheckCurrentRange();

            Attack(AttackTarget);
            return;
        }

        UnitBase Unit;
        foreach (GameObject UnitObject in UnitManager.Instance.AllyUnits)
        {
            Unit = UnitObject.GetComponent<UnitBase>();
            FindLowestDefence(Unit);
        }

        if (AttackTarget != null)
        {
            FindInRangeTargets(false, false);
            if (InRangeTargets.Contains(AttackTarget))
            {
                Attack(AttackTarget);
            }
            else
            {
                MoveAsCloseTo(TileManager.Instance.Grid[AttackTarget.Position[0], AttackTarget.Position[1]].GetComponent<Tile>());
            }

        }
        else
        {
            RandomMovement();
        }
    }

    //Attack Value 
    internal void AttackValue()
    {
        UnitBase Unit;
        AttackStats Stats;
        int TopAttackValue = 0;

        AttackProfiles.Clear();
        AttackProfiles = new List<AttackStats>();

        foreach (GameObject UnitObject in UnitManager.Instance.AllyUnits)
        {
            Stats = new AttackStats();
            Stats.TargetUnit = new Dictionary<UnitBase, int>();

            Unit = UnitObject.GetComponent<UnitBase>();
            if (Unit.isAlive)
            {
                Stats.TargetUnit.Add(Unit, CalculateDamage(Unit));
                Stats.AttackingUnit = this;
                Stats.AttackValue = (float)CalculateDamage(Unit) / (float)Unit.CurrentHealth;

                if (Stats.TargetUnit[Unit] > TopAttackValue)
                {
                    TopAttackValue = Stats.TargetUnit[Unit];
                    AttackTarget = Unit;
                    CurrentAttackStats = Stats;
                }

                //print(Unit + " - " + Stats.AttackValue);

                AttackProfiles.Add(Stats);
            }
        }

        AttackProfiles.Sort((x, y) => x.AttackValue.CompareTo(y.AttackValue));

        //foreach(AttackStats Stat in AttackProfiles)
        //{
        //    print(Stat.AttackValue);
        //}
    }

    //No over kill - Attack Value
    internal void NOKAV()
    {
        UnitAI Unit;
        bool NoChanges = true;

        if(UnitManager.Instance.DeadAllyUnits.Count == UnitManager.Instance.AllyUnits.Count -1)
        {
            return;
        }

        if (CanAttack())
        {
            CheckCurrentRange();

            Attack(AttackTarget);
            return;
        }

        do
        {
            NoChanges = false;
            foreach (GameObject UnitObject in UnitManager.Instance.EnemyUnits)
            {
                Unit = GetComponent<UnitAI>();

                if (Unit.AttackTarget == AttackTarget)
                {
                    if ((AttackTarget.CurrentHealth - CurrentAttackStats.TargetUnit[AttackTarget] - Unit.CurrentAttackStats.TargetUnit[AttackTarget]) > 0)
                    {
                        if (CurrentAttackStats.AttackValue > Unit.CurrentAttackStats.AttackValue)
                        {
                            if (AttackProfiles.Count > 1)
                            {
                                AttackProfiles.RemoveAt(0);
                                CurrentAttackStats = AttackProfiles[0];
                                NoChanges = true;
                            }
                        }
                    }
                }
            }
        } while (NoChanges);
    }
}
