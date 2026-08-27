using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000101 RID: 257
[RequireComponent(typeof(Image))]
public class GUITextureAspectFix : MonoBehaviour
{
	// Token: 0x06000A7B RID: 2683 RVA: 0x0003F1B4 File Offset: 0x0003D3B4
	private void Awake()
	{
		this.m_image = base.GetComponent<Image>();
		this.m_rectTransform = base.GetComponent<RectTransform>();
		this.m_origSizeDelta = this.m_rectTransform.sizeDelta;
		this.FixAspect();
	}

	// Token: 0x06000A7C RID: 2684 RVA: 0x0003F1E5 File Offset: 0x0003D3E5
	private void LateUpdate()
	{
		this.FixAspect();
	}

	// Token: 0x06000A7D RID: 2685 RVA: 0x0003F1F0 File Offset: 0x0003D3F0
	private void FixAspect()
	{
		if (this.m_image.sprite == null)
		{
			return;
		}
		float width = this.m_image.sprite.rect.width;
		float height = this.m_image.sprite.rect.height;
		float num = (float)Screen.width / (float)Screen.height;
		float num2 = width / height;
		Vector2 origSizeDelta = this.m_origSizeDelta;
		origSizeDelta.y *= num / num2;
		this.m_rectTransform.sizeDelta = origSizeDelta;
	}

	// Token: 0x0400091A RID: 2330
	private Image m_image;

	// Token: 0x0400091B RID: 2331
	private RectTransform m_rectTransform;

	// Token: 0x0400091C RID: 2332
	private Vector2 m_origSizeDelta;
}
