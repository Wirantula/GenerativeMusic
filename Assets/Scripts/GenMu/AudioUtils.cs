using UnityEngine;

public static class AudioUtils
{
	public static float GenerateWaveform( float t, float frequency, string waveformType, float modIndex = 0, float modFrequency = 0, string modType = "none", string modFMAM = "none" )
	{
		float carrier = waveformSelector( t, frequency, waveformType );
		float modulator = waveformSelector( t, modFrequency, modType );

		switch( modFMAM )
		{
			case "FM":
				return Mathf.Sin( 2 * Mathf.PI * frequency * t + modIndex * modulator );
			case "AM":
				return ( 1 + modIndex * modulator ) * carrier;
			default:
				return carrier; // No modulation
		}
	}

	private static float waveformSelector( float t, float frequency, string waveformType )
	{
		switch( waveformType )
		{
			case "sine":
				return Mathf.Sin( 2 * Mathf.PI * frequency * t );
			case "square":
				return Mathf.Sign( Mathf.Sin( 2 * Mathf.PI * frequency * t ) );
			case "sawtooth":
				return 2f * ( t * frequency - Mathf.Floor( t * frequency + 0.5f ) );
			case "triangle":
				return Mathf.Abs( 4f * ( t * frequency - Mathf.Floor( t * frequency + 0.5f ) ) ) - 1f;
			default:
				return Mathf.Sin( 2 * Mathf.PI * frequency * t ); // Default to sine wave
		}
	}

	public static float ADSREnvelope( int sampleIndex, int sampleRate, float attack, float decay, float sustain, float release, int totalSamples )
	{
		float time = sampleIndex / ( float )sampleRate;
		float attackTime = attack * sampleRate;
		float decayTime = decay * sampleRate + attackTime;
		float releaseTime = release * sampleRate;
		float sustainLevel = sustain;

		if( sampleIndex < attackTime )
			return ( sampleIndex / attackTime );
		else if( sampleIndex < decayTime )
			return ( 1 - ( sampleIndex - attackTime ) / ( decayTime - attackTime ) * ( 1 - sustainLevel ) );
		else if( sampleIndex < totalSamples - releaseTime )
			return sustainLevel;
		else
			return sustainLevel * ( 1 - ( sampleIndex - ( totalSamples - releaseTime ) ) / releaseTime );
	}
}