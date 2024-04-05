using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LogInScreen : MonoBehaviour
{
    public GameObject Panel;
    public GameObject PanelAvatar;
    public GameObject creationPanel;

    [SerializeField]
    private TMP_InputField Login;

    [SerializeField]
    private TMP_InputField Password;

    [SerializeField]
    private Button connexionButton;
    
    [SerializeField]
    private Button createButton;

    [SerializeField]
    private Button backButton;

    // Start is called before the first frame update
    void Start()
    {
        Password.contentType = TMP_InputField.ContentType.Password;
        // Ajoute un écouteur au bouton "Créer"
        connexionButton.onClick.AddListener(OnConnexionClicked);

        createButton.onClick.AddListener(OnCreateClicked);

        // Ajoute un écouteur au bouton "Retour"
        backButton.onClick.AddListener(onBackBtnClicked);
    }

    // Méthode appelée lors du clic sur le bouton "Créer"
    void OnConnexionClicked()
    {
        string login = Login.text.Trim();
        string password = Password.text.Trim();

        if (
            !string.IsNullOrEmpty(login)
            && !string.IsNullOrEmpty(password)
        )
        {
            // Envoi du formulaire (à implémenter)
            Debug.Log("Formulaire envoyé !");
            //mettre le Panel de connexion en off
            Panel.SetActive(false);

            //activer le Panel de User avec avatar
            PanelAvatar.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Veuillez remplir tous les champs !");
        }
    }

    void OnCreateClicked() {
        Panel.SetActive(false);
        creationPanel.SetActive(true);
    }

    // Méthode pour charger la scène précédente
    private void onBackBtnClicked()
    {
        Panel.SetActive(false);
    }
}
