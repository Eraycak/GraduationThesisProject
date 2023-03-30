using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject disabledOverlay;
    [SerializeField] private Button button;

    private CharacterSelectDisplay characterSelectDisplay;

    public Character Character { get; private set; }

    public  bool IsDisabled { get; private set; }

    public void SetCharacter(CharacterSelectDisplay characterSelectDisplay, Character character)
    {
        iconImage.sprite = character.Icon;
        this.characterSelectDisplay = characterSelectDisplay;
        Character = character;
    }

    public void SelectCharacter()
    {
        characterSelectDisplay.Select(Character);
    }

    public void SetDisabled()
    {
        IsDisabled = true;
        disabledOverlay.SetActive(true);
        button.interactable = false;
    }
}
