using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;	//#6-2 알파값 조정을 위해

public class PlayerLevel : MonoBehaviour    //#5-1
{
	public int level = 1;	//스타트 레벨 설정 // 1, 2, 3 순서대로 작은 마리오, 큰 마리오, 흰 옷 마리오
	public float changeDelayTime = 1.0f;	//#5-4 변신하는 시간 // - 이거 바꾸면 애니메이션 시간도 같이 조정해줘야 함.
	public bool cannotDamaged=false;	//#5-4 레벨 바뀌는 동안에는 다치지 않도록.

	public AudioClip levelupClip;	//#5-4 레벨 업, 다운 오디오 클립
	public AudioClip leveldownClip;		

	private GameObject player1;
	private GameObject player2;
	private GameObject player3;		//#8-2

	//#5-3 레벨 바꿀 때마다 카메라 다시 설정해주기
	FollowCamera followCamera;

	//#5-3 레벨 바꾸기 전 각 오브젝트의 Position 맞춰주기
	Transform player1Pos;
	Transform player2Pos;
	Transform player3Pos;			//#8-2

	//#9-1 레벨 바꾸기 전 각 오브젝트의 PlayerLife 맞춰주기(starState, starTimer)
	PlayerLife player1Life;
	PlayerLife player2Life;
	PlayerLife player3Life;
	
	private Animator player1Anim;	//상태 변화 애니메이션(1to2, 2to3, 3to1, 2to1)
	private Animator player2Anim;
	private Animator player3Anim;	//#8-2

	private LobbyManager lobbyManager;	//#9-3 씬 바꾸기 위한 참조	
	private TopBar topBar;

	
	void Awake()
	{	
		Transform childTransform1 = transform.GetChild(0);	//#5-4 1칸 아래 자식 중 1번째 위치 오브젝트
		Transform childTransform2 = transform.GetChild(1);	//#5-4 1칸 아래 자식 중 2번째 위치 오브젝트
		Transform childTransform3 = transform.GetChild(2);	//#8-2 1칸 아래 자식 중 3번째 위치 오브젝트
		
		player1 = transform.Find("Player1").gameObject;		//오브젝트 자체를 가져와서 나중에 활성화, 비활성화 이용 
		player2 = transform.Find("Player2").gameObject;
		player3 = transform.Find("Player3").gameObject;		//#8-2

		followCamera = GameObject.Find("Main Camera").GetComponent<FollowCamera>();	//#5-3 스크립트 참조

		player1Pos = transform.Find("Player1").transform;	//#5-3
		player2Pos = transform.Find("Player2").transform;
		player3Pos = transform.Find("Player3").transform;	//#8-2

		player1Life = transform.Find("Player1").GetComponent<PlayerLife>();
		player2Life = transform.Find("Player2").GetComponent<PlayerLife>();
		player3Life = transform.Find("Player3").GetComponent<PlayerLife>();

		player1Anim= childTransform1.Find("Player1Move").GetComponent<Animator>();	//#//Player 오브젝트의 애니메이터 가져오기
		player2Anim= childTransform2.Find("Player2Move").GetComponent<Animator>();	
		player3Anim= childTransform3.Find("Player3Move").GetComponent<Animator>();	//#8-2

		lobbyManager = GameObject.FindGameObjectWithTag("LobbyManager").GetComponent<LobbyManager>();	//#9-3

		topBar = GameObject.Find("TopBar").GetComponent<TopBar>(); //5. 실제 점수 올라가도록
	}

	void Start()
	{
		//처음엔 level에 해당하는 오브젝트만 활성화하기.
		if(player1.activeSelf == true)
			player1.SetActive(false);
		if(player2.activeSelf == true)
			player2.SetActive(false);
		if(player3.activeSelf == true)
			player3.SetActive(false);
		
		if(level ==1)
			player1.SetActive(true);	
		else if(level ==2)
			player2.SetActive(true);
		else if(level ==3)
			player3.SetActive(true);
	}
	
	void Update()
	{
		if(topBar.leftTime <0)	//#12-1
		{
			if(level >=2)		//플레이어 레벨 1모습으로 죽어
				LevelDown();
			level =0;
		}

	}

    public void LevelUp()	//약 1초
    {	
		cannotDamaged = true;	//레벨 변하는 동안은 다칠 수 없는 상태임.
		NoCollisionState(true);	//#5-4 몬스터와의 충돌처리 잠시동안 OFF.
		
		if(level==1)	//#5-4 오디오, 애니메이션
		{
			AudioSource.PlayClipAtPoint(levelupClip, player1Pos.position);	//레벨업 오디오 재생
			player1Anim.SetTrigger("1to2");	//레벨업 애니 재생	//레벨1에서 2로 레벨업
		}
		else if(level==2)
		{
			AudioSource.PlayClipAtPoint(levelupClip, player2Pos.position);
			player2Anim.SetTrigger("2to3");	//#8-2 애니 넣어야 함
		}
		
		if(level<=2)	//레벨업
			level+=1;
		if(level>3)
			return;

		//#5-3 좌표 모두 맞춰주기(현재 level에 해당하는 오브젝트를 중심으로)
		SetState(level-1);

		Time.timeScale = 0;		//#5-4 게임 화면 멈춤.(레벨업/다운하는 애니, 사운드만 제외하고)
		StartCoroutine(WaitAndResumeTime(changeDelayTime));	//#5-4 1초 후 다시 작동하도록	


		Invoke("StateChange", 0.1f);			//0.1+1(코루틴시간)초 뒤에 게임오브젝트 변경하기(작은 마리오->큰 마리오->흰 옷 마리오)
												//코루틴시간에 걸리도록 0.1f 적어준 것
		Invoke("StateChangeEnd", 0.1f);		//0.1+1초 뒤에 다칠 수 있는 상태로 변경
    }

