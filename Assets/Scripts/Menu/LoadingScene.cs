using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public GameObject LoadingScreen;
    public Image LoadingBarFill;

    public GameObject InGameMenu;

    public void OnClickExitGame()
    {
        Application.Quit();
    }

    public void OnClickGoToMainMenu()
    {
        LoadScene(0);
    }

    public void OnClickOpenSettings()
    {
        Debug.Log("Settings");
    }
    public void OnClickOpenNewGame()
    {
        LoadScene(1);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && InGameMenu != null)
        {
            InGameMenu.SetActive(!InGameMenu.activeInHierarchy);
        }
    }

    public void LoadScene(int sceneId)
    {
        LoadingScreen.SetActive(true);
        LoadingBarFill.fillAmount = 0;
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    IEnumerator LoadSceneAsync(int sceneId)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);

            LoadingBarFill.fillAmount = progressValue;
            yield return null;
        }

        LoadingScreen.SetActive(false);
    }
}
