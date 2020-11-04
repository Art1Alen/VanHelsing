﻿using UnityEngine;
using UniRx;
using System;


namespace BeastHunter
{
    public class BossMainState : BossBaseState
    {
        #region Constants

        public const float ANGLE_TARGET_RANGE = 20f;
        public const float DISTANCE_TO_START_ATTACK = 1.5f;

        private const float SPEED_COUNT_FRAME = 0.15f;
        private const float TIME_TO_NORMILIZE_WEAK_POINT = 5f;

        private const float HUNGER_TIME = 5f;

        #endregion


        #region Fields

        public Quaternion TargetRotation;

        private Vector3 _lastPosition;
        private Vector3 _currentPosition;
        private Vector3 _targetDirection;

        private float _speedCountTime;
        private float _hungerCountTime = HUNGER_TIME;
      //  private bool IsHunger;

        #endregion

        #region Properties

        public bool IsHunger { get; private set; }

        #endregion


        #region ClassLifeCycle

        public BossMainState(BossStateMachine stateMachine) : base(stateMachine)
        {
        }

        #endregion


        #region Methods

        public override void OnAwake()
        {
            _stateMachine._model.BossBehavior.OnFilterHandler += OnFilterHandler;
            _stateMachine._model.BossBehavior.OnTriggerEnterHandler += OnTriggerEnterHandler;
            _stateMachine._model.BossBehavior.OnTriggerExitHandler += OnTriggerExitHandler;
            MessageBroker.Default.Receive<OnPlayerDieEventCLass>().Subscribe(OnPlayerDieHandler);
            MessageBroker.Default.Receive<OnBossStunnedEventClass>().Subscribe(OnBossStunnedHandler);
            MessageBroker.Default.Receive<OnBossHittedEventClass>().Subscribe(OnBossHittedHandler);
            MessageBroker.Default.Receive<OnBossWeakPointHittedEventClass>().Subscribe(MakeWeakPointBurst);
            MessageBroker.Default.Receive<OnPlayerSneakingEventClass>().Subscribe(OnPlayerSneakingHandler);
        }

        public override void Initialise()
        {
            CanExit = false;
            CanBeOverriden = false;
        }

        public override void Execute()
        {
            if (!_stateMachine._model.IsDead)
            {
                SpeedCheck();
                HealthCheck();
                CheckDirection();
                HungerCheck();
                StaminaCheck();
            }          
        }

        public override void OnExit()
        {
        }

        public override void OnTearDown()
        {
            _stateMachine._model.BossBehavior.OnFilterHandler -= OnFilterHandler;
            _stateMachine._model.BossBehavior.OnTriggerEnterHandler -= OnTriggerEnterHandler;
            _stateMachine._model.BossBehavior.OnTriggerExitHandler -= OnTriggerExitHandler;
        }

        private void OnBossHittedHandler(OnBossHittedEventClass eventClass)
        {
            if (!_stateMachine._model.IsDead)
            {
              //  _stateMachine.SetCurrentStateOverride(BossStatesEnum.Hitted);
            }
        }

        private void OnBossStunnedHandler(OnBossStunnedEventClass eventClass)
        {
            if (!_stateMachine._model.IsDead)
            {
             //   _stateMachine.SetCurrentStateOverride(BossStatesEnum.Stunned);
            }         
        }

        private void OnPlayerDieHandler(OnPlayerDieEventCLass eventClass)
        {
            if (!_stateMachine._model.IsDead)
            {
                _stateMachine.SetCurrentStateOverride(BossStatesEnum.Patroling);
            }
        }

        private void SpeedCheck()
        {
            if (_speedCountTime > 0)
            {
                _speedCountTime -= Time.fixedDeltaTime;
            }
            else
            {
                _speedCountTime = SPEED_COUNT_FRAME;
                _currentPosition = _stateMachine._model.BossTransform.position;
                _stateMachine._model.CurrentSpeed = Mathf.Sqrt((_currentPosition - _lastPosition).sqrMagnitude) /
                    SPEED_COUNT_FRAME;
                _lastPosition = _currentPosition;
            }

            _stateMachine._model.BossAnimator.SetFloat("Speed", _stateMachine._model.CurrentSpeed);
        }

        private void HealthCheck()
        {
            if (_stateMachine._model.CurrentHealth <= 0)
            {
                _stateMachine.SetCurrentStateAnyway(BossStatesEnum.Dead);
            }

            if (_stateMachine._model.CurrentHealth <= _stateMachine._model.BossData._bossStats.MainStats.HealthPoints / 2)
            {
                _stateMachine.SetCurrentState(BossStatesEnum.Defencing);
            }

            if (_stateMachine._model.CurrentHealth <= _stateMachine._model.BossData._bossStats.MainStats.HealthPoints * 0.1f)
            {
                _stateMachine.SetCurrentState(BossStatesEnum.Retreating);
            }
        }

