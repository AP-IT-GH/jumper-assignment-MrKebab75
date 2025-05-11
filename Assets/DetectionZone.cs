using UnityEngine;

public class DetectionZone : MonoBehaviour
{
    public MLAgent agent;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            agent.SetObstacleDetected(true);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            agent.SetObstacleDetected(false);
        }
    }
}