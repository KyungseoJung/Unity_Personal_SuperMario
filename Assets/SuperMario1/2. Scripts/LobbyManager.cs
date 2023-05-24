using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;   //#9-2

public class LobbyManager : MonoBehaviour
{
    private Music music;
    public bool startGameScene = false;
    public float startTimer= 0.0f;
    public float startTime = 3.0f;
    
    public bool main = false;           //함수 1번만 타기 위한 안전장치
    private bool mainCheck = false;

    public bool star = false;
    private bool starCheck = false;     //함수 1번만 타기 위한 안전장치

    public bool goal = false;
    private bool goalCheck = false;     //함수 1번만 타기 위한 안전장치

    public bool gameClear = false;
    private bool gameClearCheck = false; //함수 1번만 타기 위한 안전장치

    public bool gameOver = false;
    private bool gameOVerCheck = false;  //함수 1번만 타기 위한 안전장치

    private GameObject canvas;          //#13-1 게임오버 or 게임클리어시 topBar 안 보이도록

    void Awake()
    {
        music = GameObject.FindGameObjectWithTag("Music").GetComponent<Music>();
        canvas = GameObject.FindGameObjectWithTag("Canvas");
    }
    void Start()
    {
        startTimer = 0.0f;
    }
    void Update()
    {  
        if(!startGameScene)     //게임씬이 시작되기 전까지만 타이머 증가
            startTimer+=Time.deltaTime;

        if(startTimer > startTime)
        {   
            SceneManager.LoadScene("scPlay1");
            music.MainMusicOn();     //메인 뮤직 재생

            startTimer = 0.0f; 
            startGameScene = true;
        }

        if(main && !mainCheck)
        {
            this.Main();
            mainCheck = true;   //#13-1 마지막 수정
        }
        if(star && !starCheck)
        {
            this.Star();
            starCheck = true;
        }
        if(goal && !goalCheck)
        {
            this.Goal();
            goalCheck = true;
        }
        if(gameOver && !gameOVerCheck)      //게임오버 && 체크하기 전
        {
            Invoke("Death", 0.0f);
            Invoke("GameOver", 5.0f);
            gameOVerCheck = true;
        }
        if(gameClear && !gameClearCheck)    //게임클리어 && 체크하기 전
        {
            Invoke("GameClear", 7.0f);
            gameClearCheck = true;
        }
    }
    public void Main()
    {
        music.MainMusicOn();
    }
    public void Star()
    {
        music.StarMusicOn();
    }
    public void Goal()
    {
        music.GoalMusicOn();
    }
    public void GameClear()
    {
        if(canvas.activeSelf == true)   //#13 상단바 안 보이도록
            canvas.SetActive(false);
        SceneManager.LoadScene("scGameClear");
        music.GameClearMusicOn();
    }
    void Death()
    {
        music.DeathMusicOn();
    }
    public void GameOver()  //몬스터에 죽음
    {
        if(canvas.activeSelf == true)   //#13 상단바 안 보이도록
            canvas.SetActive(false);
        SceneManager.LoadScene("scGameOver");
        music.GameOverMusicOn();

    }
}
