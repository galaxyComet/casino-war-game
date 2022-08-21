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
public class PokerControll : MonoBehaviour 
{
    private WinControll winControll;
    private TieControll tieControll;
    private GameManager gameManager;
    public int loop = 0;
    public int betloop = 0;
    public bool clickAble = true;
    public Transform Poker;
    public Transform prefab;
    private Transform PokerPieces;
    public TMP_Text everyBetAmountText;
    public int everyBetAmount = 5;

    public int WinValue = 0;
    public TMP_Text WinValueText;
    public int TieValue = 0;
    public TMP_Text TieValueText;
    public Color betColor = Color.black;
    // Start is called before the first frame update
    void Start()
    {
        winControll = FindObjectOfType<WinControll>();
        gameManager = FindObjectOfType<GameManager>();
    }
    // Update is called once per frame
    void Update(){
    }
    public void OnMouseDown(){
        switch (loop)
        {
            case 0:
                Poker.GetComponent<MeshRenderer>().materials[0].color = Color.white;
                Poker.GetComponent<MeshRenderer>().materials[1].color = Color.cyan;
                everyBetAmount = 10;
                betColor = Color.cyan;
                loop = loop + 1;
                break;
            case 1:
                Poker.GetComponent<MeshRenderer>().materials[0].color = Color.white;
                Poker.GetComponent<MeshRenderer>().materials[1].color = Color.magenta;
                everyBetAmount = 15;
                betColor = Color.magenta;
                loop = loop + 1;
                break;
            case 2:
                Poker.GetComponent<MeshRenderer>().materials[0].color = Color.white;
                Poker.GetComponent<MeshRenderer>().materials[1].color = Color.yellow;
                everyBetAmount = 20;
                betColor = Color.yellow;
                loop = loop + 1;
                break;
            case 3:
                Poker.GetComponent<MeshRenderer>().materials[0].color = Color.white;
                Poker.GetComponent<MeshRenderer>().materials[1].color = Color.blue;
                betColor = Color.blue;
                everyBetAmount = 25;
                loop = loop + 1;
                break;
            case 4:
                Poker.GetComponent<MeshRenderer>().materials[0].color = Color.white;
                Poker.GetComponent<MeshRenderer>().materials[1].color = Color.green;
                betColor = Color.green;
                everyBetAmount = 50;
                loop = loop + 1;
                break;
            case 5:
                Poker.GetComponent<MeshRenderer>().materials[0].color = Color.white;
                Poker.GetComponent<MeshRenderer>().materials[1].color = Color.red;
                everyBetAmount = 100;
                betColor = Color.red;
                loop = loop + 1;
                break;
            case 6:
                Poker.GetComponent<MeshRenderer>().materials[0].color = Color.white;
                Poker.GetComponent<MeshRenderer>().materials[1].color = Color.black;
                everyBetAmount = 5;
                betColor = Color.black;
                loop = 0;
                break;
        }
        everyBetAmountText.text = everyBetAmount.ToString();
    }
    public IEnumerator pokerOder(float x,float y,float z,string name, int n){
        if (name == "winPoker")
        {
            WinValue = WinValue + everyBetAmount;
            WinValueText.text = WinValue.ToString();
        }
        else
        {
            TieValue = TieValue + everyBetAmount;
            TieValueText.text = TieValue.ToString();
        }
        PokerPieces = Instantiate(prefab, new Vector3(1676.978f, -306.288f, -883.5956f), Quaternion.identity);
        PokerPieces.name = name + n;
        PokerPieces.GetComponent<MeshRenderer>().materials[0].color = Color.white;
        PokerPieces.GetComponent<MeshRenderer>().materials[1].color = Poker.GetComponent<MeshRenderer>().materials[1].color;
        PokerPieces.transform.GetChild(0).GetComponent<TMP_Text>().text = everyBetAmount.ToString();
        PokerPieces.transform.localScale = new Vector3(5f, 5f, 1f);
        const float seconds = 0.3f;
        float time = 0;
        float yy = y + (0.0083f * (n - 1));
        while (time < seconds)
        {
            PokerPieces.transform.position = Vector3.Lerp(new Vector3(1676.978f, -306.288f, -883.5956f),
                new Vector3(x, yy, z), time / seconds);
            PokerPieces.transform.localScale = new Vector3(5f, 5f, 1f);
            PokerPieces.transform.rotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(-128.999f, 0, 0)), Quaternion.Euler(new Vector3(-90, 0, 0)), time / seconds);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        PokerPieces.transform.position = new Vector3(x, yy, z);
        PokerPieces.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
        yield return new WaitForSeconds(0.0001f);
        clickAble = true;
    }
}
