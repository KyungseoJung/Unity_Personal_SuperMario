using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;   //#12-1 추가

public class TopBar : MonoBehaviour //#12-1
{
    public int score;
    public int coin;
    public int world;
    public float leftTime;
    public int time;
    public bool timeGoingDown = true;   //#12-2

    public Text marioScore;
    public Text coinSum;
    public Text worldNum;
    public Text timeLeft;

    private LobbyManager lobbyManager;	//#13 scPlay1이 시작되면 타이머가 감소하기 시작하도록

    void Awake()
    {
        lobbyManager = GameObject.FindGameObjectWithTag("LobbyManager").GetComponent<LobbyManager>();	//#9-3
    }

    void Start()
    {
        world = 1;
        worldNum.text = "1 - " + world;
        leftTime = 400;
    }

    void Update()
    {   
        if(lobbyManager.startGameScene) //#13 게임 시작한 이후에 타이머 감소하도록/ 화면에 나타나도록
        {
            //Time.deltaTime : 0.009~0.02 정도의 숫자
            if((leftTime >0)&& (timeGoingDown))    //(leftTime >0) && (TimeGoingDown이 true이면) && timeGoingDown//#12-2 && timeGoingDown
                leftTime -= Time.deltaTime;

            /*
            찾아본 결과 : 자료형 변환은 Mathf(Mathf.Round)를 사용하는 것보다 더 빠르고, 메모리 사용량도 적다.
            */
            time = (int)leftTime;
            timeLeft.text = time.ToString("D3");       
        }


        
    
        marioScore.text = score.ToString("D5"); //00001 모양처럼 나오도록
        coinSum.text = "X " + coin.ToString("D2");     
 
    }
}
