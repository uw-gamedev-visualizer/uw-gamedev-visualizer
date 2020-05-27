using UnityEngine;
using UnityEngine.UI;

// Countdown the time to the meeting.
public class Countdown : MonoBehaviour {
	
	private readonly KeyCode[] _keyCodes = {
		KeyCode.Alpha0,
		KeyCode.Alpha1,
		KeyCode.Alpha2,
		KeyCode.Alpha3,
		KeyCode.Alpha4,
		KeyCode.Alpha5,
		KeyCode.Alpha6,
		KeyCode.Alpha7,
		KeyCode.Alpha8,
		KeyCode.Alpha9
	};
    public float TimeLeft;
	private Text _text;
	private int[] _numArr = {0, 0};

	private void Start () {
        _text = GetComponent<Text>();
	}
	
	private void Update ()
	{
		UpdateTime();
		_text.text = FormatText(TimeLeft);
	}

	private void UpdateTime()
	{
		for (int i = 0; i < 10; i++)
		{
			if (Input.GetKeyDown(_keyCodes[i]))
			{
				_numArr[0] = _numArr[1];
				_numArr[1] = i;
			}
		}
		
		if (Input.GetKeyDown("return"))
		{
			TimeLeft = 60 * (_numArr[0] * 10 + _numArr[1]);
			_numArr[0] = 0;
			_numArr[1] = 0;
		}
		
		TimeLeft -= Time.deltaTime;
	}

	private static string FormatText(float timeLeft)
	{
		if (timeLeft <= 0)
			return "Meeting will begin\nmomentarily";
            
		int seconds = Mathf.CeilToInt(timeLeft) % 60;
		int minutes = Mathf.CeilToInt(timeLeft - seconds) / 60;

		return string.Format("Meeting will\nbegin in {0}:{1:D2}", minutes, seconds);

	}
}
