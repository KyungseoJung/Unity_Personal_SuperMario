using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour //@7-4 ===========================
{
	private Animator anim;   // Player 객체의 Animator component 를 위한 Reference(참조) 이다
    private Rigidbody2D rigidbody2d;    //@7-4 내가 추가한 GetComponent 정리용 변수
	private PlayerLevel playerLevel;	//#8-3 레벨에 따른 공격 가능 유무(ShootFire)

	// 인스펙터에 노출 안됨 
	[HideInInspector]
	public bool dirRight = true;			// 플레이어의 현재 바라보는 방향을 알기 위함 (true : 오른쪽)


	[HideInInspector]
	public bool jump = false;	   			// 플레이어가 현재 점프중인지 아닌지 알기 위함 //PlayerLife에서 쓰임
	public float jumpForce = 88f; 			//플레이어가 점프를 할때의 추가되는 힘의 양 
	public float minJump = 1600f;			//#5-4 최소 점프 힘
	public float jumpTimer;					//#5-4 점프중인 시간
	public float jumpTimeLimit = 0.3f;		//#5-4 최대 점프 시간
	public AudioClip jumpClip;  			//플레이어의 여럿 점프 사운드를 위한 오디오 클립 배열 
    public float trampleForce = 40f;   // 플레이어가 몬스터를 밟았을 때, 튕기듯이 위로 올라가도록
	
	private bool grounded = false;			// 플레이어가 땅에 있는지 아닌지 구별을 위한 변수
	
	private bool coinblockCrashed = false;		//#4-1 머리에 코인블록이 부딪혔는지(닿았으면 true)
	private bool normalblockPushed = false;		//#4-1 머리에 일반블록이 부딪혔는지(닿았으면 true)
	private bool itemblockCrashed = false;		//#4-2 머리에 아이템블록이 부딪혔는지(닿았으면 true)
	public Transform groundCheck;	 		//만약 플레이어가 땅에 있을때 position을 marking 할 곳
            //@ 선생님 파일엔 private라고 되어있음. 나는 자신 없어서 확실한 확인을 위해 public으로 선언했다~
	public Transform headCheck;				//#4-1 플레이어 머리에 뭐가 부딪히는지 체크하는 용도
	
	private float moveForce = 365f;			// 플레이어의 왼쪽 오른쪽 이동을 위한 추가되는 힘의 양	
	private float maxSpeed = 7f;			//#1-1 5f에서 3f로 변경	// 가장 빠르게 x 축 안에서 플레이어가 이동 할수있는 최고 스피드
	
	 /* #1-1 조롱 소리 빼기
	public float tauntProbability = 50f;	//플레이어가 적을 조롱하게 기회 제공을 위한 변수 
	public AudioClip[] taunts;				//플레이어가 적을 조롱 할때를 위한 오디오 클립 배열 
	private int tauntIndex;	  				//가장 최근에 플레이된(조롱) taunts 배열의 Index의 저장을 위한 변수 
	public float tauntDelay = 1f;           //조롱이 발생 할때 딜레이를 줘야만 한다. 안그러면 사운드가 중복 된다.
    */
	public Rigidbody2D fireball;				//#8-3 //# GameObject로 받을 수도 있지만, Rigidbody2D로 받는다. velocity 설정하기 편하도록
	public AudioClip fireClip;
	public float fireSpeed = 20.0f;
	public RectTransform stick;

	private float minX =-10.9f;		//#3-1 플레이어가 되돌아 갈 수 있는 최소 x위치. 더이상 뒤로 돌아가지 못하도록	
									//(-10.9f)는 Scene에서 왼쪽 낭떠러지 위치를 의미
	private bool startLine = false;	//#3-1 스타트 라인을 넘었는지 체크. startLine을 넘은

	public bool arrivalGoal = false;
	public bool goingToCastle = false;
	private Animator goalAnim;

	LobbyManager lobbyManager;	//#9-3 씬 바꾸기 위한 참조	
	TopBar topBar;


void Awake()
{
	Transform childTransform = transform.GetChild(0);	//@ 1칸 아래 자식 중, 1번째 하위 오브젝트의 Transform을 가져오겠다
    
	 // 레퍼런스(참조)들을 셋팅.
    groundCheck = childTransform.Find("groundCheck");	//@ 이것도 Player 오브젝트가 아닌, PlayerMove 오브젝트 하위에 groundCheck가 있으므로 하위 오브젝트를 참조하는 코드를 적용시킨다.
									//코드 변경 : transform.Find() -> childTransform.Find()
	headCheck = childTransform.Find("headCheck");		//#4-1 플레이어 머리에 뭐가 부딪히는지 확인
	anim = childTransform.GetComponent<Animator>();	//@ 지금 이 (PlayerCtrl이 속한)Player 게임오브젝트에는 애니메이터가 없지.
									//@ 그 하위에 PlayerMove 오브젝트에 있으므로 transform. 을 통해서 하위 애니메이터를 찾아줘야 해.
									//@ 코드 변경 : /*anim = GetComponent<Animator>(); */  -> Transform childTransform = transform.GetChild(0); 로 하위 오브젝트의 위치 가져오는 코드 추가
									//이렇게 추가하니까 Speed 변수가 없다는 에러는 사라짐 - 맞는 방법인가보다!

    //@ 아래는 내가 추가한 GetComponent 정리용 변수
    rigidbody2d = GetComponent<Rigidbody2D>();	//@ 문제 해결? : Bazooka 스크립트에서 참조해야 하는 변수니까, 그 이름을 맞춰줘야 하나? - Bazooka에서 가져오는 거 아님
				//@ 
	playerLevel = transform.root.GetComponent<PlayerLevel>();	//#8-3

	goalAnim = GameObject.FindGameObjectWithTag("Goal").GetComponent<Animator>();
	lobbyManager = GameObject.FindGameObjectWithTag("LobbyManager").GetComponent<LobbyManager>();	//#9-3  
	topBar = GameObject.Find("TopBar").GetComponent<TopBar>(); //5. 실제 점수 올라가도록

}


//@ 계속 에러 발생했던 이유 : 애니메이터, groundCheck를 가져오는데 하위 오브젝트에서 가져와야 하는 걸, 현재 오브젝트에서 가져오고 있었음.
	//@ Awake에서 하위 오브젝트에서 가져오는 코드를 작성해야 함. 
void Update()
{
	//#3-1 스타트라인을 넘은 순간. 부터 minX만큼 되돌아가지 못하는 거 적용되도록
	if(transform.position.x > -3.9)
		startLine = true;

	//#3-1 더이상 뒤로 가지 못하도록
	if(startLine && (transform.position.x > minX + 7.0f))	//(-3.9) - (-10.9) = 7.0f
		//스타트라인 넘었다면, 되돌아갈 수 있는 최소 위치(minX) 업데이트. 그리고 앞으로 나갈 때마다 또 업데이트
		minX = transform.position.x - 7.0f;	
	
	//# 레이어 구분
		// 플레이어 position 으로부터 groundcheck position 까지 linecast(두 점을 잇는 선을 그림 )할때 
		// 만약 충돌한 어떤객체가 Ground layer 라면 현재 플레이어는 땅에 있는거다.
	//#4-2 블록 위에 올라간 경우도 grounded로 업데이트 해주기(블록들의 Layer를 "Ground"로 체크할 수 있으면 좋겠지만, 이미 다른 거 구현하느라 Layer를 다르게 설정해버림)
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"))
			|| Physics2D.Linecast(transform.position, groundCheck.position, 1<< LayerMask.NameToLayer("Obstacle"))
			|| Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("NormalBlock"))
			|| Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("ItemBlock"))
			|| Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("CoinBlock")); ;  

	//#9-3 죽으면 이동 안되도록. 아래 코드들 모두 실행 안 됨.	
	//#9-3 Goal 지점에 도달하면 이동 안되도록. 아래 코드들 모두 실행 안 됨.
		if((playerLevel.level <= 0) || arrivalGoal)
		{
			grounded = false;
			topBar.timeGoingDown = false;
		}	
		//만약 점프 버튼을 눌렀를때 플레이어가 땅에 있었다면 플레이어는 점프 할 수있다.
		if(Input.GetButtonDown("Jump") && grounded)
		{
			jump = true;
			rigidbody2d.AddForce(Vector2.up * minJump);	//#5-4 최소한 이 높이 정도는 뛰도록. 딱 1번 실행되는 Update문에
			anim.SetTrigger("Jump");	// animator의 trigger(전환) parameter에 Jump를 셋팅
			AudioSource.PlayClipAtPoint(jumpClip, transform.position);
		}

	//#2-1 장애물 밟았는지 확인하는 코드는 FixedUpdate에


	if(goingToCastle)
	{
		anim.SetBool("Castle", true);	//goingToCastle이 true이면 밑에 else if문 타지 않도록
	//1번만 타도록. 어차피 애니메이터는 Goal 다음에 작동하는 거라 1번만 작동하긴 하는데, 그래도 이게 깔끔~
	}
	else if( arrivalGoal)	//성에 아직 갈 때가 아니라면 && Goal에 도착하면	
	{
		anim.SetTrigger("Goal");	
		rigidbody2d.AddForce(Vector2.down * 50f);

		goalAnim.SetTrigger("Goal");	//깃발 내려가는 애니 실행

		lobbyManager.goal = true;
	}
	//#9-3 (EndScene)다음 씬으로 넘어가도록 / 애니메이터 연결
	if((!lobbyManager.gameOver) && playerLevel.level <=0)	//아직 게임오버하기 전이고 && 플레이어 레벨이 0이라면
	{
		anim.SetTrigger("Death");	//Level1이었던 플레이어만 적용되도록
		lobbyManager.gameOver = true;
	}
}




