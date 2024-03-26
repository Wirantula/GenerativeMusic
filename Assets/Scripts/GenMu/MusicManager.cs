using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
	public GameObject player;
	public PlayerMovementTracker playerMovementTracker;
	private AudioSource melodySource, harmonySource, bassSource;
	private int currentChordIndex = 0;

	// Example chord progression in the key of C Major: C, F, G, Am
	float[][] chordProgression = new float[][] {
	new float[] { 261.63f, 329.63f, 392.00f }, // C Major
    new float[] { 349.23f, 440.00f, 523.25f }, // F Major
    new float[] { 392.00f, 493.88f, 587.33f }, // G Major
    new float[] { 220.00f, 261.63f, 329.63f }  // A Minor
	};

	float[][] currentChordProgression;

	// Example melody line (simplified)
	float[] melodyLine = new float[] { 261.63f, // C
    293.66f, // D
    329.63f, // E
    349.23f, // F
    392.00f, // G
    440.00f, // A
    493.88f, // B
    523.25f  // C (Higher Octave)
	};

	float[] ffMelodyLine = new float[] {
	329.63f, // E
    392.00f, // G
    440.00f, // A
    392.00f, // G
    329.63f, // E
    293.66f, // D
    261.63f, // C
    293.66f  // D
	};

	float[] currentMelodyLine;

	// Example bass line (simplified)
	float[] bassLine = new float[] {
	87.31f, // F
    98.00f, // G
    65.41f  // C
	};

	float[] ffBassLine = new float[] { 261.63f, // C
    73.42f, // D
    98.00f, // G
    65.41f  // C
	};

	float[] currentBassLine;

	// Hypothetical structures for music components
	float[][] fastForwardChordProgression = new float[][] {
	new float[] { 261.63f, 329.63f, 392.00f }, // C Major
    new float[] { 440.00f, 261.63f, 329.63f }, // F Major
    new float[] { 349.23f, 440.00f, 261.63f }, // G Major
    new float[] { 392.00f, 493.88f, 293.66f }
	};
	float[][] slowForwardChordProgression = new float[][] {
	new float[] { 261.63f, 329.63f, 392.00f }, // C Major
    new float[] { 349.23f, 440.00f, 523.25f }, // F Major
    new float[] { 392.00f, 493.88f, 587.33f }, // G Major
    new float[] { 220.00f, 261.63f, 329.63f }  // A Minor
	};
	// Add more chord progressions for other themes as needed

	[System.Serializable]
	public struct MusicParameters
	{
		public string waveformType;
		public float attack, decay, sustain, release;
		public float tempo; // Beats per minute (BPM)
		public float modIndex;
		public float modFrequency;
		public string modType;
		public string modFMAM;
	}

	public MusicParameters currentHarmonyParameters;
	public MusicParameters currentMelodyParameters;
	public MusicParameters currentBassParameters;
	private MusicParameters lastHarmonyParameters;
	private MusicParameters lastMelodyParameters;
	private MusicParameters lastBassParameters;
	private string currentTheme;

	private WaitForSeconds refreshInterval = new WaitForSeconds( 0.5f );
	private bool musicStop;

	AudioClip GenerateChord( float[] frequencies, string waveformType, float attack, float decay, float sustain, float release, float modIndex, float modFrequency, string modType, string modFMAM )
	{
		int sampleRate = 44100;
		int sampleLength = sampleRate; // 1 second of audio
		float[][] samples = new float[frequencies.Length][];
		float[] mixedSamples = new float[sampleLength];

		// Generate samples for each note in the chord
		for( int note = 0; note < frequencies.Length; note++ )
		{
			samples[note] = new float[sampleLength];
			for( int i = 0; i < sampleLength; i++ )
			{
				float t = i / ( float )sampleRate;
				samples[note][i] = AudioUtils.GenerateWaveform( t, frequencies[note], waveformType, modIndex, modFrequency, modType, modFMAM ) * AudioUtils.ADSREnvelope( i, sampleRate, attack, decay, sustain, release, sampleLength );
			}
		}

		// Mix samples together
		for( int i = 0; i < sampleLength; i++ )
		{
			float mixedSample = 0;
			for( int note = 0; note < frequencies.Length; note++ )
			{
				mixedSample += samples[note][i];
			}
			mixedSamples[i] = mixedSample / frequencies.Length; // Average the samples to prevent clipping
		}

		AudioClip chordClip = AudioClip.Create( "Chord", sampleLength, 1, sampleRate, false );
		chordClip.SetData( mixedSamples, 0 );
		return chordClip;
	}

	void Start()
	{
		currentChordProgression = chordProgression;
		currentMelodyLine = melodyLine;
		currentBassLine = bassLine;
		melodySource = CreateAudioSource( "Melody" );
		harmonySource = CreateAudioSource( "Harmony" );
		bassSource = CreateAudioSource( "Bass" );
		melodySource.volume = 0.25f;
		bassSource.volume = 0.05f;
		harmonySource.volume = 0.3f;

		musicStop = true;
		// Example: Start different music layers
		StartCoroutine( PlayMelody() );
		StartCoroutine( PlayHarmony() );
		StartCoroutine( PlayBassLine() );
		StartCoroutine( AdjustMusicDynamically() );
	}

	void SetMusicTheme( string theme )
	{
		if( theme != currentTheme && !theme.StartsWith( "Region" ) )
		{
			lastMelodyParameters = currentMelodyParameters;
			lastHarmonyParameters = currentHarmonyParameters;
			lastBassParameters = currentBassParameters;
		}
		switch( theme )
		{
			case "FastForward":
				currentChordProgression = fastForwardChordProgression;
				currentMelodyLine = ffMelodyLine;
				currentBassLine = ffBassLine;
				currentMelodyParameters = new MusicParameters { waveformType = "sine", attack = 0.01f, decay = 0.05f, sustain = 0.8f, release = 0.1f, tempo = ( lastMelodyParameters.tempo * playerMovementTracker.Speed ) / 4f, modIndex = ( ( lastMelodyParameters.modIndex + 5 ) * playerMovementTracker.Speed ), modFrequency = ( ( lastMelodyParameters.modFrequency + 50 ) * playerMovementTracker.Speed ) / 1.5f, modType = "sawtooth", modFMAM = "AM" };
				break;
			case "SlowForward":
				currentMelodyParameters = new MusicParameters { waveformType = "sine", attack = 0.05f, decay = 0.2f, sustain = 0.7f, release = 0.2f, tempo = 90 };
				break;
			// Add more themes as necessary
			case "RegionA":
				currentMelodyParameters.modType = "square";
				currentChordProgression = new float[][] {
				new float[] { 220.00f, 261.63f, 329.63f }, // A Minor
				new float[] { 349.23f, 440.00f, 523.25f }, // F Major
				new float[] { 261.63f, 329.63f, 392.00f }, // C Major
				new float[] { 392.00f, 493.88f, 587.33f }  // G Major
				};

				break;
			case "RegionB":
				currentMelodyParameters.modType = "sine";
				currentChordProgression = new float[][] {
				new float[] { 261.63f, 329.63f, 392.00f }, // C Major
				new float[] { 220.00f, 261.63f, 329.63f }, // A Minor
				new float[] { 349.23f, 440.00f, 523.25f }, // F Major
				new float[] { 392.00f, 493.88f, 587.33f }  // G Major
				};

				currentMelodyLine = new float[] {
				392.00f, // G
				440.00f, // A
				392.00f, // G
				349.23f, // F
				392.00f, // G
				261.63f, // C (Lower Octave)
				329.63f, // E
				392.00f  // G
				};
				break;
			case "RegionC":
				currentMelodyParameters.modType = "sawtooth";
				currentMelodyParameters.modFrequency /= 2;
				currentMelodyLine = new float[] {
				440.00f, // A
				392.00f, // G
				440.00f, // A
				493.88f, // B
				523.25f, // C (Higher Octave)
				493.88f, // B
				440.00f, // A
				392.00f  // G
				};
				break;
			default:
				currentMelodyParameters = new MusicParameters { waveformType = "sine", attack = 0.01f, decay = 0.1f, sustain = 0.7f, release = 0.2f, tempo = 120 };
				break;
		}
		if( !theme.StartsWith( "Region" ) )
		{
			currentTheme = theme;
		}
	}



	IEnumerator AdjustMusicDynamically()
	{
		while( true )
		{
			yield return refreshInterval; // Adjust frequency of checks as needed
			if( playerMovementTracker.Speed >= 3.5f ) // Threshold for "fast" movement
			{
				if( playerMovementTracker.IsMovingForward )
				{
					// Trigger Fast Movement & Forward Direction theme
					SetMusicTheme( "FastForward" );
				}
				else
				{
					// Trigger Fast Movement & Backward Direction theme (if applicable)
					SetMusicTheme( "FastBackward" );
				}
			}
			else
			{
				if( playerMovementTracker.IsMovingForward )
				{
					// Trigger Slow Movement & Forward Direction theme
					SetMusicTheme( "SlowForward" );
				}
				else
				{
					// Trigger Slow Movement & Backward Direction theme (if applicable)
					SetMusicTheme( "SlowBackward" );
				}
			}
		}
	}

	private void Update()
	{
		UpdateMusicBasedOnLocation();
	}

	void UpdateMusicBasedOnLocation()
	{
		Vector3 playerPosition = playerMovementTracker.CurrentPosition;
		// Example conditions for different regions - these should be based on your game's world design
		if( playerPosition.x < 30 && playerPosition.x > -30 )
		{
			SetMusicTheme( "RegionA" );
		}
		else if( playerPosition.x >= 30 && playerPosition.x < 200 )
		{
			SetMusicTheme( "RegionB" );
		}
		else if( playerPosition.x <= -30 )
		{
			SetMusicTheme( "RegionC" );
		}
	}

	AudioSource CreateAudioSource( string name )
	{
		var newSource = new GameObject( name ).AddComponent<AudioSource>();
		newSource.transform.parent = transform;
		return newSource;
	}

	IEnumerator PlayHarmony()
	{
		int chordIndex = 0;
		float beatDuration = 60f / currentHarmonyParameters.tempo;
		//if( !musicStop ) StopCoroutine( PlayHarmony() );
		while( musicStop )
		{
			AudioClip chordClip = GenerateChord( currentChordProgression[chordIndex], currentHarmonyParameters.waveformType, currentHarmonyParameters.attack, currentHarmonyParameters.decay, currentHarmonyParameters.sustain, currentHarmonyParameters.release, currentHarmonyParameters.modIndex, currentHarmonyParameters.modFrequency, currentHarmonyParameters.modType, currentHarmonyParameters.modFMAM );
			harmonySource.PlayOneShot( chordClip );
			yield return new WaitForSeconds( beatDuration * 2 );
			chordIndex = ( chordIndex + 1 ) % chordProgression.Length;
		}
	}

	IEnumerator PlayMelody()
	{
		int noteIndex = 0;
		float beatDuration = 60f / currentMelodyParameters.tempo;
		//if( !musicStop ) StopCoroutine( PlayMelody() );
		while( musicStop )
		{
			// Example: Play a melody note
			AudioClip noteClip = GenerateTone( currentMelodyLine[noteIndex], currentMelodyParameters.waveformType, currentMelodyParameters.attack, currentMelodyParameters.decay, currentMelodyParameters.sustain, currentMelodyParameters.release, currentMelodyParameters.modIndex, currentMelodyParameters.modFrequency, currentMelodyParameters.modType, currentMelodyParameters.modFMAM );
			melodySource.PlayOneShot( noteClip );
			yield return new WaitForSeconds( beatDuration );
			noteIndex = ( noteIndex + 1 ) % currentMelodyLine.Length;
		}
	}

	IEnumerator PlayBassLine()
	{
		int noteIndex = 0;
		float beatDuration = 60f / currentBassParameters.tempo;
		//if( !musicStop ) StopCoroutine( PlayBassLine() );
		while( musicStop )
		{
			AudioClip noteClip = GenerateTone( currentBassLine[noteIndex], currentBassParameters.waveformType, currentBassParameters.attack, currentBassParameters.decay, currentBassParameters.sustain, currentBassParameters.release, currentBassParameters.modIndex, currentBassParameters.modFrequency, currentBassParameters.modType, currentBassParameters.modFMAM );
			bassSource.PlayOneShot( noteClip );
			yield return new WaitForSeconds( beatDuration * 2 );
			noteIndex = ( noteIndex + 1 ) % currentBassLine.Length;
		}
	}

	AudioClip GenerateTone( float frequency, string waveformType, float attack, float decay, float sustain, float release, float modIndex = 0, float modFrequency = 0, string modType = "none", string modFMAM = "none" )
	{
		int sampleRate = 44100;
		int sampleLength = sampleRate; // 1 second of audio
		float[] samples = new float[sampleLength];

		for( int i = 0; i < sampleLength; i++ )
		{
			float t = i / ( float )sampleRate;
			float waveformSample = AudioUtils.GenerateWaveform( t, frequency, waveformType, modIndex, modFrequency, modType, modFMAM );
			float envelope = AudioUtils.ADSREnvelope( i, sampleRate, attack, decay, sustain, release, sampleLength );
			samples[i] = waveformSample * envelope;
		}

		AudioClip clip = AudioClip.Create( "Tone", sampleLength, 1, sampleRate, false );
		clip.SetData( samples, 0 );
		return clip;
	}
}
