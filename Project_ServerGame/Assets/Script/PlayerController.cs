using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int playerId;
    private bool hasControl = false;

    public void Setup(int id)
    {
        playerId = id;
        hasControl = (id == ClientManager.instance.clientPlayerId);
    }

    private void Update()
    {
        if (hasControl)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            transform.Translate(new Vector3(h, 0, v) * Time.deltaTime * 5);
        }
    }
}
