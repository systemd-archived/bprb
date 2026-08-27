using System;
using UnityEngine;

public class Sandbag : BasePart
{
	public override void Awake()
	{
		base.Awake();
		// Auto-find visualization nodes if not assigned
		if (!this.m_actualVisualizationNode)
		{
			Transform found = base.transform.Find("ActualVisualization") ?? base.transform.Find("Model") ?? base.transform.Find("Mesh") ?? base.transform.Find("Sprite");
			if (found)
			{
				this.m_actualVisualizationNode = found.gameObject;
				Debug.Log("Sandbag: Auto-assigned m_actualVisualizationNode to " + found.name);
			}
		}
		if (!this.m_gridVisualizationNode)
		{
			Transform found = base.transform.Find("GridVisualization") ?? base.transform.Find("Grid") ?? base.transform.Find("Overlay");
			if (found)
			{
				this.m_gridVisualizationNode = found.gameObject;
			}
		}
		if (this.m_actualVisualizationNode && this.m_gridVisualizationNode)
		{
			this.m_actualVisualizationNode.SetActive(false);
			this.m_gridVisualizationNode.SetActive(true);
		}
	}

	public override bool IsIntegralPart()
	{
		return false;
	}

	public bool IsAttached()
	{
		return !this.m_dropped;
	}

	public override void Initialize()
	{
		// Force visuals correctly at start
		if (this.m_actualVisualizationNode) this.m_actualVisualizationNode.SetActive(true);
		if (this.m_gridVisualizationNode) this.m_gridVisualizationNode.SetActive(false);

		// Find connection above
		this.m_connectedPart = base.contraption.FindPartAt(this.m_coordX, this.m_coordY + 1);
		
		if (this.m_connectedPart)
		{
			this.m_dropped = false;
			base.contraption.ChangeOneShotPartAmount(BasePart.BaseType(this.m_partType), this.EffectDirection(), 1);
		}
		else
		{
			this.m_dropped = true;
			// Use "Default" to ensure it hits the ground if "DroppedSandbag" layer is broken
			base.gameObject.layer = LayerMask.NameToLayer("Default");
		}

		this.m_partType = BasePart.PartType.Sandbag;

		// --- FIXED RECURSION (No more freezing Unity) ---
		if (this.m_numberOfBalloons > 1)
		{
			int remainingBalloons = this.m_numberOfBalloons - 1;
			this.m_numberOfBalloons = 1; // Set current to 1 so IT doesn't spawn more

			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(base.gameObject);
			gameObject.transform.position = base.transform.position;
			Sandbag component = gameObject.GetComponent<Sandbag>();
			
			component.m_numberOfBalloons = remainingBalloons;
			base.contraption.AddRuntimePart(component);
			gameObject.transform.parent = base.contraption.transform;
		}

		// Physics Setup
		if (!base.gameObject.GetComponent<SphereCollider>())
		{
			SphereCollider sphereCollider = base.gameObject.AddComponent<SphereCollider>();
			sphereCollider.radius = 0.2f; // Increased radius to prevent noclip
			sphereCollider.center = new Vector3(0f, -0.05f, 0f);
		}

		if (!base.rigidbody) base.rigidbody = base.gameObject.AddComponent<Rigidbody>();
		base.rigidbody.mass = this.m_mass;
		base.rigidbody.drag = 1f;
		base.rigidbody.angularDrag = 10f;
		base.rigidbody.constraints = (RigidbodyConstraints)56;

		if (this.m_connectedPart)
		{
			Vector3 position = base.transform.position;
			base.transform.position = this.m_connectedPart.transform.position - Vector3.up * 0.5f;
			SpringJoint springJoint = base.gameObject.AddComponent<SpringJoint>();
			springJoint.connectedBody = this.m_connectedPart.rigidbody;
			this.m_connectedLocalPos = this.m_connectedPart.transform.InverseTransformPoint(base.transform.position);
			
			springJoint.minDistance = 0f;
			springJoint.maxDistance = 0.5f;
			springJoint.anchor = Vector3.up * 0.5f;
			springJoint.spring = 100f;
			springJoint.damper = 10f;

			LineRenderer lineRenderer = base.gameObject.AddComponent<LineRenderer>();
			lineRenderer.material = this.m_stringMaterial;
			lineRenderer.positionCount = 2;
			lineRenderer.startWidth = 0.05f;
			lineRenderer.endWidth = 0.05f;
			lineRenderer.startColor = Color.black;
			lineRenderer.endColor = Color.black;
		}
	}

	protected override void OnTouch()
	{
		this.Drop();
	}

	public void Drop()
	{
		if (!this.m_dropped)
		{
			this.m_dropped = true;
			SpringJoint component = base.GetComponent<SpringJoint>();
			base.contraption.ChangeOneShotPartAmount(BasePart.BaseType(this.m_partType), this.EffectDirection(), -1);
			base.gameObject.layer = LayerMask.NameToLayer("Default");

			if (component && component.connectedBody)
			{
				component.connectedBody.AddForce(5f * Vector3.up, ForceMode.Impulse);
				base.rigidbody.AddForce(-4f * Vector3.up, ForceMode.Impulse);
			}
			if (component) UnityEngine.Object.Destroy(component);
			
			LineRenderer component2 = base.GetComponent<LineRenderer>();
			if (component2) UnityEngine.Object.Destroy(component2);
		}
	}

	public new void LateUpdate()
	{
		base.LateUpdate();
		SpringJoint component = base.GetComponent<SpringJoint>();
		LineRenderer component2 = base.GetComponent<LineRenderer>();
		
		if (!component || !component2) return;

		Vector3 position = base.transform.position + base.transform.up * 0.4f;
		if (component.connectedBody)
		{
			Vector3 position2 = component.connectedBody.transform.TransformPoint(this.m_connectedLocalPos);
			component2.SetPosition(0, position);
			component2.SetPosition(1, position2);
		}
	}

	public bool m_inWorldCoordinates = true;
	public Vector3 m_direction = Vector3.up;
	public int m_numberOfBalloons = 1;
	public bool m_dropped;
	public Material m_stringMaterial;
	public GameObject m_actualVisualizationNode;
	public GameObject m_gridVisualizationNode;
	protected BasePart m_connectedPart;
	protected Vector3 m_connectedLocalPos;
}