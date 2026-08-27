using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatsUtility : WPFMonoBehaviour
{
// Adding 'static' makes the game remember the ON/OFF state
// Static so it stays ON when you close/open the menu
    private static bool hitboxesEnabled = false;

    // This runs every frame to catch new items like the Pig or Egg
    private void Update()
    {
        if (hitboxesEnabled)
        {
            ApplyHitboxesToAll();
        }
    }

	private void Start()
	{
		float dpi = Screen.dpi;
		if (dpi < 1f)
		{
			this.m_buttonHeight = (float)Screen.height * 0.166666672f;
			this.m_buttonWidth = (float)Screen.width * 0.3125f;
		}
		else
		{
			float num = (float)Screen.width / dpi * 0.5f;
			this.rowItems = Mathf.Clamp((int)num, 3, 10);
			this.m_buttonWidth = (float)Screen.width * (1f / (1.1f * (float)this.rowItems));
			float value = (float)Screen.height / dpi;
			this.m_buttonHeight = (float)Screen.height * (1f / Mathf.Clamp(value, 3f, 10f));
		}
		string text = "sir";
		if (!string.IsNullOrEmpty(SystemInfo.deviceName))
		{
			text = SystemInfo.deviceName;
		}
		if (text.Length > 10)
		{
			text = "\n" + text;
		}
		this.textMesh.text = "Jolly good day, " + text;
	}

	   private bool horizontalGroupOpen = false;
	   private void DrawButton(string label, Action onClick)
	   {
		   if (this.currentButtonIndex == 0 || this.currentButtonIndex % this.rowItems == 0)
		   {
			   if (this.horizontalGroupOpen)
			   {
				   GUILayout.EndHorizontal();
				   this.horizontalGroupOpen = false;
			   }
			   GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			   this.horizontalGroupOpen = true;
		   }
		   if (GUILayout.Button(label, new GUILayoutOption[]
		   {
			   GUILayout.Width(this.m_buttonWidth),
			   GUILayout.Height(this.m_buttonHeight)
		   }) && onClick != null)
		   {
			   onClick();
		   }
		   this.currentButtonIndex++;
	   }

	   private void BeginGrid()
	   {
		   this.currentButtonIndex = 0;
		   this.horizontalGroupOpen = false;
	   }

	   private void EndGrid()
	   {
		   if (this.horizontalGroupOpen)
		   {
			   GUILayout.EndHorizontal();
			   this.horizontalGroupOpen = false;
		   }
	   }

	private void OnGUI()
	{
		if (!this.skinInitialized)
		{
			this.cheatSkin = GUI.skin;
			float dpi = Screen.dpi;
			if (dpi > 1f)
			{
				this.cheatSkin.label.fontSize = Mathf.FloorToInt(0.1f * dpi + 2f);
				this.cheatSkin.button.fontSize = Mathf.FloorToInt(0.1f * dpi + 2f);
			}
			else
			{
				this.cheatSkin.label.fontSize = Mathf.FloorToInt(15f);
				this.cheatSkin.button.fontSize = Mathf.FloorToInt(15f);
			}
			this.cheatSkin.button.wordWrap = true;
			GUI.skin.verticalScrollbar.fixedWidth = (float)Screen.width * 0.05f;
			GUI.skin.verticalScrollbarThumb.fixedWidth = (float)Screen.width * 0.05f;
			this.skinInitialized = true;
		}
		this.scrollbarPosition = GUILayout.BeginScrollView(this.scrollbarPosition, new GUILayoutOption[]
		{
			GUILayout.Width((float)Screen.width),
			GUILayout.Height((float)Screen.height - (float)Screen.height * 0.1f)
		});
		this.DrawButton("Reset progress warning lost", delegate
		{
			GameProgress.DeleteAll();
			GameProgress.InitializeGameProgressData();
			GameProgress.Save();
			UserSettings.DeleteAll();
			UserSettings.Save();
			if (Singleton<DailyChallenge>.IsInstantiated() && Singleton<DailyChallenge>.Instance.Initialized)
			{
				Singleton<DailyChallenge>.Instance.ForceNewChallenge();
			}
		});
		this.DrawButton("Reset level only", delegate
		{
			// We only use the methods we know exist
			GameProgress.InitializeGameProgressData();
			GameProgress.Save();

			if (Singleton<DailyChallenge>.IsInstantiated() && Singleton<DailyChallenge>.Instance.Initialized)
			{
				Singleton<DailyChallenge>.Instance.ForceNewChallenge();
			}
		});
		this.DrawButton("Unlimited Sandbox Parts", delegate
		{
			IEnumerator enumerator = Enum.GetValues(typeof(BasePart.PartType)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					BasePart.PartType partType = (BasePart.PartType)obj;
					if (partType != BasePart.PartType.Unknown && partType != BasePart.PartType.ObsoleteWheel && partType != BasePart.PartType.JetEngine)
					{
						int sandboxPartCount = GameProgress.GetSandboxPartCount(partType);
						GameProgress.AddSandboxParts(partType, 999 - sandboxPartCount, false);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		});
		if (Application.targetFrameRate == 60)
		{
			this.DrawButton("Set low target FPS", delegate
			{
				Application.targetFrameRate = 25;
			});
		}
		else if (Application.targetFrameRate == 120)
		{
			this.DrawButton("Set default target FPS", delegate
			{
				Application.targetFrameRate = 60;
			});
		}
		else
		{
			this.DrawButton("Set high target FPS", delegate
			{
				Application.targetFrameRate = 120;
			});
		}
		this.DrawButton("Add Glue, Magnet, Turbo and NightVision, superAutoBuild", delegate
		{
			GameProgress.AddSuperGlue(10000);
			GameProgress.AddSuperMagnet(10000);
			GameProgress.AddTurboCharge(10000);
			GameProgress.AddNightVision(10000);
			// GameProgress.AddSuperBluePrint(10000);
		});
		this.DrawButton("Unlock All Free Levels", delegate
		{
			GameProgress.SetBool("UnlockAllFreeLevels", true, GameProgress.Location.Local);
		});
		if (Singleton<RewardSystem>.IsInstantiated())
		{
			string text = "Reward Timer Toggle\nReward time / Reset time\n";
			switch (Singleton<RewardSystem>.Instance.GetTimerMode())
			{
				case 0:
					text += "24h / 48h";
					break;
				case 1:
					text += "15m / 30m";
					break;
				case 2:
					text += "5m / 15m";
					break;
				case 3:
					text += "1m / 1m 15s";
					break;
				case 4:
					text += "5s / 10s";
					break;
			}
			this.DrawButton(text, delegate
			{
				Singleton<RewardSystem>.Instance.ChangeTimerMode();
			});
		}
		this.DrawButton("Reset snout intro", delegate
		{
			GameProgress.SetInt("show_count_snout_intro", 0, GameProgress.Location.Local);
		});
		this.DrawButton("Add 1000 snout coins", delegate
		{
			GameProgress.AddSnoutCoins(2147483647);
		});
		this.DrawButton("Hitboxes: " + (hitboxesEnabled ? "ON" : "OFF"), delegate
				{
					hitboxesEnabled = !hitboxesEnabled;
					if (!hitboxesEnabled)
					{
						RemoveAllHitboxLines();
					}
				});

// Add this logic right after the button to keep hitboxes active on new items
if (hitboxesEnabled)
{
    foreach (Collider2D col in GameObject.FindObjectsOfType<Collider2D>())
    {
        if (col.gameObject.GetComponent<LineRenderer>() == null)
        {
            LineRenderer line = col.gameObject.AddComponent<LineRenderer>();
            line.name = "HitboxLine";
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = Color.green;
            line.endColor = Color.green;
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            line.loop = true;
            line.useWorldSpace = false; // This keeps the box stuck to the Pig
            line.sortingOrder = 32767; // Draw on top of everything
            
            line.positionCount = 4;
            line.SetPosition(0, new Vector3(-0.6f, -0.6f, 0));
            line.SetPosition(1, new Vector3(0.6f, -0.6f, 0));
            line.SetPosition(2, new Vector3(0.6f, 0.6f, 0));
            line.SetPosition(3, new Vector3(-0.6f, 0.6f, 0));
        }
    }
}
		this.DrawButton("Unlock all craftable items", delegate
		{
			this.UnlockParts(BasePart.PartTier.Common, CustomizationManager.PartFlags.Locked | CustomizationManager.PartFlags.Craftable);
			this.UnlockParts(BasePart.PartTier.Rare, CustomizationManager.PartFlags.Locked | CustomizationManager.PartFlags.Craftable);
			this.UnlockParts(BasePart.PartTier.Epic, CustomizationManager.PartFlags.Locked | CustomizationManager.PartFlags.Craftable);
			this.UnlockParts(BasePart.PartTier.Legendary, CustomizationManager.PartFlags.Locked | CustomizationManager.PartFlags.Craftable);
		});
		this.EndGrid();
		GUILayout.EndScrollView();
		GUI.Label(new Rect((float)Screen.width * 0.9f, (float)Screen.height * 0.93f, (float)Screen.width * 0.1f, (float)Screen.height * 0.1f), string.Concat(new string[]
		{
			"Debug \n(v",
			Singleton<BuildCustomizationLoader>.Instance.ApplicationVersion,
			" - ",
			Singleton<BuildCustomizationLoader>.Instance.SVNRevisionNumber,
			")"
		}));
		if (GUI.Button(new Rect((float)Screen.width * 0.2f, (float)Screen.height * 0.92f, (float)Screen.width * 0.6f, (float)Screen.height * 0.08f), "Back to Main Menu"))
		{
			Singleton<GameManager>.Instance.LoadMainMenu(false);
		}
		GUI.skin = null;
	}

	private void UnlockParts(BasePart.PartTier tier)
	{
		List<BasePart> allTierParts = CustomizationManager.GetAllTierParts(tier, CustomizationManager.PartFlags.Locked | CustomizationManager.PartFlags.Craftable | CustomizationManager.PartFlags.Rewardable);
		if (allTierParts == null || allTierParts.Count == 0)
		{
			return;
		}
		for (int i = 0; i < allTierParts.Count; i++)
		{
			CustomizationManager.UnlockPart(allTierParts[i], "Cheat");
		}
	}

	private void UnlockParts(BasePart.PartTier tier, CustomizationManager.PartFlags flags)
	{
		List<BasePart> allTierParts = CustomizationManager.GetAllTierParts(tier, flags);
		if (allTierParts == null || allTierParts.Count == 0)
		{
			return;
		}
		for (int i = 0; i < allTierParts.Count; i++)
		{
			CustomizationManager.UnlockPart(allTierParts[i], "Cheat");
		}
	}

	private void SetStarsCompletion(EpisodeLevelInfo level, int starCount)
	{
		int num = Mathf.Clamp(starCount, 0, 3);
		GameProgress.SetInt(level.sceneName + "_stars", num, GameProgress.Location.Local);
		GameProgress.SetLevelCompleted(level.sceneName);
		if (num > 0)
		{
			GameProgress.SetChallengeCompleted(level.sceneName, 0, true, true);
		}
		if (num > 1)
		{
			GameProgress.SetChallengeCompleted(level.sceneName, 1, true, true);
		}
		if (num > 2)
		{
			GameProgress.SetChallengeCompleted(level.sceneName, 2, true, true);
		}
	}

	private void OnDeviceRegistered(bool result)
	{
		if (result)
		{
			GameProgress.SetBool("TestDeviceRegistered", true, GameProgress.Location.Local);
		}
		this.m_isRegisteringDevice = false;
	}

	private void OnDeviceUnregistered(bool result)
	{
		if (result)
		{
			GameProgress.SetBool("TestDeviceRegistered", false, GameProgress.Location.Local);
		}
		this.m_isRegisteringDevice = false;
	}

private void ApplyHitboxesToAll()
    {
        // Finds everything with physics
        foreach (Collider2D col in GameObject.FindObjectsOfType<Collider2D>())
        {
            // Only add a line if it doesn't have one yet
            if (col.gameObject.GetComponent<LineRenderer>() == null)
            {
                LineRenderer line = col.gameObject.AddComponent<LineRenderer>();
                line.name = "HitboxLine";
                
                // Neon green look
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = Color.green;
                line.endColor = Color.green;
                line.startWidth = 0.08f;
                line.endWidth = 0.08f;
                line.loop = true;
                line.useWorldSpace = false; // Makes it follow the Pig
                line.sortingOrder = 32767; // Draw on top of everything
                
                // Draw the square
                line.positionCount = 4;
                line.SetPosition(0, new Vector3(-0.6f, -0.6f, 0));
                line.SetPosition(1, new Vector3(0.6f, -0.6f, 0));
                line.SetPosition(2, new Vector3(0.6f, 0.6f, 0));
                line.SetPosition(3, new Vector3(-0.6f, 0.6f, 0));
            }
        }
    }

    private void RemoveAllHitboxLines()
    {
        // Use UnityEngine.Object to avoid the CS0104 error
        foreach (LineRenderer line in GameObject.FindObjectsOfType<LineRenderer>())
        {
            if (line.name == "HitboxLine")
            {
                UnityEngine.Object.Destroy(line);
            }
        }
    }

	private void ToggleHitboxVisibility(bool visible)
	{
		// This finds all objects named "Hitbox" and enables/disables them
		// You may need to change "Hitbox" to the actual name used in your game
		foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
		{
			if (obj.name.ToLower().Contains("hitbox"))
			{
				MeshRenderer mr = obj.GetComponent<MeshRenderer>();
				if (mr != null) mr.enabled = visible;
			}
		}
	}

	[SerializeField]
	private TextMesh textMesh;

	private float m_buttonHeight;

	private float m_buttonWidth;

	private bool m_isRegisteringDevice;

	private GUISkin cheatSkin;

	private bool skinInitialized;

	private Vector2 scrollbarPosition = Vector2.zero;

	private int rowItems = 3;

	public static string versionStatusCheat = "cheatMimicOlderVersion";

	private static List<string> gameModeNames = new List<string>
	{
		"None",
		"Cake Race\nPreview Mode"
	};

	private int currentButtonIndex;
}
