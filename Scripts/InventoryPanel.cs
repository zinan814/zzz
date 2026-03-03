using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : MonoBehaviour
{
    public Button weapon1;

    public Button weapon2;

    public Button weapon3;

    public Button weapon4;

    public Button weapon5;
    public Button weapon6;

    public Button hpBtn;
    
    public GameObject[] weapons;

    public Text hpCpunt;
    // Start is called before the first frame update
    void Start()
    {
        switch (GameController.instance.playerType)
        {
            case PlayerType.player1:
                weapons = GameObject.FindGameObjectsWithTag("Player1Weapons");
                UpdateWeapon(0);
                break;

            case PlayerType.player2:
                weapons = GameObject.FindGameObjectsWithTag("Player2Weapons");
                UpdateWeapon(0);
                break;

            case PlayerType.player3:
                weapons = GameObject.FindGameObjectsWithTag("Player3Weapons");
                UpdateWeapon(0);
                break;
        }

        weapon1.onClick.AddListener(() => { UpdateWeapon(0); });
        weapon2.onClick.AddListener(() => { UpdateWeapon(1); });
        weapon3.onClick.AddListener(() => { UpdateWeapon(2); });
        weapon4.onClick.AddListener(() => { UpdateWeapon(3); });

        if (weapon5 != null)
        {
            weapon5.onClick.AddListener(() => { UpdateWeapon(4);});
        }
        if (weapon6 != null)
        {
            weapon6.onClick.AddListener(() => { UpdateWeapon(5);});
        }

        hpBtn.onClick.AddListener(() =>
        {
            if (ScoreManger.instance.hpCount > 0)
            {
                Player player = GameObject.FindObjectOfType<Player>();
                player.AddHp(25);
                ScoreManger.instance.hpCount--;
                if (ScoreManger.instance.hpCount <= 0)
                {
                    ScoreManger.instance.hpCount = 0;
                }

                hpCpunt.text = ScoreManger.instance.hpCount.ToString();
            }

           
            
        });
    this.gameObject.SetActive(false);
    }

    public void UpdateWeapon(int index)
    {
        foreach (var item in weapons)
        {
            item.gameObject.SetActive(false);
        }
        
        weapons[index].gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        hpCpunt.text = ScoreManger.instance.hpCount.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
