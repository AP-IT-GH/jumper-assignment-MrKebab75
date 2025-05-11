using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MLAgent : Agent
{
    // Instellingen voor de agent
    public float jumpForce = 7f;            // Kracht van de sprong
    public float jumpCooldown = 0.5f;       // Tijd tussen sprongen
    public GameObject detectionZone;        // Zone die obstakels detecteert

    // Private variabelen
    private Rigidbody rb;                   // Referentie naar de rigidbody
    private bool isGrounded = true;         // Of de agent op de grond staat
    private float lastJumpTime;             // Tijdstip van laatste sprong
    private bool obstacleDetected = false;  // Of er een obstakel is gedetecteerd
    private float groundCheckDistance = 0.1f; // Afstand voor grond detectie

    void Start()
    {
       rb = this.GetComponent<Rigidbody>();
       lastJumpTime = -jumpCooldown;
       
       // Maak een detectie zone aan indien nodig
       if (detectionZone == null)
       {
           CreateDetectionZone();
       }
    }

    // Handmatige input voor testen
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ForceJump();
        }
    }

    // Direct springen zonder checks
    public void ForceJump()
    {
        if (rb != null)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // Maak een detectie zone aan voor het waarnemen van obstakels
    void CreateDetectionZone()
    {
        detectionZone = new GameObject("DetectionZone");
        detectionZone.transform.parent = this.transform;
        detectionZone.transform.localPosition = new Vector3(0, 0, 2f);
        
        BoxCollider triggerCollider = detectionZone.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector3(1f, 1f, 4f);
        
        DetectionZone detector = detectionZone.AddComponent<DetectionZone>();
        detector.agent = this;
    }

    // Reset de agent bij een nieuwe episode
    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(7.81f, 0.5f, 0f);
        rb.linearVelocity = Vector3.zero;
        isGrounded = true;
        obstacleDetected = false;
        lastJumpTime = -jumpCooldown;
        
        // Soms willekeurig springen voor exploratie
        if (Academy.Instance.IsCommunicatorOn && Random.value > 0.7f)
        {
            Invoke("Jump", Random.Range(0.1f, 1f));
        }
    }

    // Geef informatie over de omgeving aan het neural network
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(isGrounded ? 1 : 0);
        sensor.AddObservation(obstacleDetected ? 1 : 0);
        sensor.AddObservation(Time.time - lastJumpTime);
    }

    // Voer acties uit die het neural network beslist
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (actions.DiscreteActions[0] == 1 && isGrounded && Time.time > lastJumpTime + jumpCooldown)
        {
            Jump();
        }
        
        // Kleine beloning voor correct wachten
        if (isGrounded && !obstacleDetected)
        {
            AddReward(0.01f * Time.fixedDeltaTime);
        }
    }

    // Spring functie met beloningen
    public void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        lastJumpTime = Time.time;
        
        // Beloon of straf gebaseerd op of er een obstakel is
        if (obstacleDetected)
        {
            AddReward(1.0f);
        }
        else
        {
            AddReward(-0.2f);
        }
    }

    // Beloon voor succesvol springen over een obstakel
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle") && !isGrounded)
        {
            AddReward(2.0f);
        }
    }

    // Update de obstakel detectie status
    public void SetObstacleDetected(bool detected)
    {
        obstacleDetected = detected;
    }

    // Check of de agent op de grond staat
    private void FixedUpdate()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
    }

    // Straf voor botsing met obstakels
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            AddReward(-5.0f);
            EndEpisode();
        }
    }

    // Handmatige besturing voor testen
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }
}