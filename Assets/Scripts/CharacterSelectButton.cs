using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    private CharacterSelectDisplay characterSelectDisplay;

    private Character character;

    public void SetCharacter(CharacterSelectDisplay characterSelectDisplay, Character character)
    {
        iconImage.sprite = character.Icon;
        this.characterSelectDisplay = characterSelectDisplay;
        this.character = character;
    }

    public void SelectCharacter()
    {
        characterSelectDisplay.Select(character);
    }
}
