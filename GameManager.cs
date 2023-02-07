using System.Collections;
using UnityEngine;

/// <summary>
/// Manages game logic and controls the UI
/// </summary>
public class GameManager : MonoBehaviour
{
    [Tooltip("Game ends when an agent extinguishes this much nectar")]
    public float winningFires = 10f;

    [Tooltip("Game ends after this many seconds have elapsed")]
    public float gameTimer = 70f;

    [Tooltip("The UI Controller")]
    public UIController uiController;

    [Tooltip("The humman player firefighter")]
    public FirefighterAgent player;

    [Tooltip("The opponent Agent firefighter")]
    public FirefighterAgent opponent;

    [Tooltip("The fire area")]
    public FireArea fireArea;

    [Tooltip("The main camera for the scene")]
    public Camera mainCamera;

    // When the game timer started
    private float TimerStartTime;

    /// <summary>
    /// All possible game states
    /// </summary>
    public enum GameState
    {
        Default,
        MainMenu,
        Preparing,
        Playing,
        Paused,
        Gameover
    }

    /// <summary>
    /// The current game state
    /// </summary>
    public GameState State { get; private set; } = GameState.Default;

    /// <summary>
    /// Gets the time remaining in the game
    /// </summary>
    public float TimeRemaining
    {
        get
        {
            if (State == GameState.Playing)
            {
                float timeRemaining = gameTimer - (Time.time - TimerStartTime);
                return Mathf.Max(0f, timeRemaining);
            }
            else
            {
                return 0f;
            }
        }
    }

    /// <summary>
    /// Exits the game
    /// </summary>
   //public void ExitGame()
   //{
   //    Application.Quit();
   //}

    //void PauseGame()
    //{
    //    Time.timeScale = 0;
    //    State = GameState.Paused;
    //    UIController.instance.ShowPauseMenu();
    //}
    //
    //void UnpauseGame()
    //{
    //    Time.timeScale = 1;
    //    State = GameState.Playing;
    //    UIController.instance.HidePauseMenu();
    //}

    /// <summary>
    /// Handles a button click in different states
    /// </summary>
    public void ButtonClicked()
    {
        if (State == GameState.Gameover)
        {
            // In the Gameover state, button click should go to the main menu
            MainMenu();
        }
        else if (State == GameState.MainMenu)
        {
            // In the MainMenu state, button click should start the game
            StartCoroutine(StartGame());
        }
        else if (State == GameState.Playing && Time.timeScale == 0)
        {
            // Unpause the game
            Time.timeScale = 1;
            uiController.HideButton();
        }
        else
        {
            Debug.LogWarning("Button clicked in unexpected state: " + State.ToString());
        }
    }

    /// <summary>
    /// Called when the game starts
    /// </summary>
    private void Start()
    {
        // Subscribe to button click events from the UI
        uiController.OnButtonClicked += ButtonClicked;

        // Start the main menu
        MainMenu();
    }

    /// <summary>
    /// Called on destroy
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe from button click events from the UI
        uiController.OnButtonClicked -= ButtonClicked;
    }

    /// <summary>
    /// Shows the main menu
    /// </summary>
    private void MainMenu()
    {
        // Set the state to "main menu"
        State = GameState.MainMenu;

        // Update the UI
        uiController.ShowBanner("");
        uiController.ShowButton("Start");

        // Use the main camera, disable agent cameras
        mainCamera.gameObject.SetActive(true);
        player.agentCamera.gameObject.SetActive(false);
        opponent.agentCamera.gameObject.SetActive(false); // Never turn this back on

        // Reset all fires
        fireArea.ResetFires();

        // Reset the agents
        player.OnEpisodeBegin();
        opponent.OnEpisodeBegin();

        // Freeze the agents
        player.FreezeAgent();
        opponent.FreezeAgent();
    }

    /// <summary>
    /// Starts the game with a countdown
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator StartGame()
    {
        // Set the state to "preparing"
        State = GameState.Preparing;

        // Update the UI (hide it)
        uiController.ShowBanner("");
        uiController.HideButton();

        // Use the player camera, disable the main camera
        mainCamera.gameObject.SetActive(false);
        player.agentCamera.gameObject.SetActive(true);

        // Show countdown
        uiController.ShowBanner("3");
        yield return new WaitForSeconds(1f);
        uiController.ShowBanner("2");
        yield return new WaitForSeconds(1f);
        uiController.ShowBanner("1");
        yield return new WaitForSeconds(1f);
        uiController.ShowBanner("Go!");
        yield return new WaitForSeconds(1f);
        uiController.ShowBanner("");

        // Set the state to "playing"
        State = GameState.Playing;

        // Start the game timer
        TimerStartTime = Time.time;

        // Unfreeze the agents
        player.UnfreezeAgent();
        opponent.UnfreezeAgent();
    }

    /// <summary>
    /// Ends the game
    /// </summary>
    private void EndGame()
    {
        // Set the game state to "game over"
        State = GameState.Gameover;

        // Freeze the agents
        player.FreezeAgent();
        opponent.FreezeAgent();

        // Update banner text depending on win/lose
        if (player.FiresExtinguished <= opponent.FiresExtinguished)
        {
            uiController.ShowBanner("Agent wins!");
        }
        else
        {
            uiController.ShowBanner("You win!");
        }

        // Update button text
        uiController.ShowButton("Main Menu");
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    private void Update()
    {
        //pause and unpauses the game when the esc key is pressed
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    if (State == GameState.Playing)
        //    {
        //        PauseGame();
        //    }
        //    else if (State == GameState.Paused)
        //    {
        //        UnpauseGame();
        //    }
        //}

        if (State == GameState.Playing)
        {
            // Check to see if time has run out or either agent or player extinguished first 10 fires
            if (TimeRemaining <= 0f ||
                player.FiresExtinguished >= winningFires ||
                opponent.FiresExtinguished >= winningFires)
            {
                EndGame();
            }

            // Update the timer and extinguished fires progress bars
            uiController.SetTimer(TimeRemaining);
            uiController.SetPlayerExtinguishes(player.FiresExtinguished / winningFires);
            uiController.SetOpponentExtinguishes(opponent.FiresExtinguished / winningFires);
        }
        else if (State == GameState.Preparing || State == GameState.Gameover)
        {
            // Update the timer
            uiController.SetTimer(TimeRemaining);
        }
        else
        {
            // Hide the timer
            uiController.SetTimer(-1f);

            // Update the progress bars
            uiController.SetPlayerExtinguishes(0f);
            uiController.SetOpponentExtinguishes(0f);
        }

    }
}
