using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGM : MonoBehaviour
{
    public static SGM ins = null;//シングルトン

    public AudioSource S_Dead, S_DeadExplosion;
    public AudioSource S_GetDmg, S_MissileExplosion, S_ShootBullet, S_ShootMissile, S_GetItem;
    public AudioSource S_UI_StartGame, S_UI_Tap, S_UI_Decision, S_UI_StartCnt, S_UI_Error;
    public AudioSource S_BGM;

    private void Awake()
    {
        //シングルトン
        if (ins == null)
            ins = this;
        else if (ins != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
}
