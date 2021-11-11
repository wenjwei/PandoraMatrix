
using UnityEditor;
using UnityEngine;
using System;

namespace PPMUtils
{
	public class LoginWebWindow : EditorWindow
	{
		WebViewHook _webView;
		WebSocketHook _socket;

		public string BkUID;
		public string BkTicket;

		bool _cookieReady = false;
		const float RefreshTokenInterval = 2.5f;
		float _nextRefreshTokenTime;

		// login page
		const string URL = "http://apps.open.oa.com/ieod-bkapp-login-test-stag/";
		public Action<string> UserLoginCallback;

		//[MenuItem("Tools/Web Window %#w")]
		public static void Load(Action<string> uesrLoginCallback)
		{
			LoginWebWindow window = GetWindow<LoginWebWindow>(true);
			window.ShowUtility();
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
			window.titleContent = new GUIContent("用户登录");
#elif UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
			window.title = "用户登录";
#endif
			window.UserLoginCallback = uesrLoginCallback;

			window.minSize = new Vector2(700, 580);
			window.maxSize = new Vector2(700, 580);
		}

		void OnEnable()
		{
			_nextRefreshTokenTime = Time.realtimeSinceStartup + RefreshTokenInterval;
			if (!_webView)
			{
				// create webView
				_webView = CreateInstance<WebViewHook>();

				// Make the server.
				// The server is not serializable so there's
				// no need to check if this exist or not.
				_socket = new WebSocketHook(9399, _webView);

				// Hook window.data to javascript
				_socket.Add("bk_uid", () => BkUID, (bk_uid) =>
				{
					BkUID = bk_uid;
					CheckCookie();
					//Debug.Log("bk_uid:" + bk_uid);
				});
				_socket.Add("bk_ticket", () => BkTicket, (bk_ticket) =>
				{
					BkTicket = bk_ticket;
					CheckCookie();
					//Debug.Log("bk_ticket:" + bk_ticket);
				});
			}
		}

		private void CheckCookie()
		{
			if (!string.IsNullOrEmpty(BkUID) && !string.IsNullOrEmpty(BkTicket))
			{
				_cookieReady = true;
				Action<ResponseStatus> callback = (rs) =>
				{
					if (UserLoginCallback != null)
					{
						UserLoginCallback(BkUID);
					}

					LoginWebWindow window = GetWindow<LoginWebWindow>(true);
					window.Close();
				};
				Requestor.SetCookie(BkUID, BkTicket, callback);
			}
		}

		public void OnBecameInvisible()
		{
			if (_webView)
			{
				// signal the browser to unhook
				_webView.Detach();
			}
		}

		void OnDestroy()
		{
			_socket.Dispose();
			//Destroy web view
			DestroyImmediate(_webView);
			UserLoginCallback = null;
		}

		void OnGUI()
		{
			Repaint();

			// hook to this window
			if (_webView.Hook(this))
			{
				// do the first thing to do
				_webView.LoadURL(URL);
				Focus();
			}

			if (Event.current.type == EventType.Repaint)
			{
				// keep the browser aware with resize
				_webView.OnGUI(new Rect(0, 20, position.width, position.height - 80));
			}

			if (_nextRefreshTokenTime < Time.realtimeSinceStartup && !_cookieReady)
			{
				_nextRefreshTokenTime = Time.realtimeSinceStartup + RefreshTokenInterval;
				_socket.UpdateDefinitions("");
			}
		}
	}
}