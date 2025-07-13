using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    [Header("Match Settings")]
    public float matchDuration = 105f;
    [HideInInspector] public float matchTimer;

    public bool matchRunning = false;

    [Header("References")]
    public List<Robot> allAgents;
    public List<GoalTracker> allGoals;
    public Transform allBlocks;

    public Transform[] redRobotSpawns;
    public Transform[] blueRobotSpawns;

    public Collider redParkingZone;
    public Collider blueParkingZone;

    [Header("Score")]
    public int redScore = 0;
    public int blueScore = 0;

    public int redMatchLoads;
    public int blueMatchLoads;

    void FixedUpdate()
    {
        if (!matchRunning) return;

        matchTimer -= Time.fixedDeltaTime;
        if (matchTimer <= 0f)
        {
            EndMatch();
        }
    }

    public void ResetMatch()
    {
        Debug.Log("Resetting match");

        matchTimer = matchDuration;
        matchRunning = true;

        redScore = 0;
        blueScore = 0;

        // Reset Goals
        foreach (var goal in allGoals) goal.ResetGoal();

        // Reset Blocks
        int blockCount = allBlocks.childCount;
        for (int i = 0; i < blockCount; i++)
        {
            var block = allBlocks.GetChild(i).GetComponent<Block>();
            if (block != null)
            {
                block.ResetBlock();
            }
        }

        // Reset Agents
        for (int i = 0; i < allAgents.Count; i++)
        {
            var agent = allAgents[i];
            Transform spawn = agent.myAlliance == Alliance.Red ? redRobotSpawns[i % redRobotSpawns.Length] : blueRobotSpawns[i % blueRobotSpawns.Length];
            agent.ResetAgent(spawn.position, spawn.rotation);
        }
    }

    public void AddScore(Alliance alliance, int points)
    {
        if (alliance == Alliance.Red) redScore += points;
        else blueScore += points;
    }

    public void EndMatch()
    {
        Debug.Log("Match ended");

        matchRunning = false;

        // Count parked agents
        int redParked = CountParked(Alliance.Red);
        int blueParked = CountParked(Alliance.Blue);

        if (redParked == 1) redScore += 8;
        if (redParked >= 2) redScore += 30;
        if (blueParked == 1) blueScore += 8;
        if (blueParked >= 2) blueScore += 30;

        Alliance winner = redScore > blueScore ? Alliance.Red : Alliance.Blue;

        // Notify agents
        foreach (var agent in allAgents)
        {
            agent.OnMatchEnd(winner);
        }

        // Optionally restart match after delay
        Invoke(nameof(ResetMatch), 2f);
    }

    public int CountParked(Alliance alliance)
    {
        int count = 0;
        foreach (var agent in allAgents)
        {
            if (agent.myAlliance == alliance && agent.IsParked()) count++;
        }
        return count;
    }
}
