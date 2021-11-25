﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGameController : MonoBehaviour
{
    private Stack<GameState> states;
    private StageData currentStage;
    private Data.GameState gameState;

    public GameObject levelParent;
    public float lightTime;
    public GameObject[] characters;
    public SpriteRenderer fade;
    public Camera gameCamera;
    public Text timeDisplay;
    public GameObject menu;
    public GameObject win;
    public GameObject lose;
    // Start is called before the first frame update
    void Start()
    {
        states = new Stack<GameState>();
        Color c = fade.color;
        c.a = 0;
        fade.color = c;
        gameState = Data.GameState.menu;
        menu.SetActive(true);
        win.SetActive(false);
        lose.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == Data.GameState.level)
        {
            if (Data.gameOver)
            {
                Data.gameOver = false;
                Lose();
                return;
            }
            else if (Data.victory)
            {
                Data.victory = false;
                Win();
                return;
            }
            else
            {
                lightTime += Data.changeTime;
                Data.changeTime = 0;
            }
            if (characters != null)
            {
                if (Input.GetKey(KeyCode.Tab))
                {
                    Data.rewinding = true;
                    Rewind();
                    if (!currentStage.timeRewindable)
                    {
                        lightTime = Mathf.Max(lightTime - Time.deltaTime, -1 * Data.unrewindableTime);
                    }
                }
                else
                {
                    lightTime = Mathf.Max(lightTime - Time.deltaTime, -1 * Data.unrewindableTime);
                    Data.rewinding = false;
                    GameState state = new GameState(characters, lightTime);
                    states.Push(state);
                }
            }
            if ((lightTime + Data.unrewindableTime) < 0.5f && (!Data.rewinding || !currentStage.timeRewindable || lightTime <= 0))
            {
                Color c = fade.color;
                c.a = Mathf.Min(1, c.a + (Time.deltaTime * 2));
                fade.color = c;
            }
            else if ((lightTime + Data.unrewindableTime) > 0)
            {
                Color c = fade.color;
                c.a = Mathf.Max(0, c.a - (Time.deltaTime * 5));
                fade.color = c;
            }
            Vector2 defaultRes = Data.defaultResolution;
            float heightRatio = Data.defaultResolution.x * Screen.height / Data.defaultResolution.y / Screen.width;
            timeDisplay.transform.localPosition = new Vector3(0, defaultRes.y * heightRatio / 2, 0);
            timeDisplay.text = "Remaining Light: " + (Mathf.Round((lightTime + Data.unrewindableTime) * 100) / 100f);
        }
        else
        {
            Color c = fade.color;
            c.a = 0;
            fade.color = c;
            if(gameState == Data.GameState.win)
            {
                lightTime -= Time.deltaTime;
                if(lightTime < 0)
                {
                    gameState = Data.GameState.menu;
                    ClearLevel();
                    win.SetActive(false);
                    menu.SetActive(true);
                }
            }
            else if(gameState == Data.GameState.lose)
            {
                lightTime -= Time.deltaTime;
                if(lightTime < 0)
                {
                    gameState = Data.GameState.level;
                    lose.SetActive(false);
                    ResetStage();
                }
            }
        }
    }

    private void Rewind()
    {
        if (states.Count > 0)
        {
            GameState state = states.Pop();
            CharacterStateData[] cStates = state.states;
            if (currentStage.timeRewindable)
            {
                lightTime = state.lightTime;
            }
            for (int i = 0; i < cStates.Length; ++i)
            {
                cStates[i].ResetCharacter();
            }
        }
    }

    public void ResetStage()
    {
        /*while(states.Count > 1)
        {
            states.Pop();
        }
        Color c = fade.color;
        c.a = 0;
        fade.color = c;
        Rewind();*/
        SetupStage(currentStage);
    }

    public void ClearLevel()
    {
        for (int i = 0; i < levelParent.transform.childCount; ++i)
        {
            GameObject.Destroy(levelParent.transform.GetChild(i).gameObject);
        }
    }

    public void SetupStage(StageData data)
    {
        ClearLevel();
        currentStage = data;
        Data.unrewindableTime = 0;
        lightTime = data.startTime;
        states = new Stack<GameState>();
        GameObject stage = Instantiate(data.stagePointer, levelParent.transform);
        List<GameObject> c = new List<GameObject>();
        for (int i = 0; i < stage.transform.childCount; ++i)
        {
            WorldObject obj = stage.transform.GetChild(i).gameObject.GetComponent<WorldObject>();
            if (obj != null && obj.rewindable)
            {
                c.Add(stage.transform.GetChild(i).gameObject);
            }
        }
        characters = new GameObject[c.Count];
        for (int i = 0; i < characters.Length; ++i)
        {
            characters[i] = c[i];
        }
        float zPosition = Mathf.Max(data.size.y / 2 / Mathf.Tan(gameCamera.fieldOfView / 2 * Mathf.Deg2Rad)
            , data.size.x / 2 / Mathf.Tan(Camera.VerticalToHorizontalFieldOfView(gameCamera.fieldOfView, gameCamera.aspect) / 2 * Mathf.Deg2Rad));
        gameCamera.transform.position = new Vector3(data.size.x / 2, data.size.y / 2, -1 * zPosition);
    }

    public void SelectLevel(StageData stage)
    {
        currentStage = stage;
        SetupStage(stage);
        gameState = Data.GameState.level;
        menu.SetActive(false);
    }

    public void Win()
    {
        gameState = Data.GameState.win;
        lightTime = 1;
        win.SetActive(true);
    }

    public void Lose()
    {
        gameState = Data.GameState.lose;
        lightTime = 1;
        lose.SetActive(true);
    }
}
