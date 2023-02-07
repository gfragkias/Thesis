using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls a very simple UI. Doesn't do anything on its own.
/// </summary>
public class UIController : MonoBehaviour
{
    [Tooltip("The extinguished fires bar for the player")]
    public Slider playerFiresBar;

    [Tooltip("The extinguished fires bar for the opponent")]
    public Slider opponentFiresBar;

    [Tooltip("The timer text")]
    public TextMeshProUGUI timerText;

    [Tooltip("The banner text")]
    public TextMeshProUGUI bannerText;

    [Tooltip("The button")]
    public Button button;

    [Tooltip("The button text")]
    public TextMeshProUGUI buttonText;

    [Tooltip("The exit button")]
    public Button exitButton;

    public delegate void ButtonClick();

    public ButtonClick OnButtonClicked;

    public void ButtonClicked()
    {
        if (OnButtonClicked != null)
        {
            OnButtonClicked();

            if (button.gameObject.activeInHierarchy)
            {
                button.gameObject.SetActive(false);
            }
            else if (exitButton.gameObject.activeInHierarchy)
            {
                Application.Quit();
            }
        }
    }

    /// <summary>
    /// Shows the button
    /// </summary>
    /// <param name="text">The text string on the button</param>
    public void ShowButton(string text)
    {
        if (text == "Exit")
        {
            exitButton.gameObject.SetActive(true);
            button.gameObject.SetActive(false);
        }
        else
        {
            buttonText.text = text;
            button.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the button
    /// </summary>
    public void HideButton()
    {
        button.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows banner text
    /// </summary>
    /// <param name="text">The text string to show</param>
    public void ShowBanner(string text)
    {
        bannerText.text = text;
        bannerText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the banner text
    /// </summary>
    public void HideBanner()
    {
        bannerText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets the timer, if timeRemaining is negative, hides the text
    /// </summary>
    /// <param name="timeRemaining">The time remaining in seconds</param>
    public void SetTimer(float timeRemaining)
    {
        if (timeRemaining > 0f)
            timerText.text = timeRemaining.ToString("00");
        else
            timerText.text = "";
    }

    /// <summary>
    /// Sets the player's amount of extinguished fires
    /// </summary>
    /// <param name="FireAmount">An amount between 0 and 1</param>
    public void SetPlayerExtinguishes(float FireAmount)
    {
        playerFiresBar.value = FireAmount;
    }

    /// <summary>
    /// Sets the opponent's amount of extinguished fires
    /// </summary>
    /// <param name="FireAmount">An amount between 0 and 1</param>
    public void SetOpponentExtinguishes(float FireAmount)
    {
        opponentFiresBar.value = FireAmount;
    }
}
