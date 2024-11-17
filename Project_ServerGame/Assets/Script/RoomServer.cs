using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections;
using TMPro;

public class RoomServer : MonoBehaviour
{
    private TcpListener server;
    [SerializeField]
    private Dictionary<int, PlayerInfo> players = new Dictionary<int, PlayerInfo>();

    private Thread serverThread;
    private int _playerCount = 0;
    private bool isServerRunning = false;

    public TextMeshProUGUI[] UsersName;
    public Transform[] spwanPos;

    public GameObject[] RoomUI;

    public class PlayerInfo
    {
        public string nickname;
        public TcpClient client;
        public GameObject character;
    }

    void Awake()
    {
        server = new TcpListener(IPAddress.Any, 7777);
        server.Start();
        isServerRunning = true;
        Debug.Log("Server started on port 7777.");

        serverThread = new Thread(HandleClientConnections);
        serverThread.Start();
    }


    void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            if (UsersName[i].text != "Empty")
            {
                //UsersName[i].text = players[i].nickname;
                UsersName[i].SetText(players[i].nickname);
                Debug.Log(i);
            }
        }
    }

    private void HandleClientConnections()
    {
        while (isServerRunning)
        {
            if (server.Pending())
            {
                TcpClient client = server.AcceptTcpClient();
                int playerId = _playerCount++;
                players.Add(playerId, new PlayerInfo { client = client });

                Debug.Log($"Client connected with ID: {playerId}");

                ReceiveNickname(client, playerId);
            }
            Thread.Sleep(10);
        }
    }

    private void ReceiveNickname(TcpClient client, int playerId)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        stream.BeginRead(buffer, 0, buffer.Length, ar =>
        {
            int bytesRead = stream.EndRead(ar);
            string name = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            players[playerId].nickname = name;

            UsersName[playerId].SetText(players[playerId].nickname);

            Debug.Log($"Player {playerId} joined with nickname: {name}");

            BroadcastMessage($"Player {name} has joined the game!");

            // SetNickName 코루틴 실행
            if (!IsInvoking(nameof(SetNickName)))
            {
                StartCoroutine(SetNickName());
            }
        }, null);
    }
    IEnumerator SetNickName()
    {
        while (true)
        {
            foreach (var player in players)
            {
                try
                {
                    if (player.Value.client.Connected)
                    {
                        NetworkStream stream = player.Value.client.GetStream();
                        string message = $"NAME:{player.Key}:{player.Value.nickname}:   ";
                        byte[] data = Encoding.UTF8.GetBytes(message);
                        stream.Write(data, 0, data.Length);
                        BroadcastMessage(message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error sending message to player {player.Key}: {ex.Message}");
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// 모든 플레이어에게 메시지를 브로드캐스트하는 함수
    /// </summary>
    /// <param name="message">브로드캐스트할 메시지</param>
    public void BroadcastMessage(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message + "\n");

        lock (players)
        {
            foreach (var player in players.Values)
            {
                try
                {
                    if (player.client.Connected)
                    {
                        NetworkStream stream = player.client.GetStream();
                        stream.Write(data, 0, data.Length);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error sending message to {player.nickname}: {ex.Message}");
                }
            }
        }

        Debug.Log($"Broadcasted message: {message}");
    }

    public void StartGame()
    {
        foreach (var UI in RoomUI)
        {
            UI.SetActive(false);
        }
        foreach (var player in players)
        {
            GameObject character = Instantiate(Resources.Load("PlayerPrefab"), spwanPos[player.Key].position, Quaternion.identity) as GameObject;
            character.GetComponent<PlayerController>().Setup(player.Key);
            player.Value.character = character;

            SendStartSignal(player.Value.client, player.Key);
        }
    }
    private void SendStartSignal(TcpClient client, int playerId)
    {
        NetworkStream stream = client.GetStream();
        string message = "START:" + playerId;
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }
}
