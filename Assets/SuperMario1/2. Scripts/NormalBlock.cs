using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBlock : MonoBehaviour  //#4-1
{   
    public bool havetoPushed=false;         //밀려야 하는 상태인지 확인  (PlayCtrl에서 머리로 박기 전은 false)
    //커브 처리
    public AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    private float UpTime = 0.1f;
    private float DownTime = 0.2f;
    private float playTimer = 0.0f;
    private Vector3 startPos;
    private Vector3 destPos;             //Transform의 좌표를 바로 바꾸는 건 안되고, Vector3를 바로 바꾸는 건 되나?\
    private PlayerLevel playerLevel;    //#6-1 플레이어 레벨에 따라 블록 부숴지는 효과 다름
    public AudioClip blockHitClip;       //#6-1 블록 밀리는 소리(플레이어 레벨 1일 때) 
    public AudioClip crashClip;          //#6-1 블록 부숴지는 소리(플레이어 레벨 2일 때)
    private Animator anim;              //#6-1 블록 부숴지는 애니메이터
    void Start()
    {
        startPos = transform.position;

        destPos = transform.position;
        destPos.y += 0.7f;  //0.7만큼 위로 올라갔다 내려옴

        playerLevel = GameObject.FindGameObjectWithTag("AllPlayer").GetComponent<PlayerLevel>();    //스크립트 가져오기
        anim = GetComponent<Animator>();
    }

    //#4-1 일반 블록을 머리로 박으면(움직임에 따라 실행되어야 하므로 FixedUpdate)
    //(플레이어가 작은 상태라면) 1. 블록 한번 위로 튕김.
    //(플레이어가 커진 상태라면) 1. 블록 깨지면서 아예 사라짐.
    void FixedUpdate()   //public으로 정의해야 다른 함수에서도 가져다 쓸 수 있음.
    {
        if(havetoPushed && (gameObject.layer == 10))
        {
            gameObject.layer = 20;
        }
        else
        {
            gameObject.layer = 10;
        }

        if(playerLevel.level == 1 && havetoPushed) //(PlayCtrl에서 머리로 박은 것)
        {
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
            else    //제자리에 돌아왔으면 
                {
                    AudioSource.PlayClipAtPoint(blockHitClip, transform.position);   //#6-1 블록 밀리는/때리는 소리
                    playTimer = 0.0f;   //playTimer는 원상복구(안 하면, 다음에 또 칠 때 curve식이 실행 안 됨.)
                    havetoPushed = false;   //원상복구
                }
        }   
        else if(playerLevel.level == 2 && havetoPushed)
        {
            anim.SetTrigger("Crashed"); //#6-1 블록 부숴지는 애니메이션. 게임오브젝트 비활성화 연결되어있음.
            AudioSource.PlayClipAtPoint(blockHitClip, transform.position);   //#6-1 블록 밀리는/때리는 소리
            AudioSource.PlayClipAtPoint(crashClip, transform.position);
            havetoPushed = false;
        }
        else if(playerLevel.level == 3 && havetoPushed)
        {
            anim.SetTrigger("Crashed"); //#6-1 블록 부숴지는 애니메이션. 게임오브젝트 비활성화 연결되어있음.
            AudioSource.PlayClipAtPoint(blockHitClip, transform.position);   //#6-1 블록 밀리는/때리는 소리
            AudioSource.PlayClipAtPoint(crashClip, transform.position);
            havetoPushed = false;
        }
    }

    public void TakeDamage()
    {
        havetoPushed = true;
    }




}
