using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ClientManager : MonoBehaviour
{
    public static ClientManager instance;
    public int clientPlayerId;

    private TcpClient client;
    private NetworkStream stream;

    public InputField NicknameField;

    private void Awake()
    {
        instance = this;
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

            // 서버에서 메시지 수신 대기
            ReceiveMessage();
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
                    // 메시지에서 플레이어 ID 추출
                    string idString = message.Split(':')[1];
                    clientPlayerId = int.Parse(idString);
                    Debug.Log($"Game starting! Your player ID is {clientPlayerId}");
                }
            }
        }, null);
    }
}
