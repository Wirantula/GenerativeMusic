using UnityEngine;

[RequireComponent( typeof( ParticleSystem ) )]
public class SimpleVisualEffect : MonoBehaviour
{
	private ParticleSystem particleSystem;
	private ParticleSystem.MainModule mainModule;
	private ParticleSystem.EmissionModule emissionModule;
	private ParticleSystem.MinMaxGradient originalColor;

	void Start()
	{
		particleSystem = GetComponent<ParticleSystem>();
		mainModule = particleSystem.main;
		emissionModule = particleSystem.emission;
		originalColor = mainModule.startColor; // Store the original color to base modifications on
	}

	void Update()
	{
		// Example of dynamic visual changes based on a music parameter (e.g., volume, pitch)
		// For demonstration, we'll use Time.time to simulate changing music properties

		// Change color over time
		float hue = Mathf.PingPong( Time.time * 0.1f, 1 );
		mainModule.startColor = Color.HSVToRGB( hue, 1, 1 );

		// Modify emission rate to simulate responsiveness to music intensity
		float emissionRate = Mathf.PingPong( Time.time * 10f, 50 ) + 10; // Ranges from 10 to 60
		emissionModule.rateOverTime = emissionRate;

		// Change particle size based on another music property (e.g., pitch)
		float size = Mathf.PingPong( Time.time * 0.1f, 0.5f ) + 0.1f; // Ranges from 0.1 to 0.6
		mainModule.startSize = size;
	}
}
