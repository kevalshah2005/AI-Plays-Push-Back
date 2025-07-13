using UnityEngine;

public class Block : MonoBehaviour
{
    // +1 for Red, -1 for Blue
    public int blockColor;
    public bool intakeable = true;
    [HideInInspector] public Vector3 initialPosition;
    [HideInInspector] public Quaternion initialRotation;
    [HideInInspector] public int initialBlockColor;

    public void Start()
    {
        UpdateVisual();
        RecordInitialState();
    }

    public void SetColor(Alliance alliance)
    {
        blockColor = alliance == Alliance.Red ? 1 : -1;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Color color = blockColor > 0 ? Color.red : blockColor < 0 ? Color.blue : Color.grey;
            rend.material.color = color;
        }
    }

    public void RecordInitialState()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialBlockColor = blockColor;
    }

    public void ResetBlock()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        blockColor = initialBlockColor;
        SetColor(blockColor > 0 ? Alliance.Red : Alliance.Blue);

        // Optional: zero velocity if using Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
