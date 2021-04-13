using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile_Script : MonoBehaviour
{

    public int missileSpeed;
    public int damage;
    private void Start()
    {
    }

    protected void Update()
    {
        transform.Translate(Vector3.forward * missileSpeed * Time.deltaTime);
    }
    //private void OnDrawGizmos()=>Gizmos.DrawWireSphere(this.transform.position, 200);

    //★クラスの名前を引数に渡したい。=> できない。
    //その理由で、DamageObjectという規定（親）クラスを作って、GetDmg()という処理を全部ここで管理し、必要な子供クラスをつなぐこと！
    private void GetSplashDmg_DmgObject(string tagName_)
    {
        //問題の原因 : LayerMask.GetMask("")はレイヤーであり、タグじゃない！
        RaycastHit[] rayhits = Physics.SphereCastAll(this.transform.position, 150, Vector3.up, 0);
        foreach (RaycastHit hitObj in rayhits)
        {
            if (hitObj.transform.gameObject.tag == tagName_)//その理由でタグをつけて識別しなければならない。
            {
                hitObj.transform.GetComponent<EnemyController>().GetDamaged(damage);
            }
        }
    }
    private void OnTriggerEnter(Collider col)
    {
        //Wall--------------------------------------------------------------------------------------------------------------
        if (this.gameObject.CompareTag("Missile") && col.gameObject.CompareTag("Wall"))//壁にぶつかったら
        {
            Destroy(Instantiate(EffectManager.ins.E_MissileExplosion, transform.position, Quaternion.identity), 3);
            GetSplashDmg_DmgObject("Wall");
            Destroy(this.gameObject);
        }

        else if (this.gameObject.CompareTag("Gun") && col.gameObject.CompareTag("Wall"))//壁にぶつかったら
        {
            Destroy(Instantiate(EffectManager.ins.E_GunExplosion, transform.position, Quaternion.identity), 3);
            Destroy(this.gameObject);
        }
        //Enemy----------------------------------------------------------------------------------------------------------------
        if (this.gameObject.CompareTag("Missile") && col.gameObject.CompareTag("Enemy"))
        {
            Destroy(Instantiate(EffectManager.ins.E_MissileExplosion, transform.position, Quaternion.identity), 3);
            GetSplashDmg_DmgObject("Enemy");
            Destroy(this.gameObject);
        }
        else if (this.gameObject.CompareTag("Gun") && col.gameObject.CompareTag("Enemy"))
        {
            Destroy(Instantiate(EffectManager.ins.E_GunExplosion, transform.position, Quaternion.identity), 3);
            Destroy(this.gameObject);
        }
        else if (this.gameObject.CompareTag("P4_Skill") && col.gameObject.CompareTag("Enemy"))
        {
            Destroy(Instantiate(EffectManager.ins.E_P4Stun, transform.position, Quaternion.identity), 3);
            Destroy(this.gameObject);
        }
        //Player----------------------------------------------------------------------------------------------------------------
        if (this.gameObject.CompareTag("Missile") && col.gameObject.CompareTag("Player"))
        {
            //発射する自分は当たらないようにするために、それぞれの子供スクリップで宣言
        }
        else if (this.gameObject.CompareTag("Gun") && col.gameObject.CompareTag("Player"))
        {
            //発射する自分は当たらないようにするために、それぞれの子供スクリップで宣言
        }
        else if (this.gameObject.CompareTag("P4_Skill") && col.gameObject.CompareTag("Player"))
        {
            //P4は当たらないように
            if (col.gameObject.layer == 11)// P4
                return;

            col.GetComponent<PlayerController>().curFullCharge_Gauge += 10;
            col.GetComponent<PlayerController>().curHp -= this.damage;
            //エフェクト生成
            //プレイヤー
            Destroy(Instantiate(EffectManager.ins.E_PlayerGetHit, col.gameObject.transform.position, Quaternion.identity), 1);
            //爆弾
            Destroy(Instantiate(EffectManager.ins.E_P4Stun, this.transform.position, Quaternion.identity), 4.5f);
            //当たったミサイルを削除
            Destroy(this.gameObject);
        }
    }
}
