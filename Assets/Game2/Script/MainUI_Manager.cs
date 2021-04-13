using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI_Manager : MonoBehaviour
{
    [SerializeField] private GameObject[] player;
    [SerializeField] private Text[] playerLife;
    void Start()
    {
        
    }

    void Update()
    {
        for(int i=0;i<playerLife.Length;i++)
        {
            if(player[i] != null)
            {
                playerLife[i].text =
                "P" + (i + 1).ToString() + " : "
                + "<color=#00ff00>" + player[i].gameObject.GetComponentInParent<PlayerController>().life.ToString() + "</color>"
                + "         "
                + "<color=#ff0000>" + player[i].gameObject.GetComponentInParent<PlayerController>().curHp + "</color>";
            }
        }
    }

}
