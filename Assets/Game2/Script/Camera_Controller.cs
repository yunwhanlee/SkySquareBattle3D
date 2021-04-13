using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Camera_Controller : MonoBehaviour
{
    public static Camera_Controller ins = null;

    [Header("3・1人称")]
    [SerializeField] public Text pointofview_Txt;

    [Header("ミニマップ")]
    [SerializeField] public Camera miniMapCam;
    public GameObject miniMap;

    //Camera
    [SerializeField] public List<Camera> Cam_Mode1List;
    [SerializeField] public List<Camera> Cam_Mode2List;

    //Player
    [SerializeField] private GameObject Player1, Player2, Player3, Player4;

    void Awake(){
        Camera.main.depth = 2;

        //シングルトン
        if (ins == null)
            ins = this;
        else if (ins != this)
            Destroy(gameObject);

        //DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        //ミニマップ
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (miniMapCam.depth == -2)
            {
                miniMapCam.depth = 2;//ON
                miniMap.transform.localPosition = new Vector3(-401, -60, 0);
                miniMap.GetComponentInChildren<Text>().text = "「T」 mini map ON";
                miniMap.GetComponentInChildren<Text>().color = Color.yellow;
            }
            else
            {
                miniMapCam.depth = -2;//OFF
                miniMap.transform.localPosition = new Vector3(-401, -366, 0);
                miniMap.GetComponentInChildren<Text>().text = "「T」 mini map OFF";
                miniMap.GetComponentInChildren<Text>().color = Color.white;
            }
        }
        //プレイヤーに当たるカメラー変換
        if (Input.GetKeyDown(KeyCode.Alpha1) && Player1.gameObject != null)
        {
            for (int i = 0; i < Cam_Mode1List.Count; i++)
            {
                if (i == 0)
                {
                    Cam_Mode1List[i].enabled = true;
                }
                else
                {
                    Cam_Mode1List[i].enabled = false;
                    Cam_Mode2List[i].enabled = false;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && Player2.gameObject != null)
        {
            for (int i = 0; i < Cam_Mode1List.Count; i++)
            {
                if (i == 1)
                {
                    Cam_Mode1List[i].enabled = true;
                }
                else
                {
                    Cam_Mode1List[i].enabled = false;
                    Cam_Mode2List[i].enabled = false;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && Player3.gameObject != null)
        {
            for (int i = 0; i < Cam_Mode1List.Count; i++)
            {
                if (i == 2)
                {
                    Cam_Mode1List[i].enabled = true;
                }
                else
                {
                    Cam_Mode1List[i].enabled = false;
                    Cam_Mode2List[i].enabled = false;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && Player4.gameObject != null)
        {
            for (int i = 0; i < Cam_Mode1List.Count; i++)
            {
                if (i == 3)
                {
                    Cam_Mode1List[i].enabled = true;
                }
                else
                {
                    Cam_Mode1List[i].enabled = false;
                    Cam_Mode2List[i].enabled = false;
                }
            }
        }

        //ズーム·イン·アウト
        //Zoom();
    }

    void Zoom()
    {
        var scroll = Input.mouseScrollDelta;
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView - scroll.y, 55f, 97f);//視野角
    }
}


