using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using TMPro;

public class Robot : Agent
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 200f;
    private Rigidbody rBody;

    [Header("Alliance")]
    public Alliance myAlliance;

    [Header("Held Blocks")]
    public int maxHeldBlocks = 15;
    public List<int> heldBlocks = new List<int>();
    public TextMeshProUGUI redBlocksText;
    public TextMeshProUGUI blueBlocksText;

    [Header("Parking Zone")]
    public Collider parkingZone;

    [Header("References")]
    public GameManager gameManager;
    public Transform intakeZone;
    public Transform depositZone;

    // Input flags (set by Discrete actions or Heuristic)
    private bool intakeButtonPressed = false;
    private bool outtakeButtonPressed = false;

    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        rBody.drag = 5f;
        rBody.angularDrag = 2f;
        rBody.centerOfMass = new Vector3(0, -0.25f, 0);
    }

    public override void OnEpisodeBegin()
    {
        rBody.velocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;
        transform.localPosition = new Vector3(0, 0.5f, 0);

        heldBlocks.Clear();
        intakeButtonPressed = false;
        outtakeButtonPressed = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.rotation.y);
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
        sensor.AddObservation((float)heldBlocks.Count / maxHeldBlocks);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Movement
        float moveX = actions.ContinuousActions[0]; // strafe
        float moveZ = actions.ContinuousActions[1]; // forward/back
        float turn = actions.ContinuousActions[2];  // rotate

        Vector3 input = new Vector3(moveX, 0, moveZ);
        if (input.magnitude > 1f) input = input.normalized;
        Vector3 move = input * moveSpeed;
        rBody.AddForce(move, ForceMode.Force);

        rBody.AddTorque(Vector3.up * turn * rotateSpeed, ForceMode.Force);

        // Intake / outtake buttons
        intakeButtonPressed = actions.DiscreteActions[0] == 1;
        outtakeButtonPressed = actions.DiscreteActions[1] == 1;
    }

    public bool OuttakeButtonPressed() => outtakeButtonPressed;

    public bool IntakeButtonPressed() => intakeButtonPressed;

    public void TryIntakeNearbyBlock(Block block)
    {
        if (!IntakeButtonPressed()) return;
        if (heldBlocks.Count >= maxHeldBlocks) return;

        heldBlocks.Add(block.blockColor);
        Destroy(block.gameObject);
    }

    public void TryDepositToNearbyGoal(GoalTracker goal, End end)
    {
        if (!OuttakeButtonPressed()) return;
        if (heldBlocks.Count == 0) return;

        Debug.Log($"Depositing block to goal: {goal.name} at end: {end}");

        int blockColor = myAlliance == Alliance.Red ? 1 : -1;

        goal.AddBlock(blockColor, end);

        if (goal.CheckControlFlip(myAlliance))
        {
            gameManager.AddScore(myAlliance, goal.GetControlBonus());
        }

        gameManager.AddScore(myAlliance, 3);

        heldBlocks.RemoveAt(0);
    }

    public bool IsParked()
    {
        return parkingZone.bounds.Contains(transform.position);
    }

    public void OnMatchEnd(Alliance winningAlliance)
    {
        bool didWin = myAlliance == winningAlliance;

        AddReward(didWin ? +10f : 0f);

        if (IsParked())
        {
            AddReward(8f);
        }

        EndEpisode();
    }

    public void ResetAgent(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        rBody.velocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;
        heldBlocks.Clear();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var c = actionsOut.ContinuousActions;
        c[0] = Input.GetAxis("Horizontal"); // strafe
        c[1] = Input.GetAxis("Vertical");   // forward/back
        c[2] = Input.GetKey(KeyCode.Q) ? -1 : Input.GetKey(KeyCode.E) ? 1 : 0; // rotate

        var d = actionsOut.DiscreteActions;
        d[0] = Input.GetKey(KeyCode.Space) ? 1 : 0; // Intake
        d[1] = Input.GetKey(KeyCode.F) ? 1 : 0;     // Outtake
    }

    private void FixedUpdate()
    {
        UpdateHeldBlocksDisplay();
    }

    private void UpdateHeldBlocksDisplay()
    {
        int redCount = 0;
        int blueCount = 0;

        foreach (int c in heldBlocks)
        {
            if (c > 0) redCount++;
            else if (c < 0) blueCount++;
        }

        if (redBlocksText != null)
        {
            redBlocksText.text = $"Red: {redCount}";
        }
        if (blueBlocksText != null)
        {
            blueBlocksText.text = $"Blue: {blueCount}";
        }
    }

    public List<Block> GetNearestBlocks(int N)
    {
        List<Block> nearest = new List<Block>();

        foreach (Block block in gameManager.allBlocks.GetComponentsInChildren<Block>())
        {
            if (block == null) continue;

            // Optionally: skip blocks that are very high (in case of stacked blocks mid-air)
            if (block.transform.position.y > 2f) continue;

            nearest.Add(block);
        }

        nearest.Sort((a, b) =>
        {
            float da = Vector3.Distance(transform.position, a.transform.position);
            float db = Vector3.Distance(transform.position, b.transform.position);
            return da.CompareTo(db);
        });

        if (nearest.Count > N)
            nearest = nearest.GetRange(0, N);

        return nearest;
    }

}
