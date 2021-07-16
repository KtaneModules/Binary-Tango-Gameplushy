using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KeepCoding;

public class binaryTangoTPScript : TPScript<binaryTangoScript>
{
    public override IEnumerator ForceSolve()
    {
        for(int i = Module.stageNumber;i< 3; i++)
        {
            List<KMSelectable> presses;
            int delta = Module.answers[i] - Module.yourAnswer;
            int isNegative = delta < 0 ? 3 : 0;
            presses = CalculatePresses(delta, isNegative);
            presses.Add(Module.submitPixel);
            presses.ForEach(p=>p.OnInteract());
        }
        while (!Module.IsSolved)
            yield return true;
    }

    public override IEnumerator Process(string command)
    {
        string[] commandSplit = command.Split();
        int value;
        List<KMSelectable> presses = new List<KMSelectable>();

        if ((IsMatch(commandSplit[0], "input")|| IsMatch(commandSplit[0], "submit")) && commandSplit.Length == 2)
        {
            if (!int.TryParse(commandSplit[1], out value))
            {
                yield return SendToChatError("That's not an integer...");
                yield break;
            }
            int delta = value - Module.yourAnswer;
            int isNegative = delta < 0 ? 3 : 0;
            presses = CalculatePresses(delta, isNegative);
        }

        if (IsMatch(commandSplit[0], "submit")&&commandSplit.Length<=2)
        {
            presses.Add(Module.submitPixel);
        }
        if (presses.Count != 0)
        {
            yield return null;
            yield return presses.ToArray();
        }       
    }

    private List<KMSelectable> CalculatePresses(int input, int isNegative)
    {
        List<KMSelectable> presses = new List<KMSelectable>();
        int[] values = { 100, 10, 1 };
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < (System.Math.Abs(input) / values[i]) % 10; j++)
            {
                presses.Add(Module.answerKeys[i + isNegative]);
            }
        }
        return presses;
    }
}
