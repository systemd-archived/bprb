using System.Collections.Generic;
using UnityEngine;

public class ParallaxManager : MonoBehaviour
{
    private void Start()
    {
        try
        {
            this.m_backgroundLayerFurther = GameObject.FindGameObjectsWithTag("ParallaxLayerFurther");
        }
        catch
        {
            this.m_backgroundLayerFurther = new GameObject[0];
        }
        try
        {
            this.m_backgroundLayerFar = GameObject.FindGameObjectsWithTag("ParallaxLayerFar");
        }
        catch
        {
            this.m_backgroundLayerFar = new GameObject[0];
        }
        try
        {
            this.m_backgroundLayerNear = GameObject.FindGameObjectsWithTag("ParallaxLayerNear");
        }
        catch
        {
            this.m_backgroundLayerNear = new GameObject[0];
        }
        try
        {
            this.m_backgroundLayerSky = GameObject.FindGameObjectsWithTag("ParallaxLayerSky");
        }
        catch
        {
            this.m_backgroundLayerSky = new GameObject[0];
        }
        try
        {
            this.m_backgroundLayerFixedFollowCamera = GameObject.FindGameObjectsWithTag("ParallaxLayerFixedFollowCamera");
        }
        catch
        {
            this.m_backgroundLayerFixedFollowCamera = new GameObject[0];
        }
        int num = 0;
        try
        {
            num = GameObject.FindGameObjectsWithTag("ParallaxLayerForeground").Length;
            this.m_backgroundLayerForeground = GameObject.FindGameObjectsWithTag("ParallaxLayerForeground");
        }
        catch
        {
            this.m_backgroundLayerForeground = new GameObject[0];
        }
        foreach (GameObject gameObject in this.m_backgroundLayerForeground)
        {
            if (gameObject != null)
            {
                gameObject.AddComponent<BaseTransform>();
            }
        }
        foreach (GameObject gameObject2 in this.m_backgroundLayerFar)
        {
            if (gameObject2 != null)
            {
                gameObject2.AddComponent<BaseTransform>();
            }
        }
        foreach (GameObject gameObject3 in this.m_backgroundLayerNear)
        {
            if (gameObject3 != null)
            {
                gameObject3.AddComponent<BaseTransform>();
            }
        }
        foreach (GameObject gameObject4 in this.m_backgroundLayerFurther)
        {
            if (gameObject4 != null)
            {
                gameObject4.AddComponent<BaseTransform>();
            }
        }
        foreach (GameObject gameObject5 in this.m_backgroundLayerSky)
        {
            if (gameObject5 != null)
            {
                gameObject5.AddComponent<BaseTransform>();
            }
        }
        foreach (GameObject gameObject6 in this.m_backgroundLayerFixedFollowCamera)
        {
            if (gameObject6 != null)
            {
                gameObject6.AddComponent<BaseTransform>();
            }
        }
        foreach (ParallaxCustomLayer parallaxCustomLayer in this.m_miscellanousLayer)
        {
            if (parallaxCustomLayer.layer == null)
            {
                continue;
            }
            parallaxCustomLayer.layer.AddComponent<BaseTransform>();
        }
        if (num > 0 && this.m_backgroundLayerForeground.Length > 0 && this.m_backgroundLayerForeground[0] != null)
        {
            this.m_fgLimitY = this.m_backgroundLayerForeground[0].transform.position.y;
        }
        this.m_oldPosition = base.transform.position;
    }

    private void SetHorizontalPosition(GameObject[] objects, float scale)
    {
        foreach (GameObject gameObject in objects)
        {
            if (gameObject == null)
            {
                continue;
            }
            BaseTransform component = gameObject.GetComponent<BaseTransform>();
            if (component == null)
            {
                continue;
            }
            Vector3 position = gameObject.transform.position;
            position.x = component.position.x + this.m_offset.x * scale;
            gameObject.transform.position = position;
        }
    }

    private void Update()
    {
        float num = base.transform.position.x - this.m_oldPosition.x;
        float num2 = base.transform.position.y - this.m_oldPosition.y;
        this.m_offset.x = this.m_offset.x + num;
        if (num != 0f)
        {
            this.SetHorizontalPosition(this.m_backgroundLayerForeground, -0.4f);
            this.SetHorizontalPosition(this.m_backgroundLayerFurther, 0.7f);
            this.SetHorizontalPosition(this.m_backgroundLayerFar, 0.6f);
            this.SetHorizontalPosition(this.m_backgroundLayerNear, 0.4f);
            this.SetHorizontalPosition(this.m_backgroundLayerSky, 0.8f);
            this.SetHorizontalPosition(this.m_backgroundLayerFixedFollowCamera, 1f);
            for (int i = 0; i < this.m_miscellanousLayer.Count; i++)
            {
                ParallaxCustomLayer parallaxCustomLayer = this.m_miscellanousLayer[i];
                if (parallaxCustomLayer.layer == null)
                {
                    continue;
                }
                BaseTransform component = parallaxCustomLayer.layer.GetComponent<BaseTransform>();
                if (component == null)
                {
                    continue;
                }
                Vector3 position = parallaxCustomLayer.layer.transform.position;
                position.x = component.position.x + this.m_offset.x * parallaxCustomLayer.speedX;
                parallaxCustomLayer.layer.transform.position = position;
            }
        }
        if (num2 != 0f)
        {
            for (int j = 0; j < this.m_backgroundLayerForeground.Length; j++)
            {
                GameObject gameObject = this.m_backgroundLayerForeground[j];
                if (gameObject == null)
                {
                    continue;
                }
                Vector3 vector = gameObject.transform.position;
                if (vector.y <= this.m_fgLimitY)
                {
                    vector -= Vector3.up * num2 * 0.2f;
                }
                else
                {
                    vector.y = this.m_fgLimitY;
                }
                gameObject.transform.position = vector;
            }
        }
        this.m_oldPosition = base.transform.position;
    }

    public void RegisterParallaxLayer(ParallaxCustomLayer pcl)
    {
        this.m_miscellanousLayer.Add(pcl);
        pcl.layer.AddComponent<BaseTransform>();
    }

    protected GameObject[] m_backgroundLayerFurther;

    protected GameObject[] m_backgroundLayerFar;

    protected GameObject[] m_backgroundLayerNear;

    protected GameObject[] m_backgroundLayerSky;

    protected GameObject[] m_backgroundLayerForeground;

    protected GameObject[] m_backgroundLayerFixedFollowCamera;

    protected float m_fgLimitY;

    protected List<ParallaxCustomLayer> m_miscellanousLayer = new List<ParallaxCustomLayer>();

    protected Vector3 m_offset;

    protected Vector3 m_oldPosition;

    public struct ParallaxCustomLayer
    {
        public GameObject layer;

        public float speedX;
    }
}
