using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class Backgrounds : MonoBehaviour
{
	private Material _material;
	private RenderTexture _renderBuffer;
	private VideoPlayer _videoPlayer;
	public List<VideoClip> Videos;
	public List<Sprite> Images;
	
	private void Start()
	{
		_material = GetComponent<MeshRenderer>().material;
		_videoPlayer = GetComponent<VideoPlayer>();
		_renderBuffer = _videoPlayer.targetTexture;
	}

	public void Switch(int index)
	{
		if (index < Videos.Count)
		{
			_videoPlayer.clip = Videos[index];
			_videoPlayer.Play();
			_material.mainTexture = _renderBuffer;
		}
		else
		{
			_videoPlayer.Stop();
			_material.mainTexture = Images[index - Videos.Count].texture;
		}
	}
}
