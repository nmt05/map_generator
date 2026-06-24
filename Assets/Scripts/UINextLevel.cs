using UnityEngine;
using System;
using UnityEngine.UI;
public class UINextLevel : MonoBehaviour
{
    [SerializeField] private Button NextLevelButton;
    [SerializeField] private Button SaveButton;
    [SerializeField] private Button LoadButton;
    [SerializeField] private Button MenuButton;

    public static Action PlayNextLevelClicked;
    public static Action PlayMenuClicked;
    
    void Awake(){
        InitButtonEvents();
    }
    private void InitButtonEvents() {
        NextLevelButton.onClick.AddListener(OnPlayNextLevelClicked);
        // LoadButton.onClick.AddListener(OnClickLoadButton);
        // SettingsButton.onClick.AddListener(OnClickSettingsButton);
        MenuButton.onClick.AddListener(OnPlayMenuClicked);
    }
    void OnPlayNextLevelClicked(){
        PlayNextLevelClicked?.Invoke();
        Debug.Log("Clucked");
    }
    void OnPlayMenuClicked(){
        PlayMenuClicked?.Invoke();
    }
}
