using System;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : BasePart
{
	public override void Awake()
	{
		base.Awake();
		base.enabled = false;
		if ((bool)m_actualVisualizationNode && (bool)m_gridVisualizationNode)
		{
			SetRenderersInChildred(m_actualVisualizationNode, enable: false);
			SetRenderersInChildred(m_gridVisualizationNode, enable: true);
		}
	}

	private void SetRenderersInChildred(GameObject target, bool enable)
	{
		if (target == null)
		{
			return;
		}
		Renderer[] componentsInChildren = target.GetComponentsInChildren<Renderer>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = enable;
			}
		}
	}

	public override bool IsIntegralPart()
	{
		return false;
	}

	public override void PrePlaced()
	{
		base.PrePlaced();
		// Removed mod-specific rotation check
	}

	public override void Initialize()
	{
		if ((bool)m_actualVisualizationNode && (bool)m_gridVisualizationNode)
		{
			SetRenderersInChildred(m_gridVisualizationNode, enable: false);
			SetRenderersInChildred(m_actualVisualizationNode, enable: true);
		}

		// Standard connection logic: Looks up to 5 blocks down
		int x = 0;
		int y = 1;
		int searchDistance = 5; 
		
		for (int i = 1; i <= searchDistance; i++)
		{
			if ((bool)m_connectedPart)
			{
				break;
			}

			if ((bool)m_connectedPart && !m_connectedPart.IsPartOfChassis() && m_connectedPart.m_partType != PartType.Pig && m_connectedPart.m_partType != PartType.Kicker)
			{
				m_connectedPart = null;
			}
		}

		m_partType = PartType.Balloon;
		base.contraption.ChangeOneShotPartAmount(BasePart.BaseType(m_partType), EffectDirection(), 1);

		if (m_numberOfBalloons > 1)
		{
			GameObject obj = UnityEngine.Object.Instantiate(base.gameObject, base.contraption.transform, worldPositionStays: true);
			obj.transform.position = base.transform.position;
			Balloon component = obj.GetComponent<Balloon>();
			component.m_numberOfBalloons = m_numberOfBalloons - 1;
			base.contraption.AddRuntimePart(component);
		}
		
		this.EnsureRigidbody();
		base.rigidbody.mass = 0.1f;
		base.rigidbody.drag = 2f;
		base.rigidbody.angularDrag = 0.5f;
		base.rigidbody.constraints = (RigidbodyConstraints)48;
        
		if ((bool)m_connectedPart)
		{
			m_connectedPart.EnsureRigidbody();
			Vector3 position = base.transform.position;
			float dist = Vector3.Distance(m_connectedPart.transform.position, position) - 0.5f;
			float pigOffset = 0f;
			Vector3 anchorPos;

			if (m_connectedPart.m_partType == PartType.Pig)
			{
				anchorPos = Vector3.zero;
				pigOffset = 0.3f;
			}
			else
			{
				anchorPos = Vector3.up * 0.5f;
			}

			base.transform.position = m_connectedPart.transform.position + anchorPos;
			m_springJoint = base.gameObject.AddComponent<SpringJoint>();
			m_springJoint.connectedBody = m_connectedPart.rigidbody;
			base.contraption.AddJointToMap(this, m_connectedPart, m_springJoint);
			
			m_springJoint.minDistance = 0f;
			m_springJoint.maxDistance = UnityEngine.Random.Range(0.8f, 1.2f) * dist + pigOffset;
			m_springJoint.anchor = Vector3.up * -0.5f;
			m_springJoint.spring = 100f;
			m_springJoint.damper = 10f;
			m_springJoint.enablePreprocessing = false;

			m_balancer = m_connectedPart.gameObject.GetComponent<BalloonBalancer>() ?? m_connectedPart.gameObject.AddComponent<BalloonBalancer>();
			m_balancer.AddBalloon();

			Transform transform = (bool)m_actualVisualizationNode ? m_actualVisualizationNode.transform : base.transform;
			m_rope = transform.gameObject.AddComponent<RopeVisualization>();
			m_connectedLocalPos = m_connectedPart.transform.InverseTransformPoint(base.transform.position);
			
			m_springJoint.autoConfigureConnectedAnchor = false;
			m_springJoint.connectedAnchor = m_connectedLocalPos;
			m_rope.m_stringMaterial = m_stringMaterial;
			m_rope.m_pos1Anchor = Vector3.up * -0.5f + 1.1f * Vector3.forward;
			m_rope.m_pos2Transform = m_connectedPart.transform;
			m_rope.m_pos2Anchor = m_connectedLocalPos + 1.1f * Vector3.forward;

			base.transform.position = position + UnityEngine.Random.Range(-1f, 1f) * Vector3.forward + UnityEngine.Random.Range(-1f, 1f) * Vector3.right * 0.5f;
		}
	}

	public void ConfigureExtraBalanceJoint(float powerFactor)
	{
		if ((bool)m_balancer)
		{
			m_balancer.Configure(powerFactor);
		}
	}

	private float LimitForceForSpeed(float forceMagnitude, Vector3 forceDir)
	{
		Vector3 velocity = base.rigidbody.velocity;
		float dot = Vector3.Dot(velocity.normalized, forceDir);
		if (dot > 0f)
		{
			Vector3 relativeVel = velocity * dot;
			if (relativeVel.magnitude > m_maximumSpeed)
			{
				return forceMagnitude / (1f + relativeVel.magnitude - m_maximumSpeed);
			}
		}
		return forceMagnitude;
	}

	public void FixedUpdate()
	{
		float liftForce = LimitForceForSpeed(m_force, m_direction);
		base.rigidbody.AddForce(liftForce * m_direction, ForceMode.Force);
		if (m_rope != null && (m_springJoint == null || m_connectedPart == null))
		{
			Pop();
		}
	}

	protected override void OnTouch()
	{
		Pop();
	}

	public override void OnCollisionEnter(Collision coll)
	{
		ContactPoint[] contacts = coll.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			if (contactPoint.otherCollider.gameObject.layer == BasePart.m_groundLayer || 
                contactPoint.otherCollider.gameObject.layer == BasePart.m_iceGroundLayer || 
                contactPoint.otherCollider.CompareTag("Sharp"))
			{
				Pop();
				break;
			}
		}
	}

	public void Pop()
	{
		if (!m_popped)
		{
			m_popped = true;
			AudioSource sfx = ((!m_ghostBalloon) ? WPFMonoBehaviour.gameData.commonAudioCollection.balloonPop : WPFMonoBehaviour.gameData.commonAudioCollection.ghostBalloonPop[UnityEngine.Random.Range(0, WPFMonoBehaviour.gameData.commonAudioCollection.ghostBalloonPop.Length)]);
			Singleton<AudioManager>.Instance.SpawnOneShotEffect(sfx, base.transform.position);
			WPFMonoBehaviour.effectManager.CreateParticles(WPFMonoBehaviour.gameData.m_ballonParticles, base.transform.position);
			
            base.contraption.ChangeOneShotPartAmount(BasePart.BaseType(m_partType), EffectDirection(), -1);
			if ((bool)m_balancer)
			{
				m_balancer.RemoveBalloon();
			}
			CheckBalloonPopperAchievement();
			base.contraption.RemovePart(this);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public override void EnsureRigidbody()
	{
		if (base.rigidbody == null)
		{
			base.rigidbody = base.gameObject.AddComponent<Rigidbody>();
		}
		base.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
	}

	public void CheckBalloonPopperAchievement()
	{
		if (!Singleton<SocialGameManager>.IsInstantiated() || !Singleton<GameManager>.Instance.IsInGame())
		{
			return;
		}
		int poppedBalloons = GameProgress.GetInt("Popped_Balloons") + 1;
		GameProgress.SetInt("Popped_Balloons", poppedBalloons);
		
		Action<List<string>> action = delegate(List<string> achievements)
		{
			foreach (string id in achievements)
			{
				if (Singleton<SocialGameManager>.Instance.TryReportAchievementProgress(id, 100.0, (int limit) => poppedBalloons >= limit))
				{
					break;
				}
			}
		};
		action(new List<string> { "grp.POPPERS_THEORY_III", "grp.POPPERS_THEORY_II", "grp.POPPERS_THEORY_I" });
	}

	public float m_force = 10f;
	public float m_maximumSpeed = 10f;
	public bool m_inWorldCoordinates = true;
	public Vector3 m_direction = Vector3.up;
	public int m_numberOfBalloons = 1;
	public bool m_popped;
	public bool m_ghostBalloon;
	public GameObject m_actualVisualizationNode;
	public GameObject m_gridVisualizationNode;
	public Material m_stringMaterial;
	protected BasePart m_connectedPart;
	protected Vector3 m_connectedLocalPos;
	protected BalloonBalancer m_balancer;
	protected SpringJoint m_springJoint;
	protected RopeVisualization m_rope;
}
