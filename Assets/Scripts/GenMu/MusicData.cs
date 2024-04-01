using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MusicData
{
	public int amountOfChords;
	public float[] chord1;
	public float[] chord2;
	public float[] chord3;
	public float[] chord4;
	public float[] chord5;
	public float[] melodyLine;
	public float[] bassLine;
	public MusicManager.MusicParameters harmonyParameters;
	public MusicManager.MusicParameters melodyParameters;
	public MusicManager.MusicParameters bassParameters;
}
