using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2_Prefab : EnemyController
{
    public Transform gunbody;
    public float range;
    public LayerMask layerMask = 0;
    public float spinSpeed = 0f;
    public float firerate = 0f;
    public GameObject E_bullet;
    float curFirerate;

    public Transform target = null;

    void SearchEnemy()
    {
        //自分の中心で、攻撃範囲HitBase矩形生成。
        Collider[] cols = Physics.OverlapSphere(transform.position, range, layerMask);
        Transform shortestTarget = null;
        if(cols.Length > 0)
        {
            float shortestDistance = Mathf.Infinity;
            foreach(Collider col_Target in cols)
            {
                //距離を求める
                float distance = Vector3.SqrMagnitude(transform.position - col_Target.transform.position);
                if (shortestDistance > distance)
                {
                    //★PlayerがP1なので、弾を探索しなくて、プレイヤータグだけ探索することにする
                    if (col_Target.gameObject.CompareTag("Player"))
                    {
                        shortestDistance = distance;
                        shortestTarget = col_Target.transform;
                    }
                }
            }
        }
        target = shortestTarget;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    void Start()
    {
        base.Start();
        InvokeRepeating("SearchEnemy", 0f, 0.5f);
        curFirerate = firerate;
    }

    void Update()
    {
        base.Update();
        if(target == null)
        {
            gunbody.Rotate(new Vector3(0, 45, 0) * Time.deltaTime);
        }
        else
        {
            Debug.Log("探索完了");
            Quaternion lookRotation = Quaternion.LookRotation(target.position - this.transform.position);
            Vector3 euler = Quaternion.RotateTowards(gunbody.rotation, lookRotation, spinSpeed * Time.deltaTime).eulerAngles;
            gunbody.rotation = Quaternion.Euler(0, euler.y, 0);

            Quaternion fireRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
            if(Quaternion.Angle(gunbody.rotation, fireRotation) < 5f)
            {
                curFirerate -= Time.deltaTime;
                if(curFirerate <= 0)
                {
                    Debug.Log("発射");
                    Vector3 shotPos = new Vector3(gunbody.transform.position.x, gunbody.transform.position.y + 30, gunbody.transform.position.z);
                    Destroy(Instantiate(E_bullet, shotPos, lookRotation),3f);
                    curFirerate = firerate;
                }
            }
        }
    }
}
