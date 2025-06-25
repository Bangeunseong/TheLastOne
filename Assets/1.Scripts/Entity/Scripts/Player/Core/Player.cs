using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.Player.StateMachineScripts;
using _1.Scripts.Weapon.Scripts;
using AYellowpaper.SerializedCollections;
using Cinemachine;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.Core
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerCondition), typeof(PlayerInteraction))]
    [RequireComponent(typeof(PlayerInput), typeof(PlayerGravity))]
    
    public class Player : MonoBehaviour
    {
        [field: Header("Animation Data")]
        [field: SerializeField] public AnimationData AnimationData { get; private set; } 
        
        [field: Header("Components")]
        [field: SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField] public CharacterController Controller { get; private set; }
        [field: SerializeField] public PlayerInput PlayerInput { get; private set; }
        [field: SerializeField] public PlayerCondition PlayerCondition { get; private set; }
        [field: SerializeField] public PlayerInteraction PlayerInteraction { get; private set; }
        [field: SerializeField] public PlayerGravity PlayerGravity { get; private set; }
        [field: SerializeField] public Transform MainCameraTransform { get; private set; }
        [field: SerializeField] public Transform CameraPivot { get; private set; }
        [field: SerializeField] public Transform WeaponPivot  { get; private set; }
        [field: SerializeField] public SerializedDictionary<string, Transform> WeaponPoints = new();
        [field: SerializeField] public CinemachineVirtualCamera FirstPersonCamera { get; private set; } // 플레이 전용
        [field: SerializeField] public CinemachineVirtualCamera ThirdPersonCamera { get; private set; } // 연출용
        
        [Header("StateMachine")]
        [SerializeField] private PlayerStateMachine stateMachine;

        [field: Header("Guns")]
        [field: SerializeField] public List<Gun> Guns { get; private set; } = new();
        [field: SerializeField] public List<bool> AvailableGuns { get; private set; } = new();
        [field: SerializeField] public int EquippedGunIndex { get; private set; } = -1;
        [field: SerializeField] public bool IsAttacking { get; set; }
        [field: SerializeField] public bool IsSwitching { get; private set; }
        [field: SerializeField] public bool IsAiming { get; private set; }

        private Coroutine switchCoroutine;
        private Coroutine aimCoroutine;
        
        public Camera cam { get; private set; }

        private void Awake()
        {
            if (!Animator) Animator = this.TryGetComponent<Animator>();
            if (!Controller) Controller = this.TryGetComponent<CharacterController>();
            if (!PlayerCondition) PlayerCondition = this.TryGetComponent<PlayerCondition>();
            if (!PlayerInteraction) PlayerInteraction = this.TryGetComponent<PlayerInteraction>();
            if (!PlayerInput) PlayerInput = this.TryGetComponent<PlayerInput>();
            if (!PlayerGravity) PlayerGravity = this.TryGetComponent<PlayerGravity>();
            if (!CameraPivot) CameraPivot = this.TryGetChildComponent<Transform>("CameraPivot");
            if (!WeaponPivot) WeaponPivot = this.TryGetChildComponent<Transform>("WeaponPivot");
            WeaponPoints["WieldPoint"] = this.TryGetChildComponent<Transform>("WieldPoint");
            WeaponPoints["AimPoint"] = this.TryGetChildComponent<Transform>("AimPoint");
            WeaponPoints["SwitchPoint"] = this.TryGetChildComponent<Transform>("SwitchPoint");

            if (!FirstPersonCamera) FirstPersonCamera = GameObject.Find("FirstPersonCamera")?.GetComponent<CinemachineVirtualCamera>();
            if (!ThirdPersonCamera) ThirdPersonCamera = GameObject.Find("ThirdPersonCamera")?.GetComponent<CinemachineVirtualCamera>();
            
            AnimationData.Initialize();
        }

        private void Reset()
        {
            if (!Animator) Animator = this.TryGetComponent<Animator>();
            if (!Controller) Controller = this.TryGetComponent<CharacterController>();
            if (!PlayerCondition) PlayerCondition = this.TryGetComponent<PlayerCondition>();
            if (!PlayerInteraction) PlayerInteraction = this.TryGetComponent<PlayerInteraction>();
            if (!PlayerInput) PlayerInput = this.TryGetComponent<PlayerInput>();
            if (!PlayerGravity) PlayerGravity = this.TryGetComponent<PlayerGravity>();
            if (!CameraPivot) CameraPivot = this.TryGetChildComponent<Transform>("CameraPivot");
            if (!WeaponPivot) WeaponPivot = this.TryGetChildComponent<Transform>("WeaponPivot");
            WeaponPoints["WieldPoint"] = this.TryGetChildComponent<Transform>("WieldPoint");
            WeaponPoints["AimPoint"] = this.TryGetChildComponent<Transform>("AimPoint");
            WeaponPoints["SwitchPoint"] = this.TryGetChildComponent<Transform>("SwitchPoint");
            
            if (!FirstPersonCamera) FirstPersonCamera = GameObject.Find("FirstPersonCamera")?.GetComponent<CinemachineVirtualCamera>();
            if (!ThirdPersonCamera) ThirdPersonCamera = GameObject.Find("ThirdPersonCamera")?.GetComponent<CinemachineVirtualCamera>();

            AnimationData.Initialize();
        }

        // Start is called before the first frame update
        private void Start()
        {
            FirstPersonCamera.Follow = CameraPivot;
            ThirdPersonCamera.LookAt = CameraPivot;
            cam = Camera.main;
            MainCameraTransform = cam?.transform;
            
            stateMachine = new PlayerStateMachine(this);
            stateMachine.ChangeState(stateMachine.IdleState);
            
            var listOfGuns = GetComponentsInChildren<Gun>(true);
            foreach (var gun in listOfGuns)
            {
                gun.Initialize(gameObject);
                Guns.Add(gun); 
                AvailableGuns.Add(false);
            }
        }

        private void FixedUpdate()
        {
            stateMachine.PhysicsUpdate();
        }

        // Update is called once per frame
        private void Update()
        {
            stateMachine.HandleInput();
            stateMachine.Update();
        }

        private void LateUpdate()
        {
            stateMachine.LateUpdate();
            
            if (IsAttacking && EquippedGunIndex >= 0) { Guns[EquippedGunIndex].OnShoot(); }
        }

        /// <summary>
        /// Play Foot Step Sound
        /// </summary>
        /// <param name="animationEvent"></param>
        private void OnFootstep(AnimationEvent animationEvent)
        {
            // TODO: 걸을 때 소리 추가
            // if (animationEvent.animatorClipInfo.weight > 0.5f)
            // {
            //     if (FootstepAudioClips.Length > 0)
            //     {
            //         var index = Random.Range(0, FootstepAudioClips.Length);
            //         AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            //     }
            // }
        }

        /// <summary>
        /// Play Land Sound
        /// </summary>
        /// <param name="animationEvent"></param>
        private void OnLand(AnimationEvent animationEvent)
        {
            // TODO: 착치할 때 소리 추가
            // if (animationEvent.animatorClipInfo.weight > 0.5f)
            // {
            //     AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            // }
        }
        
        /* - Aim 관련 메소드 - */

        public void OnAim(bool isAim, float targetFoV, float transitionTime)
        {
            if(aimCoroutine != null){StopCoroutine(aimCoroutine);}
            aimCoroutine = StartCoroutine(ChangeFoV_Coroutine(isAim, targetFoV, transitionTime));
            IsAiming = isAim;
        }
        
        private IEnumerator ChangeFoV_Coroutine(bool isAim, float targetFoV, float transitionTime)
        {
            Vector3 currentPosition = WeaponPivot.localPosition;
            Vector3 targetLocalPosition = isAim
                ? WeaponPoints["AimPoint"].localPosition
                : WeaponPoints["WieldPoint"].localPosition;
            float currentFoV = stateMachine.Player.FirstPersonCamera.m_Lens.FieldOfView;

            var time = 0f;
            while (time < transitionTime)
            {
                time += Time.deltaTime;
                float t = time / transitionTime;
                var value = Mathf.Lerp(currentFoV, targetFoV, t);
                FirstPersonCamera.m_Lens.FieldOfView = value;
                if(EquippedGunIndex >= 0)
                    WeaponPivot.localPosition = Vector3.Lerp(currentPosition, targetLocalPosition, t);
                yield return null;
            }

            FirstPersonCamera.m_Lens.FieldOfView = targetFoV;
            if(EquippedGunIndex >= 0)
                WeaponPivot.localPosition = targetLocalPosition;
            aimCoroutine = null;
        }
        
        /* ----------------- */
        
        /* - Weapon Switch 메소드 - */
        public void OnSwitchWeapon(int weaponIndex, float duration)
        {
            IsAttacking = false;
            if (switchCoroutine != null){ StopCoroutine(switchCoroutine); }
            switchCoroutine = StartCoroutine(OnSwitchWeapon_Coroutine(weaponIndex, duration));
        }

        public IEnumerator OnSwitchWeapon_Coroutine(int weaponIndex, float duration)
        {
            IsSwitching = true;
            
            if (IsAiming) OnAim(false, 60, 0.2f);
            while (IsAiming){}
            
            Vector3 currentWeaponPivotPosition = WeaponPivot.localPosition;
            Quaternion currentWeaponPivotRotation = WeaponPivot.localRotation;
            Vector3 targetLocalPosition = WeaponPoints["SwitchPoint"].localPosition;
            Quaternion targetLocalRotation = WeaponPoints["SwitchPoint"].localRotation;

            if (EquippedGunIndex >= 0)
            {
                // 무기를 밑으로 먼저 내리기
                var time = 0f;
                while (time < duration)
                {
                    time += Time.deltaTime;
                    float t = time / duration;
                    WeaponPivot.SetLocalPositionAndRotation(
                        Vector3.Lerp(currentWeaponPivotPosition, targetLocalPosition, t), 
                        Quaternion.Lerp(currentWeaponPivotRotation, targetLocalRotation, t));
                    yield return null;
                }

                WeaponPivot.transform.SetLocalPositionAndRotation(WeaponPoints["WieldPoint"].localPosition, WeaponPoints["WieldPoint"].localRotation);
                Guns[EquippedGunIndex].gameObject.SetActive(false);
                EquippedGunIndex = -1;
            }
            
            // 만약 들어온 weaponIndex에 해당하는 무기 혹은 weaponIndex가 0보다 작을 경우 예외처리
            if (weaponIndex < 0 || !AvailableGuns[weaponIndex])
            {
                IsSwitching = false;
                yield break;
            }
            
            currentWeaponPivotPosition = WeaponPivot.localPosition;
            currentWeaponPivotRotation = WeaponPivot.localRotation;
            targetLocalPosition = WeaponPoints["WieldPoint"].localPosition;
            targetLocalRotation = WeaponPoints["WieldPoint"].localRotation;
            EquippedGunIndex = weaponIndex;
            Guns[EquippedGunIndex].gameObject.SetActive(true);
            
            float weaponWieldTime = 0f;
            while (weaponWieldTime < duration)
            {
                weaponWieldTime += Time.deltaTime;
                float t = weaponWieldTime / duration;
                WeaponPivot.SetLocalPositionAndRotation(
                    Vector3.Lerp(currentWeaponPivotPosition, targetLocalPosition, t), 
                    Quaternion.Lerp(currentWeaponPivotRotation, targetLocalRotation, t));
                yield return null;
            }
            
            WeaponPivot.transform.SetLocalPositionAndRotation(WeaponPoints["WieldPoint"].localPosition, WeaponPoints["WieldPoint"].localRotation);
            switchCoroutine = null;
            IsSwitching = false;
        }
        /* --------------------- */
    }
}
