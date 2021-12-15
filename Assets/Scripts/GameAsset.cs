using UnityEngine;
using Utils;

public class GameAsset : MonoBehaviour
{
    public static GameAsset instance { get; private set; }

    [Header("Layer")] 
    public SingleUnityLayer groundLayer;
    public SingleUnityLayer outlineLayerOn;
    public SingleUnityLayer outlineLayerOff;
    public SingleUnityLayer uiLayer;
    
    [Header("Cursor")]
    public Texture2D cursorDefault;
    public Texture2D cursorAttack;
    public Texture2D cursorTalk;
    
    [Header("Color")]
    [Header("Outline Targets")]
    public Color outlineEnemy = Color.red;
    public Color outlineNeutral = Color.white;
    public Color outlineInteractable = Color.yellow;
    
    [Header("Attributes")]
    public Color health = Color.red;
    public Color mana = Color.blue;
    public Color magicShield = Color.cyan;
    
    [Header("Hit")]
    public Color criticalHit = Color.yellow;
    public Color missHit = Color.gray;
    public Color blockHit = Color.white;
    
    [Header("Equipment Rarity")]
    public Color commonItem = Color.white;
    public Color rateItem = Color.green;
    public Color epicItem = new(0.5f, 0.0f, 0.5f);
    public Color legendaryItem = Color.yellow;
    public Color uniqueItem = Color.red;

    [Header("Equipment Category")] 
    public Texture2D categoryConsumable;
    public Texture2D categoryQuestItem;
    public Texture2D categoryEquipmentOneHandWeapon;
    public Texture2D categoryEquipmentTwoHandWeapon;
    public Texture2D categoryEquipmentShield;
    public Texture2D categoryEquipmentHelm;
    public Texture2D categoryEquipmentArmor;
    public Texture2D categoryEquipmentPants;
    public Texture2D categoryEquipmentGloves;
    public Texture2D categoryEquipmentBoots;
    public Texture2D categoryEquipmentNecklace;
    public Texture2D categoryEquipmentRing;
    public Texture2D categoryEquipmentCape;
    public Texture2D categoryEquipmentPet;

    private void Awake()
    {
        instance = this;
    }
}