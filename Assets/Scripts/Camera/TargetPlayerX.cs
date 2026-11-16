using UnityEngine;

public class TargetPlayerX : MonoBehaviour
{
    public Transform Player;

    void Update()
    {
        transform.position = new Vector3(Player.position.x, transform.position.y, transform.position.z);
    }
}
