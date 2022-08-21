using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TieControll : MonoBehaviour
{
    private PokerControll pokerControll;
    private GameManager gameManager;
    public int loop = 0;
    // Start is called before the first frame update
    void Start()
    {
        pokerControll = FindObjectOfType<PokerControll>();
        gameManager = FindObjectOfType<GameManager>();
    }
    // Update is called once per frame
    void Update(){
    }
    public void OnMouseDown(){
        if (gameManager.clickflag){
            if (pokerControll.clickAble)
            {
                if (pokerControll.TieValue + pokerControll.everyBetAmount <= 100)
                {
                    loop = loop + 1;
                    pokerControll.clickAble = false;
                    StartCoroutine(pokerControll.pokerOder(1677.109f, -305.98f, -882.998f, "tiePoker", loop));
                }
            }
        }
    }
}
