using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1_Prefab : EnemyController
{
    public Transform body;
    public float range;
    public LayerMask layerMask = 0;
    public float spinSpeed = 0f;
    public int speed;
    public Vector3 firstPos, nowPos;
    public float moveArea;

    public Transform target = null;

    void SearchEnemy()
    {
        //自分の中心で、攻撃範囲HitBase矩形生成。
        Collider[] cols = Physics.OverlapSphere(transform.position, range, layerMask);
        Transform shortestTarget = null;
        if (cols.Length > 0)
        {
            float shortestDistance = Mathf.Infinity;
            foreach (Collider col_Target in cols)
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    new void Start()
    {
        base.Start();
        InvokeRepeating("SearchEnemy", 0f, 0.5f);
        firstPos = this.transform.position;
    }

    void Update()
    {
        base.Update();
        if (target == null)
        {   //元の場所に戻る。
            if (this.gameObject.transform.position != firstPos)
            {
                Debug.Log("もとに戻りましょ");
                Vector3 dir = firstPos - this.transform.position;
                dir = dir.normalized;
                transform.Translate(dir * speed * 2 * Time.deltaTime); 
                
                //基の場にもとるとき、firstPosとぴったり会わないので発生する震える現象、動きを止める調整。EX)xが200の時、x < 197～203 < x
                if (firstPos.x - 3 <= this.gameObject.transform.position.x && this.gameObject.transform.position.x <= firstPos.x + 3)
                    this.gameObject.transform.position = new Vector3(firstPos.x, firstPos.y, transform.position.z);
                if (firstPos.z - 3 <= this.gameObject.transform.position.z && this.gameObject.transform.position.z <= firstPos.z + 3)
                    this.gameObject.transform.position = new Vector3(transform.position.x, firstPos.y, firstPos.z);

            }
        }
        else
        {
            Quaternion lookRotation = Quaternion.LookRotation(target.position - this.transform.position);
            Vector3 euler = Quaternion.RotateTowards(body.rotation, lookRotation, spinSpeed * Time.deltaTime).eulerAngles;
            body.rotation = Quaternion.Euler(0, euler.y, 0);

            Quaternion fireRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
            if (Quaternion.Angle(body.rotation, fireRotation) < 5f)
            {
                //プレイヤーを追い付く
                float distance = Vector3.SqrMagnitude(target.transform.position - firstPos);
                if(distance < 350000)
                {
                    //Debug.Log("距離："+distance);
                    Vector3 dir = target.transform.position - this.transform.position;
                    dir = dir.normalized;
                    transform.Translate(dir * speed * Time.deltaTime);
                }
                //行動範囲を超えたらもとに戻す。
                else
                {
                    Vector3 dir = firstPos - this.transform.position;
                    dir = dir.normalized;
                    transform.Translate(dir * speed * 1.5f * Time.deltaTime);
                }
            }
        }
    }
}
