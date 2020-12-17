﻿using UnityEngine;
using UniRx;
using Extensions;


namespace BeastHunter
{
    [CreateAssetMenu(fileName = "NewBombtData", menuName = "CreateProjectileData/CreateBombData", order = 0)]
    public sealed class BombData : ProjectileData
    {      
        #region Methods

        public override bool FilterCollision(Collision touchedCollider)
        {
            return touchedCollider.collider.GetComponent<InteractableObjectBehavior>()?.
                Type != InteractableObjectType.Player;
        }

        public override void HitProjectile(IProjectile projectileInterface, Collision touchedCollider)
        {
            bool isHittedEnemy = touchedCollider.transform.gameObject.TryGetComponent(out InteractableObjectBehavior touchedBehavior);

            if (isHittedEnemy)
            {
                switch (touchedBehavior.Type)
                {
                    case InteractableObjectType.Enemy:
                        break;
                    case InteractableObjectType.WeakHitBox:
                        MessageBroker.Default.Publish(
                            new OnBossWeakPointHittedEventClass { WeakPointCollider = touchedCollider.collider });
                        break;
                    default:
                        break;
                }

                Context.NpcModels[touchedCollider.transform.GetMainParent().gameObject.GetInstanceID()].TakeDamage(Services.
                    SharedInstance.AttackService.CountDamage(ProjectileDamage, touchedCollider.transform.GetMainParent().gameObject.GetInstanceID()));
            }

            ExplodeBomb(projectileInterface, touchedCollider);
        }

        private void ExplodeBomb(IProjectile projectileInterface, Collision touchedCollider)
        {
            Rigidbody bombRigidbody = projectileInterface.GameObject.GetComponent<Rigidbody>();
            bombRigidbody.velocity = Vector3.zero;
            bombRigidbody.isKinematic = true;
            projectileInterface.GameObject.GetComponent<ParticleSystem>().Play();
            Destroy(projectileInterface.GameObject.GetComponent<ProjectileBehavior>());
            Destroy(projectileInterface.GameObject.GetComponent<MeshRenderer>());
            Destroy(projectileInterface.GameObject.GetComponent<Collider>());
            Destroy(projectileInterface.GameObject, 0.5f);
        }

        #endregion
    }
}

