using UnityEngine;
using TMPro;
using Photon.Pun;

public class LobbyCodeDisplay : MonoBehaviour
{
    [Header("Référence UI")]
    public TextMeshProUGUI codeText;

    void Start()
    {
        if (PhotonNetwork.InRoom && codeText != null)
        {
            string code = PhotonNetwork.CurrentRoom.Name;
            codeText.text = $"Code de la room : <b>{code}</b>";
        }
        else
        {
            codeText.text = "Pas de room active";
        }
    }
}
