using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{

    public Image fadePlane;
    // public Image fadePlane;
    public GameObject pauseButton;
    public GameObject gameOverUI;
    public GameObject pauseUI;
    public RectTransform newWaveBanner;
    public Slider[] volumeSliders;

    public TMP_Text newWaveTitle;
    public TMP_Text newWaveEnemyCount;
    //public RectTransform healthBar;

    Spawner spawner;
    PlayerController player;

    void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        player.onDeath += OnGameOver;
        
        volumeSliders[0].value = AudioManager.instance.masterVolumePercent;
        volumeSliders[1].value = AudioManager.instance.musicVolumePercent;
        volumeSliders[2].value = AudioManager.instance.sfxVolumePercent;

        

    }


    void Update()
   {
      
       float healthPercent = 0;
       if (player != null)
       {
         //  healthPercent = player.health / player.startingHealth;
       }
      // healthBar.localScale = new Vector3(healthPercent, 1, 1);
   }

   void OnNewWave(int waveNumber)
   {
       string[] numbers = { "One", "Two", "Three", "Four", "Five" };
       newWaveTitle.text = "- Wave " + numbers[waveNumber - 1] + " -";
       string enemyCountString = spawner.waves[waveNumber - 1].enemyNum + "";
       newWaveEnemyCount.text = "Enemies: " + enemyCountString;

       StopCoroutine("AnimateNewWaveBanner");
       StartCoroutine("AnimateNewWaveBanner");
   }

    void OnGameOver()
    {
        Cursor.visible = true;
        
        StartCoroutine(Fade(Color.clear, new Color(0, 0, 0, .95f), 1));
        //  healthBar.transform.parent.gameObject.SetActive(false);
        gameOverUI.SetActive(true);

    }
    
    IEnumerator AnimateNewWaveBanner()
    {

        float delayTime = 1.5f;
        float speed = 3f;
        float animatePercent = 0;
        int dir = 1;

        float endDelayTime = Time.time + 1 / speed + delayTime;

        while (animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * speed * dir;

            if (animatePercent >= 1)
            {
                animatePercent = 1;
                if (Time.time > endDelayTime)
                {
                    dir = -1;
                }
            }

            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-900, -170, animatePercent);
            yield return null;
        }

    }
    
    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    // UI Input
    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");
        gameOverUI.SetActive(true);
    }
    public void OpenPauseMenu()
    {
        Time.timeScale = 0;
        pauseButton.SetActive(false);
        pauseUI.SetActive(true);

    }
    public void Resume()
    {
        Time.timeScale = 1;
        pauseButton.SetActive(true);
        pauseUI.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void SetMasterVolume()//(float value)
    {
        // Debug.Log(volumeSliders[0].value);
        AudioManager.instance.SetVolume(volumeSliders[0].value, AudioManager.AudioChannel.Master);
    }

    public void SetMusicVolume()
    {
        AudioManager.instance.SetVolume(volumeSliders[1].value, AudioManager.AudioChannel.Music);
    }

    public void SetSfxVolume()
    {
        AudioManager.instance.SetVolume(volumeSliders[2].value, AudioManager.AudioChannel.Sfx);
    }

}
