using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPServer : MonoBehaviour
{
    private UdpClient udpServer;
    private IPEndPoint remoteEndPoint;
    
    [SerializeField]
    private GameObject _Player;

    private SynchronizationContext unityContext;

    void Start()
    {
        udpServer = new UdpClient(7777);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 7777);
        Debug.Log("UDP Server started.");

        unityContext = SynchronizationContext.Current;

        ReceiveData();

        _Player = GameObject.Find("Player");
    }

    void ReceiveData()
    {
        udpServer.BeginReceive(OnReceive, null);
    }

    void OnReceive(IAsyncResult result)
    {
        try
        {
            byte[] data = udpServer.EndReceive(result, ref remoteEndPoint);
            string message = Encoding.UTF8.GetString(data);
            Debug.Log($"Received: {message}");

            PlayerMovement movement = JsonUtility.FromJson<PlayerMovement>(message);
        
            Vector3 newPosition = new Vector3(movement.x, movement.y, movement.z);

            unityContext.Post(_ => UpdatePlayerPosition(newPosition), null);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error receiving data: {e.Message}");
        }

        ReceiveData();
    }

    void UpdatePlayerPosition(Vector3 newPosition)
    {
        if (_Player != null)
        {
            _Player.transform.position += newPosition;
        }
    }

    void OnDestroy()
    {
        udpServer.Close();
    }
}