        private void HungerCheck()
        {
            if (!IsHunger)
            {
                _hungerCountTime -= Time.deltaTime;
                if(_hungerCountTime<=0)
                {
                    IsHunger = true;
                    _hungerCountTime = HUNGER_TIME;
                }
            }
        }

        private void StaminaCheck()
        {
            if (_stateMachine._model.CurrentStamina >= _stateMachine._model.MaxStamina & !_stateMachine.CurrentState.IsBattleState)
            {
                _stateMachine.SetCurrentStateOverride(BossStatesEnum.Resting);
            }
        }

        private bool OnFilterHandler(Collider tagObject)
        {
            bool isEnemyColliderHit = false;
            InteractableObjectBehavior interacterBehavior = tagObject.GetComponent<InteractableObjectBehavior>();

            if (interacterBehavior != null)
            {
                if( interacterBehavior.Type == InteractableObjectType.Player || interacterBehavior.Type == InteractableObjectType.Food)
                {
                    isEnemyColliderHit = true;
                }
            }
            return isEnemyColliderHit;
        }

        private void OnTriggerEnterHandler(ITrigger thisdObject, Collider enteredObject)
        {
            var interactableObject = enteredObject.GetComponent<InteractableObjectBehavior>().Type;

            if (interactableObject == InteractableObjectType.Player)
            {
                _stateMachine.SetCurrentStateOverride(BossStatesEnum.Chasing);
            }

            if (interactableObject == InteractableObjectType.Food)
            {
                if (!_stateMachine._model.FoodList.Contains(enteredObject.gameObject))
                {
                    _stateMachine._model.FoodList.Add(enteredObject.gameObject);
                }
                //if (IsHunger & !_stateMachine.CurrentState.IsBattleState)
                //{
                //    if (_stateMachine.CurrentState != _stateMachine.States[BossStatesEnum.Eating])
                //    {
                //        _stateMachine.SetCurrentStateOverride(BossStatesEnum.Eating);
                //    }
                //}
            }
        }

        private void OnTriggerExitHandler(ITrigger thisdObject, Collider enteredObject)
        {
            var interactableObject = enteredObject.GetComponent<InteractableObjectBehavior>().Type;

            if (interactableObject == InteractableObjectType.Food)
            {
                if (_stateMachine._model.FoodList.Contains(enteredObject.gameObject))
                {
                    _stateMachine._model.FoodList.Remove(enteredObject.gameObject);
                }
            }
            if (interactableObject == InteractableObjectType.Player)
            {
                _stateMachine.SetCurrentStateOverride(BossStatesEnum.Searching);
            }
        }

        private void MakeWeakPointBurst(OnBossWeakPointHittedEventClass eventClass)
        {
            eventClass.WeakPointCollider.gameObject.GetComponent<ParticleSystem>().Play();
            _stateMachine._model.TakeDamage(Services.SharedInstance.AttackService.CountDamage(
                eventClass.WeakPointCollider.GetComponent<HitBoxBehavior>().AdditionalDamage, 
                    _stateMachine._model.GetStats().MainStats));
            eventClass.WeakPointCollider.GetComponent<Light>().color = Color.red;
            eventClass.WeakPointCollider.enabled = false;

            Action makeWeakPointNormalAction = () => MakeWeakPointNormal(eventClass.WeakPointCollider);
            TimeRemaining makeWeakPointNormal = new TimeRemaining(makeWeakPointNormalAction, TIME_TO_NORMILIZE_WEAK_POINT);
            makeWeakPointNormal.AddTimeRemaining(TIME_TO_NORMILIZE_WEAK_POINT);
        }

        private void MakeWeakPointNormal(Collider collider)
        {
            collider.enabled = true;
            collider.GetComponent<Light>().color = Color.yellow;
        }

        private void CheckDirection()
        {
            _targetDirection = (_stateMachine._context.CharacterModel.CharacterTransform.position -
                _stateMachine._model.BossTransform.position).normalized;
            TargetRotation = Quaternion.LookRotation(_targetDirection);
        }

        public int AngleDirection(Vector3 forward, Vector3 targetDirection, Vector3 up)
        {
            Vector3 perpendicular = Vector3.Cross(forward, targetDirection);
            float direction = Vector3.Dot(perpendicular, up);

            if (direction > 0f)
            {
                return 1;
            }
            else if (direction < 0f)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        private void OnPlayerSneakingHandler(OnPlayerSneakingEventClass eventClass)
        {
            if (eventClass.IsSneaking)
            {
                _stateMachine._model.BossSphereCollider.radius /= 
                    _stateMachine._model.BossSettings.SphereColliderRadiusDecreace;
            }
            else
            {
                _stateMachine._model.BossSphereCollider.radius = _stateMachine._model.BossSettings.SphereColliderRadius;
            }
        }

        #endregion
    }
}

