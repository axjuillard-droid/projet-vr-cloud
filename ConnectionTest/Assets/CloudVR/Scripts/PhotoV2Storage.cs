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
        explanation.text = "PHOTO REÇUE • SÉLECTIONNER UNE BANQUE LIBRE";
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
        clientLabel.text = "OUVRIR LA PHOTO";
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
