using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ServerManager : MonoBehaviour
{
    private TcpListener server;
    [SerializeField]
    private Dictionary<int, PlayerInfo> players = new Dictionary<int, PlayerInfo>();
    private int playerIdCounter = 0;
    private bool isServerRunning = false;
    private Thread serverThread;

    public Text[] UsersName;
    private int _index = 0;

    public Transform[] spwanPos;

    // 플레이어 정보 클래스
    public class PlayerInfo
    {
        public string nickname;
        public TcpClient client;
        public GameObject character;
    }

    private void Start()
    {
        // 서버 시작
        server = new TcpListener(IPAddress.Any, 7777);
        server.Start();
        isServerRunning = true;
        Debug.Log("Server started on port 7777.");

        serverThread = new Thread(HandleClientConnections);
        serverThread.Start();
    }
    

    private void HandleClientConnections()
    {
        while (isServerRunning)
        {
            if (server.Pending())
            {
                TcpClient client = server.AcceptTcpClient();
                int playerId = playerIdCounter++;
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
            if (bytesRead > 0)
            {
                string nickname = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                players[playerId].nickname = nickname;

                Debug.Log($"Player {playerId} joined with nickname: {nickname}");

                /*
                if (_index < 4)
                {
                    GameObject player = Instantiate(Resources.Load("PlayerPrefab"), spwanPos[_index], Quaternion.identity);
                    player.GetComponent<PlayerController>().Setup(_index);
                    UsersName[_index].text = nickname;
                    _index++;
                }
                */
                
                //CreatePlayerCharacter(playerId);
                //SendStartSignal(client, playerId);
            }
        }, null);
    }

    private GameObject Instantiate(UnityEngine.Object @object, Transform transform, Quaternion identity)
    {
        throw new NotImplementedException();
    }

    private void CreatePlayerCharacter(int playerId)
    {
        GameObject character = Instantiate(Resources.Load("PlayerPrefab")) as GameObject;
        character.GetComponent<PlayerController>().Setup(playerId);
        players[playerId].character = character;
        Debug.Log($"Character created for player {playerId}");
    }

    public void StartGame()
    {
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
        Debug.Log($"Sent start signal to player {playerId}");
    }

    private void OnApplicationQuit()
    {
        isServerRunning = false;
        server.Stop();
        serverThread.Abort();
    }
}
