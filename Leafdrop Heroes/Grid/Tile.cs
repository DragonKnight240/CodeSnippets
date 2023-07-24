using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool CanMoveOn;
    internal UnitBase Unit;
    internal bool Occupied;
    public GameObject CentrePoint;
    internal List<GameObject> AdjacentTiles;
    internal int[] GridPosition;
    public InteractOnGrid Special;

    public Material InRangeMaterial;
    public Material WeaponRangeMaterial; 
    public Material OverlayMaterial; 
    public Material SpecialMaterial;
    internal Material OGMaterial; 

    // Start is called before the first frame update
    void Start()
    {
        if (!CanMoveOn || Unit)
        {
            Occupied = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Unit)
        {
            if(!Unit.isActiveAndEnabled)
            {
                ChangeOccupant(null, Unit.GetComponent<BossAI>() ? Unit.GetComponent<BossAI>().isMultiTile : false);
            }
        }

        if(Special)
        {
            if(!Special.isActiveAndEnabled)
            {
                Special = null;
            }
        }

        if(!Interact.Instance.VirtualCam.activeInHierarchy || (!TurnManager.Instance.isPlayerTurn && !Interact.Instance.VirtualCam.activeInHierarchy))
        {
            Hide();
        }
    }

    public void Show(bool WeaponRange = false, bool Overlay = false, bool Reset = false)
    {
        if (WeaponRange && !Reset && Special == null)
        {
            GetComponent<MeshRenderer>().material = WeaponRangeMaterial;
        }
        else if(!Overlay && !Reset && Special == null)
        {
            GetComponent<MeshRenderer>().material = InRangeMaterial;
        }
        else if(Overlay && !Reset)
        {
            GetComponent<MeshRenderer>().material = OverlayMaterial;
        }
        else if(Special && !Reset)
        {
            GetComponent<MeshRenderer>().material = SpecialMaterial;
        }
        else
        {
            GetComponent<MeshRenderer>().material = OGMaterial;
        }
    }

    public void ChangeOccupant(UnitBase NewUnit, bool FromMulti = false)
    {
        if(FromMulti && NewUnit)
        {
            if (NewUnit.GetComponent<BossAI>())
            {
                if (NewUnit.GetComponent<BossAI>().isMultiTile)
                {
                    NewUnit.GetComponent<BossAI>().MultiPositions.Clear();
                    NewUnit.GetComponent<BossAI>().MultiPositions = new List<Tile>();
                    Tile NextTile;
                    int[] CurrentTile = new int[2];
                    CurrentTile[0] = GridPosition[0];
                    CurrentTile[1] = GridPosition[1];

                    for (int i = 0; i < NewUnit.GetComponent<BossAI>().MutiTileAmount; i++)
                    {
                        for (int j = 0; j < NewUnit.GetComponent<BossAI>().MutiTileAmount; j++)
                        {
                            NextTile = TileManager.Instance.Grid[CurrentTile[0] + i, CurrentTile[1] + j].GetComponent<Tile>();
                            NextTile.Unit = NewUnit;
                            NextTile.Occupied = NewUnit ? true : false;
                            NextTile.CanMoveOn = NewUnit ? false : true;
                            NewUnit.GetComponent<BossAI>().MultiPositions.Add(NextTile);
                        }
                    }
                }
            }
        }

        Unit = NewUnit;
        Occupied = NewUnit? true : false;
        CanMoveOn = NewUnit ? false : true;
    }

    public void Hide(bool IgnoreBoss = false)
    {
        if (UnitManager.Instance.Boss && !IgnoreBoss)
        {
            if (UnitManager.Instance.Boss.PendingAttack)
            {
                if (UnitManager.Instance.Boss.AoELocations.Contains(this))
                {
                    GetComponent<MeshRenderer>().material = WeaponRangeMaterial;
                    return;
                }
            }
        }
        GetComponent<MeshRenderer>().material = OGMaterial;
    }

    internal void FindAdjacentTiles()
    {
        AdjacentTiles = new List<GameObject>();

        //Top
        if (GridPosition[0] + 1 < TileManager.Instance.Width && GridPosition[0] + 1 >= 0)
        {
            if (GridPosition[1] < TileManager.Instance.Height && GridPosition[1] >= 0)
            {
                if (TileManager.Instance.Grid[GridPosition[0] + 1, GridPosition[1]].GetComponent<Tile>().CanMoveOn 
                    || TileManager.Instance.Grid[GridPosition[0] + 1, GridPosition[1]].GetComponent<Tile>().Unit 
                    || TileManager.Instance.Grid[GridPosition[0] + 1, GridPosition[1]].GetComponent<Tile>().Special)
                {
                    AdjacentTiles.Add(TileManager.Instance.Grid[GridPosition[0] + 1, GridPosition[1]]);
                }
            }
        }

        //Left
        if (GridPosition[0] < TileManager.Instance.Width && GridPosition[0] >= 0)
        {
            if (GridPosition[1] - 1 < TileManager.Instance.Height && GridPosition[1] - 1 >= 0)
            {
                if (TileManager.Instance.Grid[GridPosition[0], GridPosition[1] - 1].GetComponent<Tile>().CanMoveOn
                    || TileManager.Instance.Grid[GridPosition[0], GridPosition[1] -1].GetComponent<Tile>().Unit
                    || TileManager.Instance.Grid[GridPosition[0], GridPosition[1] - 1].GetComponent<Tile>().Special)
                {
                    AdjacentTiles.Add(TileManager.Instance.Grid[GridPosition[0], GridPosition[1] - 1]);
                }
            }
        }

        //Right
        if (GridPosition[0] < TileManager.Instance.Width && GridPosition[0] >= 0)
        {
            if (GridPosition[1] + 1 < TileManager.Instance.Height && GridPosition[1] + 1 >= 0)
            {
                if (TileManager.Instance.Grid[GridPosition[0], GridPosition[1] + 1].GetComponent<Tile>().CanMoveOn
                    || TileManager.Instance.Grid[GridPosition[0], GridPosition[1] + 1].GetComponent<Tile>().Unit
                    || TileManager.Instance.Grid[GridPosition[0], GridPosition[1] + 1].GetComponent<Tile>().Special)
                {
                    AdjacentTiles.Add(TileManager.Instance.Grid[GridPosition[0], GridPosition[1] + 1]);
                }
            }
        }

        //Bottom
        if (GridPosition[0] - 1 < TileManager.Instance.Width && GridPosition[0] - 1 >= 0)
        {
            if (GridPosition[1] < TileManager.Instance.Height && GridPosition[1] >= 0)
            {
                if (TileManager.Instance.Grid[GridPosition[0] - 1, GridPosition[1]].GetComponent<Tile>().CanMoveOn
                    || TileManager.Instance.Grid[GridPosition[0] - 1, GridPosition[1]].GetComponent<Tile>().Unit
                    || TileManager.Instance.Grid[GridPosition[0] - 1, GridPosition[1]].GetComponent<Tile>().Special)
                {
                    AdjacentTiles.Add(TileManager.Instance.Grid[GridPosition[0] - 1, GridPosition[1]]);
                }
            }
        }

        if (Special)
        {
            Special.GetComponent<InteractOnGrid>().TileSetter(transform.GetComponentInParent<Tile>().AdjacentTiles);
        }

    }

    private void OnMouseEnter()
    {
        if(!UnitManager.Instance.SetupFinished || !Interact.Instance.VirtualCam.activeInHierarchy  || Options.Instance.OptionsMenuUI.activeInHierarchy 
            || Interact.Instance.CombatMenu.VictoryScreen.activeInHierarchy || Interact.Instance.CombatMenu.DefeatScreen.activeInHierarchy)
        {
            return;
        }

        Interact.Instance.CurrentTile = this;

        if (Unit)
        {
            if (Unit.EndTurn || OGMaterial == null)
            {
                Hide();
                return;
            }

            if (Unit.GetComponent<BossAI>())
            {
                if (Unit.GetComponent<BossAI>().PendingAttack)
                {
                    if (!Interact.Instance.CombatMenu.AttackMenuObject.gameObject.activeInHierarchy
                        && !Interact.Instance.CombatMenu.CombatMenuObject.gameObject.activeInHierarchy)
                    {
                        TileManager.Instance.Grid[GridPosition[0], GridPosition[1]].GetComponent<Tile>().Show(false, true);
                    }
                    return;
                }
            }
        }

        if (!Interact.Instance.CombatMenu.CombatMenuObject.gameObject.activeInHierarchy
            && !Interact.Instance.CombatMenu.AttackMenuObject.gameObject.activeInHierarchy 
            && Interact.Instance.SelectedUnit == null)
        {
            if (Unit)
            {
                if (!Unit.CompareTag("Enemy"))
                {
                    if (!Unit.MovedForTurn)
                    {
                        Unit.ShowAllInRangeTiles();
                    }
                    else
                    {
                        List<GameObject> Tiles = new List<GameObject>();
                        Tiles.Add(gameObject);
                        Unit.AttackableArea(Tiles);
                    }
                }
                else
                {
                    Unit.ShowAllInRangeTiles();
                }
            }
            else if (Special)
            {
                if (Special.GetComponent<MagicOrb>())
                {
                    if (Special.GetComponent<MagicOrb>().Active)
                    {
                        Special.GetComponent<MagicOrb>().ShowRange();
                    }
                }
            }
        }

        if (Special)
        {
            if (Special.HoverTip)
            {
                Special.HoverTip.Hovering = true;
            }
        }

        if (!Interact.Instance.CombatMenu.AttackMenuObject.gameObject.activeInHierarchy
            && !Interact.Instance.CombatMenu.CombatMenuObject.gameObject.activeInHierarchy)
        {
            Show(false, true);
        }
    }

    private void OnMouseExit()
    {
        if (!UnitManager.Instance.SetupFinished)
        {
            return;
        }

        if (Interact.Instance.CurrentTile == this)
        {
            Interact.Instance.CurrentTile = null;
        }

        if (Special)
        {
            if (Special.HoverTip)
            {
                Special.HoverTip.Hovering = false;
                HoverTooltipManager.OnMouseLoseFocus();

                if (Special.GetComponent<MagicOrb>())
                {
                    Special.GetComponent<MagicOrb>().HideRange();
                    WhichColour(Interact.Instance.SelectedUnit);
                    return;
                }
            }
        }

        if (!Interact.Instance.CombatMenu.AttackMenuObject.gameObject.activeInHierarchy 
            && !Interact.Instance.CombatMenu.CombatMenuObject.gameObject.activeInHierarchy
            && Interact.Instance.SelectedUnit == null)
        {
            if (Unit && !Interact.Instance.SelectedUnit)
            {
                Unit.HideAllChangedTiles();
            }
            else if (Unit)
            {
                if (Unit != Interact.Instance.SelectedUnit)
                {
                    Unit.HideAllChangedTiles();
                }
            }

            WhichColour(Interact.Instance.SelectedUnit);
            return;
        }

        WhichColour(Interact.Instance.SelectedUnit);
    }

    internal void WhichColour(UnitBase PassedUnit = null, bool IgnoreAoEBoss = false)
    {
        bool inRange;

        if(OGMaterial == null)
        {
            return;
        }

        if(UnitManager.Instance.Boss && !IgnoreAoEBoss && !Interact.Instance.SelectedUnit)
        {
            if(UnitManager.Instance.Boss.PendingAttack)
            {
                if(UnitManager.Instance.Boss.AoELocations.Contains(this))
                {
                    Show(true);
                    return;
                }
            }
        }

        if (Interact.Instance.SelectedUnit)
        {
            inRange = Interact.Instance.SelectedUnit.AttackTiles.Contains(this);
            if (inRange)
            {
                if (!Interact.Instance.SelectedUnit.MovedForTurn)
                {
                    Show(Interact.Instance.SelectedUnit.AttackTiles.Contains(this) && Interact.Instance.SelectedUnit.MoveableTiles.Contains(this) ? false : true, !inRange);
                    return;
                }
                else
                {
                    Show(true);
                    return;
                }
            }
        }
        else if(PassedUnit)
        {
            inRange = PassedUnit.AttackTiles.Contains(this);
            if (inRange)
            {
                if (!PassedUnit.MovedForTurn)
                {
                    Show(PassedUnit.AttackTiles.Contains(this) && PassedUnit.MoveableTiles.Contains(this) ? false : true, !inRange);
                    return;
                }
                else
                {
                    Show(true);
                    return;
                }
            }
        }

        Show(false, false, true);
    }
}
