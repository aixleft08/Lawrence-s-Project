using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Gameplay : MonoBehaviour
{
    public GameObject menuUI;
    public GameObject gameplayUI;
    public GameObject menuCamera;
    public GameObject tpCamera;
    public GameState gameState;
    public Turns lastServedTurn;
    public Turns currentlyHitting;
    public bool winLast;
    public int countdownStart;
    public int playerScore;
    public int oppScore;
    public bool mouseMode;

    [Space(20)]
    public Transform player;
    public Transform opponent;
    public TMP_Text turnText;
    public TMP_Text countdownText;
    public Transform ball;
    public TMP_Text playerScoreText;
    public TMP_Text oppScoreText;

    [Space(20)]
    public ServiceArea leftServiceArea;
    public ServiceArea rightServiceArea;
    
    public enum Turns { PLAYER, OPPONENT };
    public enum GameState { SERVING, RALLY, IDLE };

    bool left;
    bool serving;
    int startingCountdown;
    
    public bool hasServedFirstTime;
    [HideInInspector] public bool ballHasBeenHit;

    public static Gameplay Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        tpCamera.SetActive(false);
        menuCamera.SetActive(true);
        gameplayUI.SetActive(false);
        menuUI.SetActive(true);
        player.gameObject.SetActive(false);
        opponent.gameObject.SetActive(false);
        ball.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        gameplayUI.SetActive(true);
        menuUI.SetActive(false);
        player.gameObject.SetActive(true);
        opponent.gameObject.SetActive(true);
        ball.gameObject.SetActive(true);
        tpCamera.SetActive(true);
        menuCamera.SetActive(false);

        gameState = GameState.SERVING;
        left = true;
        startingCountdown = countdownStart;
    }

    IEnumerator StartCountdown()
    {
        currentlyHitting = lastServedTurn;
        countdownText.text = countdownStart + "";
        yield return new WaitForSeconds(1);
        countdownStart -= 1;

        if(countdownStart >= 0)
            StartCoroutine(StartCountdown());
        else
        {
            BallHit.Instance.canHit = true;
            ball.GetComponent<Rigidbody>().isKinematic = false;
            countdownText.text = "";
        }
    }

    void Update()
    {
        if(gameState == GameState.SERVING)
        {
            if(!serving)
            {
                serving = true;
                
                SoundManager.Instance.PlayCheer();
                countdownStart = startingCountdown;
                currentlyHitting = lastServedTurn;
                BallHit.Instance.canHit = false;
                ball.GetComponent<Rigidbody>().isKinematic = true;

                GotoServiceArea();
            }
        }
        else if(gameState == GameState.RALLY) 
        {
            //AllowPlayerMovement(true);
            opponent.GetComponent<Opponent>().allowMove = true;
            serving = false;
        } else 
        {
            //AllowPlayerMovement(false);
            opponent.GetComponent<Opponent>().allowMove = false;
            serving = false;
        }

        turnText.text = "Turn: " + currentlyHitting;

        playerScoreText.text = "Player: " + playerScore;
        oppScoreText.text = "Opponent: " + oppScore;
    }

    void GotoServiceArea()
    {
        StartCoroutine(StartCountdown());
        hasServedFirstTime = false;
        
        if(winLast)
        {
            left = !left;
        } else SwitchTurns();

        if(left)
        {
            player.transform.position = new Vector3(leftServiceArea.transform.position.x, player.transform.position.y, leftServiceArea.transform.position.z);

            opponent.GetComponent<Opponent>().allowMove = false;
            opponent.transform.position = new Vector3(rightServiceArea.transform.position.x, opponent.transform.position.y, rightServiceArea.transform.position.z);
        } else
        {
            player.transform.position = new Vector3(rightServiceArea.transform.position.x, player.transform.position.y, rightServiceArea.transform.position.z);

            opponent.GetComponent<Opponent>().allowMove = false;
            opponent.transform.position = new Vector3(leftServiceArea.transform.position.x, opponent.transform.position.y, leftServiceArea.transform.position.z);
        }
        
        if(left)
        {
            if(lastServedTurn == Turns.PLAYER)
                ball.position = leftServiceArea.transform.GetChild(0).position;
            else
                ball.position = rightServiceArea.transform.GetChild(0).position;
        } 
        else
        {
            if(lastServedTurn == Turns.PLAYER)
                ball.position = rightServiceArea.transform.GetChild(0).position;
            else
                ball.position = leftServiceArea.transform.GetChild(0).position;
        }

        //AllowPlayerMovement(false);
    }

    public void SwitchTurns()
    {
        if(lastServedTurn == Turns.PLAYER)
        {
            lastServedTurn = Turns.OPPONENT;
        }
        else
        {
            lastServedTurn = Turns.PLAYER;
        }
        currentlyHitting = lastServedTurn;

    }

    public void AllowPlayerMovement(bool active)
    {
        FirstPersonController.Instance.allowMove = active;
        FirstPersonController.Instance.GetComponent<CharacterController>().enabled = active;
    }
    
}
