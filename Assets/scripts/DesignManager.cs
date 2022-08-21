using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Timers;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine.SceneManagement;
using SimpleJSON;
public class DesignManager : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("material")]
    private Transform CardObject;
    public Material[] cardMaterial;
    public Transform prefab;
    private GameManager gameManager;
    public static APIForm apiform;
    public static Globalinitial _global;
    private float[] cardX;
    private float cardY = -305.937f;
    private float[] cardZ;
    private float[] movecardX = new float[4] { 1676.983f, 1676.981f, 1677.015f, 1677.015f };
    private float[] movecardY = new float[4] { -305.998f, -305.998f, -305.997f, -305.997f };
    private float[] movecardZ = new float[4] { -883.2341f, -882.7685f, -883.2341f, -882.7685f };
    public int[] cardOrderArray;
    void Start()
    {
        cardX = new float[4];
        cardZ = new float[4];
        gameManager = FindObjectOfType<GameManager>();
    }

    public IEnumerator CardOder()
    {
        const float seconds = 0.1f;
        float time = 0;
        float beforeX = 1677.625f;
        float beforeZ = -882.7498f;
        cardX[0] = beforeX;
        while (time < seconds)
        {
            for (int i = 0; i < 52; i++)
            {
                float nextX = beforeX + 0.004f * i;
                float nextZ = beforeZ + 0.0024f * i;
                CardObject = Instantiate(prefab, Vector3.Lerp(new Vector3(beforeX, cardY, nextZ), new Vector3(nextX, cardY, nextZ), time / seconds), Quaternion.identity);
                if (i >= 0 && i < 4)
                {
                    cardX[i] = nextX;
                    cardZ[i] = nextZ;
                }
                CardObject.name = "card" + (i + 1);
                CardObject.GetComponent<SpriteRenderer>().material = cardMaterial[cardOrderArray[i]];
                CardObject.transform.localScale = new Vector3(0.008662499f, -0.008357409f, 0.65701f);
                CardObject.transform.eulerAngles = new Vector3(30, 60, 90);
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
        yield return new WaitForSeconds(1f);
    }
    // Update is called once per frame
    void Update()
    {

    }
    public IEnumerator ThrowedCardClear(bool flag)
    {
        for (int i = 0; i < 52; i++)
        {
            string name = "card" + (i + 1);
            Destroy(GameObject.Find(name));
        }
        StartCoroutine(gameManager.firstServer());
        yield return new WaitForSeconds(1.5f);
        if (flag)
        {
            StartCoroutine(CardThrow(0, 2));
            StartCoroutine(gameManager.beginServer());
        }
    }
    public IEnumerator CardThrow(int from, int to)
    {
        for (int i = from; i < to; i++)
        {
            float time = 0;
            const float seconds = 0.15f;
            string name = "card" + (i + 1);
            while (time < seconds)
            {
                GameObject.Find(name).transform.position = Vector3.Lerp(new Vector3(cardX[i], cardY, cardZ[i]), new Vector3(movecardX[i], movecardY[i], movecardZ[i]), time / seconds);
                GameObject.Find(name).transform.rotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(30, 90, 90)), Quaternion.Euler(new Vector3(-90, 90, 90)), time / seconds);
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            GameObject.Find(name).transform.position = new Vector3(movecardX[i], movecardY[i], movecardZ[i]);
            GameObject.Find(name).transform.rotation = Quaternion.Euler(new Vector3(-90, 90, 90));
            yield return new WaitForSeconds(0.1f);
        }
    }
}
