using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Scripting.APIUpdating;

public class PlayerController : MonoBehaviour
{
    Rigidbody rigid;
    Animator anim;
    public NavMeshAgent agent;
    public bool Dead_Active;

    [Header("UI")]
    [SerializeField] protected Slider Hpbar;
    [SerializeField] protected Slider FullCharge_Skillbar;
    [SerializeField] protected Image missileCooltime_Image;
    [SerializeField] protected Text ability_Txt;
    [SerializeField] protected Text KillInfo_Txt;
    [SerializeField] protected Image autoAtk_Img;
    [SerializeField] protected Canvas miniMap_Icon;

    [Header("能力値")]
    [SerializeField] public int life;
    [SerializeField] protected int maxHp;
    [SerializeField] public float curHp;

    [SerializeField, Header("移動速度")] protected int maxMove_Speed;
    [SerializeField] protected int move_Speed;
    [SerializeField] protected int move_SpeedUp;

    [SerializeField, Header("画面動きのマウス敏感度")] protected float lookSensitivity;


    [Header("カメラモード")]
    [SerializeField]    Camera cam; //前は０，５、－８でした
    [SerializeField]    Camera cam2;//0,0.5,10
    enum CameraMode{
        Main,
        Sub,
    }
    CameraMode cameraMode = CameraMode.Main;

    protected enum State
    {
        ALIVE,
        DIE
    }
    protected State myState = State.ALIVE;

    [Header("フールチャージ")]
    [SerializeField] protected GameObject Effect_fullChargeObj;
    [SerializeField] protected int maxFullCharge_Gauge = 100;
    [SerializeField] public float curFullCharge_Gauge;

    [Header("BOOMBミサイル（マウス左クリック）")]
    [SerializeField] protected GameObject missile_Prefab;
    [SerializeField] protected Transform missilePos;
    [SerializeField] protected float missileSpan;//発射周期
    protected float missileCnt;


    [Header("小さい弾（オート発射）")]
    [SerializeField] protected bool autoBullet;
    [SerializeField] protected GameObject gun_Prefab;
    [SerializeField] protected Transform gunPos_left;
    [SerializeField] protected Transform gunPos_right;

    [SerializeField] protected List<Transform> gunPos_leftList;

    [Header("アイテム習得カウント")]
    public int maxItemGetCnt = 4;
    [SerializeField] protected int shotSpdItemCnt = 1;//発射周期レベル
    [SerializeField] protected float gunSpan;//発射周期
    [SerializeField] protected int attackItemCnt = 1;
    [SerializeField] private int moveItemCnt = 1;

    protected float gunCnt;
    protected int leftRightCnt;

    [SerializeField, Header("アニメ（羽「はね」）")]protected GameObject Rotor;
    protected float rotYCnt;

    float _moveDirX;//左右移動
    //エフェクトのオブジェクト
    [SerializeField] protected GameObject[] EF_objs;

    //★ドットダメージを受ける（トゲ・敵１）
    Dictionary<GameObject, float> colDic = new Dictionary<GameObject, float>();
    public float getDamageInterval = 2f;

    //復活するときの位置
    Vector3 StartPos;


    [Header("AI")]
    public bool AI_active;
    [SerializeField] Transform target;//ターゲット
    [SerializeField] LayerMask m_layerMask;
    [SerializeField] RaycastHit rayhit;
    [SerializeField] int stoppingDis;//ターゲットと近づいたとき、後ろに移動
    [SerializeField] float m_range;
    [SerializeField] bool lookOn_Trigger;

    public int nextMoveX;
    public int nextMoveZ;
    float deg;

    protected void Start()
    {
        //アイテム習得状況
        attackItemCnt = 1;
        moveItemCnt = 1;
        shotSpdItemCnt = 1;

        //スタート位置を保存(復活するため)
        StartPos = this.transform.position;

        //UI
        KillInfo_Txt.enabled = false;
        Hpbar.value = curHp / maxHp;
        FullCharge_Skillbar.value = curFullCharge_Gauge / maxFullCharge_Gauge;
        missileCooltime_Image.fillAmount = missileCnt / missileSpan;
        ability_Txt.text =
            "攻撃LV：" + attackItemCnt + "\n" +
            "体力：" + curHp + "\n" +
            "発射速度：" + gunSpan + "\n" +
            "移動速度：" + move_Speed;

        //
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        CameraViewChange(CameraMode.Main);
        missileCnt = 0;
        gunCnt = 0;
        leftRightCnt = 0;
        curHp = maxHp;
        curFullCharge_Gauge = 0;
    }

