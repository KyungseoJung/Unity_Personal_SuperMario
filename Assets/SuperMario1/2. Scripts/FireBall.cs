using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour   //#8-3
{
    //이 스크립트는 어디에 연결? Player3야? 아니면 Fire야? -> fireball 자체를 나중에 없애야 하니까 fireball 프리팹에 연결해야지
    //그러면 Player3Move에 연결된 PlayerCtrl은 어떻게 가져와?
    // 처음 시작할 때, 모두 활성화 시킨 상태에서 시작해야 하나 - 그리고 나중에 PlayerLevel에 Start에서 비활성화를 시키면 되겠지
    // "Player" tag로 찾던 것들은 이름으로 찾게 하고.

    private Animator anim;
    private Rigidbody2D rigidbody2d;    

    void Awake()
    {
        anim = GetComponent<Animator>();    
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.layer == 10 ||
            col.gameObject.layer == 11 ||
            col.gameObject.layer == 12 ||
            col.gameObject.layer == 17)
        {
            anim.SetTrigger("Explosion");
            rigidbody2d.simulated = false;  //움직임 그만!
        }

        if(col.gameObject.tag == "Enemy")
        {
            anim.SetTrigger("Explosion");
            col.gameObject.GetComponent<Enemy>().SpinDeath();
            rigidbody2d.simulated = false;
        }
    }


}
