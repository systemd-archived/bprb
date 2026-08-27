using System;
using UnityEngine;
using UnityEngine.UI;

public class SplashText : MonoBehaviour
{
	[SerializeField]
	public TextAsset Datas;

	private string[] splashTexts;

	private Text Text;

	private string[] Secrets = new string[2] { "\"This message will never appear in the game even if its in the code for the game, isnt that weird?\"", "You should not be reading this!" };

	private bool isInitalized;

	private void Start()
	{
		Text = GetComponent<Text>();
		splashTexts = Datas.text.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		Text.text = splashTexts[UnityEngine.Random.Range(0, splashTexts.Length - 1)];
		isInitalized = true;
	}

	private void Awake()
	{
		if (isInitalized)
		{
			Text.text = splashTexts[UnityEngine.Random.Range(0, splashTexts.Length - 1)];
		}
	}
}
