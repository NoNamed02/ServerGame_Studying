using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCheck : MonoBehaviour
{

    public GameObject [] forServer;
    public GameObject [] forClient;
    public GameObject [] SetButton;
    
    private void Awake()
    {
        for(int i = 0; i < forServer.Length; i++)
            forServer[i].SetActive(false);
        for(int i = 0; i < forClient.Length; i++)
            forClient[i].SetActive(false);
        for(int i = 0; i < SetButton.Length; i++)
            SetButton[i].SetActive(true);
    }

    public void IsServer()
    {
        for(int i = 0; i < forServer.Length; i++)
            forServer[i].SetActive(true);
        for(int i = 0; i < forClient.Length; i++)
            forClient[i].SetActive(false);
    }
    public void IsClient()
    {
        for(int i = 0; i < forServer.Length; i++)
            forServer[i].SetActive(false);
        for(int i = 0; i < forClient.Length; i++)
            forClient[i].SetActive(true);
    }
}
