using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryBox : MonoBehaviour
{
	private MusicManager musicManager;
	private void Start()
	{
		musicManager = FindObjectOfType<MusicManager>();
	}
	private void OnTriggerEnter( Collider other )
	{
		if(other.tag == "Player")
		{
			musicManager.SetMusicTheme( this.name );
		}
	}
}
