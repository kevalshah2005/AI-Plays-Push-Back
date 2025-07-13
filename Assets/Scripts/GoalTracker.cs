using System.Collections.Generic;
using UnityEngine;

public enum GoalType { Long, CenterUpper, CenterLower }

public class GoalTracker : MonoBehaviour
{
    public GoalType goalType;

    public SizedDeque<int> ballColors;
    private Alliance? previousControl = null;

    void Awake()
    {
        ballColors = new SizedDeque<int>(goalType == GoalType.Long ? 15 : 7);
    }

    public void AddBlock(int colorValue, End end)
    {
        if (end == End.BACK)
        {
            ballColors.AddToBack(colorValue);
        }
        else if (end == End.FRONT)
        {
            ballColors.AddToFront(colorValue);
        }
        else
        {
            Debug.LogError("Invalid end specified for adding block to goal tracker.");
            return;
        }
    }

    public void ResetGoal()
    {
        ballColors.Clear();
        previousControl = null;
    }

    public Alliance? CurrentControlAlliance()
    {
        int score = 0;

        if (goalType == GoalType.Long)
        {
            var array = ballColors.ToArray();
            for (int i = 6; i <= 8; i++)
                score += array[i];
        }
        else
        {
            foreach (var color in ballColors.Items)
                score += color;
        }

        if (score > 0) return Alliance.Red;
        if (score < 0) return Alliance.Blue;
        return null;
    }

    public bool CheckControlFlip(Alliance agentAlliance)
    {
        Alliance? currentControl = CurrentControlAlliance();

        bool flipped = false;
        if (previousControl != currentControl)
        {
            if (currentControl != null && currentControl == agentAlliance)
                flipped = true;

            previousControl = currentControl;
        }

        return flipped;
    }

    public int GetControlBonus()
    {
        return goalType switch
        {
            GoalType.Long => 10,
            GoalType.CenterUpper => 8,
            GoalType.CenterLower => 6,
            _ => 0
        };
    }

    public string GetBallStackString()
    {
        string s = "";
        foreach (var c in ballColors.Items)
        {
            s += c > 0 ? "R " : c < 0 ? "B " : "- ";
        }
        return s;
    }
}
