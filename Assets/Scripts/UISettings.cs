using UnityEngine;
using System;
using UnityEngine.UI;
public class UISettings : MonoBehaviour
{
    [SerializeField] private Button ResumeButton;
    [SerializeField] private Button SaveButton;
    [SerializeField] private Button LoadButton;
    [SerializeField] private Button MenuButton;

    public static Action PlayResumeClicked;
    public static Action PlayMenuClicked;
    public static Action PlayLoadClicked;
    void Awake(){
        InitButtonEvents();
    }
    private void InitButtonEvents() {
        ResumeButton.onClick.AddListener(OnPlayResumeClicked);
        LoadButton.onClick.AddListener(OnClickLoadButton);
        // SettingsButton.onClick.AddListener(OnClickSettingsButton);
        MenuButton.onClick.AddListener(OnPlayMenuClicked);
    }
    void OnPlayResumeClicked(){
        PlayResumeClicked?.Invoke();
        Debug.Log("Clucked");
    }
    void OnPlayMenuClicked(){
        PlayMenuClicked?.Invoke();
    }
    void OnClickLoadButton(){
        PlayLoadClicked?.Invoke();
    }
}
