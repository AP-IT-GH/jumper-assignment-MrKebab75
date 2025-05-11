using UnityEngine;

public class ObstacleMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Vector3 moveDirection = Vector3.back;  // Richting naar de agent toe
    
    public Vector3 startPosition;
    public float resetHeight = -2f;  // Y-positie waarop het object resetten

    public string agentTag = "Player";  // Tag van de agent

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Sla de startpositie op voor reset
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Beweeg het obstakel naar voren (naar de agent toe)
        transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime);
        
        // Check of het object onder de plane is gevallen
        if (transform.position.y < resetHeight)
        {
            ResetObstacle();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Als het obstakel de agent raakt, reset dan
        if (collision.gameObject.CompareTag(agentTag))
        {
            ResetObstacle();
        }
    }

    private void ResetObstacle()
    {
        // Reset de positie van het obstakel
        transform.position = startPosition;
        
        // Reset eventuele fysica
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
