
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)] 
public class maki_manager : UdonSharpBehaviour
{
    [Header("音声系統")]
    [SerializeField] private AudioClip[] burnSounds;
    [SerializeField] private AudioSource[] auSrc;

    [Header("火元")]
    [SerializeField] private Transform fireTransform;

    private VRC_Pickup vrcPick = null;

    void Start()
    {
        //pickupコンポーネントを取得
        vrcPick = (VRC_Pickup)this.GetComponent(typeof(VRC_Pickup));
    }

    //privateでもよさそう？
    public void Update()
    {
        if (vrcPick) vrcPick.pickupable = true;
    }

    public override void OnDrop()
    {
        if (!vrcPick) vrcPick = (VRC_Pickup)this.GetComponent(typeof(VRC_Pickup));
        if (vrcPick)  vrcPick.PlayHaptics();
        
        //audio.PlayOneShot(burnSounds[Random.Range(0, burnSounds.Length)]);
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(BurnFireWood));
     
    }

    public void BurnFireWood()
    {

    }


}