using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UIMainMenu : MonoBehaviour
{
    [SerializeField] private Button StartButton;
    [SerializeField] private Button LoadButton;
    [SerializeField] private Button SettingsButton;
    [SerializeField] private Button ExitButton;

    public static Action OnPlayButtonClicked;
    void Start() {
        InitButtonEvents();
    }

    private void InitButtonEvents() {
        StartButton.onClick.AddListener(OnClickStartButton);
        LoadButton.onClick.AddListener(OnClickLoadButton);
        SettingsButton.onClick.AddListener(OnClickSettingsButton);
        ExitButton.onClick.AddListener(OnClickExitButton);
    }
    void OnClickStartButton(){
        OnPlayButtonClicked?.Invoke();
        SceneManager.LoadScene("MainPlay");
    }
    void OnClickLoadButton(){}
    void OnClickSettingsButton(){}
    void OnClickExitButton(){}
    
}
