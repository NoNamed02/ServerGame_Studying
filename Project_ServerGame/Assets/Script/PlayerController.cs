using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int playerId;
    private bool hasControl = false;

    public void Setup(int id)
    {
        playerId = id;
        // 서버로부터 받은 플레이어 ID와 현재 클라이언트의 ID를 비교해 제어 여부 결정
        hasControl = (id == ClientManager.instance.clientPlayerId);
    }

    private void Update()
    {
        if (hasControl)
        {
            // 제어 권한이 있는 플레이어만 이동 가능
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            transform.Translate(new Vector3(h, 0, v) * Time.deltaTime * 5);
        }
    }
}
