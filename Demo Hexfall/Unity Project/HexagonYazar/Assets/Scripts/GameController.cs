using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    GridManager grid;

    [SerializeField]TMP_Text scoreText;
    [SerializeField] TMP_Text gameOverScoreText;
    [SerializeField] TMP_Text blocksDestroyed;
    [SerializeField] TMP_Text BombsDestroyed;
    [SerializeField] GameObject MainMenu;
    [SerializeField] GameObject GameOverMenu;
    
    public float score = 0;

    private float level = 1;

    private void Awake() {
        grid = FindObjectOfType<GridManager>();
    }

    private void Update() {
        // update score text
        scoreText.text = "Score : " + score.ToString();

        if(score > (1000 * level))  // if passes multiples of 1000 create 1 bomb
        {
            level++;
            grid.generateBomb = true;
        }

        if(grid.gameOver)       // if game over, than open game over screen
        {
            MainMenu.SetActive(false);
            GameOverMenu.SetActive(true);
            gameOverScoreText.text = "Score : " + score.ToString();
            blocksDestroyed.text = "Blocks Destroyed : " + (score/5).ToString();
            BombsDestroyed.text = "Bombs : " + ((int)(score / 1000)).ToString();
        }
    }

    //adding points to score when we find a match
    public void EarnPoints(int point)
    {
        score += point;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
