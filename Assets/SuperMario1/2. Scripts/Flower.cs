using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour
{
  private bool eaten = false;          //버섯 아이템이 먹혔는지(true), 아닌지(false) 확인하는 변수
  public GameObject PointsUI;         // 100포인트 또는 1UP 애니메이터

  private TopBar topBar;                // Score 스크립트를 위한 레퍼런스

  private PlayerLevel playerLevel;  //#5-1 플레이어 level 증가시켜주기 위함

  void Awake()
  {
      //레퍼런스들의 셋팅
      topBar = GameObject.Find("TopBar").GetComponent<TopBar>(); 

      playerLevel = GameObject.FindGameObjectWithTag("AllPlayer").GetComponent<PlayerLevel>();  //#5-1
    
  } 


  void OnCollisionEnter2D(Collision2D col)
  {
    if(col.gameObject.tag == "Player")  //플레이어에 닿으면 = 플레이어가 먹으면
        Eaten(gameObject);  //현재 오브젝트(버섯 아이템) 먹힘 처리
  }

  public void Eaten(GameObject eatenMushroom) //#4-2 플레이어와 부딪힌 버섯 아이템을 가져와서 Destroy 할 거임
  {
    if(!eaten)      // 아직 먹지 않은 상태였다면, Destroy. 함으로써 정확히 1번만 Destroy 하도록
        Destroy(eatenMushroom);

    //꽃 아이템 먹혔다.
    eaten = true;

    //1000 포인트의 스코어 증가
    topBar.score += 1000; //Score 클래스 (객체인 score) 변수 score에 +100
    PlayPointsUIAnimation();  //버섯 종류에 따라 100point 애니 또는 1UP 애니


    //#5-1 플레이어 몸집 커지기
    if(playerLevel.level <3)
    {
        playerLevel.LevelUp();
    }

  }

  void PlayPointsUIAnimation()
  {
    Instantiate(PointsUI, transform.position, Quaternion.identity);
  }
}
