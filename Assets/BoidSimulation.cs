using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;

public class BoidSimulationControl : MonoBehaviour
{
    [SerializeField] GameObject foodPrefab; 
    [SerializeField] GameObject rockPrefab; // Prefab for obstacles
    private GameObject targetObject; 
    public GameObject boidPrefab;
    public List<Boid> boids = null;
    public List<GameObject> rocks = null; // List of obstacles
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
        targetObject = GameObject.Find("Target");

        // Spawn boids
        for (int i = 0; i < boidsToSpawn; i++)
        {
            // Random position and rotation
            Vector3 position = new Vector3(Random.Range(-1.4f, 1.4f), Random.Range(0, 1.4f), Random.Range(-.9f, .9f));
            Quaternion rotation = Random.rotation;

            GameObject spawnedBoid = Instantiate(boidPrefab, position, rotation);
            boids.Add(spawnedBoid.GetComponent<Boid>()); // List of boids

            spawnedBoid.transform.localScale *= Random.Range(.9f, 3f); // Random size

            // Randomize properties of each boid within range
            spawnedBoid.GetComponent<Boid>().speed = Random.Range(.5f, 2.5f);
            spawnedBoid.GetComponent<Boid>().accelMax = Random.Range(1f, 2f);
            spawnedBoid.GetComponent<Boid>().speedMax = Random.Range(1f, 2.3f);
        }
    }

    private void SeekModeControl()
    {
        for (int i = 0; i < boids.Count; i++)
        {
            // Call the seek function on each boid to calculate an acceleration vector
            Vector3 accel = boids[i].Seek(targetObject.transform.position, boids[i].accelMax);

            if (Input.GetMouseButton(0)) // If left mouse button is pressed
            {
                boids[i].currentLinearAcceleration += accel; // Apply acceleration 
                Debug.DrawRay(boids[i].transform.position, accel, Color.green); // Draw acceleration
            }
            else if (Input.GetMouseButton(1)) // If right mouse button is pressed
            {
                boids[i].currentLinearAcceleration -= accel; // Apply negative acceleration
                Debug.DrawRay(boids[i].transform.position, accel, Color.red); // Draw negative acceleration
            }
        }
    }

    private void PursueModeControl()
    {
        for (int i = 0; i < boids.Count; i++)
        {
            // Call the pursue function on each boid to calculate an acceleration vector
            Vector3 accel = boids[i].Pursue(targetObject.transform.position, boids[i].accelMax, boids[i].speedMax);
            if (Input.GetMouseButton(0)) // If left mouse button is pressed
            {
                boids[i].currentLinearAcceleration += accel; // Apply acceleration 
                Debug.DrawRay(boids[i].transform.position, accel, Color.green); // Draw acceleration
            }
            else if (Input.GetMouseButton(1)) // Evade mode
            {
                boids[i].currentLinearAcceleration -= accel; // Apply negative acceleration
                Debug.DrawRay(boids[i].transform.position, accel, Color.red); // Draw negative acceleration
            }
        }
    }

    private void SpawnFood()
    {
        Instantiate(foodPrefab, targetObject.transform.position, Random.rotation); // spawn food at targetObject position
    }
    
    private void SpawnObstacle()
    {
        Vector3 spawnPos = targetObject.transform.position; // Get the position of the target object
        spawnPos.y -= 0.5f; // Set the y position to 0.5 to avoid spawning below the ground
        GameObject spawnedRock = Instantiate(rockPrefab, spawnPos, Random.rotation); // spawn obstacle at targetObject position
        rocks.Add(spawnedRock); // Add the spawned rock to the list of rocks
    }

    private void FoodArrivalBehaviour()
    {
        // Have all boids seek food
        for (int i = 0; i < boids.Count; i++)
        {
            float foodSeekRadius = 0.8f; // Radius to seek food
            Collider[] colliders = Physics.OverlapSphere(boids[i].transform.position, foodSeekRadius);

            Food closestFood = null; // Variable to store the closest food
            float closestFoodDistance = float.PositiveInfinity; // Initialize closest food distance to infinity

            foreach (Collider collider in colliders)
            {
                Food food = collider.GetComponent<Food>(); // Check if the collider has a Food component
                if (food != null) // seeks food if not null
                {
                    float distanceToFood = Vector3.Distance(food.transform.position, boids[i].transform.position); // Calculate distance to food
                    if (distanceToFood < closestFoodDistance) // If left mouse button is pressed
                    {
                        closestFoodDistance = distanceToFood; // Set the closest food
                        closestFood = food; // Set the closest food
                    }
                }
            }

            if (closestFood != null)
            {
                // If a food is found, apply the seek acceleration
                Vector3 accel = boids[i].Arrive(closestFood.transform.position, boids[i].accelMax, 0.17f, 0.38f);
                boids[i].currentLinearAcceleration += accel;
                Debug.DrawRay(boids[i].transform.position, boids[i].currentLinearAcceleration, Color.green); // Draw acceleration towards food

                // Check if the boid is close enough to the food
                if (closestFoodDistance < 0.12f)
                {
                    // If the boid is close enough to the food, destroy the food object
                    Destroy(closestFood.gameObject); // Destroy the food object after consumption
                }
            }
        }
    }

    private void ResetSimulation()
    {
        for (int i = 0; i < boids.Count; i++)
        {
            Destroy(boids[i].gameObject); // Destroy all boids
        }
        boids.Clear(); // Clear the list of boids

        for (int i = 0; i < rocks.Count; i++)
        {
            Destroy(rocks[i]); // Destroy all obstacles
        }
        rocks.Clear(); // Clear the list of obstacles
        
        Start(); // Restart the simulation by calling Start method
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
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetSimulation();
        }
        
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].currentLinearAcceleration = Vector3.zero; // Reset acceleration for each boid each cycle
        }

        foreach (Boid boid in boids)
        {
            // Obstacle avoidance
            boid.currentLinearAcceleration = boid.ObstacleAvoidance(0.6f, boid.accelMax);
        }

        switch (controlMode)
        {
            case ControlMode.Seek:
                SeekModeControl();
                break;
            case ControlMode.Pursue:
                PursueModeControl();
                break;
            case ControlMode.Food:
                if (Input.GetMouseButtonDown(0)) // If left mouse button is pressed
                {
                    SpawnFood(); // Spawn food at targetObject position
                }
                break;
            case ControlMode.Obstacle:
                if (Input.GetMouseButtonDown(0)) // If left mouse button is pressed
                {
                    SpawnObstacle(); // Spawn obstacle at targetObject position
                }
                break;
        }
        
        FoodArrivalBehaviour();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo; // Will store information about the intersection if any
        bool didHit = Physics.Raycast(ray, out hitInfo, 100);

        if (didHit)
        {
            targetObject.transform.position = hitInfo.point; // Move the target to the hit point
        }

        // Enforce acceleration limit and apply velocity change
        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].currentLinearAcceleration = Vector3.ClampMagnitude(boids[i].currentLinearAcceleration, boids[i].accelMax);

            boids[i].rb.linearVelocity += boids[i].currentLinearAcceleration * Time.fixedDeltaTime;
        }
    }
}

