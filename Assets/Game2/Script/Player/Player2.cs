using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2 : PlayerController
{
    [Header("速度")]
    [SerializeField]  float speed;
    [SerializeField]  float radius;
    private float runningTime = 0;
    private Vector2 newPos = new Vector2();
    private Vector3 originalPos = new Vector3();

    //Player2弾
    [SerializeField] GameObject P2_Bullet;
    [SerializeField] float P2_BulletCnt;
    [SerializeField] float P2_BulletSpan;

    //Skill
    [SerializeField] int maxSkillCnt;
    [SerializeField] int curSkillCnt;
    new void Start()
    {
        base.Start();
        P2_BulletCnt = 0;
        curSkillCnt = 0;
        originalPos = gunPos_left.localPosition; //★localは子供のオブジェクトの座標を別にしたいとき、使う。
    }

    new void Update()
    {
        base.Update();
        //丸々曲がりながら、動く
        runningTime += Time.deltaTime * speed;
        P2_BulletCnt += Time.deltaTime;
        float x = radius * Mathf.Cos(runningTime);
        float y = radius * Mathf.Sin(runningTime);
        newPos = new Vector3(x + originalPos.x, y + originalPos.y, originalPos.z);
        gunPos_left.transform.localPosition = newPos;

        //スキルカウント初期化
        if (curSkillCnt == maxSkillCnt)
        {
            CancelInvoke("ShotSkill");
            curSkillCnt = 0;
        }
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
            if (col.gameObject.layer == 9 )
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
            if (col.gameObject.layer == 9)// P2
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
                gunSpan = 0.25f;
                break;
            case 2:
                gunSpan = 0.2f;
                break;
            case 3:
                gunSpan = 0.15f;
                break;
            case 4:
                gunSpan = 0.1f;
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
                    Destroy(Instantiate(gun_Prefab, gunPos_left.position, gunPos_left.rotation), 4);
                    break;
                case 2:
                    gunCnt = 0;
                    Destroy(Instantiate(gun_Prefab, gunPos_left.position, gunPos_left.rotation), 4);

                    //missilePos.rotation *= q; 発射すると気ごとに角度を加える

                    if(P2_BulletCnt > P2_BulletSpan)
                    {
                        P2_BulletCnt = 0;
                        for (int i = -1; i < 2; i++){
                            if (i == 0) continue;//0は処理しない。
                            Quaternion q = Quaternion.Euler(0, i * 10, 0);
                            Destroy(Instantiate(P2_Bullet, this.transform.position, missilePos.rotation * q), 4);
                        }
                    }
                    break;
                case 3:
                    gunCnt = 0;
                    Destroy(Instantiate(gun_Prefab, gunPos_left.position, gunPos_left.rotation), 4);

                    //missilePos.rotation *= q; 発射すると気ごとに角度を加える
                    if (P2_BulletCnt > P2_BulletSpan)
                    {
                        P2_BulletCnt = 0;
                        for (int i = -2; i < 3; i++)
                        {
                            if (i == 0) continue;//0は処理しない。
                            Quaternion q = Quaternion.Euler(0, i * 10, 0);
                            Destroy(Instantiate(P2_Bullet, this.transform.position, missilePos.rotation * q), 4);
                        }
                    }
                    break;
                case 4:
                    gunCnt = 0;
                    Destroy(Instantiate(gun_Prefab, gunPos_left.position, gunPos_left.rotation), 4);

                    //missilePos.rotation *= q; 発射すると気ごとに角度を加える
                    if (P2_BulletCnt > P2_BulletSpan)
                    {
                        P2_BulletCnt = 0;
                        for (int i = -3; i < 4; i++)
                        {
                            if (i == 0) continue;//0は処理しない。
                            Quaternion q = Quaternion.Euler(0, i * 10, 0);
                            Destroy(Instantiate(P2_Bullet, this.transform.position, missilePos.rotation * q), 4);
                        }
                    }
                    break;
            }
        }
    }

    protected override void Final_Skill()
    {
        if (curFullCharge_Gauge >= maxFullCharge_Gauge)
        {
            Destroy(Instantiate(EffectManager.ins.E_BeforeSkill, this.transform.position, Quaternion.identity), 2);
            curFullCharge_Gauge = 0;//値をゼロに戻す。
            InvokeRepeating("ShotSkill",0,0.2f);
        }
    }

    void ShotSkill() {
        if (myState != State.DIE)
        {
            curSkillCnt++;
            Destroy(Instantiate(missile_Prefab, gunPos_left.position, gunPos_left.rotation), 3.5f);
        }
        else
        {
            CancelInvoke("ShotSkill");
        }
        
    }


}
