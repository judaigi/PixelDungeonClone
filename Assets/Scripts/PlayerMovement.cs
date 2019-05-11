using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    GameObject gameManager;
    GameObject gridObject;

    Vector3Int? targetCell = null;

    TaskCompletionSource<bool> tcs = null;


    void Awake()
    {
        gameManager = GameObject.Find("GameManager");
        gridObject = GameObject.Find("Grid");
    }

    public async Task ProcessTurn()
    {
        if (targetCell.HasValue)
        {
            print("TargetCell Exist: " + targetCell.Value + ", " + Time.time);
            targetCell = null;
        }
        else
        {
            tcs = new TaskCompletionSource<bool>();
            await tcs.Task;
            tcs = null;
            print("TargetCell New: " + targetCell.Value + ", " + Time.time);
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

    // Move to target position by one step.
    void MoveToTarget()
    {

    }
}