//#1-1 조이스틱 부분 주석 처리
/*
public void ClickJump()	//@10-3 버튼 누르면, 점프하도록
{
	if(grounded)
		jump = true;
}
*/

//고정 시간마다 호출
void FixedUpdate()
{
//#9-3 죽으면 이동 안되도록. 아래 코드들 모두 실행 안 됨.	
//#9-3 Goal 지점에 도달하면 이동 안되도록. 아래 코드들 모두 실행 안 됨.
	if((playerLevel.level <= 0) || arrivalGoal)
	{
		return;
	}
	//else 처리를 해도 점프가 계속 탐..

//이동 제한================================================
	// 왼쪽 낭떠러지 가려고 하면 못가도록.(minX는 처음엔 -10.9) 
	//#3-1	스타트라인을 넘었고, 왔던 길을 되돌아가려 하면, 가지 못하도록. 0.1f 안 더해주면 아예 그 상태로 멈춰버림.
	if( transform.position.x < minX)	
		{
			Vector3 playerPos;
			playerPos = transform.position;
			playerPos.x += 0.2f;
			transform.position = playerPos;

			return;
		}

//달리기 ================================================
	    //Input클래스안에 다음 GetAxis() 함수 호출로 horizontal 입력을 캐치한다.
        float h = Input.GetAxis("Horizontal");  //@ h : 그냥 방향값     //@10-4 주석처리. 조이스틱으로 대체
//#1-1주석		float h = UltimateJoystick.GetHorizontalAxis("JKS");  //@10-4 조이스틱 X축 위치	

        // animator 컴포넌트에 parameter(매개변수)인 Speed에 horizontal(수평) 입력값의 절대값(Mathf.Abs())을 셋팅한다.
        anim.SetFloat("Speed", Mathf.Abs(h));   //@ 속도를 절대값으로 받아서 넣어	//@계속 에러-일단 주석

		//@ 달리기 가속도 =========================================
		//만약 플레이어의 바라보는 방향이 바뀌거나 혹은 maxSpeed에 아직 도달하지 않을때 ( h(-1.0f~1.0f)는 velocity.x를 다르게 표시한다)
		if(h * rigidbody2d.velocity.x < maxSpeed)	//# h가 음수이면 rigidbody2d.velocity.x도 음수. 마찬가지로 h가 양수이면 양수
			//플레이어 객체에 힘을 가한다.
			rigidbody2d.AddForce(Vector2.right * h * moveForce);	//오른쪽방향(1,0) * 방향 * 힘

		// 만약에 플레이어의 수평 속도가 maxSpeed 보다 크면 
		if(Mathf.Abs(rigidbody2d.velocity.x) > maxSpeed)
			{	
				//플레이어의 velocity(속도)를 x축방향으로 maxSpeed 로 셋팅해줘라 또한 기존 rigidbody2D.velocity.y 도 셋팅 해 줘야 한다.
				// Mathf.Sign() 는 매개변수를 참조해서 1 또는 -1(float)을 반환  
				rigidbody2d.velocity = new Vector2(Mathf.Sign(rigidbody2d.velocity.x) * maxSpeed, rigidbody2d.velocity.y);
			}

//#1-1 급정지 자세 ==================================
		//방향이 바뀌기 전에 딱 1번 실행되므로 아래 코드도 딱 1번 실행됨.
		if((rigidbody2d.velocity.x < -maxSpeed + 0.7f) && (!dirRight) && (h>0) )	//왼쪽으로 달리고 있다면 && 왼쪽 바라보고 있다면 && 오른쪽 방향키 누른다면
			anim.SetTrigger("ChangeDir");	//애니메이터의 Parameters 중 ChangeDir을 켠다.
		else if((rigidbody2d.velocity.x > maxSpeed - 0.7f) && (dirRight) && (h<0) )	//오른쪽으로 달리고 있다면 && 오른쪽 바라보고 있다면 && 왼쪽 방향키 누른다면
			anim.SetTrigger("ChangeDir");	//애니메이터의 Parameters 중 ChangeDir을 켠다.

		// 만약 플레이어가 왼쪽을 바로볼때(!dirRight) 플레이어를 오른쪽으로 이동하게(h>0) 입력했다면 
		if(h > 0 && !dirRight)
			// 플레이어를 뒤집어라
			Flip();
		// 그렇지 않고 만약 플레이어가 오른쪽을 바로볼때 플레이어를 왼쪽으로 이동하게 입력했다면 
		else if(h < 0 && dirRight)
			// 플레이어를 뒤집어라
			Flip();

//점프(가속도) ================================================
		if(jump)	// 만약 플레이어가 점프를 한다면
		{
			rigidbody2d.AddForce(Vector2.up * jumpForce);

			jumpTimer += Time.deltaTime;		//특정 시간만큼만 점프 가능하도록.

			//#5-4
			//오래 누르는만큼 더 높이 올라가도록	//Time.deltaTime은 컴퓨터 성능과 상관없이 동등한 조건으로 되도록
			if(!Input.GetButton("Jump") || jumpTimer > jumpTimeLimit)	//스페이스바를 떼면 점프 멈추고 추락.
			{
				jump = false;
				jumpTimer =0.0f;
			}
		}
//불 쏘기 ================================================
			//#8-3 Fire 발사
		if((playerLevel.level == 3) && (Input.GetButtonDown("Fire1")))
		{
			ShootFire();
		}
}

