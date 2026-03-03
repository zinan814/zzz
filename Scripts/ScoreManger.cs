using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManger : MonoBehaviour
{
    
    public static ScoreManger instance;
    public int score;
    
    public Text scoreText;


    public int currentKill =0;
    public int totalKills = 3;
    
    public GameObject Boss;
    
    public int hpCount = 3;
    
    private void Awake()
    {
        instance = this;
    }

    public void AddScore()
    {
        score ++;
        scoreText.text = "Number of key： " + score.ToString() +"/" +totalKills.ToString();
    }

    public void AddKill()
    {
        currentKill++;
        if (currentKill == totalKills)
        {
            //GameManger.instance.ReleaseCursor();
            Boss.gameObject.SetActive(true);
        }
    }
    
}
