using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    
    public static CanvasController Instance;
    
    public Image HpImg;
    public TextMeshProUGUI HpText;
    public Image ScoreImg;
    public TextMeshProUGUI ScoreText;

    public Button Restart;
    
    public GameObject GameEnd;
    public TextMeshProUGUI YourScore;
    public TextMeshProUGUI HighScore;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        Restart.onClick.AddListener(() => SceneManager.LoadScene("Scenes/BallPool"));
    }
    
    public void UpdateSize(Transform t,float size,float time)
    {
        t.DOKill();
        t.DOScale(Vector3.one * size, time).OnComplete(() => t.DOScale(Vector3.one, time));
    }

    public void SetHp(int current, int max)
    {
        UpdateSize(HpImg.transform, 1.25f, 0.2f);
        UpdateSize(HpText.transform,1.25f, 0.2f);
        HpText.text = $"{current}/{max}";
    }

    public void SetScore(int score)
    {
        UpdateSize(ScoreImg.transform,1.2f, 0.15f);
        UpdateSize(ScoreText.transform, 1.2f, 0.15f);
        ScoreText.text = score.ToString();
    }

    public void EndGame(int score,int highScore)
    {
        YourScore.text = "Your Score : " + score;
        HighScore.text = "High Score : " + highScore;
        GameEnd.SetActive(true);
    }

}
