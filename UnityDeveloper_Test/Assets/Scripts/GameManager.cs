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
    public AudioClip winSound;
    public AudioClip loseSound;
    
    private int _totalCubes = 0;
    private int _collectedCubes = 0;
    [SerializeField] private AudioSource _audioSource;
    
    // Track if gameplay is active
    private bool _isGameplayActive = true;
    
    // Singleton instance for easy access
    public static GameManager Instance { get; private set; }
    
    // Public property to check if gameplay is active
    public bool IsGameplayActive => _isGameplayActive;
    
    private void Awake()
    {
        // Set up singleton
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
        
        // Dynamically count all collectible cubes in the scene
        CollectibleCube[] cubes = FindObjectsOfType<CollectibleCube>();
        _totalCubes = cubes.Length;
        
        // Initially hide UI panels if assigned
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        UpdateCubeCounter();
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
        
        Debug.Log("Congratulations! All cubes collected!");
    }
    
    // Simple game over method
    public void GameOver(string message)
    {
        // Disable gameplay
        _isGameplayActive = false;
        if (_audioSource != null && winSound != null)
        {
            _audioSource.PlayOneShot(loseSound);
        }
        
        // Show game over panel after a short delay
        StartCoroutine(ShowGameOverPanel());
        
        Debug.Log("Game Over: " + message);
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
        // Log a message to the console (useful for testing in the Editor)
        Debug.Log("Quitting game...");
    
#if UNITY_EDITOR
        // If in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If in a built game
        Application.Quit();
#endif
    }
}