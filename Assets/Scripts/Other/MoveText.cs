﻿using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class MoveText : MonoBehaviour
{
	public float Speed = 200;  // pixels per second in 4k res
	public float Delay = 1;  // Seconds to wait at beginning and end of scroll
	
    private Text _text;
	private Vector3 _textBasePos;
	private float _scrollWidth;
	private TextGenerator _textGen;
	private string _prevText;
	private float _textLength;
	private float _scrollTimer;
    
    private void Start()
    {
	    _text = GetComponent<Text>();
	    _textGen = new TextGenerator();
	    _textBasePos = _text.rectTransform.anchoredPosition3D;
	    _scrollWidth = 3840 - _textBasePos.x * 2;  // Resolution minus both sides
    }
	
	private void Update ()
	{
		// Do some setup if we have a new song
		if (_prevText != _text.text)
		{
			_prevText = _text.text;
			
			// Calculate width of text.
			// Vector2.zero is textbox size, which works since we overflow in both directsions anyway
			TextGenerationSettings textGenSettings = _text.GetGenerationSettings(Vector2.zero);
			// No idea if this is resolution dependent, but it must be constant
			textGenSettings.scaleFactor = 0.5f;
			_textLength = _textGen.GetPreferredWidth(_text.text, textGenSettings);
			
			// Reset the scrolling
			_scrollTimer = 0;
			// Long text is set later, but short text wouldn't reset without this
			_text.rectTransform.anchoredPosition3D = _textBasePos;
		}

		// If we should scroll
		if (_textLength > _scrollWidth)
		{
			float scrollDist = _textLength - _scrollWidth;
			
			_scrollTimer += Time.deltaTime * Speed;
			_scrollTimer %= scrollDist + 2 * Delay * Speed;  // Delay time for both start and end
			
			float newX = Mathf.Clamp(_scrollTimer - Delay * Speed, 0, scrollDist);  // Offset is for start delay
			_text.rectTransform.anchoredPosition3D = _textBasePos + Vector3.left * newX;
		}
	}
}
