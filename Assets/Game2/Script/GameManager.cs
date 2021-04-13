using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager ins = null;

    Animator anim;
    public enum Gm_State { PAUSE, PLAYING, TITLE}
    public Gm_State gs;

    [Header("ゲームオーバー：順位表示")]
    public List<string> rankList = new List<string>();
    public Text gameoverText;
    public GameObject UI_gameResult;
    public Text[] ranks_Txt;
    public bool gameover_Active;

    [Header("UI_PAUSE")]
    public GameObject UI_Pause_Panel;

    [Header("HowtoPlay説明UI")]
    public GameObject howToPlay_Panel;
    public Image howtoPlayImage;
    public Sprite[] howtoplaySprites;
    [SerializeField] private int howtoplayIndex = 0;

    [Header("UIライフハート")]
    public bool dead_Active;
    public int life_HeartCnt;
    public Image[] life_HeartArr_Img = new Image[3];

    [Header("スタートゲーム")]
    public Text StartCnt_Txt;

    [Header("タイトル画面")]
    public GameObject GameTitleUI;

    [Header("チャラ選択UI")]
    public GameObject SelectTapUI;
    bool paused = true;
    public bool onSelectChara = false;
    public Button GameStart_Btn;
    public Button[] CharaCards_Btn;
    public Text[] Me_Txt;
    public Text[] AI_Txt;
    public GameObject[] Player;

    [Header("アイテム習得情報")]
    public Text atkItemCnt_Txt;
    public Text moveItemCnt_Txt;
    public Text shotSpdCnt_Txt;
    public int atkItemCnt = 1;
    public int moveItemCnt = 1;
    public int shotSpdItemCnt = 1;


    //シングルトン--------------------------------------

    private void Awake()
    {
        if (ins == null)
            ins = this;
        else if (ins != this)
            Destroy(gameObject);

        //DontDestroyOnLoad(gameObject);
    }
    //--------------------------------------------------

    void Start()
    { 
        anim = GetComponent<Animator>();

        //ライプ
        life_HeartCnt = 3;

        //アイテム習得初期化
        atkItemCnt_Txt.text = "X  1";
        moveItemCnt_Txt.text = "X  1";
        shotSpdCnt_Txt.text = "X  1";


        //テキスト表紙　
        foreach (Text t in Me_Txt){t.enabled = false;}
        foreach (Text t in AI_Txt){t.enabled = false;}

        //実行順番　タイトル　➡　選択　➡　ゲーム
        SelectTapUI.SetActive(false);
        GameTitleUI.SetActive(true);

    }

    void Update()
    {
        //ゲーム終了

        if(rankList.Count == 4 && gameover_Active)
        {
            gameover_Active = false;

            UI_gameResult.SetActive(true);
            rankList.Reverse();

            ranks_Txt[0].text = rankList[0];
            ranks_Txt[1].text = rankList[1];
            ranks_Txt[2].text = rankList[2];
            ranks_Txt[3].text = rankList[3];
        }

        //ライフ調整
        if(dead_Active == true)//Playerスクリプトでプレイヤーが死んだとき入れる
        {
            foreach(Image i in life_HeartArr_Img)
            {
                i.gameObject.SetActive(false);
            }

            for (int i = 0; i < life_HeartCnt; i++)
            {
                life_HeartArr_Img[i].gameObject.SetActive(true);
            }

            dead_Active = false;
        }

        if(gs == Gm_State.PLAYING)
        {
            //アイテム習得表示
            if (atkItemCnt >= 4) { atkItemCnt_Txt.text = "X  " + "MAX"; }
            else { atkItemCnt_Txt.text = "X  " + atkItemCnt.ToString(); }

            if (moveItemCnt >= 4) { moveItemCnt_Txt.text = "X  " + "MAX"; }
            else { moveItemCnt_Txt.text = "X  " + moveItemCnt.ToString(); }

            if (shotSpdItemCnt >= 4) { shotSpdCnt_Txt.text = "X  " + "MAX"; }
            else { shotSpdCnt_Txt.text = "X  " + shotSpdItemCnt.ToString(); }
        }

        //オプション（停止処理）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;

            if (paused)
            {
                UI_Pause_Panel.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                UI_Pause_Panel.SetActive(false);
                Time.timeScale = 1f;
            }
        }

    }

    public void OnClickBtn_SelectChara(int n)
    {
        SGM.ins.S_UI_Tap.Play();
        onSelectChara = true;

        Color gray = new Color(125, 125, 125);

        for (int i = 0; i < CharaCards_Btn.Length; i++)
        {
            if (i == n)//選択した物
            {
                //カメラ設定
                Camera_Controller.ins.Cam_Mode1List[i].enabled = true;
                Camera_Controller.ins.Cam_Mode2List[i].enabled = false;

                CharaCards_Btn[i].GetComponent<Image>().color = Color.white;
                Player[i].GetComponent<PlayerController>().AI_active = false;
            }
            else//選択しない物
            {
                //カメラ設定
                //未実装
                Camera_Controller.ins.Cam_Mode1List[i].enabled = false;
                Camera_Controller.ins.Cam_Mode2List[i].enabled = false;

                CharaCards_Btn[i].GetComponent<Image>().color = Color.gray;
                Player[i].GetComponent<PlayerController>().AI_active = true; //AIにする
                Player[i].GetComponent<PlayerController>().Think();
            }
                

            //テキスト表示
            Me_Txt[i].enabled = (i == n);
            AI_Txt[i].enabled = (i != n);
        }
    }

    public void OnClickBtn_GameStart()
    {
        SGM.ins.S_UI_Decision.Play();
        //選択しなくて、スタートを押した時、
        if (onSelectChara == false)
        {
            StartCoroutine(Check_SelectChara());
        }
        else
        {
            onSelectChara = false;
            SelectTapUI.SetActive(false);//ゲームスタート
            StartCoroutine(StartGameCount());
        }
    }

    IEnumerator StartGameCount()
    {
        StartCnt_Txt.enabled = true;
        StartCnt_Txt.text = "3";
        SGM.ins.S_UI_StartCnt.Play();
        StartCnt_Txt.GetComponent<Animator>().SetTrigger("StartGame_Cnt");
        yield return new WaitForSeconds(1);
        StartCnt_Txt.text = "2";
        SGM.ins.S_UI_StartCnt.Play();
        StartCnt_Txt.GetComponent<Animator>().SetTrigger("StartGame_Cnt");
        yield return new WaitForSeconds(1);
        StartCnt_Txt.text = "1";
        SGM.ins.S_UI_StartCnt.Play();
        StartCnt_Txt.GetComponent<Animator>().SetTrigger("StartGame_Cnt");
        yield return new WaitForSeconds(1);
        SGM.ins.S_UI_StartGame.Play();
        SGM.ins.S_BGM.Play();
        StartCnt_Txt.text = "Start";
        yield return new WaitForSeconds(1);
        StartCnt_Txt.enabled = false;
        gs = Gm_State.PLAYING;
    }

    IEnumerator Check_SelectChara()
    {
        SGM.ins.S_UI_Error.Play();
        GameStart_Btn.GetComponentInChildren<Text>().text = "選んでください!";
        yield return new WaitForSeconds(1);
        GameStart_Btn.GetComponentInChildren<Text>().text = "Game  Start";
    }

    public void OnClickBtn_TitleClick()
    {
        //タイトル画面
        if (gs == Gm_State.TITLE)
        {
            SGM.ins.S_UI_Tap.Play();
            GameTitleUI.SetActive(false);
            SelectTapUI.SetActive(true);
        }
    }

    public void OnClickBtn_ContinueGame_FromPause()
    {
        UI_Pause_Panel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnClickBtn_HowToPlay()
    {
        SGM.ins.S_UI_Decision.Play();
        if (howtoplayIndex == 0)
        {
            howToPlay_Panel.SetActive(true);
            howtoPlayImage.sprite = howtoplaySprites[0];
        }
        else if (howtoplayIndex == 1)
        {
            howtoPlayImage.sprite = howtoplaySprites[1];
        }
        else if (howtoplayIndex == 2)
        {
            howtoPlayImage.sprite = howtoplaySprites[2];
        }
        else if (howtoplayIndex == 3)
        {
            howtoPlayImage.sprite = howtoplaySprites[3];
        }
        else
        {
            howtoplayIndex = -1;
            howToPlay_Panel.SetActive(false);
            
        }
        //カウントする
        howtoplayIndex++;
    }

    public void OnClickBtn_ReGame()
    {
        Time.timeScale = 1f;
        SGM.ins.S_BGM.Stop();
        SceneManager.LoadScene("GameScene2");
    }
}
