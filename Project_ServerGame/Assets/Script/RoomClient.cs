using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomClient : MonoBehaviour
{
    public int clientPlayerId; // 클라이언트의 ID

    private TcpClient client;
    private NetworkStream stream;
    private bool _isConnect = false;

    public InputField NicknameField; // 닉네임 입력 필드
    public TextMeshProUGUI[] UsersName; // UI에 표시할 플레이어 닉네임 리스트
    public Text Log;

    private byte[] buffer = new byte[1024];
    private string _pendingMessage = ""; // Update에서 처리할 메시지
    private readonly object _lock = new object(); // 스레드 간 동기화를 위한 객체

    void Update()
    {
        lock (_lock) // 스레드 간 동기화
        {
            if (!string.IsNullOrEmpty(_pendingMessage))
            {
                ProcessServerMessage(_pendingMessage);
                _pendingMessage = ""; // 처리 후 초기화
            }
        }
    }

    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 7777);
            stream = client.GetStream();
            _isConnect = true;

            string name = NicknameField.text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("Nickname cannot be empty.");
                return;
            }

            byte[] data = Encoding.UTF8.GetBytes(name + "\n");
            stream.Write(data, 0, data.Length);

            //UsersName[0].text = name;
            Debug.Log("Connected to server and sent nickname.");

            StartReceiving();
        }
        catch (SocketException ex)
        {
            Debug.LogError($"Socket exception: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception: {ex.Message}");
        }
    }

    private void StartReceiving()
    {
        if (stream != null && stream.CanRead)
        {
            stream.BeginRead(buffer, 0, buffer.Length, OnMessageReceived, null);
        }
        else
        {
            Debug.LogError("Stream is not available for reading.");
        }
    }

    // 서버 메시지를 처리하는 콜백
    private void OnMessageReceived(IAsyncResult ar)
    {
        try
        {
            int bytesRead = stream.EndRead(ar);
            if (bytesRead > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Debug.Log($"Message from server: {message}");
                lock (_lock)
                {
                    _pendingMessage = message; // 스레드 안전하게 저장
                }

                StartReceiving(); // 다음 메시지 대기
            }
            else // 서버가 종료된 경우
            {
                Debug.LogWarning("Server has closed the connection.");
                Disconnect();
            }
        }
        catch (SocketException ex)
        {
            Debug.LogError($"Socket exception: {ex.Message}");
            Disconnect();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception: {ex.Message}");
            Disconnect();
        }
    }

    private void ProcessServerMessage(string message)
    {
        Debug.Log($"Raw message: {message}");
        string[] parts = message.Split(':');

        parts[0] = parts[0].Trim();
        parts[1] = parts[1].Trim();
        parts[2] = parts[2].Trim();

        Debug.Log(parts[0] + " " + parts[1] + " " + parts[2]);

        if (int.TryParse(parts[1], out int playerId))
        {
            UsersName[playerId].SetText(parts[2]);
        }
        else
        {
            Debug.LogWarning($"Invalid player ID or out of range: {parts[1]}");
        }
    }

    private void Disconnect()
    {
        _isConnect = false;

        if (stream != null)
        {
            stream.Close();
            stream = null;
        }

        if (client != null)
        {
            client.Close();
            client = null;
        }

        Debug.Log("Disconnected from server.");
    }
}
