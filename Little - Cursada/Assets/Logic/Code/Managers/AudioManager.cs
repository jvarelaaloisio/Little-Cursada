using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	#region Variables
	readonly float[] relations = { 25, 75 };

	enum SoundIndex
	{
		Music,
		Ambient1,
		Ambient2,
		AmbientBG,
		Character
	}

	public AudioClip[] MainTracks;
	public int BGMIndex,
		PitchedIndex,
		CommonIndex,
		CharacterIndex,
		CharacterWalkingIndex;

	public AudioSource[] Sources;
	public AudioMixer Mixer;
	public AudioMixerSnapshot[] SnapshotsVolDown, SnapshotsVolUp;
	#endregion

	#region Unity
	void Awake()
	{
		Sources = GetComponentsInChildren<AudioSource>();
		PlayMainMusic(BGMIndex, 0);
	}
	#endregion

	#region Private
	/// <summary>
	/// Controls the BGM
	/// </summary>
	/// <param name="Index"></param>
	/// <param name="Track"></param>
	void PlayMainMusic(int Index, int Track)
	{
		if (Track < MainTracks.Length)
		{
			Sources[Index].clip = MainTracks[Track];
			Sources[Index].Play();
		}
	}
	#endregion

	#region Public

	/// <summary>
	/// Plays a given sound trhough one of the Ambient sources source
	/// </summary>
	/// <param name="NewClip"></param>
	public void PlayAmbientSound(AudioClip NewClip)
	{
		if (Sources[(int)SoundIndex.Ambient1].isPlaying)
		{
			Sources[(int)SoundIndex.Ambient2].PlayOneShot(NewClip);
		}
		else
		{
			Sources[(int)SoundIndex.Ambient1].PlayOneShot(NewClip);
		}
	}

	/// <summary>
	/// Plays a given sound trhough the Character source
	/// </summary>
	/// <param name="NewClip"></param>
	public void PlayCharacterSound(AudioClip NewClip)
	{
		Sources[(int)SoundIndex.Character].PlayOneShot(NewClip);
	}

	/// <summary>
	/// Turns Volume Down
	/// </summary>
	public void VolumeDown()
	{
		Mixer.TransitionToSnapshots(SnapshotsVolDown, relations, 0f);
	}

	/// <summary>
	/// Turns Volume Up
	/// </summary>
	public void VolumeUp()
	{
		Mixer.TransitionToSnapshots(SnapshotsVolUp, relations, 0f);
	}
	#endregion

	#region Gizmos
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(transform.position, .5f);
	}
	#endregion
}