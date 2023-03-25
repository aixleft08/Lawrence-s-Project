using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public int groundHitCount;
    public static GroundCheck Instance;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (groundHitCount >= 2 && Gameplay.Instance.gameState != Gameplay.GameState.SERVING)
        {
            if (Gameplay.Instance.currentlyHitting == Gameplay.Turns.OPPONENT)
            {
                Gameplay.Instance.playerScore++;
                groundHitCount = 0;
                Gameplay.Instance.gameState = Gameplay.GameState.SERVING;
                Gameplay.Instance.hasServedFirstTime = false;
                return;
            }
            else
            {
                Gameplay.Instance.oppScore++;
                groundHitCount = 0;
                Gameplay.Instance.gameState = Gameplay.GameState.SERVING;
                Gameplay.Instance.hasServedFirstTime = false;
                return;
            }

        }
    }

    void OnCollisionEnter(Collision other)
    {
        // if(other.transform.tag == "Ball" && Gameplay.Instance.gameState == Gameplay.GameState.RALLY && 
        // !WallHit.Instance.hasHitWall)
        // {
        //     if(Gameplay.Instance.currentlyHitting == Gameplay.Turns.OPPONENT)
        //     {
        //         Gameplay.Instance.playerScore++;
        //     } else
        //     {
        //         Gameplay.Instance.oppScore++;
        //     }
        //     Gameplay.Instance.gameState = Gameplay.GameState.SERVING;
        // }

        if (other.transform.tag == "Ball" && Gameplay.Instance.gameState == Gameplay.GameState.RALLY &&
        WallHit.Instance.hasHitWall)
        {
            groundHitCount++;
        }
    }
}
