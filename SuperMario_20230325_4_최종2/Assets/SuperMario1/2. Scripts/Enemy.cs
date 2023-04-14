using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour  //@8-4 ======================
{
  public float moveSpeed = 5f;        //몬스터의 이동 속도
  private int HP = 1;                  // 몬스터의 생명력 (모두 1로 설정)
  public int enemyType;               //#7-1 몬스터 종류(1 : 갈색, 2 : 거북)
  private float deadTimer = 0.0f;
  private float revivalTimer = 5.0f;         //#7-2 부활 타이머
  private bool turtleWeapon = false;     //#10-1 플레이어가 거북이 등껍질을 차서 무기가 된 상황 
  private int weaponDir;        //#10-1 무기가 나아가는 방향(-1 : 왼쪽. 1 : 오른쪽)

  public AudioClip[] deathClips;      // 몬스터가 죽었을때 플레이할 수 있는 오디오 클립 배열 
  public GameObject PointsUI; // 몬스터가 죽었을때 발생하는 100의 프리팹 //@ 몬스터 죽이는 점수
                            //#2-1 enemy1 : 100점, 거북이 : 200점
  
  private SpriteRenderer ren;         // SpriteRenderer 컴포넌트를 위한 레퍼런스
  private Transform frontCheck;       // 만약 무엇이든 몬스터 앞에 있다면 체크를 위헤 사용되는 gameobject의 position을 위한 Reference 
  public bool dead = false;          //몬스터가 죽었는지 아닌지를 위한 변수 //거북이 몬스터는 부활 가능
  private TopBar topBar;                // Score 스크립트를 위한 레퍼런스
  private Animator anim;            //#6-2 죽는 모습. Destroy도 애니메이터에서
  private Rigidbody2D rigidbody2d;  //@8-4 GetComponent로 따로 선언하기 위함
  private Collider2D collider2d;    //#8-3 파이어볼에 맞아서 죽을 때, 콜라이더 비활성화
  void Awake()
  {
      //레퍼런스들의 셋팅
      ren = transform.Find("body").GetComponent<SpriteRenderer>();
      //@ 위 선언 : SpriteRenderer 를 나타내는 변수를 선언할거야
      //@ Awake에서 정의 : 바로 body의 SpriteRenderer을 나타내는 변수야

      frontCheck = transform.Find("frontCheck").transform;  
      topBar = GameObject.Find("TopBar").GetComponent<TopBar>(); 
      //@ 이 Enemy 오브젝트가 아니라, GameObject안에 있는 Find함수를 사용하기 위해서 적은 듯.
      //@ Score클래스 형식을 가진 Score라는 이름을 가져와서 사용할 거다~
      anim = GetComponent<Animator>();
      rigidbody2d = GetComponent<Rigidbody2D>();  //@8-4 GetComponent로 따로 선언하기 위함
      collider2d = GetComponent<Collider2D>();    //#8-3
  } 

  void Update()
  {
    if(deadTimer>0)
      deadTimer += Time.deltaTime;
    if(deadTimer >revivalTimer - 2.0f)  //2초전부터 부활 준비
    {
      anim.SetBool("pushDead", false);
      anim.SetBool("revivalReady", true); //부활 준비 애니 재생
    }
    if(deadTimer > revivalTimer)  //부활 성공
      {
        HP++;
        dead = false;
        
        anim.SetBool("revivalReady", false);
        deadTimer = 0.0f;
      }
    
    if(turtleWeapon && (enemyType == 2))
    {
      //무기가 된 상황이면 계속 계속 옆으로 가도록
      rigidbody2d.velocity = new Vector2(transform.localScale.x * weaponDir * 30.0f, rigidbody2d.velocity.y);
    }

  }
  void FixedUpdate()
  {
    // enemy 앞에 모든 콜라이더들의 배열을 생성 (PlayerCtrl 스크립트의 Physics2D.Linecast()함수 참고 )
    Collider2D[] frontHits = Physics2D.OverlapPointAll(frontCheck.position, 1 << LayerMask.NameToLayer("Obstacle")); 

    //@ 바로 위 코드에서 가져온 콜라이더들 각각을 체크
    /*  @ 범위 기반 for문과 같음
    @ foreach문 (=범위기반 for문) : 배열에 있는 모든 것들을 한땀한땀 넣어서 for문 돌려주겠다
    모든 콜라이더(BoxCollider2D, CircleCollider2D)의 부모는 Collider2D구나
    */
    foreach(Collider2D c in frontHits)  
    {
      //만약 어떤 콜라이더의 테그가 Obstacle 이라면 ... 즉, 장애물(파이프)에 부딪힌 거라면
      if(c.tag == "Obstacle")
      {
        //다른 콜라이더들을 체크하는 것을 멈추고 몬스터를 뒤집어라
        Flip();
        break;
      }
    }

    if(!dead) //#6-2 죽은 상태에선 이동하면 안돼.
      //몬스터의 속도를 x축방향 moveSpeed으로 셋팅
      //localSclae을 이용하여 스케일의 크기만큼 속도가 증가하며, 방향 전환(Flip())시 localScale은 x축에 (-1)이 곱해져서 반대 방향으로 이동
      rigidbody2d.velocity = new Vector2(transform.localScale.x * -moveSpeed, rigidbody2d.velocity.y);  //#2-1 왼쪽부터 가도록 moveSpeed에 -붙임


    // 만약 몬스터의 HP가 0 또는 0 미만 이고 아직 살아잇다면 죽이자 ...
    if(HP <= 0 && !dead)
      //Death()함수 호출
      Death();
  }
  void OnCollisionEnter2D(Collision2D col)  //#7-1 거북이 부활 가능성 없이 완전히 죽는 경우
  {
    if(col.gameObject.tag == "Player" && (!col.gameObject.GetComponent<PlayerLife>().starState))  //#7-1  플레이어와 닿으면 && 스타상태가 아니라면
      {
        if(enemyType == 2 && dead)    //#7-2 거북이가 등껍질인 상태(HP==0)에서
        {
          turtleWeapon = true;      //이제 무기 상태
          anim.SetBool("pushDead", true); //다시 등껍질 상태로
          deadTimer = 0.0f; //부활 가능성 없애
          AudioSource.PlayClipAtPoint(deathClips[1], transform.position); //거북이 치는 소리
          
          //자식들도 모두 변경해줘야 충돌 처리 안 함
          gameObject.layer = 20;  //장애물과 충돌하지 않도록 //대신 바닥은 충돌 인식함. 
          foreach(Transform child in transform)
          {
            child.gameObject.layer = 20;
          }
          
          if(col.gameObject.transform.position.x > transform.position.x)  //만약 플레이어의 x좌표가 오른쪽에 있다면, 왼쪽으로 날아가도록
          {
            weaponDir = -1;
          }
          else //만약 플레이어의 x좌표가 왼쪽에 있다면, 오른쪽으로 날아가도록
          {
            weaponDir = 1;
          }
          
          Invoke("DestroyEnemy", 2.0f); //화면밖으로 충분히 나간 후에 없애기(나가면 사라져)
          return;
        }
        //groundCheck 때문에 Enemy에 적은 것. 일일이 col을 가져와서 확인해야돼..
        //#6-2 여기서 jump == false 조건을 안 달아주면, 점프하다가 스쳐지나가는 상황에서도 몬스터가 죽어버리고, 플레이어가 엄청 높이 점프하게 됨
        if((!col.gameObject.GetComponent<PlayerCtrl>().jump) && 
        (col.gameObject.GetComponent<PlayerLife>().groundCheck.position.y - transform.position.y> -1.0f))
        {
          Hurt();  //#다쳤을 때 효과
		  		// hurtVector 와 hurtForce에 의해서 곱해진 Vector의 방향으로 플레이어에게 힘을 가하자 
		  		col.gameObject.GetComponent<PlayerCtrl>().PushEnemy();  
				
        }
      }
    if(col.gameObject.layer == 20) //적이랑 부딪혔고, (그게 레이어가 20이라면 = 등껍질이라면)
    { 
    //    collider2d.isTrigger= true;   // 거북이 등껍질에 맞으면 그냥 통과해서 죽어~
        SpinDeath();
    }

  }


  public void Hurt()
  {
    //몬스터의 생명력을 1만큼 줄인다.
    HP--;
  }

  public void Death()
  {
    dead = true;        //dead를 true로 셋팅
    anim.SetBool("pushDead", true);  //밟혀 죽는 애니. 애니메이터가 끝나면 자동적으로 Destory

    if(enemyType == 1)
    {
      gameObject.layer = 15; //#6-2 DeadEnemies로 변경해서 플레이어와 충돌 잃어나지 않도록
      foreach(Transform child in transform)
      {
        child.gameObject.layer = 15;
      }
    }  
    if(enemyType == 2)
      deadTimer += Time.deltaTime;  //증가하기 시작
      
    

//100 포인트의 스코어 증가
    topBar.score += 100; //Score 클래스 (객체인 score) 변수 score에 +100
//오디오
    //deathClips 배열로부터 랜덤하게 audioclip을 플레이 하자
    AudioSource.PlayClipAtPoint(deathClips[0], transform.position);
//점수
    //몬스터 바로 위에 벡터를 생성  //@ 죽은 몬스터 바로 위에 점수가 뜨도록
    Vector3 scorePos;
    scorePos = transform.position;
    scorePos.y += 1.5f;         //@ 위치를 바로 바꾸는 게 불가능하다고 했으니까 추가 변수를 이용
    //이 벡터지점에서 100포인트 트리팹을 인스턴스로 만들자
    Instantiate(PointsUI, scorePos, Quaternion.identity); 
      //@ 프리팹을 추가하겠다. 이 프리팹을. 이 위치에. 이런 회전값을 가지도록.
      //@Quaternion : 기본 회전 상태 의미

  }

  public void SpinDeath()
  {
    if(!dead)
    {
      dead = true;
      //콜라이더 아예 없애
      collider2d.enabled = false;

      anim.SetTrigger("spinDead");
  //스코어 증가
      topBar.score += 100;
  //오디오
      AudioSource.PlayClipAtPoint(deathClips[0], transform.position);
  //점수 프리팹
      Vector3 scorePos;
      scorePos = transform.position;
      scorePos.y += 1.5f;         //@ 위치를 바로 바꾸는 게 불가능하다고 했으니까 추가 변수를 이용
      //이 벡터지점에서 100포인트 트리팹을 인스턴스로 만들자
      Instantiate(PointsUI, scorePos, Quaternion.identity); 
    }
    
  }

  public void StarDeath()
  {
    if(!dead)
    {
      dead = true;
      collider2d.enabled = false;
      anim.SetTrigger("spinDead");
//스코어 증가
      topBar.score += 100;
//오디오
      AudioSource.PlayClipAtPoint(deathClips[0], transform.position);
//점수 프리팹
      Vector3 scorePos;
      scorePos = transform.position;
      scorePos.y += 1.5f;         //@ 위치를 바로 바꾸는 게 불가능하다고 했으니까 추가 변수를 이용
      //이 벡터지점에서 100포인트 트리팹을 인스턴스로 만들자
      Instantiate(PointsUI, scorePos, Quaternion.identity); 

    }
  }

  public void Flip()
  {
    // -1을 Transform 컴포넌트에 요소 localScale(벡터)의 x축에 곱하자.
    //@ 위치를 바로 바꾸는 게 불가능하다고 했으니까 추가 변수를 이용
    Vector3 enemyScale = transform.localScale;
    enemyScale.x *= -1;
    transform.localScale = enemyScale;
  }
  void DestroyEnemy()
  { 
    turtleWeapon = false;
    weaponDir = 0;
    Destroy(gameObject);
  }


}
