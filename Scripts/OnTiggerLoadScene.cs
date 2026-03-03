using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnTiggerLoadScene : MonoBehaviour
{
    public int sceneIndex;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Invoke("LoadScene",3f);
        }
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
