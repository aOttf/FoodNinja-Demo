using UnityEngine;
using UnityEngine.UI;

public class DifficultyButton : MonoBehaviour
{
    public enum Difficulty { EASY = 1, MEDIUM, HARD }; //The difficulty option set by player clicking difficulty buttons; will infect spawn rates

    private Button button;
    public GameManager gameManager;

    public Difficulty difficulty;  //easy, medium, hard

    // Start is called before the first frame update
    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SetDifficulty);
    }

    private void SetDifficulty()
    {
        gameManager.StartGame(difficulty);
    }
}