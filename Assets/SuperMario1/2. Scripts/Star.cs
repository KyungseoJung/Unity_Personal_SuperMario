using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
  public float moveSpeed = 1.0f;        //버섯 아이템의 이동 속도
  private bool eaten = false;          //버섯 아이템이 먹혔는지(true), 아닌지(false) 확인하는 변수
  public float jump=300.0f;                  //나타나자마자 점프하는 크기
  private Transform frontCheck;       // 만약 무엇이든 버섯 앞에 있다면 체크를 위헤 사용되는 gameobject의 position을 위한 Reference 
  private TopBar topBar;                // Score 스크립트를 위한 레퍼런스
  public GameObject PointsUI;         //1000포인트 애니메이터
  private Rigidbody2D rigidbody2d;  //@8-4 GetComponent로 따로 선언하기 위함
  private PlayerLevel playerLevel;  //#5-1 플레이어 level 증가시켜주기 위함


  void Awake()
  {
      //레퍼런스들의 셋팅
      
      frontCheck = transform.Find("frontCheck").transform;  
      topBar = GameObject.Find("TopBar").GetComponent<TopBar>(); 
      rigidbody2d = GetComponent<Rigidbody2D>();  //@8-4 GetComponent로 따로 선언하기 위함
      playerLevel = GameObject.Find("Player").GetComponent<PlayerLevel>();  //#5-1
  } 

  void Start()
  {
    rigidbody2d.AddForce(Vector2.up * jump);  //#8-3 처음 등장하자마자 점프하고 나타남
  }

  void FixedUpdate()    
  {
    //#4-2 이동 관련 ====================================
    //#4-2 만약 ItemBlock이 부숴졌다면, 그 위치에 스타 아이템이 등장하도록 - 이 코드는 ItemBlock에서 구현

    // mushroom 앞에 모든 콜라이더들의 배열을 생성 (PlayerCtrl 스크립트의 Physics2D.Linecast()함수 참고 )
    Collider2D[] frontHits = Physics2D.OverlapPointAll(frontCheck.position, 1 << LayerMask.NameToLayer("Obstacle")); 
    //#2-1 frontCheck에서 Layer가 Ground인 것 사이의 충돌을 모두 배열에 넣는 거야(장애물의 Layer를 Ground로 넣어버림. 마리오가 점프할 수 있도록)

    //@ 바로 위 코드에서 가져온 콜라이더들 각각을 체크
    foreach(Collider2D c in frontHits)  
    {
      //장애물(파이프)에 부딪힌 거라면  스타 뒤집어라
      if(c.gameObject.tag == "Obstacle")
      {
        Flip();
        break;
      }
    }
    //스타 아이템 이동
    //localSclae을 이용하여 스케일의 크기만큼 속도가 증가하며, 방향 전환(Flip())시 localScale은 x축에 (-1)이 곱해져서 반대 방향으로 이동
    rigidbody2d.velocity = new Vector2(transform.localScale.x * moveSpeed, rigidbody2d.velocity.y);  //#2-1 왼쪽부터 가도록 moveSpeed에 -붙임
  }

  void OnCollisionEnter2D(Collision2D col)
  {
    if(col.gameObject.tag == "Player")  //플레이어에 닿으면 = 플레이어가 먹으면
    {
      Eaten(gameObject);  //현재 오브젝트(스타 아이템) 먹힘 처리
      col.gameObject.GetComponent<PlayerLife>().starState = true; // 무적상태
        //깜빡이는 효과 ON
    }  
    
    //땅 또는 장애물에 닿으면 star 점프하도록 \
    //콜라이더 재질 처리를 Bounciness 높게 처리함 - 그래도 점프처리는 필요. 언젠가 기어다닐 수도 있으니까
    if((col.gameObject.tag == "Ground") ||  (col.gameObject.tag == "Obstacle"))
        rigidbody2d.AddForce(Vector2.up * jump);  
  }

  public void Flip()    //이동 방향 변환
  {
    // -1을 Transform 컴포넌트에 요소 localScale(벡터)의 x축에 곱하자.
    //@ 위치를 바로 바꾸는 게 불가능하다고 했으니까 추가 변수를 이용
    Vector3 enemyScale = transform.localScale;
    enemyScale.x *= -1;
    transform.localScale = enemyScale;
  }

  public void Eaten(GameObject eatenStar) //#4-2 플레이어와 부딪힌 스타 아이템을 가져와서 Destroy 할 거임
  {
    if(!eaten)      // 아직 먹지 않은 상태였다면, Destroy. 함으로써 정확히 1번만 Destroy 하도록
        Destroy(eatenStar);

    //스타 아이템 먹혔다.
    eaten = true;

    //1000 포인트의 스코어 증가
    topBar.score += 1000; //Score 클래스 (객체인 score) 변수 score에 +100    
    PlayPointsUIAnimation();
    //레벨업 안 해
    //#8-3 대신 무적 상태(n초동안)
  }
  void PlayPointsUIAnimation()
  {
    Instantiate(PointsUI, transform.position, Quaternion.identity);
  }

}
