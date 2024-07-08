using System;
using System.Collections.Generic;
using UniInject;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

class ChaseTheCrownGameRoundModifierControl : GameRoundModifierControl
{

    public ModObjectContext modContext;


    public Dictionary<int, ChaseTheCrownPlayerControl> playerControls = new Dictionary<int, ChaseTheCrownPlayerControl>();
    public Dictionary<int, int> previousScores = new Dictionary<int, int>();


    [Inject]
    private SingSceneControl singSceneControl;


    public ChaseTheCrownModSettings modSettings;

    private Dictionary<int, Label> playerCrownLabels = new Dictionary<int, Label>();

    private int crownOwnerPlayerId = -1;

    List<LTDescr> crownAnimations = new List<LTDescr>();

    private VisualElement crown;

    private double multiplierBonus = 0;


    private void Start()
    {

        foreach (var playerControl in singSceneControl.PlayerControls)
        {
            // initialize the player's Label to display the  multiplier
            Label multiplierLabel = new Label("1X Multiplier");
            multiplierLabel.style.color = new StyleColor(Color.white);
            multiplierLabel.style.position = Position.Absolute;
            VisualElement playerScoreLabel = getPlayerContainer(playerControl, "playerScoreLabel");
            // display above the score label
            multiplierLabel.style.top = playerScoreLabel.worldBound.y - 15;
            multiplierLabel.style.left = playerScoreLabel.worldBound.x;
            multiplierLabel.style.fontSize = 14;
            multiplierLabel.style.unityFontStyleAndWeight = FontStyle.Bold;


            playerCrownLabels[playerControl.GetInstanceID()] = multiplierLabel;
            singSceneControl.background.Add(multiplierLabel);

            Action onCrownCollected = () =>
                {
                    multiplierBonus = 0;
                    Debug.Log("Crown collected by " + playerControl.PlayerProfile.Name);
                    if (crownOwnerPlayerId == -1)
                    {
                        VisualElement itemVE = new VisualElement();
                        itemVE.name = "Crown";
                        ImageManager.LoadSpriteFromUri($"{modContext.ModFolder}/crown.png")
                            .Subscribe(sprite => itemVE.style.backgroundImage = new StyleBackground(sprite));
                        itemVE.style.width = 50;
                        itemVE.style.height = 50;
                        itemVE.style.position = Position.Absolute;
                        singSceneControl.background.Add(itemVE);
                        crown = itemVE;
                    }
                    foreach (var label in playerCrownLabels)
                    {
                        // color all  labels white
                        if (label.Key == playerControl.GetInstanceID())
                        {
                            label.Value.style.color = new StyleColor(Color.green);
                            label.Value.text = $"{modSettings.multiplier + multiplierBonus}x Multiplier";
                            label.Value.style.fontSize = 15;

                        }
                        else
                        {
                            label.Value.style.color = new StyleColor(Color.white);
                            label.Value.text = "1x Multiplier";
                            label.Value.style.fontSize = 14;
                        }
                    }
                    if (crownOwnerPlayerId != -1)
                    {
                        playerControls[crownOwnerPlayerId].IsCrownOwner = false;
                    }

                    AnimateCrown(playerControl);
                    crownOwnerPlayerId = playerControl.GetInstanceID();
                };

            ChaseTheCrownPlayerControl ChaseTheCrownPlayerControl = new ChaseTheCrownPlayerControl();
            ChaseTheCrownPlayerControl.modSettings = modSettings;
            ChaseTheCrownPlayerControl.onCrownCollected = onCrownCollected;
            ChaseTheCrownPlayerControl.IsCrownOwner = false;
            playerControl.PlayerUiControl.Injector.Inject(ChaseTheCrownPlayerControl);

            playerControls[playerControl.GetInstanceID()] = ChaseTheCrownPlayerControl;
            previousScores[playerControl.GetInstanceID()] = 0;

            playerControl.PlayerScoreControl.ScoreChangedEventStream.Subscribe(score =>
            {
                if (crownOwnerPlayerId == playerControl.GetInstanceID())
                {
                    int newNoteScore = playerControl.PlayerScoreControl.CalculationData.NormalNotesTotalScore;
                    int newSentenceScore = playerControl.PlayerScoreControl.CalculationData.PerfectSentenceBonusTotalScore;
                    int newGoldenNoteScore = playerControl.PlayerScoreControl.CalculationData.GoldenNotesTotalScore;

                    int newTotalScore = newNoteScore + newSentenceScore + newGoldenNoteScore;

                    int previousScore = previousScores[playerControl.GetInstanceID()];
                    int scoreDiff = newTotalScore - previousScore;

                    if (scoreDiff > 0)
                    {
                        int newModTotalScore = (int)(playerControl.PlayerScoreControl.CalculationData.ModTotalScore + scoreDiff * (modSettings.multiplier + multiplierBonus) - scoreDiff);
                        previousScores[playerControl.GetInstanceID()] = newTotalScore;

                        playerControl.PlayerScoreControl.SetModTotalScore(newModTotalScore, true);
                        ShowItemRating(playerControl, $"+{Math.Round(scoreDiff * (modSettings.multiplier + multiplierBonus) - scoreDiff)} 👑");
                    }

                    previousScores[playerControl.GetInstanceID()] = newTotalScore;


                    if (modSettings.maxMultiplier > modSettings.multiplier + multiplierBonus)
                    {
                        multiplierBonus += 0.05;
                    }
                    if (crownOwnerPlayerId == playerControl.GetInstanceID())
                    {
                        playerCrownLabels[crownOwnerPlayerId].text = $"{modSettings.multiplier + multiplierBonus}x Multiplier";

                    }
                }
            });
        }
    }




