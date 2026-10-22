using UnityEngine;

/// <summary>Décor du trajet, sans signification métrique ni mesure de débit.</summary>
public class NetworkRideVisuals : MonoBehaviour
{
    public const int Layer=30;
    public Transform[] packets;
    public void Animate(float seconds)
    {
        for(int i=0;i<packets.Length;i++)
        {
            float angle=i*2.399963f;
            packets[i].localPosition=new Vector3(Mathf.Cos(angle)*2.6f,3.6f+Mathf.Sin(angle)*2.6f,Mathf.Repeat(seconds*15+i*4.7f,58));
        }
    }
}
