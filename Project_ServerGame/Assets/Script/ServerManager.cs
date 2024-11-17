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
    private Dictionary<int, PlayerInfo> players = new Dictionary<int, PlayerInfo>();
    private int playerIdCounter = 0;
    private bool isServerRunning = false;
    private Thread serverThread;
    public Text user;

    // 실시간 유저 목록 저장용 리스트
    private List<string> playerNicknames = new List<string>();

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

        // 별도 스레드에서 클라이언트 수락
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

                // 클라이언트로부터 닉네임을 수신
                ReceiveNickname(client, playerId);
            }
            Thread.Sleep(10); // CPU 사용량을 줄이기 위한 딜레이
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
                
                lock (playerNicknames)  // 스레드 안전하게 닉네임 추가
                {
                    playerNicknames.Add(nickname);
                }

                Debug.Log($"Player {playerId} joined with nickname: {nickname}");
            }
        }, null);
    }

    private void Update()
    {
        // 실시간으로 user 텍스트를 업데이트
        lock (playerNicknames) // 스레드 안전성 확보
        {
            user.text = "Players: " + string.Join(", ", playerNicknames);
        }
    }

    public void StartGame()
    {
        // 딕셔너리 크기만큼 캐릭터 생성 및 할당
        foreach (var player in players)
        {
            GameObject character = Instantiate(Resources.Load("PlayerPrefab")) as GameObject;
            character.GetComponent<PlayerController>().Setup(player.Key);
            player.Value.character = character;

            // 클라이언트에게 시작 신호 및 제어 권한 전송
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
