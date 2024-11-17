using System.Data;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ClientManager : MonoBehaviour
{
    public static ClientManager _instance;
    public int clientPlayerId;

    private TcpClient client;
    private NetworkStream stream;
    public InputField NicknameField;

    private bool _clientStart = false;
    private int _playerIndex = 0;
    public Transform[] spwanPos;

    private void Awake()
    {
        _instance = this;
    }
    private void Update()
    {
        if (_clientStart)
            ReceiveMessage();
    }

    public void GotoLobby()
    {
        ConnectToServer(NicknameField.text);
    }

    public void ConnectToServer(string nickname)
    {
        try
        {
            client = new TcpClient("127.0.0.1", 7777);
            stream = client.GetStream();

            // 닉네임 전송
            byte[] data = Encoding.UTF8.GetBytes(nickname);
            stream.Write(data, 0, data.Length);

            if (!_clientStart)
                _clientStart = true;
        }
        catch (SocketException ex)
        {
            Debug.LogError($"Socket exception: {ex.Message}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception: {ex.Message}");
        }
    }

    private void ReceiveMessage()
    {
        byte[] buffer = new byte[1024];
        stream.BeginRead(buffer, 0, buffer.Length, ar =>
        {
            int bytesRead = stream.EndRead(ar);
            if (bytesRead > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (message.StartsWith("START:"))
                {
                    string idString = message.Split(':')[1];
                    clientPlayerId = int.Parse(idString);
                    Debug.Log($"Game starting! Your player ID is {clientPlayerId}");

                    for (int i = 0; i < 4; i++)
                    {
                        GameObject player = Instantiate(Resources.Load("PlayerPrefab"), spwanPos[i].position, Quaternion.identity) as GameObject; // 모르겠는데 이건
                        player.GetComponent<PlayerController>().Setup(i);
                    }
                }
            }
        }, null);
    }
}
