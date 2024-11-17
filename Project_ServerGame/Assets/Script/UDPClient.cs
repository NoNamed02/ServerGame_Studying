using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPClient : MonoBehaviour
{
    private UdpClient udpClient;
    private IPEndPoint serverEndPoint;

    private GameObject _Player;

    void Start()
    {
        udpClient = new UdpClient();
        serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);
        Debug.Log("UDP Client started.");
        _Player = GameObject.Find("Player");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Vector3 position = new Vector3(0, 0, 1);
            SendMovement(position);
            _Player.transform.position += position;
        }
    }

    void SendMovement(Vector3 position)
    {
        PlayerMovement movement = new PlayerMovement(position.x, position.y, position.z);
        string json = JsonUtility.ToJson(movement);
        byte[] data = Encoding.UTF8.GetBytes(json);
        udpClient.Send(data, data.Length, serverEndPoint);
        Debug.Log($"Sent: {json}");
    }

    void OnDestroy()
    {
        udpClient.Close();
    }
}
