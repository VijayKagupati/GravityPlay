using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject winPanel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI cubeCountText;
    public TextMeshProUGUI timerText;
    public AudioClip winSound;
    public AudioClip loseSound;
    
    [Header("Game Settings")]
    public float gameTimeLimit = 120f;  // 2 minutes in seconds
    
    private int _totalCubes = 0;
    private int _collectedCubes = 0;
    [SerializeField] private AudioSource _audioSource;
    
    // Track if gameplay is active
    private bool _isGameplayActive = true;
    private float _remainingTime;
    
    // Singleton instance for easy access
    public static GameManager Instance { get; private set; }
    
    // Public property to check if gameplay is active
    public bool IsGameplayActive => _isGameplayActive;
    
    private void Awake()
    {
        // Set up singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null && winSound != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Initialize timer
        _remainingTime = gameTimeLimit;
        
        // Dynamically count all collectible cubes in the scene
        CollectibleCube[] cubes = FindObjectsOfType<CollectibleCube>();
        _totalCubes = cubes.Length;
        
        // Initially hide UI panels
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        UpdateCubeCounter();
        UpdateTimerDisplay();
    }
    
    private void Update()
    {
        if (_isGameplayActive)
        {
            // Update timer
            _remainingTime -= Time.deltaTime;
            
            UpdateTimerDisplay();
            
            // Check if time is up
            if (_remainingTime <= 0)
            {
                _remainingTime = 0;
                UpdateTimerDisplay();
                GameOver("Time's up!");
            }
        }
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(_remainingTime / 60);
            int seconds = Mathf.FloorToInt(_remainingTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    public void CubeCollected()
    {
        _collectedCubes++;
        UpdateCubeCounter();
        
        // Check if all cubes are collected
        if (_collectedCubes >= _totalCubes)
        {
            StartCoroutine(WinGame());
        }
    }
    
    private void UpdateCubeCounter()
    {
        if (cubeCountText != null)
        {
            cubeCountText.text = $"Cubes: {_collectedCubes} / {_totalCubes}";
        }
    }
    
    private IEnumerator WinGame()
    {
        // Disable gameplay
        _isGameplayActive = false;
        
        // Play win sound
        if (_audioSource != null && winSound != null)
        {
            _audioSource.PlayOneShot(winSound);
        }
        
        // Small delay before showing win panel
        yield return new WaitForSeconds(0.5f);
        
        // Show win panel
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        
        // Debug.Log("Congratulations! All cubes collected!");
    }
    
    public void GameOver(string message)
    {
        // Only process game over once
        if (!_isGameplayActive) return;
        
        // Disable gameplay
        _isGameplayActive = false;
        if (_audioSource != null && loseSound != null)
        {
            _audioSource.PlayOneShot(loseSound);
        }
        
        // Show game over panel after a short delay
        StartCoroutine(ShowGameOverPanel());
        
        // Debug.Log("Game Over: " + message);
    }
    
    private IEnumerator ShowGameOverPanel()
    {
        // Small delay before showing game over panel
        yield return new WaitForSeconds(0.5f);
        
        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    public void RestartGame()
    {
        // Get the current scene index and reload it
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    public void QuitGame()
    {
        // Debug.Log("Quitting game...");
    
#if UNITY_EDITOR
        // If in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If in a built game
        Application.Quit();
#endif
    }
}