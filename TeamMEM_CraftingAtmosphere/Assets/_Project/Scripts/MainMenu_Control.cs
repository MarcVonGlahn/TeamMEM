using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu_Control : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] Button quitButton;
    [SerializeField] Image blackOutPanel;
    [SerializeField] float fadeDuration = 3.0f;

    private void OnEnable()
    {
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);

        blackOutPanel.gameObject.SetActive(false);
    }


    private void OnDisable()
    {
        startButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
    }


    private void StartGame()
    {
        blackOutPanel.gameObject.SetActive(true);
        blackOutPanel.DOFade(1.0f, fadeDuration)
            .OnComplete(() =>
             {
                 SceneManager.LoadScene("01_PlaythroughScene");
             });
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
