using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static DifficultyButton;

internal enum SUBGAP { SHORT = 1, MEDIUM, LONG }

public class GameManager : MonoBehaviour
{
    public List<GameObject> targets;
    private AudioSource shootFoodData;

    [Header("===== UI Components ======")]
    public TextMeshProUGUI scoreText;

    public TextMeshProUGUI liveText;
    public TextMeshProUGUI gameoverText;
    public TextMeshProUGUI timeText;
    public Button restartButton;

    [Header("===== Game Scenes =====")]
    public GameObject titleScreen;  //Start UI

    public GameObject activeGameScreen;
    public GameObject pauseScreen;

    //Variables

    [Header("===== Spawning Related Variables =====")]
    [Space(5)]
    [SerializeField] private float gapBetweenWaves = 1.0f;

    private float gapBetweenSubWaves;

    [SerializeField] private int maxItemsPerWave = 17;
    [SerializeField] private int maxItemsPerSubWave = 3;

    [SerializeField] private float shortSubGap = 0.2f;
    [SerializeField] private float mediumSubGap = 0.5f;
    [SerializeField] private float longSubGap = 1.0f;

    [SerializeField] private float gravity = -9.8f;
    private int score = 0;
    private int live = 5;
    private float timeLeft = 90;
    private bool isGameActive = true;
    public bool IsGameOver => !isGameActive;
    private bool isPaused = false;

    private void Start()
    {
        shootFoodData = GetComponent<AudioSource>();

        Physics.gravity = new Vector3(0, gravity, 0);
    }

    private void Update()
    {
        if (!IsGameOver && activeGameScreen.activeSelf)
            UpdateTime();

        if (timeLeft <= 0 || live <= 0)
            GameOver();

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    //---------------------------------- Spawn Related Functions -----------------------------------//

    private IEnumerator SpawnWaves()
    {
        while (!IsGameOver)
        {
            //Index for which item to spawn
            int indexItem;

            //Get the number of items this wave should spawn
            int numThisWave = Random.Range(5, maxItemsPerWave + 1);
            //print("This Wave has " + numThisWave + " Items");

            //Generate the number of items for each subwave in the list
            var subWaves = GenerateSubWaves(numThisWave);

            foreach (int wave in subWaves)
            {
                int curNum = wave;

                //If encounters a single mode, then spawn the remaining in single mode
                if (curNum == 1)
                    break;

                //get subgap for this non-single-spawn subwave
                gapBetweenSubWaves = GenerateSubGap(wave);
                //print("This SubWave has " + curNum + " Items with " + gapBetweenSubWaves + " gap");
                //Spawn
                while (curNum-- > 0)
                {
                    //Pick an item to spawn
                    indexItem = Random.Range(0, targets.Count);

                    //Spawn
                    Instantiate(targets[indexItem], transform.position, Quaternion.identity);

                    //Reduce from total number
                    numThisWave--;
                }

                shootFoodData.Play();
                //Wait for gaps between SubWaves
                yield return new WaitForSeconds(gapBetweenSubWaves);
            }

            //Get sub gap time for single-spawn mode
            gapBetweenSubWaves = GenerateSubGap(1);
            //print("This is a single spawn subwave with " + gapBetweenSubWaves + " gap");
            //Spawn the remaining items in Single-Spawn Mode
            while (numThisWave-- > 0)
            {
                //Pick an item to spawn
                indexItem = Random.Range(0, targets.Count);

                //Spawn
                Instantiate(targets[indexItem], transform.position, Quaternion.identity);

                shootFoodData.Play();
                //Wait for gaps between SubWaves
                yield return new WaitForSeconds(gapBetweenSubWaves);
            }

            //Wait for gaps between Waves
            yield return new WaitForSeconds(gapBetweenWaves);
        }
    }

    private List<int> GenerateSubWaves(int NumThisWave)
    {
        var res = new List<int>();
        var prevNum = NumThisWave;
        int curNum;
        do
        {
            res.Add(curNum = Mathf.Min(NumThisWave, maxItemsPerSubWave, Random.Range(1, prevNum + 1)));
            NumThisWave -= curNum;
            prevNum = curNum;
        }
        while (!(curNum == 1 || NumThisWave == 0));
        return res;
    }

    private float GenerateSubGap(int NumThisSubWave)
    {
        int rand = Random.Range((int)SUBGAP.SHORT, (int)SUBGAP.MEDIUM + 1);
        return NumThisSubWave switch
        {
            1 => ConvertSubGap2Float((SUBGAP)rand),
            _ => ConvertSubGap2Float((SUBGAP)(rand + 1)),
        };
    }

    private float ConvertSubGap2Float(SUBGAP Gap)
    {
        return Gap switch
        {
            SUBGAP.SHORT => shortSubGap,
            SUBGAP.MEDIUM => mediumSubGap,
            SUBGAP.LONG => longSubGap,
            _ => throw new System.NotImplementedException()
        };
    }

    //-------------------------------------------------------------------------------//

    //---------------------------UI Related Functions -----------------------------//
    private void UpdateTime()
    {
        timeLeft -= Time.deltaTime;
        timeText.text = "Time: " + Mathf.RoundToInt(timeLeft);
    }

    private void GameOver()
    {
        isGameActive = false;
        gameoverText.gameObject.SetActive(true);
        Invoke(nameof(ShowRestartButton), 2f);
    }

    private void ShowRestartButton()
    {
        restartButton.gameObject.SetActive(true);
    }

    private void SetUpActiveGameScreen()
    {
        activeGameScreen.SetActive(true);
        UpdateScore(0);
        UpdateLive(0);
    }

    //------------------------------------------------------------------------------//

    //--------------------------- Message Functions --------------------------------//

    //===== Messages Called by Button Manager =====//
    public void StartGame(Difficulty difficulty)
    {
        //Close the title screen
        titleScreen.SetActive(false);
        isGameActive = true;
        //Show in game screen
        SetUpActiveGameScreen();

        //Set spawn rate
        gapBetweenWaves /= ((int)difficulty);

        StartCoroutine(SpawnWaves());
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ChangePauseStatus()
    {
        if (!isPaused)
        {
            pauseScreen.SetActive(true);
            Time.timeScale = 0;
            isPaused = true;
        }
        else
        {
            pauseScreen.SetActive(false);
            Time.timeScale = 1;
            isPaused = false;
        }
    }

    //=================================================//

    //===== Messages Clalled by Swiper =====//
    public void UpdateScore(int scoreAdd)
    {
        scoreText.text = "Score: " + (score += scoreAdd);
    }

    public void UpdateLive(int liveAdd)
    {
        liveText.text = "Live:" + (live += liveAdd);
    }

    public void BUlletTime4Bomb()
    {
        StartCoroutine("_bulletTime4Bomb");
    }

    //=================================================//

    //---------------------------------------------------------------------//

    private IEnumerator _bulletTime4Bomb()
    {
        Time.timeScale = 0.5f;
        // print("0.5f");
        yield return new WaitForSecondsRealtime(1.5f);
        //print("I have recovered!");
        Time.timeScale = 1;
        yield return null;
    }
}