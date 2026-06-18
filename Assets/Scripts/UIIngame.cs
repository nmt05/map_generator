using UnityEngine;
using UnityEngine.UI;
public class UIIngame : MonoBehaviour
{
    [SerializeField] public Text scoreText;
    private int score = 0;
    void Awake(){
        UpdateScore();
    }
    public void UpdateScore()
    {
        if (scoreText == null) return;
        Monster2Controller.DieEvent += UpdateScoreOnce;
    }
    private void UpdateScoreOnce()
    {
        Debug.Log("Score+");
        score++;
        scoreText.text = "Score: " + score.ToString();
    }


}
