using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Collectibles")]
    public GameObject winPanel;
    public TextMeshProUGUI cubeCountText;
    public AudioClip winSound;
    
    private int _totalCubes = 0;
    private int _collectedCubes = 0;
    [SerializeField] private AudioSource _audioSource;
    
    // Singleton instance for easy access
    public static GameManager Instance { get; private set; }
    
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
        
        // Initially hide win panel if assigned
        if (winPanel != null)
        {
            winPanel.SetActive(false);
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
}