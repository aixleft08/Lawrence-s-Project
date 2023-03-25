using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallHit : MonoBehaviour
{
    public bool hasHitWall;

    public static WallHit Instance;

    void Awake()
    {
        Instance = this;
    }
    
    void OnCollisionEnter(Collision other)
    {
        if(other.transform.tag == "Ball" && Gameplay.Instance.ballHasBeenHit)
        {
            if(!Gameplay.Instance.hasServedFirstTime)
            {
                Gameplay.Instance.hasServedFirstTime = true;
                if(Gameplay.Instance.ball.position.y >= 4.7f)
                {
                    Gameplay.Instance.SwitchTurns();
                    Gameplay.Instance.ballHasBeenHit = false;
                    hasHitWall = true;
                    GroundCheck.Instance.groundHitCount = 0;
                } else
                {
                    if(Gameplay.Instance.currentlyHitting == Gameplay.Turns.OPPONENT)
                    {
                        Gameplay.Instance.playerScore++;
                        Gameplay.Instance.gameState = Gameplay.GameState.SERVING;
                        return;
                    } else
                    {
                        Gameplay.Instance.oppScore++;
                        Gameplay.Instance.gameState = Gameplay.GameState.SERVING;
                        return;
                    }
                }
            } else
            {
                Gameplay.Instance.SwitchTurns();
                    Gameplay.Instance.ballHasBeenHit = false;
                    hasHitWall = true;
                    GroundCheck.Instance.groundHitCount = 0;
            }

        }
    }
}
