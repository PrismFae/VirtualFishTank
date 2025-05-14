using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;

public class BoidSimulationControl : MonoBehaviour
{
    public GameObject boidPrefab;
    public List<Boid> boids = null;
    public int boidsToSpawn = 10;

    // To select 
    public enum ControlMode
    {
        Seek,
        Pursue,
        Food,
        Obstacle,
    }

    // Curent mode
    public ControlMode controlMode = ControlMode.Seek;

    private void Start()
    {
        // Spawn boids
        for(int i = 0; i < boidsToSpawn; i++)
        {
            // Random position and rotation
            Vector3 position = new Vector3(Random.Range(-1.4f, 1.4f), Random.Range(0, 1.4f), Random.Range(-.9f, .9f));
            Quaternion rotation = Random.rotation;

            GameObject spawnedBoid = Instantiate(boidPrefab, position, rotation);
            boids.Add(spawnedBoid.GetComponent<Boid>()); // List of boids

            spawnedBoid.transform.localScale *= Random.Range(.9f, 3f); // Random size
        }
    }

    private void Update()
    {
        // Number Keys to switch between modes
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            controlMode = ControlMode.Seek;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            controlMode = ControlMode.Pursue;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            controlMode = ControlMode.Food;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            controlMode = ControlMode.Obstacle;
        }
    }
}

