using UnityEditor;
using UnityEngine;

public class Food : MonoBehaviour
{
    public void Start()
    {
        Destroy(gameObject, 20f); // Destroy food after 20 seconds
    }
}
