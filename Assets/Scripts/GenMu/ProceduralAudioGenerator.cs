using UnityEngine;

public class ProceduralAudioGenerator : MonoBehaviour
{
	private AudioSource audioSource;
	private AudioClip[] noteClips;
	private float[] noteFrequencies = new float[] { 261.63f, 293.66f, 329.63f, 349.23f, 392.00f, 440.00f, 493.88f, 523.25f }; // Frequencies for C4 to C5

	void Start()
	{
		audioSource = gameObject.AddComponent<AudioSource>();
		GenerateNoteClips();
		StartCoroutine( PlayScale() );
	}

	void GenerateNoteClips()
	{
		noteClips = new AudioClip[noteFrequencies.Length];
		for( int i = 0; i < noteFrequencies.Length; i++ )
		{
			noteClips[i] = CreateToneAudioClip( noteFrequencies[i] );
		}
	}

	AudioClip CreateToneAudioClip( float frequency )
	{
		int sampleDurationSecs = 1;
		int sampleRate = 44100;
		int sampleLength = sampleRate * sampleDurationSecs;
		float[] samples = new float[sampleLength];

		for( int i = 0; i < sampleLength; i++ )
		{
			float t = ( float )i / sampleRate;
			samples[i] = Mathf.Sin( 2 * Mathf.PI * frequency * t );
		}

		AudioClip clip = AudioClip.Create( "Tone", sampleLength, 1, sampleRate, false );
		clip.SetData( samples, 0 );
		return clip;
	}

	System.Collections.IEnumerator PlayScale()
	{
		foreach( var clip in noteClips )
		{
			audioSource.clip = clip;
			audioSource.Play();
			yield return new WaitForSeconds( 1 ); // Wait for the note to finish
		}
	}
}
