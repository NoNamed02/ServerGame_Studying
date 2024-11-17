using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCheck : MonoBehaviour
{

    public GameObject [] forServer;
    public GameObject [] forClient;
    public GameObject [] SetButton;
    public GameObject [] NickName;

    private void Awake()
    {
        for(int i = 0; i < forServer.Length; i++)
            forServer[i].SetActive(false);
        for(int i = 0; i < forClient.Length; i++)
            forClient[i].SetActive(false);
        for(int i = 0; i < SetButton.Length; i++)
            SetButton[i].SetActive(true);
        NickNameSetActive(false);
    }

    public void IsServer()
    {
        for(int i = 0; i < forServer.Length; i++)
            forServer[i].SetActive(true);
        for(int i = 0; i < forClient.Length; i++)
            forClient[i].SetActive(false);
            
        NickNameSetActive(true);
    }
    public void IsClient()
    {
        for(int i = 0; i < forServer.Length; i++)
            forServer[i].SetActive(false);
        for(int i = 0; i < forClient.Length; i++)
            forClient[i].SetActive(true);
        NickNameSetActive(true);
    }
    
    private void NickNameSetActive(bool set)
    {
        for(int i = 0; i < NickName.Length; i++)
            NickName[i].SetActive(set);
    }
}
