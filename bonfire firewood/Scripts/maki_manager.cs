
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class maki_manager : UdonSharpBehaviour
{
    [Header("燃える音(この中で1つランダム)")]
    [SerializeField] private AudioClip[] burnSounds;

    [Header("音の発生源")]
    [SerializeField] private AudioSource audioSrc;

    [Header("火元")]
    [SerializeField] private Transform[] fireTransform;
    private Vector3[] firePosition;

    [Header("火元判定距離(m)")]
    [SerializeField] float distBurnThreshold = 0.4f;

    [Header("薪燃焼エフェクト(すべて再生)")]
    [SerializeField] private ParticleSystem[] burnParticle;

    [FieldChangeCallback(nameof(vrcPick))]
    private VRC_Pickup _vrcPick;
    private VRC_Pickup vrcPick => _vrcPick ? _vrcPick : (_vrcPick = (VRC_Pickup)GetComponent(typeof(VRC_Pickup)));

    private Vector3 initPos;
    private Quaternion initRot;

    [Header("燃え始めの瞬間～リスポーンまでの時間(sec)")]
    [SerializeField] private float respawnTime = 2.0f;

    private float currentTime = 0f;
    private bool isBurning = false;
    private int fireIndex = 0;

    public void Start()
    {
        vrcPick.InteractionText = "FireWood";
        vrcPick.DisallowTheft = true;

        initPos = this.gameObject.transform.position;
        initRot = this.gameObject.transform.rotation;

        if (fireTransform != null)
        {
            firePosition = new Vector3[fireTransform.Length];
            for (int i = 0; i < fireTransform.Length; i++)
            {
                firePosition[i] = fireTransform[i].position;
            }
        }

        isBurning = false;
    }

    public void Update()
    {
        if (isBurning)
        {
            DisablePickUp();
            if (currentTime < 0)
            {
                EnablePickUp();
                isBurning = false;
                if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
                    this.transform.SetPositionAndRotation(initPos,initRot);
            }
            else
            {
                currentTime -= Time.deltaTime;
            }
        }
        else
        {
            EnablePickUp();
        }
    }

    public override void OnDrop()
    {
        PlayHaptics();

        float min_dist = float.MaxValue;
        if (fireTransform != null)
        {
            for(int i = 0; i < firePosition.Length; i++)
            {
                float dist = Vector3.Distance(this.gameObject.transform.position, firePosition[i]);
                if (min_dist > dist)
                {
                    min_dist = dist;
                    fireIndex = i;
                }
            }
        }
        
        if (min_dist < distBurnThreshold)
        {
            currentTime = respawnTime;
            isBurning = true;
            DisablePickUp();

            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(BurnFireWood));
        }
    }

    public void BurnFireWood()
    {
        //Sounds            
        if (audioSrc && !audioSrc.isPlaying)
        {
            audioSrc.transform.position = fireTransform[fireIndex].position;
            audioSrc.PlayOneShot(burnSounds[Random.Range(0, burnSounds.Length)]);
        }
        
        //Particles
        foreach (ParticleSystem burn_pt in burnParticle) if (burn_pt != null) burn_pt.Play();
    }

    private void DisablePickUp() => vrcPick.pickupable = false;
    private void EnablePickUp() => vrcPick.pickupable = true;
    private void PlayHaptics() => vrcPick.PlayHaptics();
}