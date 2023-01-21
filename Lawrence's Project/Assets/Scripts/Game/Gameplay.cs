using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameplay : MonoBehaviour
{
    public GameState gameState;
    public Turns currentTurn;

    [Space(20)]
    public Transform player;
    public Transform opponent;

    [Space(20)]
    public ServiceArea leftServiceArea;
    public ServiceArea rightServiceArea;
    
    public enum Turns { PLAYER, OPPONENT };
    public enum GameState { SERVING, RALLY, IDLE };

    void Update()
    {
        if(gameState == GameState.SERVING) GotoServiceArea();
        else if(gameState == GameState.RALLY) 
        {
            AllowPlayerMovement(true);
            opponent.GetComponent<Opponent>().allowMove = true;
        } else 
        {
            AllowPlayerMovement(false);
            opponent.GetComponent<Opponent>().allowMove = false;
        }
    }

    void GotoServiceArea()
    {
        opponent.GetComponent<Opponent>().allowMove = false;
        opponent.transform.position = new Vector3(leftServiceArea.transform.position.x, opponent.transform.position.y, leftServiceArea.transform.position.z);

        AllowPlayerMovement(false);
        player.transform.position = new Vector3(rightServiceArea.transform.position.x, player.transform.position.y, rightServiceArea.transform.position.z);
    }

    void AllowPlayerMovement(bool active)
    {
        FirstPersonController.Instance.allowMove = active;
        FirstPersonController.Instance.GetComponent<CharacterController>().enabled = active;
    }

}
