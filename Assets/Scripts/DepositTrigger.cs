using UnityEngine;

public class DepositTrigger : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (!other.isTrigger) return;

        GoalTracker goalTracker = other.GetComponentInParent<GoalTracker>();
        if (goalTracker == null) return;

        End detectedEnd = DetectEnd(goalTracker, other);

        Robot agent = GetComponentInParent<Robot>();
        if (agent == null) return;

        agent.TryDepositToNearbyGoal(goalTracker, detectedEnd);
    }

    private End DetectEnd(GoalTracker goalTracker, Collider goalCollider)
    {
        string name = goalCollider.gameObject.name.ToLower();

        if (name.Contains("front"))
            return End.FRONT;
        if (name.Contains("back"))
            return End.BACK;

        // Fallback:
        Debug.LogWarning($"Could not detect goal end, defaulting to Front. Name was {name}.");
        return End.FRONT;
    }
}
