using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public delegate void GameDelegate();
    public static event GameDelegate OnGameStarted;
    public static event GameDelegate OnGameOverConfirmed;

    public static GameManager Instance;

    public GameObject startPage;
    public GameObject gameOverPage;
    public GameObject countdownPage;
		public GameObject sceneGround;
		public GameObject sceneSpace;

    public Text scoreText;

    enum PageState {
        None,
        Start,
        GameOver,
        Countdown
    }

    int score = 0;
    bool gameOver = true;

    public bool GameOver { get { return gameOver; } }
 
    void Awake() {
			if (Instance != null) {
				Destroy(gameObject);
			}
			else {
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
    }

    void OnEnable() {
      TapController.OnPlayerDied += OnPlayerDied;
			TapController.OnPlayerScored += OnPlayerScored;
			TapController.OnPlayerGoToSpace += OnPlayerGoToSpace;
			TapController.OnPlayerGoToGround += OnPlayerGoToGround;
			CountdownText.OnCountdownFinished += OnCountdownFinished;
    }

    void OnDisable() {
      TapController.OnPlayerDied -= OnPlayerDied;
			TapController.OnPlayerScored -= OnPlayerScored;
			TapController.OnPlayerGoToSpace -= OnPlayerGoToSpace;
			TapController.OnPlayerGoToGround -= OnPlayerGoToGround;
			CountdownText.OnCountdownFinished -= OnCountdownFinished;
    }

  	void OnCountdownFinished() {
			SetPageState(PageState.None);
			OnGameStarted();
			score = 0;
			scoreText.gameObject.SetActive(true);
			gameOver = false;
		}

		void OnPlayerScored() {
			score++;
			scoreText.text = score.ToString();
		}

		void OnPlayerDied() {
			gameOver = true;
			int savedScore = PlayerPrefs.GetInt("HighScore");
			if (score > savedScore) {
				PlayerPrefs.SetInt("HighScore", score);
			}
			SetPageState(PageState.GameOver);
		}

		void OnPlayerGoToSpace() {
			sceneSpace.SetActive(true);
			sceneGround.SetActive(false);
			OnGameOverConfirmed();
			OnGameStarted();
		}

		void OnPlayerGoToGround() {
			sceneGround.SetActive(true);
			sceneSpace.SetActive(false);
			OnGameOverConfirmed();
			OnGameStarted();
		}

    void SetPageState(PageState state) {
			switch (state) {
				case PageState.None:
					startPage.SetActive(false);
					gameOverPage.SetActive(false);
					countdownPage.SetActive(false);
					break;
				case PageState.Start:
					startPage.SetActive(true);
					gameOverPage.SetActive(false);
					countdownPage.SetActive(false);
					break;
				case PageState.Countdown:
					startPage.SetActive(false);
					gameOverPage.SetActive(false);
					countdownPage.SetActive(true);
					break;
				case PageState.GameOver:
					startPage.SetActive(false);
					gameOverPage.SetActive(true);
					countdownPage.SetActive(false);
					break;
			}
	}

	public void ConfirmGameOver() {
		scoreText.gameObject.SetActive(false);
		SetPageState(PageState.Start);
		scoreText.text = "0";
		OnGameOverConfirmed();
	}

	public void StartGame() {
		SetPageState(PageState.Countdown);
	}

}
