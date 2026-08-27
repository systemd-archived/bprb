using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000DF RID: 223
public class FPSCounter : MonoBehaviour
{
	// Token: 0x06000876 RID: 2166 RVA: 0x0003637B File Offset: 0x0003457B
	private void Start()
	{
		if (this.m_fps == null)
		{
			this.m_fps = base.GetComponent<Text>();
		}
		if (this.m_stopwatch == null)
		{
			this.m_stopwatch = new Stopwatch();
		}
		this.m_stopwatch.Start();
		this.timeleft = this.updateInterval;
	}

	// Token: 0x06000877 RID: 2167 RVA: 0x000363B0 File Offset: 0x000345B0
	private void Update()
	{
		this.timeleft -= Time.deltaTime;
		this.frames += 1f;
		if (this.timeleft <= 0f)
		{
			float num = (float)this.m_stopwatch.ElapsedMilliseconds / 1000f;
			if (num > 0f)
			{
				float num2 = this.frames / num;
				if (this.m_fps != null)
				{
					this.m_fps.text = num2.ToString("F2") + " FPS";
				}
			}
			this.frames = 0f;
			this.timeleft = this.updateInterval;
			this.m_stopwatch.Restart();
		}
	}

	// Token: 0x040007E9 RID: 2025
	[SerializeField]
	private Text m_fps;

	// Token: 0x040007EA RID: 2026
	private float updateInterval = 0.5f;

	// Token: 0x040007EB RID: 2027
	private float frames;

	// Token: 0x040007EC RID: 2028
	private float timeleft;

	// Token: 0x040007ED RID: 2029
	private Stopwatch m_stopwatch = new Stopwatch();
}