	public void LevelDown()	//레벨 다운 함수	//약 1초 (다운된 레벨에서 깜빡이는 투명인간. -> 3초 후 깜빡이 사라짐)
	{	
		cannotDamaged = true;	//레벨 변하는 동안은 다칠 수 없는 상태임.
		NoCollisionState(true);	//#5-4 몬스터와의 충돌처리 잠시동안 OFF.

		if(level==2)	//#5-4 오디오, 애니메이션
			player2Anim.SetTrigger("2to1");	//레벨다운 애니 재생	//레벨2에서 1로 레벨다운
		
		if(level==3)
		{
			player3Anim.SetTrigger("3to1");	//#8-2 레벨다운
		}
		AudioSource.PlayClipAtPoint(leveldownClip, player2Pos.position);	//레벨다운 오디오 재생

		//#5-3 좌표 모두 맞춰주기(변경 전 level에 해당하는 오브젝트를 중심으로)
		SetState(level);

		if(level<=3)	//레벨다운(2에서도 1로 변경. 3에서도 1로 변경)
			level=1;	

		Time.timeScale = 0;		//#5-4 게임 화면 멈춤.(레벨업/다운하는 애니, 사운드만 제외하고)
		StartCoroutine(WaitAndResumeTime(changeDelayTime));	//#5-4 1초 후 다시 작동하도록



		Invoke("StateChange", 0.1f);		//0.1+1(코루틴시간)초 뒤에 StateChange로 상태 변경하기(흰 옷 마리오->큰 마리오->작은 마리오)
		Invoke("StateChangeEnd", 2.0f);		//3초(2.0+1) 뒤에 다칠 수 있는 상태로 변경
	}

	void StateChange()	//#5-1 level에 따른 상태변화
	{	
		//일단 모두 다 비활성화. 그 후> life에 맞는 해당 오브젝트만 활성화
		if(player1.activeSelf == true)
			player1.SetActive(false);
		if(player2.activeSelf == true)
			player2.SetActive(false);
		if(player3.activeSelf == true)
			player3.SetActive(false);

		//바뀐 후의 level에 해당하는 오브젝트가 나타나도록.
		if((level ==1) && (player1.activeSelf == false))	//(만약 life == 1이고&&Player1 비활성화 상태라면), 작은 마리오 오브젝트(Player1) 활성화	
			player1.SetActive(true);
		else if((level ==2) && (player2.activeSelf == false) )	//(만약 life == 2이면&&Player2 비활성화 상태라면), 큰 마리오 오브젝트(Player2) 활성화
			player2.SetActive(true);
		else if((level==3) && (player3.activeSelf == false))	//(만약 life == 3이면&&Player3 비활성화 상태라면), 흰 옷 마리오 오브젝트(Player3) 활성화
			player3.SetActive(true);
		

		//#5-3 카메라 위치 재설정
		followCamera.Awake();	//public void로 설정 변경
	}

	void StateChangeEnd()		//변화 종료. 무적상태 종료.
	{
		cannotDamaged = false;	//레벨 다 변했으니까 이제 다칠 수 있음.
		NoCollisionState(false);	//#5-4 몬스터와의 충돌처리 다시 ON
	}

	IEnumerator	WaitAndResumeTime(float waitTime)
	{
	//	yield return new WaitForSeconds(waitTime);	//위에서 시간을 멈춰놓아서, WaitForSeconds에서 시간이 흐르지 않는 거야.
		yield return new WaitForSecondsRealtime(waitTime);	//시간 영향을 받지 않는 WaitForSecondsRealTime 함수를 이용하자.
		Time.timeScale =1;
	}

	void SetState(int nowLevel)	//StateChange의 DelayTime과 맞춰주기 위함
	{
		switch(nowLevel)
		{
			case 1 :
				Invoke("SetState1", 0.1f);
				break;
			case 2 : 
				Invoke("SetState2", 0.1f);
				break;
			case 3 :		//#8-2
				Invoke("SetState3", 0.1f);	
				break;
		}
	}

	void SetState1()	//Player1 위치를 기준으로 모두 적용하기
	{
		player2Pos.position = player1Pos.position;
		player3Pos.position = player1Pos.position;

		player2Life.starState = player1Life.starState;
		player3Life.starState = player1Life.starState;

		player2Life.starTimer = player1Life.starTimer;
		player3Life.starTimer = player1Life.starTimer;
	}

	void SetState2()	//Player2 위치를 기준으로 모두 적용하기
	{
		player1Pos.position = player2Pos.position;
		player3Pos.position = player2Pos.position;

		player1Life.starState = player2Life.starState;
		player3Life.starState = player2Life.starState;

		player1Life.starTimer = player2Life.starTimer;
		player3Life.starTimer = player2Life.starTimer;
	}	
	void SetState3()	//#8-2
	{
		player1Pos.position = player3Pos.position;
		player2Pos.position = player3Pos.position;

		player1Life.starState = player3Life.starState;
		player2Life.starState = player3Life.starState;

		player1Life.starTimer = player3Life.starTimer;
		player2Life.starTimer = player3Life.starTimer;
	}

	void NoCollisionState(bool TorF)
	{	
		//자식 오브젝트까지 Layer가 변경 적용되려나? - 아닌듯
		switch(TorF)
		{
			case true :
				player1.layer = 14;	//LevelChange
				player2.layer = 14;
				player3.layer = 14;	//#8-2
				break;
			case false :
				player1.layer = 16;	//Players
				player2.layer = 16;
				player3.layer = 16;	//#8-2
				break;
		}	
	}


}

