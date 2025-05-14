using UnityEngine;

public class Boid : MonoBehaviour
{
    public Rigidbody rb;
    private void Awake()
    {
        // Randomize the color of the boid
        GetComponent<Renderer>().material.SetColor("_BaseColor", Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1));

        // Randomizes the velocity of the boid
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Random.insideUnitCircle;
    }

    public void Update()
    {
        AlignToVelocity();
    }

    // Align to the velocity of the boid
    public void AlignToVelocity()
    {
        transform.forward = Vector3.RotateTowards(transform.forward, rb.linearVelocity.normalized, Mathf.Deg2Rad * 1800 * Time.deltaTime, 100);
    }
}
