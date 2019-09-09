using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelection : MonoBehaviour {
    private int selectedCharacterIndex = 0;
    private bool bSelected;
    private Color desiredColor;

    [Header("List of characters")]
    [SerializeField] private List<CharacterSelectObject> characterList = new List<CharacterSelectObject>();

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI characterName = null;
    [SerializeField] private Image characterSplash = null;
    [SerializeField] private Image backgroundColor = null;
    
    //[Header("Sounds")]
    //[SerializeField] private AudioSource arrowClickSFX;

    [Header("Tweaks")]
    [SerializeField] private float backgroundColorTransitionSpeed = 10.0f;

    private void Start() {
        bSelected = false;
        string player = PlayerPrefs.GetString("PlayerSelected", "player_aaron");
        float playerHeight = PlayerPrefs.GetFloat("PlayerHeight", 3.0f);
        for (int i=0; i < characterList.Count; i++) {
            if (characterList[i].prefabName == player) {
                selectedCharacterIndex = i;
            }
        }
        
        UpdateCharacterSelectionUI();
    }

    private void Update() {
        backgroundColor.color = Color.Lerp(backgroundColor.color, desiredColor, Time.deltaTime * backgroundColorTransitionSpeed);
        if (bSelected) {
            if (PlayerPrefs.GetString("PlayerSelected") == characterList[selectedCharacterIndex].prefabName) {
                SceneManager.LoadScene("game");
            }
        }
    }

    public void LeftArrow() {
        //arrowClickSFX.Play();
        selectedCharacterIndex--;
        if (selectedCharacterIndex < 0) {
            selectedCharacterIndex = characterList.Count - 1;
        }
        UpdateCharacterSelectionUI();
    }

    public void RightArrow() {
        //arrowClickSFX.Play(); 
        selectedCharacterIndex++;
        if (selectedCharacterIndex == characterList.Count) {
            selectedCharacterIndex = 0;
        }
        UpdateCharacterSelectionUI();
    }

    public void SelectCharacter() {
        //arrowClickSFX.Play();
        string player = characterList[selectedCharacterIndex].prefabName;
        float playerHeight = characterList[selectedCharacterIndex].prefabHeight;
        PlayerPrefs.SetString("PlayerSelected", player);
        PlayerPrefs.SetFloat("PlayerHeight", playerHeight);
        
        bSelected = true;
    }

    private void UpdateCharacterSelectionUI() {
        //splash, name, color
        characterSplash.sprite = characterList[selectedCharacterIndex].splash;
        characterName.text = characterList[selectedCharacterIndex].name;
        desiredColor = characterList[selectedCharacterIndex].color;
    }
        
    [System.Serializable]
    public class CharacterSelectObject {
        public Sprite splash;
        //public GameObject character;
        public string name;
        public string prefabName;
        public float prefabHeight;
        public Color color;
    }

}
