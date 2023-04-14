using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBlock : MonoBehaviour  //#4-1
{   
    public bool havetoCrashed=false;         //부숴져야 하는 상태인지 확인  (PlayCtrl에서 머리로 박기 전 false)
    public int crashTimes = 1;           //아이템 블록은 1번만 부숴질 수 있음
    public bool bonusBlock = false;      //#8-1 숨어있는 블록은 따로 인스펙터에서 true처리(머리 박기 전은 알파값 0으로)
    public bool starBlock = false;      //#8-3
    private SpriteRenderer[] itemBlockSprite;

    //1. 커브 처리
    public AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    public float UpTime = 0.1f;
    public float DownTime = 0.2f;
    private float playTimer = 0.0f;
    private Vector3 startPos;
    private Vector3 destPos;
    //Transform의 좌표를 바로 바꾸는 건 안되고, Vector3를 바로 바꾸는 건 되나?

    //3. 버섯 프리팹 등장하도록
    //부가 : 점수는 아이템을 먹어야 증가함. 아이템이 등장한다고 증가하는 게 X.
    //#4-2 부숴진 후엔 애니메이터 멈추도록
    private Animator anim;

    private PlayerLevel playerLevel;    //#8-1 레벨에 따라 다른 아이템 나오도록
    public GameObject Mushrooms;        //#4-2 버섯 아이템 프리팹들을 저장. 아이템 블록 부숴지면 나타나도록 Instantiate
    public GameObject BonusMushrooms;   //#8-1 보너스 버섯(점수만 증가함)
    public GameObject Flowers;          //#8-1 꽃 아이템 프리팹들을 저장.
    public GameObject Stars;               //#8-3
    public AudioClip itemAppearClip;      //#4-2 버섯/꽃 등장시, 오디오 클립
    public AudioClip blockHitClip;      //#6-1 하드 블록에 부딪히는 소리


    void Awake()
    {
         anim = GetComponent<Animator>();   //애니메이터 설정
         playerLevel = GameObject.FindGameObjectWithTag("AllPlayer").GetComponent<PlayerLevel>();
         itemBlockSprite = GetComponentsInChildren<SpriteRenderer>();  //#8-1
    }
    void Start()
    {
        startPos = transform.position;

        destPos = transform.position;
        destPos.y += 0.7f;  //0.7만큼 위로 올라갔다 내려옴

        if(bonusBlock)
            SetAlpha(0.0f);
    }

    //#4-1 코인 블록을 머리로 박으면(움직임에 따라 실행되어야 하므로 FixedUpdate)
    // 1. 부딪히면 위로 튕김, 2. 부딪히자마자 하드블록으로 이미지 변경, 3, 버섯 등장
    void FixedUpdate()   //public으로 정의해야 다른 함수에서도 가져다 쓸 수 있음.
    {
        if(havetoCrashed && (gameObject.layer == 11))
        {
            gameObject.layer = 20;  //벽돌치면 무기로 작동
        }
        else
        {
            gameObject.layer = 11;
        }

        if(havetoCrashed && (crashTimes>0)) //부숴져야 하는데(PlayCtrl에서 머리로 박은 것), 아직 다 부숴진 상태가 아니라면 
        {
            if(bonusBlock)
                SetAlpha(1.0f);

            anim.SetBool("Crashed", true); //부숴진 이미지로 드러나도록. 더이상 깜빡거리지 않도록 
            
            //#8-1 플레이어 현재 레벨에 따라 다른 아이템이 나오도록

            if(bonusBlock)   //#8-2 점수만 증가하는 초록 버섯
                anim.SetTrigger("BonusMushroom");   //애니메이터에 AppearBonus 연결     
            else if(starBlock)
                anim.SetTrigger("Star");       
            else if(playerLevel.level ==1)       
                anim.SetTrigger("Mushroom");    //1초동안 버섯이 올라오는 애니  //애니메이터에 AppearMushroom 연결해줬음. //Invoke("AppearMushroom", 1.0f);     //#4-2 1초 뒤에 버섯 프리팹 등장하도록
            else if(playerLevel.level >=2)      //#8-1 계속 AppearFlower 작동 안 되는 오류. >> 2이상이어서 이 식이 여러번 타게 됨. 
                            //애니메이션 끝까지 가지 못하고 계속 같은 애니가 여러번 실행된 것. 그래서 끝에 있는 함수가 호출이 안되었던 거야
                            //인 줄 알았는데, 그게 아니고 애니메이션 시간을 mushroom처럼 조정하니까 고쳐졌다.(꽃->Crashed로 가는 Transition의 Exit Time을 제일 높여줌)
                {
                    anim.SetTrigger("Flower");      //1초동안 꽃 올라오는 애니  //애니메이터에 AppearFlower 연결
                }
                    
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
                    crashTimes -= 1;    //한번만 부숴지도록
                    havetoCrashed = false;  //한번 부수고 나면 다시 false로 돌아가도록. 한번에 여러번 부숴지는 거 방지
                    AudioSource.PlayClipAtPoint(itemAppearClip, transform.position);   //버섯 아이템 등장 오디오

                }
        }  
        if(havetoCrashed && (crashTimes == 0))  //#6-1 하드블록에 박는 소리
            {
                AudioSource.PlayClipAtPoint(blockHitClip, transform.position);
                havetoCrashed = false;
            }  
    }

    public void TakeDamage()
    {
        havetoCrashed = true;
    }

    void AppearMushroom()   //#4-2
    {
        Vector3 appearPos;
        appearPos = transform.position;
        appearPos.y += 1.5f;    //1.5f 정도 위로 올라가서 버섯 나타나도록
        Instantiate(Mushrooms, appearPos, Quaternion.identity); // 버섯 아이템 등장
    }
    void AppearBonus()      //#8-1
    {
        Vector3 appearPos;
        appearPos = transform.position;
        appearPos.y += 1.5f;   
        Instantiate(BonusMushrooms, appearPos, Quaternion.identity); // 보너스 버섯 아이템 등장
    }
    void AppearFlower()     //#8-1
    {   
        Vector3 appearPos;
        appearPos = transform.position;
        appearPos.y += 1.5f;
        Instantiate(Flowers, appearPos, Quaternion.identity);   //꽃 아이템 등장
    }
    void AppearStar()   //#8-3
    {
        Vector3 appearPos;
        appearPos = transform.position;
        appearPos.y += 1.5f;
        Instantiate(Stars, appearPos, Quaternion.identity);   //꽃 아이템 등장
    }
    void SetAlpha(float alpha)
    {
        foreach(SpriteRenderer spr in itemBlockSprite)
        {
            Color itemBlockColor = spr.color;
            itemBlockColor.a = alpha;
            spr.color = itemBlockColor;
        }
        
    }


}
