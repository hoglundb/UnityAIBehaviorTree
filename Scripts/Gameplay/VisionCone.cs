using System;
using UnityEngine;

namespace AI
{
    public class VisionCone : MonoBehaviour
    {
        public event Action<Transform> PlayerSpotted;
        public event Action LostSightOfPlayer;

        [SerializeField] private float viewRadius = 10f;
        [SerializeField][Range(0, 360)] private float viewAngle = 90f;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private LayerMask obstructionMask;

        private Transform playerTransform;

        private void Update() => FindVisibleTargets();

        private void FindVisibleTargets()
        {
            var targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                var target = targetsInViewRadius[i].transform;
                var dirToTarget = (target.position - transform.position).normalized;
             
                if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2f)
                {
                    var dstToTarget = Vector3.Distance(transform.position, target.position);

                    if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstructionMask)) 
                    {
                        if (playerTransform == null)
                        {
                            playerTransform = target;
                            PlayerSpotted?.Invoke(target);
                        }
                        else if (playerTransform != target)
                        {
                            playerTransform = target;
                            PlayerSpotted?.Invoke(target);
                        }
                    }
                    else if (playerTransform != null)
                    {
                        playerTransform = null;
                        LostSightOfPlayer?.Invoke();
                    }
                }
                else if (playerTransform != null && target == playerTransform)
                {
                    playerTransform = null;
                    LostSightOfPlayer?.Invoke();
                }
            }
            if (targetsInViewRadius.Length == 0 && playerTransform != null)
            {
                playerTransform = null;
                LostSightOfPlayer?.Invoke();
            }
        }

        public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
        {
            if (!angleIsGlobal)
            {
                angleInDegrees += transform.eulerAngles.y;
            }
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }
    }
}