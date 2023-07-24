using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : UnitAI
{
    public bool isMultiTile = false;
    public int MutiTileAmount = 3;
    internal List<Tile> MultiPositions;
    public List<Tile> AoELocations;
    internal int TurnCount;
    internal int CurrentTurns;
    internal bool PendingAttack = false;
    internal bool PendingDamage = false;
    internal bool ToAoEAttack;
    internal bool ToTaunt = false;
    public AudioClip TauntSound;
    internal Transform LookAtAttack;

    override public void Update()
    {
        base.Update();

        ReturnAttackPossible = false;

        if (PendingDamage && Interact.Instance.VirtualCam.activeInHierarchy)
        {
            AoEAttack();
        }

        if(!Moving && ToTaunt)
        {
            transform.LookAt(LookAtAttack);
            ZoomTaunt();
        }

        if(ToTaunt && AttackCamera.transform.position == Interact.Instance.transform.position)
        {
            ToTaunt = false;
            HideAllChangedTiles();

            PlayTauntAnim();
        }

        //if (PendingAttack)
        //{
        //    ShowDamageRange();
        //}
        //else
        //{
        //    HideDamageRange();
        //}
    }

    public void ZoomTaunt()
    {
        AttackCamera.SetActive(true);
        Interact.Instance.VirtualCam.SetActive(false);

        ShowDamageRange();
    }

    public void PlayTauntAnim()
    {
        AnimControl.ChangeAnim("Taunt", CombatAnimControl.AnimParameters.Attack);
        //print("Taunt sound " + gameObject);
        SoundManager.Instance.PlaySFX(TauntSound);

        ResetMoveableTiles();

        ToTaunt = false;
    }

    public void AoEAttack()
    {
        foreach (Tile tile in AoELocations)
        {
            if (tile.Unit)
            {
                if (tile.Unit != this)
                {
                    AttackTarget = tile.Unit;
                    tile.Unit.AttackTarget = this;
                    tile.Unit.ShowLongDistanceDamageNumbers(CalculateDamage());

                    if (tile.Unit.CurrentHealth <= 0)
                    {
                        UnitManager.Instance.PendingDeath.Add(tile.Unit);
                    }
                }
            }
        }

        HideDamageRange();

        PendingDamage = false;
        PendingAttack = false;
        AoELocations.Clear();
    }

    internal void AoEPatient()
    {
        AttackTarget = null;

        if (PendingAttack)
        {
            IncreaseTurn();
        }
        else
        {
            if (CanAttack())
            {
                CheckCurrentRange();
            }

            if (AttackTarget != null)
            {
                print(AttackTarget);
                MoveAsCloseTo(TileManager.Instance.Grid[AttackTarget.Position[0], AttackTarget.Position[1]].GetComponent<Tile>());

                FindInRangeTargets(false, false);
                CheckCurrentRange();

                if (AttackTarget)
                {
                    AoEDirection(AttackTarget);
                    ToTaunt = true;
                }
            }
        }

        WaitUnit();
    }


    internal void IncreaseTurn()
    {
        CurrentTurns++;

        if(CurrentTurns >= TurnCount)
        {
            CurrentTurns = 0;
            PendingDamage = true;
            ToAttack = true;
        }
        else
        {
            ToTaunt = true;
        }
    }

    internal void AoEDirection(UnitBase Unit)
    {
        //print("Find route");
        //List<Tile> Route = new List<Tile>();

        //Route = FindRouteTo(TileManager.Instance.Grid[Unit.Position[0], Unit.Position[1]].GetComponent<Tile>(), true);

        Direction Dir = Direction.Down;
        Tile ClosestTile = null;

        //print(Route.Count);

        int LastIndex = Path.Count - 1;

        if (Path.Count > 1)
        {
            //print(Path[0].GridPosition[0] + " , " + Path[0].GridPosition[1]);
            //print(AttackTarget.Position[0] + " , " + AttackTarget.Position[1]);
            if (AttackTarget.Position[1] == Path[LastIndex].GridPosition[1])
            {
                if (Path[LastIndex].GridPosition[0] < AttackTarget.Position[0])
                {
                    Dir = Direction.Right;
                }
                else
                {
                    Dir = Direction.Left;
                }
            }
            else
            {
                if (Path[LastIndex].GridPosition[1] < AttackTarget.Position[1])
                {
                    Dir = Direction.Up;
                }
                else
                {
                    Dir = Direction.Down;
                }
            }

            //print(Dir);

            ClosestTile = Path[LastIndex];
        }
        else
        {
            //print("No path");
            foreach (GameObject tile in TileManager.Instance.Grid[Position[0], Position[1]].GetComponent<Tile>().AdjacentTiles)
            {
                //print(tile.name + " " + TileManager.Instance.Grid[Unit.Position[0], Unit.Position[1]]);
                if(tile == TileManager.Instance.Grid[Unit.Position[0], Unit.Position[1]])
                {
                    if (Position[1] == tile.GetComponent<Tile>().GridPosition[1])
                    {
                        if (tile.GetComponent<Tile>().GridPosition[0] > Position[0])
                        {
                            Dir = Direction.Right;
                        }
                        else
                        {
                            Dir = Direction.Left;
                        }
                    }
                    else
                    {
                        if (tile.GetComponent<Tile>().GridPosition[1] > Position[1])
                        {
                            Dir = Direction.Up;
                        }
                        else
                        {
                            Dir = Direction.Down;
                        }
                    }

                    print(ClosestTile);
                    ClosestTile = tile.GetComponent<Tile>();
                    break;
                }
            }
        }

        //print(ClosestTile);


        if (ClosestTile != null)
        {
            AoEDamage(Dir, ClosestTile.gameObject);

            LookAtAttack = ClosestTile.CentrePoint.transform;
        }
        else
        {
            print("no close tile");
        }
    }

    internal void AoEDamage(Direction Dir, GameObject Tile)
    {
        int[] CurrentPosition = new int[2];

        int[] Centering = new int[2];

        Centering[0] = 0;
        Centering[1] = 0;

        AoESpecialAttack AttackAoE = (AoESpecialAttack)CurrentAttack;
        List<GameObject> Tiles = new List<GameObject>();

        Tiles.Add(Tile);


        for (int i = 0; i < (AttackAoE.VerticalRange > AttackAoE.HorizontalRange? AttackAoE.VerticalRange: AttackAoE.HorizontalRange); i++)
        {
            Tiles = FindAoE(Tiles, Dir);
        }

        CurrentTurns = 1;
        TurnCount = AttackAoE.TurnTilDamage;
        PendingAttack = true;
    }

    internal void ShowDamageRange()
    {
        //print("Damge Range");

        foreach(Tile tile in AoELocations)
        {
            tile.WhichColour();
        }
    }

    internal void HideDamageRange()
    {
        foreach (Tile tile in AoELocations)
        {
            tile.WhichColour(Interact.Instance.SelectedUnit, true);
        }
    }

    internal List<GameObject> FindAoE(List<GameObject> tiles, Direction Dir)
    {
        List<GameObject> NextLayer = new List<GameObject>();

        foreach (GameObject tile in tiles)
        {
            foreach (GameObject AdjacentTile in tile.GetComponent<Tile>().AdjacentTiles)
            {
                switch(Dir)
                {
                    case Direction.Up:
                        {
                            if(AdjacentTile.GetComponent<Tile>().GridPosition[1] < Position[1] + 1)
                            {
                                continue;
                            }
                            break;
                        }
                    case Direction.Down:
                        {
                            if (AdjacentTile.GetComponent<Tile>().GridPosition[1] > Position[1] - 1)
                            {
                                continue;
                            }
                            break;
                        }
                    case Direction.Left:
                        {
                            if (AdjacentTile.GetComponent<Tile>().GridPosition[0] > Position[0] - 1)
                            {
                                continue;
                            }
                            break;
                        }
                    case Direction.Right:
                        {
                            if (AdjacentTile.GetComponent<Tile>().GridPosition[0] < Position[0] + 1)
                            {
                                continue;
                            }
                            break;
                        }
                }

                if (!NextLayer.Contains(AdjacentTile))
                {
                    NextLayer.Add(AdjacentTile);
                }

                if (!AoELocations.Contains(AdjacentTile.GetComponent<Tile>()))
                {
                    AoELocations.Add(AdjacentTile.GetComponent<Tile>());
                }

            }
        }

        return NextLayer;
    }
}
