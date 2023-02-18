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
            Gameplay.Instance.SwitchTurns();
            Gameplay.Instance.ballHasBeenHit = false;
            hasHitWall = true;
        }
    }
}
