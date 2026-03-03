using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GameManger : MonoBehaviour
{
    public static GameManger instance;
    
    public GameObject[] players;
    public GameObject[] targets;
    public CinemachineFreeLook  freeLook;
    
    public WeaponType weaponType;
    
    
    
    private void Awake()
    {
        instance = this;
        
        switch (GameController.instance.playerType)
        {
            case PlayerType.player1:
                players[0].gameObject.SetActive(true);
                freeLook.Follow = targets[0].transform;
                freeLook.LookAt = targets[0].transform;
                break;
            case PlayerType.player2:
                players[1].gameObject.SetActive(true);
                freeLook.Follow = targets[1].transform;
                freeLook.LookAt = targets[1].transform;
                break;
            case PlayerType.player3:
                players[2].gameObject.SetActive(true);
                freeLook.Follow = targets[2].transform;
                freeLook.LookAt = targets[2].transform;
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ReleaseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }*/
}
