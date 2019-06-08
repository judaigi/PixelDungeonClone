using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    GameObject gameManager;
    GameObject gridObject;
    GameObject blocking;

    new Rigidbody2D rigidbody;
    Animator animator;

    float speed = 10.0f;
    float moveTimeLimit = 1.0f;

    Vector3Int? targetCell = null;
    TaskCompletionSource<bool> tcs = null;
    Coroutine coroutineMove = null;


    void Awake()
    {
        gameManager = GameObject.Find("GameManager");
        gridObject = GameObject.Find("Grid");
        blocking = GameObject.Find("Blocking");
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        moveTimeLimit = 1.5f / speed; // 대각선 고려
    }

    public async Task ProcessTurn()
    {
        if (targetCell.HasValue)
        {
            print("TargetCell Exist: " + targetCell.Value + ", " + Time.time);
            MoveTowardTarget();
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

        MoveTowardTarget();
        
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

            TileBase tile = blocking.GetComponent<Tilemap>().GetTile(candidate);
            if (tile == null)
                return candidatePosition;
        }
        return null;
    }

    // Move to target position by one step.
    void MoveTowardTarget()
    {
        Vector3? movePosition = GetMovePosition();

        if (movePosition.HasValue)
        {
            if (coroutineMove != null)
            {
                StopCoroutine(coroutineMove);
                coroutineMove = null;
            }
            coroutineMove = StartCoroutine(SmoothMovement(movePosition.Value));
        }
        else
            targetCell = null;
    }

    //Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        print("SmoothMovement enter");
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.shortNameHash == Animator.StringToHash("Idle"))
            animator.SetTrigger("Run");

        float startTime = Time.time;
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        while ((sqrRemainingDistance > float.Epsilon) && ((Time.time - startTime) < moveTimeLimit))
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, end, speed * Time.deltaTime);
            transform.position = newPosition;
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            print("SmoothMovement: " + newPosition + ", " + sqrRemainingDistance + ", " + (Time.time - startTime));
            yield return null;
        }
        transform.position = end;

        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.shortNameHash == Animator.StringToHash("Run"))
            animator.SetTrigger("Idle");

        print("SmoothMovement exit");
        coroutineMove = null;
    }

}
