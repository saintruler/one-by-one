﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.Events;

public class RelodScene : MonoBehaviour
{
    [SerializeField]
    protected string NextSceneName = "";
    [SerializeField]
    protected int SceneNumber = 0;

    public bool isPointVictory = false;
    public int pointsToVictory;
    // How much monsters should be spawned after limit is exceeded (not exactly, waves are not cut)
    public int monsterAdditionLimit = 12;
    public static bool isVictory = false;
    public int TotalValue = 0;
    private float maxvalue = 0;

    protected GameObject Canvas;

    public static UnityEvent OnSceneChange = new UnityEvent();

    protected virtual void Awake()
    {
        CharacterLife.isDeath = false;
        Canvas = GameObject.FindGameObjectWithTag("Canvas");
        var arena = GetComponent<ArenaEnemySpawner>();
        maxvalue = arena.EnemyCount();

        Canvas.transform.GetChild(0).gameObject.SetActive(false);
        isVictory = false;
        PlayerPrefs.SetInt("CurrentScene", SceneManager.GetActiveScene().buildIndex);
    }

    protected virtual void Start()
    {
        MonsterLife.OnEnemyDead.AddListener(UpdateScoreByOne);
    }

    private void UpdateScoreByOne()
    {
        UpdateScore(1);
    }

    public virtual void UpdateScore(int val = 1)
    {
        TotalValue = TotalValue + val;
        CheckVictoryCondition();
    }

    private bool experimentalReloadRoom = false; 

    protected virtual void Update()
    {
        if (CharacterLife.isDeath) PressR();

        ArenaEnemySpawner arena = GetComponent<ArenaEnemySpawner>();
        if (isVictory) {
            if (!arena.labirintMode) { ProcessVictory(); }
            else { ProcessRoomVictory(); }
        }

        if (Input.GetKeyDown(KeyCode.R) && (Input.GetKey(KeyCode.LeftControl) || CharacterLife.isDeath))
        {
            Metrics.OnDeath();
            if (experimentalReloadRoom)
                Labirint.instance.ReloadRoom();
            else
                Reload();
        }            
    }

    protected virtual void ProcessVictory()
    {
        CurrentEnemyUI.SetCurrentEnemy(" ");
        isVictory = true;
        Canvas.transform.GetChild(0).gameObject.SetActive(true);
        if (Input.GetKeyDown(KeyCode.F) && !CharacterLife.isDeath)
        {
            Canvas.transform.GetChild(0).gameObject.SetActive(false);
            SceneManager.LoadScene(NextSceneName);
            Metrics.OnWin();
            MonsterLife.ClearUsedNames();
            OnSceneChange?.Invoke();
        }
    }

    protected virtual void ProcessRoomVictory()
    {
        CurrentEnemyUI.SetCurrentEnemy(" ");
        isVictory = true;
        //Metrics.OnWin();
        GetComponent<Room>().UnlockRoom();
    }

    /// <summary>
    /// Updates isVictory field and returns it
    /// </summary>
    /// <returns></returns>
    protected virtual bool CheckVictoryCondition()
    {
        var pointToVictory = isPointVictory ? pointsToVictory : maxvalue;
        isVictory = TotalValue >= pointToVictory;
        return isVictory;
    }

    protected virtual void Reload()
    {
        TotalValue = 0;
        Canvas.transform.GetChild(1).gameObject.SetActive(false);
        MonsterLife.ClearUsedNames();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PressR()
    {
        Canvas.transform.GetChild(1).gameObject.SetActive(true);
    }
}

/*[CustomEditor(typeof(RelodScene))]
public class MyEditorClass : Editor
{
    public override void OnInspectorGUI()
    {
        // If we call base the default inspector will get drawn too.
        // Remove this line if you don't want that to happen.
        //base.OnInspectorGUI();

        RelodScene myReload = target as RelodScene;

        myReload.NextSceneName = EditorGUILayout.TextField("NextLevel", myReload.NextSceneName);
        myReload.SceneNumber = EditorGUILayout.IntField("Scene Number", myReload.SceneNumber);
        myReload.isPointVictory = EditorGUILayout.Toggle("isPointVictory", myReload.isPointVictory);

        if (myReload.isPointVictory)
        {
            myReload.pointsToVictory = EditorGUILayout.IntField("Points to victory:", myReload.pointsToVictory);

        }
    }
}*/
