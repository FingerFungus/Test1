using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Assign all enemies in this room")]
    public GameObject[] enemies;

    [Header("Assign all doors to open")]
    public GameObject[] doors;

    private bool doorsOpened = false;

    void Update()
    {
        if (!doorsOpened && AllEnemiesDead())
        {
            OpenDoors();
            doorsOpened = true;
        }
    }

    bool AllEnemiesDead()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null) // still alive
                return false;
        }
        return true;
    }

    void OpenDoors()
    {
        foreach (GameObject door in doors)
        {
            door.SetActive(false); // or play animation, or enable collider, etc.
        }
    }
}
