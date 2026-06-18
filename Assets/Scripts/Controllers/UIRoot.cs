using UnityEngine;
using UnityEngine.SceneManagement;
public class UIRoot : MonoBehaviour
{   
    private UIMainMenu mainMenu;
    private UIIngame ingame;
    private UISettings settingMenu;
    void Awake(){
        mainMenu = CanvasManager.Instance.LoadUIPrefabs<UIMainMenu>("MenuUI");
        ingame = CanvasManager.Instance.LoadUIPrefabs<UIIngame>("IngameUI");
        settingMenu = CanvasManager.Instance.LoadUIPrefabs<UISettings>("SettingsUI");
        LoadUIInGame();
        LoadUIMainMenu();
        LoadUISettings();
        UnLoadUIInGame();
        UnLoadUISettings();
        DontDestroyOnLoad(gameObject);

    }


    private void LoadUIMainMenu()
    {
        if (CanvasManager.Instance == null)return;
        CanvasManager.Instance.AddUI(mainMenu);
        UIMainMenu.OnPlayButtonClicked += MenuToPlay;
        PlayerController.EscTrigger += PlayToSettings;
        UISettings.PlayMenuClicked += SettingsToMenu;
        UISettings.PlayResumeClicked += SettingsToPlay;
    }
    private void UnLoadUIMainMenu(){
        if (CanvasManager.Instance == null)return;
        CanvasManager.Instance.RemoveUI(mainMenu);

    }
    private void LoadUIInGame(){
        if(CanvasManager.Instance == null)return;
        CanvasManager.Instance.AddUI(ingame);
        // ingame.gameObject.SetActive(true);
    }
    private void UnLoadUIInGame(){
        if(CanvasManager.Instance == null)return;
        CanvasManager.Instance.RemoveUI(ingame);
    }
    private void LoadUISettings(){
        if(CanvasManager.Instance == null)return;
        CanvasManager.Instance.AddUI(settingMenu);
    }
    private void UnLoadUISettings(){
        if(CanvasManager.Instance == null)return;
        CanvasManager.Instance.RemoveUI(settingMenu);
    }
    private void MenuToPlay(){
        
        UnLoadUIMainMenu();
        LoadUIInGame();
    }
    private void PlayToSettings(){
        UnLoadUIInGame();
        LoadUISettings();
        Time.timeScale = 0f;
    }
    private void SettingsToMenu(){
        UnLoadUISettings();
        LoadUIMainMenu();
        Time.timeScale = 0f;
        Debug.Log("Set to menu");
        
    }
    private void SettingsToPlay(){
        UnLoadUISettings();
        LoadUIInGame();
        Time.timeScale = 1f;
        Debug.Log("Set to play");
    }
}