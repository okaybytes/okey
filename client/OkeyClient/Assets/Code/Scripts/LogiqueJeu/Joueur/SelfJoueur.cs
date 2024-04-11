namespace LogiqueJeu.Joueur
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Xml.Serialization;
    using UnityEngine;
    using UnityEngine.Networking;

    public sealed class SelfJoueur : Joueur
    {
        public string TokenConnexion { get; set; }

        public SelfJoueur()
            : base()
        {
            this.TokenConnexion = null;
            this.InGame.Pos = Position.SoiMeme;
        }

        public override void LoadSelf(MonoBehaviour Behaviour)
        {
            if (File.Exists(Application.persistentDataPath + Constants.SELF_PLAYER_SAVE_FILE))
            {
                TextReader reader = null;
                try
                {
                    var serializer = new XmlSerializer(typeof(SelfJoueur));
                    reader = new StreamReader(
                        Application.persistentDataPath + Constants.SELF_PLAYER_SAVE_FILE
                    );
                    this.CopyFrom((SelfJoueur)serializer.Deserialize(reader));
                }
                finally
                {
                    reader?.Close();
                }
            }
            if (this.TokenConnexion != null && this.NomUtilisateur != null)
            {
                Behaviour.StartCoroutine(this.FetchUserBG(this.UnmarshalAndInit));
            }
        }

        public void SaveXML()
        {
            if (this.TokenConnexion == null)
            {
                return;
            }
            TextWriter writer = null;
            try
            {
                var serializer = new XmlSerializer(this.GetType());
                writer = new StreamWriter(
                    Application.persistentDataPath + Constants.SELF_PLAYER_SAVE_FILE
                );
                serializer.Serialize(writer, this);
            }
            finally
            {
                writer?.Close();
            }
        }

        public void DeleteXML()
        {
            try
            {
                File.Delete(Application.persistentDataPath + Constants.SELF_PLAYER_SAVE_FILE);
            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException or NotSupportedException)
                {
                    Debug.Log("Could not delete SelfJoueur save file");
                    return;
                }
                throw;
            }
        }

        protected override IEnumerator FetchUserBG(Action<string> CallbackJSON = null)
        {
            var Response = "";

            var www = UnityWebRequest.Get(
                Constants.API_URL_DEV + "/compte/watch/" + this.NomUtilisateur
            );
            www.SetRequestHeader("Authorization", "Bearer " + this.TokenConnexion);
            www.certificateHandler = new BypassCertificate();
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Response = www.downloadHandler.text;
            }

            CallbackJSON?.Invoke(Response);
        }

        private void CopyFrom(SelfJoueur SelfJoueur)
        {
            base.CopyFrom(SelfJoueur);
            this.TokenConnexion = SelfJoueur.TokenConnexion;
        }

        protected override void UnmarshalAndInit(string Json)
        {
            var unmarshal = JsonUtility.FromJson<SelfJoueurAPICompteDTO>(Json);
            this.NomUtilisateur = unmarshal.username;
            this.Elo = unmarshal.elo;
            this.Achievements = unmarshal.achievements;
            this.SaveXML();
        }

        public void ConnexionCompte(
            MonoBehaviour Behaviour,
            string NomUtilisateur,
            string MotDePasse,
            Action<int> CallbackResult = null
        )
        {
            var JSON = JsonUtility.ToJson(
                new SelfJoueurAPIConnexionDTO(NomUtilisateur, MotDePasse)
            );
            Behaviour.StartCoroutine(
                this.PostUserConnexionBG(Behaviour, JSON, IsCreation: false, CallbackResult)
            );
        }

        public void CreationCompte(
            MonoBehaviour Behaviour,
            string NomUtilisateur,
            string MotDePasse,
            Action<int> CallbackResult = null
        )
        {
            var JSON = JsonUtility.ToJson(
                new SelfJoueurAPIConnexionDTO(NomUtilisateur, MotDePasse)
            );
            Behaviour.StartCoroutine(
                this.PostUserConnexionBG(Behaviour, JSON, IsCreation: true, CallbackResult)
            );
        }

        public void DeconnexionCompte()
        {
            this.DeleteXML();
            this.CopyFrom(new SelfJoueur());
        }

        // No error handling is built in for now. Need to handle the following cases
        // and any other ones that I may have missed mentioning:
        //
        // In case of logging into an existing account
        // - User not found (Invalid username)
        // - Password mismatch (Invalid password)
        //
        // In case of creating a new account
        // - Invalid username (illegal character, already taken...)
        // - Invalid password (length, complexity...)
        private IEnumerator PostUserConnexionBG(
            MonoBehaviour Behaviour,
            string JSON,
            bool IsCreation = false,
            Action<int> CallbackResult = null
        )
        {
            var Response = "";

            var www = UnityWebRequest.Post(
                Constants.API_URL_DEV + "/compte/" + (IsCreation ? "register" : "login"),
                JSON,
                "application/json"
            );
            www.certificateHandler = new BypassCertificate();
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Response = www.downloadHandler.text;
                var unmarshal = JsonUtility.FromJson<SelfJoueurAPIConnexionResponseDTO>(Response);
                this.NomUtilisateur = unmarshal.userName;
                this.TokenConnexion = unmarshal.token;
                this.LoadSelf(Behaviour);
            }
            else
            {
                Debug.Log(www.error);
            }

            CallbackResult?.Invoke((int)www.responseCode);
        }

        public override string ToString()
        {
            return $@"
                                TokenConnexion: {this.TokenConnexion},
                                ";
            ;
        }
    }
}
