﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallaxer : MonoBehaviour {

	class PoolObject {
		public Transform transform;
		public bool inUse;
		public PoolObject(Transform t) { 
			transform = t; 
		}
		public void Use() { 
			inUse = true; 
		}
		public void Dispose() { 
			inUse = false; 
		}
	}

	[System.Serializable]
	public struct YSpawnRange {
		public float min;
		public float max;
	}

	public GameObject[] Prefabs;
	public int poolSize;
	public float defaultShiftSpeed;
	public float spawnRate;

	public YSpawnRange ySpawnRange;
	public Vector3 defaultSpawnPos;
	public bool spawnImmediate; // particle prewarm
	public Vector3 immediateSpawnPos;
	public Vector2 targetAspectRatio;

	float spawnTimer;
	float targetAspect;
	PoolObject[] poolObjects;
	GameManager game;
	int currentObject;
	float shiftSpeed;

	void Awake() {
		Configure();
	}

	void Start() {
		game = GameManager.Instance;
	}

	void OnEnable() {
		GameManager.OnGameOverConfirmed += OnGameOverConfirmed;
		GameManager.OnSpeedUp += OnSpeedUp;
        GameManager.OnSpeedDown += OnSpeedDown;
	}

	void OnDisable() {
		GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;
		GameManager.OnSpeedUp -= OnSpeedUp;
        GameManager.OnSpeedDown -= OnSpeedDown;
	}

	void OnGameOverConfirmed() {
		for (int i = 0; i < poolObjects.Length; i++) {
			poolObjects[i].Dispose();
			poolObjects[i].transform.position = Vector3.one * 1000;
		}

		if (spawnImmediate) {
			SpawnImmediate();
		}
		//Configure();
	}

	void OnSpeedUp() {
        //increase speed, adjust spawnRate
		shiftSpeed += 0.25f;
		spawnRate = 19.0f / shiftSpeed; 
    }
    
    void OnSpeedDown() {
		//decrease speed, adjust spawnRate
        shiftSpeed -= 0.25f;
        if (shiftSpeed < defaultShiftSpeed) {
            shiftSpeed = defaultShiftSpeed;
        }
		spawnRate = 19.0f / shiftSpeed; 
    }

	void Update() {
		if (game.GameOver) return;

		Shift();
		spawnTimer += Time.deltaTime;
		if (spawnTimer > spawnRate) {
			Spawn();
			spawnTimer = 0;
		}
	}

	void Configure() {
		//spawning pool objects
		shiftSpeed = defaultShiftSpeed;
		currentObject = 0;
		targetAspect = targetAspectRatio.x / targetAspectRatio.y;
		poolObjects = new PoolObject[poolSize];
		for (int i = 0; i < poolObjects.Length; i++) {
			GameObject go = Instantiate( getPrefab() ) as GameObject;
			Transform t = go.transform;
			t.SetParent(transform);
			t.position = Vector3.one * 1000;
			poolObjects[i] = new PoolObject(t);
		}

		if (spawnImmediate) {
			SpawnImmediate();
		}
	}

	void Spawn() {
		//moving pool objects into place
		Transform t = GetPoolObject();
		if ( t == null ) return;
		Vector3 pos = Vector3.zero;
		pos.x = (defaultSpawnPos.x * Camera.main.aspect) / targetAspect;
		pos.y = Random.Range(ySpawnRange.min, ySpawnRange.max);
		t.position = pos;
	}

	void SpawnImmediate() {
		Transform t = GetPoolObject();
		if ( t == null ) return;
		Vector3 pos = Vector3.zero;
		pos.x = (immediateSpawnPos.x * Camera.main.aspect) / targetAspect;
		pos.y = Random.Range(ySpawnRange.min, ySpawnRange.max);
		t.position = pos; 
		Spawn();
	}

	void Shift() {
		//loop through pool objects moving them //discarding them as they go off screen
		for (int i = 0; i < poolObjects.Length; i++) {
			poolObjects[i].transform.localPosition += -Vector3.right * shiftSpeed * Time.deltaTime;
			CheckDisposeObject(poolObjects[i]);
		}
	}

	void CheckDisposeObject(PoolObject poolObject) {
		//place objects off screen
		if (poolObject.transform.position.x < (-defaultSpawnPos.x * Camera.main.aspect) / targetAspect) {
			poolObject.Dispose();
			poolObject.transform.position = Vector3.one * 1000;
		}
	}

	Transform GetPoolObject() {
		//retrieving next pool object
		currentObject++;
		if (currentObject > poolObjects.Length - 1) {
			currentObject = 0;
		}
		if (!poolObjects[currentObject].inUse) {
			poolObjects[currentObject].Use();
			return poolObjects[currentObject].transform;
		} else {
			for (int i = 0; i < poolObjects.Length; i++) {
				if (!poolObjects[i].inUse) {
					poolObjects[i].Use();
					return poolObjects[i].transform;
				}
			}
		}
		return null;
	}

	GameObject getPrefab() {
		int i = Random.Range( 0, Prefabs.Length );
		return Prefabs[i];
	}

}