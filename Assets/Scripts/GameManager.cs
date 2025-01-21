using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // For scene management

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Ghost[] ghosts;
    [SerializeField] private Pacman pacman;
    [SerializeField] private Transform pellets;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;

    public int score { get; private set; } = 0;
    public int lives { get; private set; } = 1; // Default to 1

    private int ghostMultiplier = 1;
    private AudioManager audioManager;

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
     audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

        GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioObject != null)
        {
            audioManager = audioObject.GetComponent<AudioManager>();
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Ensure an object tagged 'Audio' exists with an AudioManager component.");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (gameOverText != null) gameOverText.enabled = false;
        NewGame();
    }

    private void Update()
    {
        if (lives <= 0 && Input.anyKeyDown)
        {
            NewGame();
        }
    }

    private void NewGame()
    {
        SetScore(0);
        SetLives(1); // Lives remain 1
        NewRound();
    }

    private void NewRound()
    {
        if (gameOverText != null) gameOverText.enabled = false;

        foreach (Transform pellet in pellets)
        {
            pellet.gameObject.SetActive(true);
        }

        ResetState();
    }

    private void ResetState()
    {
        foreach (Ghost ghost in ghosts)
        {
            ghost.ResetState();
        }

        if (pacman != null)
        {
            pacman.ResetState();
        }
        else
        {
            Debug.LogWarning("Pacman not assigned in GameManager!");
        }
    }

    private void GameOver()
    {
        if (gameOverText != null) gameOverText.enabled = true;

        foreach (Ghost ghost in ghosts)
        {
            ghost.gameObject.SetActive(false);
        }

        if (pacman != null) pacman.gameObject.SetActive(false);

        Invoke(nameof(ReturnToMainMenu), 1f);
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Main Menu"); // Update with your scene name
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        if (livesText != null)
        {
            livesText.text = "x" + lives.ToString();
        }
        else
        {
            Debug.LogWarning("LivesText not assigned in GameManager!");
        }
    }

    private void SetScore(int score)
    {
        this.score = score;
        if (scoreText != null)
        {
            scoreText.text = score.ToString().PadLeft(2, '0');
        }
        else
        {
            Debug.LogWarning("ScoreText not assigned in GameManager!");
        }
    }

    public void PacmanEaten()
{
    if (pacman != null)
    {
        pacman.DeathSequence();
    }

    if (audioManager != null)
    {
        audioManager.PlaySFX(audioManager.death); // Play the death sound effect
    }
    else
    {
        Debug.LogWarning("AudioManager is null. Cannot play the death sound effect.");
    }

    SetLives(lives - 1);

    if (lives > 0)
    {
        Invoke(nameof(ResetState), 3f);
    }
    else
    {
        GameOver();
    }
}


    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * ghostMultiplier;
        SetScore(score + points);

        ghostMultiplier++;

        if (audioManager != null) audioManager.PlaySFX(audioManager.ghosteat);
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);

        SetScore(score + pellet.points);

        if (!HasRemainingPellets())
        {
            if (pacman != null) pacman.gameObject.SetActive(false);
            Invoke(nameof(NewRound), 3f);
        }

        if (audioManager != null) audioManager.PlaySFX(audioManager.pointeat);
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        foreach (Ghost ghost in ghosts)
        {
            ghost.frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);

        if (audioManager != null) audioManager.PlaySFX(audioManager.fruiteat);
    }

    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                return true;
            }
        }

        return false;
    }

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }
}