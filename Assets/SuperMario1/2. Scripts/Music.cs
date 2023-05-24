using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Music : MonoBehaviour
{
    public AudioSource[] gameMusicArr;
    /*
    0 : mainMusic
    1 : starMusic
    2 : deathMusic
    3 : goalMusic
    4 : gameClearMusic
    5 : gameOverMusic
    */
  
    void Start()
    {   
        AllMusicOff();
    }
    private void AllMusicOff()
    {
        for(int i=0; i<gameMusicArr.Length; i++)
        {
            gameMusicArr[i].gameObject.SetActive(false);
        }
    }
    public void MainMusicOn()
    {   
        AllMusicOff();
        gameMusicArr[0].gameObject.SetActive(true);
    }
    public void StarMusicOn()
    {
        AllMusicOff();
        gameMusicArr[1].gameObject.SetActive(true);
    }
    public void DeathMusicOn()
    {
        AllMusicOff();
        gameMusicArr[2].gameObject.SetActive(true);
    }
    public void GoalMusicOn()
    {
        AllMusicOff();
        gameMusicArr[3].gameObject.SetActive(true);
    }
    public void GameClearMusicOn()
    {
        AllMusicOff();
        gameMusicArr[4].gameObject.SetActive(true);
    }
    public void GameOverMusicOn()
    {
        AllMusicOff();
        gameMusicArr[5].gameObject.SetActive(true);
    }
}