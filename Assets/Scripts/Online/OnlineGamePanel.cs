using System.Collections;
using TMPro;
using UnityEngine;

public class OnlineGamePanel : MenuPanel
{
    [SerializeField] private TMP_Text wordText;
    [SerializeField] private TMP_Text categoryText;
    [SerializeField] private Transform playersContainer;
    [SerializeField] private GameObject playerButtonPrefab;

    [SerializeField] private GameObject endVoteButton;

    // Bouton de l'hôte affiché après la révélation, pour revenir au salon et relancer.
    [SerializeField] private GameObject backToRoomButton;
    [SerializeField] private ErrorPopup errorPopup;

    [SerializeField] private GameObject game;
    [SerializeField] private GameObject reveal;

    [SerializeField] private TMP_Text revealVotedPlayerText;
    [SerializeField] private TMP_Text revealVotedPlayerRoleText;
    [SerializeField] private TMP_Text revealImpostorText;
    [SerializeField] private TMP_Text revealWordText;

    [SerializeField] private CanvasGroup votedPlayerGroup;
    [SerializeField] private CanvasGroup votedPlayerRoleGroup;
    [SerializeField] private CanvasGroup impostorGroup;
    [SerializeField] private CanvasGroup wordGroup;

    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float delayBetweenTexts = 0.5f;

    private bool isHost;

    public void Setup(
        string word,
        string category,
        bool isImpostor)
    {
        wordText.text = word;
        categoryText.text = category;

        game.SetActive(true);
        reveal.SetActive(false);

        SetBackToRoomVisible(false);
    }

    public void AddPlayer(
        ulong playerId,
        string playerName,
        bool isLocalPlayer)
    {
        GameObject playerButton =
            Instantiate(
                playerButtonPrefab,
                playersContainer
            );

        playerButton
            .GetComponent<OnlinePlayerButton>()
            .Setup(
                playerId,
                playerName,
                this,
                isLocalPlayer
            );
    }

    public void RemovePlayer(ulong playerId)
    {
        foreach (Transform child in playersContainer)
        {
            OnlinePlayerButton playerButton = child.GetComponent<OnlinePlayerButton>();

            if (playerButton != null && playerButton.GetPlayerId() == playerId)
                Destroy(child.gameObject);
        }
    }

    public void SelectPlayer(ulong playerId)
    {
        foreach (Transform child in playersContainer)
        {
            OnlinePlayerButton playerButton =
                child.GetComponent<OnlinePlayerButton>();

            if (playerButton == null)
                continue;

            playerButton.SetSelected(
                playerButton.GetPlayerId() == playerId
            );
        }
    }

    public void ShowReveal(
        string votedPlayerName,
        bool votedPlayerIsImpostor,
        string impostorPlayerName,
        string realWord)
    {
        game.SetActive(false);
        reveal.SetActive(true);

        revealVotedPlayerText.text =
            votedPlayerName;

        revealVotedPlayerRoleText.text =
            votedPlayerIsImpostor
                ? "IMPOSTEUR"
                : "INNOCENT";

        revealImpostorText.text =
            impostorPlayerName;

        revealWordText.text =
            realWord;

        StartCoroutine(RevealSequence());
    }

    private IEnumerator RevealSequence()
    {
        SetAlpha(votedPlayerGroup, 0f);
        SetAlpha(votedPlayerRoleGroup, 0f);
        SetAlpha(impostorGroup, 0f);
        SetAlpha(wordGroup, 0f);

        yield return FadeIn(votedPlayerGroup);

        yield return new WaitForSeconds(delayBetweenTexts);

        yield return FadeIn(votedPlayerRoleGroup);

        yield return new WaitForSeconds(delayBetweenTexts);

        yield return FadeIn(impostorGroup);

        yield return new WaitForSeconds(delayBetweenTexts);

        yield return FadeIn(wordGroup);

        // Une fois tout révélé, l'hôte peut ramener tout le monde au salon.
        SetBackToRoomVisible(isHost);
    }

    private IEnumerator FadeIn(CanvasGroup group)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            group.alpha =
                Mathf.Clamp01(elapsed / fadeDuration);

            yield return null;
        }

        group.alpha = 1f;
    }

    private void SetAlpha(
        CanvasGroup group,
        float alpha)
    {
        group.alpha = alpha;
    }

    private void SetBackToRoomVisible(bool visible)
    {
        if (backToRoomButton != null)
            backToRoomButton.SetActive(visible);
    }

    public void ClearPlayers()
    {
        foreach (Transform child in playersContainer)
            Destroy(child.gameObject);
    }

    public override void Close()
    {
        StopAllCoroutines();
        ClearPlayers();

        game.SetActive(false);
        reveal.SetActive(false);

        SetBackToRoomVisible(false);
    }

    public void SetHost(bool isHost)
    {
        this.isHost = isHost;
        endVoteButton.SetActive(isHost);
    }

    public void EndVote()
    {
        string error = ServiceLocator
            .Get<OnlineGame>()
            .EndVote();

        if (error != null && errorPopup != null)
            ErrorPopup.Show(errorPopup, error);
    }

    public void BackToRoom()
    {
        ServiceLocator
            .Get<OnlineGame>()
            .BackToRoom();
    }
}
