using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls a very simple UI. Doesn't do anything on its own.
/// </summary>
public class UIController : MonoBehaviour
{
    public Image myImage;

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

    [Tooltip("The exit button text")]
    public TextMeshProUGUI exitButtonText;

    public delegate void ButtonClick();

    public ButtonClick OnButtonClicked;

    public delegate void ExitButtonClick();

    public ExitButtonClick OnExitButtonClicked;

    public void ButtonClicked()
    {
        if (OnButtonClicked != null) OnButtonClicked();
    }

    /// <summary>
    /// Shows the button
    /// </summary>
    /// <param name="text">The text string on the button</param>
    public void ShowButton(string text)
    {
        buttonText.text = text;
        button.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the button
    /// </summary>
    public void HideButton()
    {
        button.gameObject.SetActive(false);
    }

    //The same for the exit button
    public void ShowExitButton(string text)
    {
        exitButtonText.text = text;
        exitButton.gameObject.SetActive(true);
    }

    public void HideExitButton()
    {
        exitButton.gameObject.SetActive(false);
    }

    public void ExitButtonClicked()
    {
        if (OnExitButtonClicked != null) OnExitButtonClicked();
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
    /// <param name="fireAmount">An amount between 0 and 1</param>
    public void SetPlayerExtinguishes(float fireAmount)
    {
        playerFiresBar.value = fireAmount;
    }

    /// <summary>
    /// Sets the opponent's amount of extinguished fires
    /// </summary>
    /// <param name="fireAmount">An amount between 0 and 1</param>
    public void SetOpponentExtinguishes(float fireAmount)
    {
        opponentFiresBar.value = fireAmount;
    }
}
