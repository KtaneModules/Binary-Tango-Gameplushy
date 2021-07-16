using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RNG = UnityEngine.Random;
using KeepCoding;
using System;
using KModkit;

public class binaryTangoScript : ModuleScript {

	const float BASEBG = 0.1254902f;

	internal byte yourAnswer = 0;
	public TextMesh yourAnswerUI;
	public TextMesh[] userAnswerUI;
	public KMSelectable[] answerKeys;

	private byte[,] numbers = new byte[3, 2];
	public TextMesh[] numbersUI;

	internal byte[] answers = new byte[3];

	internal byte stageNumber = 0;

	public KMSelectable submitPixel;
	public MeshRenderer[] pixelStrips;
	public Material[] stageCompletedMat;

	public Renderer BGMat;

	private readonly int[] bases = { 10, 16, 8 };
	private readonly string[] solveNames = { "StageOne", "StageTwo", "Solve" };

	// Use this for initialization
	void Start () {

	}

    public override void OnActivate()
    {
        base.OnActivate();
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 2; j++) numbers[i, j] = (byte)RNG.Range(byte.MinValue, byte.MaxValue);
		}
		WriteNumbers(0);
		CalculateAnswers();
		answerKeys.Assign(onInteract: ChangeAnswer);
		submitPixel.Assign(onInteract: CheckAnswer);
	}

    private void ChangeAnswer(int i)
	{
		ButtonEffect(answerKeys[i], 0.25f, KMSoundOverride.SoundEffect.ButtonPress);
		if (!IsSolved)
        {
			int[] possibilities = new int[] { 100, 10, 1, -100, -10, -1 };
			yourAnswer = (byte)Mathf.Clamp(yourAnswer + possibilities[i], Byte.MinValue, Byte.MaxValue);
			yourAnswerUI.text = Convert.ToString(yourAnswer, bases[stageNumber]).ToUpper();
		}

	}
	private void WriteNumbers(int stage)
    {
		numbersUI[0].text = Convert.ToString(numbers[stage, 0], bases[stageNumber]).ToUpper();
		numbersUI[1].text = Convert.ToString(numbers[stage, 1], bases[stageNumber]).ToUpper();
		if (stage == 0) yourAnswerUI.text = "0";
	}

	private void CalculateAnswers()
    {
		KMBombInfo edgeworkChecker = Get<KMBombInfo>();
		bool[] firstBitChooser = new bool[] { edgeworkChecker.GetBatteryCount()%2==1, edgeworkChecker.GetIndicators().LengthOrDefault()%2==1, edgeworkChecker.GetPortPlateCount()%2==1};
		for(int i = 0; i < 3; i++)
        {
			for(int bit = 0; bit < 8; bit++)
            {
				answers[i] += (byte)(Byte.Parse(GetBinaryWord(numbers[i,firstBitChooser[i]?1-(bit%2):(bit%2)])[bit].ToString())*Math.Pow(2,7-bit));
				//Log("Number {0} : bit {1} is {2} gives +{3}".Form(bit%2,bit, GetBinaryWord(numbers[i, bit % 2])[bit],(byte)(GetBinaryWord(numbers[i, bit%2])[bit]*Math.Pow(2,7-bit))));
            }
			Log("Stage {0}, in base {1} : {2}({3}|{4}) and {5}({6}|{7}) makes {8}({9}|{10}). User the {11} number for the most significant bit, than switch back and forth.".Form(i+1, bases[i],Convert.ToString(numbers[i, 0],bases[i]), numbers[i, 0], GetBinaryWord(numbers[i, 0]), Convert.ToString(numbers[i, 1], bases[i]), numbers[i, 1], GetBinaryWord(numbers[i, 1]), Convert.ToString(answers[i], bases[i]), answers[i], GetBinaryWord(answers[i]),firstBitChooser[i]?"second":"first" ));
		}
    }
	private void CheckAnswer()
    {
		ButtonEffect(submitPixel, 1, KMSoundOverride.SoundEffect.ButtonPress);
        if (!IsSolved)
        {
			if (yourAnswer == answers[stageNumber])
				NextStage();
            else
            {
				Log("You submitted {0} when {1} was expected. Strike ! ".Form(yourAnswer, answers[stageNumber]));
				PlaySound("Strike");
				Strike();
			}	
		}
    }

	public void NextStage()
    {
		pixelStrips[stageNumber].sharedMaterial = stageCompletedMat[stageNumber];
		string logText = "Correctly submitted {0}. ".Form(yourAnswer);
		yourAnswer = 0; yourAnswerUI.text = "0";
		float red, green, blue; red = green = blue = BASEBG;
		red+=(float)answers[0] * 223 / 255 / 255;
		if(stageNumber>=1) green+=(float)answers[1] * 223 / 255 / 255;
		if (stageNumber == 2) blue += (float)answers[2] * 223 / 255 / 255;
		BGMat.material.color = new Color(red, green, blue);
		PlaySound(solveNames[stageNumber]);
		if (++stageNumber == 3)
		{
			logText += "Module solved !";
			PlaySound(KMSoundOverride.SoundEffect.CorrectChime);
			Solve();
			pixelStrips.ForEach(p => p.material.color = new Color((float)answers[0]/255, (float)answers[1]/255, (float)answers[2]/255));
			numbersUI[0].text = "WELL";
			numbersUI[1].text = "DONE";
		}
		else
		{
			logText += "Starting stage {0}...".Form(stageNumber+1);
			WriteNumbers(stageNumber);
		}
		Log(logText);
    }

	public string GetBinaryWord(byte decimalWord)
	{
		return Convert.ToString(decimalWord, 2).PadLeft(8, '0');
	}

}
