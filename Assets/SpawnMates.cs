using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnMates : MonoBehaviour
{
    [ReadOnly]
    public MateAgent prototypeObject;

    [ReadOnly]
    public BoxCollider spawnVolume;

    [ReadOnly]
    public float spawnAmount = 100;

    public bool canCommunicateFear = true;
    private bool _enableFear;
    private bool enableFear
    {
        get { return _enableFear;  }
        set
        {
            if (value != _enableFear)
            {
                MateAgent[] agents = GameObject.FindObjectsOfType<MateAgent>();
                for (int i = 0; i < agents.Length; ++i)
                    agents[i].isFearContagious = value;

                _enableFear = value;
            }
        }
    }

    public bool ellectQueen = true;
    private bool _enableQueen;
    private bool enableQueen
    {
        get { return _enableQueen; }
        set
        {
            if (value != _enableQueen)
            {
                if (value)
                {
                    Queen = spawnedMates[Random.Range(0, spawnedMates.Count - 1)];
                    Queen.GetComponent<Renderer>().material = QueenMaterial;
                }
                else
                {
                    Queen.GetComponent<Renderer>().material = MateMaterial;
                }
                _enableQueen = value;
            }
        }
    }


    [ReadOnly]
    public Material MateMaterial;

    [ReadOnly]
    public Material QueenMaterial;

    List<MateAgent> spawnedMates = new List<MateAgent>();

	// Use this for initialization
	void Awake()
    {
        var foreverAloneGameObject = GameObject.FindGameObjectWithTag("ForeverAlone");

        Bounds bounds = spawnVolume.bounds;
        for (int i  = 0; i < spawnAmount; ++i)
        {
            Vector3 position = new Vector3(Random.Range(bounds.min.x, bounds.max.x), 0.5f, Random.Range(bounds.min.z, bounds.max.z));
            MateAgent mate = Instantiate<MateAgent>(prototypeObject);
            mate.transform.position = position;
            mate.whatToAvoid = foreverAloneGameObject != null ? foreverAloneGameObject.transform : null;
            mate.isFearContagious = canCommunicateFear;
            spawnedMates.Add(mate);
        }
    }

    public static MateAgent Queen
    {
        get;
        private set;
    }

	// Update is called once per frame
	void Update()
    {
        // Enable Queen if when needed
        enableQueen = ellectQueen;
        // Enable fear communication among agents when needed
        enableFear = canCommunicateFear;
    }
}