void OnCollisionEnter2D (Collision2D col)
{
	//#4-1 Normal 블록 머리로 밀기/깨기
	normalblockPushed = Physics2D.Linecast(transform.position, headCheck.position, 1<<LayerMask.NameToLayer("NormalBlock"));
	if(normalblockPushed)
	{
		NormalBlock normalBlock = col.gameObject.GetComponent<NormalBlock>();	//부딪힌 오브젝트에서 NormalBlock스크립트를 가져온다.

		if(normalBlock != null)	//null에러 방지 if문
			normalBlock.TakeDamage();	//밀려야 하는 상태로 업데이트
	}	
		
	//#4-2 Item 블록 머리로 밀기
	itemblockCrashed = Physics2D.Linecast(transform.position, headCheck.position, 1<<LayerMask.NameToLayer("ItemBlock"));
	if(itemblockCrashed)
	{
		ItemBlock itemBlock = col.gameObject.GetComponent<ItemBlock>();
		if(itemBlock != null)
			itemBlock.TakeDamage();	//부숴져야 하는 상태로 업데이트
	}

		//#4-1 코인 블록 머리로 깨기
	coinblockCrashed = Physics2D.Linecast(transform.position, headCheck.position, 1<<LayerMask.NameToLayer("CoinBlock"));
	if(coinblockCrashed)
	{
		CoinBlock coinBlock = col.gameObject.GetComponent<CoinBlock>();

		if(coinBlock != null)
			coinBlock.TakeDamage();	//부숴져야 하는 상태로 업데이트
	}

	//치명적! 가끔씩 각 오브젝트의 스크립트를 찾지 못하는 문제 발생함. if문(블록!=null) 걸어주는 걸로 해결

}

