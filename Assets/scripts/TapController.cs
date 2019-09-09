using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TapController : MonoBehaviour {

	public delegate void PlayerDelegate();
	public static event PlayerDelegate OnPlayerDied;
	public static event PlayerDelegate OnPlayerScored;
	public static event PlayerDelegate OnPlayerGoToSpace;
	public static event PlayerDelegate OnPlayerGoToGround;

	public float tapForce;
	public float tiltSmooth;
	public Vector3 startPos;
	public SpriteRenderer[] boostImages;
	public ParticleSystem smokeTrail;
	public AudioSource tapSound;
	public AudioSource scoreSound;
	public AudioSource dieSound;

	Rigidbody2D rigidBody;
	Quaternion downRotation;
	Quaternion forwardRotation;
	float time;

	GameManager game;
	bool mouseDown;

	void Start() {
		game = GameManager.Instance;
		rigidBody = GetComponent<Rigidbody2D>();
		downRotation = Quaternion.Euler(0, 0 ,-30);
		forwardRotation = Quaternion.Euler(0, 0, 30);
		rigidBody.simulated = false;
		time = Time.time;
		for (int i = 0; i < boostImages.Length; i++) {
			boostImages[i].enabled = false;
		} 
	}

	void OnEnable() {
		GameManager.OnGameStarted += OnGameStarted;
		GameManager.OnGameOverConfirmed += OnGameOverConfirmed;
	}

	void OnDisable() {
		GameManager.OnGameStarted -= OnGameStarted;
		GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;
	}

	void OnGameStarted() {
		rigidBody.velocity = Vector3.zero;
		rigidBody.simulated = true;
		smokeTrail.Play();
	}

	void OnGameOverConfirmed() {
		transform.localPosition = startPos;
		transform.rotation = Quaternion.identity;
		rigidBody.velocity = Vector3.zero;
		rigidBody.simulated = false;
	}

	void Update() {
		if (game.GameOver) return;

		if (Input.GetMouseButtonDown(0)){
			mouseDown = true;
			var emission = smokeTrail.emission;
			emission.rateOverTimeMultiplier = 100.0f;
		}
		
		if (Input.GetMouseButtonUp(0)){
			mouseDown = false;	
			var emission = smokeTrail.emission;
			emission.rateOverTimeMultiplier = 10.0f;		
		}

		if (mouseDown) {
			rigidBody.AddForce(Vector2.up * tapForce, ForceMode2D.Force);
			transform.rotation = Quaternion.Lerp(transform.rotation, forwardRotation, tiltSmooth * Time.deltaTime);	
			//tapSound.Play();

			// boost images
			for (int i = 0; i < boostImages.Length; i++) {
				//static on
				//boostImages[i].enabled = true;

				//toggle on
				if (boostImages[i].enabled) {
					boostImages[i].enabled = false;
				} else {
					boostImages[i].enabled = true;
					if (boostImages[i].flipY) {
				 		boostImages[i].flipY = false;
					} else {
						boostImages[i].flipY = true;
					}
				}
			}

		} else {
			transform.rotation = Quaternion.Lerp(transform.rotation, downRotation, tiltSmooth * Time.deltaTime);

			for (int i = 0; i < boostImages.Length; i++) {
				boostImages[i].enabled = false;
			}
		}
	}

	void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.tag == "ScoreZone") {
			if (Time.time > time + 1f ) {
				time = Time.time;
				OnPlayerScored(); // event sent to GameManager
				// play a sound
				scoreSound.Play();
			}
		}
		if (col.gameObject.tag == "DeadZone") {
			rigidBody.velocity = Vector3.zero;
			rigidBody.simulated = false;
			mouseDown = false;
			smokeTrail.Stop();
			OnPlayerDied(); // event sent to GameManager
			// play a sounds
			dieSound.Play();
		}
		if (col.gameObject.tag == "GroundZone") {
			if (Time.time > time + 0.2f ) {
				transform.localPosition = startPos;
				smokeTrail.Clear();
				time = Time.time;
				OnPlayerGoToGround(); // event sent to GameManager
				// play a sounds
				scoreSound.Play();
			}
		}
		if (col.gameObject.tag == "SpaceZone") {
			if (Time.time > time + 0.2f ) {
				transform.localPosition = startPos;
				smokeTrail.Clear();
				time = Time.time;
				OnPlayerGoToSpace(); // event sent to GameManager
				// play a sounds
				scoreSound.Play();
			}
		}
	}
}