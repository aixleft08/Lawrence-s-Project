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
        if(groundHitCount >= 2) Gameplay.Instance.gameState = Gameplay.GameState.SERVING;
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.transform.tag == "Ball" && Gameplay.Instance.gameState == Gameplay.GameState.RALLY && 
        WallHit.Instance.hasHitWall)
        {
            groundHitCount++;
        }
    }
}
