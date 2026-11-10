using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class PhotoV2Storage : MonoBehaviour
{
    public GameObject alternative;
    public Transform packet;
    public Transform archiveSlot;
    public Text explanation;
    public Text clientLabel;
    public RawImage clientImage;

    Texture2D _stored;
    public bool Saved => _stored != null;
    public bool Readable => clientImage.enabled;
    public bool PredictedStorage { get; private set; }

    public IEnumerator Save(Texture2D photo, bool predictedStorage)
    {
        PredictedStorage = predictedStorage;
        explanation.text = predictedStorage
            ? "Oui. La photo rejoint la baie de stockage."
            : "Le serveur a produit l’aperçu. Ici, la baie de stockage conserve la photo.";
        var start = packet.position;
        packet.gameObject.SetActive(true);
        for (float elapsed = 0; elapsed < 1.15f; elapsed += Time.deltaTime)
        {
            packet.position = Vector3.Lerp(start, archiveSlot.position, Mathf.SmoothStep(0,1,elapsed / 1.15f));
            yield return null;
        }
        packet.position = archiveSlot.position;
        _stored = photo;
        clientLabel.text = "OUVRIR LA PHOTO";
        Debug.Log("CLOUDVR_V2_PHOTO_SAVED");
    }

    public bool Read()
    {
        if (!Saved) return false;
        clientImage.texture = _stored;
        clientImage.enabled = true;
        clientLabel.text = "";
        Debug.Log("CLOUDVR_V2_PHOTO_READ");
        return true;
    }

    public bool Shows(Texture2D expected) => Readable && clientImage.texture == expected;
}
