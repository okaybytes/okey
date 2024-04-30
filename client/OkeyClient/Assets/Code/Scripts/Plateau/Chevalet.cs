using System;
using System.Collections.Generic;
using System.Text;
using Code.Scripts.SignalR.Packets;
using UnityEngine;

public class Chevalet : MonoBehaviour
{
    public static readonly GameObject[] Placeholders = new GameObject[28]; // Tableau des 28 Placeholders (la grille)

    public TuileData[,] Tuiles2D = new TuileData[2, 14];
    public TuileData[,] TuilesPack = new TuileData[2, 14];
    private readonly Stack<Tuile> _pileGauche = new Stack<Tuile>();
    private Stack<Tuile> _pileDroite = new Stack<Tuile>();
    private readonly Stack<Tuile> _pilePioche = new Stack<Tuile>();
    public static GameObject PileGauchePlaceHolder;
    public static GameObject PilePiochePlaceHolder;

    public static GameObject PileDroitePlaceHolder;
    public static GameObject JokerPlaceHolder;

    public bool IsJete { get; set; }
    public TuilePacket TuileJete { get; set; }

    public TuilePacket TuilePiochee = null;

    public static bool neverReceivedChevalet = true;

    public static Chevalet Instance { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("Destroying duplicate instance of Chevalet.");
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            Debug.Log("Chevalet instance set.");
        }
    }

    public void Init()
    {
        this.InitPlaceholders();
        this.InitializeBoardFromTuiles();
        this.TuileJete = null;
        this.IsJete = false;
    }

    private void Start()
    {
        this.InitPlaceholders();
        this.Awake();
        //this.InitializeBoardFromPlaceholders();
        //this.InitializeBoardFromTuiles();

        //PrintTuilesArray();
    }

    private void InitPlaceholders()
    {
        // Remplir le tableau avec les placeholders dans la scene
        for (var i = 1; i <= 28; i++) //i commence a 1 car le premier placeholder est "PlaceHolder1"
        {
            var placeholder = GameObject.Find("PlaceHolder" + i);
            if (placeholder != null)
            {
                Placeholders[i - 1] = placeholder; // Mettre le placeholder dans le tableau
            }
            else
            {
                Debug.LogError("PlaceHolder" + i + " not found!");
            }
        }
        //ToDo : Instancier les piles en fonction de ce qu'on reçoit du Back
        PileGauchePlaceHolder = GameObject.Find("PileGauchePlaceHolder");
        PileDroitePlaceHolder = GameObject.Find("PileDroitePlaceHolder");
        PilePiochePlaceHolder = GameObject.Find("PiochePlaceHolder");
        JokerPlaceHolder = GameObject.Find("Okey");
        this._pileGauche.Push(
            PileGauchePlaceHolder.transform.GetChild(0).gameObject.GetComponent<Tuile>()
        );
        this._pileGauche.Push(
            PileGauchePlaceHolder.transform.GetChild(0).gameObject.GetComponent<Tuile>()
        );
        this._pilePioche.Push(
            PilePiochePlaceHolder.transform.GetChild(0).gameObject.GetComponent<Tuile>()
        );
        PileGauchePlaceHolder
            .transform.GetChild(0)
            .gameObject.GetComponent<Tuile>()
            .SetIsDeplacable(false);
        PileGauchePlaceHolder
            .transform.GetChild(0)
            .gameObject.GetComponent<Tuile>()
            .SetIsInStack(true);
        PileDroitePlaceHolder
            .transform.GetChild(0)
            .gameObject.GetComponent<Tuile>()
            .SetIsDeplacable(false);
        JokerPlaceHolder
            .transform.GetChild(0)
            .gameObject.GetComponent<Tuile>()
            .SetIsDeplacable(false);

        PilePiochePlaceHolder
            .transform.GetChild(0)
            .gameObject.GetComponent<Tuile>()
            .SetIsDeplacable(false);
        PilePiochePlaceHolder
            .transform.GetChild(0)
            .gameObject.GetComponent<Tuile>()
            .SetIsInPioche(true);
    }

    public GameObject ClosestPlaceholder(Vector3 Position)
    {
        GameObject closestPlaceholder = null;
        var closestDistance = Mathf.Infinity;

        // Loop through placeholders using absolute positions
        foreach (var placeholder in Placeholders)
        {
            if (placeholder != null)
            {
                var distance = Vector3.Distance(Position, placeholder.transform.position); // Absolute position
                if (distance < closestDistance)
                {
                    closestPlaceholder = placeholder;
                    closestDistance = distance;
                }
            }
        }

        // Check specific piles using absolute positions
        var distancePileDroite = Vector3.Distance(
            Position,
            PileDroitePlaceHolder.transform.position
        ); // Absolute position

        var distanceJoker = Vector3.Distance(Position, JokerPlaceHolder.transform.position);

        // Determine closest placeholder based on distance and tile number
        if (distancePileDroite < closestDistance && GetTilesNumber() == 15)
        {
            closestDistance = distancePileDroite;
            closestPlaceholder = PileDroitePlaceHolder;
        }
        else if (
            (distanceJoker < closestDistance && GetTilesNumber() == 15)
            && (distanceJoker < distancePileDroite)
        )
        {
            closestDistance = distanceJoker;
            closestPlaceholder = JokerPlaceHolder;
        }
        else if (
            Position.y > 0
            && GetTilesNumber() == 15 /* et peut gagner*/
        )
        {
            closestPlaceholder = PilePiochePlaceHolder;
        }
        return closestPlaceholder;
    }

    public void UpdateTiles(GameObject Placeholder)
    {
        // Vérifier si le placeholder a des enfants (des tuiles)
        if (Placeholder.transform.childCount > 1)
        {
            // Déterminer le numéro du placeholder actuel
            var currentPlaceholderNumber = GetPlaceholderNumber(Placeholder.name);
            var indexTab = currentPlaceholderNumber - 1;

            // Vérifier s'il y a un placeholder à droite et s'il n'est pas plein
            if (
                currentPlaceholderNumber < 29
                && Placeholders[(indexTab + 1) % 28].transform.childCount <= 1
            )
            {
                // Déplacer la première tuile du placeholder actuel vers le placeholder à droite
                var tile = Placeholder.transform.GetChild(0).gameObject;
                tile.GetComponent<Tuile>().AttachToPlaceholder(Placeholders[(indexTab + 1) % 28]);

                // Appeler récursivement la fonction UpdateTiles sur le placeholder à droite
                this.UpdateTiles(Placeholders[(indexTab + 1) % 28]);
            }
        }
    }

    public void Draw(bool PiocheDroite)
    {
        if (
            GetTilesNumber() == 14 /* et c'est mon tour*/
        )
        {
            if (PiocheDroite)
            {
                PileGauchePlaceHolder
                    .transform.GetChild(0)
                    .gameObject.GetComponent<Tuile>()
                    .SetIsDeplacable(true);
                PileGauchePlaceHolder
                    .transform.GetChild(0)
                    .gameObject.GetComponent<Tuile>()
                    .SetIsInStack(false);
                this._pileGauche.Pop();
                if (this._pileGauche.Count > 0)
                {
                    var newChild = Instantiate(
                        this._pileGauche.Peek().gameObject,
                        PileGauchePlaceHolder.transform
                    );
                    newChild.transform.localPosition = Vector3.zero;
                    newChild.GetComponent<Tuile>().SetIsInStack(true);
                    newChild.GetComponent<Tuile>().SetIsDeplacable(false);
                }
                //ToDo : Envoyer "Pioche à Droite"
                //
            }
            else
            {
                PilePiochePlaceHolder
                    .transform.GetChild(0)
                    .gameObject.GetComponent<Tuile>()
                    .SetIsDeplacable(true);
                PilePiochePlaceHolder
                    .transform.GetChild(0)
                    .gameObject.GetComponent<Tuile>()
                    .SetIsInPioche(false);
                this._pilePioche.Pop();
                if (this._pilePioche.Count > 0)
                {
                    var newChild = Instantiate(
                        this._pilePioche.Peek().gameObject,
                        PilePiochePlaceHolder.transform
                    );
                    newChild.transform.localPosition = Vector3.zero;
                    newChild.GetComponent<Tuile>().SetIsInStack(true);
                    newChild.GetComponent<Tuile>().SetIsDeplacable(false);
                }
                //ToDo : Envoyer "Pioche au centre"
            }
        }
    }

    public void ThrowTile(Tuile Tuile)
    {
        this._pileDroite.Push(Tuile);

        Debug.Log($"Tuile recue {Tuile.GetCouleur()}");
        var tuileData = this.GetTuilePacketFromChevalet(Tuile, false);
        Debug.Log($"{tuileData.Y}, {tuileData.X}");
        this.IsJete = true;
        this.TuileJete = tuileData;
    }

    public void ThrowTileToWin(Tuile Tuile)
    {
        var tuileData = this.GetTuilePacketFromChevalet(Tuile, true);
        Debug.Log($"{tuileData.Y}, {tuileData.X}, {tuileData.gagner}");
        this.IsJete = true;
        this.TuileJete = tuileData;
        //ToDo : Envoyer TuileData + Pile Pioche + tuiles2D
        //Attendre Vérif
        //Et communiquer le résultat
        //pilePioche.Push(tuile);
    }

    public TuilePacket GetTuilePacketFromChevalet(Tuile T, bool gain)
    {
        if (T != null)
        {
            for (var x = 0; x < 2; x++)
            {
                for (var y = 0; y < 14; y++)
                {
                    if (
                        T.GetCouleur().Equals("V", StringComparison.Ordinal)
                        || T.GetCouleur().Equals("J", StringComparison.Ordinal)
                    )
                    {
                        if (
                            this.TuilesPack[x, y].couleur.Equals("V", StringComparison.Ordinal)
                            || this.TuilesPack[x, y].couleur.Equals("J", StringComparison.Ordinal)
                        )
                        {
                            if (
                                T.GetValeur() == this.TuilesPack[x, y].num
                                && T.GetIsJoker() == this.TuilesPack[x, y].isJoker
                            )
                            {
                                return new TuilePacket
                                {
                                    X = "" + y,
                                    Y = "" + x,
                                    gagner = gain
                                };
                            }
                        }
                    }
                    else
                    {
                        if (
                            T.GetCouleur()
                                .Equals(this.TuilesPack[x, y].couleur, StringComparison.Ordinal)
                            && T.GetValeur() == this.TuilesPack[x, y].num
                        )
                        {
                            return new TuilePacket
                            {
                                X = "" + y,
                                Y = "" + x,
                                gagner = gain
                            };
                        }
                    }
                }
            }
        }
        Debug.Log("On jete une tuile nulle.");
        return new TuilePacket();
    }

    // Fonction auxiliaire pour extraire le numéro du placeholder à partir de son nom
    public static int GetPlaceholderNumber(string Name)
    {
        var numberString = Name.Substring(11);
        int number;
        int.TryParse(numberString, out number);
        return number;
    }

    private void InitializeBoardFromPlaceholders()
    {
        for (var i = 0; i < Placeholders.Length; i++)
        {
            var x = i / 14; // Calculate the row based on index.
            var y = i % 14; // Calculate the column based on index.

            var placeholder = Placeholders[i];

            //Debug.Log($"Checking placeholder {i}, Child count: {placeholder.transform.childCount}");

            if (placeholder.transform.childCount > 0)
            {
                // Get the first child of the placeholder
                var child = placeholder.transform.GetChild(0).gameObject;

                if (child.TryGetComponent<SpriteRenderer>(out var childSpriteRenderer))
                {
                    // note: "color_value" should be used for naming the sprites
                    var properties = childSpriteRenderer.sprite.name.Split('_');
                    //Debug.Log(properties[0] + properties[1]);

                    if (properties.Length == 2)
                    {
                        var couleur = this.ConvertToFrontendColorToBackendEnumName(properties[0]);
                        //                    Debug.Log(couleur);
                        var num = int.Parse(properties[1]);
                        var isJoker = properties[0] == "FakeJoker";
                        this.Tuiles2D[x, y] = new TuileData(couleur, num, isJoker);
                        var tuilePlaceHolder = placeholder.GetComponent<Tuile>();
                        tuilePlaceHolder.SetCouleur(couleur.ToString());
                        tuilePlaceHolder.SetValeur(num);
                        tuilePlaceHolder.SetIsJoker(isJoker);
                        //this.tuilesPack[x, y] = new TuileData(couleur, num, isJoker);
                        //Debug.Log("["+x+"]"+"["+y+"]"+" "+this.tuiles2D[x, y].couleur+ " " +this.tuiles2D[x, y].num+" "+this.tuiles2D[x, y].isJoker);
                    }
                    else
                    {
                        // If the name does not contain both color and value, log an error or set as null
                        Debug.LogError(
                            $"Child sprite of placeholder at index {i} does not have a properly formatted name."
                        );
                        this.Tuiles2D[x, y] = null;
                        var tuilePlaceHolder = placeholder.GetComponent<Tuile>();
                        tuilePlaceHolder.SetCouleur(null);
                        tuilePlaceHolder.SetValeur(0);
                        //this.tuilesPack[x, y] = null;
                    }
                }
                else
                {
                    // If there is no SpriteRenderer component, set the corresponding array position to null
                    this.Tuiles2D[x, y] = null;
                    var tuilePlaceHolder = placeholder.GetComponent<Tuile>();
                    tuilePlaceHolder.SetCouleur(null);
                    tuilePlaceHolder.SetValeur(0);
                    //this.tuilesPack[x, y] = null;
                }
            }
            else //empty placeholder
            {
                // If the placeholder is empty, set the corresponding array position to null
                this.Tuiles2D[x, y] = null;
                var tuilePlaceHolder = placeholder.GetComponent<Tuile>();
                tuilePlaceHolder.SetCouleur(null);
                tuilePlaceHolder.SetValeur(0);
            }
        }
    }

    private static string FromTuileToSpriteName(TuileData Tuile)
    {
        if (
            Tuile.isJoker
            || Tuile.couleur.Equals("M", StringComparison.Ordinal)
            || Tuile.couleur.Equals("X", StringComparison.Ordinal)
        )
        {
            return "Fake Joker_1";
        }
        var name = "";
        switch (Tuile.couleur)
        {
            case "B":
                name = "Blue_";
                break;
            case "R":
                name = "Red_";
                break;
            case "N":
                name = "Black_";
                break;
            case "J":
                name = "Green_";
                break;
        }

        name += Tuile.num;
        return name;
    }

    private void InitializeBoardFromTuiles()
    {
        var sprites = Resources.LoadAll<Sprite>("Tiles");

        var spritesDic = new Dictionary<string, Sprite>();

        for (var i = 0; i < 13; i++)
        {
            spritesDic.Add($"Black_{i + 1}", sprites[i]);
        }

        for (var i = 13; i < 26; i++)
        {
            spritesDic.Add($"Blue_{(i + 1) - 13}", sprites[i]);
        }

        spritesDic.Add("Fake Joker_1", sprites[26]);
        spritesDic.Add("Fake Joker_2", sprites[27]);

        for (var i = 28; i < 41; i++)
        {
            spritesDic.Add($"Green_{(i + 1) - 28}", sprites[i]);
        }

        spritesDic.Add($"Pioche", sprites[41]);

        for (var i = 42; i < 55; i++)
        {
            spritesDic.Add($"Red_{(i + 1) - 42}", sprites[i]);
        }

        for (var i = 0; i < Placeholders.Length; i++)
        {
            var x = i / 14;
            var y = i % 14;
            var placeholder = Placeholders[i];
            if (this.Tuiles2D[x, y] != null)
            {
                var childObject = new GameObject("SpriteChild");
                childObject.transform.SetParent(placeholder.transform);
                var spriteRen = childObject.AddComponent<SpriteRenderer>();
                var mat = new Material(Shader.Find("Sprites/Default"))
                {
                    color = new Color(0.9529411764705882f, 0.9411764705882353f, 0.8156862745098039f)
                };
                spriteRen.material = mat;
                spriteRen.sprite = spritesDic[FromTuileToSpriteName(this.Tuiles2D[x, y])];
                spriteRen.sortingOrder = 3;
                var transform1 = spriteRen.transform;
                transform1.localPosition = new Vector3(0, 0, 0);
                transform1.localScale = new Vector3(1, 1, 1);
                childObject.AddComponent<Tuile>();
                var boxCollider2D = childObject.AddComponent<BoxCollider2D>();
                boxCollider2D.size = new Vector2((float)0.875, (float)1.25);

                placeholder.GetComponent<Tuile>().SetValeur(this.Tuiles2D[x, y].num);
                placeholder.GetComponent<Tuile>().SetCouleur(this.Tuiles2D[x, y].couleur);
                placeholder.GetComponent<Tuile>().SetIsJoker(this.Tuiles2D[x, y].isJoker);
            }
        }
    }

    private CouleurTuile ConvertToFrontendColorToBackendEnumName(string FrontendColor)
    {
        //Debug.Log(FrontendColor);
        return FrontendColor.ToLower() switch
        {
            "yellow" => CouleurTuile.J,
            "black" => CouleurTuile.N,
            "red" => CouleurTuile.R,
            "blue" => CouleurTuile.B,
            "green" => CouleurTuile.V,
            // other cases still needed
            "fakejoker" => CouleurTuile.X,
            _ => CouleurTuile.M, // the okey
        };
    }

    private (int, int) ConvertPlaceHolderNumberToMatrixCoordinates(int PlaceholderNumber)
    {
        //conversion du numero du place holder 1->28 en coordonnées dans la matrice [2][14]
        var x = (PlaceholderNumber - 1) / 14;
        var y = (PlaceholderNumber - 1) % 14;
        return (x, y);
    }

    public void UpdateMatrixAfterMovement(
        GameObject PreviousPlaceHolder,
        GameObject NextPlaceholder
    )
    {
        //cas de deplacement a l'interieur du chevalet : Chevalet -> Chevalet
        var chevaletFront = GameObject.Find("ChevaletFront");
        if (
            PreviousPlaceHolder.transform.IsChildOf(chevaletFront.transform)
            && NextPlaceholder.transform.IsChildOf(chevaletFront.transform)
        )
        {
            if (!PreviousPlaceHolder.name.Equals(NextPlaceholder.name, StringComparison.Ordinal))
            {
                var tuile = PreviousPlaceHolder.GetComponent<Tuile>();
                var nt = NextPlaceholder.GetComponent<Tuile>();
                nt.SetCouleur(tuile.GetCouleur());
                nt.SetValeur(tuile.GetValeur());
                nt.SetIsJoker(tuile.GetIsJoker());

                tuile.SetCouleur(null);
                tuile.SetValeur(0);
                this.InitializeBoardFromPlaceholders(); // il faut re parcourir le chevalets pour recuperer les nouvelles position car il y'a le decalage des tuiles
            }
            this.InitializeBoardFromPlaceholders(); // il faut re parcourir le chevalets pour recuperer les nouvelles position car il y'a le decalage des tuiles
        }
        //cas de tirage : pioche ou pile gauche -> Chevalet
        else if (
            PreviousPlaceHolder == PileGauchePlaceHolder
            || (
                PreviousPlaceHolder == PilePiochePlaceHolder
                && NextPlaceholder.transform.IsChildOf(chevaletFront.transform)
            )
        )
        {
            //ajouter la piece pioché au chevalet
            this.InitializeBoardFromPlaceholders(); // il faut re parcourir le chevalets car quand on pioche on peut la mettre entre 2 tuiles et ca crée un decalage
            var tuile = PreviousPlaceHolder.GetComponent<Tuile>();
            var nt = NextPlaceholder.GetComponent<Tuile>();
            nt.SetCouleur(tuile.GetCouleur());
            nt.SetValeur(tuile.GetValeur());
            nt.SetIsJoker(tuile.GetIsJoker());

            tuile.SetCouleur(null);
            tuile.SetValeur(0);
            //Faudra parler a lequipe du backend pour savoir si ca leur suffit la matrice mis a jour et le contenu des defausses ou ils veulent exactement la piece pioché
        }
        //cas de jet : Chevalet -> pile droite / tentative de gain
        else if (
            PreviousPlaceHolder.transform.IsChildOf(chevaletFront.transform)
            && (NextPlaceholder == PileDroitePlaceHolder || NextPlaceholder == JokerPlaceHolder)
        )
        {
            //enlever la piece jeté du chevalet
            var prvPhPos = this.ConvertPlaceHolderNumberToMatrixCoordinates(
                GetPlaceholderNumber(PreviousPlaceHolder.name)
            );

            //Debug.Log($"tuile[{prv_ph_pos.Item1}][{prv_ph_pos.Item1}] a été jeté");
            Debug.Log(
                ""
                    + this.Tuiles2D[prvPhPos.Item1, prvPhPos.Item2].couleur
                    + this.Tuiles2D[prvPhPos.Item1, prvPhPos.Item2].num
                    + this.Tuiles2D[prvPhPos.Item1, prvPhPos.Item2].isJoker
            );

            this.Tuiles2D[prvPhPos.Item1, prvPhPos.Item2] = null;

            var tuile = PreviousPlaceHolder.GetComponent<Tuile>();
            var nt = NextPlaceholder.GetComponent<Tuile>();
            nt.SetCouleur(tuile.GetCouleur());
            nt.SetValeur(tuile.GetValeur());
            nt.SetIsJoker(tuile.GetIsJoker());
            tuile.SetCouleur(null);
            tuile.SetValeur(0);

            //Faudra parler a lequipe du backend pour savoir si ca leur suffit la matrice mis a jour et le contenu des defausses ou ils veulent exactement la piece jeté
        }
        else //cas derreur
        {
            Debug.LogError("error updating matrix after movement");

            Debug.Log($"Source: {PreviousPlaceHolder.name}, Destination: {NextPlaceholder.name}");
        }

        this.Print2DMatrix();
    }

    public void Print2DMatrix() //Really useful to visualize tiles placement matrix
    {
        // Initialize a string to store the table
        var table = "";

        for (var x = 0; x < 2; x++) // Rows
        {
            for (var y = 0; y < 14; y++) // Columns
            {
                if (this.Tuiles2D[x, y] != null)
                {
                    table +=
                        $"| {this.Tuiles2D[x, y].couleur} {this.Tuiles2D[x, y].num} {this.Tuiles2D[x, y].isJoker} ";
                }
                else
                {
                    table += "| Vide ";
                }
            }
            table += "|\n                    "; // End of the row
        }

        // Print the table
        Debug.Log(table);
    }

    public void SetTuiles(TuileData[,] Tuiles)
    {
        this.Tuiles2D = Tuiles;
    }

    //public void SetTuile(int x, int y, TuileData tuile)
    //{
    //    tuiles2D[x, y] = tuile;
    //}

    //// get Tuile de l'index donnee
    //public TuileData GetTuile(int x, int y)
    //{
    //    if (x >= 0 && x < 2 && y >= 0 && y < 14)
    //    {
    //        return tuiles2D[x, y];
    //    }
    //    else
    //    {
    //        Debug.LogError("GetTuile: Index out of bounds");
    //        return null;
    //    }
    //}

    public TuileData[,] GetTuilesArray()
    {
        return this.Tuiles2D;
    }

    private void PrintTuilesArray()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Board State:");

        for (var x = 0; x < this.Tuiles2D.GetLength(0); x++)
        {
            for (var y = 0; y < this.Tuiles2D.GetLength(1); y++)
            {
                var tile = this.Tuiles2D[x, y];
                if (tile != null)
                {
                    sb.AppendLine(
                        $"Tile at [{x},{y}]: Color = {tile.couleur}, Number = {tile.num}, IsJoker = {tile.isJoker}"
                    );
                }
                else
                {
                    sb.AppendLine($"Tile at [{x},{y}]: None");
                }
            }
        }

        Debug.Log(sb.ToString());
    }

    private static int GetTilesNumber()
    {
        var num = 0;
        foreach (var placeholder in Placeholders)
        {
            if (placeholder.transform.childCount == 1)
            {
                num++;
            }
        }
        return num;
    }
}








