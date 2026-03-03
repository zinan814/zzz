using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public enum PlayerType
{
    player1,
    player2,
    player3
}

public enum WeaponType
{
    weapon1,
    weapon2,
    weapon3
}

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public PlayerType playerType;


    private void Awake()
    {
        instance = this;
    }

    public void SetPlayerType1()
    {
        playerType = PlayerType.player1;
    }
    
    public void SetPlayerType2()
    {
        playerType = PlayerType.player2;
    }
    
    public void SetPlayerType3()
    {
        playerType = PlayerType.player3;
    }
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
