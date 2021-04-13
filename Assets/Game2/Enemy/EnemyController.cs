using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public int maxHp, curHp;
    public int clashDmg;//とげ専用
    public Slider hpSlider;
    protected void Start()
    {
        curHp = maxHp;

        hpSlider.value = (float)curHp / maxHp;
    }

    protected void Update()
    {
        HandleHp();

        if (curHp < 0)
        {
            if(this.gameObject.tag == "Wall")
            {
                Destroy(Instantiate(EffectManager.ins.E_WallDestroy, this.gameObject.transform.position, Quaternion.identity), 3);
            }
            else
            {
                Destroy(Instantiate(EffectManager.ins.E_EnemyDead, this.gameObject.transform.position, Quaternion.identity), 3);
            }
            
            Destroy(this.gameObject);
        }
            
    }

    void HandleHp()
    {
        hpSlider.value = Mathf.Lerp(hpSlider.value, (float)curHp / maxHp, Time.deltaTime * 10);
    }


    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Gun"))
        {
            SGM.ins.S_GetDmg.Play();
            curHp -= col.GetComponent<Missile_Script>().damage;
        }
        else if (col.gameObject.CompareTag("Missile"))
        {
            SGM.ins.S_MissileExplosion.Play();
            //ミサイルスクリプトから直接に値を与える
        }
        else if (col.gameObject.CompareTag("P4_Skill"))
        {
            SGM.ins.S_MissileExplosion.Play();
            curHp -= col.GetComponent<Missile_Script>().damage;
        }
    }

    public void GetDamaged(int dmg)
    {
        curHp -= dmg;
    }
}
