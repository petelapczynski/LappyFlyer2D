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
	public AudioSource tapSound;
	public AudioSource scoreSound;
	public AudioSource dieSound;

	Rigidbody2D rigidBody;
	Quaternion downRotation;
	Quaternion forwardRotation;

	GameManager game;
	bool mouseDown;

	void Start() {
		rigidBody = GetComponent<Rigidbody2D>();
		downRotation = Quaternion.Euler(0, 0 ,-30);
		forwardRotation = Quaternion.Euler(0, 0, 30);
		game = GameManager.Instance;
		rigidBody.simulated = false;
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
	}

	void OnGameOverConfirmed() {
		transform.localPosition = startPos;
		transform.rotation = Quaternion.identity;
	}

	void Update() {
		if (game.GameOver) return;

		if (Input.GetMouseButtonDown(0)){
			mouseDown = true;
		}

		if (Input.GetMouseButton(0)) {
			//rigidBody.velocity = Vector2.zero;
			//transform.rotation = forwardRotation;
			rigidBody.AddForce(Vector2.up * tapForce, ForceMode2D.Force);
			transform.rotation = Quaternion.Lerp(transform.rotation, forwardRotation, tiltSmooth * Time.deltaTime);
			//tapSound.Play();
		}

		if (Input.GetMouseButtonUp(0)){
			mouseDown = false;
		}

		if (!mouseDown) {
			transform.rotation = Quaternion.Lerp(transform.rotation, downRotation, tiltSmooth * Time.deltaTime);
		}
		
	}

	void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.tag == "ScoreZone") {
			OnPlayerScored(); // event sent to GameManager
			// play a sound
			scoreSound.Play();
		}
		if (col.gameObject.tag == "DeadZone") {
			rigidBody.simulated = false;
			OnPlayerDied(); // event sent to GameManager
			// play a sounds
			dieSound.Play();
		}
		if (col.gameObject.tag == "SpaceZone") {
			//rigidBody.simulated = false;
			OnPlayerGoToSpace(); // event sent to GameManager
			// play a sounds
			scoreSound.Play();

			//OnGameOverConfirmed();
			//OnGameStarted();
			// transform.localPosition = startPos;
			// rigidBody.velocity = Vector3.zero;
			// rigidBody.AddForce(Vector2.up * tapForce, ForceMode2D.Force);
		}
		if (col.gameObject.tag == "GroundZone") {
			//rigidBody.simulated = false;
			OnPlayerGoToGround(); // event sent to GameManager
			// play a sounds
			scoreSound.Play();

			//OnGameOverConfirmed();
			//OnGameStarted();
			// transform.localPosition = startPos;
			// rigidBody.velocity = Vector3.zero;
			// rigidBody.AddForce(-Vector2.up * tapForce, ForceMode2D.Force);
		}
	}
}