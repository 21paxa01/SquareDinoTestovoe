using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button startButton;

    private void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnStartClicked()
    {
        string name = playerNameInput.text;

        if (string.IsNullOrEmpty(name))
        {
            name = "Player_" + Random.Range(1000, 9999);
        }

        PlayerPrefs.SetString("PlayerName", name);

        SceneManager.LoadScene("GameScene");
    }
}