// 케릭터의 현재 방향을 바꿔주는 함수 
void Flip()
{
    //플레이어의 바라보는 방향을 바꾸자 
    dirRight = !dirRight;

    //플레이어의 local scale x에 -1을 곱하자
    Vector3 theScale = transform.localScale;
    theScale.x *= -1;
    transform.localScale = theScale;

    //@7-4 그냥 바로 -1을 곱해주지 않는 이유? 
    /* 챗 GPT : 
    원래 값 자체를 변경하면, 이전의 값을 유지하기 어려움.
    더 안전하고 간단한 방법을 위해 theScale 변수를 이용해서 -1 곱한 값을 넣어준 것
    */
}

public void PushEnemy()	//적 밟고 나서 약간 위로 점프하게 됨
{
	// 위쪽 힘과 함께 적에서부터 플레이어까지를 담을 수 있는 vector의 생성 
	// 미는 힘 = 살짝 점프(0 1 0 * 100f)
	Vector3 trampleVector = Vector3.up * 30f;

	// hurtVector 와 hurtForce에 의해서 곱해진 Vector의 방향으로 플레이어에게 힘을 가하자 
	GetComponent<Rigidbody2D>().AddForce(trampleVector * trampleForce);  
	
}
void ShootFire()	//#8-3 불 쏘기
{
	anim.SetTrigger("Shoot");
	AudioSource.PlayClipAtPoint(fireClip, transform.position);

	if(dirRight)	//만약 오른쪽을 보고 있다면
	{
		//파이어볼생성, 속도 주기
		Rigidbody2D fireInstance = Instantiate(fireball, transform.position, Quaternion.Euler(new Vector3(0,0,0))) as Rigidbody2D;
		fireInstance.velocity = new Vector2(fireSpeed, 0);
	}
	else
	{
		Rigidbody2D fireInstance = Instantiate(fireball, transform.position, Quaternion.Euler(new Vector3(0,0,0))) as Rigidbody2D;
		fireInstance.velocity = new Vector2(-fireSpeed, 0);
	}
}


}
