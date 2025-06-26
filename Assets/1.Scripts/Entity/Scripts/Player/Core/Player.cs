using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.Player.StateMachineScripts;
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
        [field: SerializeField] public Transform CameraPoint { get; private set; }
        [field: SerializeField] public Transform WeaponPivot  { get; private set; }
        [field: SerializeField] public SerializedDictionary<string, Transform> WeaponPoints = new();
        [field: SerializeField] public CinemachineVirtualCamera FirstPersonCamera { get; private set; } // 플레이 전용
        [field: SerializeField] public CinemachineVirtualCamera ThirdPersonCamera { get; private set; } // 연출용
        
        [Header("StateMachine")]
        [SerializeField] private PlayerStateMachine stateMachine;

        [field: Header("Camera Settings")]
        [field: SerializeField] public float OriginalFoV { get; private set; }
        [field: SerializeField] public float ZoomFoV { get; private set; } = 40f;
        [field: SerializeField] public float TransitionTime { get; private set; } = 0.5f;
        
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
            if (!CameraPoint) CameraPoint = this.TryGetChildComponent<Transform>("CameraPoint");
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
            if (!CameraPoint) CameraPoint = this.TryGetChildComponent<Transform>("CameraPoint");
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
            FirstPersonCamera.Follow = CameraPoint;
            ThirdPersonCamera.LookAt = CameraPivot;
            cam = Camera.main;
            MainCameraTransform = cam?.transform;
            OriginalFoV = FirstPersonCamera.m_Lens.FieldOfView;
            
            stateMachine = new PlayerStateMachine(this); 
            stateMachine.ChangeState(stateMachine.IdleState);
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
    }
}
