using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBlock : MonoBehaviour  //#4-1
{   
    public bool havetoCrashed=false;         //부숴져야 하는 상태인지 확인  (PlayCtrl에서 머리로 박기 전)
    public bool multipleCoinBlock = false;    //블록이 아닌 것 같이 생겼으면서, 코인 6개 나오는 블록
    public int crashTimes = 1;           //여러번 부숴져야 하는 블록은 총 6번 부숴짐(Start함수에서 설정하기)

    //1. 커브 처리
    public AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    private float UpTime = 0.1f;
    private float DownTime = 0.2f;
    private float playTimer = 0.0f;
    private Vector3 startPos;
    private Vector3 destPos;

    //3. 코인 튕기는 애니메이터
    public GameObject CoinUI;
    public AudioClip coinClip;  //#6-1 코인 획득 사운드
    public AudioClip blockHitClip;   //#6-1 하드 블록에 부딪히는 소리
    //4. 점수 나오는 애니메이터
    public GameObject PointsUI;
    private float DelayTime = 0.5f; //0.5초 뒤에 점수 애니 실행되도록
    //5. 실제 점수 올리기
    private TopBar topBar;
    //#4-2 부숴진 후엔 애니메이터 멈추도록
    private Animator anim;

    void Awake()
    {
        topBar = GameObject.Find("TopBar").GetComponent<TopBar>(); //5. 실제 점수 올라가도록
        anim = GetComponent<Animator>();   //애니메이터 설정
    }
    void Start()
    {
        startPos = transform.position;

        destPos = transform.position;
        destPos.y += 0.7f;  //0.7만큼 위로 올라갔다 내려옴

        //multipleCoinBlock이 true이면, 8번 부숴야 끝남. 매번 코인은 나오고.
        if(multipleCoinBlock)
            crashTimes = 8;

    }

    //#4-1 코인 블록을 머리로 박으면(움직임에 따라 실행되어야 하므로 FixedUpdate)
    // 1. 블록은 한번 위로 튕기고, 2. 그 후 이미지 바뀌고, 3. 코인 애니메이터 나오고, 4. 점수도 띄워주고, 5. 실제 점수 올라가고
    void FixedUpdate()   //public으로 정의해야 다른 함수에서도 가져다 쓸 수 있음.
    {
        if(havetoCrashed && (gameObject.layer == 12))
        {
            gameObject.layer = 20;//벽돌치면 무기로 작동
        }
        else
        {
            gameObject.layer = 12;
        }

        if(havetoCrashed && (crashTimes>0)) //부숴져야 하는데(PlayCtrl에서 머리로 박은 것), 아직 다 부숴진 상태가 아니라면
        {
            if(crashTimes ==1 ) // 코인 나오는 게 1번 남은 거라면
                anim.SetBool("Crashed", true); //부숴진 이미지로 드러나도록. 더이상 깜빡거리지 않도록  
                                        //문제 : Parameter로 Crashed를 추가해놓았는데, 계속 does not exist라고 함. -> 단순 오타 문제
            if(playTimer<= UpTime)  //playTime > UpTime이기 전까지 실행
            {
                transform.localPosition = Vector3.Lerp(startPos, destPos, curve.Evaluate(playTimer/UpTime));
                playTimer += Time.deltaTime;
            }
            else if(playTimer<= DownTime)   //다시 내려가도록
            {
                transform.localPosition = Vector3.Lerp(destPos, startPos, curve.Evaluate(playTimer/DownTime));
                playTimer += Time.deltaTime;
            }
            else    //제자리에 돌아왔으면 이미 부숴진 상태로 업데이트
                {
                    AudioSource.PlayClipAtPoint(coinClip, transform.position);  //#6-1 코인 획득 사운드
                    crashTimes -=1;
                    CrashBlock();
                    havetoCrashed = false;  //한번 부수고 나면 다시 false로 돌아가도록. 한번에 여러번 부숴지는 거 방지
                    playTimer = 0.0f;       //playTimer는 원상복구(안 하면,  다음에 또 칠 때 curve식이 실행 안 됨.)
                }
        }     

    }
   
    //3. 코인 애니메이터 나오고(애니 마지막에 DestroyGameObject 연결), 4. 점수도 띄워주고, 5. 실제 점수 올라가고
    public void CrashBlock()        //딱 1번만 실행됨
    {
                                    //위치가 이상하게 나왔었음 - 애니메이터에서 부모의 위치를 건드려서
        Vector3 coinPos;            //3. 
        coinPos = transform.position;
        coinPos.y += 1.5f;
        Instantiate(CoinUI, coinPos, Quaternion.identity);

        //4. 점수 애니 실행 //유용한 함수 Invoke : InvokeRepeating은 여러번 실행하지만, Invoke는 한번만 실행함.
        Invoke("PlayPointsUIAnimation", DelayTime);        //n초 후에 점수 애니 실행

        topBar.score +=200;  //5.
        topBar.coin +=1;
    }

    void PlayPointsUIAnimation()
    {
        Vector3 scorePos;
        scorePos = transform.position;
        scorePos.y += 1.5f;
        Instantiate(PointsUI, scorePos, Quaternion.identity);
    }

    public void TakeDamage()
    {
        havetoCrashed = true;
    }



}
