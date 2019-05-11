using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    GameObject player;

    public readonly float turnTick = 0.5f;

    bool finishLoopTurn = false;

    private void Awake()
    {
        player = GameObject.Find("Player");
    }

    private void Start()
    {
        LoopTurn();
    }

    async void LoopTurn()
    {
        await Task.Delay(100);

        while (! finishLoopTurn)
        {
            await player.GetComponent<PlayerMovement>().ProcessTurn();

            await Task.Delay((int)(turnTick * 1000));
        }
    }
}
