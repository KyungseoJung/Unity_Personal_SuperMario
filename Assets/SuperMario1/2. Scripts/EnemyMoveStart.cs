using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveStart : MonoBehaviour
{
    public bool enemyMoveStart = false;  //처음엔 일단 안 움직이도록
    Enemy enemy;
   Transform cameraTransform;

   void Awake()
   {
        cameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        enemy = transform.GetComponent<Enemy>();
   }
   void Start()
   {    
        if(enemy.enabled == true)
            enemy.enabled = false;
   }
   void Update()
   {
        if(cameraTransform.position.x > transform.position.x - 20)
            enemy.enabled = true;
   }
}
