using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour   //#9-3
{
    public bool goalPillar;
    public bool goalUnder;

    private Collider2D collider2d;  //각 Pillar 또는 Under의 콜라이더가 담기게 될 거임

    private LobbyManager lobbyManager;	//#9-3 씬 바꾸기 위한 참조

    void Awake()
    {
        collider2d = gameObject.GetComponent<BoxCollider2D>();
        lobbyManager = GameObject.FindGameObjectWithTag("LobbyManager").GetComponent<LobbyManager>();	//#9-3

    }
    void OnCollisionEnter2D(Collision2D col)
    {
        //#9-3 플레이어가 Goal 지점의 기둥에 닿하면
        if(goalPillar && (col.gameObject.tag == "Player"))  
        {
            col.gameObject.GetComponent<PlayerCtrl>().arrivalGoal = true;   //목표 지점 도착
            //밑으로 내려가도록 - PlayerCtrl에서 조정

            collider2d.enabled = false;
        }

        //#9-3 플레이어가 Goal 지점의 바닥에 닿으면
        if(goalUnder && (col.gameObject.tag == "Player"))
        {
            //바닥에 닿으면 이제 성으로 가야할 시간
            col.gameObject.GetComponent<PlayerCtrl>().goingToCastle = true;

            lobbyManager.gameClear = true;
        }
    }
}
