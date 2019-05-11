using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    GameObject gameManager;
    GameObject gridObject;
    GameObject blocking;

    Vector3Int? targetCell = null;

    TaskCompletionSource<bool> tcs = null;


    void Awake()
    {
        gameManager = GameObject.Find("GameManager");
        gridObject = GameObject.Find("Grid");
        blocking = GameObject.Find("Blocking");
    }

    public async Task ProcessTurn()
    {
        if (targetCell.HasValue)
        {
            print("TargetCell Exist: " + targetCell.Value + ", " + Time.time);
            MoveToTarget();
        }
        else
        {
            tcs = new TaskCompletionSource<bool>();
            await tcs.Task;
            tcs = null;
            print("TargetCell New: " + targetCell + ", " + Time.time);
        }

    }

    public void SetTargetPosition(Vector2 position)
    {
        print("SetTargetPosition: " + position);
        Vector3Int cell = gridObject.GetComponent<Grid>().WorldToCell(position);
        targetCell = cell;

        MoveToTarget();
        
        if (tcs != null)
            tcs.SetResult(true);
    }

    int GetUnit(int value)
    {
        return value > 0 ? 1 : (value < 0 ? -1 : 0);
    }

    List<Vector3Int> GetMoveCandidates()
    {
        var candidates = new List<Vector3Int>();

        if (targetCell.HasValue)
        {
            Vector3Int cell = gridObject.GetComponent<Grid>().WorldToCell(transform.position);
            var diffCell = targetCell.Value - cell;

            var diffUnit = new Vector3Int(GetUnit(diffCell.x), GetUnit(diffCell.y), 0);

            if (diffUnit.x != 0 && diffUnit.y != 0)
                candidates.Add(cell + diffUnit);
            if (diffUnit.x != 0)
                candidates.Add(cell + new Vector3Int(diffUnit.x, 0, 0));
            if (diffUnit.y != 0)
                candidates.Add(cell + new Vector3Int(0, diffUnit.y, 0));
        }

        return candidates;
    }

    Vector3? GetMovePosition()
    {
        var candidates = GetMoveCandidates();
        foreach (var candidate in candidates)
        {
            print("Candidate: " + candidate);
            Vector2 candidatePosition = gridObject.GetComponent<Grid>().GetCellCenterWorld(candidate);

            print("Candidate Position: " + candidatePosition);
            print("Candidate Position Overlap: " + blocking.GetComponent<CompositeCollider2D>().OverlapPoint(candidatePosition));
            print("Candidate Position Overlap 2: " + blocking.GetComponent<CompositeCollider2D>().OverlapPoint(gridObject.GetComponent<Grid>().CellToWorld(candidate)));
            if (! blocking.GetComponent<CompositeCollider2D>().OverlapPoint(candidatePosition))
                return candidatePosition;
        }
        return null;
    }

    // Move to target position by one step.
    void MoveToTarget()
    {
        Vector3? movePosition = GetMovePosition();

        if (movePosition.HasValue)
            transform.position = movePosition.Value;
        else
            targetCell = null;
    }
}
