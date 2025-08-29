using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game State")]
    [SerializeField] private bool isGamePaused = false;
    [SerializeField] private bool isGameOver = false;
    
    [Header("Player References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject mainHUD;
    
    private GameObject currentPlayer;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        InitializeGame();
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void InitializeGame()
    {
        // Spawn player if not exists
        if (currentPlayer == null && playerPrefab != null)
        {
            Vector3 spawnPosition = playerSpawnPoint != null ? 
                playerSpawnPoint.position : Vector3.zero;
            
            currentPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        }
        
        // Setup UI
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (gameOverMenu != null) gameOverMenu.SetActive(false);
        if (mainHUD != null) mainHUD.SetActive(true);
        
        // Resume game
        ResumeGame();
    }
    
    private void HandleInput()
    {
        // Pause/Resume with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        
        // Show cursor for 3rd person game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(true);
        }
        
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public bool IsGamePaused()
    {
        return isGamePaused;
    }
    
    public bool IsGameOver()
    {
        return isGameOver;
    }
    
    public GameObject GetCurrentPlayer()
    {
        return currentPlayer;
    }
}
