using System;
using UnityEngine;

public class HUD : WPFMonoBehaviour
{
    private void Start()
    {
        if (WPFMonoBehaviour.gameData.m_blueprintPrefab)
        {
            // 1. Instantiate the prefab
            Transform blueprintTransform = UnityEngine.Object.Instantiate<Transform>(
                WPFMonoBehaviour.gameData.m_blueprintPrefab, 
                Vector3.zero, 
                Quaternion.identity
            );

            blueprintTransform.name = "BlueprintUI";

            // 2. Parent to the game's specific GUI root
            GameObject guiRoot = GameObject.Find("InGameGUI");
            if (guiRoot != null)
                blueprintTransform.SetParent(guiRoot.transform, false);

            // 3. Position and Scale
            blueprintTransform.localPosition = new Vector3(0, 5f, 0); // World-style units
            blueprintTransform.localScale = Vector3.one;

            // 4. ADD THE GAME'S CUSTOM BUTTON COMPONENT
            // This is the class from your Button.cs file
            Button customBtn = blueprintTransform.gameObject.GetComponent<Button>();
            if (customBtn == null) 
            {
                customBtn = blueprintTransform.gameObject.AddComponent<Button>();
            }

            // 5. Setup the Collider (Required for the Button to work)
            BoxCollider collider = blueprintTransform.gameObject.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = blueprintTransform.gameObject.AddComponent<BoxCollider>();
            }
            // Size the collider to match the button size
            collider.size = new Vector3(2f, 2f, 1f); 

            // 6. Apply the Texture/Sprite to the Renderer
            // Bad Piggies often uses SpriteRenderer for these widgets
            if (WPFMonoBehaviour.levelManager.m_blueprintTexture)
            {
                SpriteRenderer renderer = blueprintTransform.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    Texture2D tex = WPFMonoBehaviour.levelManager.m_blueprintTexture;
                    renderer.sprite = UnityEngine.Sprite.Create(
                        tex, 
                        new Rect(0f, 0f, tex.width, tex.height), 
                        new Vector2(0.5f, 0.5f)
                    );
                }
            }
        }
    }
}