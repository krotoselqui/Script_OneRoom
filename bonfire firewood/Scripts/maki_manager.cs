
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class maki_manager : UdonSharpBehaviour
{
    [Header("音声系統(なくても動く)")]
    [Header("手放したときに再生する音(この中でランダム)")]
    [SerializeField] private AudioClip[] burnSounds;
    [Header("音の発生源")]
    [SerializeField] private AudioSource[] auSrc;

    [Header("火元")]
    [SerializeField] private Transform fireTransform;

    [Header("薪燃焼エフェクト (world依存に設定して下さい)")]
    [SerializeField] private ParticleSystem burnParticle;

    private VRC_Pickup vrcPick = null;

    private Vector3 initPosition = new Vector3(0, 0, 0);

    [Header("薪を手放してからリスポーンするまでの時間")]
    [SerializeField] private float respawnTime = 2.0f;

    private float currentTime = 0f;
    private bool isBurning = false;


    void Start()
    {
        //pickupコンポーネントを取得
        vrcPick = (VRC_Pickup)this.GetComponent(typeof(VRC_Pickup));
        initPosition = this.gameObject.transform.position;

        isBurning = false;
    }

    //privateでもよさそう？
    public void Update()
    {
        if (isBurning)
        {
            if (vrcPick) vrcPick.pickupable = false;
            if (currentTime < 0)
            {
                if (vrcPick) vrcPick.pickupable = true;
                isBurning = false;
                if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
                {
                    this.transform.position = initPosition;
                }
            }
            else 
            {
                currentTime -= Time.deltaTime;
            }
        }
        else
        {
            if (vrcPick) vrcPick.pickupable = true;
        }
    }

    public override void OnDrop()
    {
        if (!vrcPick) vrcPick = (VRC_Pickup)this.GetComponent(typeof(VRC_Pickup));
        if (vrcPick) vrcPick.PlayHaptics();


        currentTime = respawnTime;
        isBurning = true;
        if (vrcPick) vrcPick.pickupable = false;


        //audio.PlayOneShot(burnSounds[Random.Range(0, burnSounds.Length)]);
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(BurnFireWood));

    }

    public void BurnFireWood()
    {
        foreach (AudioSource audio in auSrc)
        {
            if (!audio) continue;
            if (!audio.isPlaying)
            {
                if (burnSounds.Length <= 0) break;
                audio.transform.position = fireTransform.position;
                audio.PlayOneShot(burnSounds[Random.Range(0, burnSounds.Length)]);

                break;
            }
        }


        if (burnParticle != null)
        {
            burnParticle.Play();
        }


    }


}