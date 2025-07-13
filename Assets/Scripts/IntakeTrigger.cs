using UnityEngine;

public class IntakeTrigger : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        Block block = other.GetComponent<Block>();
        if (block == null) return;

        Robot agent = GetComponentInParent<Robot>();
        if (agent == null) return;
        
        agent.TryIntakeNearbyBlock(block);
    }
}
