using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using UnityEditor;

namespace PPM
{
	[InitializeOnLoad]
	public static class ThreadUtils
	{
		private class ExecuteItem
		{
			public Action ExecuteAction;
			public float ExecuteTime;
		}

		private static Action _threadAction;
		private static readonly object _threadLocker = new object();
		private static readonly object _actionLocker = new object();
		private static readonly Queue<ExecuteItem> _actionQueue = new Queue<ExecuteItem>();
		private static readonly ExecutionContext _unityExecutionContext;

		static ThreadUtils()
		{
			EditorApplication.update += Update;
			_unityExecutionContext = Thread.CurrentThread.ExecutionContext;
		}

		public static void ExecuteOnMainThread(Action action)
		{
			if (action == null)
				return;

			if (_unityExecutionContext == Thread.CurrentThread.ExecutionContext)
				action();
			else
				ExecuteOnNextFrame(action);
		}

		public static void ExecuteOnNextFrame(Action action)
		{
			if (action == null)
				return;

			lock (_actionLocker)
			{
				ExecuteItem item = new ExecuteItem();
				item.ExecuteAction = action;
				item.ExecuteTime = DateTime.Now.Second;
				_actionQueue.Enqueue(item);
			}
		}

		public static void DelayedExecute(Action action, float delayedSeconds)
		{
			if (action == null)
				return;

			lock (_actionLocker)
			{
				ExecuteItem item = new ExecuteItem();
				item.ExecuteAction = action;
				item.ExecuteTime = DateTime.Now.Second + delayedSeconds;
				_actionQueue.Enqueue(item);
			}
		}

		private static void Update()
		{
			if (_actionQueue.Count > 0)
			{
				Queue<ExecuteItem> actionQueueCopy;
				lock (_actionQueue)
				{
					actionQueueCopy = new Queue<ExecuteItem>(_actionQueue);
					_actionQueue.Clear();
				}
				while (actionQueueCopy.Count > 0)
				{
					try
					{
						ExecuteItem item = actionQueueCopy.Dequeue();
						if (item.ExecuteTime < DateTime.Now.Second)
							item.ExecuteAction();
						else
							_actionQueue.Enqueue(item);
					}
					catch (Exception e)
					{
						PPMHelper.LogError(e.Message);
						PPMHelper.LogError(e.StackTrace);
					}
				}
			}
		}

		public static Thread StartThread(Action threadAction)
		{
			_threadAction = threadAction;
			Thread thread = new Thread(ThreadRunner);
			thread.Start();
			return thread;
		}

		private static void ThreadRunner()
		{
			lock(_threadLocker)
			{
				if (_threadAction != null)
				{
					_threadAction();
				}
			}
		}

		public static void TerminateThread(Thread thread)
		{
			if (thread != null)
				thread.Abort();
		}
	}
}
