using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Mofuryu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class maki_manager : UdonSharpBehaviour
    {
        //SERIALIZED
        [Header("燃える音(この中で1つランダム)")]
        [SerializeField] private AudioClip[] burnSound;

        [Header("音発生源")]
        [SerializeField] private AudioSource audioSrc;

        [Header("火元")]
        [SerializeField] private Transform[] fireTransform;

        [Header("火元判定距離(m)")]
        [SerializeField] float distBurnThreshold = 0.4f;

        [Header("薪燃焼エフェクト(すべて再生)")]
        [SerializeField] private ParticleSystem[] burnParticle;
        
        [Header("燃え始めの瞬間～リスポーンまでの時間(sec)")]
        [SerializeField] private float respawnTime = 2.0f;

        //INTERNAL
        private Vector3[] firePosition;
        private Vector3 initPos;
        private Quaternion initRot;

        private float currentTime = 0f;
        private bool isBurning = false;
        
        //VRCPICKUP
        [FieldChangeCallback(nameof(vrcPick))]
        private VRC_Pickup _vrcPick;
        private VRC_Pickup vrcPick => _vrcPick ? _vrcPick : (_vrcPick = (VRC_Pickup)GetComponent(typeof(VRC_Pickup)));

        public void Start()
        {
            SetTextPickup("FireWood");
            SetTheftablePickup(false);

            initPos = this.gameObject.transform.position;
            initRot = this.gameObject.transform.rotation;

            firePosition = new Vector3[fireTransform?.Length ?? 0];

            for(int i = 0; i < fireTransform?.Length ?? 0; i++){
                firePosition[i] = fireTransform[i].position;
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
                    isBurning = false;
                    if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
                        this.transform.SetPositionAndRotation(initPos,initRot);
                    EnablePickUp();
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
            foreach (Vector3 position in firePosition ?? new Vector3[0])
            {
                float dist = Vector3.Distance(transform.position, position);
                if (dist < min_dist) min_dist = dist;
            }
            
            if (min_dist < distBurnThreshold)
            {
                currentTime = respawnTime;
                isBurning = true;
                DisablePickUp();
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(Burn));
            }
        }

        public void Burn()
        {
            //Sounds            
            if (audioSrc && !audioSrc.isPlaying)
                audioSrc.PlayOneShot(burnSound[Random.Range(0, burnSound.Length)]);
            
            //Particles
            foreach (ParticleSystem burn_pt in burnParticle) if (burn_pt != null) burn_pt.Play();
        }

        //DETACHABLE WITH VRCPICKUP DECLARES
        private void DisablePickUp() => vrcPick.pickupable = false;
        private void EnablePickUp() => vrcPick.pickupable = true;
        private void PlayHaptics() => vrcPick.PlayHaptics();
        private void SetTextPickup(string st) => vrcPick.InteractionText = st;
        private void SetTheftablePickup(bool b) => vrcPick.DisallowTheft = b;

    }
}