    public VisualElement ShowItemRating(PlayerControl targetPlayerControll, string text)
    {
        VisualElement label = new Label(text);

        label.style.position = Position.Absolute;

        label.style.backgroundColor = new StyleColor(Color.green);
        label.style.color = new StyleColor(Color.black);
        label.style.marginLeft = 19;
        label.style.paddingBottom = 5;
        label.style.paddingTop = 5;
        label.style.paddingLeft = 5;
        label.style.paddingRight = 5;
        label.style.borderTopLeftRadius = 8;
        label.style.borderTopRightRadius = 8;
        label.style.borderBottomLeftRadius = 8;
        label.style.borderBottomRightRadius = 8;
        VisualElement targetPlayerScoreLabel = getPlayerScoreLabel(targetPlayerControll);
        Vector2 positionOfScore = targetPlayerScoreLabel.worldBound.position;
        // Move the visual element to the background so that it is not affected by Parents layouting.

        singSceneControl.background.Add(label);
        Vector2 parentPosition = singSceneControl.background.worldBound.position;
        positionOfScore -= parentPosition;
        label.style.left = positionOfScore.x + 20;
        label.style.top = positionOfScore.y;


        float fromValue = 0;
        float untilValue = 20;

        LeanTween.value(singSceneControl.gameObject, fromValue, untilValue, 1f)
            .setEaseInSine()
            .setOnUpdate(interpolatedValue =>
            {
                // Set Position so it goes upwards
                label.style.top = positionOfScore.y - interpolatedValue;

            })
            .setOnComplete(label.RemoveFromHierarchy);
        return label;
    }

    private VisualElement getPlayerScoreLabel(PlayerControl targetPlayerControl)
    {
        String playerName = targetPlayerControl.PlayerProfile.Name;

        List<VisualElement> playerInfoContainer = singSceneControl.background.Query<VisualElement>().Where(ve => ve.name == "playerNameContainer").ToList();
        foreach (VisualElement ve in playerInfoContainer)
        {
            Label playerNameLabel = ve.Query<Label>().Where(label => label.name == "playerNameLabel").First();

            if (playerNameLabel.text == playerName)
            {
                return ve.Query<VisualElement>().Where(ves => ves.name == "playerScoreLabel").First();
            }
        }
        return null;

    }

    private void AnimateCrown(PlayerControl playerControl)
    {
        foreach (LTDescr lTDescr in crownAnimations)
        {
            LeanTween.cancel(lTDescr.id);
        }
        crownAnimations.Clear();

        float x = crown.style.left.value.value;
        float y = crown.style.top.value.value;

        Vector2 endPosition = getPlayerContainer(playerControl, "playerImage").worldBound.position;



        // Start the tween
        LTDescr lTDescr1 = LeanTween.value(singSceneControl.gameObject, x, endPosition.x, 0.6f).setOnUpdate((float val) =>
        {
            crown.style.left = val + 23;

        }).setEaseInSine().setOnComplete(() =>
        {
            crown.style.left = endPosition.x + 23;
        });

        LTDescr lTDescr2 = LeanTween.value(singSceneControl.gameObject,
        y, endPosition.y, 0.6f).setOnUpdate((float val) =>
        {
            crown.style.top = val + 23;
        }).setEaseInSine().setOnComplete(() =>
        {
            crown.style.top = endPosition.y + 23;
        });

        crownAnimations.Add(lTDescr1);
        crownAnimations.Add(lTDescr2);
    }

    private VisualElement getPlayerContainer(PlayerControl targetPlayerControl, string elementName)
    {
        string playerName = targetPlayerControl.PlayerProfile.Name;

        List<VisualElement> playerInfoContainer = singSceneControl.background.Query<VisualElement>().Where(ve => ve.name == "playerInfoContainer").ToList();
        foreach (VisualElement ve in playerInfoContainer)
        {
            Label playerNameLabel = ve.Query<Label>().Where(label => label.name == "playerNameLabel").First();

            if (playerNameLabel.text == playerName)
            {

                return ve.Query<VisualElement>().Where(ves => ves.name == elementName).First();
            }
        }
        Debug.LogError("Could not find player profile pic container");
        return null;
    }

    private void Update()
    {

    }


}
