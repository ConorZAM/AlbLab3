using UnityEngine;

public class CollisionSnd : MonoBehaviour
{
    public bool isActive = true;
    public string tagString = "Default";
    public float minVelocity = 0;

    [Space]
    public AudioSource audioSource;
    public float pitch = -1;
    public AudioClip hitSND;

    [Space]
    public bool showMsg = false;
    public string msgString = "Hit!";
    public float msgTime = 0.75f;



    //
    void Awake() { if (audioSource == null) audioSource = GetComponent<AudioSource>(); }
    //
    void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;
        if (tagString != "" && !collision.gameObject.CompareTag(tagString)) return;
        if (minVelocity > 0 && collision.relativeVelocity.magnitude < minVelocity) return;

        if (showMsg) DisplayMsg.showAll(msgString, msgTime);
        if (hitSND != null) playSnd(hitSND);

        //print(collision.gameObject.name + " : Vel = " + collision.relativeVelocity.magnitude);
    }
    //
    void playSnd(AudioClip audioClip)
    {
        if (audioClip == null) return;

        if (pitch == -1) audioSource.pitch = 1 + Random.Range(-0.015f, 0f); else if (pitch > 0) audioSource.pitch = pitch;
        audioSource.PlayOneShot(audioClip);
    }
    //
}
