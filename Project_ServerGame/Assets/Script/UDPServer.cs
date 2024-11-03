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

    // SynchronizationContext를 사용해 메인 스레드로 작업을 전달하기 위한 변수
    private SynchronizationContext unityContext;

    void Start()
    {
        udpServer = new UdpClient(7777);  // 서버 포트 번호
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 7777);
        Debug.Log("UDP Server started.");

        // 현재 메인 스레드의 SynchronizationContext 저장
        unityContext = SynchronizationContext.Current;

        ReceiveData();  // 데이터 수신 시작

        _Player = GameObject.Find("Player");  // Player 오브젝트 찾기
    }

    void ReceiveData()
    {
        udpServer.BeginReceive(OnReceive, null);  // 비동기로 데이터 수신 시작
    }

    void OnReceive(IAsyncResult result)
    {
        try
        {
            byte[] data = udpServer.EndReceive(result, ref remoteEndPoint);  // 데이터 수신 완료
            string message = Encoding.UTF8.GetString(data);
            Debug.Log($"Received: {message}");

            // 수신한 데이터를 PlayerMovement로 역직렬화
            PlayerMovement movement = JsonUtility.FromJson<PlayerMovement>(message);
        
            Vector3 newPosition = new Vector3(movement.x, movement.y, movement.z);

            // 메인 스레드에서 UpdatePlayerPosition 실행
            unityContext.Post(_ => UpdatePlayerPosition(newPosition), null);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error receiving data: {e.Message}");
        }

        ReceiveData();  // 다시 수신 대기 상태로 돌아감
    }

    // 메인 스레드에서 실행될 메서드
    void UpdatePlayerPosition(Vector3 newPosition)
    {
        if (_Player != null)
        {
            _Player.transform.position += newPosition;  // 플레이어 위치 업데이트
        }
    }

    void OnDestroy()
    {
        udpServer.Close();  // 서버 종료 시 소켓 닫기
    }
}
