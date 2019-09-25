using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public delegate void GameDelegate();
    public static event GameDelegate OnGameStarted;
    public static event GameDelegate OnGameOverConfirmed;
	public static event GameDelegate OnSpeedUp;
	public static event GameDelegate OnSpeedDown;

    public static GameManager Instance;

    public GameObject startPage;
    public GameObject gameOverPage;
    public GameObject countdownPage;
	public GameObject infoPage;
	public GameObject sceneGround;
	public GameObject sceneSpace;
	public GameObject sceneMoon;
	public GameObject sceneSolar;
	public GameObject speedDown;
	public GameObject speedUp;

    public Text scoreText;

    enum PageState {
        None,
        Start,
        GameOver,
        Countdown,
		Info
    }

	string GameScene; 
    int score = 0;
    bool gameOver = true;

    public bool GameOver { get { return gameOver; } }
 
    void Awake() {
			if (Instance != null) {
			 	Destroy(gameObject);
			}
			else { 
				Instance = this;
			// 	DontDestroyOnLoad(gameObject);
			}
    }

	void Start() {
		SetGameScene("GROUND");
		SetPageState(PageState.Start);
		PlayerSelected();
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
		speedDown.SetActive(true);
		speedUp.SetActive(true);
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
		OnGameOverConfirmed();
		switch (GameScene) {
			case "GROUND":
				SetGameScene("SPACE");
				break;
			case "SPACE":
				SetGameScene("MOON");		
				break;
			case "MOON":
				SetGameScene("SOLAR");			
				break;	
			case "SOLAR":
				break;					
		}
		OnGameStarted();
	}

	void OnPlayerGoToGround() {
		OnGameOverConfirmed();
		switch (GameScene) {
			case "GROUND":
				break;
			case "SPACE":
				SetGameScene("GROUND");		
				break;
			case "MOON":
				SetGameScene("SPACE");			
				break;	
			case "SOLAR":
				SetGameScene("MOON");	
				break;					
		}
		OnGameStarted();
	}

	void SetGameScene(string zone) {
		//Debug.Log("GameManager: SetGameScene: " + zone);
		sceneGround.SetActive(false);
		sceneSpace.SetActive(false);
		sceneMoon.SetActive(false);
		sceneSolar.SetActive(false);

		switch (zone) {
			case "GROUND":
				sceneGround.SetActive(true);
				break;
			case "SPACE":
				sceneSpace.SetActive(true);			
				break;
			case "MOON":
				sceneMoon.SetActive(true);				
				break;	
			case "SOLAR":
				sceneSolar.SetActive(true);
				break;					
		}
		GameScene = zone;
	}

    void SetPageState(PageState state) {
		startPage.SetActive(false);
		gameOverPage.SetActive(false);
		countdownPage.SetActive(false);
		infoPage.SetActive(false);
		
		switch (state) {
			case PageState.None:
				break;
			case PageState.Start:
				startPage.SetActive(true);
				break;
			case PageState.Countdown:
				countdownPage.SetActive(true);
				break;
			case PageState.GameOver:
				gameOverPage.SetActive(true);
				break;
			case PageState.Info:
				infoPage.SetActive(true);
				break;
		}
	}

	void PlayerSelected() {
		string playerSelected = PlayerPrefs.GetString("PlayerSelected","player_mini");
		GameObject[] gobs;
		gobs = Resources.FindObjectsOfTypeAll<GameObject>();

		foreach (GameObject go in gobs) {
			if (go.name == playerSelected) {
				go.SetActive(true);
				//Debug.Log("GameManager: PlayerSelected: " + go.name);
				float playerHeight = go.GetComponent<PolygonCollider2D>().bounds.size.y;
				float playerWidth = go.GetComponent<PolygonCollider2D>().bounds.size.x;

				if (playerHeight > 0) {
					PlayerPrefs.SetFloat("PlayerHeight", playerHeight);
				}
				if (playerWidth > 0) {
					PlayerPrefs.SetFloat("PlayerWidth", playerWidth);
				}
				
			}
		}

	}

	public void ConfirmGameOver() {
		scoreText.gameObject.SetActive(false);
		speedDown.SetActive(false);
		speedUp.SetActive(false);
		SetPageState(PageState.Countdown);
		scoreText.text = "0";
		OnGameOverConfirmed();
	}

	public void StartGame() {
		SetPageState(PageState.Countdown);
	}

	public void SelectPlayerScene() {
		SceneManager.LoadScene("characterSelection");
	}

	public void StartInfo() {
		SetPageState(PageState.Info);
	}
	
	public void ReturnToStart() {
		SetPageState(PageState.Start);
	}

	public void ExitGame() {
		Application.Quit();
	}

	public void ClickSpeedUp() {
		OnSpeedUp();
	}

	public void ClickSpeedDown() {
		OnSpeedDown();
	}

}
