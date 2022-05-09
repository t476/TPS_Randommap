using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour {

	public enum AudioChannel {Master, Sfx, Music};
   //这样别的类只能访问，不能改写
	public float masterVolumePercent { get; private set; }//主
	public float sfxVolumePercent { get; private set; }
	public float musicVolumePercent { get; private set; }//音效

	AudioSource sfx2DSource;//2d声音
	AudioSource[] musicSources;//音乐淡入淡出
	int activeMusicSourceIndex;

	public static AudioManager instance;//单例模式

	Transform audioListener;
	Transform playerT;

	SoundLibrary library;

	void Awake() {
		if (instance != null) {
			Destroy (gameObject);
		} else {
           
			instance = this;
			DontDestroyOnLoad (gameObject);

			library = GetComponent<SoundLibrary> ();

			musicSources = new AudioSource[2];
			for (int i = 0; i < 2; i++) {
				GameObject newMusicSource = new GameObject ("Music source " + (i + 1));//命名
				musicSources [i] = newMusicSource.AddComponent<AudioSource> ();
				newMusicSource.transform.parent = transform;
			}
			GameObject newSfx2Dsource = new GameObject ("2D sfx source");
			sfx2DSource = newSfx2Dsource.AddComponent<AudioSource> ();
			newSfx2Dsource.transform.parent = transform;

			audioListener = FindObjectOfType<AudioListener> ().transform;
			if (FindObjectOfType<PlayerController> () != null) {
				playerT = FindObjectOfType<PlayerController> ().transform;
			}

			masterVolumePercent = PlayerPrefs.GetFloat ("master vol", 1);
			sfxVolumePercent = PlayerPrefs.GetFloat ("sfx vol", 1);
			musicVolumePercent = PlayerPrefs.GetFloat ("music vol", 1);
		}
	}

    
 /*
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
            if (playerT == null)
            {
                if (FindObjectOfType<PlayerController>() != null)
                {
                    playerT = FindObjectOfType<PlayerController>().transform;
                }
            }
        }
    */
    void OnLevelWasLoaded(int index) {
		if (playerT == null) {
			if (FindObjectOfType<PlayerController> () != null) {
				playerT = FindObjectOfType<PlayerController> ().transform;
			}
		}
	}

	void Update() {
		if (playerT != null) {
			audioListener.position = playerT.position;
		}
	}

	public void SetVolume(float volumePercent, AudioChannel channel) {
		switch (channel) {
		case AudioChannel.Master:
			masterVolumePercent = volumePercent;
			break;
		case AudioChannel.Sfx:
			sfxVolumePercent = volumePercent;
			break;
		case AudioChannel.Music:
			musicVolumePercent = volumePercent;
			break;
		}

		musicSources [0].volume = musicVolumePercent * masterVolumePercent;
		musicSources [1].volume = musicVolumePercent * masterVolumePercent;
        //用于数据本地持久化保存与读取的类
        PlayerPrefs.SetFloat ("master vol", masterVolumePercent);
		PlayerPrefs.SetFloat ("sfx vol", sfxVolumePercent);
		PlayerPrefs.SetFloat ("music vol", musicVolumePercent);
		PlayerPrefs.Save ();
	}

	public void PlayMusic(AudioClip clip, float fadeDuration = 1) {
        musicSources[1 - activeMusicSourceIndex].enabled = true;
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
		musicSources [activeMusicSourceIndex].clip = clip;
		musicSources [activeMusicSourceIndex].Play ();

		StartCoroutine(AnimateMusicCrossfade(fadeDuration));
        musicSources[1 - activeMusicSourceIndex].enabled=false;


    }
    //可以重载
	public void PlaySound(AudioClip clip, Vector3 pos) {
		if (clip != null) {
			AudioSource.PlayClipAtPoint (clip, pos, sfxVolumePercent * masterVolumePercent);
		}
	}

	public void PlaySound(string soundName, Vector3 pos) {
		PlaySound (library.GetClipFromName (soundName), pos);
	}

    //比如levelcompleted
	public void PlaySound2D(string soundName) {
		sfx2DSource.PlayOneShot (library.GetClipFromName (soundName), sfxVolumePercent * masterVolumePercent);
	}


	IEnumerator AnimateMusicCrossfade(float duration) {
		float percent = 0;

		while (percent < 1) {
			percent += Time.deltaTime * 1 / duration;
			musicSources [activeMusicSourceIndex].volume = Mathf.Lerp (0, musicVolumePercent * masterVolumePercent, percent);
			musicSources [1-activeMusicSourceIndex].volume = Mathf.Lerp (musicVolumePercent * masterVolumePercent, 0, percent);
			yield return null;
		}
	}
}
