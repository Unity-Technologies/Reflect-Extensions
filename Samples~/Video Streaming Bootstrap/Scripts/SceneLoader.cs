using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
	public UnityEvent onSceneLoaded;
	AsyncOperation asyncOperation;
	WaitForSeconds waitForOneSecond = new WaitForSeconds(1f);
	bool _ready;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	private void Start()
	{
		StartCoroutine(StartLoading());
	}

	IEnumerator StartLoading()
	{
		yield return waitForOneSecond;

		asyncOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
		asyncOperation.allowSceneActivation = false;
		//asyncOperation.completed += AsyncOp_completed;

		while (!_ready)
		{
			if (!asyncOperation.isDone && asyncOperation.progress < 0.9f)
			{
				yield return waitForOneSecond;
			}
			else
			{
				onSceneLoaded?.Invoke();
				_ready = true;
			}
		}
	}

	private void AsyncOp_completed(AsyncOperation obj)
	{
		onSceneLoaded?.Invoke();
	}

	public void FinishLoading()
	{
		asyncOperation.allowSceneActivation = true;
		Destroy(gameObject);
	}
}