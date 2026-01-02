using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private SetupPlayer setupPlayer;
    [SerializeField] private WordReveal wordReveal;
    [SerializeField] private StartGame startGame;
    [SerializeField] private Options options;
    [SerializeField] private Transition transition;

    [SerializeField] private int oddsSecondFake;
    [SerializeField] private int oddsAllFake;

    private List<string> players;
    private List<int> fakeIndices;
    private int currentIndex;

    private string category;
    private string word;

    private void Awake()
    {
        players = new List<string>();
        setupPlayer.OnPlayClicked += Play;
        setupPlayer.OnOptionsClicked += OpenOptions;
        wordReveal.OnNextButtonClicked += OnNext;
        startGame.OnNextButtonClicked += OnEnd;
        options.OnValidateButton += CloseOptions;
    }

    private void Start()
    {
        setupPlayer.Show();
        wordReveal.Hide();
        startGame.Hide();
        options.Hide();
        transition.Hide();
    }

    private void Play()
    {
        setupPlayer.ForceValidate();
        setupPlayer.DeleteEmptyBoxes();

        if (setupPlayer.playerBoxes.Count < 2) return;

        players.Clear();
        foreach (var box in setupPlayer.playerBoxes)
            players.Add(box.GetName());

        DetermineFakes();

        (category, word) = WordsDatabase.GetRandomWord(CategorySettings.GetEnabledCategoryNames(), options.AllowShuffle());

        transition.FadeInOut();

        void Callback()
        {
            setupPlayer.Hide();
            wordReveal.Show();
            transition.OnFadeInComplete -= Callback;
        }

        transition.OnFadeInComplete += Callback;

        currentIndex = 0;
        NextPlayer();
    }

    private void DetermineFakes()
    {
        fakeIndices = new List<int>();

        if (options.AllowAllFake() && Random.Range(0, 100) < oddsAllFake)
        {
            for (int i = 0; i < players.Count; i++)
                fakeIndices.Add(i);
            return;
        }

        int firstFake = DrawRandomPlayer();
        fakeIndices.Add(firstFake);

        if (options.AllowSecondFake() && players.Count > 2)
        {
            if (Random.Range(0, 100) < oddsSecondFake)
            {
                int secondFake;
                do
                {
                    secondFake = DrawRandomPlayer();
                } while (fakeIndices.Contains(secondFake));

                fakeIndices.Add(secondFake);
            }
        }
    }

    private int DrawRandomPlayer()
    {
        if (players.Count == 0)
            throw new System.InvalidOperationException("No players available for random draw.");

        return Random.Range(0, players.Count);
    }

    private int DrawBeginPlayer()
    {
        if (players.Count == 0)
            throw new System.InvalidOperationException("No players available to start.");

        if (options.AllowFakeStart())
            return Random.Range(0, players.Count);

        var nonFakeIndices = Enumerable.Range(0, players.Count)
            .Where(i => !fakeIndices.Contains(i))
            .ToList();

        return nonFakeIndices.Count > 0 ? nonFakeIndices[Random.Range(0, nonFakeIndices.Count)] : Random.Range(0, players.Count);
    }

    private void OnNext()
    {
        if (currentIndex < players.Count)
            NextPlayer();
        else
            StartGame();
    }

    private void OnEnd()
    {
        transition.FadeInOut();

        void Callback()
        {
            setupPlayer.Show();
            startGame.Hide();
            transition.OnFadeInComplete -= Callback;
        }

        transition.OnFadeInComplete += Callback;
    }

    private void NextPlayer()
    {
        if (currentIndex >= players.Count) return;

        bool isFake = fakeIndices.Contains(currentIndex);
        string displayedWord = isFake ? "IMPOSTEUR" : word;
        string playerName = players[currentIndex];
        currentIndex++;

        transition.FadeInOut();

        void Callback()
        {
            wordReveal.Setup(category, displayedWord, playerName, isFake);
            transition.OnFadeInComplete -= Callback;
        }

        transition.OnFadeInComplete += Callback;
    }

    private void StartGame()
    {
        int beginIndex = DrawBeginPlayer();
        List<string> fakes = fakeIndices.Select(i => players[i]).ToList();
        startGame.Setup(players[beginIndex], fakes);

        transition.FadeInOut();

        void Callback()
        {
            wordReveal.Hide();
            startGame.Show();

            transition.OnFadeInComplete -= Callback;
        }

        transition.OnFadeInComplete += Callback;
    }

    private void OpenOptions()
    {
        options.Show();
        setupPlayer.Hide();
    }

    private void CloseOptions()
    {
        options.Hide();
        setupPlayer.Show();
    }
}
