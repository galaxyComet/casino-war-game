using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Timers;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;
using SimpleJSON;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //Start is called before the first frame update
    private DesignManager designManager;
    private PokerControll pokerControll;
    private TieControll tieControll;
    private WinControll winControll;
    public TMP_Text totalPriceText;
    private float totalValue;
    public Button betbtn;
    private string betbtnflag = "Deal";
    public Button foldbtn;
    private string foldbtnflag = "Clear";
    public bool clickflag = true;
    public int clickNum = 0;

    public static APIForm apiform;
    public static Globalinitial _global;
    [DllImport("__Internal")]
    private static extern void GameReady(string msg);
    BetPlayer _player;
    public void RequestToken(string data)
    {
        JSONNode usersInfo = JSON.Parse(data);
        _player.token = usersInfo["token"];
        _player.username = usersInfo["userName"];
        float i_balance = float.Parse(usersInfo["amount"]);
        totalValue = i_balance;
        totalPriceText.text = totalValue.ToString("F2");
    }
    void Start()
    {
        _player = new BetPlayer();
#if UNITY_WEBGL == true && UNITY_EDITOR == false
                            GameReady("Ready");
#endif
        StartCoroutine(firstServer());
        designManager = FindObjectOfType<DesignManager>();
        pokerControll = FindObjectOfType<PokerControll>();
        tieControll = FindObjectOfType<TieControll>();
        winControll = FindObjectOfType<WinControll>();
        betbtn.interactable = true;
        foldbtn.interactable = true;
    }
    // Update is called once per frame
    void Update()
    {
    }
    private void BetOrRebet()
    {
        if (pokerControll.WinValue == 0)
        {
            StartCoroutine(alert("Set WinBalance!", "other"));
        }
        else
        {
            if (totalValue >= pokerControll.WinValue)
            {
                if (totalValue >= 5)
                {
                    betbtn.interactable = false;
                    foldbtn.interactable = false;
                    clickflag = false;
                    switch (betbtnflag)
                    {
                        case "Deal":
                            StartCoroutine(UpdateCoinsAmount(totalValue, totalValue - pokerControll.TieValue - pokerControll.WinValue));
                            StartCoroutine(designManager.CardThrow(0, 2));
                            StartCoroutine(beginServer());
                            break;
                        case "Repeat":
                            StartCoroutine(UpdateCoinsAmount(totalValue, totalValue - pokerControll.TieValue - pokerControll.WinValue));
                            StartCoroutine(BetClear());
                            break;
                        case "Go To War":
                            betbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Repeat";
                            betbtnflag = "Repeat";
                            foldbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Clear";
                            foldbtnflag = "Clear";
                            StartCoroutine(designManager.CardThrow(2, 4));
                            StartCoroutine(UpdateCoinsAmount(totalValue, totalValue - pokerControll.WinValue));
                            StartCoroutine(GotowarServer());
                            break;
                    }
                }
                else
                {
                    StartCoroutine(alert("Insufficient balance!", "other"));
                }
            }
            else
            {
                StartCoroutine(alert("Insufficient balance!", "other"));
            }
        }
    }
    private void NewOrFold()
    {
        switch (foldbtnflag)
        {
            case "Clear":
                StartCoroutine(AllClear());
                break;
            case "Surrender":
                StartCoroutine(SurrenderServer());
                break;
        }
    }
    public IEnumerator firstServer()
    {
        yield return new WaitForSeconds(0.5f);
        WWWForm form = new WWWForm();
        form.AddField("userName", _player.username);
        form.AddField("token", _player.token);
        _global = new Globalinitial();
        UnityWebRequest www = UnityWebRequest.Post(_global.BaseUrl + "api/CardOder", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<APIForm>(strdata);
            if (apiform.serverMsg == "Success")
            {
                designManager.cardOrderArray = apiform.cardOder;
                yield return new WaitForSeconds(0.0001f);
                StartCoroutine(designManager.CardOder());
            }
            else
            {
                StartCoroutine(alert(apiform.serverMsg, "other"));
            }
        }
        else
        {
            StartCoroutine(alert("Can't find server!", "other"));
        }
    }
    public IEnumerator beginServer()
    {
        yield return new WaitForSeconds(1f);
        WWWForm form = new WWWForm();
        form.AddField("userName", _player.username);
        form.AddField("token", _player.token);
        _global = new Globalinitial();
        UnityWebRequest www = UnityWebRequest.Post(_global.BaseUrl + "api/bet-casinoWar", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<APIForm>(strdata);
            if (apiform.serverMsg == "Success")
            {
                StartCoroutine(Server(apiform.check));
                if (apiform.check == 0)
                {
                    betbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Go To War";
                    betbtnflag = "Go To War";
                    foldbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Surrender";
                    foldbtnflag = "Surrender";
                }
                else
                {
                    betbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Repeat";
                    betbtnflag = "Repeat";
                    foldbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Clear";
                    foldbtnflag = "Clear";
                }
            }
            else
            {
                StartCoroutine(alert(apiform.serverMsg, "other"));
                StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + pokerControll.WinValue + pokerControll.TieValue));
            }
        }
        else
        {
            StartCoroutine(alert("Can't find server!", "other"));
            StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + pokerControll.WinValue + pokerControll.TieValue));
        }
    }
    IEnumerator Server(int flag)
    {
        yield return new WaitForSeconds(0.5f);
        WWWForm form = new WWWForm();
        form.AddField("userName", _player.username);
        form.AddField("token", _player.token);
        form.AddField("winAmount", pokerControll.WinValue.ToString());
        form.AddField("tieAmount", pokerControll.TieValue.ToString());
        form.AddField("result", flag.ToString());
        form.AddField("amount", totalValue.ToString("F2"));
        _global = new Globalinitial();
        UnityWebRequest www = UnityWebRequest.Post(_global.BaseUrl + "api/result-CasinoWar", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<APIForm>(strdata);
            if (apiform.serverMsg == "Success")
            {
                if (apiform.check == 0 || apiform.check == 1)
                {
                    StartCoroutine(alert(apiform.msg, "win"));
                    StartCoroutine(UpdateCoinsAmount(totalValue, apiform.total));
                }
                else
                {
                    StartCoroutine(alert(apiform.msg, "lose"));
                }
            }
            else
            {
                StartCoroutine(alert(apiform.serverMsg, "other"));
                StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + pokerControll.WinValue + pokerControll.TieValue));
            }
            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            StartCoroutine(alert("Can't find server!", "other"));
            StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + pokerControll.WinValue + pokerControll.TieValue));
        }
    }
    IEnumerator GotowarServer()
    {
        yield return new WaitForSeconds(0.5f);
        WWWForm form = new WWWForm();
        form.AddField("userName", _player.username);
        form.AddField("token", _player.token);
        form.AddField("winAmount", pokerControll.WinValue.ToString());
        form.AddField("amount", totalValue.ToString("F2"));
        _global = new Globalinitial();
        UnityWebRequest www = UnityWebRequest.Post(_global.BaseUrl + "api/goto-CasinoWar", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<APIForm>(strdata);
            if (apiform.serverMsg == "Success")
            {
                if (apiform.raisePrice == 0)
                {
                    StartCoroutine(alert(apiform.msg, "lose"));
                }
                else
                {
                    StartCoroutine(alert(apiform.msg, "win"));
                    StartCoroutine(UpdateCoinsAmount(totalValue, apiform.total));
                }
            }
            else
            {
                StartCoroutine(alert(apiform.serverMsg, "other"));
                StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + pokerControll.WinValue));
            }
        }
        else
        {
            StartCoroutine(alert("Can't find server!", "other"));
            StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + pokerControll.WinValue));
        }
    }
    IEnumerator SurrenderServer()
    {
        yield return new WaitForSeconds(0.5f);
        WWWForm form = new WWWForm();
        form.AddField("userName", _player.username);
        form.AddField("token", _player.token);
        form.AddField("winAmount", pokerControll.WinValue.ToString());
        form.AddField("amount", totalValue.ToString("F2"));
        _global = new Globalinitial();
        UnityWebRequest www = UnityWebRequest.Post(_global.BaseUrl + "api/surrender-CasinoWar", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<APIForm>(strdata);
            if (apiform.serverMsg == "Success")
            {
                StartCoroutine(UpdateCoinsAmount(totalValue, apiform.total));
                StartCoroutine(alert(apiform.msg, "other"));
                yield return new WaitForSeconds(1f);
                betbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Repeat";
                betbtnflag = "Repeat";
                foldbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Clear";
                foldbtnflag = "Clear";
            }
            else
            {
                StartCoroutine(alert(apiform.serverMsg, "other"));
                StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + pokerControll.WinValue));
            }
        }
        else
        {
            StartCoroutine(alert("Can't find server!", "other"));
            StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + pokerControll.WinValue));
        }
    }
    IEnumerator AllClear()
    {
        betbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Deal";
        betbtnflag = "Deal";
        foldbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Clear";
        foldbtnflag = "Clear";
        clickflag = true;
        pokerControll.WinValue = 0;
        pokerControll.TieValue = 0;
        pokerControll.WinValueText.text = pokerControll.WinValue.ToString();
        pokerControll.TieValueText.text = pokerControll.TieValue.ToString();
        for (int i = 0; i < tieControll.loop; i++)
        {
            string name = "tiePoker" + (i + 1);
            Destroy(GameObject.Find(name));
        }
        tieControll.loop = 0;
        for (int i = 0; i < winControll.loop; i++)
        {
            string name = "winPoker" + (i + 1);
            Destroy(GameObject.Find(name));
        }
        winControll.loop = 0;
        StartCoroutine(designManager.ThrowedCardClear(false));
        yield return new WaitForSeconds(0.1f);
    }
    IEnumerator BetClear()
    {
        betbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Repeat";
        betbtnflag = "Repeat";
        foldbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Clear";
        foldbtnflag = "Clear";
        StartCoroutine(designManager.ThrowedCardClear(true));
        yield return new WaitForSeconds(1f);
    }
    IEnumerator UpdateCoinsAmount(float preValue, float changeValue)
    {
        // Animation for increasing and decreasing of coins amount
        const float seconds = 0.2f;
        float elapsedTime = 0;
        while (elapsedTime < seconds)
        {
            totalPriceText.text = Mathf.Floor(Mathf.Lerp(preValue, changeValue, (elapsedTime / seconds))).ToString();
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        totalValue = changeValue;
        totalPriceText.text = totalValue.ToString();
    }
    public IEnumerator alert(string msg, string state)
    {
        if (state == "win")
        {
            AlertController.isWin = true;
        }
        else
        {
            AlertController.isLose = true;
        }
        GameObject.Find("alert").GetComponent<TMP_Text>().text = msg;
        yield return new WaitForSeconds(3f);
        AlertController.isWin = false;
        AlertController.isLose = false;
        yield return new WaitForSeconds(1.5f);
        betbtn.interactable = true;
        foldbtn.interactable = true;
    }
}
public class BetPlayer
{
    public string username;
    public string token;
}