/*
    to keep just in case :
    public static Tuile[] GetTilesPlacementInChevaletTab()
    {
        // Creation d'un tableau pour stocker le placement courant des tuiles sur le chevalet
        Tuile[] TilesArray = new Tuile[placeholders.Length];

        //loop sur les placeholders
        foreach (GameObject placeholder in placeholders)
        {
            int index = Array.IndexOf(placeholders, placeholder);

            if (placeholder.transform.childCount > 0)
            { // le placeholder a un child -> il contient une tuile -> on la met dans le tableau avec l'index du placeholder parent
                GameObject child = placeholder.transform.GetChild(0).gameObject;
                Tuile tuile = child.GetComponent<Tuile>();

                if (tuile != null)
                {
                    // Place the Tuile in the 1D array
                    TilesArray[index] = tuile;
                }
                else
                {
                    Debug.LogError("Error : placeholder has a child that isn't a Tile");
                }
            }
            else // si le placeholder n'a aucun child -> il est vide -> on met null a l'index correspondant au placeholder dans le tableau
            {
                TilesArray[index] = null;
            }
        }

        return TilesArray;
    }

    public static void PrintTilesArrayForTest()
    {
        // Get the tiles placement array using the method
        Tuile[] TilesArray = GetTilesPlacementInChevaletTab();

        // Check if TilesArray is not null
        if (TilesArray != null)
        {
            // Loop through the TilesArray to print tile number and color
            for (int i = 0; i < TilesArray.Length; i++)
            {
                if (TilesArray[i] != null)
                {
                    // Check if GetValeur() and GetCouleur() are not null
                    if (
                        TilesArray[i].GetValeur().ToString() != null
                        && TilesArray[i].GetCouleur().ToString() != null
                    ) // supposons que les valeurs sont init a 0
                    {
                        Debug.Log(
                            "Tile Number "
                                + (i + 1)
                                + " :"
                                + TilesArray[i].GetValeur().ToString()
                                + ", Tile Color: "
                                + TilesArray[i].GetCouleur().ToString()
                        );
                    }
                    else
                    {
                        Debug.Log("Tile at index " + i + " has null values.");
                    }
                }
                else
                {
                    Debug.Log("Tile at index " + i + " is null.");
                }
            }
        }
        else
        {
            Debug.Log("TilesArray is null.");
        }
    }
    */
