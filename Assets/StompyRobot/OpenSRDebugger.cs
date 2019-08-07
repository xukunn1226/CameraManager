using UnityEngine;

namespace Framework
{
	public class OpenSRDebugger : MonoBehaviour
	{
        string _psw = "96321";
        string _cur = "";

		void Update()
		{
#if UNITY_STANDALONE_WIN
			DoPCGesture();
#else
			DoMobileGesture();
#endif
		}

		void OnTouchDown()
		{
			_cur = "";           
		}

		void OnTouchMove()
		{
			Vector2 screenPos = Input.mousePosition;

			int w = Screen.width;
			int h = Screen.height;

			int col = (int)(3 * screenPos.x / w);
			int row = (int)(3 * screenPos.y / h);
			if (col < 0) col = 0;
			if (col > 2) col = 2;
			if (row < 0) row = 0;
			if (row > 2) row = 2;

			int num = row * 3 + col + 1;
			if (!_cur.Contains("" + num))
			{
				_cur += num;
			}
		}

		void OnTouchEnd()
		{
			if (_cur == _psw)
			{
				if (!SRDebug.Instance.IsDebugPanelVisible)
				{
					SRDebug.Instance.ShowDebugPanel();
				}
			}
		}

		void DoMobileGesture()
		{
			if (Input.touchCount != 1) return;
			Touch touch = Input.GetTouch(0);

			if (touch.phase == TouchPhase.Began)
			{
				OnTouchDown();
			}

			if (touch.phase == TouchPhase.Moved)
			{
				OnTouchMove();
			}

			if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
			{
				OnTouchEnd();
			}
		}

		void DoPCGesture()
		{
			if (Input.GetMouseButtonDown(0))
			{
				OnTouchDown();
			}

			if (Input.GetMouseButton(0))
			{
				OnTouchMove();
			}

			if (Input.GetMouseButtonUp(0))
			{
				OnTouchEnd();
			}
		}
	}
}