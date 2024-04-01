using UnityEngine;
using System.Collections;
using System.Text;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Drawing;

public class MusicManager : MonoBehaviour
{
	public GameObject player;
	public MusicData currentMusicData = new MusicData();
	private AudioUtils audioUtils = new AudioUtils();
	//public PlayerMovementTracker playerMovementTracker;
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

	public MusicParameters baseMusicParams;
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
				samples[note][i] = audioUtils.GenerateWaveform( t, frequencies[note], waveformType, modIndex, modFrequency, modType, modFMAM ) * audioUtils.ADSREnvelope( i, sampleRate, attack, decay, sustain, release, sampleLength );
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
		melodySource.volume = 0.02f;
		bassSource.volume = 0.18f;
		harmonySource.volume = 0.2f;

		musicStop = true;
		// Example: Start different music layers
		StartCoroutine( PlayMelody() );
		StartCoroutine( PlayHarmony() );
		StartCoroutine( PlayBassLine() );
	}

	public void SetMusicTheme( string theme )
	{
		StartCoroutine( GenerateContent( "I want you to make a single MusicData class in JSON format based on this class: " + JsonUtility.ToJson( baseMusicData ) + " .The MusicParameters are based on this struct:" + JsonUtility.ToJson(baseMusicParams) + " and are the data necassary for this static class:  " + JsonUtility.ToJson( audioUtils ) +
					"The MusicData you're creating should fit the theme: " + theme + ". amoundOfChords is an int with a value between 2-5 and should correspond with how many chord[] are filled, chord1-chord5 are each a float[], inside of it should be 3 floats which are note frequencies, representing chords. " +
					"melodyLine should be a float[] filled with floats which are note frequencies, together this should form a melody of notes. bassLine should be a float[] filled with floats which are note frequencies, together this should form a baseLine of notes." +
					"waveformType and modType both are strings representing a waveformat, possible strings here are: sine, square, sawtooth, triangle. modFMAM represents which type of modulation to apply, the options being: FM or AM. " +
					"Try to make all values in such a way that it would create music fitting the theme: " + theme + ", so make corresponding chords, melodies and basslines as well as making sure that all modulations aren't too extreme and make the music sound pleasent " +
					"be sure to only respond with the JSON and not have anything else in the repsonse message. " +
					"Dont forget you have to close off the response with the right closing quotation mark. " +
					"Make sure there are no syntax errors in the JSON. An example of how a correct response could look in JSON format is: " +
					"[\"amountOfChords\": 4,\"chord1\": [261.63, 329.63, 392.00], \"chord2\": [349.23, 440.00, 523.25], \"chord3\": [392.00, 493.88, 587.33], \"chord4\": [220.00, 261.63, 329.63],\"melodyLine\": [329.63, 392.0, 392.0, 440.0, 493.88, 493.88, 523.25, 587.33],\"bassLine\": [164.81, 174.61, 196.0, 174.61, 196.0, 174.61, 196.0, 130.81],\"harmonyParameters\": {\"waveformType\": \"sine\",\"attack\": 0.3,\"decay\": 0.2,\"sustain\": 0.7,\"release\": 0.4,\"tempo\": 110.0,\"modIndex\": 1.5,\"modFrequency\": 2.0,\"modType\": \"sawtooth\",\"modFMAM\": \"FM\"},\"melodyParameters\": {\"waveformType\": \"sawtooth\",\"attack\": 0.2,\"decay\": 0.1,\"sustain\": 0.5,\"release\": 0.3,\"tempo\": 110.0,\"modIndex\": 12.0,\"modFrequency\": 23.0,\"modType\": \"sine\",\"modFMAM\":\"AM\"},\"bassParameters\": {\"waveformType\": \"sawtooth\",\"attack\": 0.2,\"decay\": 0.1,\"sustain\": 0.5,\"release\": 0.3,\"tempo\": 110.0,\"modIndex\": 12.0,\"modFrequency\": 23.0,\"modType\": \"sine\",\"modFMAM\":\"AM\"}]"
					 + " This however is only an example, actual values should not be the same as the example and you should come up with values that would sound good together and fit the theme: " + theme));
	}
	private void Update()
	{
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
			float waveformSample = audioUtils.GenerateWaveform( t, frequency, waveformType, modIndex, modFrequency, modType, modFMAM );
			float envelope = audioUtils.ADSREnvelope( i, sampleRate, attack, decay, sustain, release, sampleLength );
			samples[i] = waveformSample * envelope;
		}
		AudioClip clip = AudioClip.Create( "Tone", sampleLength, 1, sampleRate, false );
		clip.SetData( samples, 0 );
		return clip;
	}

	#region AIGenerator
	private MusicData baseMusicData = new MusicData();
	IEnumerator GenerateContent( string promptI )
	{
		OpenAIrequestBody requestBody = new OpenAIrequestBody { prompt = promptI };
		string json = JsonUtility.ToJson( requestBody );
		byte[] bodyRaw = Encoding.UTF8.GetBytes( json );
		using( UnityWebRequest request = new UnityWebRequest( "https://api.openai.com/v1/completions", "POST" ) )
		{
			request.uploadHandler = new UploadHandlerRaw( bodyRaw );
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader( "Content-Type", "application/json" );
			request.SetRequestHeader( "Authorization", "Bearer " );
			yield return request.SendWebRequest();

			if( request.result == UnityWebRequest.Result.Success )
			{
				try
				{
					string jsonResponse = request.downloadHandler.text;
					OpenAIResponse response = JsonUtility.FromJson<OpenAIResponse>( jsonResponse );

					if( response.choices != null && response.choices.Length > 0 )
					{
						string replyText = response.choices[0].text;
						string trimmedReplyText = response.choices[0].text.Trim();
						Debug.Log( replyText );
						Debug.Log( trimmedReplyText );
						currentMusicData = JsonUtility.FromJson<MusicData>( trimmedReplyText );
						float[][] createdChordProgression = new float[currentMusicData.amountOfChords][];
						for( int i = 0; i < currentMusicData.amountOfChords; i++)
						{
							switch(i)
							{
								case 0:
									createdChordProgression[i] = currentMusicData.chord1;
									break;
								case 1:
									createdChordProgression[i] = currentMusicData.chord2;
									break;
								case 2:
									createdChordProgression[i] = currentMusicData.chord3;
									break;
								case 3:
									createdChordProgression[i] = currentMusicData.chord4;
									break;
								case 4:
									createdChordProgression[i] = currentMusicData.chord5;
									break;
							}
							
						}
						Debug.Log( createdChordProgression );
						currentChordProgression = createdChordProgression;
						currentMelodyLine = currentMusicData.melodyLine;
						currentBassLine = currentMusicData.bassLine;
						currentHarmonyParameters = currentMusicData.harmonyParameters;
						currentMelodyParameters = currentMusicData.melodyParameters;
						currentBassParameters = currentMusicData.bassParameters;
					}
					else
					{
						Debug.LogWarning( "no choices available in repsonse." );
					}
				}
				catch( Exception ex )
				{
					Debug.LogError( $"Error parsing JSON: {ex.Message}" );
					Debug.Log( "RETRYING NEW GPT RESPONSE..." );
					StartCoroutine( GenerateContent( promptI));
				}
			}
			else
			{
				Debug.LogError( $"Error generating content: {request.error}, Response Code: {request.responseCode}" );
			}
		}
	}

	#endregion
}


[Serializable]
public class OpenAIrequestBody
{
	public string model = "gpt-3.5-turbo-instruct";
	public string prompt;
	public int max_tokens = 500;
};

[Serializable]
public class OpenAIResponse
{
	public Choice[] choices;
}

[Serializable]
public class Choice
{
	public string text;
}

[Serializable]
public class MusicDataList
{
	public MusicData[] musicData;
}
