using System.Collections;
using UnityEngine;

public class AdvancedMusicGenerator : MonoBehaviour
{
	public AudioSource audioSource;
	private float[] noteFrequencies = new float[] {
		261.63f, // C4
        293.66f, // D4
        329.63f, // E4
        349.23f, // F4
        392.00f, // G4
        440.00f, // A4
        493.88f, // B4
        523.25f  // C5
    };
	private int[][] chordProgressions = new int[][] {
		new int[] { 0, 4, 5, 3 }, // I-V-vi-IV
        new int[] { 5, 4, 0, 3 }  // vi-V-I-IV
    };
	private int currentChordProgressionIndex = 0;
	private GameObject player;
	private Vector3 lastPlayerPosition;
	private float movementThreshold = 1.0f;

	void Start()
	{
		audioSource = gameObject.AddComponent<AudioSource>();
		player = GameObject.FindGameObjectWithTag( "Player" );
		lastPlayerPosition = player.transform.position;
		StartCoroutine( PlaySong() );
	}

	IEnumerator PlaySong()
	{
		while( true )
		{
			int[] currentProgression = chordProgressions[currentChordProgressionIndex % chordProgressions.Length];
			foreach( int chordRoot in currentProgression )
			{
				int[] melodyNotes = GetMelodyNotesForChord( chordRoot );
				foreach( var noteIndex in melodyNotes )
				{
					// Make sure to stay within the array bounds
					if( noteIndex < noteFrequencies.Length )
					{
						PlayNoteBasedOnPlayerDistance( noteFrequencies[noteIndex] );
						yield return new WaitForSeconds( 0.5f ); // Adjust based on tempo
					}
				}
			}
			// Check player movement to decide if we change the progression
			ChangeChordProgressionBasedOnMovement();
		}
	}

	void ChangeChordProgressionBasedOnMovement()
	{
		if( Vector3.Distance( player.transform.position, lastPlayerPosition ) > movementThreshold )
		{
			currentChordProgressionIndex++;
			lastPlayerPosition = player.transform.position;
		}
	}

	void PlayNoteBasedOnPlayerDistance( float frequency )
	{
		AudioClip clip = CreateToneAudioClip( frequency, true, 0.01f, 0.1f, 0.7f, 0.2f );

		// Dynamic volume based on distance
		float distance = Vector3.Distance( player.transform.position, transform.position );
		audioSource.volume = Mathf.Clamp( 1 - ( distance / 10 ), 0.1f, 1 );

		audioSource.PlayOneShot( clip );
	}

	AudioClip CreateToneAudioClip( float frequency, bool useSquareWave, float attack, float decay, float sustain, float release )
	{
		int sampleDurationSecs = 1;
		int sampleRate = 44100;
		int totalSamples = sampleRate * sampleDurationSecs;
		float[] samples = new float[totalSamples];

		for( int i = 0; i < totalSamples; i++ )
		{
			float t = i / ( float )sampleRate;
			// Choose waveform based on parameter
			float waveform = useSquareWave ? SquareWave( t, frequency ) : Mathf.Sin( 2 * Mathf.PI * frequency * t );
			// Apply ADSR envelope
			samples[i] = waveform * ADSREnvelope( i, sampleRate, attack, decay, sustain, release, totalSamples );
		}

		AudioClip clip = AudioClip.Create( "Tone", totalSamples, 1, sampleRate, false );
		clip.SetData( samples, 0 );
		return clip;
	}

	int[] GetMelodyNotesForChord( int chordRoot )
	{
		// Simple triad: root, third, fifth
		return new int[] { chordRoot, ( chordRoot + 2 ) % noteFrequencies.Length, ( chordRoot + 4 ) % noteFrequencies.Length };
	}

	float ADSREnvelope( int sampleIndex, int sampleRate, float attack, float decay, float sustainLevel, float release, int totalSamples )
	{
		float time = sampleIndex / ( float )sampleRate;
		float attackTime = attack;
		float decayTime = attack + decay;
		float releaseTime = release;

		if( time <= attackTime )
		{
			// Attack phase: linear increase from 0 to 1
			return time / attackTime;
		}
		else if( time <= decayTime )
		{
			// Decay phase: linear decrease from 1 to sustain level
			return 1 - ( ( time - attackTime ) / ( decayTime - attackTime ) ) * ( 1 - sustainLevel );
		}
		else if( sampleIndex <= totalSamples - sampleRate * release )
		{
			// Sustain phase: maintain sustain level
			return sustainLevel;
		}
		else
		{
			// Release phase: linear decrease from sustain level to 0
			float releasePhaseDuration = time - ( totalSamples / ( float )sampleRate - releaseTime );
			return sustainLevel * ( 1 - releasePhaseDuration / releaseTime );
		}
	}

	float SquareWave( float t, float frequency )
	{
		return Mathf.Sign( Mathf.Sin( 2 * Mathf.PI * frequency * t ) );
	}
}
