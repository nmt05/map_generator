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
    public static bool PlayFromSave { get; private set; }

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
        PlayFromSave = false;
        OnPlayButtonClicked?.Invoke();
        SceneManager.LoadScene("MainPlay");
    }
    void OnClickLoadButton(){
        PlayFromSave = true;
        OnPlayButtonClicked?.Invoke();
        SceneManager.LoadScene("MainPlay");
    }
    void OnClickSettingsButton(){}
    void OnClickExitButton(){}
    
}
