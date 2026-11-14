using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class PhotoV2Storage : MonoBehaviour
{
    public Collider busyBank;
    public Renderer bankIndicator;
    public Material writtenIndicator;
    public Camera view;
    public Transform packetCarrier;
    public RawImage packet;
    public LineRenderer serverLink;
    public LineRenderer delivery;
    public Material deliverySignal;
    public LineRenderer branch;
    public Text explanation;
    public Text bankLabel;
    public Text clientLabel;
    public RawImage clientImage;
    public RawImage clientPreview;
    public Collider originalChoice;
    public Collider previewChoice;
    public LineRenderer readPath;
    public bool Verified { get; private set; }

    Texture2D _inFlight;
    Texture2D _stored;
    public bool ReadyToRoute { get; private set; }
    public bool Saved => _stored != null;
    public bool Readable => clientImage.enabled;

    void LateUpdate()
    {
        if (packet.enabled)
            packetCarrier.rotation = Quaternion.LookRotation(packetCarrier.position - view.transform.position);
    }

    public IEnumerator Deliver(Texture2D photo, Vector3 from)
    {
        _inFlight = photo;
        packet.texture = photo;
        packetCarrier.position = from;
        packet.enabled = true;
        serverLink.SetPosition(0, from);
        serverLink.enabled = true;
        delivery.sharedMaterial = deliverySignal;
        yield return Follow(serverLink, 1.8f);
        yield return Follow(delivery, 3.7f);
        ReadyToRoute = true;
        explanation.text = "ORIGINAL REÇU • CHOISIR UNE BANQUE LIBRE";
    }

    public void ReportBusy()
    {
        explanation.text = "BANQUE A OCCUPÉE • ESSAYER LA BANQUE B";
    }

    public IEnumerator Save()
    {
        ReadyToRoute = false;
        explanation.text = "BANQUE B • ÉCRITURE EN COURS";
        bankLabel.text = "B / ÉCRITURE";
        branch.enabled = true;
        yield return Follow(branch, 1.6f);
        _stored = _inFlight;
        packet.enabled = false;
        bankIndicator.sharedMaterial = writtenIndicator;
        bankLabel.text = "B / ENREGISTRÉE";
        explanation.text = "BANQUE B • PHOTO ENREGISTRÉE";
        clientLabel.text = "RELIRE LA BANQUE B";
        Debug.Log("CLOUDVR_V2_PHOTO_SAVED");
    }

    IEnumerator Follow(LineRenderer path, float speed)
    {
        for (int i = 1; i < path.positionCount; i++)
        {
            var from = path.GetPosition(i - 1);
            var to = path.GetPosition(i);
            float duration = Vector3.Distance(from, to) / speed;
            for (float time = 0; time < duration; time += Time.deltaTime)
            {
                packetCarrier.position = Vector3.Lerp(from, to, time / duration);
                yield return null;
            }
            packetCarrier.position = to;
        }
    }

    public IEnumerator Read(Texture2D preview)
    {
        clientLabel.text = "LECTURE DE LA BANQUE B";
        readPath.enabled = true;
        packetCarrier.position = readPath.GetPosition(0);
        packet.enabled = true;
        yield return Follow(readPath, 3.2f);
        packet.enabled = false;
        clientImage.texture = _stored;
        clientImage.enabled = true;
        clientPreview.texture = preview;
        clientPreview.enabled = true;
        originalChoice.gameObject.SetActive(true);
        previewChoice.gameObject.SetActive(true);
        clientLabel.text = "CHOISISSEZ LE FICHIER ORIGINAL";
        Debug.Log("CLOUDVR_V2_PHOTO_READ");
    }

    public bool ConfirmOriginal(Collider target)
    {
        if (target == previewChoice)
            clientLabel.text = "24 × 16 · APERÇU DU CPU";
        if (target != originalChoice) return false;
        Verified = true;
        clientLabel.text = "ORIGINAL VÉRIFIÉ · BANQUE B";
        originalChoice.enabled = false;
        previewChoice.enabled = false;
        return true;
    }

    public bool Shows(Texture2D expected) => Readable && clientImage.texture == expected;
}
