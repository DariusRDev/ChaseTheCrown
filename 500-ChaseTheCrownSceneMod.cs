using System.Collections.Generic;
using UniInject;
using UnityEngine;
using UnityEngine.UIElements;

// Mod interface to do something when a scene is loaded.
// Available scenes are found in the EScene enum.
public class ChaseTheCrownMod : IGameRoundMod
{

    [Inject]
    private ModObjectContext modContext;

    [Inject]
    private UiManager uiManager;

    [Inject]
    private ChaseTheCrownModSettings modSettings;

    public string DisplayName => "Chase The Crown";

    public double DisplayOrder => 2;


    private ChaseTheCrownGameRoundModifierControl control;

    public GameRoundModifierControl CreateControl()
    {
        Debug.Log("ChaseTheCrownMod: CreateControl");
        control = GameObjectUtils.CreateGameObjectWithComponent<ChaseTheCrownGameRoundModifierControl>();
        control.modContext = modContext;
        control.modSettings = modSettings;

        return control;

    }


    public VisualElement CreateConfigurationVisualElement()
    {
        StyleColor grey = new StyleColor();
        grey.value = new Color(0.5f, 0.5f, 0.5f, 1f);
        List<IModSettingControl> modSettingControls = modSettings.GetModSettingControls();
        VisualElement visualElement = new VisualElement();


        Button button = new Button(() =>
        {
            uiManager.CreateInfoDialogControl(Translation.Of("How it works"), Translation.Of("The Player who hits " + modSettings.getCrownOnThisManyPerfectNotes + " perfect notes in a row gets the crown. \nThe crown gives a " + modSettings.multiplier + "x score multiplier. The multiplier will be increased, every Phrase the Player owns the crown, by 0.1x until the Max Multiplier (" + modSettings.maxMultiplier + "x) is reached. Short Notes will be ignored, because they can be tricky to hit. All these values can be adjusted."));
        });
        button.text = "How it works";

        visualElement.Add(button);
        var itemDivider = new VisualElement();
        itemDivider.style.height = 1;
        itemDivider.style.backgroundColor = grey; //grey
        visualElement.Add(itemDivider);

        foreach (IModSettingControl modSettingControl in modSettingControls)
        {
            visualElement.Add(modSettingControl.CreateVisualElement());

        }
        var itemDivider2 = new VisualElement();
        itemDivider2.style.height = 1;
        itemDivider2.style.backgroundColor = grey; //grey
        visualElement.Add(itemDivider2);
        return visualElement;

    }


}
