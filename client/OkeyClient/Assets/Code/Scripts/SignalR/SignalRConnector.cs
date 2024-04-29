using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Scripts.SignalR.Packets;
using Code.Scripts.SignalR.Packets.Rooms;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SignalRConnector : MonoBehaviour
{
    private HubConnection hubConnection;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async void InitializeConnection()
    {
        this.hubConnection = new HubConnectionBuilder().WithUrl(Constants.SIGNALR_HUB_URL).Build();

        this.ConfigureHubEvents();

        try
        {
            await this.hubConnection.StartAsync();
            Debug.Log("SignalR connection established.");
            LobbyManager.Instance.SetConnectionStatus(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"SignalR connection failed: {ex.Message}");
        }
    }

    private void ConfigureHubEvents()
    {
        this.hubConnection.On<RoomsPacket>(
            "UpdateRoomsRequest",
            Rooms =>
            {
                Debug.Log("Received room update request.");
                foreach (var room in Rooms.listRooms)
                {
                    if (room == null)
                    {
                        Debug.Log("Null");
                        continue;
                    }
                    Debug.Log($"Room: {room.Name}, Players: {room.Players.Count}/{room.Capacity}");
                }
                // UpdateRoomDisplay(rooms)

                if (UIManagerPFormulaire.Instance != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        UIManagerPFormulaire.Instance.setActiveShowRooms();
                        LobbyManager.Instance.rommsListFilled = true;
                        LobbyManager.Instance.playerCount = Rooms.listRooms[0].Players.Count;
                        LobbyManager.Instance.SetPlayers(Rooms.listRooms[0].Players);
                        DisplayRooms.Instance.updateLabel();
                    });
                }
                else
                {
                    Debug.LogError("UIManagerPFormulaire instance is null.");
                }
            }
        );

        this.hubConnection.On<PacketSignal>(
            "ReceiveMessage",
            Message =>
            {
                Debug.Log(Message.message);
                // DisplayRooms.Instance.messageLogs.Add(message.message);
            }
        );

        this.hubConnection.On(
            "StartGame",
            () =>
            {
                Debug.Log($"Le jeu a commence");
                MainThreadDispatcher.Enqueue(() =>
                {
                    SceneManager.LoadScene("PlateauInit");
                });
            }
        );

        this.hubConnection.On<string>(
            "TurnSignal",
            (PlayerName) =>
            {
                Chevalet.Instance.IsJete = false;
                Chevalet.Instance.TuileJete = null;
                Debug.Log($"C'est le tour de {PlayerName}");
                MainThreadDispatcher.Enqueue(() =>
                {
                    LobbyManager.Instance.SetMyTurn(false);
                    LobbyManager.Instance.SetCurrentPlayerTurn(PlayerName);
                });
            }
        );

        this.hubConnection.On(
            "YourTurnSignal",
            () =>
            {
                Debug.Log($"C'est votre tour");
                MainThreadDispatcher.Enqueue(() =>
                {
                    LobbyManager.Instance.SetMyTurn(true);
                });
            }
        );

        this.hubConnection.On<TuilePacket>(
            "JeterRequest",
            () =>
            {
                var chevaletInstance = Chevalet.Instance;
                Debug.Log("Ok il faut jeter une tuile");
                var tuile = new TuilePacket
                {
                    X = "0",
                    Y = "0",
                    gagner = false
                };

                while (chevaletInstance.IsJete == false) { }
                Debug.Log("La tuile vient d'etre jetee");

                if (chevaletInstance.TuileJete != null)
                {
                    return chevaletInstance.TuileJete;
                }

                // add code here signal
                MainThreadDispatcher.Enqueue(() =>
                {
                    LobbyManager.Instance.SetMyTurn(false);
                });
                return tuile;
            }
        );

        this.hubConnection.On<DefaussePacket>(
            "ReceiveListeDefausse",
            (defausse) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    var tuilesList = new List<TuileData>();

                    if (defausse.Defausse != null)
                    {
                        foreach (var tuileData in defausse.Defausse)
                        {
                            if (
                                !tuileData.Equals(
                                    "couleur=;num=;defausse=;dansPioche=;Nom=;",
                                    StringComparison.Ordinal
                                )
                            )
                            {
                                var keyValuePairs = tuileData.Split(';');
                                string couleur = null;
                                var num = 0;
                                var defausse = false;
                                var dansPioche = false;
                                string nom = null;

                                foreach (var pair in keyValuePairs)
                                {
                                    if (string.IsNullOrWhiteSpace(pair))
                                    {
                                        continue;
                                    }

                                    var parts = pair.Split('=');

                                    if (parts.Length != 2)
                                    {
                                        continue;
                                    }

                                    var key = parts[0].Trim();
                                    var value = parts[1].Trim();
                                    switch (key)
                                    {
                                        case "couleur":
                                            couleur = value;
                                            break;
                                        case "num":
                                            int.TryParse(value, out num);
                                            break;
                                        case "defausse":
                                            bool.TryParse(value, out defausse);
                                            break;
                                        case "dansPioche":
                                            bool.TryParse(value, out dansPioche);
                                            break;
                                        case "Nom":
                                            nom = value;
                                            break;
                                        default:
                                            // Handle unknown keys (if needed)
                                            break;
                                    }
                                }

                                CouleurTuile coul;
                                switch (couleur)
                                {
                                    case "J":
                                        coul = CouleurTuile.J;
                                        break;
                                    case "N":
                                        coul = CouleurTuile.N;
                                        break;
                                    case "R":
                                        coul = CouleurTuile.R;
                                        break;
                                    case "B":
                                        coul = CouleurTuile.B;
                                        break;
                                    case "M":
                                        coul = CouleurTuile.M;
                                        break;
                                    default:
                                        throw new Exception();
                                }
                                tuilesList.Add(
                                    new TuileData(
                                        coul,
                                        num,
                                        nom != null && nom.Equals("Jo", StringComparison.Ordinal)
                                    )
                                );
                            }
                            else
                            {
                                tuilesList.Add(null);
                            }
                        }

                        // Fonctionaliteé qui affiche les defausses, a implementer plus tard....
                    }
                });
            }
        );

        this.hubConnection.On(
            "TuilesDistribueesSignal",
            () =>
            {
                Debug.Log($"Les tuiles ont ete distribuees");
            }
        );

        this.hubConnection.On<TuilePacket>(
            "FirstJeterActionRequest",
            () =>
            {
                var chevaletInstance = Chevalet.Instance;
                Debug.Log("Ok il faut jeter une tuile");

                while (chevaletInstance.IsJete == false) { }
                Debug.Log("La tuile vient d'etre jetee");

                if (chevaletInstance.TuileJete != null)
                {
                    return chevaletInstance.TuileJete;
                }

                MainThreadDispatcher.Enqueue(() =>
                {
                    LobbyManager.Instance.SetMyTurn(false);
                });
                return new TuilePacket
                {
                    X = "0",
                    Y = "0",
                    gagner = false
                };
            }
        );

        this.hubConnection.On<PiochePacket>(
            "PiochePacketRequest",
            () =>
            {
                var tuile = new PiochePacket { Centre = false, Defausse = true };
                return tuile;
            }
        );

        this.hubConnection.On<ChevaletPacket>(
            "ReceiveChevalet",
            async (chevalet) =>
            {
                var chevaletInstance = Chevalet.Instance;
                MainThreadDispatcher.Enqueue(() =>
                {
                    while (chevaletInstance == null)
                    {
                        chevaletInstance = Chevalet.Instance;
                    }

                    var tuilesData = new TuileData[2, 14];
                    var i = 0;
                    foreach (var tuileStr in chevalet.PremiereRangee)
                    {
                        if (
                            !tuileStr.Equals(
                                "couleur=;num=;defausse=;dansPioche=;Nom=;",
                                StringComparison.Ordinal
                            )
                        )
                        {
                            var keyValuePairs = tuileStr.Split(';');
                            string couleur = null;
                            var num = 0;
                            var defausse = false;
                            var dansPioche = false;
                            string nom = null;

                            foreach (var pair in keyValuePairs)
                            {
                                if (string.IsNullOrWhiteSpace(pair))
                                {
                                    continue;
                                }

                                var parts = pair.Split('=');

                                if (parts.Length != 2)
                                {
                                    continue;
                                }

                                var key = parts[0].Trim();
                                var value = parts[1].Trim();
                                switch (key)
                                {
                                    case "couleur":
                                        couleur = value;
                                        break;
                                    case "num":
                                        int.TryParse(value, out num);
                                        break;
                                    case "defausse":
                                        bool.TryParse(value, out defausse);
                                        break;
                                    case "dansPioche":
                                        bool.TryParse(value, out dansPioche);
                                        break;
                                    case "Nom":
                                        nom = value;
                                        break;
                                    default:
                                        // Handle unknown keys (if needed)
                                        break;
                                }
                            }

                            CouleurTuile coul;
                            switch (couleur)
                            {
                                case "J":
                                    coul = CouleurTuile.J;
                                    break;
                                case "N":
                                    coul = CouleurTuile.N;
                                    break;
                                case "R":
                                    coul = CouleurTuile.R;
                                    break;
                                case "B":
                                    coul = CouleurTuile.B;
                                    break;
                                case "M":
                                    coul = CouleurTuile.M;
                                    break;
                                default:
                                    throw new Exception();
                            }

                            tuilesData[0, i] = new TuileData(
                                coul,
                                num,
                                nom != null && nom.Equals("Jo", StringComparison.Ordinal)
                            );
                            i++;
                        }
                    }

                    i = 0;
                    foreach (var tuileStr in chevalet.SecondeRangee)
                    {
                        if (
                            !tuileStr.Equals(
                                "couleur=;num=;defausse=;dansPioche=;Nom=;",
                                StringComparison.Ordinal
                            )
                        )
                        {
                            var keyValuePairs = tuileStr.Split(';');

                            string couleur = null;
                            var num = 0;
                            var defausse = false;
                            var dansPioche = false;
                            string nom = null;

                            foreach (var pair in keyValuePairs)
                            {
                                if (string.IsNullOrWhiteSpace(pair))
                                {
                                    continue;
                                }

                                var parts = pair.Split('=');

                                if (parts.Length != 2)
                                {
                                    continue;
                                }

                                var key = parts[0].Trim();
                                var value = parts[1].Trim();
                                switch (key)
                                {
                                    case "couleur":
                                        couleur = value;
                                        break;
                                    case "num":
                                        int.TryParse(value, out num);
                                        break;
                                    case "defausse":
                                        bool.TryParse(value, out defausse);
                                        break;
                                    case "dansPioche":
                                        bool.TryParse(value, out dansPioche);
                                        break;
                                    case "Nom":
                                        nom = value;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            CouleurTuile coul;
                            switch (couleur)
                            {
                                case "J":
                                    coul = CouleurTuile.J;
                                    break;
                                case "N":
                                    coul = CouleurTuile.N;
                                    break;
                                case "R":
                                    coul = CouleurTuile.R;
                                    break;
                                case "B":
                                    coul = CouleurTuile.B;
                                    break;
                                case "M":
                                    coul = CouleurTuile.M;
                                    break;
                                default:
                                    throw new Exception();
                            }

                            tuilesData[1, i] = new TuileData(
                                coul,
                                num,
                                nom != null && nom.Equals("Jo", StringComparison.Ordinal)
                            );
                            i++;
                        }
                    }

                    if (Chevalet.neverReceivedChevalet)
                    {
                        chevaletInstance.SetTuiles(tuilesData);
                        chevaletInstance.TuilesPack = tuilesData;
                        chevaletInstance.Print2DMatrix();
                        chevaletInstance.Init();
                        Chevalet.neverReceivedChevalet = false;
                    }
                    else
                    {
                        chevaletInstance.TuilesPack = tuilesData;
                    }
                });
            }
        );
    }

    //public static void UpdateRoomDisplay(RoomsPacket roomsPacket)
    //{
    //    //if (PlayerInLobbyProgressScreen.Instance != null)
    //    //{
    //    //    PlayerInLobbyProgressScreen.Instance.SetRooms(roomsPacket);
    //    //}
    //    //else
    //    //{
    //    //    Debug.LogError("PlayerInLobbyProgressScreen instance is not found.");
    //    //}

    //    if (DisplayRooms.Instance != null)
    //    {
    //        DisplayRooms.Instance.changeLabel(roomsPacket);
    //    }
    //    else
    //    {
    //        Debug.LogError("DisplayRooms instance is not found.");
    //    }

    //}




    public async Task JoinRoom() // takes parameter roomName in future
    {
        if (this.hubConnection != null && this.hubConnection.State == HubConnectionState.Connected)
        {
            try
            {
                await this.hubConnection.SendAsync("LobbyConnection", "room1");
                MainThreadDispatcher.Enqueue(() =>
                {
                    UIManagerPFormulaire.Instance.showRooms.SetActive(false);
                    UIManagerPFormulaire.Instance.lobbyPlayerWaiting.SetActive(true);
                });
                Debug.Log($"Request to join room 1 sent.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to join room 1: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Cannot join room. Hub connection is not established.");
        }
    }

    public async void SendBoardState(TuileData[,] boardState)
    {
        // Ensure the connection is open before attempting to send a message
        if (this.hubConnection != null && this.hubConnection.State == HubConnectionState.Connected)
        {
            try
            {
                await this.hubConnection.InvokeAsync("SendBoardState", boardState);
                Debug.Log("Board state sent successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send board state: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Hub connection is not established.");
        }
    }

    private async void OnDestroy()
    {
        if (this.hubConnection != null && this.hubConnection.State == HubConnectionState.Connected)
        {
            await this.hubConnection.StopAsync();
            await this.hubConnection.DisposeAsync();
        }
    }

    // private IEnumerator ActivateShowRooms()
    // {
    //     yield return new WaitForEndOfFrame();

    //     if (UIManagerPFormulaire.Instance == null)
    //     {
    //         Debug.LogError("UIManagerPFormulaire instance is null.");
    //         yield break;
    //     }
    //     Debug.Log("setActiveShowRooms");
    //     UIManagerPFormulaire.Instance.setActiveShowRooms();
    // }
}
