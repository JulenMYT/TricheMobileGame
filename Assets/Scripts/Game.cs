using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private ScreenFader screenFader;

    [SerializeField] private SetupPlayer setupPlayer;
    [SerializeField] private WordReveal wordReveal;
    [SerializeField] private StartGame startGame;
    [SerializeField] private Options options;

    [SerializeField] private int oddsSecondFake;
    [SerializeField] private int oddsAllFake;

    private List<string> players = new();
    private List<int> fakePlayerIndices = new();

    private int currentPlayerIndex;

    private string category;
    private string word;
    private string fakeWord;

    private MenuManager menuManager;

    private void Start()
    {
        menuManager = ServiceLocator.Get<MenuManager>();
    }

    public void StartGame()
    {
        if (!setupPlayer.IsValid())
            return;

        ResetGame();

        StartCoroutine(StartGameSequence());
    }

    private void ResetGame()
    {
        players.Clear();
        fakePlayerIndices.Clear();

        currentPlayerIndex = 0;

        category = null;
        word = null;
        fakeWord = null;
    }

    private IEnumerator StartGameSequence()
    {
        yield return screenFader.Fade(0f, 1f, 0.5f);

        InitializeGame();

        menuManager.ToggleMenu(wordReveal);

        SetupNextPlayer();

        yield return screenFader.Fade(1f, 0f, 0.5f);
    }

    private void InitializeGame()
    {
        players = setupPlayer.GetPlayers();

        AssignWords();
        AssignFakes();
    }

    private void AssignWords()
    {
        (category, word) = WordsDatabase.GetRandomWord(
            CategorySettings.GetEnabledCategoryNames(),
            options.AllowShuffle()
        );

        fakeWord = null;

        if (!options.IsUndercoverMode())
            return;

        do
        {
            fakeWord = WordsDatabase
                .GetWordCategory(category)
                .GetRandomWord();

        } while (fakeWord == word);
    }

    private void AssignFakes()
    {
        fakePlayerIndices.Clear();

        if (ShouldMakeAllPlayersFake())
        {
            for (int i = 0; i < players.Count; i++)
                fakePlayerIndices.Add(i);

            return;
        }

        AddRandomFake();

        if (ShouldAddSecondFake())
            AddRandomFake();
    }

    private bool ShouldMakeAllPlayersFake()
    {
        return options.AllowAllFake()
            && !options.IsUndercoverMode()
            && Random.Range(0, 100) < oddsAllFake;
    }

    private bool ShouldAddSecondFake()
    {
        return options.AllowSecondFake()
            && !options.IsUndercoverMode()
            && players.Count > 3
            && Random.Range(0, 100) < oddsSecondFake;
    }

    private void AddRandomFake()
    {
        int fakeIndex;

        do
        {
            fakeIndex = Random.Range(0, players.Count);
        }
        while (fakePlayerIndices.Contains(fakeIndex));

        fakePlayerIndices.Add(fakeIndex);
    }

    public void RevealNextPlayer()
    {
        StartCoroutine(RevealNextPlayerTransition());
    }

    private IEnumerator RevealNextPlayerTransition()
    {
        yield return screenFader.Fade(0f, 1f, 0.5f);

        if (currentPlayerIndex >= players.Count)
        {
            ShowStartScreen();
        }
        else
        {
            SetupNextPlayer();
        }

        yield return screenFader.Fade(1f, 0f, 0.5f);
    }

    private void SetupNextPlayer()
    {
        bool isFake = fakePlayerIndices.Contains(currentPlayerIndex);

        wordReveal.Setup(
            category,
            GetPlayerWord(isFake),
            players[currentPlayerIndex],
            isFake && !options.IsUndercoverMode()
        );

        currentPlayerIndex++;
    }

    private string GetPlayerWord(bool isFake)
    {
        if (!isFake)
            return word;

        if (options.IsUndercoverMode())
            return fakeWord;

        return "IMPOSTEUR";
    }

    private void ShowStartScreen()
    {
        int startingPlayerIndex = SelectStartingPlayer();

        List<string> fakePlayers = fakePlayerIndices
            .Select(index => players[index])
            .ToList();

        startGame.Setup(
            players[startingPlayerIndex],
            fakePlayers,
            word
        );

        menuManager.ToggleMenu(startGame);
    }

    private int SelectStartingPlayer()
    {
        if (options.AllowFakeStart())
            return Random.Range(0, players.Count);

        List<int> validIndices = Enumerable
            .Range(0, players.Count)
            .Where(index => !fakePlayerIndices.Contains(index))
            .ToList();

        return validIndices.Count > 0
            ? validIndices[Random.Range(0, validIndices.Count)]
            : Random.Range(0, players.Count);
    }

    public void EndGame()
    {
        menuManager.ToggleMenu(setupPlayer);
    }
}