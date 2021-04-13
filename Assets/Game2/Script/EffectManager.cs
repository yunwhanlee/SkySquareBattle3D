using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectManager : MonoBehaviour
{
    public static EffectManager ins = null;
    Animator anim;

    //エフェクトオブジェクト
    public GameObject E_MissileExplosion;
    public GameObject E_GunExplosion;
    public GameObject E_PlayerDead, E_EnemyDead;
    public GameObject E_GetHpItem, E_GetLevelUpItem, E_GetMoveSpeedItem, E_GetShotSpeedItem;
    public GameObject E_PlayerGetHit;
    public GameObject E_FullCharge, E_BeforeSkill, E_P4Stun;
    public GameObject E_WallDestroy;

    //UI
    public Text lockOn_Txt;

    void Awake()
    {
        //シングルトン
        if (ins == null)
            ins = this;
        else if (ins != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
}
