using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneJump : MonoBehaviour
{
	public Animator anim;
	public GameObject StopUI;
	private int targetSceneIndex = -1;
	private bool isLoading = false;
	void Start() 
	{
		
	}
	void Update () 
	{
		if (isLoading && targetSceneIndex >= 0)
		{
			AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
			if (stateInfo.IsName("Fade_Start") && stateInfo.normalizedTime >= 1.0f)
			{
				int sceneToLoad = targetSceneIndex;
				targetSceneIndex = -1;
				isLoading = false;
				SceneManager.LoadScene(sceneToLoad);
			}
		}
	}
	public void JumpToGame()
	{
		Time.timeScale = 1;
		targetSceneIndex = 1;
		isLoading = true;
		anim.SetTrigger("Start");
	}
	public void JumpToGameSingle()
	{
		Time.timeScale = 1;
		targetSceneIndex = 2;
		isLoading = true;
		anim.SetTrigger("Start");
	}
	public void StopGame()
	{
		StopUI.SetActive(true);
		Time.timeScale = 0;
	}
	public void ContinueGame()
	{
		StopUI.SetActive(false);
		Time.timeScale = 1;
	}
	public void RestartGame()
	{
		Time.timeScale = 1;
		targetSceneIndex = SceneManager.GetActiveScene().buildIndex;
		isLoading = true;
		anim.SetTrigger("Start");
	}
	public void BackTo0()
	{
		Time.timeScale = 1;
		targetSceneIndex = 0;
		isLoading = true;
		anim.SetTrigger("Start");
	}
	public void ExitGame()
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}
	IEnumerator LoadLevel(int levelIndex)
	{
		anim.SetTrigger("Start");
		yield return new WaitForSeconds(1f);
		SceneManager.LoadScene(levelIndex);
	}
	
}
