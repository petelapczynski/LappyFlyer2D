using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeParallaxer : MonoBehaviour
{

    class PoolObject
    {
        public Transform transform;
        public bool inUse;
        public int poolObjIndex;
        public PoolObject(Transform t, int i)
        {
            transform = t;
            poolObjIndex = i;
        }
        public void Use()
        {
            inUse = true;
        }
        public void Dispose()
        {
            inUse = false;
        }
        
    }

    [System.Serializable]
    public struct YSpawnRange
    {
        public float min;
        public float max;
    }

    public GameObject[] Prefabs;
    public GameObject ScoreZone;
    public int poolSize;
    public float shiftSpeed;
    public float spawnRate;

    public YSpawnRange ySpawnRange;
    public Vector3 defaultSpawnPos;
    public bool spawnImmediate; // particle prewarm
    public Vector3 immediateSpawnPos;
    public Vector2 targetAspectRatio;

    float spawnTimer;
    float targetAspect;
    List<PoolObject> poolObjects;
    GameManager game;
    int currentObject;
    float pipeGap;

    void Awake()
    {
        Configure();
    }

    void Start()
    {
        game = GameManager.Instance;
    }

    void OnEnable()
    {
        GameManager.OnGameOverConfirmed += OnGameOverConfirmed;
    }

    void OnDisable()
    {
        GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;
    }

    void OnGameOverConfirmed()
    {
        for (int i = 0; i < poolObjects.Count; i++)
        {
            poolObjects[i].Dispose();
            Vector3 pos = poolObjects[i].transform.position;
            pos.x = 50;
            poolObjects[i].transform.position = pos;
        }

        if (spawnImmediate)
        {
            SpawnImmediate();
        }

    }

    void Update()
    {
        if (game.GameOver) return;

        Shift();
        spawnTimer += Time.deltaTime;
        if (spawnTimer > spawnRate)
        {
            Spawn();
            spawnTimer = 0;
        }
    }

    void Configure()
    {
        //spawning pool objects
        pipeGap = PlayerPrefs.GetFloat("PlayerHeight",3.0f);
        currentObject = 0;
        targetAspect = targetAspectRatio.x / targetAspectRatio.y;
        poolObjects = new List<PoolObject>();
        int iPoolSize = 0;
        float yPos = 0f;
        do {
            var preFabs = getPrefab();
            for (int i = 0; i < preFabs.Length; i++) {
                GameObject go = Instantiate( preFabs[i] ) as GameObject;
                Transform t = go.transform;
		        t.SetParent(transform);
                Vector3 pos = Vector3.zero;
                pos.x = 50;
                if (i == 0) {
                    yPos = Random.Range(ySpawnRange.min, ySpawnRange.max);
                    pos.y = yPos;
                } else {
                    yPos += (pipeGap);
                    pos.y = yPos;
                }
                
		        t.position = pos;
                        
                poolObjects.Add( new PoolObject(t,i) );
                iPoolSize ++;
            }
        } while (iPoolSize <= poolSize);	
		if (spawnImmediate) {
			SpawnImmediate();
		}
    }

    void Spawn()
    {
        //moving pool objects into place
        bool bDone = false;
        do {
            Transform t = GetPoolObject();
            if (t == null) return;
            Vector3 pos = t.position;
            pos.x = (defaultSpawnPos.x * Camera.main.aspect) / targetAspect;
            t.position = pos;
            NextPoolObject();
            if (GetPoolObjectIndex() == 0) {
                bDone = true;
            }
        } while (bDone == false);
        
    }

    void SpawnImmediate()
    {
        //moving pool objects into Immediate Spawn place
        bool bDone = false;
        do {
            Transform t = GetPoolObject();
            if (t == null) return;
            Vector3 pos = t.position;
            pos.x = (immediateSpawnPos.x * Camera.main.aspect) / targetAspect;
            t.position = pos;
            NextPoolObject();
            if (GetPoolObjectIndex() == 0) {
                bDone = true;
            }
        } while (bDone == false);

        Spawn();
    }

    void Shift()
    {
        //loop through pool objects moving them //discarding them as they go off screen
        for (int i = 0; i < poolObjects.Count; i++)
        {
            poolObjects[i].transform.localPosition += -Vector3.right * shiftSpeed * Time.deltaTime;
            CheckDisposeObject(poolObjects[i]);
        }
    }

    void CheckDisposeObject(PoolObject poolObject)
    {
        //place objects off screen
        if (poolObject.transform.position.x < (-defaultSpawnPos.x * Camera.main.aspect) / targetAspect)
        {
            poolObject.Dispose();
            Vector3 pos = poolObject.transform.position;
            pos.x = 50;
            poolObject.transform.position = pos;
        }
    }

    void NextPoolObject() {
        //next pool object
        currentObject++;
        if (currentObject > poolObjects.Count - 1) {
            currentObject = 0;
        }
    }

    Transform GetPoolObject()
    {
        if (!poolObjects[currentObject].inUse)
        {
            poolObjects[currentObject].Use();
            return poolObjects[currentObject].transform;
        }
        else
        {
            for (int i = 0; i < poolObjects.Count; i++)
            {
                if (!poolObjects[i].inUse)
                {
                    poolObjects[i].Use();
                    return poolObjects[i].transform;
                }
            }
        }
        return null;
    }

    int GetPoolObjectIndex() {
        return poolObjects[currentObject].poolObjIndex;
    }

    GameObject[] getPrefab()
    {
        // Prefabs[0] = Pipe, Prefabs[1] = PipeBottom, Prefabs[2] = PipeTop
        int i = Random.Range( 0, Prefabs.Length - 1 );
        
        if (i == 0)
        {
            //Add top AND bottom pipe
            return new GameObject[] { Prefabs[1], Prefabs[2], ScoreZone };
        }
        else
        {
            //Add top OR bottom pipe
            return new GameObject[] { Prefabs[i], ScoreZone };
        }
    }

}