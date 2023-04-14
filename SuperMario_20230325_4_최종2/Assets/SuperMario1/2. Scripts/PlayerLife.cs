using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLife : MonoBehaviour //@7-5
{
    private Animator anim;  // Player객체의 포함된 Animator 컴포넌트를 위한 Reference

    private PlayerCtrl playerCtrl;  // PlayerCtrl 컴포넌트를 위한 Reference
	private PlayerLevel playerLevel;	//#5-1
	public Transform groundCheck;
    //private SpriteRenderer lifeBar; // life Bar의 SpriteRenderer 컴포넌트를 위한 Reference

    public AudioClip[] ouchClips;   // 플레이어가 데미지를 받을때 플레이되는 클립 배열 
                                    //#2-1 몬스터에 닿아서 죽을 때 플레이 사운드
	public bool starState;			//#9-1 starState : true이면 무적상태
	public float starTimer;
	public float starEndTime = 9.0f;	//무적 유지 시간
	private LobbyManager lobbyManager;	//#9-3 씬 바꾸기 위한 참조	
	private Rigidbody2D rigidbody2d;
    void Awake()
    {	
		// 레퍼런스들의 셋팅 
		Transform childTransform = transform.GetChild(0);	//@ 1칸 아래 자식 중, 1번째 하위 오브젝트의 Transform을 가져오겠다
    	anim = childTransform.GetComponent<Animator>();		//@ Player 오브젝트에 있는 게 아니라 그 하위인 PlayerMove 오브젝트에 있으니까 그냥 GetComponent로 선언하면 제대로 연결 안됨.

		playerCtrl = GetComponent<PlayerCtrl>();
		playerLevel = transform.root.GetComponent<PlayerLevel>();	//#5-1
	    groundCheck = childTransform.Find("groundCheck").transform;
		lobbyManager = GameObject.FindGameObjectWithTag("LobbyManager").GetComponent<LobbyManager>();	//#9-3
		rigidbody2d = GetComponent<Rigidbody2D>();  //@9 죽었을 때 더 밑으로 떨어지지 않도록
    }

	void Update()
	{	
		if(starState)
		{	
			lobbyManager.star = true;
			starTimer += Time.deltaTime;

		}
		if(starTimer > starEndTime)
		{
			starState = false;
			starTimer = 0f;	//시간 초기화
			lobbyManager.main = true;
		}
			
	}
	void FixedUpdate()
	{
		if((playerLevel.level !=0) && (transform.position.y < -10.0f))
		{
			if(playerLevel.level ==1)
				rigidbody2d.simulated = false;
			playerLevel.level = 0;
		}
	}

    //충돌 Callback 함수
        //@ - Trigger 체크 안했으니까 OnTriggerEnter가 아닌 OnCollisionEnter 사용하겠지
	void OnCollisionEnter2D (Collision2D col)
	{
		// 만약 충돌한 gameobject가 Enemy라면 
		if(col.gameObject.tag == "Enemy")
		{   
			//#2-1 그냥 부딪힌 게 아니라, 만약 밟은 거라면============================= 플레이어 발 y좌표(6.52-0.184) - 적 y좌표(0) = 6.336
			if((playerCtrl.jump == false) && (groundCheck.position.y - col.transform.position.y >=-1.0f))	//#2-1 내려오고 있고 && 닿았는데 Player가 위에 위치한 상황
						//#6-2 여기서 jump == false 조건을 안 달아주면, 점프하다가 스쳐지나가는 상황에서도 몬스터가 죽어버리고, 플레이어가 엄청 높이 점프하게 됨
			{
				return;
			}

			if(starState)	//#9-1 스타 효과 발동
			{
				col.gameObject.GetComponent<Enemy>().SpinDeath();
				return;
			}

			//#2-1 아래는 다쳤을 때 or 죽었을 때 실행되어야 하는 코드===============================
			// 그리고 만약 시간이 (마지막 충돌된 시간 + 재 충돌의 시간) 를 초과 한다면 
			if (!playerLevel.cannotDamaged && !col.gameObject.GetComponent<Enemy>().dead)	//다칠 수 없는 게 아니라면 = 다칠 수 있는 상태라면 && 몬스터 죽은 상태 아니라면
			{
				// 그리고 만약 플레이어가 아직까지 생명력이 0 이상이면 
				if(playerLevel.level >=2 )	//#5-2 if문 먼저 1번 탄 후, lastHitTime이 정의되고, repeatDamagePeriod 흐른 걸 적용하게 됨
								//그니까 repeatDamagePeriod을 계산하기 전에, 이미 TakeDamage는 실행되는 거지
								//lastHitTime 대신에 PlayerLevel에서 cannotDamaged 변수를 이용함.
				{
					playerCtrl.jump = false;	//반드시 플레이어가 점프를 할수없게 하자 
					playerLevel.LevelDown();	//#5-1 레벨 다운. 그에 맞는 상태 변화는 PlayerLevel 스크립트에서
				}
				//그렇지 않고 만약 플레이어가 생명력이 0 이하이면 스테이지 리셋을 위해 땅으로 떨어뜨려
				else //#2-1 if(playerLevel.level == 1) 만약 레벨이 1만 남은 상태였다면
				{
					playerLevel.level =0;
					gameObject.layer = 21;	//DeadPlayer로. (Ground예외) 어떤 것과도 충돌하지 않도록
				}
			}
		}
	}







}