    //Control And AI------------------------------------------------------------------
    protected void Awake()
    {
        agent.speed = Time.deltaTime * move_Speed * 14;
        if (AI_active)
        {
            Think();
        }
    }

    protected void FixedUpdate()
    {
        if (myState == State.ALIVE && GameManager.ins.gs == GameManager.Gm_State.PLAYING)
        {
            if (!AI_active)//自分で操作
            {
                //CancelInvoke();
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                //プレイヤー移動
                Move();
                //マウスで左右視点移動
                //CharacterRotation();
            }
            else//★AI_active ON
            {
                //AIで自動操作
                target = SearchObject();//敵・アイテム・プレイヤーなどを探索

                if (target != null)//★★ターゲットが有る
                {
                    if(this.gameObject.GetComponent<NavMeshAgent>().enabled)
                        agent.SetDestination(target.position);

                    //角度を対応して、相手に迎える
                    transform.LookAt(target.transform);

                    if (target.CompareTag("Player") || target.CompareTag("Enemy"))
                    {
                        //ターゲット周囲を曲がる
                        rigid.AddForce(Vector3.right * Time.deltaTime * move_Speed);

                        //発射
                        ShootBullet();
                        ShootMissile();
                        Final_Skill();

                        //一定距離を維持する
                        float target_dis = Vector3.Magnitude(this.transform.position - target.transform.position);
                        if (target_dis < stoppingDis && agent.stoppingDistance != 0)
                        {
                            //Debug.Log(target_dis);
                            transform.Translate(Vector3.back * (move_Speed / 6) * Time.deltaTime);
                        }
                    }
                    else if (target.gameObject.layer == 13)
                    {
                        rigid.velocity = Vector3.zero;
                        rigid.angularVelocity = Vector3.zero;
                        agent.stoppingDistance = 0;
                    }
                }
                else////★★ターゲットがない
                {
                    rigid.velocity = Vector3.zero;
                    rigid.angularVelocity = Vector3.zero;
                    rigid.velocity = new Vector3(nextMoveX * move_Speed * 5 * Time.deltaTime, rigid.velocity.y, nextMoveZ * move_Speed * 5 * Time.deltaTime);
                }
            }
        }       
    }


    public void Think()
    {
        if(target == null)
        {
            nextMoveX = Random.Range(-1, 2);
            nextMoveZ = Random.Range(-1, 2);


            float nextThinkTime = Random.Range(3f, 6.5f);

            if (nextMoveX == 0 && nextMoveZ == 0)
                nextThinkTime = 1;

            Invoke("Think", nextThinkTime);
        }
    }


