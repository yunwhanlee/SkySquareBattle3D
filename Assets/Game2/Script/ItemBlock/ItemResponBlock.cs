using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemResponBlock : MonoBehaviour
{
    [Header("アイテム種類")]
    [SerializeField] GameObject[] items;
    [SerializeField] int span;//アイテムが生成する周期
    public float cnt = 0;

    //アイテムあるかないか確認（重なり防止）
    public GameObject item_Effect;
    void Start()
    {
        item_Effect.SetActive(false);
    }

    void Update()
    {
        cnt += Time.deltaTime;
        if(cnt > span && item_Effect.activeSelf == false)
        {
            cnt = 0;
            item_Effect.SetActive(true);

            //アイテム生成
            int rand = Random.Range(0, items.Length);
            Vector3 pos = new Vector3(transform.position.x, transform.position.y + 70, transform.position.z);
            Instantiate(items[rand], pos, Quaternion.identity);
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            cnt = 0;
            item_Effect.SetActive(false);
        }
    }
}
