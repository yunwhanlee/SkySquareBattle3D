using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player4 : PlayerController
{
    //Skill
    [SerializeField] GameObject P4_SkillMissile;
    [SerializeField] float P4_BulletCnt;
    [SerializeField] float P4_BulletSpan;

    [SerializeField] int skillCnt = 0;
    new void Start()
    {
        base.Start();
    }

    new void Update()
    {
        base.Update();
    }

    //継承して再定義する物（オーバーライド）------------------------------------------------------------------------------
    protected override void OnTriggerEnter(Collider col)
    {
        //既存の当たり判定
        base.OnTriggerEnter(col);

        //自分の弾・ミサイルは当たらないように
        if (col.gameObject.tag == "Gun")//Bullet
        {
            //自身の弾は当たらないように
            if (col.gameObject.layer == 11)
                return;

            curFullCharge_Gauge += 5;
            this.curHp -= col.gameObject.GetComponent<Missile_Script>().damage;
            //エフェクト生成
            //プレイヤー
            SGM.ins.S_GetDmg.Play();
            Destroy(Instantiate(EffectManager.ins.E_PlayerGetHit, this.gameObject.transform.position, Quaternion.identity), 1);
            //弾
            Destroy(Instantiate(EffectManager.ins.E_GunExplosion, transform.position, Quaternion.identity), 3);

            //当たったミサイルを削除
            Destroy(col.gameObject);
        }
        if (col.gameObject.tag == "Missile" || col.gameObject.tag == "P4_Skill")//BOMB
        {
            //自身の弾は当たらないように
            if (col.gameObject.layer == 11)// P4
                return;

            curFullCharge_Gauge += 10;
            this.curHp -= col.gameObject.GetComponent<Missile_Script>().damage;
            //エフェクト生成
            //プレイヤー
            SGM.ins.S_GetDmg.Play();
            Destroy(Instantiate(EffectManager.ins.E_PlayerGetHit, this.gameObject.transform.position, Quaternion.identity), 1);
            //爆弾
            SGM.ins.S_MissileExplosion.Play();
            Destroy(Instantiate(EffectManager.ins.E_MissileExplosion, transform.position, Quaternion.identity), 3);

            //当たったミサイルを削除
            Destroy(col.gameObject);
        }
    }

    protected override void Change_ShotSpeed()//弾の周期
    {
        switch (shotSpdItemCnt)
        {
            case 1:
                gunSpan = 0.45f;
                break;
            case 2:
                gunSpan = 0.4f;
                break;
            case 3:
                gunSpan = 0.35f;
                break;
            case 4:
                gunSpan = 0.3f;
                break;
        }
    }

    protected override void ShootBullet() 
    {
        if (gunCnt >= gunSpan)
        {
            SGM.ins.S_ShootBullet.Play();
            switch (attackItemCnt)
            {
                case 1:
                    gunCnt = 0;
                        Destroy(Instantiate(gun_Prefab, gunPos_left.position, missilePos.rotation), 4);
                        Destroy(Instantiate(gun_Prefab, gunPos_right.position, missilePos.rotation), 4);
                    break;
                case 2:
                    gunCnt = 0;
                    Destroy(Instantiate(gun_Prefab, gunPos_left.position, missilePos.rotation), 4);
                    Destroy(Instantiate(gun_Prefab, gunPos_right.position, missilePos.rotation), 4);
                    for (int i = -1; i < 2; i++)
                    {
                        Quaternion q = Quaternion.Euler(0, i * 10, 0);
                        Destroy(Instantiate(gun_Prefab, this.transform.position, missilePos.rotation * q), 4);
                    }
                    break;
                case 3:
                    gunCnt = 0;
                    Destroy(Instantiate(gun_Prefab, gunPos_left.position, missilePos.rotation), 4);
                    Destroy(Instantiate(gun_Prefab, gunPos_right.position, missilePos.rotation), 4);
                    for (int i = -2; i < 3; i++)
                    {
                        Quaternion q = Quaternion.Euler(0, i * 10, 0);
                        Destroy(Instantiate(gun_Prefab, this.transform.position, missilePos.rotation * q), 4);
                    }
                    break;
                case 4:
                    gunCnt = 0;
                    Destroy(Instantiate(gun_Prefab, gunPos_left.position, missilePos.rotation), 4);
                    Destroy(Instantiate(gun_Prefab, gunPos_right.position, missilePos.rotation), 4);
                    for (int i = -3; i < 4; i++)
                    {
                        Quaternion q = Quaternion.Euler(0, i * 10, 0);
                        Destroy(Instantiate(gun_Prefab, this.transform.position, missilePos.rotation * q), 4);
                    }
                    break;
            }
        }
    }

    protected override void Final_Skill()
    {
        if(curFullCharge_Gauge >= maxFullCharge_Gauge)
        {
            Destroy(Instantiate(EffectManager.ins.E_BeforeSkill, this.transform.position, Quaternion.identity), 2);

            curFullCharge_Gauge = 0;//値をゼロに戻す。
            ShotSkill();
        }
    }

    protected override void RotorAnim()
    {
        //何もしない。
    }

    void ShotSkill()
    {
        Destroy(Instantiate(P4_SkillMissile, gunPos_left.position, gunPos_left.rotation), 4f);
    }


}
