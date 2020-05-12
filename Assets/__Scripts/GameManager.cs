using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public delegate void GenericDelegate();
    static public event GenericDelegate LEVEL_START_EVENT;
    static public event GenericDelegate LEVEL_END_EVENT;

    private static GameManager _S;
    public static GameManager S
    {
        get { return _S; }
        set
        {
            if(_S != null)
            {
                Debug.LogError("Game Manager already exists.");
                return;
            }
            _S = value;
        }
    }

    public enum eGameManagerState
    {
        idle, preLevel, level, postLevel
    }

    [Header("Set in Inspector")]
    [Tooltip("The names of the scenes for various levels.")]
    public List<string> levelNames;

    [Header("Set Dynamically")]
    public string currentLevelName = "";
    [SerializeField]
    protected int currentLevelNum = 0;
    [SerializeField]
    protected eGameManagerState state;

    void Start()
    {
        S = this;
        currentLevelNum = 0;
        state = eGameManagerState.postLevel;
        LevelAdvancePanel.FadeInToEndLevel(LoadLevel);
    }

    void LoadLevel()
    {
        LoadLevel(-1);
    }

    void LoadLevel(int lNum)
    {
        if (lNum == -1)
        {
            lNum = currentLevelNum;
        }
        currentLevelNum = lNum;

        StartCoroutine(LoadSceneAndSetActive(currentLevelName));
    }

    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        if(SceneManager.sceneCount > 1)
        {
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }

        InteractingPlayer.SetPosition(new Vector3(10000, 10000, 10000), Quaternion.identity);

        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        Scene newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        SceneManager.SetActiveScene(newlyLoadedScene);

        state = eGameManagerState.preLevel;
        LevelAdvancePanel.FadeOutToBeginLevel(StartLevel);
    }

    void StartLevel()
    {
        AlertModeManager.SwitchToAlertMode(false);
        GameObject go = GameObject.Find("_PlayerStart");
        if(go != null)
        {
            InteractingPlayer.SetPosition(go.transform.position, go.transform.rotation);
            StealthPlayerCamera.ResetToFarPosition();
        }

        state = eGameManagerState.level;

        if(LEVEL_START_EVENT != null)
        {
            LEVEL_START_EVENT();
        }
    }

    void ReloadLevel()
    {
        state = eGameManagerState.postLevel;
        --currentLevelNum;
        EndLevel();
    }

    void EndLevel()
    {
        state = eGameManagerState.postLevel;
        ++currentLevelNum;
        if(LEVEL_END_EVENT != null)
        {
            LEVEL_END_EVENT();
        }
        LevelAdvancePanel.FadeInToEndLevel(LoadLevel);
    }

    static public void LevelComplete()
    {
        S.EndLevel();
    }

    static public void LevelFailed()
    {
        S.ReloadLevel();
    }

    static public int GAME_LEVEL
    {
        get { return S.currentLevelNum; }
    }

    static public string GAME_LEVEL_NAME
    {
        get { return S.currentLevelName; }
    }

    static public eGameManagerState STATE
    {
        get { return S.state; }
    }
}