    Transform SearchObject()
    {
        Collider[] target_cols = Physics.OverlapSphere(transform.position, m_range, m_layerMask);
        Transform themostClosed_Target = null;

        if (target_cols.Length > 0)
        {
            
            float themostClosed_Dis = Mathf.Infinity;
            foreach (Collider col in target_cols)
            {
                //ミサイルと弾は探索しない
                if(!col.gameObject.CompareTag("Missile") && !col.gameObject.CompareTag("Gun")){

                    agent.stoppingDistance = stoppingDis;

                    float target_dis = Vector3.SqrMagnitude(transform.position - col.transform.position);
                    if (themostClosed_Dis > target_dis)
                    {
                        themostClosed_Dis = target_dis;
                        themostClosed_Target = col.transform;
                    }
                }
            }
        }
        return themostClosed_Target;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDis);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(this.transform.position, m_range);
    }

    //---------------------------------------------------------------------------------------------------
    protected void Update()
    {
        if(this.gameObject != null)
        {
            if (myState == State.ALIVE && GameManager.ins.gs == GameManager.Gm_State.PLAYING)
            {
                if (!AI_active)
                {
                    //UI
                    Hpbar.value = curHp / maxHp;
                    FullCharge_Skillbar.value = curFullCharge_Gauge / maxFullCharge_Gauge;
                    missileCooltime_Image.fillAmount = missileCnt / missileSpan;
                    ability_Txt.text =
                        "ShootLV：" + attackItemCnt + "\n" +
                        "H     P：" + curHp + "\n" +
                        "AtkSpd ：" + gunSpan + "\n" +
                        "MoveSpd：" + move_Speed;
                }



                //体力バグ対応
                if (curHp > maxHp) curHp = maxHp;
                else if (curHp < 0)
                {
                    curHp = 0;
                    Destroy(Instantiate(EffectManager.ins.E_PlayerDead, this.transform.position, Quaternion.identity), 2.5f);

                    Dead();
                }

                //フールチャージ
                AutoGaugeUp();
                FullCharge_Gauge();

                //プロペラアニメ
                RotorAnim();

                //視点変換★
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    CameraMode nextMode = CameraMode.Main;

                    if (cameraMode == CameraMode.Main)
                    {
                        nextMode = CameraMode.Sub;
                    }
                    CameraViewChange(nextMode);
                }


                gunCnt += Time.deltaTime;
                missileCnt += Time.deltaTime;
                if (!AI_active)
                {

                    CharacterRotation();

                    if (Input.GetMouseButtonDown(0))//自動攻撃（AUTO）
                    {
                        autoBullet = !autoBullet;
                        if (autoBullet)
                        {
                            autoAtk_Img.color = Color.white;
                        }
                        else
                        {
                            autoAtk_Img.color = Color.gray;
                        }
                    }

                    if (autoBullet)
                    {
                        ShootBullet();//弾発射
                        ShootMissile();
                    }


                    //ミサイル発射
                    //if (Input.GetMouseButtonDown(1)){ShootMissile();}

                    if (Input.GetKeyDown(KeyCode.R))//必殺技
                    {
                        Final_Skill();
                    }

                }


                //★定期的に体当たりの攻撃オブジェクトリストを掃除しておく
                CheckEnemy();

                //ズーム
                if (cam.enabled == true)
                {
                    Zoom();
                }
            }
        }
        
    }

    public void Dead()
    {
        //状態⇒「死んだ」にする
        myState = State.DIE;

        SGM.ins.S_Dead.Play();
        SGM.ins.S_DeadExplosion.Play();

        //ゲームマネージャに値を与える
        GameManager.ins.dead_Active = true;
        GameManager.ins.life_HeartCnt =  --life;

        //初期化
        //イメージ（非活性化）
        var mrs = this.gameObject.GetComponentsInChildren<MeshRenderer>();
        foreach(var mr in mrs)
        {
            mr.enabled = false;
        }
        //当たり判定（非活性化）
        this.gameObject.GetComponent<BoxCollider>().enabled = false;

        for (int i = 0; i < EF_objs.Length; i++)
        {
            EF_objs[i].SetActive(false);
        }

        curHp = 0;

        
        if (life > 0)//まだ命が残っているとき
            StartCoroutine(Respown());

        else//★ゲームオーバーかないかの処理
        {
            //死んだプラグON
            Dead_Active = true;
            this.gameObject.GetComponent<NavMeshAgent>().enabled = false;
            miniMap_Icon.enabled = false;
            Effect_fullChargeObj.SetActive(false);

            //自分がゲームオーバーになったら、早くゲームを終了
            if (!this.AI_active)
            {
                Time.timeScale = 5;
                GameManager.ins.gameoverText.gameObject.SetActive(true);
            }
            //GAMEOVER 登録 順位
            if (gameObject.layer == 8 && !AI_active) { GameManager.ins.rankList.Add("<color=#0000ff>" + "Player1 (ME)"+"</color>"); }
            else if (gameObject.layer == 8) { GameManager.ins.rankList.Add("Player1"); }
            if (gameObject.layer == 9 && !AI_active) { GameManager.ins.rankList.Add("<color=#0000ff>" + "Player2 (ME)" + "</color>"); }
            else if (gameObject.layer == 9) { GameManager.ins.rankList.Add("Player2"); }
            if (gameObject.layer == 10 && !AI_active) { GameManager.ins.rankList.Add("<color=#0000ff>" + "Player3 (ME)" + "</color>"); }
            else if (gameObject.layer == 10) { GameManager.ins.rankList.Add("Player3"); }
            if (gameObject.layer == 11 && !AI_active) { GameManager.ins.rankList.Add("<color=#0000ff>" + "Player4 (ME)" + "</color>"); }
            else if (gameObject.layer == 11) { GameManager.ins.rankList.Add("Player4"); } 

            //最後に残ったオブジェクトを登録
            if (GameManager.ins.rankList.Count == 3)
            {
                // 4番目のプレイヤーを探し出す
                var allPlayers = GameManager.ins.Player;
                string finalPlayerName = "";
                foreach(var player in allPlayers)
                {
                    bool isHit = false;
                    foreach(var playerName in GameManager.ins.rankList)
                    {
                        // ランクリストとプレイヤー名に合致するものがある
                        if (playerName.IndexOf(player.name) >= 0 && player != null)
                        {
                            isHit = true;
                            break;
                        }
                    }

                    if (isHit) continue;

                    // 4番目のプレイヤーを登録
                    PlayerController plc = player.GetComponent<PlayerController>();
                    if (plc != null)
                    {
                        if (plc.AI_active) finalPlayerName = player.name;
                        else finalPlayerName = player.name + "<color=#0000ff>" + " (Me)" + "</color>";

                        GameManager.ins.rankList.Add(finalPlayerName);
                    }
                    break;
                }

                GameManager.ins.gameover_Active = true;
                Time.timeScale = 1;
            }
            
        }
    }
    void Zoom()
    {
        var scroll = Input.mouseScrollDelta;
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - scroll.y, 55f, 90f);//視野角
    }
    IEnumerator Respown()
    {
        //キール状況お知らせるUI
        KillInfo_Txt.enabled = true;
        if (gameObject.layer == 8) { KillInfo_Txt.text = "<color=#ffff00ff>" + "Player 1 Dead" + "</color>"; }
        if (gameObject.layer == 9) { KillInfo_Txt.text = "<color=#ffa500ff>" + "Player 2 Dead" + "</color>"; }
        if (gameObject.layer == 10) { KillInfo_Txt.text = "<color=#0000ffff>" + "Player 3 Dead" + "</color>"; }
        if (gameObject.layer == 11) { KillInfo_Txt.text = "<color=#ff0000ff>" + "Player 4 Dead" + "</color>"; }

        yield return new WaitForSeconds(3);
        KillInfo_Txt.enabled = false;
        //状態⇒「生きる」にする
        myState = State.ALIVE;

        //イメージ（活性化）
        var mrs = this.gameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in mrs)
        {
            mr.enabled = true;
        }
        //当たり判定（活性化）
        this.gameObject.GetComponent<BoxCollider>().enabled = true;
        //位置
        this.gameObject.transform.position = StartPos;
        //体力
        curHp = maxHp;

        for(int i = 0; i < EF_objs.Length; i++)
        {
            EF_objs[i].SetActive(true);
        }
        
    }

    void CheckEnemy()
    {
        float nowTime = Time.time;
        Dictionary<GameObject, float> okList = new Dictionary<GameObject, float>();
        foreach (var dic in colDic)
        {
            if (dic.Key == null)// Key => GameObject
            {
                Debug.Log("<color=green>Remove</color>");
                continue;
            }
            if (dic.Value < Time.time)// Value => float
            {
                Debug.Log("<color=green>Remove</color>");
                continue;
            }
            okList.Add(dic.Key, dic.Value);
        }
        colDic.Clear();
        colDic = okList;
    }




    void AutoGaugeUp() => curFullCharge_Gauge += Time.deltaTime;

    //フールチャージ
    void FullCharge_Gauge()
    {
        if (curFullCharge_Gauge >= maxFullCharge_Gauge)
            Effect_fullChargeObj.SetActive(true);
        else
            Effect_fullChargeObj.SetActive(false);

        //最大値を超えないように固定
        if (curFullCharge_Gauge > maxFullCharge_Gauge)
            curFullCharge_Gauge = maxFullCharge_Gauge;
    }

    //当たり判定
    protected virtual void OnTriggerEnter(Collider col)
    {
        if(gameObject != null)
        {
            switch (col.gameObject.tag)
            {
                //アイテム-------------------------------------------------------------------------------------------------------
                case "LevelUpItem":
                    SGM.ins.S_GetItem.Play();
                    curFullCharge_Gauge += 10;
                    ++this.attackItemCnt;
                    if (this.attackItemCnt > maxItemGetCnt)
                        this.attackItemCnt = maxItemGetCnt;
                    if (!AI_active)//プレイヤーの習得したアイテム情報だけ渡す
                        GameManager.ins.atkItemCnt = this.attackItemCnt;
                    Destroy(Instantiate(EffectManager.ins.E_GetLevelUpItem, col.transform.position, Quaternion.identity), 3);//エフェクト生成
                    Destroy(col.gameObject);
                    break;
                case "MoveSpeedItem":
                    SGM.ins.S_GetItem.Play();
                    curFullCharge_Gauge += 10;
                    ++this.moveItemCnt;
                    move_Speed += move_SpeedUp;
                    if (this.moveItemCnt > maxItemGetCnt)
                    {
                        this.moveItemCnt = maxItemGetCnt;
                        move_Speed = maxMove_Speed;
                    }
                    if (!AI_active)//プレイヤーの習得したアイテム情報だけ渡す
                        GameManager.ins.moveItemCnt = this.moveItemCnt;
                    Destroy(Instantiate(EffectManager.ins.E_GetMoveSpeedItem, col.transform.position, Quaternion.identity), 3);//エフェクト生成
                    Destroy(col.gameObject);
                    break;
                case "ShotSpeedItem":
                    SGM.ins.S_GetItem.Play();
                    curFullCharge_Gauge += 10;
                    ++this.shotSpdItemCnt;
                    if (this.shotSpdItemCnt > maxItemGetCnt)
                        this.shotSpdItemCnt = maxItemGetCnt;
                    if (!AI_active)//プレイヤーの習得したアイテム情報だけ渡す
                        GameManager.ins.shotSpdItemCnt = this.shotSpdItemCnt;
                    Change_ShotSpeed();
                    Destroy(Instantiate(EffectManager.ins.E_GetShotSpeedItem, col.transform.position, Quaternion.identity), 3);//エフェクト生成
                    Destroy(col.gameObject);
                    break;
                case "HpItem":
                    SGM.ins.S_GetItem.Play();
                    this.curHp += 50;
                    curFullCharge_Gauge += 10;
                    if (this.curHp > maxHp)
                        this.curHp = maxHp;
                    Destroy(Instantiate(EffectManager.ins.E_GetHpItem, col.transform.position, Quaternion.identity), 3);//エフェクト生成
                    Destroy(col.gameObject);
                    break;

                //敵-----------------------------------------------------------------------------------------------------------
                case "EnemyBullet":
                    curFullCharge_Gauge += 10;
                    SGM.ins.S_GetDmg.Play();
                    this.curHp -= col.gameObject.GetComponent<Enemy_Missile>().damage;
                    //エフェクト生成
                    Destroy(Instantiate(EffectManager.ins.E_PlayerGetHit, this.gameObject.transform.position, Quaternion.identity), 1);
                    Destroy(col.gameObject);
                    break;
                //障害物-----------------------------------------------------------------------------------------------------------
                case "Obstacle":
                    curFullCharge_Gauge += 10;
                    this.curHp -= col.gameObject.GetComponent<Enemy_Missile>().damage;
                    //エフェクト生成
                    Destroy(Instantiate(EffectManager.ins.E_PlayerGetHit, this.gameObject.transform.position, Quaternion.identity), 1);
                    break;
                //壁-----------------------------------------------------------------------------------------------------------
                case "Wall":
                    if (AI_active)
                    {
                        Debug.Log("AI 壁にぶつかった");
                        //反対方向
                        nextMoveX *= -1;
                        nextMoveZ *= -1;
                        CancelInvoke();
                        Think();
                    }
                    break;
                    //弾やミサイルに当たる処理はそれぞれの子供スクリップに宣言。
            }
        }
        
    }

    private void OnTriggerStay(Collider col)//ドットダメージを受ける（トケ、敵1）
    {
        switch (col.gameObject.tag)
        {
            case "Obstacle":
            case "Enemy":
                // すでに登録されている場合
                if (colDic.ContainsKey(col.gameObject) == true)
                {
                    if (Time.time >= colDic[col.gameObject])
                    {
                        onDamageByEnemyBody(col.gameObject);
                        colDic[col.gameObject] += getDamageInterval;
                    }
                }
                // 未登録の場合
                else
                {
                    onDamageByEnemyBody(col.gameObject);
                    colDic.Add(col.gameObject, Time.time + getDamageInterval);//登録
                    Debug.Log("<color=red>Add:" + col.gameObject.name +"</color>");
                }
                break;
            //case "Wall":
            //    Debug.Log("壁の前に戻す");
            //    Vector3 dir = this.transform.position - col.gameObject.transform.position;
            //    dir = dir.normalized;
            //    rigid.velocity = Vector3.zero;
            //    transform.position += new Vector3(dir.x * move_Speed * Time.deltaTime, 0, dir.z * move_Speed * Time.deltaTime);
            //    break;
        }
    }

    void onDamageByEnemyBody(GameObject enemy)
    {
        SGM.ins.S_GetDmg.Play();
        Destroy(Instantiate(EffectManager.ins.E_PlayerGetHit, this.gameObject.transform.position, Quaternion.identity), 2);
        curHp -= enemy.GetComponent<EnemyController>().clashDmg;
    }

    void Move()
    {
        float _moveDirZ = Input.GetAxisRaw("Vertical");
        _moveDirX = Input.GetAxisRaw("Horizontal");
        Vector3 _moveVertical = transform.forward * _moveDirZ;
        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _velocity = (_moveVertical + _moveHorizontal).normalized * move_Speed;
        
        rigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }

    void CameraViewChange(CameraMode cm)
    {
        if (!AI_active)
        {
            switch (cm)
            {
                case CameraMode.Main:
                    Camera_Controller.ins.pointofview_Txt.text = "「Z」3人称";
                    cam.enabled = true;
                    cam2.enabled = false;
                    break;

                case CameraMode.Sub:
                    Camera_Controller.ins.pointofview_Txt.text = "「Z」1人称";
                    cam.enabled = false;
                    cam2.enabled = true;
                    break;
            }
            cameraMode = cm;
        }
    }

    void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        rigid.MoveRotation(rigid.rotation * Quaternion.Euler(_characterRotationY)); 
    }

    
    void LookMousePosition()
    {
        //Ray cameraRay = cam.ScreenPointToRay(Input.mousePosition);
        //Plane GroupPlane = new Plane(Vector3.up, Vector3.zero);
        //float rayLength;
        //if (GroupPlane.Raycast(cameraRay, out rayLength))
        //{
        //    Vector3 pointTolook = cameraRay.GetPoint(rayLength);
        //    transform.LookAt(new Vector3(pointTolook.x, transform.position.y, pointTolook.z));
        //}

        //Vector3 mousePos = Input.mousePosition;
        //mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z);
        //var target = Camera.main.ScreenToWorldPoint(mousePos);
        //float dz = target.z - transform.position.z;
        //float dx = target.x - transform.position.x;
        //float angle = Mathf.Atan2(dz, dx) * Mathf.Rad2Deg;
        //transform.eulerAngles = new Vector3(0, angle, 0);
        //print("target" + target);
    }

    void ShootMissile()
    {
        if(missileCnt >= missileSpan)
        {
            SGM.ins.S_ShootMissile.Play();
            missile_Prefab.GetComponent<Missile_Script>().missileSpeed = 200;

            Destroy(Instantiate(missile_Prefab, missilePos.position, missilePos.rotation), 5);
            missileCnt = 0;
        }
    }
    //継承して再定義する物（オーバーライド）------------------------------------------------------------------------------

    //AIは「規定クラス（ここ）」から行われるので、何も書いていない関数をそのまま呼び出しちゃってしまいます。
    //それぞれの「子供クラス」に定義したメッソドを逆にここで参考した「関数ポインタ」で呼び出す必要がある。
    //protected delegate void AI_ShootBullet_HandlerFromChild();//関数ポインタ
    //protected AI_ShootBullet_HandlerFromChild shootBulletHandler;

    protected virtual void ShootBullet()//弾の発射
    {
        //ここではなし
        //shootBulletHandler();//★★各自の子供ShootBullet()メッソドを代入した関数ポインタを実行する。
        //何か、そのようにしなくても、自動的に子供のクラスが呼び出さそう。。
    }
    protected virtual void Change_ShotSpeed()//弾の周期
    {
        //ここではなし
    }
    protected virtual void Final_Skill()//必殺技
    {
        //ここではなし
    }
    //-----------------------------------------------------------------------------------------------------------------------

    protected virtual void RotorAnim(){
        rotYCnt++;
        Rotor.transform.Rotate(0, rotYCnt * Time.deltaTime, 0);
        if (rotYCnt > 3000)
            rotYCnt = 3000;
    }
}
