using System;
using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.Player.StateMachineScripts;
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
        [field: SerializeField] public CinemachineVirtualCamera FirstPersonCamera { get; private set; } // 플레이 전용
        [field: SerializeField] public CinemachineVirtualCamera ThirdPersonCamera { get; private set; } // 연출용
        
        private PlayerStateMachine stateMachine;
        
        public Camera cam { get; private set; }

        private void Awake()
        {
            if (!Animator) Animator = this.TryGetChildComponent<Animator>("Body");
            if (!Controller) Controller = this.TryGetComponent<CharacterController>();
            if (!PlayerCondition) PlayerCondition = this.TryGetComponent<PlayerCondition>();
            if (!PlayerInteraction) PlayerInteraction = this.TryGetComponent<PlayerInteraction>();
            if (!PlayerInput) PlayerInput = this.TryGetComponent<PlayerInput>();
            if (!PlayerGravity) PlayerGravity = this.TryGetComponent<PlayerGravity>();
            if (!CameraPivot) CameraPivot = this.TryGetChildComponent<Transform>("CameraPivot");

            if (!FirstPersonCamera) FirstPersonCamera = GameObject.Find("FirstPersonCamera")?.GetComponent<CinemachineVirtualCamera>();
            if (!ThirdPersonCamera) ThirdPersonCamera = GameObject.Find("ThirdPersonCamera")?.GetComponent<CinemachineVirtualCamera>();
            
            AnimationData.Initialize();
        }

        private void Reset()
        {
            if (!Animator) Animator = this.TryGetChildComponent<Animator>("Body");
            if (!Controller) Controller = this.TryGetComponent<CharacterController>();
            if (!PlayerCondition) PlayerCondition = this.TryGetComponent<PlayerCondition>();
            if (!PlayerInteraction) PlayerInteraction = this.TryGetComponent<PlayerInteraction>();
            if (!PlayerInput) PlayerInput = this.TryGetComponent<PlayerInput>();
            if (!PlayerGravity) PlayerGravity = this.TryGetComponent<PlayerGravity>();
            if (!CameraPivot) CameraPivot = this.TryGetChildComponent<Transform>("CameraPivot");

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
    }
}
