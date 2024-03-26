using UnityEngine;
using static UnityEngine.ParticleSystem;

[RequireComponent( typeof( ParticleSystem ) )]
public class DynamicVisualEffect : MonoBehaviour
{
	public GameObject player; // Assign the player GameObject in the inspector
	private ParticleSystem particleSystem;
	private ParticleSystem.MainModule mainModule;
	private ParticleSystem.EmissionModule emissionModule;
	private ParticleSystemRenderer particleRenderer;
	private Material particleMaterial;
	public Color closeColor = Color.red; // Color when the player is close
	public Color farColor = Color.blue; // Color when the player is far
	[SerializeField]
	private float proximityThreshold = 5f; // Distance threshold for color change

	void Start()
	{
		particleSystem = GetComponent<ParticleSystem>();
		particleRenderer = GetComponent<ParticleSystemRenderer>();
		particleMaterial = particleRenderer.material; // Create an instance of the material
		mainModule = particleSystem.main;
		emissionModule = particleSystem.emission;
	}

	void Update()
	{
		// Calculate the distance between the player and this object
		float distanceToPlayer = Vector3.Distance( player.transform.position, transform.position );

		// Lerp the color based on the player's distance
		float lerpFactor = Mathf.Clamp01( ( distanceToPlayer / proximityThreshold ) );
		Color targetColor = Color.Lerp( closeColor, farColor, lerpFactor );

		// Apply the lerped color to the material
		particleMaterial.color = targetColor;

		// Modify emission rate to simulate responsiveness to music intensity
		float emissionRate = Mathf.PingPong( Time.time * 10f, 50 ) + 10; // Ranges from 10 to 60
		emissionModule.rateOverTime = emissionRate;

		// Change particle size based on another music property (e.g., pitch)
		float size = Mathf.PingPong( Time.time * 0.1f, 0.5f ) + 0.1f; // Ranges from 0.1 to 0.6
		mainModule.startSize = size;
	}
}
