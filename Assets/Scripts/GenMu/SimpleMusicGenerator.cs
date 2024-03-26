using System.Collections;
using UnityEngine;

public class SimpleMusicGenerator : MonoBehaviour
{
	public AudioSource audioSource;
	public AudioClip[] notes; // Assign this in the inspector, ordered by scale degree
	private int[] melodyPattern = { 0, 2, 4, 5, 7, 9, 11, 12, 14, 12, 11, 9, 7, 5, 4, 2 }; // Simple melody pattern using scale degrees
	private float[] rhythmPattern = { 1.0f, 0.5f, 0.5f, 1.0f, 1.5f, 0.5f, 1.0f, 0.5f, 0.5f, 1.0f, 1.5f, 0.5f, 1.0f, 0.5f, 0.5f, 1.0f }; // Rhythm pattern

	private void Start()
	{
		StartCoroutine( PlayMelody() );
	}

	IEnumerator PlayMelody()
	{
		int patternIndex = 0; // Start at the beginning of the melody pattern
		while( true )
		{
			int noteIndex = melodyPattern[patternIndex % melodyPattern.Length]; // Loop through the melody pattern
			float noteLength = rhythmPattern[patternIndex % rhythmPattern.Length]; // Get the corresponding rhythm
			audioSource.PlayOneShot( notes[noteIndex] );
			yield return new WaitForSeconds( noteLength ); // Wait for the duration of the note before playing the next
			patternIndex++;
		}
	}
}
